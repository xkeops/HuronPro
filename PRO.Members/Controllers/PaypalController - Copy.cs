using PRO.Members.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PRO.Members.Controllers
{
    public class PaypalControllerCopy : Controller
    {

        string endpoint = "https://api-3t.sandbox.paypal.com/nvp";
        string redirectUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token={0}";
        public string connectionString = "Initial Catalog=TSTDB;Data Source=danlaptop;Password=xyz;Persist Security Info=True;User ID=usrone;";
        //string returnURL = "https://members.huronet.com/PayPal/PaymentReceive";
        //string cancelURL = "https://members.huronet.com/PayPal/PaymentCancel";
        string returnURL = "https://localhost:44320/PayPal/PaymentReceive";
        string cancelURL = "https://localhost:44320/PayPal/PaymentCancel";
        string Your_Test_Merchant_Account_User = "dan.p.b_api1.gmail-business.com";
        string Your_Test_Account_Password = "595NHDCZQUZGM4QZ";
        string Your_Test_Account_Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31Am.wVsyYEVgjySzw5m3Tj9q9KCk5";

        // GET: Paypal
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public ActionResult PaymentSend(BufferRegister model)
        {
            try
            {
                //get the information as saved from DB for security purposes
                // example: send the exact amount as indicated in the database, do not use model (model can be changed in browser inspect source etc)
                //BufferRegister dbModel = DBHelper.DBMembership.GetModelByEmail(model.Email);
                string query;
                string nameProduct = HttpUtility.UrlEncode(model.BufferMembership.Title);
                string descProduct = HttpUtility.UrlEncode(model.BufferMembership.Description);
                string currency = "CAD";
                decimal tax = (decimal)13 / 100;

                string sTax = Helpers.Tools.PriceFormat(tax);
                string Amount = Helpers.Tools.PriceFormat(model.BufferMembership.Price);
                string totalAmount = Helpers.Tools.PriceFormat(model.BufferMembership.Price * (1 + tax));
                string sTaxAmt = Helpers.Tools.PriceFormat(tax * model.BufferMembership.Price);


                // This are the URLs the PayPal process uses. The endpoint URL is created using the NVP string generated below while the redirect url is where the page the user will navigate to when leaving PayPal plus the PayerID and the token the API returns when the request is made.
                string NVP = string.Empty;

                // API call method: add the desired checkout method. As I've mentioned above, we're using the express checkout.
                NVP += "METHOD=SetExpressCheckout";
                NVP += "&VERSION=86";

                // Credentials identifying you as the merchant
                NVP += "&USER=" + Your_Test_Merchant_Account_User;
                NVP += "&PWD=" + Your_Test_Account_Password;
                NVP += "&SIGNATURE=" + Your_Test_Account_Signature;

                // Redirect from PayPal portal
                NVP += "&RETURNURL=" + returnURL;   // Return URL from the PayPal portal for completed payment
                NVP += "&CANCELURL=" + cancelURL;   // Return URL from the PayPal portal for a cancelled purchase

                NVP += "&L_BILLINGTYPE0=RecurringPayments";
                NVP += "&L_BILLINGAGREEMENTDESCRIPTION0=" + nameProduct;
                
                // Products involved in the transaction
                NVP += "&PAYMENTREQUEST_0_PAYMENTACTION=Sale";
                NVP += "&PAYMENTREQUEST_0_CURRENCYCODE=" + currency;         // Tax amount
                NVP += "&PAYMENTREQUEST_0_AMT=" + totalAmount;      // Total payment for the transaction   

                //L Payment
                NVP += "&L_PAYMENTREQUEST_0_AMT0=" + totalAmount;       // Product price
                //NVP += "&L_PAYMENTREQUEST_0_ITEMAMT0=" + Amount;       // Product price
                NVP += "&L_PAYMENTREQUEST_0_TAXAMT0=" + sTaxAmt;       // Product price
                NVP += "&L_PAYMENTREQUEST_0_QTY0=1";                            // Product quantity
                NVP += "&L_PAYMENTREQUEST_0_NAME0=" + nameProduct;                                                  // Product name

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
                        model.BufferTransaction.TransactionToken = HttpUtility.UrlDecode(token);
                        //save return token update model in database
                        using (AppDbContext context = new AppDbContext())
                        {
                            query = "update BufferRegister set TransactionToken = @TransactionToken where Email = @Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", model.BufferContact.Email),
                                      new SqlParameter("@TransactionToken", model.BufferTransaction.TransactionToken));
                        }

                        redirectUrl = string.Format(redirectUrl, token);
                        //redirect to paypal website
                        Response.Redirect(redirectUrl);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", ex.Message);
                return View("Error");
            }
        }
        [ValidateAntiForgeryToken]
        public ActionResult PaymentItemSend(BufferRegister model)
        {
            try
            {
                //get the information as saved from DB for security purposes
                // example: send the exact amount as indicated in the database, do not use model (model can be changed in browser inspect source etc)
                //BufferRegister dbModel = DBHelper.DBMembership.GetModelByEmail(model.Email);
                string query;
                string nameProduct = HttpUtility.UrlEncode(model.BufferMembership.Title);
                string descProduct = HttpUtility.UrlEncode(model.BufferMembership.Description);

                decimal tax = 13m / 100;

                // This are the URLs the PayPal process uses. The endpoint URL is created using the NVP string generated below while the redirect url is where the page the user will navigate to when leaving PayPal plus the PayerID and the token the API returns when the request is made.
                string NVP = string.Empty;

                // API call method: add the desired checkout method. As I've mentioned above, we're using the express checkout.
                NVP += "METHOD=SetExpressCheckout";
                NVP += "&VERSION=123";

                // Credentials identifying you as the merchant
                NVP += "&USER=" + Your_Test_Merchant_Account_User;
                NVP += "&PWD=" + Your_Test_Account_Password;
                NVP += "&SIGNATURE=" + Your_Test_Account_Signature;

                // Redirect from PayPal portal
                NVP += "&RETURNURL=" + returnURL;   // Return URL from the PayPal portal for completed payment
                NVP += "&CANCELURL=" + cancelURL;   // Return URL from the PayPal portal for a cancelled purchase

                // Payment request information
                NVP += "&PAYMENTREQUEST_0_PAYMENTACTION=Sale";   // Type of transaction
                NVP += "&PAYMENTREQUEST_0_AMT=" + Helpers.Tools.PriceFormat(model.BufferMembership.Price * (1 + tax));      // Total payment for the transaction   
                NVP += "&PAYMENTREQUEST_0_ITEMAMT=" + Helpers.Tools.PriceFormat(model.BufferMembership.Price);              // Purchased product price
                NVP += "&PAYMENTREQUEST_0_SHIPPINGAMT=0";         // Shipping amount
                NVP += "&PAYMENTREQUEST_0_HANDLINGAMT=0";         // Handling charges
                NVP += "&PAYMENTREQUEST_0_TAXAMT=" + Helpers.Tools.PriceFormat(model.BufferMembership.Price * tax);         // Tax amount

                // Products involved in the transaction
                NVP += "&L_PAYMENTREQUEST_0_NAME0=" + nameProduct;                                                  // Product name
                NVP += "&L_PAYMENTREQUEST_0_DESC0=" + descProduct;                                                  // Product description
                NVP += "&L_PAYMENTREQUEST_0_AMT0=" + Helpers.Tools.PriceFormat(model.BufferMembership.Price);       // Product price
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
                        model.BufferTransaction.TransactionToken = HttpUtility.UrlDecode(token);
                        //save return token update model in database
                        using (AppDbContext context = new AppDbContext())
                        {
                            query = "update BufferRegister set TransactionToken = @TransactionToken where Email = @Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", model.BufferContact.Email),
                                      new SqlParameter("@TransactionToken", model.BufferTransaction.TransactionToken));
                        }

                        redirectUrl = string.Format(redirectUrl, token);
                        //redirect to paypal website
                        Response.Redirect(redirectUrl);
                    }
                }
                return View();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", ex.Message);
                return View("Error");
            }
        }

        public ActionResult PaymentReceive(string token, string payerID)
        {
            try
            {
                string currency = "CAD";

                string query;
                //string payerID = payerID;
                // First thing's first. We'll need to get the token and payer ID returned from the previous call:
                //|| string.IsNullOrEmpty(payerID)
                if (string.IsNullOrEmpty(token) )
                {
                    ViewBag.ErrorMessage = "Something went wrong, payment didn't go through the gateway.";
                    return View();
                }


                //BufferRegister model = new BufferRegister();

                BufferRegister dbModel; //= DBHelper.DBMembership.GetModelByTransactionToken(token);
                Helpers.DB xObj = new Helpers.DB();
                dbModel = xObj.GetBufferRegisterByPayToken(token, new AppDbContext());

                string nameProduct = HttpUtility.UrlEncode(dbModel.BufferMembership.Title);
                string descProduct = HttpUtility.UrlEncode(dbModel.BufferMembership.Description);
                decimal tax = 13 / 100;
                string sTax = Helpers.Tools.PriceFormat(tax);
                string Amount = Helpers.Tools.PriceFormat(dbModel.BufferMembership.Price);
                string totalAmount = Helpers.Tools.PriceFormat(dbModel.BufferMembership.Price * (1 + tax));
                string sTaxAmt = Helpers.Tools.PriceFormat(tax * dbModel.BufferMembership.Price);

                // Than we add the tokens to string type variables as we'll need to rebuild the NVP string

                // Rebuilding the NVP string for the request;
                string NVP = string.Empty;
                NVP += "USER=" + Your_Test_Merchant_Account_User;
                NVP += "&PWD=" + Your_Test_Account_Password;
                NVP += "&SIGNATURE=" + Your_Test_Account_Signature;

                NVP += "&METHOD=CreateRecurringPaymentsProfile";
                NVP += "&VERSION=86";

                //product
                NVP += "&DESC=" + nameProduct;  //#Profile description - same value as a billing agreement description
                //NVP += "&NAME=" + nameProduct;  //#Profile description - same value as a billing agreement description

                //payment         
                NVP += "&TOKEN=" + token;
                NVP += "&TAXAMT=" + sTaxAmt;
                //NVP += "&INITAMT=" + Amount;
                NVP += "&BILLINGPERIOD=Year";  //    #Period of time between billings
                NVP += "&BILLINGFREQUENCY=1";   //    #Frequency of charges
                NVP += "&AMT=" + Amount;       //    #The amount the buyer will pay in a payment period
                NVP += "&MAXFAILEDPAYMENTS=3";  //    #Maximum failed payments before suspension of the profile
                NVP += "&NOSHIPPING=2";  //    #Maximum failed payments before suspension of the profile
                NVP += "&CURRENCYCODE=" + currency;

                // Products involved in the transaction
                NVP += "&PAYMENTREQUEST_0_AMT=" + totalAmount;      // Total payment for the transaction   
                NVP += "&PAYMENTREQUEST_0_ITEMAMT=" + Amount;              // Purchased product price
                NVP += "&PAYMENTREQUEST_0_TAXAMT=" + sTaxAmt;         // Tax amount
                NVP += "&PAYMENTREQUEST_0_QTY=1";         
                //NVP += "&L_PAYMENTREQUEST_0_NAME0=" + nameProduct;                                                  // Product name
                //NVP += "&L_PAYMENTREQUEST_0_DESC0=" + descProduct;                                                  // Product description
                //NVP += "&L_PAYMENTREQUEST_0_AMT0=" + totalAmount;       // Product price
                //NVP += "&L_PAYMENTREQUEST_0_QTY0=1";                            // Product quantity

                //Payer Information Fields
                NVP += "&EMAIL=" + dbModel.BufferContact.Email;

                NVP += "&SHIPTONAME=" + dbModel.BufferContact.FirstName + " " + dbModel.BufferContact.LastName;

                NVP += "&SUBSCRIBERNAME="+ dbModel.BufferContact.FirstName+" "+dbModel.BufferContact.LastName;
                NVP += "&PROFILEREFERENCE=INVOICENUMBERORSTUFF";
                NVP += "&PROFILESTARTDATE=" + DateTime.UtcNow.ToString(); //  #Billing date start, in UTC/GMT format
                NVP += "&FIRSTNAME="+dbModel.BufferContact.FirstName;
                NVP += "&LASTNAME=" + dbModel.BufferContact.LastName;
                NVP += "&STREET=1 Anything Street";
                NVP += "&ADDRESS1=1 Anything Street";
                NVP += "&CITY=" + dbModel.BufferContact.City;
                //NVP += "&STATE=ON";
                //NVP += "&ZIP=L4Z3U2";
                NVP += "&COUNTRYCODE=CA";
                NVP += "&BUSINESS=City of Hamilton";
                NVP += "&AUTOBILLAMT=AddToNextBilling";


                // Making the API call
                string response = APICall(NVP);

                // Interpreting the response from PayPal; As a simple UI for checking the transaction, I'm displaying the transaction ID in the page on success so to make things easier when I'm checking the transaction log in PayPal's web UI.
                if (response.Contains("Success"))
                {
                    string Profile = response.Substring(response.IndexOf("PROFILEID"), response.IndexOf("&", response.IndexOf("PROFILEID")) - response.IndexOf("PROFILEID"));
                    string TransactNumber = Profile.Split('=')[1];
                    ViewBag.PaymentStatus += TransactNumber;

                    //model = DBHelper.DBMembership.CheckTransactionTokenReturn(token);
                    if (dbModel.isBufferValid())
                    {
                        PayTransaction modelTrans = new PayTransaction();
                        modelTrans.PayerID = payerID;
                        modelTrans.TransactionNumber = TransactNumber;
                        dbModel.BufferTransaction = modelTrans;

                        using (AppDbContext context = new AppDbContext())
                        {
                            //validate transaction, update database
                            query = "update BufferRegister set PayerID=@PayerID, TransactionNumber=@TransactionNumber where Email=@Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", dbModel.BufferContact.Email),
                                      new SqlParameter("@TransactionNumber", dbModel.BufferTransaction.TransactionNumber),
                                      new SqlParameter("@PayerID", dbModel.BufferTransaction.PayerID));

                            //buffer transfer, update database
                            query = "BufferTransfer @Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", dbModel.BufferContact.Email));
                        }
                        //create application username
                        Helpers.MembUsers.CreateMember(dbModel, new AuthDbContext());

                    }
                }
                else
                {
                    //model = (MembershipContactInfoModel)Session["Payer"];
                    throw new Exception("Something went wrong, payment didn't go through the gateway.");
                }

                return View(dbModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", ex.Message);
                return View("Error");
            }



        }

        public ActionResult PaymentItemReceive(string token, string payerID)
        {
            try {

                string query;

                // First thing's first. We'll need to get the token and payer ID returned from the previous call:
                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(payerID))
                {
                    ViewBag.ErrorMessage = "Something went wrong, payment didn't go through the gateway.";
                    return View();
                }


                //BufferRegister model = new BufferRegister();

                BufferRegister dbModel; //= DBHelper.DBMembership.GetModelByTransactionToken(token);
                Helpers.DB xObj = new Helpers.DB();
                dbModel = xObj.GetBufferRegisterByPayToken(token, new AppDbContext());
                
               // using (AppDbContext context = new AppDbContext())
               // {
               //     query = "SELECT BufferRegister.*, Membership.Title, Membership.[Description], Membership.Price from BufferRegister " + 
               //             " LEFT JOIN Membership on Membership.MembershipID = BufferRegister.MembershipID " +
               //             " WHERE BufferRegister.TransactionToken = @TransactionToken ";
               //     dbModel = context.Database.SqlQuery<BufferRegister>(query, 
               //         new SqlParameter("@TransactionToken", token)
               //         ).First();
               //}

                string nameProduct = HttpUtility.UrlEncode(dbModel.BufferMembership.Title);
                string descProduct = HttpUtility.UrlEncode(dbModel.BufferMembership.Description);
                decimal tax = 13 / 100;
                string sTax = Helpers.Tools.PriceFormat(tax);
                string Amount = Helpers.Tools.PriceFormat(dbModel.BufferMembership.Price);
                string totalAmount = Helpers.Tools.PriceFormat(dbModel.BufferMembership.Price * (1+tax));


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
                NVP += "&PAYMENTREQUEST_0_AMT=" + totalAmount;
                NVP += "&PAYMENTREQUEST_0_ITEMAMT=" + Amount;
                NVP += "&PAYMENTREQUEST_0_SHIPPINGAMT=0";
                NVP += "&PAYMENTREQUEST_0_HANDLINGAMT=0";
                NVP += "&PAYMENTREQUEST_0_TAXAMT=" + sTax;

                // Making the API call
                string response = APICall(NVP);

                // Interpreting the response from PayPal; As a simple UI for checking the transaction, I'm displaying the transaction ID in the page on success so to make things easier when I'm checking the transaction log in PayPal's web UI.
                if (response.Contains("Success"))
                {
                    string transactionId = response.Substring(response.IndexOf("PAYMENTINFO_0_TRANSACTIONID"), response.IndexOf("&", response.IndexOf("PAYMENTINFO_0_TRANSACTIONID")) - response.IndexOf("PAYMENTINFO_0_TRANSACTIONID"));
                    string TransactNumber = transactionId.Split('=')[1];
                    ViewBag.PaymentStatus += transactionId;

                    //model = DBHelper.DBMembership.CheckTransactionTokenReturn(token);
                    if (dbModel.isBufferValid())
                    {
                        PayTransaction modelTrans = new PayTransaction();
                        modelTrans.PayerID = payerID;
                        modelTrans.TransactionNumber = TransactNumber;
                        dbModel.BufferTransaction = modelTrans;

                        using (AppDbContext context = new AppDbContext())
                        {
                            //validate transaction, update database
                            query = "update BufferRegister set PayerID=@PayerID, TransactionNumber=@TransactionNumber where Email=@Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", dbModel.BufferContact.Email),
                                      new SqlParameter("@TransactionNumber", dbModel.BufferTransaction.TransactionNumber),
                                      new SqlParameter("@PayerID", dbModel.BufferTransaction.PayerID));

                            //buffer transfer, update database
                            query = "BufferTransfer @Email";
                            context.Database.ExecuteSqlCommand(query,
                                      new SqlParameter("@Email", dbModel.BufferContact.Email));
                        }
                        //create application username
                        Helpers.MembUsers.CreateMember(dbModel, new AuthDbContext());

                    }
                }
                else
                {
                    //model = (MembershipContactInfoModel)Session["Payer"];
                    throw new Exception("Something went wrong, payment didn't go through the gateway.");
                }

                return View(dbModel);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("CustomError", ex.Message);
                return View("Error");
            }


 
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