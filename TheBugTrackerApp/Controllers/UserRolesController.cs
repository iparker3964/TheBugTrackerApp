using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TheBugTrackerApp.Extensions;
using TheBugTrackerApp.Models;
using TheBugTrackerApp.Models.ViewModels;
using TheBugTrackerApp.Services.Interfaces;

namespace TheBugTrackerApp.Controllers
{
    [Authorize]
    public class UserRolesController : Controller
    {
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyInfoService _companyInfoService;
        public UserRolesController(IBTRolesService rolesService, IBTCompanyInfoService companyInfoService)
        {
            _rolesService = rolesService;
            _companyInfoService = companyInfoService;
        }
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();

            int companyId = User.Identity.GetCompanyId().Value;

            List<BTUser> users = await _companyInfoService.GetAllMembersAsync(companyId);

            foreach (BTUser user in users)
            {
                ManageUserRolesViewModel viewModel = new();

                viewModel.BTUser = user;
                viewModel.SelectedRoles = (await _rolesService.GetUserRolesAsync(user)).ToList();
                viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", viewModel.SelectedRoles);

                model.Add(viewModel);
            }
           
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel member)
        {
            //Get companyId
            int companyId = User.Identity.GetCompanyId().Value;
            //Instantiate the BTUser
            BTUser user = (await _companyInfoService.GetAllMembersAsync(companyId)).FirstOrDefault(u => u.Id == member.BTUser.Id);
            //Get Roles for the user
            IEnumerable<string> roles = await _rolesService.GetUserRolesAsync(user);
            //Grab the selected role
            string selectedRole = member.SelectedRoles.FirstOrDefault();

            if (!string.IsNullOrEmpty(selectedRole))
            {
                //remove user from their roles
                if (await _rolesService.RemoveUserFromRolesAsync(user,roles))
                {
                    //add user to the new role
                    await _rolesService.AddUserToRoleAsync(user,selectedRole);
                }
            }

            //Navigate back to the view
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}
