using LeasingPortalApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    //[MyAuthentication(Roles = "User,Admin")]
    public class TicketController : Controller
    {
        // GET: Admin/Ticket

       
        //[HttpPost]
        //public ActionResult Useremail(int UserId)
        //{
        //    var search = LoginDao.Gets().FirstOrDefault(x => x.Id == UserId);
        //    var email = "";
        //    var mobile = "";
        //    if (search != null)
        //    {
        //        email = search.Email;
        //        mobile = search.Mobile;

        //    }

        //    return Json(new { email = email, mobile = mobile });

        //}
        public string secretKey = "6LfJV9MkAAAAAIDXl8-N0GRju5YOkkcVsZFKMEn6";
        public ActionResult Index()
        {
            return View(TicketDao.Gets());
        }
        [HttpPost]
        public ActionResult Attachment()
        {
            HttpFileCollectionBase files = Request.Files;


            for (int j = 0; j < files.Count; j++)
            {
                HttpPostedFileBase file = files[j];
                string fname;


                if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                {
                    string[] testfiles = file.FileName.Split(new char[] { '\\' });
                    fname = testfiles[testfiles.Length - 1];
                }
                else
                {
                    fname = file.FileName;
                }
                var uniquename = DateTime.Now.Ticks;
                var fileName = file.FileName;
                var ext = Path.GetExtension(fname);
                var myUniqueFileName = string.Format(@"{0}" + ext, DateTime.Now.Ticks);
                string createpath = "TicketAttachment";
                bool exists = System.IO.Directory.Exists(Server.MapPath("~/Content/Upload/" + createpath + ""));

                if (!exists)
                    System.IO.Directory.CreateDirectory(Server.MapPath("~/Content/Upload/" + createpath + ""));
                fname = Path.Combine(Server.MapPath("~/Content/Upload/TicketAttachment/"), myUniqueFileName);
                file.SaveAs(fname);


                return Content(myUniqueFileName);
            }
            return Content("");
        }
        [HttpPost]
        public ActionResult SaveData(TicketViewModel TicketData)
        {
            var client = new WebClient();
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + TicketData.Captcha_Response;
            var response = client.DownloadString(url);
            var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            if (result.Success == true)
            {
                if (TicketDao.Add(TicketData))
                {
                    return Content("Success");
                }
                else
                {
                    return Content("Failed");

                }
            }
            else
            {
                TempData["err"] = "Captcha Validation Required!";
                return Json(new { msg = "CaptchaInvalid" });
            }            
        }

        [HttpPost]
        public ActionResult LoadDataTable(string tab)
        {
            try
            {
                //Creating instance of DatabaseContext class
                using (var db = new LeasingDbEntities())
                {
                    var draw = Request.Form.GetValues("draw").FirstOrDefault();
                    var start = Request.Form.GetValues("start").FirstOrDefault();
                    var length = Request.Form.GetValues("length").FirstOrDefault();
                    var sortColumn = Request.Form.GetValues("columns[" + Request.Form.GetValues("order[0][column]").FirstOrDefault() + "][name]").FirstOrDefault();
                    var sortColumnDir = Request.Form.GetValues("order[0][dir]").FirstOrDefault();
                    var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                    //Paging Size (10,20,50,100)  
                    int pageSize = length != null ? Convert.ToInt32(length) : 0;
                    int skip = start != null ? Convert.ToInt32(start) : 0;
                    int recordsTotal = 0;
                    List<TicketViewModel> invoicedata = new List<TicketViewModel>();

                    // Getting all Customer data  
                    invoicedata = (from ps in db.TicketDetails
                                   where tab == "openticket" ? ps.Status == 1 : tab == "closeticket" ? ps.Status == 0 : ps.Status == null
                                   select new TicketViewModel
                                   {
                                       Id = ps.Id,                                       
                                       FullName = (ps.FullName == "" || ps.FullName == null ? "-" : ps.FullName),
                                       Department = (ps.Department == "" || ps.Department == null ? "-" : ps.Department),
                                       Subject = (ps.Subject == "" || ps.Subject == null ? "-" : ps.Subject)                                       
                                   }).ToList();

                    var invoiceData = invoicedata.AsQueryable();

                    ////Sorting  
                    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDir)))
                    {
                        invoiceData = invoiceData.OrderBy(sortColumn + " " + sortColumnDir);
                    }

                    //Search
                    if (!string.IsNullOrEmpty(searchValue))
                    {
                        invoiceData = invoiceData.Where(m => m.FullName.ToLower().Contains(searchValue.ToLower())
                        || m.Department.ToLower().Contains(searchValue.ToLower())
                        || m.Subject.ToLower().Contains(searchValue.ToLower())
                  );
                    }

                    //var colVal = Request.Form.GetValues("columns[" + i + "][search][value]").FirstOrDefault();

                    //total number of rows count  
                    if (invoiceData.Any())
                    {
                        recordsTotal = invoiceData.Count();
                    }

                    //Paging   
                    var data = invoiceData.Skip(skip).Take(pageSize).ToList();
                    //data.ForEach(x => x.Enc = Encryption.Encrypt(x.Id.ToString()));
                    //Returning Json Data  
                    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult ViewDetails(int Id)
        {
            return Json(TicketDao.GetById(Id));
        }

        [HttpPost]
        public ActionResult UpdateActionData(List<TicketViewModel> TicketData, string Action)
        {
            if (Action == "close" || Action == "open")
            {
                if (TicketDao.UpdateAction(TicketData))
                {
                    return Content("Success");
                }
                else
                {
                    return Content("Failed");
                }
            }
            else if (Action == "delete")
            {
                if (TicketDao.DeleteAction(TicketData))
                {
                    return Content("Success");
                }
                else
                {
                    return Content("Failed");
                }
            }
            else
            {
                return Content("Failed");
            }
        }
        [HttpPost]
        public ActionResult SaveReply(List<TicketReplyViewModel> records, string details)
        {
            if (TicketReplyDao.Save(records, details))
            {
                return Content("Success");
            }
            else
            {
                return Content("Failed");
            }
        }
        [HttpGet]
        public ActionResult ViewTicket(int id)
        {
            if (id != 0)
            {
                var model = TicketDao.getdata(id);
                return View(model);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}