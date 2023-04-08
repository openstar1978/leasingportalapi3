using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class FeedBackController : Controller
    {
        // GET: FeedBack
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SaveFeedback(FeedbackViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (model.musername != null && model.museremail != null && model.muserdescription != null)
                {
                    //SessionWrapper.UserId = re.Id;
                    var salutation = "";
                    var db = new LeasingDbEntities();
                    //var senderEmail = new MailAddress(, salutation);
                    var emailto = model.museremail;

                    var senderEmail = new MailAddress("info@equipyourschool.co.uk", "EYS");
                    var receiverEmail = new MailAddress(emailto, "Receiver");
                    string smtpuser = "AKIATBV6YLKK3YGZU27A";
                    string smtppwd = "BB1ebiOgQsievGzCK2GKxRiDoAyJt6h0Fbab85Oauz4K";
                    string Host = "email-smtp.eu-west-2.amazonaws.com";
                    int Port = 587;
                    bool EnableSsl = true;
                    var getmailsetting = db.mailsettings.FirstOrDefault();
                    if (getmailsetting != null)
                    {
                        Host = getmailsetting.host;
                        smtpuser = getmailsetting.smtpu;
                        smtppwd = getmailsetting.smtpp;
                        Port = getmailsetting.port.Value;
                        EnableSsl = getmailsetting.enablessl == 1 ? true : false;
                    }
                    var smtp = new SmtpClient
                    {

                        /*Host = "relay-hosting.secureserver.net",
                        Port = 25,
                        EnableSsl = false,*/

                        /*Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,*/
                        Host = Host,
                        Port = Port,
                        EnableSsl = EnableSsl,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        //Credentials = new NetworkCredential(senderEmail.Address, password)
                        Credentials = new NetworkCredential(smtpuser, smtppwd)
                    };
                    var body = "Thank you for valuable feedback with <a href='https://www.equipyourschool.co.uk' >equipyourschool.co.uk</a><br/><br/>";
                    body += "Name:" + model.musername + "<br/>";
                    body += "Email: " + model.museremail + "<br/>";
                    body += "Description: " + model.muserdescription + "<br/><br/>";
                    body += "It you have any queries, please feel free to call us on xxxxxxxxxx.<br/><br/>";

                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = "Thank you for feedback with us",
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        mess.CC.Add("technical@saicomputer.com");
                        //mess.Attachments.Add(new Attachment(new MemoryStream(bytes), "Agreement.pdf", "application/pdf"));
                        smtp.Send(mess);
                    }

                    return Json(new { Data = model, msg = "Success" });
                    //return RedirectToAction("index", "Dashboard", new { area = re.Role });
                }
                else
                {
                    TempData["err"] = "Invalid Details";
                    return Json(new { msg = "Invalid" });
                }
            }
            else
            {
                TempData["err"] = "Invalid Details";
                return Json(new { msg = "Invalid" });
            }
            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult SaveNewsLetter(string nemail)
        {
            if (ModelState.IsValid)
            {
                if (nemail != null)
                {
                    //SessionWrapper.UserId = re.Id;
                    var salutation = "";
                    var db = new LeasingDbEntities();

                    //var senderEmail = new MailAddress(, salutation);
                    var searchdata = db.newsletters.FirstOrDefault(x => x.newsletter_email == nemail);
                    if (searchdata == null)
                    {
                        var newsletter = new newsletter
                        {
                            newsletter_email = nemail
                        };
                        db.newsletters.Add(newsletter);
                        db.SaveChanges();
                        var senderEmail = new MailAddress("info@equipyourschool.co.uk", "EYS");
                        var receiverEmail = new MailAddress(nemail, "Receiver");
                        string smtpuser = "AKIATBV6YLKK3YGZU27A";
                        string smtppwd = "BB1ebiOgQsievGzCK2GKxRiDoAyJt6h0Fbab85Oauz4K";
                        string Host = "email-smtp.eu-west-2.amazonaws.com";
                        int Port = 587;
                        bool EnableSsl = true;
                        var getmailsetting = db.mailsettings.FirstOrDefault();
                        if (getmailsetting != null)
                        {
                            Host = getmailsetting.host;
                            smtpuser = getmailsetting.smtpu;
                            smtppwd = getmailsetting.smtpp;
                            Port = getmailsetting.port.Value;
                            EnableSsl = getmailsetting.enablessl == 1 ? true : false;
                        }
                        var smtp = new SmtpClient
                        {

                            /*Host = "relay-hosting.secureserver.net",
                            Port = 25,
                            EnableSsl = false,*/

                            /*Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,*/
                            Host = Host,
                            Port = Port,
                            EnableSsl = EnableSsl,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            //Credentials = new NetworkCredential(senderEmail.Address, password)
                            Credentials = new NetworkCredential(smtpuser, smtppwd)
                        };
                        var body = "Thank you subscribing to our newsletter on <a href='https://www.equipyourschool.co.uk'>www.equipyourschool.co.uk</a><br/><br/>";
                        body += "Why don't you browse the first-ever leasing platform for schools created by a specialised education team in our industry.<br/><br/>";
                        //body += "It you have any queries, please feel free to call us on xxxxxxxxxx.<br/><br/>";
                        body+="<a href='https://equipyourschool.co.uk'><img class='mb-30' width='260' style='-webkit-user-drag:none !important;' src='https://www.equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' /></a>";
                        body += "<br/><br/>For unsubscripe <a href='https://www.equipyourschool.co.uk/feedback/unsubscribe?nemail=" + Encryption.Encrypt(nemail) + "'>Unsubscribe</a>";
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = "Thank you for subscription with us",
                            Body = body,
                            IsBodyHtml = true
                        })
                        {
                            mess.CC.Add("technical@saicomputer.com");
                            //mess.Attachments.Add(new Attachment(new MemoryStream(bytes), "Agreement.pdf", "application/pdf"));
                            smtp.Send(mess);
                        }
                    }
                    

                    return Json(new {msg = "Success" },JsonRequestBehavior.AllowGet);
                    //return RedirectToAction("index", "Dashboard", new { area = re.Role });
                }
                else
                {
                    TempData["err"] = "Invalid Details";
                    return Json(new { msg = "Invalid" }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                TempData["err"] = "Invalid Details";
                return Json(new { msg = "Invalid" }, JsonRequestBehavior.AllowGet);
            }
            return RedirectToAction("index");
        }
        [HttpGet]
        public ActionResult unsubscribe(string nemail)
        {
            try
            {
                var unsub = Encryption.Decrypt(nemail);
                var db = new LeasingDbEntities();
                var searchdata = db.newsletters.FirstOrDefault(x => x.newsletter_email == unsub);
                if (searchdata != null)
                {
                    db.Entry(searchdata).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return RedirectToAction("index", "leasing");
            }
            catch(Exception e)
            {
                return RedirectToAction("index","leasing");
            }
            
           
        }
    }
}