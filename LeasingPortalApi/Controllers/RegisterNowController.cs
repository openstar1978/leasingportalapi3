using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class RegisterNowController : Controller
    {
        // GET: EYSCampaign
        public ActionResult Index()
        {
            return View("Registration");
        }
        public ActionResult Registration()
        {
            return View();
        }
        public ActionResult Thankyou()
        {
            return View();
        }
        public ActionResult TermsConditions()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}