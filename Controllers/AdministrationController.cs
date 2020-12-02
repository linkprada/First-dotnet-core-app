using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyApp.Models;
using MyApp.ViewModels;

namespace MyApp.Controllers
{
    // [Authorize(Policy = "AdminRolePolicy")]
    public class AdministrationController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ILogger<AdministrationController> logger;

        public AdministrationController(RoleManager<IdentityRole> roleManager,
                                        UserManager<ApplicationUser> userManager,
                                        ILogger<AdministrationController> logger)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied ()
        {
            return View();
        }

        [HttpGet]
        public ViewResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync(CreateRoleViewModel model)
        {
            if(ModelState.IsValid)
            {
                var identityRole = new IdentityRole{
                    Name = model.RoleName
                };

                var result = await roleManager.CreateAsync(identityRole);

                if (result.Succeeded) 
                {
                    return RedirectToAction("listroles","administration");    
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }
            }
            return View();
        }

        public IActionResult ListRoles ()
        {
            var roles = roleManager.Roles ;
            return View(roles);
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRoleAsync (string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) 
            {
                ViewBag.ErrorMessage = $"Role with Id = {id} cannot be found" ;
                return RedirectToAction("notfound");   
            }

            var model = new EditRoleViewModel
            {
                Id = role.Id,
                RoleName = role.Name    
            };

            foreach(var user in await userManager.GetUsersInRoleAsync(role.Name))
            {
                model.Users.Add(user.UserName);
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> EditRoleAsync (EditRoleViewModel model)
        {
            var role = await roleManager.FindByIdAsync(model.Id);
            if (role == null) 
            {
                ViewBag.ErrorMessage = $"Role with Id = {model.Id} cannot be found" ;
                return RedirectToAction("notfound");   
            }
            
            role.Name = model.RoleName;
            var result = await roleManager.UpdateAsync(role);

            if (result.Succeeded) 
            {
                return RedirectToAction("listroles");    
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("",error.Description);    
            }
            
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> EditUsersInRoleAsync (string RoleId)
        {
            ViewBag.RoleId = RoleId ;
            var role = await roleManager.FindByIdAsync(RoleId);
            if (role == null) 
            {
                ViewBag.ErrorMessage = $"Role with id = {role.Id} cannot be found";
                return RedirectToAction("NotFound");    
            }

            var model = new List<UserRoleViewModel>();

            foreach(var user in await userManager.Users.ToListAsync())
            {
                var userRoleViewModel = new UserRoleViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName
                };

                if (await userManager.IsInRoleAsync(user, role.Name)) 
                {
                    userRoleViewModel.IsSelected = true ;
                }
                else
                {
                    userRoleViewModel.IsSelected = false ;
                }

                model.Add(userRoleViewModel);
            }

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditUsersInRoleAsync (List<UserRoleViewModel> model , string roleId)
        {
            var role = await roleManager.FindByIdAsync(roleId);
            if (role == null) 
            {
                ViewBag.ErrorMessage = $"Role with id = {roleId} cannot be found";
                return RedirectToAction("NotFound");    
            }

            IdentityResult result = new IdentityResult();

            for (int i = 0; i < model.Count; i++)
            {
                var user = await userManager.FindByIdAsync(model[i].UserId);

                if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user,role.Name)))
                {
                    result = await userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && (await userManager.IsInRoleAsync(user,role.Name)))
                {
                    result = await userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }

                if (result.Succeeded) 
                {
                    continue;
                }
            }

            return RedirectToAction("EditRole",new{ Id = roleId});
        }

        public IActionResult ListUsers ()
        {
            var users = userManager.Users ;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserAsync (string Id)
        {
            var user = await userManager.FindByIdAsync(Id);

            if (user == null) 
            {
                ViewBag.ErrorMessage = $"User with id = {Id} cannot be found";
                return RedirectToAction("NotFound");    
            }

            var userClaims = await userManager.GetClaimsAsync(user);
            var userRoles = await userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                City = user.City,
                Claims = userClaims.Select(claim =>claim.Type +" : "+ claim.Value).ToList(),
                Roles = userRoles
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditUserAsync (EditUserViewModel model)
        {
            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null) 
            {
                ViewBag.ErrorMessage = $"User with id = {model.Id} cannot be found";
                return RedirectToAction("NotFound");    
            }

            user.UserName = model.UserName;
            user.Email = model.Email;
            user.City = model.City;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded) 
            {
                return RedirectToAction("listusers");    
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("",error.Description);    
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUserAsync(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null) {
                ViewBag.ErrorMessage = $"User with id = {id} cannot be found";
                return RedirectToAction("NotFound");
            }
            var result = await userManager.DeleteAsync(user);
            if (result.Succeeded) 
            {
                return RedirectToAction("ListUsers");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("",error.Description);
            }

            return RedirectToAction("ListUsers");
        }

        [HttpPost]
        [Authorize(Policy = ("DeleteRolePolicy"))]
        public async Task<IActionResult> DeleteRoleAsync(string id)
        {
            var role = await roleManager.FindByIdAsync(id);
            if (role == null) {
                ViewBag.ErrorMessage = $"Role with id = {id} cannot be found";
                return RedirectToAction("NotFound");
            }
            try
            {
                var result = await roleManager.DeleteAsync(role);

                if (result.Succeeded) 
                {
                    return RedirectToAction("ListRoles");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("",error.Description);
                }

                return View("ListRoles");
            }
            catch (DbUpdateException ex)
            {
                logger.LogError($"Exception Occured : {ex}");

                ViewBag.ErrorTitle = $"{role.Name} role is in use";
                ViewBag.ErrorMessage = $"{role.Name} role cannot be deleted as there are users in this role."+
                                        $"If you want to delete this role,"+ 
                                        $"please remove the users from the role and then try to delete";
                return View("Error");
            }
            
        }

        [HttpGet]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRolesAsync(string userId)
        {
            ViewBag.userId = userId ;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null) {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return RedirectToAction("NotFound");
            }

            var model = new List<UserRolesViewModel>();

            foreach(var role in await roleManager.Roles.ToListAsync())
            {
                var userRolesViewModel = new UserRolesViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                };

                if (await userManager.IsInRoleAsync(user, role.Name)) 
                {
                    userRolesViewModel.IsSelected = true ;
                }
                else
                {
                    userRolesViewModel.IsSelected = false ;
                }

                model.Add(userRolesViewModel);
            }

            return View(model);
        }
        [HttpPost]
        [Authorize(Policy = "EditRolePolicy")]
        public async Task<IActionResult> ManageUserRolesAsync (List<UserRolesViewModel> model , string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return RedirectToAction("NotFound");    
            }

            var roles = await userManager.GetRolesAsync(user);

            var result = await userManager.RemoveFromRolesAsync(user,roles);

            if (!result.Succeeded) 
            {
                ModelState.AddModelError("","Cannot remove user existing roles");    
            }

            result = await userManager.AddToRolesAsync(user, model.Where(role => role.IsSelected).Select(r => r.RoleName));

            if (!result.Succeeded) 
            {
                ModelState.AddModelError("","Cannot add selected roles to user");    
            }

            return RedirectToAction("EditUser",new{ Id = userId});
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserClaimsAsync(string userId)
        {
            ViewBag.userId = userId ;

            var user = await userManager.FindByIdAsync(userId);

            if (user == null) {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return RedirectToAction("NotFound");
            }

            var existingUserClaims = await userManager.GetClaimsAsync(user);

            var model = new List<UserClaimsViewModel>();

            foreach(Claim claim in ClaimsStore.AllClaims)
            {
                var userClaimsViewModel = new UserClaimsViewModel
                {
                    ClaimType = claim.Type
                };

                if (existingUserClaims.Any(c => c.Type == claim.Type && c.Value == "true")) 
                {
                    userClaimsViewModel.IsSelected = true ;
                }
                else
                {
                    userClaimsViewModel.IsSelected = false ;
                }

                model.Add(userClaimsViewModel);
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ManageUserClaimsAsync (List<UserClaimsViewModel> model , string userId)
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                ViewBag.ErrorMessage = $"User with id = {userId} cannot be found";
                return RedirectToAction("NotFound");    
            }

            var claims = await userManager.GetClaimsAsync(user);

            var result = await userManager.RemoveClaimsAsync(user,claims);

            if (!result.Succeeded) 
            {
                ModelState.AddModelError("","Cannot remove user existing claims");    
            }

            result = await userManager.AddClaimsAsync(user,
                model.Select(c => new Claim(c.ClaimType, c.IsSelected ? "true" : "false")));

            if (!result.Succeeded) 
            {
                ModelState.AddModelError("","Cannot add selected claims to user");    
            }

            return RedirectToAction("EditUser",new{ Id = userId});
        }
    }
}