using PRO.Admin.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace PRO.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersAdminController : Controller
    {
        public UsersAdminController()
        {
        }

        public UsersAdminController(ApplicationUserManager userManager, ApplicationRoleManager roleManager)
        {
            UserManager = userManager;
            RoleManager = roleManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }

        //
        // GET: /Users/
        public async Task<ActionResult> Index()
        {
            return View(await UserManager.Users.ToListAsync());
        }

        //
        // GET: /Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);

            ViewBag.RoleNames = await UserManager.GetRolesAsync(user.Id);

            return View(user);
        }


        //
        // GET: /Users/Create
        public async Task<ActionResult> Create()
        {
            //Get the list of Roles
            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
            return View();
        }

        //
        // POST: /Users/Create
        [HttpPost]
        public async Task<ActionResult> Create(RegisterViewModel userViewModel, params string[] selectedRoles)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = userViewModel.Email,
                    Email =
                    userViewModel.Email,
                    // Add the Address Info:
                    //Address = userViewModel.Address,
                    //City = userViewModel.City,
                    //State = userViewModel.State,
                    //PostalCode = userViewModel.PostalCode
                };

                // Add the Address Info:
                //user.Address = userViewModel.Address;
                //user.City = userViewModel.City;
                //user.State = userViewModel.State;
                //user.PostalCode = userViewModel.PostalCode;

                // Then create:
                var adminresult = await UserManager.CreateAsync(user, userViewModel.Password);

                //Add User to the selected Roles 
                if (adminresult.Succeeded)
                {
                    if (selectedRoles != null)
                    {
                        var result = await UserManager.AddToRolesAsync(user.Id, selectedRoles);
                        if (!result.Succeeded)
                        {
                            ModelState.AddModelError("", result.Errors.First());
                            ViewBag.RoleId = new SelectList(await RoleManager.Roles.ToListAsync(), "Name", "Name");
                            return View();
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", adminresult.Errors.First());
                    ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
                    return View();

                }
                return RedirectToAction("Index");
            }
            ViewBag.RoleId = new SelectList(RoleManager.Roles, "Name", "Name");
            return View();
        }

        //
        // GET: /Users/Edit/1
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var userRoles = await UserManager.GetRolesAsync(user.Id);

            return View(new EditUserViewModel()
            {
                Id = user.Id,
                Email = user.Email,
                // Include the Addresss info:
                //Address = user.Address,
                //City = user.City,
                //State = user.State,
                //PostalCode = user.PostalCode,
                RolesList = RoleManager.Roles.ToList().Select(x => new SelectListItem()
                {
                    Selected = userRoles.Contains(x.Name),
                    Text = x.Name,
                    Value = x.Name
                })
            });
        }

        //
        // POST: /Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Email,Id")] EditUserViewModel editUser, params string[] selectedRole)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(editUser.Id);
                if (user == null)
                {
                    return HttpNotFound();
                }

                user.UserName = editUser.Email;
                user.Email = editUser.Email;
                //user.Address = editUser.Address;
                //user.City = editUser.City;
                //user.State = editUser.State;
                //user.PostalCode = editUser.PostalCode;

                var userRoles = await UserManager.GetRolesAsync(user.Id);

                selectedRole = selectedRole ?? new string[] { };

                var result = await UserManager.AddToRolesAsync(user.Id, selectedRole.Except(userRoles).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                result = await UserManager.RemoveFromRolesAsync(user.Id, userRoles.Except(selectedRole).ToArray<string>());

                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", "Something failed.");
            return View();
        }

        //
        // GET: /Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        //
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var user = await UserManager.FindByIdAsync(id);
                if (user == null)
                {
                    return HttpNotFound();
                }
                var result = await UserManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        public ActionResult RolesByUserID(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not specified!");
            }
            var user = UserManager.FindById(id);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User does not exist!");
            }

            // Get the list of Roles for this User
            var roles = new List<ApplicationRole>();

            // Get the list of Users in this Role
            foreach (var role in RoleManager.Roles.ToList())
            {
                if (UserManager.IsInRole(user.Id, role.Name))
                {
                    roles.Add(role);
                }
            }

            ViewBag.Roles = roles;
            ViewBag.RolesCount = roles.Count();
            return View(user);
        }

        public async Task<ActionResult> RemoveRoleFromUser(string userid, string roleid)
        {
            if (userid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not specified.");
            }
            if (roleid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role not specified.");
            }

            var role = await RoleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return HttpNotFound("Role not found.");
            }

            var user = await UserManager.FindByIdAsync(userid);
            if (user == null)
            {
                return HttpNotFound("User not found.");
            }

            IdentityResult deletionResult = await UserManager.RemoveFromRolesAsync(userid, role.Name);

            return RedirectToAction("RolesByUserID", new { id = userid });
            //return View(role);
        }

        [HttpPost]
        public ActionResult SearchUserBox(string roleid, string Email)
        {
            if (roleid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role not specified!");
            }
            var role = RoleManager.FindById(roleid);

            if (role == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role does not exist!");
            }

            List<ApplicationUser> uList = new List<ApplicationUser>();
            if (string.IsNullOrEmpty(Email))
            {
                uList = UserManager.Users.ToList();
            }
            else {
                uList = UserManager.Users.Where(r => r.Email.Contains (Email)).ToList();
            }

            
            // Get the list of Users
            var users = new List<ApplicationUser>();
            // Get the list of Users in this Role
            foreach (var user in uList)
            {
                users.Add(user);
            }

            ViewBag.Users = users;
            ViewBag.UsersCount = users.Count();
            ViewBag.Role = role;
            return View();
        }

        [HttpGet]
        public ActionResult SearchUserBox(string roleid)
        {
            if (roleid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role not specified!");
            }
            var role = RoleManager.FindById(roleid);

            if (role == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role does not exist!");
            }

            ViewBag.UsersCount = 0;
            ViewBag.Role = role;
            return View();

            //return View(role);
        }

        //
        // Users/Edit/5
        public async Task<ActionResult> AddUserToRole(string userid, string roleid)
        {
            var user = await UserManager.FindByIdAsync(userid);
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User does not exist!");
                //return HttpNotFound();
            }

            var role = await RoleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role does not exist!");
            }

            //var userRoles = await UserManager.GetRolesAsync(userid);

            var bRet = await UserManager.IsInRoleAsync(userid, role.Name);

            if (bRet) {
                //current userid is already in roleid 
                ViewBag.Success = "EXIST";
                ViewBag.Role = role;
                return View();
            }

            //string[] selectedRole = new string[] {roleid };

            var result = await UserManager.AddToRolesAsync(userid, role.Name);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", result.Errors.First());
                return View();
            }

            ViewBag.Success = "PASS";
            ViewBag.Role = role;
            return View();
        }



    }
}