using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LeasingPortalApi.Controllers.Api
{
    public class RatingApiController : ApiController
    {
        [HttpPost]
        [Route("api/RatingApi/UploadFile")]
        public IHttpActionResult UploadFile()
        {
            
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                string Url = HttpContext.Current.Request.Url.Host;
                if (Url == "www.saicomputer.com" || Url == "www.saicomputers.com")
                {
                    Url = "G:/PleskVhosts/saicomputers.com/httpdocs/saicomputer.com/LeaseAdmin/Content/RatingDoc";
                }
                else if (Url == "www.ticklease.co.uk" || Url == "equipyourschool.co.uk")
                {
                    Url = HttpContext.Current.Server.MapPath("/LeaseAdmin/Content/RatingDoc");
                }
                else
                {
                    Url = "F:/palak/bhadreshkaka/openstar/websites/leasing/project/LeasingPortalAdmin/LeasingPortalAdmin/LeasingPortalAdmin/Content/RatingDoc/";
                    //HttpContext.Current.Server.MapPath("~/Content/LeaseDoc/")
                }
                var files = HttpContext.Current.Request.Files;
                List<string> filename = new List<string>();

                for (var j = 0; j < files.Count; j++)
                {
                    var httpPostedFile = files[j];
                    string fname;


                    fname = httpPostedFile.FileName;
                    var uniquename = DateTime.Now.Ticks;
                    var fileName = httpPostedFile.FileName;
                    var ext = Path.GetExtension(fname);
                    var myUniqueFileName = string.Format(@"{0}" + ext, DateTime.Now.Ticks);
                    string createpath = "LeaseDoc";
                    bool exists = System.IO.Directory.Exists(Url);

                    if (!exists)
                    {
                        System.IO.Directory.CreateDirectory(Url);
                    }
                    fname = Path.Combine(Url, myUniqueFileName);
                    httpPostedFile.SaveAs(fname);
                    return Json(myUniqueFileName);
                    //filename.Add(myUniqueFileName);
                }

                
                    
                //}
                return Ok("");


            }
            return Ok("empty");
        }
        [HttpGet]
        [Route("api/RatingApi/DeleteReview")]
        public IHttpActionResult DeleteReview(int mreviewid)
        {
            var db = new LeasingDbEntities();
            //int id = int.Parse(Encryption.Decrypt(prodid));
            var searchdata = db.productreviews.FirstOrDefault(x => x.mreviewid==mreviewid);
            if (searchdata != null)
            {
                var reviewid = searchdata.mreviewid;
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                var searchimages = db.productreviewpics.Where(x => x.mreviewid == reviewid).ToList();
                if (searchimages.Any())
                {
                    foreach(var ab in searchimages)
                    {
                        var filePath = HttpContext.Current.Server.MapPath("/LeaseAdmin/Content/RatingDoc/" + ab.mpicname);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                        db.Entry(ab).State = System.Data.Entity.EntityState.Deleted;
                        db.SaveChanges();

                    }
                }

                return Ok("Success");
            }
            else
            {
                return Ok("empty");
            }
        }
        [HttpGet]
        [Route("api/RatingApi/DeleteImage")]
        public IHttpActionResult DeleteImage(int id)
        {
            var db = new LeasingDbEntities();
            var searchdata = db.productreviewpics.FirstOrDefault(x => x.mid == id);
            if (searchdata != null)
            {
                var filePath = HttpContext.Current.Server.MapPath("/LeaseAdmin/Content/RatingDoc/"+ searchdata.mpicname);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return Ok("Success");
            }
            else
            {
                return Ok("empty");
            }
        }
    }
}
