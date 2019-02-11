using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PRO.Members.Models;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;
using System.Data.Common;

namespace PRO.Members.Controllers
{
    public class RegistrationController : Controller
    {
        // GET: Package
        public ActionResult Packages()
        {
            try {

                AppDbContext context = new AppDbContext();

                //var gateways = context.Database.SqlQuery<Gateway>("usp_GetAuthorByName @AuthorName",new SqlParameter("@AuthorName", "author"));
                //var affectedRows = context.Database.ExecuteSqlCommand("usp_CreateAuthor @AuthorName, @Email",
                //          new SqlParameter("@AuthorName", "author"),
                //          new SqlParameter("@Email", "email"));

                /*
                    var bookIdParameter = new SqlParameter();
                    bookIdParameter.ParameterName = "@BookId";
                    bookIdParameter.Direction = ParameterDirection.Output;
                    bookIdParameter.SqlDbType = SqlDbType.Int;
                    var authors = context.Database.ExecuteSqlCommand("usp_CreateBook @BookName, @ISBN, @BookId OUT",
                        new SqlParameter("@BookName", "Book"),
                        new SqlParameter("@ISBN", "ISBN"),
                        bookIdParameter);
                    Console.WriteLine(bookIdParameter.Value);
                 */
                string query = "select * from Membership where IsActive=1";
                List<Membership> lstMembership = context.Database.SqlQuery<Membership>(query).ToList();

                //var packages = context.Database.ExecuteSqlCommand("select * from Membership where IsActive=1");

                return View(lstMembership);
            }
            catch (Exception ex) {
                ModelState.AddModelError("CustomError", "Error: "+ ex.Message);
                return View("Error");
            }
        }

        public ActionResult BufferInit(BufferRegister model) {
            try {
                //BufferRegister model = new BufferRegister();

                using (AppDbContext context = new AppDbContext())
                {
                    string query;
                    //get selected membership
                    //query = "select * from Membership where MembershipId=@MembershipId";
                    //Membership mMembership = context.Database.SqlQuery<Membership>(query, new SqlParameter("@MembershipId", model.BufferMembership.MembershipID)).First();
                    //model.BufferMembership = mMembership;

                    //gateway
                    query = "select * from Gateways where IsDefault=1";
                    Gateway mGateway = context.Database.SqlQuery<Gateway>(query).First();
                    model.BufferGateway= mGateway;
                }
                return View(model);
            }
            catch (Exception ex) {
                ModelState.AddModelError("CustomError", "Error: "+ ex.Message);
                return View("Error");
                }
        }

        /// <summary>
        /// the goal of this function is to save information in BufferRegister
        /// do not allow registration if email is already a member - you get in trouble with creating a aspnetuser 
        /// 
        /// save contact information in BufferRegister table
        /// BufferRegisterContact calls a stored procedure that checks for email if it exists in the BufferRegister
        /// if it exists, only update with latest information, else inserts the line in buffer
        /// when payment received, the Buffer will be transferred into Contacts and a username will be created
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public ActionResult BufferSave(BufferRegister model)
        {
            try {

                //check if model email exists
                if (Helpers.MembUsers.UserExists(model.BufferContact.Email))
                {
                    throw new Exception("Invalid information, this account already exists, please return and correct the information.");
                }

                using (AppDbContext context = new AppDbContext())
                {
                    //save buffer model
                    string query = "BufferRegisterSave @Email, @FirstName, @LastName, @City, @MembershipId";

                    //*** this needs to change, call function and properly fill the model object (currently it doesnt get filled)
                    BufferRegister mMembership = context.Database.SqlQuery<BufferRegister>(query,
                            new SqlParameter("@Email", model.BufferContact.Email),
                            new SqlParameter("@FirstName", model.BufferContact.FirstName),
                            new SqlParameter("@LastName", model.BufferContact.LastName),
                            new SqlParameter("@City", model.BufferContact.City),
                            new SqlParameter("@MembershipId", model.BufferMembership.MembershipID))
                    .First();
                }
                return View(model);
            }
            catch (Exception ex) {
                ModelState.AddModelError("CustomError", "Error: " + ex.Message);
                return View("Error");
            }

        }

        [ValidateAntiForgeryToken]
        public ActionResult Paypal(BufferRegister model) {
            try {
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", "Error: " + ex.Message);
                return View("Error");
            }
        }

        [ValidateAntiForgeryToken]
        public ActionResult Paystripe(BufferRegister model)
        {
            try
            {
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", "Error: " + ex.Message);
                return View("Error");
            }
        }
    }
}