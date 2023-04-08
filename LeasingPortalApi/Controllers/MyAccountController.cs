using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class MyAccountController : Controller
    {
        // GET: MyAccount
        public ActionResult Index(string tab, int? content)
        {
            ViewBag.msg = TempData["MSG"];
            ViewBag.activetab = content;
            return View();
            
        }
        public ActionResult ViewAgreement()
        {
            string Url = HttpContext.Request.Url.Host;
            if (Url == "www.saicomputer.com" || Url == "www.saicomputers.com")
            {
                Url = "G:/PleskVhosts/saicomputers.com/httpdocs/saicomputer.com/LeaseAdmin/Content/";
            }
            else if (Url == "www.equipyourschool.co.uk" || Url == "equipyourschool.co.uk" || Url == "www.equipyourschool.co.uk")
            {
                Url = HttpContext.Server.MapPath("/templates/");
            }
            else
            {
                Url = "E:/bhadreshkaka/openstar/website//LeasingPortalApi/LeasingPortalApi/LeasingPortalApi/templates/";
                //HttpContext.Current.Server.MapPath("~/Content/LeaseDoc/")
            }
            string path = Path.Combine(Url + "/EYS Standard Agreement.pdf");


            byte[] bytes = System.IO.File.ReadAllBytes(path);
            string base64String = Convert.ToBase64String(bytes, 0, bytes.Length);
            ViewBag.bytes = base64String;
            return View();
        }
    }
}