using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PRO.Members.Models;
using System.Data.SqlClient;
using Stripe;

namespace PRO.Members.Controllers
{
    public class PayStripeController : Controller
    {
        // GET: PayStripe
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Submit Stripe Payment from payment page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult PaymentReceive(BufferRegister model)
        {
            try
            {
                string token = model.BufferTransaction.TransactionToken;
                //save return token update model in database
                string query = "update BufferRegister set TransactionToken = @TransactionToken where Email = @Email";
                using (AppDbContext context = new AppDbContext()) {
                    context.Database.ExecuteSqlCommand(query,
                              new SqlParameter("@Email", model.BufferContact.Email),
                              new SqlParameter("@TransactionToken", token));

                    //refresh model from DB
                    Helpers.DB objDB = new Helpers.DB();
                    model = objDB.GetBufferRegisterByPayToken(token, context);
                }

                string currency = "cad";
                int amt = int.Parse( Helpers.Tools.PriceFormat(model.BufferMembership.Price * 100));
                decimal tax = 13 / 100;

                //customer subscription
                var chargeCustomer = new StripeCustomerCreateOptions
                {
                    Email = model.BufferContact.Email,
                    Description = model.BufferMembership.Title,
                    PlanId = model.BufferMembership.Title,
                    SourceToken = model.BufferTransaction.TransactionToken,
                    TaxPercent = tax,
                    Metadata = new Dictionary<string, string>()
                                {
                                    { "Name", model.BufferContact.FirstName + " " + model.BufferContact.LastName }
                                }
                };
                var custServ = new StripeCustomerService();
                var stripeCustomer = custServ.Create(chargeCustomer);
                if (stripeCustomer != null) {
                    model.BufferTransaction.TransactionNumber = stripeCustomer.Id;
                    model.BufferTransaction.PayerID = stripeCustomer.DefaultSourceId;

                    using (AppDbContext context = new AppDbContext())
                    {
                        //validate transaction, update database
                        query = "update BufferRegister set PayerID = @PayerID, TransactionNumber=@TransactionNumber where Email = @Email";
                        context.Database.ExecuteSqlCommand(query,
                                  new SqlParameter("@Email", model.BufferContact.Email),
                                  new SqlParameter("@TransactionNumber", model.BufferTransaction.TransactionNumber),
                                  new SqlParameter("@PayerID", model.BufferTransaction.PayerID));

                        //buffer transfer, update database
                        query = "BufferTransfer @Email";
                        context.Database.ExecuteSqlCommand(query,
                                  new SqlParameter("@Email", model.BufferContact.Email));
                    }
                    //create application username
                    Helpers.MembUsers.CreateMember(model, new AuthDbContext());
                }

                ////send to Stripe
                //var charge = new StripeChargeCreateOptions
                //{
                //    Amount = amt,
                //    Currency = currency,
                //    ReceiptEmail = model.BufferContact.Email,
                //    Description = model.BufferMembership.Title,
                //    SourceTokenOrExistingSourceId = model.BufferTransaction.TransactionToken,
                //};

                //var chargeService = new StripeChargeService();
                //var stripeCharge = chargeService.Create(charge);
                //if (stripeCharge.Paid)
                //{
                //    model.BufferTransaction.TransactionNumber = stripeCharge.Id;
                //    model.BufferTransaction.PayerID = stripeCharge.Source.Id;

                //    using (AppDbContext context = new AppDbContext())
                //    {
                //        //validate transaction, update database
                //        query = "update BufferRegister set PayerID = @PayerID, TransactionNumber=@TransactionNumber where Email = @Email";
                //        context.Database.ExecuteSqlCommand(query,
                //                  new SqlParameter("@Email", model.BufferContact.Email),
                //                  new SqlParameter("@TransactionNumber", model.BufferTransaction.TransactionNumber),
                //                  new SqlParameter("@PayerID", model.BufferTransaction.PayerID));

                //        //buffer transfer, update database
                //        query = "BufferTransfer @Email";
                //        context.Database.ExecuteSqlCommand(query,
                //                  new SqlParameter("@Email", model.BufferContact.Email));
                //    }
                //    //create application username
                //    Helpers.MembUsers.CreateMember(model, new AuthDbContext());
                //}
                return View(model);
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                ModelState.AddModelError("CustomError", ex.Message);
                return View("Error");
            }
        }

        public ActionResult Error()
        {
            return View();
        }
    }
}