using LeasingPortalApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class HelpCenterController : Controller
    {
        public string secretKey = "6LfJV9MkAAAAAIDXl8-N0GRju5YOkkcVsZFKMEn6";
        // GET: HelpCenter
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Ticket()
        {
            return View(TicketDao.Gets());
        }
        public ActionResult RequestCallback()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendRequest(ContactViewModel cnt)
        {
            var client = new WebClient();
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + cnt.Captcha_Response;
            var response = client.DownloadString(url);
            var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            if (result.Success == true)
            {
                if (cnt.Name != null && cnt.ContactNo != null)
                {

                    var body = "";
                    body += "<table><tr height=50></tr></table>";
                    body += "<table width=100%><tr>";
                    body += "<td align=center>  <img src='https://equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' id='eyslogo'  height='60' border='0' alt='' align='center'></td></tr>";
                    body += "<tr height=10></tr></table>";
                    body += "<table width=100%><tr align=center><td ><font size='4' color='' face=calibri>Below are the details for callback request.</font></td></tr>";
                    body += "<tr height=20></tr></table>";
                    body += "<table width=100%>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Name: <strong>" + cnt.Name + "</strong></font></b></td></tr>";
                    if (cnt.SchoolName != "")
                    {
                        body += "<tr align=center><td><font size='3' color='' face=calibri>School Name: <strong>" + cnt.SchoolName + "</strong></font></b></td></tr>";
                    }
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Contact No: <strong>" + cnt.ContactNo + "</strong></font></b></td></tr>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Contact Email: <strong>" + cnt.Email + "</strong></font></b></td></tr>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Message: <strong>" + cnt.Message + "</strong></font></b></td></tr>";
                    body += "</table>";
                    byte[] b = { };
                    MailDao.MailSend("Request a Call Back", body, "info@equipyourschool.co.uk", b);
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
        public ActionResult WriteUs()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendWriteUs(ContactViewModel cnt)
        {
            var client = new WebClient();
            var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + cnt.Captcha_Response;
            var response = client.DownloadString(url);
            var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

            if (result.Success == true)
            {
                if (cnt.Name != null && cnt.ContactNo != null)
                {

                    var body = "";
                    body += "<table><tr height=50></tr></table>";
                    body += "<table width=100%><tr>";
                    body += "<td align=center>  <img src='https://equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' id='eyslogo'  height='60' border='0' alt='' align='center'></td></tr>";
                    body += "<tr height=10></tr></table>";
                    body += "<table width=100%><tr align=center><td ><font size='4' color='' face=calibri>Below are the details for write us.</font></td></tr>";
                    body += "<tr height=20></tr></table>";
                    body += "<table width=100%>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Name: <strong>" + cnt.Name + "</strong></font></b></td></tr>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Contact No: <strong>" + cnt.ContactNo + "</strong></font></b></td></tr>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Contact Email: <strong>" + cnt.Email + "</strong></font></b></td></tr>";
                    body += "<tr align=center><td><font size='3' color='' face=calibri>Write Us: <strong>" + cnt.Message + "</strong></font></b></td></tr>";
                    body += "</table>";
                    byte[] b = { };
                    MailDao.MailSend("Write us from EYS", body, "info@equipyourschool.co.uk", b);
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
}