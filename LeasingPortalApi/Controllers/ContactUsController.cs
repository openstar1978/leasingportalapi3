using LeasingPortalApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class ContactUsController : Controller
    {
        // GET: ContactUs
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendContact(ContactViewModel cnt)
        {
            var client = new WebClient();
            var secretKey = "6LfJV9MkAAAAAIDXl8-N0GRju5YOkkcVsZFKMEn6";
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + cnt.Captcha_Response;
            var response = client.DownloadString(url);
            var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            if (result.Success == true)
            {
                if (cnt.Name!=null && cnt.ContactNo!=null)
                {
                    
                    var body = "Thank you for contacting with equipyourschool.co.uk<br/><br/>";
                    
                    body += "<table width='750' align='center' border='0' cellpadding='0' cellspacing='0' style='border: 1px solid #44266c;padding: 23px;'>" +
                        "<tr>" +
                        "<td align=left>Dear "+ cnt.Name + "</td>" +
                        "</tr>" +
                        "<tr height=30></tr>" +
                        "<tr>" +
                        "<td align=center>  <img src='https://equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log-dark.png' id='eyslogo'  height='60' border='0' alt='' align='center'></td>" +
                        "</tr>" +
                        "<tr height=10></tr>" +
                          "<tr align=center>" +
                        "<td><font size='3' color='#9900ff' face=calibri><strong>A portal strictly for Education only</strong></font></td>" +
                        "</tr>" +
                        "<tr height=30></tr>" +
                        "<tr align=left >" +
                        "<td><font size='3' color='' face=calibri>Thank you for contacting on equipyourschool.co.uk.</ font></td>" +
                        "</tr>" +
                        "<tr height=20></tr>" +
                        "<tr align=left >" +
                        "<td><font size='3' color='' face=calibri>We have received your following mesage/enquiry on " + DateTime.Now.ToString("dd/MM/yyyy") + " </ font></td>" +
                        "</tr>" +
                        "<tr align=left >" +
                        "<td>" +
                        "<table width=100% cellspacing='0' cellpadding='0' border='0' style='border:3px solid #dcdcdc;padding:5px;'>" +
                        "<tr><td><strong>Contact No :</strong></td><td>" + cnt.ContactNo + "</td></tr>" +
                        "<tr><td><strong>Email :</strong></td><td>" + cnt.Email + "</td></tr>" +
                        "<tr><td colspan='2'><strong>Message :</strong></td></tr>" +
                        "<tr><td colspan='2'>"+ cnt.Message + "</td></tr>" +
                        "</table>" +
                        "</td>"+
                        "</tr>" +
                        "<tr height=20></tr>" +
                        "<tr width=100%>" +
                    "<td><font size='3' color='' face=calibri>We will endeavour to response tp you within the next 24 hours</ font></td>" +
                    "</tr>" +
                    "<tr height=20></tr>" +
                        "<tr width=100%>" +
                    "<td><font size='3' color='' face=calibri>Thank you</ font></td>" +
                    "</tr>" +
                        "<tr height=20></tr>" +
                        "<tr width=100%>" +
                    "<td><font size='3' color='' face=calibri>Yours sincerely</font></td>" +
                    "</tr>" +
                    "<tr height=20 width=50%>" +
                    "<td><font size='3' color='' face=calibri>EYS Admin</font></td>" +
                    "</tr>" +
                        "</table>";
                    byte[] b = { };
                    MailDao.MailContactSend("Thank you for contacting on equipyourschool.co.uk", body,"info@equipyourschool.co.uk",cnt.Email, b);
                    return Json(new { msg = "Success", data = "LogIn" });
                }
                else
                {
                    TempData["err"] = "Please fill out all field";
                    return Json(new { msg = "Required" });
                }
            }
            else
            {
                TempData["err"] = "Captcha Validation Required!";
                return Json(new { msg = "CaptchaInvalid" });
            }
        }
    }
    public class ContactViewModel
    {
        public string  Name { get; set; }
        public string ContactNo { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public string SchoolName { get; set; }
        public string Captcha_Response { get; set; }

    }
    internal class ReCaptchaResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public string ValidatedDateTime { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}