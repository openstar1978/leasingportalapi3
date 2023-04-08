using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeasingPortalApi.Models;
using System.Threading;
using System.IO;
using HtmlAgilityPack;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace LeasingPortalApi.Controllers
{
    public class QuotationController : Controller
    {
        // GET: Quotation
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult LoadQuotationOld(int mquoteid,List<int> subquoteids)
        {
            var db = new LeasingDbEntities();
            var models = (from q in db.userquotationmasters
                          where q.mquoteid == mquoteid && q.mquotestatus == "pending"
                          select new SingleQuotationViewModel
                          {
                              mquotedate=q.createddate,
                              mquoteref=q.mquoteref,
                              mquoteid = q.mquoteid,
                              mquotetotal = q.mquotetotal,
                              muserid = q.muserid.HasValue ? q.muserid.Value : 0
                          }).FirstOrDefault();
            ViewBag.subquoteid = subquoteids;   
            return View(models);
        }
        [HttpPost]
        public ActionResult LoadQuotation(int muserid,List<int> subquoteids)
        {
            var db = new LeasingDbEntities();
            ViewBag.userid = muserid;
            ViewBag.subquoteid = subquoteids;
            return View();
        }
        [HttpPost]
        public ActionResult LoadSingleItemQuotation(int muserid, SaveCartViewModel product)
        {
            var db = new LeasingDbEntities();
            //new Thread(() =>
            //{
                var printlogs = new printquotelogviewmodel
                {
                    prod_id = product.mprodvarid,
                    user_id = muserid,
                    supplier_id = product.msupplierid,
                    mprice=product.mprice,
                    mtermf=product.mtermf,
                    mtermp=product.mtermp,
                    quantity=product.mqty,
                   
                    
                };

                var qid=QuotationDAO.SavePrintQuoteLog(printlogs);
            //}).Start();
            ViewBag.quoteid = qid;
            ViewBag.userid = muserid;
            ViewBag.subquoteid =product;
            /*var c = "";
            using (StringWriter writer = new StringWriter())
            {
                ViewEngineResult vResult = ViewEngines.Engines.FindView(ControllerContext,"LoadSingleItemQuotation", null);
                ViewContext vContext = new ViewContext(this.ControllerContext, vResult.View, ViewData, new TempDataDictionary(), writer);
                vResult.View.Render(vContext, writer);
                c=writer.ToString();
            }
            HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
            HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
            HtmlDocument doc = new HtmlDocument();
            doc.OptionFixNestedTags = true;
            doc.LoadHtml(c);
            c = doc.DocumentNode.OuterHtml;
            MemoryStream stream = new System.IO.MemoryStream();
            var fnmae = "tempQp" + DateTime.Now.Ticks+".pdf";

            try
            {
                string fullPath = Path.Combine(Server.MapPath("~/Content/temp"), fnmae);
                FileStream file;
                if (System.IO.File.Exists(fullPath))
                {
                    file = new FileStream(fullPath, FileMode.Open, FileAccess.Write);
                }
                else
                {
                    file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);

                    //file = new FileStream(fullPath, FileMode.Open, FileAccess.Write);
                }

                Encoding unicode = Encoding.UTF8;
                StringReader sr = new StringReader(c);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                writer.CloseStream = false;
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                
                stream.WriteTo(file);
                file.Close();
                // file.sa stream.ToArray();
                                     //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");

            }
            finally
            {
                stream.Dispose();
                db.Dispose();
            }


            return Content(fnmae);*/
            return View();
        }
        [HttpPost]
        public ActionResult LoadReferenceQuotation(int muserid, List<int> subquoteids)
        {
            var db = new LeasingDbEntities();
            ViewBag.userid = muserid;
            ViewBag.subquoteid = subquoteids;
            return View();
        }
    }
}