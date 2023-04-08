using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;
using System.Web;
using System.Net;
using System.Web.Http.Routing;
using System.Net.Mail;

namespace LeasingPortalApi.Controllers
{
    public class HomeDisplayController : ApiController
    {
        [Route("api/HomeApi/GelAllMainCategories")]
        [HttpGet]
        public IHttpActionResult GetAllMainCategories()
        {
            List<ProductCategoryViewModel> categories = null;
            using (var ctx = new LeasingDbEntities())
            {
              categories = ctx.productcats.Select(s => new ProductCategoryViewModel()
                {
                    mcatname = s.mcatname,
                    mprodcatid = s.mprodcatid,
                    mpreviouscat = s.mpreviouscat.HasValue ? s.mpreviouscat.Value : 0,
                    
                }).OrderBy(x=>x.mcatname).ToList<ProductCategoryViewModel>();
            }
            if (categories.Count == 0)
            {
                return NotFound();
            }
            categories.ForEach(x =>
                {
                    x.mprodcaturl = "Category=" + HttpUtility.UrlEncode(x.mcatname) + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()));
                    x.mcaturl= "Dept=cat&a=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString())) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mcatname)) + "&aid="+0;
                }
            ) ;
            return Ok(categories);
        }


        [Route("api/HomeApi/GetMainCategories")]
        [HttpGet]
        public IHttpActionResult GetMainCategories()
        {
            var status = 1;
            List<ProductCategoryViewModel> categories = null;
            using (var ctx = new LeasingDbEntities())
            {
                categories = ctx.productcats.Select(s => new ProductCategoryViewModel()
                {
                    mcatname = s.mcatname,
                    mprodcatid = s.mprodcatid,
                    mpreviouscat = s.mpreviouscat.HasValue ? s.mpreviouscat.Value : 0,

                }).ToList<ProductCategoryViewModel>();
            }
            if (categories.Count == 0)
            {
                return NotFound();
            }
            categories.ForEach(x => x.mprodcaturl = "Category=" + HttpUtility.UrlEncode(x.mcatname) + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString())));
           
            return Ok(new { status=status,data=new { category = categories } });
        }
        [Route("api/HomeApi/SubscribeEmail")]
        [HttpGet]
        public IHttpActionResult SubscribeEmail(string email)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }
            var salutation = "Openstar School Leasing";
            //var senderEmail = new MailAddress(SessionWrapper.UserEmail, salutation);
            //var senderEmail = new MailAddress("technical@saicomputer.com", "Openstar School Lease");
            
            var body = "This is subscription email from EYS<br/><br/>";
            body += "Subscriber Email :" + email + "<br/>";
            byte[] b = { };
            if (MailDao.MailSend("Lessor School : You have subscription email", body, "info@equipyourschool.co.uk", b) == "done")
            {
                return Ok("done");
            }
            else
            {
                return Ok("");
            }
            
            // var pdfbinary= Convert.FromBase64String(data);
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            return Ok("done");



        }
    }
}
