using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using PRO.Admin.Models;

[assembly: OwinStartupAttribute(typeof(PRO.Admin.Startup))]
namespace PRO.Admin
{
    public partial class Startup
    {
        //
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            bool bUserRoles = false;
            bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["InitUserRoles"].ToString(), out bUserRoles);
            if (bUserRoles) createRolesandUsers();
        }

        private void createRolesandUsers()
        {
            AuthDbContext context = new AuthDbContext();

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));


            // In Startup iam creating first Admin Role and creating a default Admin User    
            if (!roleManager.RoleExists("Admin"))
            {

                // first we create Admin rool   
                var role = new ApplicationRole("Admin");
                // Save the new Description property:
                role.Description = "Administrator Role";
                roleManager.Create(role);

                //var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                //role.Name = "Admin";
                //roleManager.Create(role);

                //Here we create a Admin super user who will maintain the website                  

                var user = new ApplicationUser();
                user.Email = "dan.pitu@gmail.com";
                user.UserName = user.Email;

                string userPWD = "xyz";

                var chkUser = UserManager.Create(user, userPWD);

                //Add default User to Role Admin   
                if (chkUser.Succeeded)
                {
                    var result1 = UserManager.AddToRole(user.Id, "Admin");

                }
            }

            // creating Creating Manager role    
            if (!roleManager.RoleExists("Manager"))
            {
                //var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                var role = new ApplicationRole();
                role.Name = "Manager";
                roleManager.Create(role);
            }

            // creating Creating Employee role    
            if (!roleManager.RoleExists("Employee"))
            {
                //var role = new Microsoft.AspNet.Identity.EntityFramework.IdentityRole();
                var role = new ApplicationRole("Employee");
                roleManager.Create(role);

            }
        }

    }
}
