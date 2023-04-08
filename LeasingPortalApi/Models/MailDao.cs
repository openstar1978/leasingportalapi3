using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.IO;
namespace LeasingPortalApi.Models
{
    public class MailDao
    {
        public static string MailSend(string subject, string body, string emailto, byte[] bytes)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }

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
            //var smtp = new SmtpClient
            //{

            //    /*Host = "relay-hosting.secureserver.net",
            //    Port = 25,
            //    EnableSsl = false,*/

            //    /*Host = "smtp.gmail.com",
            //    Port = 587,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,*/
            //    Host = Host,
            //    Port = Port,
            //    EnableSsl = EnableSsl,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    //Credentials = new NetworkCredential(senderEmail.Address, password)
            //    Credentials = new NetworkCredential(smtpuser, smtppwd)
            //};
            using (var client = new SmtpClient(Host, Port))
            {

                // Pass SMTP credentials
                client.Credentials = new NetworkCredential(smtpuser, smtppwd);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    //Console.WriteLine("Attempting to send email...");
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        if (testemail == 1)
                        {
                            mess.CC.Add("testeys21@gmail.com");
                        }
                        if (bytes.Any())
                        {
                            mess.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(bytes), "Agreement.pdf", "application/pdf"));
                        }
                        client.Send(mess);
                    }

                    Console.WriteLine("Email sent!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);

                }
            }

            return "done";
        }
        public static string MailContactSend(string subject, string body, string emailto,string useremail, byte[] bytes)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }

            var senderEmail = new MailAddress("info@equipyourschool.co.uk", "EYS");
            var receiverEmail = new MailAddress(useremail, "Receiver");
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
            //var smtp = new SmtpClient
            //{

            //    /*Host = "relay-hosting.secureserver.net",
            //    Port = 25,
            //    EnableSsl = false,*/

            //    /*Host = "smtp.gmail.com",
            //    Port = 587,
            //    EnableSsl = true,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,*/
            //    Host = Host,
            //    Port = Port,
            //    EnableSsl = EnableSsl,
            //    DeliveryMethod = SmtpDeliveryMethod.Network,
            //    UseDefaultCredentials = false,
            //    //Credentials = new NetworkCredential(senderEmail.Address, password)
            //    Credentials = new NetworkCredential(smtpuser, smtppwd)
            //};
            using (var client = new SmtpClient(Host, Port))
            {

                // Pass SMTP credentials
                client.Credentials = new NetworkCredential(smtpuser, smtppwd);

                // Enable SSL encryption
                client.EnableSsl = true;

                // Try to send the message. Show status in console.
                try
                {
                    //Console.WriteLine("Attempting to send email...");
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    })
                    {
                        mess.CC.Add(emailto);
                        if (testemail == 1)
                        {
                            mess.CC.Add("testeys21@gmail.com");
                        }
                        if (bytes.Any())
                        {
                            mess.Attachments.Add(new System.Net.Mail.Attachment(new MemoryStream(bytes), "Agreement.pdf", "application/pdf"));
                        }
                        client.Send(mess);
                    }

                    Console.WriteLine("Email sent!");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("The email was not sent.");
                    Console.WriteLine("Error message: " + ex.Message);

                }
            }

            return "done";
        }
    }
}