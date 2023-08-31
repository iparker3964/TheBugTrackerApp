using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Data;
using TheBugTrackerApp.Models;
using TheBugTrackerApp.Models.Enums;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Services
{
    public class BTProjectService : IBTProjectService
    {
        #region Properties
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        #endregion

        #region Constructor
        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }
        #endregion

        #region Add New Project
        //CRUD - Create
        public async Task AddNewProjectAsync(Project project)
        {
            _context.Add(project);
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Add Project Manager
        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            BTUser currentPM = await GetProjectManagerAsync(projectId);

            if (currentPM != null)
            {
                try
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"**** ERROR **** Error removing current PM --> {ex.Message}");
                    return false;
                }
            }
            try
            {
                await AddUserToProjectAsync(userId,projectId);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error removing current PM --> {ex.Message}");
                return false;
            }
        }
        #endregion
        #region Add User To Project
        public async Task<bool> AddUserToProjectAsync(string userId, int projectId)
        {
            BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user != null)
            {
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
                if (project != null && !await IsUserOnProjectAsync(userId, projectId))
                {
                    try
                    {
                        project.Members.Add(user);
                        await _context.SaveChangesAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion

        //CRUD - Archive(Delete)
        #region Archive Project
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;

                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("****************************");
                Console.WriteLine("Error archiving project");
                Console.WriteLine(ex.Message);
                Console.WriteLine("****************************");
            }
        }
        #endregion

        #region Get All Project Members Except Project Manager
        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            List<BTUser> admins = await GetProjectMembersByRoleAsync(projectId, Roles.Admin.ToString());
            List<BTUser> developers = await GetProjectMembersByRoleAsync(projectId, Roles.Developer.ToString());
            List<BTUser> submitters = await GetProjectMembersByRoleAsync(projectId, Roles.Submitter.ToString());

            List<BTUser> team = admins.Concat(developers).Concat(submitters).ToList();

            return team;
        }
        #endregion


        public async Task<List<Project>> GetAllProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == false)
                                             .Include(p => p.Members)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.ProjectPriority)
                                             .ToListAsync();

            return projects;
        }

        public async Task<List<Project>> GetAllProjectsByPriority(int companyId, string priorityName)
        {
            List<Project> projects = new();

            projects = await GetAllProjectsByCompany(companyId);

            int priorityId = await LookUpProjectPriorityId(priorityName);

            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompany(int companyId)
        {
            List<Project> projects = new();

            projects = await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true)
                                             .Include(p => p.Members)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.Notifications)
                                             .Include(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                             .Include(p => p.ProjectPriority)
                                             .ToListAsync();

            return projects;
        }

        public Task<List<BTUser>> GetDevelopersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }
        //CRUD - Read
        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project project = await _context.Projects
                                            .Include(p=> p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t=> t.TicketType)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t=> t.DeveloperUser)
                                            .Include(p => p.Tickets)
                                                .ThenInclude(t => t.OwnerUser)
                                            .Include(p=> p.Members)
                                            .Include(p=> p.ProjectPriority)
                                            .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);


            return project;
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            BTUser result = null;

            foreach (BTUser member in project?.Members)
            {
                if (await _rolesService.IsUserInRoleAsync(member,Roles.ProjectManager.ToString()))
                {
                    return member;
                }
            }

            return result;
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            List<BTUser> members = new();

            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            if (project != null)
            {
                foreach (BTUser user in project.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user,role))
                    {
                        members.Add(user);
                    }
                }
              
            }

            return members;
        }

        public Task<List<BTUser>> GetSubmittersOnProjectAsync(int projectId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            List<Project> userProjects = new();

            try
            {
                userProjects = (await _context.Users
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Company)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Members)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.OwnerUser)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                              .Include(u => u.Projects)
                                                .ThenInclude(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                              .FirstOrDefaultAsync(u => u.Id == userId)
                               ).Projects.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error getting user projects ---> {ex.Message}");
                throw;
            }
           
            return userProjects;
        }

        public async Task<List<BTUser>> GetUsersNotOnProjectAsync(int projectId, int companyId)
        {
            List<BTUser> users = await _context.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToListAsync();

            return users.Where(u => u.CompanyId == companyId).ToList();
        }
        #region Is Assigned Project Manager
        public async Task<bool> IsAssignedProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                string projectManagerId = (await GetProjectManagerAsync(projectId))?.Id;

                if(userId == projectManagerId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("*********Error checking assigned project manager*********");
                Console.WriteLine(ex.Message);
                Console.WriteLine("***************************************************");
                throw;
            }
        }
        #endregion


        public async Task<bool> IsUserOnProjectAsync(string userId, int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            bool result = false;

            if (project != null)
            {
                result = project.Members.Any(m => m.Id == userId);
            }
            return result;
        }

        public async Task<int> LookUpProjectPriorityId(string priorityName)
        {
            int result = (await _context.ProjectPriorities.FirstOrDefaultAsync(p => p.Name.Equals(priorityName))).Id;

            return result;
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            Project project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

            try
            {
                foreach (BTUser member in project?.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member,Roles.ProjectManager.ToString()))
                    {
                        await RemoveUserFromProjectAsync(member.Id,projectId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error removing project manager --> {ex.Message}");
                throw;
            }
        }

        public async Task RemoveUserFromProjectAsync(string userId, int projectId)
        {
            if(await IsUserOnProjectAsync(userId, projectId))
            {
                try
                {
                    BTUser user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                    Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                    project.Members.Remove(user);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"**** ERROR **** - Error Removing User from project. ---> {ex.Message}");
                }
            }
        }

        public async Task RemoveUsersFromProjectByRoleAsync(string role, int projectId)
        {
            try
            {
                List<BTUser> members = await GetProjectMembersByRoleAsync(projectId,role);
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);

                foreach(BTUser user in members)
                {
                    try
                    {
                        project.Members.Remove(user);
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"**** ERROR **** Error removing users from project. --> {ex.Message}");
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;

                await UpdateProjectAsync(project);

                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;

                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("****************************");
                Console.WriteLine("Error archiving project");
                Console.WriteLine(ex.Message);
                Console.WriteLine("****************************");
            }
        }

        //CRUD - Update
        public async Task UpdateProjectAsync(Project project)
        {
            _context.Update(project);
            await _context.SaveChangesAsync();
        }
    }
}
