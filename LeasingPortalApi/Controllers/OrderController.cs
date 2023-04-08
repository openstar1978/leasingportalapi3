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
using System.Web.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using LeasingPortalApi.Models;
using Newtonsoft.Json;

namespace LeasingPortalApi.Controllers
{
    public class OrderController : Controller
    {
        private string apiurl = WebConfigurationManager.AppSettings["esigndoc"];

        private string apikey = "8663CoES!syS";
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
            //var content = "";
            byte[] content = { };
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
                                            mcontactid = l.mcontactid,
                                            mdefaultadd = l.mdefaultadd.HasValue ? l.mdefaultadd.Value : 0,
                                            mpayment = l.mpayment.HasValue ? l.mpayment.Value : 1,
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
                        string viewContent = "";
                        var envbyte = CreateAgreement(products);
                        //string viewContent = ConvertViewToString("LoadOrders", products);
                        //content = content + viewContent;
                        content = envbyte;
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

                byte[] b = content; //ExportPdfs(content);
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
                    ///---------------------New Field--------------
                    doc.TransformPdfFields = "true";
                    //----------------------------------
                    envDef.Documents = new List<DocuSign.eSign.Model.Document>();
                    envDef.Documents.Add(doc);
                    // Add a recipient to sign the documeent  
                    DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
                    signer.Email = email;
                    signer.Name = name;
                    signer.RecipientId = muid.ToString();
                    signer.ClientUserId = muid.ToString() + "" + mleaseid.ToString();
                    signer.RoutingOrder = "1";
                    /*DocuSign.eSign.Model.Signer signer2 = new DocuSign.eSign.Model.Signer();
                    signer2.Email = "testeys21@gmail.com";
                    signer2.Name = "Test Eys";
                    signer2.RecipientId = Guid.NewGuid().ToString();
                    signer2.ClientUserId = "00" + "" + mleaseid.ToString();
                    signer2.RoutingOrder = "2";
                    */
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
                        PageNumber = "1",
                        XPosition = "68",
                        YPosition = "558",
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
                    SignHere signHere4 = new SignHere
                    {
                        AnchorString = "SignHere",
                        AnchorYOffset = "0",
                        AnchorUnits = "pixels",
                        AnchorXOffset = "25",
                        Width = "80",
                        Height = "10",


                    };
                    signer.Tabs.SignHereTabs.Add(signHere);
                    signer.Tabs.SignHereTabs.Add(signHere2);
                    signer.Tabs.SignHereTabs.Add(signHere3);
                    signer.Tabs.SignHereTabs.Add(signHere4);
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
                        TabLabel = "TR No"
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

                    /*signer2.Tabs = new Tabs();
                    signer2.Tabs.SignHereTabs = new List<SignHere>();
                    signer2.Tabs.SignHereTabs.Add(RsignHere);
                    signer2.Tabs.TextTabs = new List<Text>();
                    signer2.Tabs.TextTabs.Add(item);
                    signer2.Tabs.DateTabs = new List<Date>();
                    signer2.Tabs.DateTabs.Add(rdate);
                    */
                    envDef.Recipients = new DocuSign.eSign.Model.Recipients();
                    envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
                    envDef.Recipients.Signers.Add(signer);
                    //envDef.Recipients.Signers.Add(signer2);
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
        [HttpPost]
        public JsonResult LoadOrdersNew(List<int> mleaseids, List<int> remainids)
        {
            var db = new LeasingDbEntities();
            var ms = new MemoryStream();
            FileStream file;
            //string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), "Agreements.zip");
            byte[] content = { };
            var email = "";
            var name = "";
            var mleaseid = 0;
            var muid = 0;
            if (mleaseids.Any())
            {
                //using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                //{
                foreach (var a in mleaseids)
                {
                    var products = (from l in db.userleaseagreements
                                    join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                    where l.mleaseno == a
                                    select new exportproductviewmodel
                                    {
                                        mcontactid = l.mcontactid,
                                        mdefaultadd = l.mdefaultadd.HasValue ? l.mdefaultadd.Value : 0,
                                        mpayment = l.mpayment.HasValue ? l.mpayment.Value : 1,
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
                    var envbyte = CreateAgreement(products);
                    string viewContent = "";
                    //string viewContent = ConvertViewToString("LoadOrders", products);
                    //content.Concat(envbyte);
                    content = envbyte;
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

                //}
                var embededidstring = "";
                if (remainids != null && remainids.Count > 0)
                {
                    SessionWrapper.agnos = remainids;
                    /*foreach(var ab in remainids)
                    {
                        embededidstring = (embededidstring != "" ? embededidstring + "," + Encryption.Encrypt(ab.ToString()) : Encryption.Encrypt(ab.ToString()));
                    }*/
                }
                else
                {
                    var emptyno = new List<int>();
                    SessionWrapper.agnos = emptyno;

                }
                var eno = SessionWrapper.agnos;
                byte[] b = content;// ExportPdfs(content);
                try
                {
                    using (var client = new HttpClient())
                    {
                        //client.BaseAddress = new Uri(apiurl+"v3/uploads");
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        var up = new EeUploadViewModel()
                        {
                            title = "EYS Agreement - " + mleaseid.ToString(),
                            base64 = System.Convert.ToBase64String(b),
                            extension = "pdf",
                            type = "document",
                            contains_tags = false,
                        };
                        var uplist = new List<EeUploadViewModel>();
                        uplist.Add(up);
                        var uploads = new EUploadViewModel()
                        {
                            uploads = uplist
                        };
                        var json = JsonConvert.SerializeObject(uploads);
                        var data = new StringContent(json, Encoding.UTF8, "application/json");
                        //System.Diagnostics.Debug.WriteLine(data);
                        HttpResponseMessage response = client.PostAsync(apiurl + "v3/uploads", data).Result;
                        var get_response = response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode)
                        {
                            var uploadresult = JsonConvert.DeserializeObject<EResponseUploadViewModel>(get_response.Result);
                            var docid = "";
                            if (uploadresult != null)
                            {
                                if (uploadresult.uploads.Count > 0 && uploadresult.uploads[0].id != null)
                                {
                                    docid = uploadresult.uploads[0].id;
                                    var envelope_opt = new envelopeoptionsViewMoel
                                    {
                                        dont_send_signing_emails = true,
                                        sign_in_sequential_order = true,
                                        days_envelope_expires = "10",
                                        validate_signer_mailboxes = false
                                    };
                                    var signers = new List<signersViewModel>();
                                    var signer1 = new signersViewModel
                                    {
                                        name = name,
                                        email = email,

                                    };
                                    signers.Add(signer1);
                                    //var signer2 = new signersViewModel
                                    //{
                                    //    name = "Eys Developer",
                                    //    //email = "eysdeveloper21@gmail.com",
                                    //    email = "shahpalakh@yahoo.co.in"

                                    //};
                                    //signers.Add(signer2);
                                    var dfields = new List<documentfieldsViewModel>();
                                    var documentfileds = new documentfieldsViewModel
                                    {
                                        signer_email = email,
                                        signer_idx = muid.ToString(),
                                        field_type = "signature",
                                        field_required = true,
                                        document_position = new DocumentPositionViewModel
                                        {
                                            y = "45.8%",
                                            x = "9.1%",
                                            width = "10.5%",
                                            height = "10.5%",
                                            page = 1,
                                        }
                                    };
                                    dfields.Add(documentfileds);
                                    var documentfileds2 = new documentfieldsViewModel
                                    {
                                        signer_email = email,
                                        signer_idx = muid.ToString(),
                                        field_type = "date",
                                        field_required = true,
                                        document_position = new DocumentPositionViewModel
                                        {
                                            y = "51.5%",
                                            x = "44%",
                                            width = "10.5%",
                                            height = "10.5%",
                                            page = 1,
                                        }
                                    };
                                    dfields.Add(documentfileds2);
                                    var documentfileds3 = new documentfieldsViewModel
                                    {
                                        signer_email = email,
                                        signer_idx = muid.ToString(),
                                        field_type = "date",
                                        field_required = true,
                                        document_position = new DocumentPositionViewModel
                                        {
                                            y = "98.5%",
                                            x = "55.1%",
                                            width = "10.5%",
                                            height = "10.5%",
                                            page = 1,
                                        }
                                    };
                                    dfields.Add(documentfileds3);
                                    var documentfileds4 = new documentfieldsViewModel
                                    {
                                        signer_email = email,
                                        signer_idx = muid.ToString(),
                                        field_type = "signature",
                                        field_required = true,
                                        document_position = new DocumentPositionViewModel
                                        {
                                            y = "68.5%",
                                            x = "11.9%",
                                            width = "10.5%",
                                            height = "10.5%",
                                            page = 2,
                                        }
                                    };
                                    dfields.Add(documentfileds4);
                                    var documentfileds5 = new documentfieldsViewModel
                                    {
                                        signer_email = email,
                                        signer_idx = muid.ToString(),
                                        field_type = "date",
                                        field_required = true,
                                        document_position = new DocumentPositionViewModel
                                        {
                                            y = "74%",
                                            x = "44.8%",
                                            width = "10.5%",
                                            height = "10.5%",
                                            page = 2,
                                        }
                                    };
                                    dfields.Add(documentfileds5);

                                    var ddocs = new List<DocumentViewModel>();
                                    var docum = new DocumentViewModel
                                    {
                                        title = "EYS Agreement - " + mleaseid,
                                        upload_file = new UploadFileViewModel
                                        {
                                            id = docid,
                                        },
                                        document_fields = dfields,
                                    };
                                    ddocs.Add(docum);
                                    var redirect_uri = "https://equipyourschool.co.uk/Order/ConfirmOrderWithAgreement?agid=" + Url.Encode(Encryption.Encrypt(mleaseids[0].ToString())) + "&r=" + (embededidstring == "" ? Url.Encode(Encryption.Encrypt("empty")) : embededidstring);
                                    var enve = new EnvelopeViewModel
                                    {
                                        title = "Agreements Pack",
                                        subject = "You have agreement to sign",
                                        signer_redirect_uri = redirect_uri,
                                        documents = ddocs,
                                        signers = signers,
                                        envelope_options = envelope_opt,

                                    };
                                    var finalenvelope = new ESignViewModel
                                    {
                                        envelope = enve,
                                    };
                                    json = JsonConvert.SerializeObject(finalenvelope);
                                    data = new StringContent(json, Encoding.UTF8, "application/json");
                                    //System.Diagnostics.Debug.WriteLine(data);
                                    response = client.PostAsync(apiurl + "v3/envelopes", data).Result;
                                    get_response = response.Content.ReadAsStringAsync();
                                    try
                                    {
                                        if (response.IsSuccessStatusCode)
                                        {
                                            var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                                            var url = apiurl + "link?e=" + newuploadresult.envelope.id + "&s=" + newuploadresult.envelope.signers[0].id;
                                            var leasedata = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseid);
                                            if (leasedata != null)
                                            {
                                                leasedata.menvelopeid = newuploadresult.envelope.id;
                                                db.Entry(leasedata).State = System.Data.Entity.EntityState.Modified;
                                                db.SaveChanges();
                                            }
                                            return Json(new { url = url, accountId = "", envelopid = newuploadresult.envelope.id, mleaseid = mleaseid });
                                        }
                                        else
                                        {
                                            var newuploadresult = JsonConvert.DeserializeObject<List<ErrorViewModel>>(get_response.Result);
                                            return Json(new { url = "", accountId = "", envelopid = "", mleaseid = mleaseid, message = newuploadresult[0].details });
                                        }

                                    }
                                    catch (Exception e)
                                    {
                                        return Json(new { url = "", accountId = "", envelopid = "", mleaseid = mleaseid });
                                    }

                                }
                            }
                        }
                        else
                        {

                            var newuploadresult = JsonConvert.DeserializeObject<List<ErrorViewModel>>(get_response.Result);
                            return Json(new { url = "", accountId = "", envelopid = "", mleaseid = mleaseid, message = newuploadresult[0].details });

                        }


                    }

                    /*ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
                    Configuration.Default.ApiClient = apiClient;
                    string Url = Request.Url.Host;
                    
                    string accountId = loginApi(credential.UserName, credential.Password);
                    byte[] fileBytes = b;
                    EnvelopeDefinition envDef = new EnvelopeDefinition();
                    envDef.EmailSubject = "Please sign this EYS Agreement doc";
                    DocuSign.eSign.Model.Document doc = new DocuSign.eSign.Model.Document();
                    doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
                    doc.Name = "EYS Agreement";
                    doc.DocumentId = mleaseid.ToString();
                    envDef.Documents = new List<DocuSign.eSign.Model.Document>();
                    envDef.Documents.Add(doc);
                    DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
                    signer.Email = email;
                    signer.Name = name;
                    signer.RecipientId = muid.ToString();
                    signer.ClientUserId = muid.ToString() + "" + mleaseid.ToString();
                    signer.RoutingOrder = "1";
                    DocuSign.eSign.Model.Signer signer23 = new DocuSign.eSign.Model.Signer();
                    signer23.Email = "testeys21@gmail.com";
                    signer23.Name = "Test Eys";
                    signer23.RecipientId = Guid.NewGuid().ToString();
                    signer23.ClientUserId = "00" + "" + mleaseid.ToString();
                    signer23.RoutingOrder = "2";
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
                    
                    signer23.Tabs = new Tabs();
                    signer23.Tabs.SignHereTabs = new List<SignHere>();
                    signer23.Tabs.SignHereTabs.Add(RsignHere);
                    signer23.Tabs.TextTabs = new List<Text>();
                    signer23.Tabs.TextTabs.Add(item);
                    signer23.Tabs.DateTabs = new List<Date>();
                    signer23.Tabs.DateTabs.Add(rdate);

                    envDef.Recipients = new DocuSign.eSign.Model.Recipients();
                    envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
                    envDef.Recipients.Signers.Add(signer);
                    envDef.Recipients.Signers.Add(signer23);
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

                    ViewBag.url = viewUrl.Url;*/
                    //return Json(new { url = viewUrl.Url, accountId = accountId, envelopid = envelopeSummary.EnvelopeId, mleaseid = mleaseid });

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
        public byte[] CreateAgreement(List<exportproductviewmodel> products)
        {
            StringBuilder sb = new StringBuilder();
            var mcontactid = products.First().mcontactid;
            var mdefaultadd = products.First().mdefaultadd;
            var mpayment = products.First().mpayment;
            var mleaseno = products.First().mleaseno;
            //ViewBag.subquoteid = sp;
            var muserid = products.First().muserid;
            var ffee = 99;
            var msorderref = products.First().msorderref;
            var sfee = 50;
            //var mcontactid = products.First().mcontactid;
            var msorderdate = products.First().msorderdate.HasValue ? products.First().msorderdate.Value.ToString("dd/MM/yyyy") : "";
            var db = new LeasingDbEntities();
            var suname = "";
            var schoolname = "";
            var saddress = "";
            var supaddress = "";
            var contacperson = "";
            var telno = "";
            var contactemail = "";
            var position = "";
            var i = 1;

            var userdata = UsersDao.GetByOrgId(muserid.Value);
            if (userdata != null)
            {
                var cf = false;
                if (mcontactid > 0)
                {
                    var getcontact = UsersDao.getusercontact(mcontactid.Value);
                    if (getcontact != null)
                    {
                        contacperson = getcontact.cusername;
                        contactemail = getcontact.cemailaddress;
                        position = getcontact.cposition;
                        telno = getcontact.cphone;
                        cf = true;
                    }

                }
                if (!cf)
                {
                    contacperson = userdata.mname;
                    contactemail = userdata.museridemail;
                    position = userdata.mposition;
                    telno = userdata.mphone;

                }

                schoolname = userdata.morgname;
                var ca = false;
                if (mdefaultadd > 0)
                {
                    var getcontact = UsersDao.getuseraddress(mdefaultadd);
                    if (getcontact != null)
                    {
                        saddress = getcontact.maddress1;
                        if (getcontact.maddress2 != null)
                        {
                            saddress += ", " + getcontact.maddress1;
                        }
                        saddress += ", " + (getcontact.mcity != null ? " " + getcontact.mcity : "") + (getcontact.mpostcode != null ? " - " + getcontact.mpostcode : "");
                        ca = true;
                    }
                }
                if (!ca)
                {
                    saddress = userdata.maddress1;
                    if (userdata.maddress2 != null)
                    {
                        saddress += ", " + userdata.maddress1;
                    }
                    saddress += ", " + (userdata.mcity != null ? " " + userdata.mcity : "") + (userdata.mpostcode != null ? " - " + userdata.mpostcode : "");

                }

                saddress += (userdata.mphone != null ? ", Phone : " + userdata.mphone : "");

            }
            var bankdetail = new LeaseBankViewModel();
            if (mpayment == 2)
            {
                bankdetail = QuotationDAO.GetBanks(mleaseno.Value, db);

            }
            //string newFile = @"c:\Temp\PDF\completed_fw42.pdf";
            using (MemoryStream ms = new MemoryStream())
            {

                var doc = new iTextSharp.text.Document();
                //var pCopy = new PdfSmartCopy(doc, ms);
                iTextSharp.text.pdf.PdfCopy PDFwriter = new iTextSharp.text.pdf.PdfCopy(doc, ms);
                //PDFwriter.Open();
                if (PDFwriter == null)
                {
                    return ms.ToArray();
                }
                doc.Open();

                string pdfTemplate = Path.Combine(Server.MapPath("~/templates/"), "EYS Standard Agreement.pdf");
                PdfReader pdfReader = new PdfReader(pdfTemplate);
                var msTemp = new MemoryStream();
                PdfStamper stamper = new PdfStamper(pdfReader, msTemp);
                //{

                AcroFields pdfFormFields = stamper.AcroFields;
                foreach (var de in pdfReader.AcroFields.Fields)
                {
                    //sb.Append(de.Key.ToString() + Environment.NewLine);
                    pdfFormFields.SetFieldProperty(de.Key.ToString(), "textsize", (float)8, null);
                    //i++;
                }
                pdfFormFields.SetField("Hirer's Name", schoolname);
                //pdfFormFields.SetFieldProperty("Hirer's Name", "textsizes", 30f, null);
                pdfFormFields.SetField("Hirer's Address", saddress);
                //pdfFormFields.SetFieldProperty("Hirer's Address", "textsizes", 30f, null);
                List<int> sp = new List<int>();
                double? total = 0;
                foreach (var p in products)
                {
                    sp.Add(p.subquoteids);
                }
                var productdetail = QuotationDAO.LoadNewSubQuotationDetails(sp);
                if (productdetail.Count() <= 2)
                {
                    var k = 2;
                    foreach (var ab in productdetail)
                    {
                        var addr = "";
                        if (ab.DifferentAddress != null && ab.DifferentAddress != "")
                        {
                            var dadd = int.Parse(ab.DifferentAddress);
                            var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == dadd);
                            if (maddress != null)
                            {
                                addr = maddress.maddress1;
                                if (maddress.maddress2 != null)
                                {
                                    addr = addr + "\n" + maddress.maddress2;
                                }
                                if (maddress.mlandmark != null)
                                {
                                    addr = addr + "\n" + maddress.mlandmark;

                                }
                                addr = addr + "\n" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                            }

                        }

                        total = total + ((ab.mprice.HasValue ? ab.mprice.Value : 0) * ab.quantity);
                        pdfFormFields.SetField("QTY" + k, ab.quantity.Value.ToString());
                        pdfFormFields.SetField("Make" + k, ab.Make);
                        //pdfFormFields.SetField("model" + k, );
                        pdfFormFields.SetField("equipdesc" + k, ab.mproductname);
                        //pdfFormFields.SetField("equipdesc" + k, "This is for testing product name\nakdsjhsakdhaskjksafjlaskjfasjfj\nlkjsdljalsjdlj");
                        pdfFormFields.SetField("new" + k, ab.Condition);
                        pdfFormFields.SetField("location" + k, addr);
                        k++;
                    }

                }
                else
                {
                    if (productdetail.Count > 2)
                    {
                        foreach (var ab in productdetail)
                        {
                            total = total + ((ab.mprice.HasValue ? ab.mprice.Value : 0) * ab.quantity);

                        }
                        pdfFormFields.SetField("equipdesc2", "Please check page no.3 for your ordered items");
                    }
                }
                pdfFormFields.SetField("printnames", contacperson + ", " + position);
                pdfFormFields.SetField("title", "");
                pdfFormFields.SetField("surname", contacperson);
                pdfFormFields.SetField("position", position);
                pdfFormFields.SetField("telephone", telno);
                pdfFormFields.SetField("email", contactemail);
                pdfFormFields.SetField("nameschool", schoolname);
                if (mpayment == 2 && bankdetail != null)
                {
                    pdfFormFields.SetField("DD-account-holder1", bankdetail.maccountholder);
                    pdfFormFields.SetField("DD-bank", bankdetail.mbankname);
                    for (int b = 0; b < bankdetail.mbankaccount.Length; b++)
                    {
                        pdfFormFields.SetField("DD-acc-num" + (b + 1), (bankdetail.mbankaccount[b] - '0').ToString());
                    }
                    var sortdatas = bankdetail.mbankshort.Split('-');
                    var s = 1;
                    for (int b = 0; b < sortdatas.Length; b++)
                    {
                        for (int k = 0; k < sortdatas[b].Length; k++)
                        {
                            pdfFormFields.SetField("DD-sort-code" + s, (sortdatas[b][k] - '0').ToString());
                            s++;
                        }
                    }
                }
                pdfFormFields.SetField("years2", (productdetail.First().mtermp / 12).Value.ToString());
                pdfFormFields.SetField("months2", "0");
                pdfFormFields.SetField("deliverydate", QuotationDAO.getPeriodInWords((productdetail.First().mtermp / productdetail.First().mtermf) - 1));
                pdfFormFields.SetField("first2", Math.Round(total.HasValue ? total.Value : 0, 2).ToString());
                pdfFormFields.SetField("further2", (Math.Round(total.HasValue ? total.Value : 0, 2)).ToString());
                switch (productdetail.First().mtermf)
                {
                    case 1:
                        pdfFormFields.SetField("Radio Button 1", "Choice1");
                        break;
                    case 3:
                        pdfFormFields.SetField("Radio Button 1", "Choice2");
                        break;
                    case 6:
                        pdfFormFields.SetField("Radio Button 1", "Choice3");
                        break;
                    case 12:
                        pdfFormFields.SetField("Radio Button 1", "Choice4");
                        break;

                }
                if (stamper.AcroFields != null && stamper.AcroFields.GenerateAppearances != true)
                { stamper.AcroFields.GenerateAppearances = true; }
                stamper.FormFlattening = true;
                // close the pdf  
                stamper.Close();
                var pdfFile = new PdfReader(msTemp.ToArray());

                for (int j = 1; j <= pdfFile.NumberOfPages; j++)
                {
                    //((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, j));
                    //pCopy.FreeReader(pdfFile);
                    iTextSharp.text.pdf.PdfImportedPage page = PDFwriter.GetImportedPage(pdfFile, j);
                    PDFwriter.AddPage(page);
                }
                pdfFile.Close();
                if (productdetail.Count() > 2)
                {
                    var totalproduct = productdetail.Count();
                    var skip = 0;
                    var take = 15;
                    var page = 3;
                    var indexd = 1;
                    while (totalproduct > 15)
                    {
                        pdfTemplate = Path.Combine(Server.MapPath("~/templates/"), "page3withoutsign.pdf");
                        pdfReader = new PdfReader(pdfTemplate);
                        msTemp = new MemoryStream();
                        stamper = new PdfStamper(pdfReader, msTemp);
                        pdfFormFields = stamper.AcroFields;
                        foreach (var de in pdfReader.AcroFields.Fields)
                        {
                            //sb.Append(de.Key.ToString() + Environment.NewLine);
                            pdfFormFields.SetFieldProperty(de.Key.ToString(), "textsize", (float)8, null);
                            //i++;
                        }
                        indexd = 1;
                        var newproducts = productdetail.Skip(skip).Take(15);
                        foreach (var ab in newproducts)
                        {
                            var addr = "";
                            if (ab.DifferentAddress != null && ab.DifferentAddress != "")
                            {
                                var dadd = int.Parse(ab.DifferentAddress);
                                var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == dadd);
                                if (maddress != null)
                                {
                                    addr = maddress.maddress1;
                                    if (maddress.maddress2 != null)
                                    {
                                        addr = addr + "\n" + maddress.maddress2;
                                    }
                                    if (maddress.mlandmark != null)
                                    {
                                        addr = addr + "\n" + maddress.mlandmark;

                                    }
                                    addr = addr + "\n" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                                }

                            }

                            total = total + ((ab.mprice.HasValue ? ab.mprice.Value : 0) * ab.quantity);
                            pdfFormFields.SetField("Qty" + indexd, ab.quantity.Value.ToString());
                            pdfFormFields.SetField("Make" + indexd, ab.Make);
                            //pdfFormFields.SetField("model" + k, );
                            //pdfFormFields.SetField("equipdesc" + k, ab.mproductname);
                            pdfFormFields.SetField("Desc" + indexd, ab.mproductname);
                            pdfFormFields.SetField("New" + indexd, ab.Condition);
                            //pdfFormFields.SetField("location" + indexd, addr);
                            indexd++;
                        }
                        pdfFormFields.SetField("schoolname", schoolname + "\n" + saddress);
                        pdfFormFields.SetField("pageno", page.ToString());
                        //pdfFormFields.SetField("Hirer's Name", schoolname);
                        //pdfFormFields.SetFieldProperty("Hirer's Name", "textsizes", 30f, null);
                        page++;
                        skip = 15;
                        totalproduct = totalproduct - 15;
                        stamper.AcroFields.GenerateAppearances = true;
                        stamper.FormFlattening = true;
                        // close the pdf  
                        stamper.Close();
                        var pdfFile1 = new PdfReader(msTemp.ToArray());
                        for (int j = 1; j <= pdfFile1.NumberOfPages; j++)
                        {
                            //((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, j));
                            //pCopy.FreeReader(pdfFile);
                            var page2 = PDFwriter.GetImportedPage(pdfFile1, j);
                            PDFwriter.AddPage(page2);
                        }
                        pdfFile1.Close();
                    }
                    var remainproduct = productdetail.Skip(0).Take(totalproduct);
                    indexd = 1;
                    pdfTemplate = Path.Combine(Server.MapPath("~/templates/"), "page3.pdf");
                    pdfReader = new PdfReader(pdfTemplate);
                    msTemp = new MemoryStream();
                    stamper = new PdfStamper(pdfReader, msTemp);
                    pdfFormFields = stamper.AcroFields;
                    foreach (var de in pdfReader.AcroFields.Fields)
                    {
                        //sb.Append(de.Key.ToString() + Environment.NewLine);
                        pdfFormFields.SetFieldProperty(de.Key.ToString(), "textsize", (float)8, null);
                        //i++;
                    }
                    foreach (var ab in remainproduct)
                    {
                        var addr = "";
                        if (ab.DifferentAddress != null && ab.DifferentAddress != "")
                        {
                            var dadd = int.Parse(ab.DifferentAddress);
                            var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == dadd);
                            if (maddress != null)
                            {
                                addr = maddress.maddress1;
                                if (maddress.maddress2 != null)
                                {
                                    addr = addr + "\n" + maddress.maddress2;
                                }
                                if (maddress.mlandmark != null)
                                {
                                    addr = addr + "\n" + maddress.mlandmark;

                                }
                                addr = addr + "\n" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                            }

                        }

                        total = total + ((ab.mprice.HasValue ? ab.mprice.Value : 0) * ab.quantity);
                        pdfFormFields.SetField("Qty" + indexd, ab.quantity.Value.ToString());
                        pdfFormFields.SetField("Make" + indexd, ab.Make);
                        //pdfFormFields.SetField("model" + k, );
                        //pdfFormFields.SetField("equipdesc" + k, ab.mproductname);
                        pdfFormFields.SetField("Desc" + indexd, ab.mproductname);
                        pdfFormFields.SetField("New" + indexd, ab.Condition);
                        //pdfFormFields.SetField("location" + indexd, addr);
                        indexd++;
                    }
                    pdfFormFields.SetField("PrintName", contacperson);
                    pdfFormFields.SetField("Position2", position);

                    pdfFormFields.SetField("pageno2", page.ToString());
                    pdfFormFields.SetField("schoolname", schoolname + "\n" + saddress);
                    stamper.AcroFields.GenerateAppearances = true;
                    stamper.FormFlattening = true;
                    // close the pdf  
                    stamper.Close();
                    var pdfFile2 = new PdfReader(msTemp.ToArray());

                    for (int j = 1; j <= pdfFile2.NumberOfPages; j++)
                    {
                        //((PdfSmartCopy)pCopy).AddPage(pCopy.GetImportedPage(pdfFile, j));
                        //pCopy.FreeReader(pdfFile);
                        var page3 = PDFwriter.GetImportedPage(pdfFile2, j);
                        PDFwriter.AddPage(page3);
                    }
                    pdfFile2.Close();
                }

                PDFwriter.Close();
                doc.Close();
                //return "";
                // do stuff      
                //}


                return ms.ToArray();
            }
            //PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));

        }
        public ActionResult ResponseData()
        {
            return View();
        }
        public ActionResult ConfirmOrderWithAgreement(string agid, string r)
        {
            var db = new LeasingDbEntities();
            var d = SessionWrapper.agnos;
            if (agid != null)
            {
                var decid = int.Parse(Encryption.Decrypt(agid));
                var agreement = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == decid);
                if (agreement != null)
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                        var mleaseno = decid;
                        var email = ""; var name = ""; var muid = 0;
                        var response = client.GetAsync(apiurl + "v3/envelopes/" + agreement.menvelopeid).Result;
                        var get_response = response.Content.ReadAsStringAsync();
                        try
                        {
                            var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                            if (newuploadresult.envelope.envelope_status == "Signed")
                            {
                                var pdfUrl = newuploadresult.envelope.documents[0].document_file.uri;
                                using (WebClient wclient = new WebClient())
                                {
                                    var bytes = wclient.DownloadData(pdfUrl);
                                    string base64String = Convert.ToBase64String(bytes);
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
                                    //if (recipients.Signers[0].Email == email && recipients.Signers[0].Status == "completed")
                                    //{
                                    var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                                    if (leasedatas != null && leasedatas.mstatus != "partial")
                                    {
                                        leasedatas.mstatus = "partial";
                                        db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                                        db.SaveChanges();
                                        new Thread(() =>
                                        {
                                            var sendmail = SendThankYouemailBinaryFile(bytes, email, mleaseno, products.Count, products.FirstOrDefault().msorderref, "");
                                        }).Start();
                                    }

                                    //}
                                    d.Remove(decid);
                                    SessionWrapper.agnos = d;
                                    ViewBag.base64 = base64String;
                                    return View();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            return RedirectToAction("Index", "MyAccount");
                        }
                    }
                }
                if (d.Count() > 0)
                { }
                else
                {
                    return RedirectToAction("Index", "MyAccount");
                }

            }


            return View();
        }
        [HttpPost]
        public ActionResult LoadDocumentEsign(string accountId, string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiurl + "v3/uploads");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var mleaseno = int.Parse(documentid);
                var email = ""; var name = ""; var muid = 0;
                var response = client.GetAsync(apiurl + "v3/envelopes/" + envelopId).Result;
                var get_response = response.Content.ReadAsStringAsync();
                try
                {
                    var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                    if (newuploadresult.envelope.envelope_status == "Signed")
                    {
                        var pdfUrl = newuploadresult.envelope.documents[0].document_file.uri;
                        using (WebClient wclient = new WebClient())
                        {
                            var bytes = wclient.DownloadData(pdfUrl);
                            string base64String = Convert.ToBase64String(bytes);
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
                            //if (recipients.Signers[0].Email == email && recipients.Signers[0].Status == "completed")
                            //{
                            var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                            if (leasedatas != null)
                            {
                                leasedatas.mstatus = "partial";
                                db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            var sendmail = SendThankYouemailBinaryFile(bytes, email, mleaseno, products.Count, products.FirstOrDefault().msorderref, "");
                            //}

                            return Json(base64String);
                        }
                    }
                    return Json("");
                }
                catch (Exception e)
                {
                    return Json("");
                }
            }


        }
        [HttpPost]
        public ActionResult PendingDocumentEsign(string envelopId, string documentid)
        {
            var db = new LeasingDbEntities();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiurl + "v3/uploads");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var mleaseno = int.Parse(documentid);
                var email = ""; var name = ""; var muid = 0;
                var response = client.GetAsync(apiurl + "v3/envelopes/" + envelopId).Result;
                var get_response = response.Content.ReadAsStringAsync();
                try
                {
                    var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                    if (newuploadresult.envelope.envelope_status == "Signed")
                    {
                        var pdfUrl = newuploadresult.envelope.documents[0].document_file.uri;
                        using (WebClient wclient = new WebClient())
                        {
                            var bytes = wclient.DownloadData(pdfUrl);
                            string base64String = Convert.ToBase64String(bytes);
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
                            //if (recipients.Signers[0].Email == email && recipients.Signers[0].Status == "completed")
                            //{
                            var leasedatas = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
                            if (leasedatas != null)
                            {
                                leasedatas.mstatus = "partial";
                                db.Entry(leasedatas).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            var sendmail = SendThankYouemailBinaryFile(bytes, email, mleaseno, products.Count, products.FirstOrDefault().msorderref, "");
                            //}

                            return Json(new { url = base64String, st = "done" });
                        }
                    }
                    else
                    {
                        var url = apiurl + "link?e=" + newuploadresult.envelope.id + "&s=" + newuploadresult.envelope.signers[0].id;
                        return Json(new { url = url, st = "pending" });
                    }

                }
                catch (Exception e)
                {
                    return Json(new { url = "", st = "failed" });
                }
            }
            return Json(new { url = "", st = "failed" });



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
                    new Thread(() =>
                    {
                        var sendmail = SendThankYouemailBinaryFile(b, email, mleaseno, products.Count, products.FirstOrDefault().msorderref, "");
                    }).Start();
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
            if (envelopId != null && envelopId != "")
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
        public ActionResult getDocusignpdfEsign(string envelopId, string documentid)
        {
            //var db = new LeasingDbEntities();
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiurl + "v3/uploads");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var mleaseno = int.Parse(documentid);
                var email = ""; var name = ""; var muid = 0;
                var response = client.GetAsync(apiurl + "v3/envelopes/" + envelopId).Result;
                var get_response = response.Content.ReadAsStringAsync();
                try
                {
                    var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                    var pdfUrl = newuploadresult.envelope.documents[0].document_file.uri;
                    using (WebClient wclient = new WebClient())
                    {
                        var bytes = wclient.DownloadData(pdfUrl);
                        string base64String = Convert.ToBase64String(bytes);
                        //}

                        return Json(new { url = base64String, st = "done" });
                    }


                }
                catch (Exception e)
                {
                    return Json(new { url = "", st = "failed" });
                }

            }

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
                ViewBag.mdefaultadd = products.First().mdefaultadd;
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
        public static bool SendThankYouemail(string data, string emailto, int a, int noofitem, string pono, string requestedurl)
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
            if (MailDao.MailSend("Thank you for ordering with us", body, emailto, bytes) == "done")
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
                                mcontactid = l.mcontactid.HasValue ? l.mcontactid.Value : 0,
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
                byte[] b = { };

                UrlHelper url = new UrlHelper();
                var id = url.Encode(Encryption.Encrypt(a.ToString()));
                var body = "<table><tr height=50></tr></table>";
                body += "<table width='800' align='center' border='0' cellpadding='0' cellspacing='0'><tr>" +
                    "<td align=center>  <img src='https://equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log-dark.png' id='eyslogo'  height='60' border='0' alt='' align='center'></td>" +
                    "</tr>" +
                    "<tr height=10></tr>" +
                    "</table>";
                body += "<table width='800' align='center' border='0' cellpadding='0' cellspacing='0'>" +
                    "<tr align=center>" +
                    "<td><font size='5' color='' face=calibri>You have " + searchsenddata.Count + " pending agreements awaiting to be signed on equipyourschool.co.uk</font></td>" +
                    "</tr>" +
                    "<tr align=center>" +
                    "<td><font size='3' color='#9900ff' face=calibri><strong>A portal strictly for Education only</strong></font></td>" +
                    "</tr>" +
                    "<tr height=20></tr>" +
                    "</table>";
                body += "<table width='800' align='center' border='0' cellpadding='0' cellspacing='0' style='border:3px solid #dcdcdc; background-color:#f1f1f1; font-family:Verdana,Arial,Helvetica,sans-serif; font-size:12px;'>" +
                    "<tr align=center width=100% height=50>" +
                    "<td>" +
                    "<table width='800' align='center' border='0' cellpadding='0' cellspacing='0' style='background:#ffffff; border-style:solid; line-height:20px;font-family:Verdana,Arial,Helvetica,sans-serif; font-size:12px;'>" +
                    "<tr><td colspan='4' align='center' style='background:#02aff3;'><b style='color:#FFFFFF'>Your Pending Agreements awaiting to be signed</b></td></tr>" +
                    "<tr><td style='background:#c7c7c7;color:#000;'>Order Number</td><td style='background:#c7c7c7;color:#000;'>Date Agreement Generated</td><td style='background:#c7c7c7;color:#000;'>Generated By</td><td style='background:#c7c7c7;color:#000;'>Number of items</td></tr>";
                foreach (var searchsend in searchsenddata)
                {
                    var userdata = db.user_registration.FirstOrDefault(x => x.muid == searchsend.createdby);
                    var uname = "";
                    if (userdata != null)
                    {
                        uname = userdata.mname;
                    }
                    var usersubquote = db.usersubquotations.Where(x => x.magreementno == searchsend.mleaseno).ToList();
                    body += "<tr><td style='width:90px;'>" + searchsend.mleaseno + "</td><td>" + (searchsend.createddate.HasValue ? searchsend.createddate.Value.ToString("dd/MM/yyyy") : "") + "</td><td>" + uname + "</td><td>" + usersubquote.Count + "</td></tr>";
                }
                body += "</table></td></tr></table>" +
                "</td>" +
                    "</tr></table>" +
                    /*"<tr align=center width=100% height=50>" +
                    "<td><font size='3' color='blue' face=calibri><u>Please click here to check your pending agreement/s awaiting to be signed</u></font></td>" +
                    "</tr>" +*/
                    "<table width='800' align='center' border='0' cellpadding='0' cellspacing='0'>" +
                    "<tr height=30 width=100%>" +
                    "<td><font size='3' color='' face=calibri>If you have already signed this/these agreement/s, please ignore this email.</font></td>" +
                    "</tr>" +
                    "<tr height=30 width=100%>" +
                    "<td><font size='3' color='' face=calibri> If you have any queries in regards to this order, please request a callback by clicking <a style='text-decoration:none;text-align:left;' href = 'http://equipyourschool.co.uk/MyAccount/Index?tab=pendingsignatureleases&content=12' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;color:#017fbe;padding:0px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:0px'> Here </span></a></font></td>" +
                    "</tr>" +
                    "<tr height=30 width=50%>" +
                    "<td><font size='3' color='' face=calibri>Yours sincerely</font></td>" +
                    "</tr>" +
                    "<tr height=30 width=50%>" +
                    "<td><font size='3' color='' face=calibri>EYS Sales</font></td>" +
                    "</tr>" +
                    "</table>";

                /*body = "Respected School,<br/>";
                body += "This is pending signature reminder email from EYS<br/><br/>";
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
                    body += "<tr><td style='width:90px;'>" + searchsend.mleaseno + "</td><td>" + (searchsend.createddate.HasValue ? searchsend.createddate.Value.ToString("dd/MM/yyyy") : "") + "</td><td>" + uname + "</td><td>" + usersubquote.Count + "</td></tr>";
                }
                body += "</table></td></tr></table></td></tr></table>";

                //body += "We have attached a copy of your agreement generated for your order number " + a + "<br/><br/>";

                body += "Please click <a style='text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/MyAccount/Index?tab=pendingsignatureleases&content=12' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;color:#017fbe;padding:0px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:0px'> here </span></a> to check your pending agreement awaiting to be signed";
                body += "<br/><br/><p>*Please ignore if you already signed it.</p>";
                */
                /*body += "<div style = 'margin:0 auto;text-align:left;' ><p> Please confirm that if you have sent attached agreement duly signed by post to lessor.</p><br/><a style = 'text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/Leasing/UpdateAgSend?ag=" + id + "&status=yes' target = '_blank' data-saferedirecturl='#'><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> Yes </span></a>" +
                                 "<a style='text-decoration:none;text-align:left;' href = 'http://"+requestedurl+"/Leasing/UpdateAgSend?ag=" +  id + "&status=no' target = '_blank' data - saferedirecturl = '#' ><span style = 'text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'> No </span></a>" +
                                "</div>" +
                                "</div>";*/

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
                                  where ag.mleaseno == a && us.order_visible != 1
                                  select new
                                  {
                                      ag.mleaseno,
                                      ag.createddate,
                                      ag.muserid
                                  }).FirstOrDefault();
            if (searchsenddata != null)
            {
                var todaydata = DateTime.Today;
                var twodaylater = searchsenddata.createddate.HasValue ? searchsenddata.createddate.Value.AddDays(2) : DateTime.Today.AddDays(2);
                if (todaydata >= twodaylater)
                {
                    byte[] b = { };
                    UrlHelper url = new UrlHelper();
                    var id = url.Encode(Encryption.Encrypt(a.ToString()));
                    var getschooldata = db.user_registration_more.FirstOrDefault(x => x.musermoreid == searchsenddata.muserid);
                    var body = "This is reminder email from equipyourschool.co.uk<br/><br/>";
                    body += "Order Number: " + a + "<br/>";
                    body += "School Name: " + getschooldata.morgname + "<br/>";
                    body += "Order Date: " + (searchsenddata.createddate.HasValue ? searchsenddata.createddate.Value.ToString("dd/MM/yyyy") : "") + "<br/>";
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
