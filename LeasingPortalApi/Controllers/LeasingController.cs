using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class LeasingController : Controller
    {
        // GET: Leasing
        
        public ActionResult Index()
        {
            ViewBag.verify = TempData["verify"];
            return View();
        }
        public ActionResult Indexold()
        {
            return View();
        }
        public ActionResult TestProduct()
        {
            return View();
        }
        
        public ActionResult SearchProducts(string category, string searchproducts,string post_type,string brand)
        {
            if (searchproducts == null || searchproducts == "")
            {
                return RedirectToAction("Index");
            }
            ViewBag.Category = category;

            ViewBag.SearchProduct = (post_type=="bpt"?brand:searchproducts);
            ViewBag.post_type = post_type;
            return View();
        }
        public ActionResult Products(string Category, string getproducts, string p)
        {
            if (getproducts == null)
            {
                return RedirectToAction("Index");
            }
            var db = new LeasingDbEntities();
            var id = Encryption.Decrypt(getproducts);
            var newid = int.Parse(id);
            var str = "";
            str = GetParent(newid, str);
            ViewBag.ParentCat = p;
            ViewBag.NewCat = Category;
            ViewBag.Category = str;
            ViewBag.id = id;

            if (p == null)
            {
                return View();
            }

            else { 

                return View("ProductByCategory");
            }
        }
        public ActionResult CategoryDetails(string dept, string a, string p, string aid)
        {
            ViewBag.area = dept;
            ViewBag.areaname = Encryption.Decrypt(p);
            ViewBag.areaid = Encryption.Decrypt(a);
            var str = "";
            ViewBag.ParentCat = p;
            str = GetAreaParent(int.Parse(Encryption.Decrypt(a)), str, aid, "cat");
            ViewBag.Category = str;
            return View();
            
        }
        public ActionResult ShopByArea(string dept,string a,string p,string aid)
        {
            ViewBag.area = dept;
            ViewBag.areaname = Encryption.Decrypt(p);
            ViewBag.areaid= Encryption.Decrypt(a);
            var str = "";
            if (dept == "area")
            {
                ViewBag.aid = a;

                str = "<span class=\"delimiter\"><i class=\"fa fa-angle-right\"></i></span><a href='ShopByArea?Dept=area&a=" + HttpUtility.UrlEncode(a) + "&p=" + HttpUtility.UrlEncode(p) + "' style='background:transparent'>" + Encryption.Decrypt(p)+"</a>";
                
            }
            else
            {
                if (dept == "cat" && aid=="")
                {
                    
                    ViewBag.aid = aid;

                    var db = new LeasingDbEntities();
                    str = GetAreaParent(int.Parse(Encryption.Decrypt(a)), str, aid, "area");
                    //var arid = int.Parse(Encryption.Decrypt(aid));
                    
                    //str = "<span class=\"delimiter\"><i class=\"fa fa-angle-right\"></i></span><a href='ShopByArea?Dept=area&a=" + HttpUtility.UrlEncode(aid) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(getareaname)) + "' style='background:transparent'>" + getareaname + "</a> " + str;
                }
                else
                {
                    ViewBag.aid = aid;

                    var db = new LeasingDbEntities();
                    str = GetAreaParent(int.Parse(Encryption.Decrypt(a)), str, aid, "area");
                    var arid = int.Parse(Encryption.Decrypt(aid));
                    var getareaname = db.tblschoolareas.FirstOrDefault(x => x.mareaid == arid).mareaname;

                    str = "<span class=\"delimiter\"><i class=\"fa fa-angle-right\"></i></span><a href='ShopByArea?Dept=area&a=" + HttpUtility.UrlEncode(aid) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(getareaname)) + "' style='background:transparent'>" + getareaname + "</a> " + str;
                }
                
            }
            
            ViewBag.ParentCat = p;
            ViewBag.Category = str;
            return View();
        }
        public ActionResult ProductByCategory()
        {
            return View();
        }
        public string GetParent(int id,string str)
        {
            var db = new LeasingDbEntities();
            if (id == 0)
            {
                return str;
            }
            else
            {
                var stdata = db.productcats.FirstOrDefault(x => x.mprodcatid == id);
                if (stdata != null)
                {
                    if (stdata.mpreviouscat != 0 || str=="")
                    {
                        str = "<span class=\"delimiter\"><i class=\"fa fa-angle-right\"></i></span>" + stdata.mcatname + "" + str;
                    }
                    return GetParent(stdata.mpreviouscat.HasValue?stdata.mpreviouscat.Value:0, str); 
                }
                return str;
            }
            //return str;
        }
        public string GetAreaParent(int id, string str,string aid,string from)
        {
            var db = new LeasingDbEntities();
            if (id == 0)
            {
                return str;
            }
            else
            {
                var stdata = db.productcats.FirstOrDefault(x => x.mprodcatid == id);
                if (stdata != null)
                {
                    if (stdata.mpreviouscat != 0 || str == "")
                    {
                        var f = "ShopByArea";
                        if (from == "cat")
                        {
                            f = "CategoryDetails";
                        }
                        str = "<span class=\"delimiter\"><i class=\"fa fa-angle-right\"></i></span><a href='"+f+"?Dept=cat&a=" + HttpUtility.UrlEncode(Encryption.Encrypt(id.ToString())) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(stdata.mcatname)) + "&aid="+ HttpUtility.UrlEncode(aid) + "' style='background:transparent'>" + stdata.mcatname + "</a> " + str;
                    }
                    return GetParent(stdata.mpreviouscat.HasValue ? stdata.mpreviouscat.Value : 0, str);
                }
                return str;
            }
            //return str;
        }
        
        public ActionResult ProductOption(string getproduct, string productname)
        {
            if (getproduct == null)
            {
                return RedirectToAction("Index");
            }
            var id = Encryption.Decrypt(getproduct);
            //ViewBag.Category = Category;
            ViewBag.id = id;
            return View();
        }
        public ActionResult AllProduct()
        {
            return View();
        }
        public ActionResult ViewCart()
        {
            ViewBag.parenturl = Request.UrlReferrer;
            return RedirectToAction("YourBag");
        }
        public ActionResult ViewCart2()
        {
            ViewBag.parenturl = Request.UrlReferrer;
            return View();
        }
        public ActionResult YourBag()
        {
            ViewBag.parenturl = Request.UrlReferrer;
            return View();
        }
        public ActionResult ViewProduct(string getproduct, string productname, string update)
        {
            ViewBag.parenturl = Request.UrlReferrer;
            ViewBag.Product = Encryption.Decrypt(getproduct);
            ViewBag.ProductUpdate = update;
            return View();
        }
        public ActionResult ViewUpdateProduct(string getproduct, string productname)
        {
            ViewBag.parenturl = Request.UrlReferrer;
            ViewBag.Product = Encryption.Decrypt(getproduct);
            ViewBag.ProductUpdate = "update";
            return View("viewProduct");
        }
        public ActionResult Checkout()
        {
            return View();
        }
        public ActionResult ProductDetail(string getproduct, string productname, string ptype,string getsup)
        {
            //var db = new LeasingDbEntities();
            var id = Encryption.Decrypt(getproduct);
            ViewBag.Product = productname;
            ViewBag.id = id;
            ViewBag.ptype = ptype;
            var getsup2 = "0";
            if (getsup != null)
            {
                getsup2= Encryption.Decrypt(getsup);
            }
            ViewBag.getsup = getsup2;
            var prodid = int.Parse(id);
            //ViewBag.imggallery = db.productsubpics.Where(x => x.mprodsubid == prodid).ToList();
            return View();
        }
        public ActionResult ProductDetail2(string getproduct, string productname)
        {
            //var db = new LeasingDbEntities();
            var id = Encryption.Decrypt(getproduct);
            ViewBag.Product = productname;
            ViewBag.id = id;
            var prodid = int.Parse(id);
            //ViewBag.imggallery = db.productsubpics.Where(x => x.mprodsubid == prodid).ToList();
            return View();
        }
        [HttpGet]
        public ActionResult MoreSupplier(string getsuppliers, string product)
        {
            //var db = new LeasingDbEntities();
            var id = Encryption.Decrypt(getsuppliers);
            ViewBag.id = id;
            ViewBag.productname = product;
            ViewBag.supplier = getsuppliers;
            var prodid = int.Parse(id);

            //ViewBag.imggallery = db.productsubpics.Where(x => x.mprodsubid == prodid).ToList();
            return View();
        }
        public ActionResult YourQuotation()
        {
            return View();
        }
        [HttpGet]
        public ActionResult QuoteDetail1()
        {
            //var id = Encryption.Decrypt(quoteid);
            //SViewBag.id = id;
            ViewBag.activetab = "";
            return View();
        }
        [HttpGet]
        
        public ActionResult QuoteDetail(string tab,int? content)
        {
            //var id = Encryption.Decrypt(quoteid);
            //SViewBag.id = id;
            ViewBag.msg = TempData["MSG"];
            ViewBag.activetab = content;
            return View();
        }
        [HttpGet]

        public ActionResult myaccount(string tab, int? content)
        {
            //var id = Encryption.Decrypt(quoteid);
            //SViewBag.id = id;
            ViewBag.msg = TempData["MSG"];
            ViewBag.activetab = content;
            return View();
        }
        [HttpPost]
        public ActionResult QuoteDetailOld(string quoteid)
        {
            var id = Encryption.Decrypt(quoteid);
            ViewBag.id = id;
            return View();
        }
        [HttpGet]
        public ActionResult QuoteApprovement()
        {
            //var id = Encryption.Decrypt(quoteid);
            //ViewBag.id = id;
            return View();
        }
        [HttpPost]
        public ActionResult QuoteApprovementOld(string quoteid)
        {
            var id = Encryption.Decrypt(quoteid);
            ViewBag.id = id;
            return View();
        }
        public ActionResult Registration()
        {
            return View();
        }
        public ActionResult YourOrders()
        {
            return View();
        }
        public ActionResult ConfirmOrder()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult OrderDetail(string quoteid)
        {
            var id = Encryption.Decrypt(quoteid);
            ViewBag.id = id;
            return View();
        }
        public ActionResult ProductNew(string Category, string getproducts)
        {
            if (getproducts == null)
            {
                return RedirectToAction("Index");
            }
            var id = Encryption.Decrypt(getproducts);
            ViewBag.Category = Category;
            ViewBag.id = id;
            return View();
        }
        public ActionResult PartialWarrantyModal(int wid)
        {
            var db = new LeasingDbEntities();
            var productview = (from w in db.productwarrantybysuppliers
                               where w.WarrantyId == wid
                               select new ProductWarrantyViewModel
                               {
                                    WarrantyId=w.WarrantyId,
                                   WarrantyCode=w.WarrantyCode,
                                   WarrantyDesc=w.WarrantyDesc,
                                   WarrantyTitle=w.WarrantyTitle,
                                   DesignedFor=w.DesignedFor,
                                   ServiceAvailability=w.ServiceAvailability,
                                   ServiceSupport=w.ServiceSupport,
                               }).FirstOrDefault();
            return View(productview);
        }
        public ActionResult faqs()
        {
            return View();
        }
        public ActionResult SchoolFaqs()
        {
            return View();
        }
        public ActionResult SupplierFaqs()
        {
            return View();
        }
        public ActionResult PrivacyPolicy()
        {
            return RedirectToAction("PP");
        }
        public ActionResult PP()
        {
            return View();
        }
        public ActionResult Disclaimer()
        {
            return View();
        }
        public ActionResult ReturnRefundPolicy()
        {
            return View();
        }
        public ActionResult TermsConditions()
        {
            return View();
        }
        public ActionResult FeatureProducts()
        {
            return View();
        }
        public ActionResult NewArrivalProducts()
        {
            return View();
        }
        public ActionResult SpecialOfferProducts()
        {
            return View();
        }
        public ActionResult VirtualTour()
        {
            return View();
        }
        [HttpGet]
        public ActionResult PartialAddOn(int prodid)
        {
            var id = prodid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = new List<ProductDisplayViewModel>();
            var ctx = new LeasingDbEntities();
            var paddon = ctx.productaddondetails.Where(x => x.mselecteprod == id);
            if (paddon.Any())
            {
                foreach (var ab in paddon)
                {
                    var p2 = (from pcv in ctx.productsubs
                              join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                              join p in ctx.products on pcv.mprodid equals p.mprodid
                              join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                              join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                              join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                              from users in userdata.DefaultIfEmpty()
                              where psupp.Id == ab.maddonprodid && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                              select new ProductDisplayViewModel()
                              {
                                  mprodid = p.mprodid,

                                  mprodname = p.mprodname,
                                  mprodcatname = pc.mcatname,
                                  mbrandid = pbrand.mbrandid,
                                  mbrand = pbrand.mbrandname,
                                  mtermf = 1,
                                  mtermp = 60,
                                  msuppliername = users.morgname,
                                  //msubpicname = y.Key.pic,
                                  mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                  mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                  mprodvarid = pcv.mprodsubid,
                                  minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                  msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                  msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                  msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                  msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                  msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                  msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                  manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                  MPN = p.mmanufacturercode,
                                  EYS = pcv.muniquecode
                              }).FirstOrDefault();
                    if (p2 != null)
                    {
                        products.Add(p2);
                    }


                }

                if (products.Count == 0)
                {
                    return Content("failed");
                }
                products = products.ToList();
                products.ForEach(x =>
                {
                    var getrating = getStarRating(x.mprodvarid, ctx);
                    if (getrating.Any())
                    {
                        x.avgrating = getrating[0];
                        x.totalrating = getrating[1];

                    }
                    x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                    x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
                });

                return View(products);
            }
            return Content("failed");
            
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
        [HttpGet]
        public ActionResult ReviewandRating(string productname,string getproduct)
        {
            if (getproduct == null || getproduct == "")
            {
                return RedirectToAction("Index");
            }
            //getproduct = HttpUtility.UrlDecode(getproduct);
            ViewBag.msg = TempData["msg"] as string;
            ViewBag.data = TempData["data"] as string;
            TempData["msg"] = "";
            TempData["data"] = "";
            ViewBag.SearchProduct = Encryption.Decrypt(getproduct);
            return View();
        }
        [HttpGet]
        public ActionResult UpdateAgSend(string ag,string status)
        {
            var db = new LeasingDbEntities();
            var agid = int.Parse(Encryption.Decrypt(ag));
            if (status == "yes")
            {

                var searchag = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == agid);
                if (searchag != null)
                {
                    searchag.msend = 2;
                    db.Entry(searchag).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }
            }
            else
            {
                var searchag = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == agid);
                if (searchag != null)
                {
                    searchag.msend = 1;
                    db.Entry(searchag).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            TempData["MSG"] = "Success";
            return RedirectToAction("Index","MyAccount");
        }
        
    }

}
