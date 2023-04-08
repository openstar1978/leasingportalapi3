using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using LeasingPortalApi.Models;
using System.IO.Compression;
using System.ComponentModel;
using System.Web.Routing;
using System.Net.Mail;
using System.Net;
using Hangfire;
using Hangfire.Common;
using System.Web.Hosting;
using System.Threading;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.end;
using System.Text.RegularExpressions;
using DocuSign.eSign.Client;
using DocuSign.eSign.Api;
using DocuSign.eSign.Model;
using System.Diagnostics;

namespace LeasingPortalApi.Controllers
{
    public class OrderOldController : Controller
    {
        private string INTEGRATOR_KEY = "775d9b45-326c-4253-9d63-930da6899635";
        public static ManualResetEvent WaitForCallbackEvent = null;
        MyCredentialViewModel credential = new MyCredentialViewModel();

        // GET: Order
        public ActionResult Index()
        {
            return View();
        }
        public string loginApi(string usr, string pwd)
        {

            // we set the api client in global config when we configured the client  
            Configuration.Default.ApiClient = new ApiClient("https://demo.docusign.net/restapi");
            ApiClient apiClient = Configuration.Default.ApiClient;
            string authHeader = "{\"Username\":\"" + usr + "\", \"Password\":\"" + pwd + "\", \"IntegratorKey\":\"" + INTEGRATOR_KEY + "\"}";
            Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);
            // we will retrieve this from the login() results  
            string accountId = null;
            // the authentication api uses the apiClient (and X-DocuSign-Authentication header) that are set in Configuration object  
            AuthenticationApi authApi = new AuthenticationApi();
            LoginInformation loginInfo = authApi.Login();
            // find the default account for this user  
            foreach (DocuSign.eSign.Model.LoginAccount loginAcct in loginInfo.LoginAccounts)
            {
                if (loginAcct.IsDefault == "true")
                {
                    accountId = loginAcct.AccountId;
                    break;
                }
            }
            if (accountId == null)
            { // if no default found set to first account  
                accountId = loginInfo.LoginAccounts[0].AccountId;
            }
            return accountId;
        }
        [HttpPost]
        public ActionResult LoadOrder2(int mleaseid)
        {
            var db = new LeasingDbEntities();
            var content = "";
            var products = (from l in db.userleaseagreements
                            join o in db.usersubquotations on l.mleaseno equals o.magreementno
                            where l.mleaseid == mleaseid
                            select new exportproductviewmodel
                            {
                                msorderref = l.msorderref,
                                mleaseno = l.mleaseno,
                                subquoteids = o.msubquoteid,
                                ffee = l.ffee,
                                sfee = l.sfee,
                                muserid = l.muserid
                            }).ToList();
            var id = products.FirstOrDefault().muserid;
            string viewContent = ConvertViewToString("LoadOrders", products);
            content = content + viewContent;


            byte[] b = ExportPdfs(content);

            string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
            return Json(base64String);
        }
        [HttpPost]
        public ActionResult LoadOrder(int mleaseid)
        {
            var db = new LeasingDbEntities();
            var products = (from l in db.userleaseagreements
                            join o in db.usersubquotations on l.mleaseno equals o.magreementno
                            where l.mleaseid == mleaseid
                            select new exportproductviewmodel
                            {
                                mleaseno = l.mleaseno,
                                subquoteids = o.msubquoteid,
                                ffee = l.ffee,
                                sfee = l.sfee,
                                muserid = l.muserid
                            }).ToList();
            if (products.Any())
            {
                List<int> sp = new List<int>();
                foreach (var ab in products)
                {
                    sp.Add(ab.subquoteids);
                }
                ViewBag.mleaseno = products.First().mleaseno;
                ViewBag.subquoteid = sp;
                ViewBag.muserid = products.First().muserid;
                ViewBag.ffee = products.First().ffee;

                ViewBag.sfee = products.First().sfee;
                return View();
            }
            return View();
        }

        [HttpPost]
        public JsonResult LoadOrdersold(List<int> mleaseids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");
            var content = "";
            var email = "";
            if (mleaseids.Any())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var a in mleaseids)
                    {
                        var products = (from l in db.userleaseagreements
                                        join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                        where l.mleaseno == a
                                        select new exportproductviewmodel
                                        {
                                            msorderref = l.msorderref,
                                            mleaseno = l.mleaseno,
                                            subquoteids = o.msubquoteid,
                                            ffee = l.ffee,
                                            sfee = l.sfee,
                                            muserid = l.muserid
                                        }).ToList();
                        var id = products.FirstOrDefault().muserid;

                        email = db.user_registration.FirstOrDefault(x => x.musermoreid == id).museridemail;
                        string viewContent = ConvertViewToString("LoadOrders", products);
                        content = content + viewContent;

                        new Thread(() =>
                        {
                            TimeSpan span = DateTime.UtcNow.AddMinutes(10) - DateTime.UtcNow;
                            var jobid = (id.HasValue ? id.Value.ToString() : "") + a.ToString();
                            var manager = new RecurringJobManager();
                            string Url = "equipyourschool.co.uk";//Request.Url.Host;
                                                           //manager.AddOrUpdate(jobid, Hangfire.Common.Job.FromExpression(() => ReminderEmail(viewContent, email, a, jobid, products.Count, products.FirstOrDefault().msorderref,Url)), Cron.Daily());


                            var sendmail = SendThankYouemail(viewContent, email, a, products.Count, products.FirstOrDefault().msorderref, Url);
                            // Send the emails here
                        }).Start();


                    }

                }

                byte[] b = ExportPdfs(content);
                string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
                return Json(base64String);
            }
            return Json("empty");
            //return File(ms.ToArray(), "application/zip", "Attachments.zip");
        }
        [HttpPost]
        public JsonResult LoadOrder3(List<int> mleaseids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");
            var content = "";
            var email = "";
            if (mleaseids.Any())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var a in mleaseids)
                    {
                        var products = (from l in db.userleaseagreements
                                        join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                        where l.mleaseno == a
                                        select new exportproductviewmodel
                                        {
                                            msorderref = l.msorderref,
                                            mleaseno = l.mleaseno,
                                            subquoteids = o.msubquoteid,
                                            ffee = l.ffee,
                                            sfee = l.sfee,
                                            muserid = l.muserid
                                        }).ToList();
                        var id = products.FirstOrDefault().muserid;

                        email = db.user_registration.FirstOrDefault(x => x.musermoreid == id).museridemail;
                        string viewContent = ConvertViewToString("LoadOrders", products);
                        content = content + viewContent;

                        

                    }

                }

                return Json(content);
            }
            return Json("empty");
            //return File(ms.ToArray(), "application/zip", "Attachments.zip");
        }
        [HttpPost]
        public JsonResult LoadOrders(List<int> mleaseids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");
            var content = "";
            var email = "";
            var name = "";
            var mleaseid = 0;
            var muid = 0;
            if (mleaseids.Any())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var a in mleaseids)
                    {
                        var products = (from l in db.userleaseagreements
                                        join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                        where l.mleaseno == a
                                        select new exportproductviewmodel
                                        {
                                            mcontactid=l.mcontactid,
                                            mdefaultadd=l.mdefaultadd.HasValue?l.mdefaultadd.Value:0,
                                            mpayment=l.mpayment.HasValue?l.mpayment.Value:1,
                                            msorderdate = l.createddate,
                                            msorderref = l.msorderref,
                                            mleaseno = l.mleaseno,
                                            subquoteids = o.msubquoteid,
                                            ffee = l.ffee,
                                            sfee = l.sfee,
                                            muserid = l.muserid
                                        }).ToList();
                        var id = products.FirstOrDefault().muserid;
                        mleaseid = a;
                        var udata = db.user_registration.FirstOrDefault(x => x.musermoreid == id);
                        email = udata.museridemail;
                        name = udata.mname;
                        muid = udata.muid;
                        string viewContent = ConvertViewToString("LoadOrders", products);
                        content = content + viewContent;

                        new Thread(() =>
                        {
                            TimeSpan span = DateTime.UtcNow.AddMinutes(10) - DateTime.UtcNow;
                            var jobid = (id.HasValue ? id.Value.ToString() : "") + "Sign";
                            var manager = new RecurringJobManager();
                            string Url = "equipyourschool.co.uk";
                            //Request.Url.Host;
                            //manager.AddOrUpdate(jobid, Hangfire.Common.Job.FromExpression(() => ReminderEmail(viewContent, email, a, jobid, products.Count, products.FirstOrDefault().msorderref,products.FirstOrDefault().msorderdate,Url)), "00 9 * * *", TimeZoneInfo.Local);
                            var getjobdata = db.Hashes.FirstOrDefault(x => x.Key.Contains(jobid));
                            if (getjobdata == null)
                            {
                                manager.AddOrUpdate(jobid, Hangfire.Common.Job.FromExpression(() => ReminderSignature(viewContent, email, id.HasValue ? id.Value : 0, jobid, products.Count, products.FirstOrDefault().msorderref, products.FirstOrDefault().msorderdate, Url)), "30 9 * * *", TimeZoneInfo.Local);
                                
                            }
                            var newjobid = "LR" + "" + a.ToString();
                            getjobdata = db.Hashes.FirstOrDefault(x => x.Key.Contains(newjobid));
                            if (getjobdata == null)
                            {
                                manager.AddOrUpdate(newjobid, Hangfire.Common.Job.FromExpression(() => ReminderLessorOrderProcess(viewContent, email, id.HasValue ? id.Value : 0, newjobid, products.Count, products.FirstOrDefault().msorderref, products.FirstOrDefault().msorderdate, Url)), "30 9 * * *", TimeZoneInfo.Local);

                            }
                            //var sendmail = SendThankYouemail(viewContent, email, a, products.Count, products.FirstOrDefault().msorderref, Url);
                            // Send the emails here
                        }).Start();


                    }

                }

                byte[] b = ExportPdfs(content);
                try
                {
                    ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
                    Configuration.Default.ApiClient = apiClient;
                    string Url = Request.Url.Host;
                    /*string accountServerAuthUrl = apiClient.GetAuthorizationUri("37c40ceb-cad1-4168-bb3a-bfe4cea953e2", redirect_uri, true, stateOptional);
                    System.Diagnostics.Process.Start(accountServerAuthUrl);

                    WaitForCallbackEvent = new ManualResetEvent(false);

                    // Launch a self-hosted web server to accepte the redirect_uri call
                    // after the user finishes authentication.
                    using (WebApp.Start<Startup>("http://localhost:3000"))
                    {
                        Trace.WriteLine("WebServer Running. Waiting for access_token...");

                        // This waits for the redirect_uri to be received in the REST controller
                        // (see classes below) and then sleeps a short time to allow the response
                        // to be returned to the web browser before the server session ends.
                        WaitForCallbackEvent.WaitOne(60000, false);
                        Thread.Sleep(1000);
                    }*/

                    //Verify Account Details  
                    string accountId = loginApi(credential.UserName, credential.Password);
                    // Read a file from disk to use as a document.  
                    byte[] fileBytes = b;
                    EnvelopeDefinition envDef = new EnvelopeDefinition();
                    envDef.EmailSubject = "Please sign this EYS Agreement doc";
                    // Add a document to the envelope  
                    DocuSign.eSign.Model.Document doc = new DocuSign.eSign.Model.Document();
                    doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
                    doc.Name = "EYS Agreement";
                    doc.DocumentId = mleaseid.ToString();
                    envDef.Documents = new List<DocuSign.eSign.Model.Document>();
                    envDef.Documents.Add(doc);
                    // Add a recipient to sign the documeent  
                    DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
                    signer.Email = email;
                    signer.Name = name;
                    signer.RecipientId = muid.ToString();
                    signer.ClientUserId = muid.ToString() + "" + mleaseid.ToString();
                    signer.RoutingOrder = "1";
                    DocuSign.eSign.Model.Signer signer2 = new DocuSign.eSign.Model.Signer();
                    signer2.Email = "testeys21@gmail.com";
                    signer2.Name = "Test Eys";
                    signer2.RecipientId = Guid.NewGuid().ToString();
                    signer2.ClientUserId = "00" + "" + mleaseid.ToString();
                    signer2.RoutingOrder = "2";
                    signer.Tabs = new Tabs();
                    signer.Tabs.SignHereTabs = new List<SignHere>();
                    signer.Tabs.TextTabs = new List<Text>();
                    signer.Tabs.DateSignedTabs = new List<DateSigned>();
                    signer.Tabs.FullNameTabs = new List<FullName>();
                    SignHere signHere = new SignHere
                    {
                        AnchorString = "Signature of hirer(s):",
                        AnchorYOffset = "20",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "20",
                        Width = "80",
                        Height = "15",
                    };
                    SignHere signHere2 = new SignHere
                    {
                        AnchorString = "Signature X",
                        AnchorYOffset = "-10",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "50",
                        Width = "80",
                        Height = "10",

                    };
                    SignHere signHere3 = new SignHere
                    {
                        AnchorString = "Signature:",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "55",
                        Width = "80",
                        Height = "10",

                    };
                    FullName sprintname = new FullName
                    {
                        AnchorString = "Print Name:",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "55",
                        Width = "80",
                        Height = "10",

                    };
                    Text sposition = new Text
                    {
                        AnchorString = "Position:",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "40",
                        Width = "80",
                        Height = "10",
                        
                    };
                    DateSigned sdatelast = new DateSigned
                    {
                        AnchorString = "Date:",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "30",
                        Width = "80",
                        Height = "10",
                        TabLabel = "Date"
                    };
                    DateSigned sdate = new DateSigned
                    {
                        AnchorString = "Date __",
                        AnchorYOffset = "-3",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "15",
                        Width = "80",
                        Height = "10",
                        TabLabel = "Date",
                        
                    };
                    DateSigned sdate2 = new DateSigned
                    {
                        AnchorString = "_ Date_",
                        AnchorYOffset = "-3",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "20",
                        Width = "80",
                        Height = "10",
                        TabLabel = "Date"
                    };
                    signer.Tabs.SignHereTabs.Add(signHere);
                    signer.Tabs.SignHereTabs.Add(signHere2);
                    signer.Tabs.SignHereTabs.Add(signHere3);
                    signer.Tabs.DateSignedTabs.Add(sdate);
                    signer.Tabs.DateSignedTabs.Add(sdate2);
                    signer.Tabs.FullNameTabs.Add(sprintname);
                    signer.Tabs.TextTabs.Add(sposition);
                    signer.Tabs.DateSignedTabs.Add(sdatelast);
                    Text item = new Text
                    {
                        AnchorString = "AGREEMENT NO.",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "70",
                        Width = "80",
                        Height = "10", 
                        TabLabel="TR No"
                    };
                    Date rdate = new Date
                    {
                        AnchorString = "Commencement Date",
                        AnchorYOffset = "-3",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "70",
                        Width = "80",
                        Height = "10",
                        TabLabel = "Date"
                    };
                    SignHere RsignHere = new SignHere
                    {
                        AnchorString = "Signature__",
                        AnchorYOffset = "-10",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "50",
                        Width = "80",
                        Height = "10",

                    };
                    
                    signer2.Tabs = new Tabs();
                    signer2.Tabs.SignHereTabs = new List<SignHere>();
                    signer2.Tabs.SignHereTabs.Add(RsignHere);
                    signer2.Tabs.TextTabs = new List<Text>();
                    signer2.Tabs.TextTabs.Add(item);
                    signer2.Tabs.DateTabs = new List<Date>();
                    signer2.Tabs.DateTabs.Add(rdate);

                    envDef.Recipients = new DocuSign.eSign.Model.Recipients();
                    envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
                    envDef.Recipients.Signers.Add(signer);
                    envDef.Recipients.Signers.Add(signer2);
                    envDef.Status = "sent";
                    EnvelopesApi envelopesApi = new EnvelopesApi();

                    EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
                    string clientUserId = muid.ToString() + "" + mleaseid.ToString();
                    // get a URL that can be placed in a browser or embedded in an IFrame
                    string returnUrl = "http://localhost:15000/Order/ResponseData";
                    if (Url == "www.saicomputer.com" || Url == "www.ticklease.com" || Url == "ticklease.com" || Url == "www.ticklease.co.uk" || Url == "ticklease.co.uk" || Url == "equipyourschool.co.uk")
                    {
                        returnUrl = "https://www.equipyourschool.co.uk/Order/ResponseData";
                    }
                    RecipientViewRequest recipientView = new RecipientViewRequest()
                    {
                        ReturnUrl = returnUrl,
                        ClientUserId = clientUserId,
                        AuthenticationMethod = "email",
                        UserName = envDef.Recipients.Signers[0].Name,
                        Email = envDef.Recipients.Signers[0].Email
                    };
                    ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, envelopeSummary.EnvelopeId, recipientView);
                    var leasedata = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseid);
                    if (leasedata != null)
                    {
                        leasedata.menvelopeid = envelopeSummary.EnvelopeId;
                        db.Entry(leasedata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                    ViewBag.url = viewUrl.Url;
                    return Json(new { url = viewUrl.Url, accountId = accountId, envelopid = envelopeSummary.EnvelopeId, mleaseid = mleaseid });

                }
                catch (DocuSign.eSign.Client.ApiException apiEx)
                {
                    return Json(apiEx.Message.ToString());

                }
                //string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
                //return Json(base64String);
            }
            return Json("empty");
            //return File(ms.ToArray(), "application/zip", "Attachments.zip");
        }
        public ActionResult ResponseData()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoadDocument(string accountId, string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            EnvelopesApi envelopesApi = new EnvelopesApi();
            String filePath = String.Empty;
            FileStream fs = null;
            // GetDocument() API call returns a MemoryStream
            //Envelope envelope = envelopesApi.GetEnvelope(accountId, envelopId, null);
            var mleaseno = int.Parse(documentid);
            var email = ""; var name = ""; var muid = 0;
            MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, envelopId, documentid);
            // let's save the document to local file system
            byte[] b = docStream.ToArray();
            string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
            Recipients recipients = envelopesApi.ListRecipients(accountId, envelopId);

            if (recipients.Signers.Count > 0)
            {

                var products = (from l in db.userleaseagreements
                                join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                where l.mleaseno == mleaseno
                                select new exportproductviewmodel
                                {
                                    msorderref = l.msorderref,
                                    mleaseno = l.mleaseno,
                                    subquoteids = o.msubquoteid,
                                    ffee = l.ffee,
                                    sfee = l.sfee,
                                    muserid = l.muserid
                                }).ToList();
                var id = products.FirstOrDefault().muserid;
                var udata = db.user_registration.FirstOrDefault(x => x.musermoreid == id);
                email = udata.museridemail;
                name = udata.mname;
                muid = udata.muid;
                if (recipients.Signers[0].Email == email && recipients.Signers[0].Status == "completed")
                {
                    var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                    if (leasedatas != null)
                    {
                        leasedatas.mstatus = "partial";
                        db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    var sendmail = SendThankYouemailBinaryFile(b, email, mleaseno, products.Count, products.FirstOrDefault().msorderref, "");
                }



            }
            return Json(base64String);
        }
        [HttpPost]
        public ActionResult PendingDocument(string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            string Url = Request.Url.Host;
            string accountId = loginApi(credential.UserName, credential.Password);
            EnvelopesApi envelopesApi = new EnvelopesApi();
            String filePath = String.Empty;
            FileStream fs = null;
            // GetDocument() API call returns a MemoryStream
            Envelope envelope = envelopesApi.GetEnvelope(accountId, envelopId, null);
            var mleaseno = int.Parse(documentid);
            var email = ""; var name = ""; var muid = 0;
            var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
            if (leasedatas != null)
            {
                var udata = db.user_registration.FirstOrDefault(x => x.musermoreid == leasedatas.muserid);
                email = udata.museridemail;
                name = udata.mname;
                muid = udata.muid;
                if (envelope.Status != "completed")
                {
                    Recipients recipients = envelopesApi.ListRecipients(accountId, envelopId);
                    if (recipients.Signers[0].Email == email && recipients.Signers[0].Status != "completed")
                    {
                        string returnUrl = "http://localhost:15000/Order/ResponseData";
                        if (Url == "www.saicomputer.com" || Url == "www.ticklease.com" || Url == "ticklease.com" || Url == "www.ticklease.co.uk" || Url == "ticklease.co.uk" || Url == "equipyourschool.co.uk")
                        {
                            returnUrl = "https://www.equipyourschool.co.uk/Order/ResponseData";
                        }
                        //var clientUserId = "00" + "" + documentid;
                        var clientUserId = muid.ToString() + "" + documentid;
                        RecipientViewRequest recipientView = new RecipientViewRequest()
                        {
                            ReturnUrl = returnUrl,
                            ClientUserId = clientUserId,
                            AuthenticationMethod = "email",
                            UserName = name,
                            Email = email
                            //Email = "testeys21@gmail.com",
                            //Name = "Test Eys";
                    };
                        ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, envelopId, recipientView);
                        return Json(new { url = viewUrl.Url, st = "pending", accountId = accountId, envelopid = envelopId, mleaseid = mleaseno });
                    }
                    else
                    {
                        leasedatas.mstatus = "partial";
                        db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, envelopId, documentid);
                        byte[] b = docStream.ToArray();
                        string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
                        return Json(new { url = base64String, st = "done" });
                    }

                }
                else
                {
                    leasedatas.mstatus = envelope.Status;
                    db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, envelopId, documentid);
                    byte[] b = docStream.ToArray();
                    string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
                    return Json(new { url = base64String, st = "done" });
                }

            }
            return Json(new { url = "", st = "failed" });


        }
        [HttpPost]
        public ActionResult DeletePendingDocument(string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            string Url = Request.Url.Host;
            if(envelopId!=null && envelopId != "")
            {
                string accountId = loginApi(credential.UserName, credential.Password);
                EnvelopesApi envelopesApi = new EnvelopesApi();
                Envelope envelope = envelopesApi.GetEnvelope(accountId, envelopId, null);

                var mleaseno = int.Parse(documentid);
                var email = ""; var name = ""; var muid = 0;
                var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                if (leasedatas != null)
                {
                    var usub = db.usersubquotations.Where(x => x.magreementno == mleaseno).ToList();
                    if (usub.Any())
                    {
                        foreach (var ab in usub)
                        {
                            db.AgreementAddresses.RemoveRange(db.AgreementAddresses.Where(x => x.msubquoteid == ab.msubquoteid));
                            db.Entry(ab).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                        }


                    }
                    db.Entry(leasedatas).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                EnvelopeDefinition edf = new EnvelopeDefinition();
                DocuSign.eSign.Model.Document doc = new DocuSign.eSign.Model.Document();
                doc.DocumentId = documentid;
                edf.Documents = new List<DocuSign.eSign.Model.Document>();
                edf.Documents.Add(doc);
                EnvelopeDocumentsResult ed = envelopesApi.DeleteDocuments(accountId, envelopId, edf);
                return Json(new { data = ed.ToJson(), st = "done", accountId = accountId, envelopid = envelopId, mleaseid = mleaseno });
            }
            else
            {
                var mleaseno = int.Parse(documentid);
                var email = ""; var name = ""; var muid = 0;
                var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                if (leasedatas != null)
                {
                    var usub = db.usersubquotations.Where(x => x.magreementno == mleaseno).ToList();
                    if (usub.Any())
                    {
                        foreach (var ab in usub)
                        {
                            db.AgreementAddresses.RemoveRange(db.AgreementAddresses.Where(x => x.msubquoteid == ab.msubquoteid));
                            db.Entry(ab).State = System.Data.Entity.EntityState.Deleted;
                            db.SaveChanges();
                        }


                    }
                    db.Entry(leasedatas).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return Json(new { data = "", st = "done", accountId = "", envelopid = envelopId, mleaseid = mleaseno });
            }

            

        }
        [HttpPost]
        public ActionResult getDocusignpdf(string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            string Url = Request.Url.Host;
            string accountId = loginApi(credential.UserName, credential.Password);
            EnvelopesApi envelopesApi = new EnvelopesApi();
            String filePath = String.Empty;
            FileStream fs = null;
            // GetDocument() API call returns a MemoryStream
            Envelope envelope = envelopesApi.GetEnvelope(accountId, envelopId, null);
            var mleaseno = int.Parse(documentid);
            var email = ""; var name = ""; var muid = 0;
            var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
            if (leasedatas != null)
            {
                var udata = db.user_registration.FirstOrDefault(x => x.musermoreid == leasedatas.muserid);
                email = udata.museridemail;
                name = udata.mname;
                muid = udata.muid;
                if (envelope.Status != "completed")
                {
                    Recipients recipients = envelopesApi.ListRecipients(accountId, envelopId);
                    if (recipients.Signers[0].Email == email && recipients.Signers[0].Status != "completed")
                    {
                        leasedatas.mstatus = envelope.Status;
                    }
                    else
                    {
                        leasedatas.mstatus = "partial";
                    }
                    db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            MemoryStream docStream = (MemoryStream)envelopesApi.GetDocument(accountId, envelopId, documentid);
            // let's save the document to local file system
            byte[] b = docStream.ToArray();
            string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
            return Json(new { url = base64String, st = "done" });

        }
        [HttpPost]
        public JsonResult LoadSingleOrders(List<int> mleaseids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");
            var content = "";
            var email = "";
            if (mleaseids.Any())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var a in mleaseids)
                    {
                        var products = (from l in db.userleaseagreements
                                        join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                        where l.mleaseno == a
                                        select new exportproductviewmodel
                                        {
                                            msorderref = l.msorderref,
                                            mleaseno = l.mleaseno,
                                            subquoteids = o.msubquoteid,
                                            ffee = l.ffee,
                                            sfee = l.sfee,
                                            muserid = l.muserid
                                        }).ToList();
                        var id = products.FirstOrDefault().muserid;

                        email = db.user_registration.FirstOrDefault(x => x.musermoreid == id).museridemail;
                        string viewContent = ConvertViewToString("LoadOrders", products);
                        content = content + viewContent;
                        /*TimeSpan span = DateTime.UtcNow.AddMinutes(10) - DateTime.UtcNow;
                        var jobid = (id.HasValue ? id.Value.ToString() : "") + a.ToString();
                        var manager = new RecurringJobManager();
                        manager.AddOrUpdate(jobid, Hangfire.Common.Job.FromExpression(() => ReminderEmail(viewContent, email,a,jobid,products.Count,products.FirstOrDefault().msorderref)), Cron.Daily());
                        var sendmail = SendThankYouemail(viewContent, email,a,products.Count, products.FirstOrDefault().msorderref);
                        */
                    }

                }


                return Json(content);
            }
            return Json("empty");
            //return File(ms.ToArray(), "application/zip", "Attachments.zip");
        }
        [HttpPost]
        public JsonResult LoadOrdersWithPdf(List<int> mleaseids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");

            if (mleaseids.Any())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var a in mleaseids)
                    {
                        var products = (from l in db.userleaseagreements
                                        join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                        where l.mleaseno == a
                                        select new exportproductviewmodel
                                        {
                                            mleaseno = l.mleaseno,
                                            subquoteids = o.msubquoteid,
                                            ffee = l.ffee,
                                            sfee = l.sfee,
                                            muserid = l.muserid
                                        }).ToList();
                        string viewContent = ConvertViewToString("LoadOrders", products);
                        var stream = ExportPdfs(viewContent);
                        byte[] bytes = stream;
                        var zipEntry = archive.CreateEntry("Agreement-Order-" + a + ".pdf",
                            CompressionLevel.Fastest);
                        using (var zipStream = zipEntry.Open())
                        {
                            zipStream.Write(bytes, 0, bytes.Length);
                        }

                    }
                }
                if (System.IO.File.Exists(fullPath))
                {
                    file = new FileStream(fullPath, FileMode.Open, FileAccess.Write);
                }
                else
                {
                    file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
                }
                //ms.Seek(0, SeekOrigin.Begin);
                ms.WriteTo(file);
                file.Close();

                return Json(new { fileName = "Agreements.zip", errorMessage = "" });
            }
            return Json(new { fileName = "", errorMessage = "" });
            //return File(ms.ToArray(), "application/zip", "Attachments.zip");
        }
        [HttpGet]
        [DeleteFileAttribute] //Action Filter, it will auto delete the file after download, 
                              //I will explain it later
        public ActionResult Download(string file)
        {
            //get the temp folder and file path in server
            string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), file);

            //return the file for download, this is an Excel 
            //so I set the file content type to "application/vnd.ms-excel"
            return File(fullPath, "application/zip", file);
        }
        public class DeleteFileAttribute : ActionFilterAttribute
        {
            public override void OnResultExecuted(ResultExecutedContext filterContext)
            {
                filterContext.HttpContext.Response.Flush();

                //convert the current filter context to file and get the file path
                string filePath = (filterContext.Result as FilePathResult).FileName;

                //delete the file after download
                System.IO.File.Delete(filePath);
            }
        }

        public string ConvertViewToString(string viewName, List<exportproductviewmodel> products)
        {

            List<int> sp = new List<int>();
            if (products.Any())
            {
                foreach (var ab in products)
                {
                    sp.Add(ab.subquoteids);
                }
                ViewBag.mcontactid = products.First().mcontactid;
                ViewBag.mdefaultadd= products.First().mdefaultadd;
                ViewBag.mpayment = products.First().mpayment;
                ViewBag.mleaseno = products.First().mleaseno;
                ViewBag.subquoteid = sp;
                ViewBag.muserid = products.First().muserid;
                ViewBag.ffee = products.First().ffee;
                ViewBag.msorderref = products.First().msorderref;
                ViewBag.sfee = products.First().sfee;
                ViewBag.mcontactid = products.First().mcontactid;
                ViewBag.msorderdate = products.First().msorderdate.HasValue ? products.First().msorderdate.Value.ToString("dd/MM/yyyy") : "";

            }
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindView(ControllerContext, viewName, null);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                return writer.ToString();
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string html)
        {
            HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(html);
            html = doc.DocumentNode.OuterHtml;

            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                Encoding unicode = Encoding.UTF8;
                StringReader sr = new StringReader(html);
                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();


                //Response.AddHeader("content-disposition", "inline;filename=OrderStatus.pdf");
                return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            }

            /*HtmlDocument doc = new HtmlDocument();
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
            }*/
        }
        public static byte[] ExportPdfs(string html)
        {

            HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["meta"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(html);
            html = doc.DocumentNode.OuterHtml;
            //html=Regex.Replace(html, @"(<script[^*]*</script>)", "", RegexOptions.IgnoreCase);
            //var p = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/Content/test.css"));
            //var css = System.IO.File.ReadAllText(p);
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                Encoding unicode = Encoding.UTF8;

                StringReader sr = new StringReader(html);

                iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return stream.ToArray();
                //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            }

            /*HtmlDocument doc = new HtmlDocument();
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
            }*/
        }
        public static byte[] nExportPdfs(string html)
        {
            HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(html);
            html = doc.DocumentNode.OuterHtml;
            string htmlString = html;
            var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
            tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
            //tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor()); // use our new processor

            var output = new MemoryStream();
            // css files code resolves the css while generating pdf
            List<string> cssFiles = new List<string>();
            cssFiles.Add(@"/Content/text.css");
            //cssFiles.Add(@"/Content/Site.css");
            StringReader sr = new StringReader(html);
            var input = new MemoryStream(Encoding.UTF8.GetBytes(string.Format(html)));
            var document = new iTextSharp.text.Document();
            var writer = PdfWriter.GetInstance(document, output);
            writer.CloseStream = false;
            document.Open();
            var htmlContext = new HtmlPipelineContext(null);

            // htmlContext.SetTagFactory(iTextSharp.tool.xml.html.Tags.GetHtmlTagProcessorFactory());

            htmlContext.SetTagFactory(tagProcessors);
            //map the css files to apply the css styles in the pdf.
            ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
            cssFiles.ForEach(i => cssResolver.AddCssFile(System.Web.HttpContext.Current.Server.MapPath(i), true));

            var pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext, new PdfWriterPipeline(document, writer)));
            var worker = new XMLWorker(pipeline, true);
            var p = new XMLParser(worker);
            p.Parse(input);
            document.Close();
            output.Position = 0;
            return output.ToArray();

        }
        public static byte[] mExportPdfs(string html)
        {

            HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(html);
            html = doc.DocumentNode.OuterHtml;
            var p = System.IO.Path.Combine(System.Web.HttpContext.Current.Server.MapPath("/Content/test.css"));
            var css = System.IO.File.ReadAllText(p);
            using (var cssmemory = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(css)))
            {
                using (MemoryStream stream = new System.IO.MemoryStream())
                {
                    Encoding unicode = Encoding.UTF8;
                    StringReader sr = new StringReader(html);
                    StringReader sr2 = new StringReader(css);
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                    pdfDoc.Close();
                    return stream.ToArray();
                    //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
                }

            }

            /*HtmlDocument doc = new HtmlDocument();
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "Grid.pdf");
            }*/
        }
        public bool SendThankYouemail(string data, string emailto, int a, int noofitem, string pono, string requestedurl)
        {
            // var pdfbinary= Convert.FromBase64String(data);
            var bytes = ExportPdfs(data);
                var body = "Thank you for ordering with equipyourschool.co.uk<br/><br/>";
            body += "Your Order Ref:" + a + "<br/>";
            body += "Your PO No: " + pono + "<br/>";
            body += "Number of items: " + noofitem + "<br/><br/>";
            body += "We have attached a copy of your Agreement generated for your order number " + a + "<br/><br/>";
            body += "If you have not already printed your Agreement from the portal, please print out the attached document and arrange to sign it at the places indicated with an “X”<br/><br/>";
            body += "Please post the signed original Agreement to the address shown..<br/><br/>";
            body += "It you have any queries, please feel free to call us on xxxxxxxxxx.<br/><br/>";
            if (MailDao.MailSend("Thank you for ordering with us", body, emailto, bytes)=="done")
            {
                return true;
            }
            else
            {
                return false;
            }

            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");



        }
        public bool SendThankYouemailBinaryFile(byte[] bytes, string emailto, int a, int noofitem, string pono, string requestedurl)
        {
            
            var body = "Thank you for ordering with equipyourschool.co.uk<br/><br/>";
            body += "Your Order Ref:" + a + "<br/>";
            body += "Your PO No: " + pono + "<br/>";
            body += "Number of items: " + noofitem + "<br/><br/>";
            body += "We have attached a copy of your agreement generated for your order number " + a + "<br/><br/>";
            body += "If you have not already printed your Agreement from the portal, please print out the attached document and arrange to sign it at the places indicated with an “X”<br/><br/>";
            body += "Please post the signed original Agreement to the address shown..<br/><br/>";
            body += "It you have any queries, please feel free to call us on xxxxxxxxxx.<br/><br/>";
            if (MailDao.MailSend("Thank you for ordering with us", body, emailto, bytes) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            


        }
        public ActionResult ViewPurchaseOrder(string msorderref, int mschoolid)
        {
            var db = new LeasingDbEntities();
            var content = "";
            var products = (from l in db.userleaseagreements
                            join o in db.usersubquotations on l.mleaseno equals o.magreementno
                            where l.msorderref == msorderref && l.muserid == mschoolid
                            select new exportproductviewmodel
                            {
                                msorderdate = l.createddate,
                                msorderref = l.msorderref,
                                mleaseno = l.mleaseno,
                                subquoteids = o.msubquoteid,
                                ffee = l.ffee,
                                sfee = l.sfee,
                                muserid = l.muserid,
                                mcontactid=l.mcontactid.HasValue?l.mcontactid.Value:0,
                            }).ToList();
            var id = products.FirstOrDefault().muserid;

            string viewContent = ConvertViewToString("LoadPurchaseOrder", products);
            content = content + viewContent;


            byte[] b = ExportPdfs(content);
            string base64String = Convert.ToBase64String(b, 0, b.Length); //"data:application/pdf;base64," + Convert.ToBase64String(b, 0, b.Length);
            return Json(base64String);

        }
        public ActionResult LoadPurchaseOrder(string msorderref, int mschoolid)
        {
            var db = new LeasingDbEntities();
            var products = (from l in db.userleaseagreements
                            join o in db.usersubquotations on l.mleaseno equals o.magreementno
                            where l.msorderref == msorderref && l.muserid == mschoolid
                            select new exportproductviewmodel
                            {
                                msorderdate = l.createddate,
                                msorderref = l.msorderref,
                                mleaseno = l.mleaseno,
                                subquoteids = o.msubquoteid,
                                ffee = l.ffee,
                                sfee = l.sfee,
                                muserid = l.muserid
                            }).ToList();
            var id = products.FirstOrDefault().muserid;
            List<int> sp = new List<int>();
            if (products.Any())
            {
                foreach (var ab in products)
                {
                    sp.Add(ab.subquoteids);
                }
                ViewBag.mleaseno = products.First().mleaseno;
                ViewBag.subquoteid = sp;
                ViewBag.muserid = products.First().muserid;
                ViewBag.ffee = products.First().ffee;
                ViewBag.msorderref = products.First().msorderref;
                ViewBag.sfee = products.First().sfee;
                ViewBag.msorderdate = products.First().msorderdate.HasValue ? products.First().msorderdate.Value.ToString("dd/MM/yyyy") : "";
            }
            return View();
        }
        public static string ReminderSignature(string data, string emailto, int a, string jobid, int noofitem, string pono, Nullable<DateTime> msorderdate, string requestedurl)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }
            var searchsenddata = db.userleaseagreements.Where(x => x.muserid == a && x.mstatus != "completed" && x.mstatus != "partial").ToList();
            if (searchsenddata.Any())
            {
                byte[] b= { };

                UrlHelper url = new UrlHelper();
                var id = url.Encode(Encryption.Encrypt(a.ToString()));

                var body = "Respected School,<br/>";
                    body+= "This is pending signature reminder email from EYS<br/><br/>";
                body += "<table width='650' cellspacing='0' cellpadding='0' border='0' style='border:3px solid #dcdcdc; background-color:#f1f1f1; font-family:Verdana,Arial,Helvetica,sans-serif; font-size:12px;'>";
                body += "<tr><td><table width='650' border='0' cellpadding='0' cellspacing='0' style='font-family:Verdana,Arial,Helvetica,sans-serif; font-size:12px; background-color:#f1f1f1'>";
                body += "<tr><td style='PADDING-LEFT:20px;PADDING-BOTTOM:10px;PADDING-RIGHT:20px;PADDING-TOP:10px;'><p align='left'>";
                body += "<table width='100%' align='center' cellspacing='5' cellpadding='5' border='0' style='background:#ffffff; border-style:solid; line-height:20px;font-family:Verdana,Arial,Helvetica,sans-serif; font-size:12px;'>";
                body += "<tr><td colspan='4' align='center' style='background:#02aff3;'><b style='color:#FFFFFF'>Your Pending Agreements awaiting to be signed</b></td>";
                body += "<tr><td style='background:#c7c7c7;color:#000;'>Order Number</td><td style='background:#c7c7c7;color:#000;'>Date Agreement Generated</td><td style='background:#c7c7c7;color:#000;'>Generated By</td><td style='background:#c7c7c7;color:#000;'>Number of items</td></tr>";

                foreach (var searchsend in searchsenddata)
                {
                    var userdata = db.user_registration.FirstOrDefault(x => x.muid == searchsend.createdby);
                    var uname = "";
                    if (userdata != null)
                    {
                        uname = userdata.mname;
                    }
                    var usersubquote = db.usersubquotations.Where(x => x.magreementno == searchsend.mleaseno).ToList();
                    body += "<tr><td style='width:90px;'>"+searchsend.mleaseno+"</td><td>"+(searchsend.createddate.HasValue?searchsend.createddate.Value.ToString("dd/MM/yyyy"):"")+"</td><td>"+uname+"</td><td>"+usersubquote.Count+"</td></tr>";
                }
                body += "</table></td></tr></table></td></tr></table>";

                //body += "We have attached a copy of your agreement generated for your order number " + a + "<br/><br/>";

                body += "Please click <a style='text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/MyAccount/Index?tab=pendingsignatureleases&content=12' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;color:#017fbe;padding:0px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:0px'> here </span></a> to check your pending agreement awaiting to be signed";
                /*body += "<div style = 'margin:0 auto;text-align:left;' ><p> Please confirm that if you have sent attached agreement duly signed by post to lessor.</p><br/><a style = 'text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/Leasing/UpdateAgSend?ag=" + id + "&status=yes' target = '_blank' data-saferedirecturl='#'><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> Yes </span></a>" +
                                 "<a style='text-decoration:none;text-align:left;' href = 'http://"+requestedurl+"/Leasing/UpdateAgSend?ag=" +  id + "&status=no' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> No </span></a>" +
                                "</div>" +
                                "</div>";*/
                body += "<br/><br/><p>*Please ignore if you already signed it.</p>";
                if (MailDao.MailSend("You have " + searchsenddata.Count + " pending Agreements awaiting to be signed", body, emailto, b) == "done")
                {
                    return "done";
                }
                else
                {
                    return "";
                }
                

            }
            else
            {
                RecurringJob.RemoveIfExists(jobid);
            }
            // var pdfbinary= Convert.FromBase64String(data);
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            return "done";



        }
        public static string ReminderLessorOrderProcess(string data, string emailto, int a, string jobid, int noofitem, string pono, Nullable<DateTime> msorderdate, string requestedurl)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }
            var searchsenddata = (from us in db.usersubquotations 
                                  join ag in db.userleaseagreements on us.magreementno equals ag.mleaseno
                                  where ag.mleaseno==a && us.order_visible!=1
                                  select new
                                  {
                                      ag.mleaseno,
                                      ag.createddate,
                                      ag.muserid
                                  }).FirstOrDefault();
            if (searchsenddata!=null)
            {
                var todaydata = DateTime.Today;
                var twodaylater = searchsenddata.createddate.HasValue ? searchsenddata.createddate.Value.AddDays(2):DateTime.Today.AddDays(2);
                if (todaydata >= twodaylater)
                {
                    byte[] b = { };
                    UrlHelper url = new UrlHelper();
                    var id = url.Encode(Encryption.Encrypt(a.ToString()));
                    var getschooldata = db.user_registration_more.FirstOrDefault(x => x.musermoreid == searchsenddata.muserid);
                    var body = "This is reminder email from equipyourschool.co.uk<br/><br/>";
                    body += "Order Number: " + a + "<br/>";
                    body += "School Name: " + getschooldata.morgname + "<br/>";
                    body += "Order Date: " + (searchsenddata.createddate.HasValue ? searchsenddata.createddate.Value.ToString("dd/MM/yyyy"):"") + "<br/>";
                    body += "<br/><br/>This order not yet been passed to supplier for processing";
                    body += "Please click <a style='text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/LeaseAdmin/Customer/CustomerOrder' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;color:#017fbe;padding:0px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:0px'> here </span></a> to view your order list in order to action to reminder";
                    body += "<br/><br/><p>*Please ignore if you already processed.</p>";

                    if (MailDao.MailSend("EYS Reminder - Order No: " + a + " has not been processed for 2 days or more", body, emailto, b) == "done")
                    {
                        return "done";
                    }
                    else
                    {
                        return "";
                    }
                    
                }


            }
            else
            {
                RecurringJob.RemoveIfExists(jobid);
            }
            // var pdfbinary= Convert.FromBase64String(data);
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            return "done";



        }
        public static string ReminderEmail(string data, string emailto, int a, string jobid, int noofitem, string pono, Nullable<DateTime> msorderdate, string requestedurl)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }
            var searchsend = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == a);
            if (searchsend != null)
            {
                if (searchsend.mstatus != "completed" && searchsend.mstatus != "partial")
                {


                    var bytes = ExportPdfs(data);

                    var salutation = "Openstar School Leasing";
                    UrlHelper url = new UrlHelper();
                    var id = url.Encode(Encryption.Encrypt(a.ToString()));
                    var body = "This is reminder email from equipyourschool.co.uk<br/><br/>";
                    body += "Your Order Ref: " + a + "<br/>";
                    body += "Your PO No: " + pono + "<br/>";
                    body += "Number of items: " + noofitem + "<br/>";
                    if (msorderdate.HasValue)
                    {
                        body += "Agreement generated on: " + msorderdate.Value.ToString("dd/MM/yyyy") + "<br/>";

                    }
                    body += "We have attached a copy of your agreement generated for your order number " + a + "<br/><br/>";
                    body += "Please click <a style='text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/MyAccount/Index?tab=yourleaseorder&content=12' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> here </span></a> to check your pending agreement awaiting to be signed";
                    /*body += "<div style = 'margin:0 auto;text-align:left;' ><p> Please confirm that if you have sent attached agreement duly signed by post to lessor.</p><br/><a style = 'text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/Leasing/UpdateAgSend?ag=" + id + "&status=yes' target = '_blank' data-saferedirecturl='#'><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> Yes </span></a>" +
                                     "<a style='text-decoration:none;text-align:left;' href = 'http://"+requestedurl+"/Leasing/UpdateAgSend?ag=" +  id + "&status=no' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> No </span></a>" +
                                    "</div>" +
                                    "</div>";*/
                    body += "<br/><br/><p>*Please ignore if you already signed it.</p>";
                    byte[] b = { };
                    if (MailDao.MailSend("You have  pending agreements awaiting to be signed", body, emailto, b) == "done")
                    {
                        return "done";
                    }
                    else
                    {
                        return "";
                    }
                    
                }
                else
                {
                    RecurringJob.RemoveIfExists(jobid);
                }
            }
            else
            {
                RecurringJob.RemoveIfExists(jobid);
            }
            // var pdfbinary= Convert.FromBase64String(data);
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            return "done";



        }

    }

}
