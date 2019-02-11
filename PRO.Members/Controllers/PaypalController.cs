using PRO.Members.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;
using PRO.Members.Pay;

namespace PRO.Members.Controllers
{
    public class PaypalController : Controller
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

        decimal priceItem = 328.31m;
        string currency = "CAD";
        decimal tax = 13m / 100m;
        string sItemName = "tShirt man sport manchester";
        string sEmail = "john.dogg@gmail.com";
        string sPhone = "416-908-9222";
        string sCity = "Kingston";
        string sCountryCode = "CA";
        string sAddressLine1 = "123 Main st";
        string sAddressLine2 = "432 King St";
        string sPostalCode = "L2J 9K5";
        string sProvince = "ON";
        string sFirstName = "Johnny";
        string sLastName = "Mnemonic";

        public ActionResult CreateBillingPlan() {

            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/webapps/developer/docs/api/#create-a-plan
            var billingPlan = new Plan
            {
                name = "Tuts+ Plus",
                description = "Monthly plan for courses.",
                type = "fixed",
                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                merchant_preferences = new MerchantPreferences()
                {
                    setup_fee = clsPayPal.GetCurrency("0"), // $0
                    return_url = returnURL , // Retrieve from config
                    cancel_url = cancelURL, // Retrieve from config
                    auto_bill_amount = "YES",
                    initial_fail_amount_action = "CONTINUE",
                    max_fail_attempts = "0"
                },
                payment_definitions = new List<PaymentDefinition>
                    {
                        // Define a trial plan that will only charge $9.99 for the first
                        // month. After that, the standard plan will take over for the
                        // remaining 11 months of the year.
                        new PaymentDefinition()
                        {
                            name = "Trial Plan",
                            type = "TRIAL",
                            frequency = "MONTH",
                            frequency_interval = "1",
                            amount = clsPayPal.GetCurrency("0"), // Free for the 1st month
                            cycles = "1",
                            charge_models = new List<ChargeModel>
                            {
                                new ChargeModel()
                                {
                                    type = "TAX",
                                    amount = clsPayPal.GetCurrency("1.65") // If we need to charge Tax
                                },
                                new ChargeModel()
                                {
                                    type = "SHIPPING",
                                    amount = clsPayPal.GetCurrency("9.99") // If we need to charge for Shipping
                                }
                            }
                        },
                        // Define the standard payment plan. It will represent a monthly
                        // plan for $19.99 USD that charges once month for 11 months.
                        new PaymentDefinition
                        {
                            name = "Standard Plan",
                            type = "REGULAR",
                            frequency = "MONTH",
                            frequency_interval = "1",
                            amount = clsPayPal.GetCurrency("15.00"),
                            // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                            cycles = "11",
                            charge_models = new List<ChargeModel>
                            {
                                new ChargeModel
                                {
                                    type = "TAX",
                                    amount = clsPayPal.GetCurrency("2.47")
                                },
                                new ChargeModel()
                                {
                                    type = "SHIPPING",
                                    amount = clsPayPal.GetCurrency("9.99")
                                }
                            }
                        }
                    }
            };

            // Get a reference to the config
            var config = ConfigManager.Instance.GetProperties();

            // Use OAuthTokenCredential to request an access token from PayPal
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            Session["accesstoken"] = accessToken;

            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = config;

            Plan tstPlan = Plan.Get(apiContext, "P-9U027989WH84188135YIVBOI");


            // Create Plan
            var plan = billingPlan.Create(apiContext);

            //ACTIVATE PLAN
            // Activate the plan
            var patchRequest = new PatchRequest()
                    {
                        new Patch()
                        {
                            op = "replace",
                            path = "/",
                            value = new Plan() { state = "ACTIVE" }
                        }
                    };
            plan.Update(apiContext, patchRequest);

            return View();

        }

        public ActionResult TestRecurring()
        {

            string sPricePlan = Helpers.Tools.PriceFormat(priceItem);
            string sTaxAmt = Helpers.Tools.PriceFormat(priceItem * tax);
            //string sSubtotal = Helpers.Tools.PriceFormat(decimal.Parse(sPriceItem) - decimal.Parse(sTaxAmt));

            APIContext apiContext = clsPayPal.PayPalConfig.GetAPIContext();
            Session["accesstoken"] = apiContext.AccessToken;

            ChargeModel cModel = new ChargeModel() {
                amount = new Currency() {
                    currency = currency,
                    value = sTaxAmt 
                },
                type = "TAX"
            };

            //cModel = new ChargeModel()
            //{
            //    amount = new Currency()
            //    {
            //        currency = currency,
            //        value = sTaxAmt
            //    },
            //    type = "SHIPPING"
            //};

            List<ChargeModel> lstModels = new List<ChargeModel>();
            lstModels.Add(cModel);

            PaymentDefinition pDef = new PaymentDefinition() {
                amount= new Currency()
                {
                    currency = currency,
                    value = sPricePlan
                },
                cycles = "11",
                charge_models = lstModels,
                frequency="MONTH",
                frequency_interval="1",
                type="REGULAR",
                name="pmt recurring",
            };
            List<PaymentDefinition> lstPayDef = new List<PaymentDefinition>();
            lstPayDef.Add(pDef);

            MerchantPreferences merPref = new MerchantPreferences()
            {
                setup_fee = new Currency() {
                    currency = currency,
                    value = "1"
                },
                auto_bill_amount = "YES",
                max_fail_attempts = "0",
                initial_fail_amount_action = "CONTINUE",
                return_url = returnURL,
                cancel_url=cancelURL
            };

            Plan createdPlan = Plan.Get(apiContext, "P-4NL72850M8850524J7ZREBTY");
            if (createdPlan == null) {
                Plan myPlan = new Plan();
                myPlan.name = "Plan One";
                myPlan.description = "Plan One Description";
                myPlan.type = "fixed";
                myPlan.payment_definitions = lstPayDef;
                myPlan.merchant_preferences = merPref;
                var guid = Convert.ToString((new Random()).Next(100000));
                myPlan.merchant_preferences.return_url = Request.Url.ToString() + "?guid=" + guid;
                createdPlan = myPlan.Create(apiContext);
            }

            // Activate the plan
            var patchRequest = new PatchRequest()
            {
                new Patch()
                {
                    op = "replace",
                    path = "/",
                    value = new Plan() { state = "ACTIVE" }
                }
            };

            createdPlan.Update(apiContext, patchRequest);

            ShippingAddress shippAddr = new ShippingAddress() {
                line1 = "111 First Street",
                city = "Toronto",
                state = "ON",
                postal_code = "95070",
                country_code = "CA"
            };

            //Agreement newSub =clsPayPal.CreateBillingAgreement(hPlan.id, shippAddr, "some name", "some description", DateTime.Now);

            var agreement = new Agreement()
            {
                name = "some name",
                description = "some description",
                start_date = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                payer = new Payer()
                {
                    payment_method = "paypal",
                },
                plan = new Plan() { id = createdPlan.id },
                shipping_address = shippAddr,               
            };
            

            var createdAgreement = agreement.Create(apiContext);
            //return createdAgreement;



            /////

            //Details dtl = new Details()
            //{
            //    tax = sTaxAmt,
            //    subtotal = sSubtotal,
            //    shipping = "0",
            //};

            ////contact infor
            //Address addr = new Address()
            //{
            //    city = sCity,
            //    country_code = sCountryCode,
            //    line1 = sAddressLine1,
            //    line2 = sAddressLine2,
            //    phone = sPhone,
            //    postal_code = sPostalCode,
            //    state = sProvince,
            //};

            //PayerInfo payInfo = new PayerInfo()
            //{
            //    //billing_address = addr,
            //    //country_code= sCountryCode,
            //    first_name = sFirstName,
            //    last_name = sLastName,
            //    //middle_name ="",
            //    email = sEmail,
            //    //phone =sPhone,                                
            //};

            //Amount amnt = new Amount()
            //{
            //    currency = currency,
            //    total = sPricePlan,
            //    details = dtl,
            //};

            //List<Transaction> transactionList = new List<Transaction>();
            //Transaction tran = new Transaction()
            //{
            //    description = sItemName,
            //    custom = sItemName + " some additional information",
            //    amount = amnt
            //};
            //transactionList.Add(tran);

            //Payer payr = new Payer();
            //payr.payment_method = "paypal";
            //payr.payer_info = payInfo;

            //RedirectUrls redirUrls = new RedirectUrls();
            //redirUrls.cancel_url = "https://devtools-paypal.com/guide/pay_paypal/dotnet?cancel=true";
            ////redirUrls.return_url = "https://devtools-paypal.com/guide/pay_paypal/dotnet?success=true";
            //redirUrls.return_url = "https://localhost:44320/PayPal/ReturnTestRecurring";

            //Payment pymnt = new Payment();
            //pymnt.intent = "sale";
            //pymnt.payer = payr;
            //pymnt.transactions = transactionList;
            //pymnt.redirect_urls = redirUrls;

            //Payment createdPayment = pymnt.Create(apiContext);
            //string ApprovalURL = createdPayment.GetApprovalUrl();

            //Response.Redirect(ApprovalURL);

            return View();
        }

        public ActionResult ReturnTestRecurring(string token, string payerID, string paymentId)
        {
            string sTaxAmt = Helpers.Tools.PriceFormat(priceItem * tax);

            // Get a reference to the config
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = (string)Session["accesstoken"];

            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = config;

            Payment payment = new Payment();
            payment.id = paymentId;

            PaymentExecution pymntExecution = new PaymentExecution();
            pymntExecution.payer_id = payerID;

            Payment executedPayment = payment.Execute(apiContext, pymntExecution);

            return View();
        }

        public ActionResult Test() {

            //decimal subtotal = priceItem - priceItem * tax;
            string sPriceItem = Helpers.Tools.PriceFormat(priceItem);
            string sTax = Helpers.Tools.PriceFormat(priceItem * tax);
            string sSubtotal = Helpers.Tools.PriceFormat(decimal.Parse(sPriceItem) - decimal.Parse(sTax));

            // Get a reference to the config
            var config = ConfigManager.Instance.GetProperties();

            // Use OAuthTokenCredential to request an access token from PayPal
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            Session["accesstoken"] = accessToken;

            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = config;

            Details dtl = new Details()
            {
                tax = sTax,
                subtotal = sSubtotal,
                shipping= "0",
            };

            //contact infor
            Address addr = new Address() {
                city = sCity,
                country_code = sCountryCode,
                line1 = sAddressLine1,
                line2=sAddressLine2,
                phone=sPhone,
                postal_code=sPostalCode,
                state=sProvince,
            };

            PayerInfo payInfo = new PayerInfo() {                
                //billing_address = addr,
                //country_code= sCountryCode,
                first_name = sFirstName,
                last_name = sLastName,
                //middle_name ="",
                email =sEmail,
                //phone =sPhone,                                
            };

            Amount amnt = new Amount()
            {
                currency = currency,
                total = sPriceItem,
                details = dtl,
            };

            List<Transaction> transactionList = new List<Transaction>();
            Transaction tran = new Transaction() {
                description = sItemName,
                custom = sItemName + " some additional information",
                amount = amnt
            };
            transactionList.Add(tran);

            Payer payr = new Payer();
            payr.payment_method = "paypal";
            payr.payer_info = payInfo;       

            RedirectUrls redirUrls = new RedirectUrls();
            redirUrls.cancel_url = "https://devtools-paypal.com/guide/pay_paypal/dotnet?cancel=true";
            //redirUrls.return_url = "https://devtools-paypal.com/guide/pay_paypal/dotnet?success=true";
            redirUrls.return_url = "https://localhost:44320/PayPal/ReturnTest";

            Payment pymnt = new Payment();
            pymnt.intent = "sale";
            pymnt.payer = payr;
            pymnt.transactions = transactionList;
            pymnt.redirect_urls = redirUrls;

            Payment createdPayment = pymnt.Create(apiContext);
            string ApprovalURL = createdPayment.GetApprovalUrl();
            
            // saving the paymentID in the key guid
            //Session.Add(guid, createdPayment.id);

            Response.Redirect(ApprovalURL);

            return View();
        }

        public ActionResult ReturnTest(string token, string payerID, string paymentId)
        {
            ////contact
            //PayerInfo payInfo = new PayerInfo()
            //{
            //    //billing_address = addr,
            //    //country_code= sCountryCode,
            //    first_name = sFirstName,
            //    last_name = sLastName,
            //    //middle_name ="",
            //    email = sEmail,
            //    //phone =sPhone,                                
            //};
            //Payer pyr = new Payer() { payer_info = payInfo };

            // Get a reference to the config
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = (string)Session["accesstoken"];
            APIContext apiContext = new APIContext(accessToken);
            apiContext.Config = config;
                        
            Payment payment = new Payment(); 
            payment.id = paymentId;
            //payment.payer = pyr;

            PaymentExecution pymntExecution = new PaymentExecution();
            pymntExecution.payer_id = payerID;

            Payment executedPayment = payment.Execute(apiContext, pymntExecution);

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
                        Models.PayTransaction modelTrans = new Models.PayTransaction();
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
                        Models.PayTransaction modelTrans = new Models.PayTransaction();
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