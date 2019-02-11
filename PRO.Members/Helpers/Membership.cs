using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PRO.Members.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRO.Members.Helpers
{
    public class MembUsers
    {

        public static bool UserExists(string email)
        {
            try {
                //check if model email exists
                AuthDbContext context = new AuthDbContext();
                var usrManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                if (usrManager.FindByEmail(email) != null)
                {
                    //already a member
                    return true;
                }
                else
                {
                    //email doesnt exist in the members area
                    return false;
                }
            }
            catch {
                throw;
            }
        }

        public static bool UserExists(string email, AuthDbContext ctx)
        {
            try
            {
                //check if model email exists
                var usrManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
                if (usrManager.FindByEmail(email) != null)
                {
                    //already a member
                    return true;
                }
                else
                {
                    //email doesnt exist in the members area
                    return false;
                }
            }
            catch 
            {
                throw;
                //return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool CreateMember(BufferRegister model)
        {
            AuthDbContext context = new AuthDbContext();

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            //Here we create a Admin super user who will maintain the website                  
            var user = new ApplicationUser();
            user.Email = model.BufferContact.Email;
            user.UserName = user.Email;
            string userPWD = "xyz";
            var chkUser = UserManager.Create(user, userPWD);

            //if (!RoleManager.RoleExists(model.BufferMembership.RoleName)) { }

            //Add default User to Role Admin   
            if (chkUser.Succeeded)
            {
                var result1 = UserManager.AddToRole(user.Id, model.BufferMembership.RoleName);
            }

            return true;
        }
        public static bool CreateMember(BufferRegister model, AuthDbContext ctx)
        {

            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(ctx));
            //Here we create a Admin super user who will maintain the website                  
            var user = new ApplicationUser();
            user.Email = model.BufferContact.Email;
            user.UserName = user.Email;
            string userPWD = "xyz";
            var chkUser = UserManager.Create(user, userPWD);

            //if (!RoleManager.RoleExists(model.BufferMembership.RoleName)) { }

            //Add default User to Role Admin   
            if (chkUser.Succeeded)
            {
                var result1 = UserManager.AddToRole(user.Id, model.BufferMembership.RoleName);
            }

            return true;
        }

    }
}