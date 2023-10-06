using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TheBugTrackerApp.Data;
using TheBugTrackerApp.Extensions;
using TheBugTrackerApp.Models;
using TheBugTrackerApp.Models.Enums;
using TheBugTrackerApp.Models.ViewModels;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTLookupService _btLookupService;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyInfoService _companyInfoService;
        public ProjectsController(IBTRolesService rolesService, IBTLookupService btLookupService, IBTFileService fileService, IBTProjectService projectService,UserManager<BTUser> userManager, IBTCompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _fileService = fileService;
            _projectService = projectService;
            _btLookupService = btLookupService;
            _userManager = userManager;
            _companyInfoService = companyInfoService;
        }
        public async Task<IActionResult> MyProjects()
        {
            BTUser user = await _userManager.GetUserAsync(User);

            List<Project> projects = await _projectService.GetUserProjectsAsync(user.Id);

            return View(projects);
        }
        public async Task<IActionResult> AllProjects()
        {
            List<Project> projects = new();

            int companyId = User.Identity.GetCompanyId().Value;

            if (User.IsInRole(nameof(Roles.Admin)) || User.IsInRole(nameof(Roles.ProjectManager)))
            {
                projects = await _companyInfoService.GetAllProjectsAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetAllProjectsByCompany(companyId);
            }

            return View(projects);
        }
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;


            List<Project> projects = await _projectService.GetArchivedProjectsByCompany(companyId);

            return View(projects);
        }
        [Authorize(Roles="Admin,ProjectManager")]
        [HttpGet]
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();

            int companyId = User.Identity.GetCompanyId().Value;

            model.Project = await _projectService.GetProjectByIdAsync(id,companyId);

            List<BTUser> developers = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId)).ToList();
            List<BTUser> submitters = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId)).ToList();

            List<BTUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> currentProjectMembers = model.Project.Members.Select(m => m.Id).ToList();

            model.Users = new MultiSelectList(companyMembers,"Id","FullName",currentProjectMembers);

            return View(model);
        }
        [Authorize(Roles="Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if (model.SelectedUsers != null)
            {
                List<string> memberIds = (await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id)).Select(m => m.Id).ToList();

                //remove current members
                foreach (string memberId in memberIds)
                {
                    await _projectService.RemoveUserFromProjectAsync(memberId, model.Project.Id);
                }

                //add selected members
                foreach (string user in model.SelectedUsers)
                {
                    await _projectService.AddUserToProjectAsync(user, model.Project.Id);
                }

                return RedirectToAction(nameof(Details),"Projects",new { id = model.Project.Id});
            }

            return RedirectToAction(nameof(AssignMembers),new { id = model.Project.Id});
        }
        [Authorize(Roles="Admin")]
        [HttpGet]
        public async Task<IActionResult> AssignPM(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            AssignPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id,companyId);
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId),"Id","FullName");

            return View(model);
        }
        [Authorize(Roles="Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(AssignPMViewModel model)
        {
            if (!string.IsNullOrEmpty(model.PMID))
            {
                await _projectService.AddProjectManagerAsync(model.PMID,model.Project.Id);

                return RedirectToAction(nameof(Details),new { id = model.Project.Id});
            }

            return RedirectToAction(nameof(AssignPM),new { id = model.Project.Id});
        }
        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            Project project = await _projectService.GetProjectByIdAsync(id.Value,companyId); 

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity.GetCompanyId().Value;
           
            AddProjectWithPMViewModel model = new();

            //Load selectlist PMList and PriorityList
            model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(),companyId),"Id","FullName");
            model.PriorityList = new SelectList(await _btLookupService.GetProjectPrioritiesAsync(),"Id","Name");
        
            return View(model);
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                int companyId = User.Identity.GetCompanyId().Value;
                 
                try
                {
                    //check if image has been selected
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    }
                    model.Project.CompanyId = companyId;
                    await _projectService.AddNewProjectAsync(model.Project);

                    //Add PM if one was choosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                }
                catch(Exception ex){
                    Console.WriteLine("***************************");
                    Console.WriteLine("Error creating project!");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("***************************");

                    throw;
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Create");
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity.GetCompanyId().Value;

            AddProjectWithPMViewModel model = new();

            model.Project = await _projectService.GetProjectByIdAsync(id.Value,companyId);

            //Load selectlist PMList and PriorityList

            BTUser pmUser = await _projectService.GetProjectManagerAsync(id.Value);
            if (pmUser != null)
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName",pmUser.Id);
            }
            else
            {
                model.PMList = new SelectList(await _rolesService.GetUsersInRoleAsync(Roles.ProjectManager.ToString(), companyId), "Id", "FullName");
            }

            if (model.Project.ProjectPriorityId != null)
            {
                model.PriorityList = new SelectList(await _btLookupService.GetProjectPrioritiesAsync(), "Id", "Name", model.Project.ProjectPriorityId);
            }
            else
            {
                model.PriorityList = new SelectList(await _btLookupService.GetProjectPrioritiesAsync(), "Id", "Name");
            }
           

            return View(model);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AddProjectWithPMViewModel model)
        {
            if (model != null)
            {
                try
                {
                    //check if image has been selected
                    if (model.Project.ImageFormFile != null)
                    {
                        model.Project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(model.Project.ImageFormFile);
                        model.Project.ImageContentType = model.Project.ImageFormFile.ContentType;
                        model.Project.ImageFileName = model.Project.ImageFormFile.FileName;
                    }
      
                    await _projectService.UpdateProjectAsync(model.Project);

                    //Add PM if one was choosen
                    if (!string.IsNullOrEmpty(model.PmId))
                    {
                        await _projectService.AddProjectManagerAsync(model.PmId, model.Project.Id);
                    }

                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await ProjectExists(model.Project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        Console.WriteLine("***************************");
                        Console.WriteLine("Error creating project!");
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("***************************");

                        throw;
                    }
                    
                }

                return RedirectToAction("Index");
            }
            return RedirectToAction("Edit");
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id,companyId);

            await _projectService.ArchiveProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }
        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Restore/5
        [Authorize(Roles = "Admin,ProjectManager")]
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            await _projectService.RestoreProjectAsync(project);

            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity.GetCompanyId().Value;

            return (await _projectService.GetAllProjectsByCompany(companyId)).Any(p => p.Id == id);
        }
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity.GetCompanyId().Value;

            List<Project> projects = new();

            projects = await _projectService.GetUnassignedProjectsAsync(companyId);

            return View(projects);

        }
    }
}
