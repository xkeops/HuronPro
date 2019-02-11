using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using PRO.UI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PRO.UI.Controllers
{
    public class PayPalController : Controller
    {
        string endpoint = "https://api-3t.sandbox.paypal.com/nvp";
        string redirectUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token={0}";
        public string connectionString = "Initial Catalog=TSTDB;Data Source=danlaptop;Password=xyz;Persist Security Info=True;User ID=usrone;";


        // GET: PayPal
        public ActionResult Index()
        {
            return View();
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
        public ActionResult PaymentSave(MembershipContactInfoModel model)
        {
            //if (model.GatewayName != "PayPal") {
            //    //ModelState.AddModelError(string.Empty, "Wrong gateway channel, please contact your Administrator.");
            //    return View("Error");
            //}

            ViewBag.ErrorMessage = "";
            if (!model.isContactValid())
            {
                return RedirectToAction("Index", "Membership");
            }

            //check if model email exists
            AuthDbContext context = new AuthDbContext();
            var usrManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (usrManager.FindByEmail(model.Email) != null) {
                //already a member
                ViewBag.ErrorMessage = "Invalid information, this account already exists, please return and correct the information.";
            }

            if (!model.isContactValid())
            {
                ViewBag.ErrorMessage = "Invalid information, please return and fill out all the required information.";
                return View(model);
            }

            DBHelper.DBMembership.BufferRegisterContact(ref model);

            return View(model);
        }


        [ValidateAntiForgeryToken]
        public ActionResult PaymentSend(MembershipContactInfoModel model) {

            //get the information as saved from DB for security purposes
            // example: send the exact amount as indicated in the database, do not use model (model can be changed in browser inspect source etc)
            MembershipContactInfoModel dbModel = DBHelper.DBMembership.GetModelByEmail(model.Email);

            //string returnURL = "https://localhost:44354/PayPal/PaymentReceive";
            //string cancelURL = "https://localhost:44354/PayPal/PaymentCancel";

            string returnURL = "https://huronet.com/PayPal/PaymentReceive";
            string cancelURL = "https://huronet.com/PayPal/PaymentCancel";


            string Your_Test_Merchant_Account_User = "dan.p.b_api1.gmail-business.com";
            string Your_Test_Account_Password = "595NHDCZQUZGM4QZ";
            string Your_Test_Account_Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31Am.wVsyYEVgjySzw5m3Tj9q9KCk5";

            string nameProduct = HttpUtility.UrlEncode(dbModel.MembershipTitle);
            string descProduct = HttpUtility.UrlEncode( dbModel.MembershipDescription);

            double tax = 13 / 100;

            // This are the URLs the PayPal process uses. The endpoint URL is created using the NVP string generated below while the redirect url is where the page the user will navigate to when leaving PayPal plus the PayerID and the token the API returns when the request is made.
            string NVP = string.Empty;

            // API call method: add the desired checkout method. As I've mentioned above, we're using the express checkout.
            NVP += "METHOD=SetExpressCheckout";
            NVP += "&VERSION=123";

            // Credentials identifying you as the merchant
            NVP += "&USER="+ Your_Test_Merchant_Account_User;
            NVP += "&PWD="+ Your_Test_Account_Password;
            NVP += "&SIGNATURE="+ Your_Test_Account_Signature;

            // Redirect from PayPal portal
            NVP += "&RETURNURL=" + returnURL;   // Return URL from the PayPal portal for completed payment
            NVP += "&CANCELURL=" + cancelURL;   // Return URL from the PayPal portal for a cancelled purchase

            // Payment request information
            NVP += "&PAYMENTREQUEST_0_PAYMENTACTION=Sale";   // Type of transaction
            NVP += "&PAYMENTREQUEST_0_AMT=" + DBHelper.DBMembership.PriceFormat(dbModel.Price*(1+tax)); ;                 // Total payment for the transaction   
            NVP += "&PAYMENTREQUEST_0_ITEMAMT=" + DBHelper.DBMembership.PriceFormat(dbModel.Price); ;             // Purchased product price
            NVP += "&PAYMENTREQUEST_0_SHIPPINGAMT=0";         // Shipping amount
            NVP += "&PAYMENTREQUEST_0_HANDLINGAMT=0";         // Handling charges
            NVP += "&PAYMENTREQUEST_0_TAXAMT=" + DBHelper.DBMembership.PriceFormat(dbModel.Price * tax); ;             // Tax amount

            // Products involved in the transaction
            NVP += "&L_PAYMENTREQUEST_0_NAME0="+ nameProduct;                // Product name
            NVP += "&L_PAYMENTREQUEST_0_DESC0="+descProduct;    // Product description
            NVP += "&L_PAYMENTREQUEST_0_AMT0="+DBHelper.DBMembership.PriceFormat(dbModel.Price);                            // Product price
            NVP += "&L_PAYMENTREQUEST_0_QTY0=1";                            // Product quantity


            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            
            // Make the API call to the PayPal Service
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";
            request.ContentLength = NVP.Length;

            string sResponse = string.Empty;
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(NVP);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                sResponse = sr.ReadToEnd();
            }

            // Receive the token for the operation and redirect the user to the generated URL for the PayPal portal
            string token = string.Empty;
            string[] splitResponse = sResponse.Split('&');
            if (splitResponse.Length > 0)
            {
                foreach (string responseField in splitResponse)
                {
                    if (responseField.Contains("TOKEN"))
                    {
                        token = responseField.Substring(6);
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(token))
                {
                    //token receiving - payment sent ok 
                    model.TransactionToken = HttpUtility.UrlDecode(token);
                    DBHelper.DBMembership.UpdateTransactionToken(model);
                    //seesion is lost after redirect ??!
                    //Session["TransactionToken"] = token;
                    //Session["Payer"] = model;

                    redirectUrl = string.Format(redirectUrl, token);
                    //redirect to paypal website
                    Response.Redirect(redirectUrl);
                }
            }

            // If we get here, something went wrong;
            //lblError.Visible = true;            // Simple error handling :)


            return View();
        }

        public ActionResult PaymentReceive(string token,string payerID) {

            ViewBag.ErrorMessage = "";
            // First thing's first. We'll need to get the token and payer ID returned from the previous call:
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(payerID))
            {
                ViewBag.ErrorMessage = "Something went wrong, payment didn't go through the gateway.";
                return View();
            }

            string Your_Test_Merchant_Account_User = "dan.p.b_api1.gmail-business.com";
            string Your_Test_Account_Password = "595NHDCZQUZGM4QZ";
            string Your_Test_Account_Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31Am.wVsyYEVgjySzw5m3Tj9q9KCk5";

            MembershipContactInfoModel model = new MembershipContactInfoModel();

            MembershipContactInfoModel dbModel = DBHelper.DBMembership.GetModelByTransactionToken(token);
            string nameProduct = HttpUtility.UrlEncode(dbModel.MembershipTitle);
            string descProduct = HttpUtility.UrlEncode(dbModel.MembershipDescription);
            double tax = 13 / 100;

            try
            {

                // Than we add the tokens to string type variables as we'll need to rebuild the NVP string

                // Rebuilding the NVP string for the request; I've hardcoded the payment values again as this sample app does not have a database behind it.
                string NVP = string.Empty;

                NVP += "METHOD=DoExpressCheckoutPayment";
                NVP += "&VERSION=123";

                NVP += "&USER=" + Your_Test_Merchant_Account_User;
                NVP += "&PWD=" + Your_Test_Account_Password;
                NVP += "&SIGNATURE=" + Your_Test_Account_Signature;

                NVP += "&TOKEN=" + token;
                NVP += "&PAYERID=" + payerID;

                NVP += "&PAYMENTREQUEST_0_PAYMENTACTION=Sale";
                NVP += "&PAYMENTREQUEST_0_AMT="+ DBHelper.DBMembership.PriceFormat(dbModel.Price * (1 + tax));
                NVP += "&PAYMENTREQUEST_0_ITEMAMT="+ DBHelper.DBMembership.PriceFormat(dbModel.Price);
                NVP += "&PAYMENTREQUEST_0_SHIPPINGAMT=0";
                NVP += "&PAYMENTREQUEST_0_HANDLINGAMT=0";
                NVP += "&PAYMENTREQUEST_0_TAXAMT="+ DBHelper.DBMembership.PriceFormat(tax);

                // Making the API call
                string response = APICall(NVP);

                // Interpreting the response from PayPal; As a simple UI for checking the transaction, I'm displaying the transaction ID in the page on success so to make things easier when I'm checking the transaction log in PayPal's web UI.
                if (response.Contains("Success"))
                {
                    string transactionId = response.Substring(response.IndexOf("PAYMENTINFO_0_TRANSACTIONID"), response.IndexOf("&", response.IndexOf("PAYMENTINFO_0_TRANSACTIONID")) - response.IndexOf("PAYMENTINFO_0_TRANSACTIONID"));
                    string TransactNumber = transactionId.Split('=')[1];
                    ViewBag.PaymentStatus += transactionId;
                    model = DBHelper.DBMembership.CheckTransactionTokenReturn(token);
                    if (model.isContactValid()) {
                        model.PayerID = payerID;
                        model.TransactionNumber = TransactNumber;
                        //update transaction number and payerid to buffer
                        DBHelper.DBMembership.ValidateTransaction (model);
                        //create username
                        DBHelper.DBMembership.CreateMember(model);
                        //transfer Buffer into Contacts and the rest of tables
                        DBHelper.DBMembership.BufferTransfer(model);
                    }
                }
                else
                {
                    //model = (MembershipContactInfoModel)Session["Payer"];
                    ViewBag.ErrorMessage = "Something went wrong, payment didn't go through the gateway.";
                }
            }
            catch (Exception ex) {
                //model = (MembershipContactInfoModel)Session["Payer"];
                ViewBag.ErrorMessage = ex.Message;
            }


            return View(model);
    }

        private string APICall(string NVP)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";
            request.ContentLength = NVP.Length;

            string sResponse = string.Empty;
            using (StreamWriter sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(NVP);
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using (StreamReader sr = new StreamReader(response.GetResponseStream()))
            {
                sResponse = sr.ReadToEnd();
            }

            return sResponse;
        }



    }
}