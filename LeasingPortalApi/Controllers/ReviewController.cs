using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class ReviewController : Controller
    {
        // GET: Review
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult AddReviewImage()
        {
            HttpFileCollectionBase files = Request.Files;

            List<string> filename = new List<string>();
            for (var j = 0; j < files.Count; j++)
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
                fname = Path.Combine(Server.MapPath("~/leaseadmin/Content/RatingDoc/"), myUniqueFileName);

                file.SaveAs(fname);
                filename.Add(fname);

            }
            if (filename.Any())
            {
                return Json(filename);
            }
            return Content("");
        }
        [HttpPost]
        public ActionResult SaveReview(ProductRatingViewModel model)
        {
            var db = new LeasingDbEntities();
            var prodid = int.Parse(model.emprodid);
            var search = db.productreviews.FirstOrDefault(x => x.muserid == model.muserid && x.mprodid == prodid);
            if (search == null)
            {
                var obj = new productreview
                {
                    mrankno = model.mrankno,
                    mprodid = prodid,
                    muserid = model.muserid,
                    mreviewtitle = model.mreviewtitle,
                    mcomments = model.mcomments,
                    mreviewdate = DateTime.Now,
                    mactive = 0
                };
                db.productreviews.Add(obj);
                db.SaveChanges();
                var reviewid = obj.mreviewid;
                if (model.files != null)
                {
                    if (model.files.Count > 0)
                    {
                        foreach (var a in model.files)
                        {
                            if (a != null && a != "")
                            {
                                var obj2 = new productreviewpic
                                {
                                    mreviewid = reviewid,
                                    mpicname = a,
                                };
                                db.productreviewpics.Add(obj2);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                

                TempData["msg"] = "Success";
                TempData["data"] = "Review";
                return Content("Success");

            }
            else
            {
                return Content("failed");
            }
            return Content("Failed");
            //return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult UpdateReview(ProductRatingViewModel model)
        {
            var db = new LeasingDbEntities();
            var prodid = int.Parse(model.emprodid);
            var search = db.productreviews.FirstOrDefault(x => x.muserid == model.muserid && x.mprodid == prodid);
            if (search != null)
            {

                search.mrankno = model.mrankno;
                search.mprodid = prodid;
                search.muserid = model.muserid;
                search.mreviewtitle = model.mreviewtitle;
                search.mcomments = model.mcomments;
                search.mreviewdate = DateTime.Now;
                db.Entry(search).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                var reviewid = search.mreviewid;
                if (model.files != null)
                {
                    if (model.files.Count > 0)
                    {
                        foreach (var a in model.files)
                        {
                            if (a != null && a != "")
                            {
                                var obj2 = new productreviewpic
                                {
                                    mreviewid = reviewid,
                                    mpicname = a,
                                };
                                db.productreviewpics.Add(obj2);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                TempData["msg"] = "Update";
                TempData["data"] = "Review";
                return Content("Success");

            }
            else
            {
                return Content("failed");
            }
            return Content("Failed");
            //return RedirectToAction("Index");
        }
        
        
    }
}
