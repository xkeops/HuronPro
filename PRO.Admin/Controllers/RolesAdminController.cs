using PRO.Admin.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;

namespace PRO.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesAdminController : Controller
    {
        public RolesAdminController()
        {
        }

        public RolesAdminController(ApplicationUserManager userManager,
            ApplicationRoleManager roleManager)
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
            set
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
        // GET: /Roles/
        public ActionResult Index()
        {
            return View(RoleManager.Roles);
        }

        //
        // GET: /Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            // Get the list of Users in this Role
            var users = new List<ApplicationUser>();

            // Get the list of Users in this Role
            foreach (var user in UserManager.Users.ToList())
            {
                if (await UserManager.IsInRoleAsync(user.Id, role.Name))
                {
                    users.Add(user);
                }
            }

            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }

        //
        // GET: /Users/Details/5
        public ActionResult ListUsersByRoleID(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role has not been specified!");
            }
            var role = RoleManager.FindById(id);

            if (role == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Role does not exist!");
            }

            // Get the list of Users in this Role
            var users = new List<ApplicationUser>();

            // Get the list of Users in this Role
            foreach (var user in UserManager.Users.ToList())
            {
                if (UserManager.IsInRole(user.Id, role.Name))
                {
                    users.Add(user);
                }
            }

            ViewBag.Users = users;
            ViewBag.UserCount = users.Count();
            return View(role);
        }

        //
        // GET: /Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Roles/Create
        [HttpPost]
        public async Task<ActionResult> Create(RoleViewModel roleViewModel)
        {
            if (ModelState.IsValid)
            {
                var role = new ApplicationRole(roleViewModel.Name);

                // Save the new Description property:
                role.Description = roleViewModel.Description;
                var roleresult = await RoleManager.CreateAsync(role);
                if (!roleresult.Succeeded)
                {
                    ModelState.AddModelError("", roleresult.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Roles/Edit/Admin
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            RoleViewModel roleModel = new RoleViewModel { Id = role.Id, Name = role.Name };

            // Update the new Description property for the ViewModel:
            roleModel.Description = role.Description;
            return View(roleModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Name,Id,Description")] RoleViewModel roleModel)
        {
            if (ModelState.IsValid)
            {
                var role = await RoleManager.FindByIdAsync(roleModel.Id);
                role.Name = roleModel.Name;

                // Update the new Description property:
                role.Description = roleModel.Description;
                await RoleManager.UpdateAsync(role);
                return RedirectToAction("Index");
            }
            return View();
        }

        //
        // GET: /Roles/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        public async Task<ActionResult> RemoveUserFromRole(string userid, string roleid) {
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

            return RedirectToAction("ListUsersByRoleID", new { id = roleid });
            //return View(role);
        }

        //
        // POST: /Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, string deleteUser)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                var role = await RoleManager.FindByIdAsync(id);
                if (role == null)
                {
                    return HttpNotFound();
                }
                IdentityResult result;
                if (deleteUser != null)
                {
                    result = await RoleManager.DeleteAsync(role);
                }
                else
                {
                    result = await RoleManager.DeleteAsync(role);
                }
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", result.Errors.First());
                    return View();
                }
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult SearchRoleBox(string userid, string rolename)
        {
            if (userid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not specified!");
            }
            var user = UserManager.FindById(userid);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "user does not exist!");
            }

            List<ApplicationRole> uList = new List<ApplicationRole>();
            if (string.IsNullOrEmpty(rolename))
            {
                uList = RoleManager.Roles.ToList();
            }
            else
            {
                uList = RoleManager.Roles.Where(r => r.Name.Contains(rolename)).ToList();
            }


            // Get the list of Users
            var roles = new List<ApplicationRole>();
            // Get the list of Users in this Role
            foreach (var role in uList)
            {
                roles.Add(role);
            }

            ViewBag.Roles = roles;
            ViewBag.RolesCount = roles.Count();
            ViewBag.User = user;
            return View();
        }

        [HttpGet]
        public ActionResult SearchRoleBox(string userid)
        {
            if (userid == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User not specified!");
            }
            var user = UserManager.FindById(userid);

            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "User does not exist!");
            }

            ViewBag.RolesCount = 0;
            ViewBag.User = user;
            return View();

        }

        //
        // Users/Edit/5
        public async Task<ActionResult> AddRoleToUser(string userid, string roleid)
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

            if (bRet)
            {
                //current userid is already in roleid 
                ViewBag.Success = "EXIST";
                ViewBag.Role = role;
                ViewBag.User = user;
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
            ViewBag.User = user;
            return View();
        }

    }
}
