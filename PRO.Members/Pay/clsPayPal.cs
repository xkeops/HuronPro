using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PRO.Members.Pay
{

    public class clsPayPal
    {

        static string endpoint = "https://api-3t.sandbox.paypal.com/nvp";
        static string redirectUrl = "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token={0}";
        //string returnURL = "https://members.huronet.com/PayPal/PaymentReceive";
        //string cancelURL = "https://members.huronet.com/PayPal/PaymentCancel";
        static string returnURL = "https://localhost:44320/PayPal/PaymentReceive";
        static string cancelURL = "https://localhost:44320/PayPal/PaymentCancel";
        static string Your_Test_Merchant_Account_User = "dan.p.b_api1.gmail-business.com";
        static string Your_Test_Account_Password = "595NHDCZQUZGM4QZ";
        static string Your_Test_Account_Signature = "AFcWxV21C7fd0v3bYYYRCpSSRl31Am.wVsyYEVgjySzw5m3Tj9q9KCk5";

        decimal priceItem = 328.31m;
        static string currency = "CAD";
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

        public static class PlanType
        {
            public static readonly string Fixed = "Fixed";
        }

        public static class PayPalConfig{
            public static APIContext GetAPIContext() {
                // Get a reference to the config
                var config = ConfigManager.Instance.GetProperties();

                // Use OAuthTokenCredential to request an access token from PayPal
                var accessToken = new OAuthTokenCredential(config).GetAccessToken();

                APIContext apiContext = new APIContext(accessToken);
                apiContext.Config = config;
                return apiContext;
            }
        }

        public static Plan CreatePlanObject(string planName, string planDescription, string returnUrl, string cancelUrl,
    string frequency, int frequencyInterval, decimal planPrice,
    decimal shippingAmount = 0, decimal taxPercentage = 0, bool trial = false, int trialLength = 0, decimal trialPrice = 0)
        {
            // Define the plan and attach the payment definitions and merchant preferences.
            // More Information: https://developer.paypal.com/docs/rest/api/payments.billing-plans/
            return new Plan
            {
                name = planName,
                description = planDescription,
                type = PlanType.Fixed,

                // Define the merchant preferences.
                // More Information: https://developer.paypal.com/webapps/developer/docs/api/#merchantpreferences-object
                merchant_preferences = new MerchantPreferences()
                {
                    setup_fee = GetCurrency("1"),
                    return_url = returnUrl,
                    cancel_url = cancelUrl,
                    auto_bill_amount = "YES",
                    initial_fail_amount_action = "CONTINUE",
                    max_fail_attempts = "0"
                },
                payment_definitions = GetPaymentDefinitions(trial, trialLength, trialPrice, frequency, frequencyInterval, planPrice, shippingAmount, taxPercentage)
            };
        }

        private static List<PaymentDefinition> GetPaymentDefinitions(bool trial, int trialLength, decimal trialPrice,
            string frequency, int frequencyInterval, decimal planPrice, decimal shippingAmount, decimal taxPercentage)
        {
            var paymentDefinitions = new List<PaymentDefinition>();

            if (trial)
            {
                // Define a trial plan that will charge 'trialPrice' for 'trialLength'
                // After that, the standard plan will take over.
                paymentDefinitions.Add(
                    new PaymentDefinition()
                    {
                        name = "Trial",
                        type = "TRIAL",
                        frequency = frequency,
                        frequency_interval = frequencyInterval.ToString(),
                        amount = GetCurrency(trialPrice.ToString()),
                        cycles = trialLength.ToString(),
                        charge_models = GetChargeModels(trialPrice, shippingAmount, taxPercentage)
                    });
            }

            // Define the standard payment plan. It will represent a 'frequency' (monthly, etc)
            // plan for 'planPrice' that charges 'planPrice' (once a month) for #cycles.
            var regularPayment = new PaymentDefinition
            {
                name = "Standard Plan",
                type = "REGULAR",
                frequency = frequency,
                frequency_interval = frequencyInterval.ToString(),
                amount = GetCurrency(planPrice.ToString()),
                // > NOTE: For `IFNINITE` type plans, `cycles` should be 0 for a `REGULAR` `PaymentDefinition` object.
                cycles = "11",
                charge_models = GetChargeModels(trialPrice, shippingAmount, taxPercentage)
            };
            paymentDefinitions.Add(regularPayment);

            return paymentDefinitions;
        }

        private static List<ChargeModel> GetChargeModels(decimal planPrice, decimal shippingAmount, decimal taxPercentage)
        {
            // Create the Billing Plan
            var chargeModels = new List<ChargeModel>();
            if (shippingAmount > 0)
            {
                chargeModels.Add(new ChargeModel()
                {
                    type = "SHIPPING",
                    amount = GetCurrency(shippingAmount.ToString())
                });
            }
            if (taxPercentage > 0)
            {
                chargeModels.Add(new ChargeModel()
                {
                    type = "TAX",
                    amount = GetCurrency(String.Format("{0:f2}", planPrice * taxPercentage / 100))
                });
            }

            return chargeModels;
        }

        public static Currency GetCurrency(string amt)
        {
            return new Currency()
            {
                currency = currency,
                value = amt,
            };
        }

        /// <summary>
        /// 
        /// UPDATE:
        ///     UpdateBillingPlan(
        ///        planId: "P-5FY40070P6526045UHFWUVEI",
        ///        path: "/",
        ///        value: new Plan { description = "new description" }
        ///        );
        /// DELETE:       
        ///     UpdateBillingPlan(
        ///         planId: "P-5FY40070P6526045UHFWUVEI",
        ///         path: "/",
        ///         value: new Plan { state = "INACTIVE" });
        /// </summary>
        /// <param name="planId"></param>
        /// <param name="path"></param>
        /// <param name="value"></param>
        public static void UpdateBillingPlan(string planId, string path, object value)
        {
            // PayPal Authentication tokens
            var apiContext = PayPalConfig.GetAPIContext();

            // Retrieve Plan
            var plan = Plan.Get(apiContext, planId);

            // Activate the plan
            var patchRequest = new PatchRequest()
                    {
                        new Patch()
                        {
                            op = "replace",
                            path = path,
                            value = value
                        }
                    };
            plan.Update(apiContext, patchRequest);
        }

        public static Agreement CreateBillingAgreement(string planId, ShippingAddress shippingAddress,
    string name, string description, DateTime startDate)
        {
            // PayPal Authentication tokens
            var apiContext = PayPalConfig.GetAPIContext();
            
            var agreement = new Agreement()
            {
                name = name,
                description = description,
                start_date = startDate.ToString("yyyy-MM-ddTHH:mm:ss") + "Z",
                payer = new Payer() {
                    payment_method = "paypal",                    
                },
                plan = new Plan() { id = planId },
                shipping_address = shippingAddress
            };

            var createdAgreement = agreement.Create(apiContext);
            return createdAgreement;
        }
    }
}