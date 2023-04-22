using iTextSharp.text.pdf;
using LeasingPortalApi.Models;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class HomeController : Controller
    {
        private string apiurl = WebConfigurationManager.AppSettings["esigndoc"];
        private string apikey = "8663CoES!syS";
        // GET: Home
        public ActionResult Index()
        {
            string Url = Request.Url.Host;
            //SessionWrapper.UserUrl = Url;

            ViewBag.msg = TempData["err"] as string;
            if (Url == "www.saicomputers.com" || Url=="equipyourschool.co.uk" || Url == "www.equipyourschool.co.uk")
            //if (Url == "www.saicomputers.com")
            {
                //return Redirect("index.htm");
                return RedirectToAction("index","RegisterNow");
            }
            else
            {
                return View();

            }
        }

        public ActionResult Indexblue1()
        {
            var db = new LeasingDbEntities();
            var products = (from l in db.userleaseagreements
                            join o in db.usersubquotations on l.mleaseno equals o.magreementno
                            where l.mleaseno == 328
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
            var mleaseid = 314;
            var udata = db.user_registration.FirstOrDefault(x => x.musermoreid == id);
            var email = udata.museridemail;
            var name = udata.mname;
            var muid = udata.muid;
            var content = CreateAgreement(products);
            byte[] b = content;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(apiurl + "v3/uploads");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Token token=" + apikey);
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var up = new EeUploadViewModel()
                {
                    title = "EYS Agreement - 101",
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
                try
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
                            var enve = new EnvelopeViewModel
                            {
                                title = "Agreements Pack",
                                subject = "You have agreement to sign",
                                documents = ddocs,
                                signers = signers,
                                envelope_options = envelope_opt,
                                redirect_uri = "https://equipyourschool.co.uk/Leasing/ConfirmOrder"
                            };
                            var finalenvelope = new ESignViewModel
                            {
                                envelope = enve,
                            };
                            json = JsonConvert.SerializeObject(finalenvelope);
                            data = new StringContent(json, Encoding.UTF8, "application/json");
                            //System.Diagnostics.Debug.WriteLine(data);
                            /*response = client.PostAsync(apiurl + "v3/envelopes", data).Result;
                            get_response = response.Content.ReadAsStringAsync();
                            var newuploadresult = JsonConvert.DeserializeObject<ESignViewModel>(get_response.Result);
                            var url = apiurl + "link?e=" + newuploadresult.envelope.id + "&s=" + newuploadresult.envelope.signers[0].id;
                            var leasedata = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseid);
                            if (leasedata != null)
                            {
                                leasedata.menvelopeid = docid;
                                db.Entry(leasedata).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }*/
                            return Json(new { url = "", accountId = "", envelopid = "", mleaseid = mleaseid });
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }


            }
            return View();
        }
        public byte[] CreateAgreemento(List<exportproductviewmodel> products)
        {
            string pdfTemplate = Path.Combine(Server.MapPath("~/templates/"), "Deferred Agreement.pdf");
            PdfReader pdfReader = new PdfReader(pdfTemplate);
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
                using (PdfStamper stamper = new PdfStamper(pdfReader, ms))
                {

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
                        var k = 1;
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
                            //pdfFormFields.SetField("equipdesc" + k, ab.mproductname);
                            pdfFormFields.SetField("equipdesc" + k, "This is for testing product name\nakdsjhsakdhaskjksafjlaskjfasjfj\nlkjsdljalsjdlj");
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
                    /*Agreement NO date Signature Field 1  deliverydate 2  
                     * date 1 Signature Field 2 
                     * DD-account-holder2 DD-manager  DD-address2 DD-postcode DD-date3 DD-acc-num1 
                     * DD-acc-num2 DD-acc-num3 DD-acc-num4 DD-acc-num5 DD-acc-num6 DD-acc-num7 DD-acc-num8 
                     * DD-sort-code1 DD-sort-code2 DD-sort-code3 DD-sort-code4 DD-sort-code5 DD-sort-code6 
                     * DD-ref1 DD-ref2 DD-ref3 DD-ref4 DD-ref5 DD-ref6 DD-ref7 
                     * Signature Field 3 Signature Field 4 
                     * Check Box 7 Radio Button 1 QTY2 Make2 model2 equipdesc2 new2 new3 serial2 location2 QTY3 Make3 model3 years2 
                     * deliverydate 3 months2 first2 further2 equipdesc3 serial3 location3*/
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
                    pdfFormFields.SetField("years1", (productdetail.First().mtermp / 12).Value.ToString());
                    pdfFormFields.SetField("months1", "0");
                    pdfFormFields.SetField("deliverydate", QuotationDAO.getPeriodInWords((productdetail.First().mtermp / productdetail.First().mtermf) - 1));
                    pdfFormFields.SetField("first1", Math.Round(total.HasValue ? total.Value : 0, 2).ToString());
                    pdfFormFields.SetField("further1", (Math.Round(total.HasValue ? total.Value : 0, 2)).ToString());
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
                    /*foreach (var de in pdfReader.AcroFields.Fields)
                    {
                        sb.Append(de.Key.ToString() + Environment.NewLine);
                        pdfFormFields.SetField(de.Key.ToString(), "Palak" + i);
                        i++;
                    }*/
                    //var keys = pdfFormFields.GetAppearanceStates("Radio Button 1");

                    //pdfFormFields.SetField("Radio Button 1", "Choice2");
                    // Write the string builder's content to the form's textbox
                    //pdfTemplate = @"c:\Temp\PDF\fw4.pdf";
                    // set form pdfFormFields  
                    // The first worksheet and W-4 form  
                    // report by reading values from completed PDF  
                    //string sTmp = "W-4 Completed for " + pdfFormFields.GetField("f1_09(0)") + " " + pdfFormFields.GetField("f1_10(0)");
                    //MessageBox.Show(sTmp, "Finished");
                    // flatten the form to remove editting options, set it to false  
                    // to leave the form open to subsequent manual edits  
                    stamper.AcroFields.GenerateAppearances = true;
                    stamper.FormFlattening = true;
                    // close the pdf  
                    stamper.Close();
                    //return "";
                    // do stuff      
                }
                return ms.ToArray();
            }
            //PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(newFile, FileMode.Create));

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

                string pdfTemplate = Path.Combine(Server.MapPath("~/templates/"), "Deferred Agreement.pdf");
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
                    var k = 1;
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
                pdfFormFields.SetField("years1", (productdetail.First().mtermp / 12).Value.ToString());
                pdfFormFields.SetField("months1", "0");
                pdfFormFields.SetField("deliverydate", QuotationDAO.getPeriodInWords((productdetail.First().mtermp / productdetail.First().mtermf) - 1));
                pdfFormFields.SetField("first1", Math.Round(total.HasValue ? total.Value : 0, 2).ToString());
                pdfFormFields.SetField("further1", (Math.Round(total.HasValue ? total.Value : 0, 2)).ToString());
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
                stamper.AcroFields.GenerateAppearances = true;
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

                    pdfFormFields.SetField("pageno", page.ToString());
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
        public ActionResult Indexblue2()
        {
            List<int> a = new List<int>();
            a.Add(1);
            a.Add(2);
            SessionWrapper.CustomerName = "Palak";
            SessionWrapper.agnos = a;
            a = SessionWrapper.agnos;
            return View();
        }
        public ActionResult Indexblue3()
        {
            return View();
        }
        public ActionResult Indexblue4()
        {
            return View();
        }
        public ActionResult Indexblue5()
        {
            return View();
        }

        public ActionResult Indexblue()
        {
            //string smtpuser = "accounts@funding4education.co.uk";
            //string smtppwd = "Welcome1";
            string smtpuser = "accounts@compulease.co.uk";
            //string smtppwd = "mwugimzqhkjyfcph";
            string smtppwd = "Qaq07315";
            string Host = "smtp.office365.com";
            int Port = 587;
            bool EnableSsl = true;
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
            using (var mess = new MailMessage("accounts@compulease.co.uk", "shahpalakh@gmail.com")
            {
                Subject = "Test Email",
                Body = "test",
                IsBodyHtml = true
            })
            {
                smtp.Send(mess);
            }
            return View();
        }
        public ActionResult Indexpurple()
        {
            return View();
        }
        public ActionResult Index3()
        {
            string Url = Request.Url.Host;
            //SessionWrapper.UserUrl = Url;


            return View();


        }
        public ActionResult Index4()
        {
            string Url = Request.Url.Host;
            //SessionWrapper.UserUrl = Url;


            return View();

        }
        public ActionResult IEIndex()
        {
            return View();
        }
        public ActionResult oldIndex()
        {
            string Url = Request.Url.Host;
            //SessionWrapper.UserUrl = Url;

            ViewBag.msg = TempData["err"] as string;
            if (Url == "www.saicomputers.com")
            {
                return Redirect("index.htm");

            }
            else
            {
                return View();

            }
        }
        public ActionResult Index2()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetAllProductById(ProductParameterViewModel param)
        {
            var getid = param.getid;
            var row = param.row;
            var rowperpage = param.rowperpage;
            var filter = param.filter;
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetProducts(getid).ToList();
            var getprod = getallproducts;
            if (param.min > 0 && param.max > 0)
            {
                getprod = (from p in getallproducts
                           where p.mprodcatid == id && p.mrp >= param.min && p.mrp <= param.max
                           select p).ToList();
            }
            if (param.filterdata.Any(x => x.FilterUrl == "bn"))
            {
                getprod = (from p in getprod
                           where p.mprodcatid == id && param.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                           select p).ToList();
            }

            if (param.filterdata.Any(x => x.FilterUrl == "a"))
            {
                List<int> productid = new List<int>();
                List<int> getproducts = new List<int>();

                foreach (var ab in param.filterdata.Where(x => x.FilterUrl == "a"))
                {
                    var ss = ab.FilterValue.Split(',');
                    if (getproducts.Count <= 0 || !getproducts.Any())
                    {
                        getproducts = (from c in ctx.productsubtwoes
                                       where c.mdetailheadid == ab.FilterId && ss.Any(z => z == c.mdetailvalue)
                                       select c.mprodid.Value).ToList();

                    }
                    else
                    {
                        getproducts = (from c in ctx.productsubtwoes
                                       where c.mdetailheadid == ab.FilterId && ss.Any(z => z == c.mdetailvalue) && getproducts.Any(z => z == c.mprodid)
                                       select c.mprodid.Value).ToList();
                    }
                    //if (getproducts.Any())
                    //{
                    productid = getproducts;
                    //}
                }
                getprod = (from p in getprod
                           join pids in productid on p.mprodid equals pids
                           where p.mprodcatid == id
                           select p).ToList();

            }
            /*if (getprod == null)
            {
                getprod = (from p in getallproducts
                           where p.mprodcatid == id
                           select p).ToList();

            }*/
            var finalprod = getprod;//.ToList();
            products = (from p in finalprod
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mbrandlogo = p.mbrandlogo,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msstock = p.msstock.HasValue ? p.msstock.Value : 0,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<ProductDisplayViewModel>();

            if (products.Count == 0)
            {
                return Content("");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });

                var attrfilter = (from g in ctx.spGetFilters(getid).ToList()
                                  select g).GroupBy(x => x.mdetailheadid).Select(y => new ProductFilterViewModel
                                  {
                                      FilterHead = y.FirstOrDefault().mdetailhead,
                                      FilterData = y.GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(k => new ProductFilterDetailViewModel
                                      {
                                          FilterId = k.FirstOrDefault().mdetailheadid.Value,
                                          FilterUrl = "a",
                                          FilterValue = k.Key
                                      }).OrderBy(x => x.FilterValue).ToList()
                                  });
                filters.AddRange(attrfilter);
                /*var getfiltercat = ctx.productcomparativefields.Where(x => x.Filter == "Y" && x.CategoryId == getid).ToList();
                if (getfiltercat.Any())
                {
                    foreach (var ab in getfiltercat)
                    {
                        var getdetail = ctx.productdetailmasters.FirstOrDefault(x => x.mprodetailid == ab.DetailId);
                        var attributefilters = (from g in products
                                                join subtwo in ctx.productsubtwoes on g.mprodid equals subtwo.mprodid
                                                where subtwo.mdetailheadid == ab.DetailId && subtwo.mdetailvalue != "" && subtwo.mdetailvalue != null
                                                select subtwo).GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(y => new ProductFilterDetailViewModel
                                                {
                                                    FilterId = y.FirstOrDefault().mdetailheadid.Value,
                                                    FilterUrl = "a",
                                                    FilterValue = y.Key
                                                }).OrderBy(x => x.FilterValue).ToList();
                        if (attributefilters.Any())
                        {
                            filters.Add(new ProductFilterViewModel
                            {
                                FilterHead = getdetail.mdetailhead,
                                FilterData = attributefilters
                            });
                        }
                    }
                }*/
            }
            
            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            if (param.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).ToList();
            }
            else if (param.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).ToList();
            }
            else if (param.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderByDescending(x => x.msstock).Skip(row).Take(rowperpage).ToList();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                var prodvariaton = (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null));
                x.mprodname = x.mprodname + " " + (prodvariaton.Any() ? prodvariaton.AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                var subpicdata = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
        }
        public static int getNumberOfOffer(int prodid, int msupplierid)
        {


            var ctx = new LeasingDbEntities();
            var products = (from pcv in ctx.productsubs
                            join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                            where psub.ProdSubId == prodid && psub.SupplierId != msupplierid
                            select psub).ToList();

            if (products.Count <= 0)
            {
                var products2 = (from pcv in ctx.productsubs
                                 join p in ctx.products on pcv.mprodid equals p.mprodid
                                 join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                 where p.mprodcatid != prodid && pcv.mprodsubid != prodid
                                 select pcv).ToList();
                if (products2.Count > 0)
                {
                    return products2.Count;
                }
                return 0;

            }
            else
            {
                return products.Count;
            }
        }
        public List<string> getStarRating(int prodid, LeasingDbEntities db)
        {
            var getdata = db.productreviews.Where(x => x.mprodid == prodid);
            List<string> ratedata = new List<string>();
            if (getdata.Any())
            {
                var ratestar = getdata.Average(x => x.mrankno);
                var cssper = (ratestar * 100) / 5;
                ratedata.Add(cssper.ToString());
                ratedata.Add(getdata.Count().ToString());
            }
            return ratedata;
        }
        public static List<SingleProductDetailViewModel> GetSpecs(int prodid)
        {
            var ctx = new LeasingDbEntities();
            var productdetail = new List<SingleProductDetailViewModel>();
            productdetail = (from p in ctx.products
                             join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                             join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                             join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                             from pu in pudata.DefaultIfEmpty()
                             where p.mprodid == prodid && pc.mdetailvalue != null && pc.mdetailvalue != ""
                             select new SingleProductDetailViewModel()
                             {
                                 morder = pd.morder.HasValue ? pd.morder.Value : 100,
                                 mprodid = prodid,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).OrderBy(x => x.morder).Take(4).ToList();

            return productdetail;
        }
        public ActionResult SamplePdf(int mleaseid)
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

    }
}