using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PRO.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Stripe;

namespace PRO.UI.Controllers
{
    public class PayStripeController : Controller
    {

        public string connectionString = "Initial Catalog=TSTDB;Data Source=danlaptop;Password=xyz;Persist Security Info=True;User ID=usrone;";

        // GET: PayStripe
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// get form submit from ContactInfo
        /// Save information to Buffer
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult PaymentSave(MembershipContactInfoModel model)
        {
            if (model.GatewayName != "Stripe") {
                ModelState.AddModelError("CustomError", "Error: Wrong gateway channel");
                return View("Error");
            }

            try
            {
                ViewBag.ErrorMessage = "";
                if (!model.isContactValid())
                {
                    return RedirectToAction("Index", "Membership");
                }

                //check if model email exists
                AuthDbContext context = new AuthDbContext();
                var usrManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
                if (usrManager.FindByEmail(model.Email) != null)
                {
                    //already a member
                    ModelState.AddModelError("CustomError", "Error: Invalid information, this account already exists, please return and correct the information.");
                    return View("Error");
                }

                //save contact information into Buffer
                DBHelper.DBMembership.BufferRegisterContact(ref model);

                return View(model);
            }
            catch (Exception ex) {
                ModelState.AddModelError("CustomError", "Error: " + ex.Message);
                return View("Error");
            }
        }


        public ActionResult PaymentSend(MembershipContactInfoModel model) {
            //
            return View();
        }

        /// <summary>
        /// Submit Stripe Payment from payment page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult PaymentReceive(MembershipContactInfoModel model)
        {
            try {
                //save token update model
                DBHelper.DBMembership.BufferRegisterContact(ref model);
                DBHelper.DBMembership.UpdateTransactionToken(model);

                string currency = "cad";
                int amt = int.Parse(DBHelper.DBMembership.PriceFormat(model.Price * 100));

                var charge = new StripeChargeCreateOptions
                {
                    Amount = amt,
                    Currency = currency,
                    ReceiptEmail = model.Email,
                    Description = model.MembershipTitle,
                    SourceTokenOrExistingSourceId = model.TransactionToken,
                };
                var chargeService = new StripeChargeService();
                var stripeCharge = chargeService.Create(charge);

                ViewBag.ChargeID = stripeCharge.Id;
                model.TransactionNumber = stripeCharge.Id;
                model.PayerID = stripeCharge.Source.Id;
                if (stripeCharge.Paid) {
                    DBHelper.DBMembership.ValidateTransaction(model);
                    DBHelper.DBMembership.BufferTransfer(model);
                }
                return View();
            }
            catch (Exception ex) {
                Console.Write(ex.Message);
                return View("Error");
            }

        }

        public ActionResult Error()
        {
            return View();
        }

    }
}