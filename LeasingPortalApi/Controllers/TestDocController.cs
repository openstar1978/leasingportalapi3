using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using LeasingPortalApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class TestDocController : Controller
    {
        MyCredential credential = new MyCredential();
        private string INTEGRATOR_KEY = "775d9b45-326c-4253-9d63-930da6899635";
        public ActionResult SendDocumentforSign()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendDocumentforSign(Recipient recipient, HttpPostedFileBase UploadDocument)
        {
            Models.Recipient recipientModel = new Models.Recipient();
            string directorypath = Server.MapPath("~/App_Data/" + "Files/");
            if (!Directory.Exists(directorypath))
            {
                Directory.CreateDirectory(directorypath);
            }
            byte[] data;
            using (Stream inputStream = UploadDocument.InputStream)
            {
                MemoryStream memoryStream = inputStream as MemoryStream;
                if (memoryStream == null)
                {
                    memoryStream = new MemoryStream();
                    inputStream.CopyTo(memoryStream);
                }
                data = memoryStream.ToArray();
            }
            var serverpath = directorypath + recipient.Name.Trim() + ".pdf";
            System.IO.File.WriteAllBytes(serverpath, data);
            var r=docusign(serverpath, recipient.Name, recipient.Email);
            ViewBag.r = r;
            return View("Index");
        }
        public ActionResult Index()
        {
            string usr = "palak@saicomputer.com";
            string pwd = "saibaba99";
            
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
            ViewBag.acc = accountId;
            ViewBag.r = TempData["r"] as string;
            return View();
        }
        public string loginApi(string usr, string pwd)
        {
            
            // we set the api client in global config when we configured the client  
            Configuration.Default.ApiClient= new ApiClient("https://demo.docusign.net/restapi");
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
        public void docusign2(string path, string recipientName, string recipientEmail)
        {
            ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
            Configuration.Default.ApiClient = apiClient;
            //Verify Account Details  
            string accountId = loginApi(credential.UserName, credential.Password);
            // Read a file from disk to use as a document.  
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            EnvelopeDefinition envDef = new EnvelopeDefinition();
            envDef.EmailSubject = "Please sign this doc";
            // Add a document to the envelope  
            Document doc = new Document();
            doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
            doc.Name = Path.GetFileName(path);
            doc.DocumentId = "1";
            envDef.Documents = new List<Document>();
            envDef.Documents.Add(doc);
            // Add a recipient to sign the documeent  
            DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
            signer.Email = recipientEmail;
            signer.Name = recipientName;
            signer.RecipientId = "1";
            envDef.Recipients = new DocuSign.eSign.Model.Recipients();
            envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
            envDef.Recipients.Signers.Add(signer);
            //set envelope status to "sent" to immediately send the signature request  
            envDef.Status = "sent";
            // |EnvelopesApi| contains methods related to creating and sending Envelopes (aka signature requests)  
            EnvelopesApi envelopesApi = new EnvelopesApi();
           
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
            // print the JSON response  
            var result = JsonConvert.SerializeObject(envelopeSummary);
        }
        
        public string docusign(string path, string recipientName, string recipientEmail)
        {
            try
            {
                ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
                Configuration.Default.ApiClient = apiClient;
                //Verify Account Details  
                string accountId = loginApi(credential.UserName, credential.Password);
                // Read a file from disk to use as a document.  
                byte[] fileBytes = System.IO.File.ReadAllBytes(path);
                EnvelopeDefinition envDef = new EnvelopeDefinition();
                envDef.EmailSubject = "Please sign this doc";
                // Add a document to the envelope  
                Document doc = new Document();
                doc.DocumentBase64 = System.Convert.ToBase64String(fileBytes);
                doc.Name = Path.GetFileName(path);
                doc.DocumentId = "1";
                envDef.Documents = new List<Document>();
                envDef.Documents.Add(doc);
                // Add a recipient to sign the documeent  
                DocuSign.eSign.Model.Signer signer = new DocuSign.eSign.Model.Signer();
                signer.Email = recipientEmail;
                signer.Name = recipientName;
                signer.RecipientId = "1";
                signer.ClientUserId = "1234";
                envDef.Recipients = new DocuSign.eSign.Model.Recipients();
                envDef.Recipients.Signers = new List<DocuSign.eSign.Model.Signer>();
                envDef.Recipients.Signers.Add(signer);
                envDef.Status = "sent";
                EnvelopesApi envelopesApi = new EnvelopesApi();

                EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(accountId, envDef);
                string clientUserId = "1234";
                // get a URL that can be placed in a browser or embedded in an IFrame
                string returnUrl = "http://localhost:15000/TestDoc/ResponseData";
                RecipientViewRequest recipientView = new RecipientViewRequest()
                {
                    ReturnUrl = returnUrl,
                    ClientUserId = clientUserId,
                    AuthenticationMethod = "email",
                    UserName = envDef.Recipients.Signers[0].Name,
                    Email = envDef.Recipients.Signers[0].Email
                };

                ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, envelopeSummary.EnvelopeId, recipientView);
                //Assert.IsNotNull(viewUrl);
                //Assert.IsTrue(!string.IsNullOrWhiteSpace(viewUrl.Url));
                Trace.WriteLine("ViewUrl is " + viewUrl);
                ViewBag.url = viewUrl.Url;
                return viewUrl.Url;
                /// Start a browser to Sign
                //System.Diagnostics.Process.Start(viewUrl.Url);
            }
            catch (DocuSign.eSign.Client.ApiException apiEx)
            {
                return apiEx.Message.ToString();
                //Assert.IsNotNull(apiEx.ErrorCode);
                //Assert.IsTrue(!string.IsNullOrWhiteSpace(apiEx.Message));
                //Assert.IsTrue(false, "Failed with ErrorCode: " + apiEx.ErrorCode + ", Message: " + apiEx.Message);
            }
        }
        public ActionResult GetEnvelopeInformationTest()
        {
            try
            {

                // get the logininformation and accountId
                ApiClient apiClient = new ApiClient("https://demo.docusign.net/restapi");
                Configuration.Default.ApiClient = apiClient;
                //Verify Account Details  
                string accountId = loginApi(credential.UserName, credential.Password);

               
                EnvelopesApi envelopesApi = new EnvelopesApi();
                
                // Get the status of the envelope
                Envelope envelope = envelopesApi.GetEnvelope(accountId, "157c618d-92d3-4a76-b929-5040c3256fb4", null);
                dynamic e = JsonConvert.DeserializeObject<dynamic>(envelope.ToJson());
                var e2 = envelope.Status;
                
                Trace.WriteLine("Envelope: " + envelope.ToJson());
                ViewBag.d = e2.ToString();
                return View();
            }
            catch (DocuSign.eSign.Client.ApiException apiEx)
            {
                return Content(apiEx.Message.ToString());
            }
        }
        public ActionResult ResponseData()
        {
            return View();
        }
    }
    public class MyCredential
    {
        public string UserName
        {
            get;
            set;
        } = "palak@saicomputer.com";
        public string Password
        {
            get;
            set;
        } = "saibaba99";
    }
}
