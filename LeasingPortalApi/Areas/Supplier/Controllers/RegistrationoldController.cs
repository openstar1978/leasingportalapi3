using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeasingPortalApi.Models;
using System.IO;

namespace LeasingPortalApi.Areas.Supplier.Controllers
{
    public class RegistrationoldController : Controller
    {
        // GET: Seller/Registration
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public string uploadSupplierLogo()
        {
            HttpFileCollectionBase files = Request.Files;
            try
            {
                
                    HttpPostedFileBase file = files[0];
                    //string fname;
                    string Url = Request.Url.Host;
                    if (Url == "www.saicomputer.com" || Url == "www.saicomputers.com")
                    {
                        Url = "G:/PleskVhosts/saicomputers.com/httpdocs/saicomputer.com/LeaseAdmin/Content/SupplierLogo";
                    }
                    else if (Url == "www.ticklease.co.uk" || Url == "equipyourschool.co.uk")
                    {
                        Url = Server.MapPath("/Supplier/Content/SupplierLogo");
                    }
                    else
                    {

                        Url = "F:/palak/bhadreshkaka/openstar/websites/leasing/project/LeasingPortalSupplier/LeasingPortalSupplier/LeasingPortalSupplier/Content/SupplierLogo/";
                        //HttpContext.Current.Server.MapPath("~/Content/LeaseDoc/")
                    }
                    string fname;



                    fname = file.FileName;
                    var uniquename = DateTime.Now.Ticks;
                    var fileName = file.FileName;
                    var ext = Path.GetExtension(fname);
                    var myUniqueFileName = string.Format(@"{0}" + ext, DateTime.Now.Ticks);

                    bool exists = System.IO.Directory.Exists(Url);

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Url);
                    }
                    fname = Path.Combine(Url, myUniqueFileName);

                    file.SaveAs(fname);



                return myUniqueFileName;


            }
            catch (Exception ex)
            {
                return "";
            }
            
        }
        [HttpPost]
        //public ActionResult Register(SupplierViewModel model,supplierequipmentviewmodel equipment,suppliermoreviewmodel more,List<suppliercontactsViewmodel> contacts)
        public ActionResult Register(SupplierViewModel model)
        {

            var db = new LeasingDbEntities();
            var search = db.supplier_registration.FirstOrDefault(x => x.museridemail == model.museridemail);
            if (search == null)

            {
                if (SupplierDao.Adduser(model))
                {
                    TempData["msg"] = "Success";
                    TempData["data"] = "Category";
                    return Content("Success");
                }
            }
            else
            {
                return Content("Found");
            }
            return Content("Failed");

        }
        [HttpPost]
        public ActionResult checkUser(string email)
        {

            var db = new LeasingDbEntities();
            var search = db.supplier_registration.FirstOrDefault(x => x.museridemail == email);
            if (search == null)

            {
                return Content("failed");
            }
            else
            {
                return Content("Found");
            }
            return Content("Failed");

        }
        public ActionResult ThankYou()
        {
            return View();
        }
        public ActionResult TermsandConditions()
        {
            return View();
        }
    }
}