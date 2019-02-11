using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PRO.UI.Models;
//using Stripe;

namespace PRO.UI.Controllers
{
    public class MembershipController : Controller
    {

        /// <summary>
        /// GET: Membership
        /// listing all active subscriptions
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<MembershipModel> lstMembership = DBHelper.DBMembership.getActiveMemberships();
            return View(lstMembership);
        }

        /// <summary>
        /// GET from subscriptions page
        /// </summary>
        /// <param name="mid">membership id</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ContactInfo(int? mid)
        {
            if (mid==null){
                return RedirectToAction("Index");
            }
            ViewBag.MembershipId = mid.Value;
            MembershipModel model = DBHelper.DBMembership.getMembershipById(mid.Value);
            ViewBag.MembershipTitle = model.Title;
            ViewBag.Price = model.Price;

            Gateway modelGateway = DBHelper.DBGateway.getDefaultGateway();

            MembershipContactInfoModel memModel = new MembershipContactInfoModel();
            memModel.MembershipId = mid.Value;
            memModel.MembershipTitle = model.Title;
            memModel.Price = model.Price;
            memModel.GatewayId = modelGateway.GatewayId;
            memModel.GatewayName = modelGateway.GatewayName;

            return View(memModel);
        }

        /// <summary>
        /// POST back from save payment page
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ContactInfo(MembershipContactInfoModel model)
        {
            ViewBag.MembershipId = model.MembershipId;
            ViewBag.MembershipTitle = DBHelper.DBMembership.getMembershipById(model.MembershipId).Title;
            ViewBag.Price = model.Price;
            return View(model);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult MembershipPay(MembershipContactInfoModel model) {
            return View();
        }

    }
}