using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;
using System.Web.SessionState;
using System.Web;
using System.Threading;
using Hangfire;

namespace LeasingPortalApi.Controllers.Api
{
    public class CookieCartController : ApiController
    {
        [HttpPost]
        [Route("api/CookieApi/SaveCookieCart")]
        public IHttpActionResult SaveCookieCart(SaveCartViewModel datas)
        {
            var db = new LeasingDbEntities();
            List<string> d = new List<string>();

            if (datas.mprice != null)
            {
                //foreach(var datas in datas2)
                //{
                var sessionid = "";
                if (datas.Session_Id != "" && datas.Session_Id != null)
                {
                    sessionid = datas.Session_Id;

                }
                else
                {
                    sessionid = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace('+', '_').TrimEnd('=');
                    sessionid = sessionid + "-" + (DateTime.Now.Ticks);
                }
                var obj2 = new SaveCart
                {
                    mprodid = datas.mprodid,
                    mprice = datas.mprice,
                    mproducturl = datas.mproducturl,
                    mprodvarid = datas.mprodvarid,
                    msubpicname = datas.msubpicname,
                    msupplierid = datas.msupplierid,
                    mtermf = datas.mtermf,
                    mtermp = datas.mtermp,
                    mprodname = datas.mprodname,
                    mqty = datas.mqty,
                    moffer=datas.moffer,
                    mwarrantyflag=0,
                    Session_Id = sessionid,
                    minstallationflag=datas.minstallationflag,
                    mcreatedtime=DateTime.Now,
                };
                db.SaveCarts.Add(obj2);
                db.SaveChanges();
                if (datas.warrantyexist)
                {
                    var obj3 = new SaveCart
                    {
                        msupplierid = datas.wcart.msupplierid,
                        mprodid = datas.mprodid,
                        mprice = datas.wcart.mprice,
                        mprodvarid = obj2.Id,
                        mtermf = datas.wcart.mtermf,
                        mtermp = datas.wcart.mtermp,
                        mprodname = datas.wcart.mprodname,
                        mqty = 1,
                        mwarrantid=datas.subwarrantid,
                        mwarrantyflag=1,
                        Session_Id = sessionid
                    };
                    db.SaveCarts.Add(obj3);
                    db.SaveChanges();
                }
                var addondatas = db.productaddondetails.FirstOrDefault(x => x.mselecteprod == datas.mprodid);
                if (addondatas != null)
                {
                    d.Add("Successwithaddon");
                }
                else
                {
                    d.Add("Success");
                }
                
                d.Add(sessionid);
                new Thread(() =>
                {
                    TimeSpan span = DateTime.UtcNow.AddDays(365) - DateTime.UtcNow;
                    var jobid = obj2.Id+""+sessionid;
                    var manager = new BackgroundJobClient();
                    manager.Schedule(() => OldData(obj2.Id, jobid),span);
                }).Start();
            }
            else
            {
                d.Add("failed");
            }
            return Ok(d);
        }
        public static string OldData(int a, string jobid)
        {
            var db = new LeasingDbEntities();
            var searchsend = db.SaveCarts.FirstOrDefault(x => x.Id == a);
            if (searchsend != null)
            {
                db.Entry(searchsend).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
            }
            else
            {
                BackgroundJob.Delete(jobid);
            }
            // var pdfbinary= Convert.FromBase64String(data);
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");
            return "done";



        }
        [HttpPost]
        [Route("api/CookieApi/SaveSubCookieCart")]
        public IHttpActionResult SaveSubCookieCart(SaveSubCartViewModel datas)
        {
            var f = false;
            var db = new LeasingDbEntities();
            List<string> d = new List<string>();
            var datafound = db.SaveSubCarts.FirstOrDefault(x => x.mcompareprodid == datas.savecartid);
            if (datas.product.Any() && datafound==null)
            {
                foreach (var a in datas.product)
                {
                    var obj2 = new SaveSubCart
                    {
                        mcompareprodid = datas.savecartid,
                        msubquoteid = a.mprodvarid,
                        msupplierid = a.msupplierid
                    };
                    db.SaveSubCarts.Add(obj2);
                    db.SaveChanges();
                    f = true;
                }
            }
                if (f)
                {
                    d.Add("success");
                }else
                {
                    d.Add("failed");
                }
       
            return Ok(d);
        }
        [HttpPost]
        [Route("api/CookieApi/UpdateCookieCart")]
        public IHttpActionResult UpdateCookieCart(SaveCartViewModel datas)
        {
            var db = new LeasingDbEntities();
            List<string> d = new List<string>();

            if (datas.mprice != null)
            {
                //foreach(var datas in datas2)
                //{
                var sessionid = "";
                if (datas.Session_Id != "")
                {
                    sessionid = datas.Session_Id;
                    var datas2 = db.SaveCarts.FirstOrDefault(x => x.Session_Id == sessionid && x.mprodvarid == datas.mprodvarid && x.msupplierid == datas.msupplierid);
                    if (datas2 != null)
                    {

                        datas2.mqty = datas2.mqty + datas.mqty;
                        db.Entry(datas2).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        var datas3 = db.SaveCarts.FirstOrDefault(x => x.Session_Id == sessionid && x.mprodvarid == datas2.Id && x.msupplierid == datas.msupplierid && x.mwarrantyflag==1);
                        if (datas3 != null)
                        {
                            datas3.mqty = 1;
                            db.Entry(datas3).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        var addondatas = db.productaddondetails.FirstOrDefault(x => x.mselecteprod == datas.mprodid);
                        if (addondatas != null)
                        {
                            d.Add("Successwithaddon");
                        }
                        else
                        {
                            d.Add("Success");
                        }
                        //d.Add("Success");
                        d.Add(sessionid);
                    };



                }
            }
            else
            {
                d.Add("failed");
            }
            return Ok(d);
        }
        [HttpPost]
        [Route("api/CookieApi/UpdateCookieFromCart")]
        public IHttpActionResult UpdateCookieFromCart(SaveCartViewModel datas)
        {
            var db = new LeasingDbEntities();
            List<string> d = new List<string>();

            if (datas.mprice != null)
            {
                //foreach(var datas in datas2)
                //{
                var sessionid = "";
                if (datas.Session_Id != "")
                {
                    sessionid = datas.Session_Id;
                    var datas2 = db.SaveCarts.FirstOrDefault(x => x.Session_Id == sessionid && x.mprodvarid == datas.mprodvarid && x.msupplierid == datas.msupplierid);
                    if (datas2 != null)
                    {
                        datas2.mtermf = datas.mtermf;
                        datas2.mtermp = datas.mtermp;
                        datas2.mqty = datas.mqty;
                        datas2.mprice = datas.mprice;
                        datas2.moffer = datas.moffer;
                        var wdatas = db.SaveCarts.FirstOrDefault(x => x.mprodvarid == datas2.Id && x.mwarrantyflag == 1);
                        if (wdatas != null && datas.warrantyexist)
                        {
                            
                            wdatas.mtermp = datas.wcart.mtermp;
                            wdatas.mtermf = datas.wcart.mtermf;
                            wdatas.mprodname = datas.wcart.mprodname;
                            wdatas.mqty = datas.wcart.mqty;
                            wdatas.mwarrantid = datas.wcart.subwarrantid;
                            db.Entry(wdatas).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            if (wdatas != null && datas.wcart!=null)
                            {
                                //wdatas.mqty = datas.wcart.mqty;
                                db.Entry(wdatas).State = System.Data.Entity.EntityState.Modified;
                                db.SaveChanges();
                            }
                            
                        }
                        
                        db.Entry(datas2).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        d.Add("Success");
                        d.Add(sessionid);
                    };



                }
            }
            else
            {
                d.Add("failed");
            }
            return Ok(d);
        }
        
        [HttpPost]
        [Route("api/CookieApi/UpdateCartAddress")]
        public IHttpActionResult UpdateCartAddress(AgreementAddressViewModel data)
        {
            var db = new LeasingDbEntities();
            var subaddress = db.SubCartAddresses.FirstOrDefault(x => x.mcartid == data.id);
            if (subaddress != null)
            {
                subaddress.maddress = data.address;
                db.Entry(subaddress).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                
                var obj = new SubCartAddress
                {
                    mcartid = data.id,
                    maddress = data.address
                };
                db.SubCartAddresses.Add(obj);
                db.SaveChanges();
                var savecarts = db.SaveCarts.FirstOrDefault(x => x.Id == data.id);
                if (savecarts != null)
                {
                    savecarts.maddressflag = 1;
                    db.Entry(savecarts).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return Ok("success");
        }
        [HttpGet]
        [Route("api/CookieApi/RemoveCartAddress")]
        public IHttpActionResult RemoveCartAddress(int id)
        {
            var db = new LeasingDbEntities();
            var subaddress = db.SubCartAddresses.FirstOrDefault(x => x.mcartid == id);
            if (subaddress != null)
            {
                db.Entry(subaddress).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                var savecarts = db.SaveCarts.FirstOrDefault(x => x.Id == id);
                if (savecarts != null)
                {
                    savecarts.maddressflag = 0;
                    db.Entry(savecarts).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return Ok("success");
            }
            return Ok("failed");
        }
        //return Ok(datas2);
        //return Ok("failed");
        [HttpGet]
        [Route("api/CookieApi/GetCookieCart")]
        public IHttpActionResult GetCookieCart(string id)
        {
            var db = new LeasingDbEntities();
            List<SaveCartNewViewModel> d = null;
            List<SaveCartNewViewModel> inactived = null;
            if (id != null)
            {
                //foreach(var datas in datas2)
                //{
                d = (from ab in db.SaveCarts
                     join prodsub in db.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                     join prodsupp in db.ProductSubSuppliers on prodsub.mprodsubid equals prodsupp.ProdSubId
                     join p in db.products on prodsub.mprodid equals p.mprodid
                     join users in db.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                     from users in usersdata.DefaultIfEmpty()
                     where ab.Session_Id == id && users.mstatus=="active" && ab.mwarrantyflag == 0
                     select new SaveCartNewViewModel
                     {
                         Id = ab.Id,
                         Session_Id = ab.Session_Id,
                         Product = new CartProductViewModel
                         {

                             mprodid = ab.mprodid,
                             mprice = prodsupp.mrp,
                             mproducturl = ab.mproducturl,
                             mprodvarid = ab.mprodvarid,
                             msubpicname = ab.msubpicname,
                             msupplierid = ab.msupplierid,
                             mtermf = ab.mtermf,
                             mtermp = ab.mtermp,
                             mprodname = ab.mprodname,
                             moffer=ab.moffer,
                             minstallationflag=ab.minstallationflag==1?"yes":"no",
                             suppliername=p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                         },
                         Quantity = ab.mqty
                     }).ToList();
                     inactived = (from ab in db.SaveCarts
                     join prodsub in db.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                                  join p in db.products on prodsub.mprodid equals p.mprodid
                                  join users in db.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                     from users in usersdata.DefaultIfEmpty()
                     where ab.Session_Id == id && users.mstatus!="active" && ab.mwarrantyflag == 0
                             select new SaveCartNewViewModel
                     {
                         Id = ab.Id,
                         Session_Id = ab.Session_Id,
                         Product = new CartProductViewModel
                         {

                             mprodid = ab.mprodid,
                             mprice = ab.mprice,
                             mproducturl = ab.mproducturl,
                             mprodvarid = ab.mprodvarid,
                             msubpicname = ab.msubpicname,
                             msupplierid = ab.msupplierid,
                             mtermf = ab.mtermf,
                             mtermp = ab.mtermp,
                             mprodname = ab.mprodname,
                             moffer = ab.moffer,
                             minstallationflag = ab.minstallationflag == 1 ? "yes" : "no",
                             suppliername = p.mmanufacturer,//users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                         },
                         Quantity = ab.mqty
                     }).ToList();
                if (d.Count == 0 && inactived.Count==0)
                {
                    return Ok("empty");
                }
                if (d.Count != 0)
                {
                    d.ForEach(x =>
                    {

                        var vat = db.featuremasters.FirstOrDefault(z => z.featurename == "vat");
                        x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                        x.Product.mbaseprice = getbaseprice(x.Product.mprodvarid, x.Product.msupplierid, db);
                        x.Product.mleadtime = GetLeadTime(x.Product.mprodvarid, x.Product.msupplierid);
                        x.Product.warrantydetails = getWarranty(x.Id,x.Product.mprodname, db);
                        x.Product.maxwarrantyflag=x.Product.warrantydetails!=null?(x.Product.warrantydetails.mtermp<(x.Product.mtermp/12)?checkmaxWarranty(x.Product.warrantydetails.mtermp,x.Product.mprodid,db):false):false;
                        if (x.Product.moffer > 0)
                        {
                            var pwdata = db.productweekdeals.FirstOrDefault(z => z.mprodid == x.Product.mprodid);
                            if (pwdata != null)
                            {

                                x.Product.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.Product.mbaseprice.HasValue ? x.Product.mbaseprice.Value : 0;
                                x.Product.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                                x.Product.mofferdetail = pwdata.mofferdetail;
                            }
                        }

                    });

                }
                if (inactived.Count != 0)
                {
                    inactived.ForEach(x =>
                    {

                        var vat = db.featuremasters.FirstOrDefault(z => z.featurename == "vat");
                        x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                        x.Product.mbaseprice = getbaseprice(x.Product.mprodvarid, x.Product.msupplierid, db);
                        x.Product.mleadtime = GetLeadTime(x.Product.mprodvarid, x.Product.msupplierid);
                        x.Product.ProductOffer = getNumberOfOffer(x.Product.mprodvarid, x.Product.msupplierid,db);
                        x.Product.warrantydetails = getWarranty(x.Id,x.Product.mprodname, db);

                        if (x.Product.moffer > 0)
                        {
                            var pwdata = db.productweekdeals.FirstOrDefault(z => z.mprodid == x.Product.mprodid);
                            if (pwdata != null)
                            {

                                x.Product.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.Product.mbaseprice.HasValue ? x.Product.mbaseprice.Value : 0;
                                x.Product.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                                x.Product.mofferdetail = pwdata.mofferdetail;
                            }
                        }

                    });

                }
                return Json(new { data = d, inactivedata = inactived });
            }
            return Ok("failed");

        }

        
        public static double getbaseprice(int? provarid,int? supplierid,LeasingDbEntities db)
        {
            double baseprice = 0;
            
            if(provarid!=0 && provarid != null)
            {
                var products = (from pcv in db.productsubs
                                join p in db.products on pcv.mprodid equals p.mprodid
                                join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                                join psupp in db.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                                from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                                where pcv.mprodsubid == provarid && psupp.SupplierId == supplierid
                                select new {
                                    mrp=(pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                }).FirstOrDefault();
                if (products != null)
                {
                    baseprice = products.mrp;
                }
                
            }
            return baseprice;
        }
        public static WarrantyCartViewModel getWarranty(int cartid,string mprodname, LeasingDbEntities db)
        {
            var p = new WarrantyCartViewModel();
            p=(from pcv in db.SaveCarts
             join prodws in db.productsubwarrantybysuppliers on pcv.mwarrantid equals prodws.subwarrantid
             join prodw in db.productwarrantybysuppliers on prodws.warrantyid equals prodw.WarrantyId
             where pcv.mprodvarid==cartid && pcv.mwarrantyflag==1
             select new WarrantyCartViewModel
             {
                 id=pcv.Id,
                 mprodname=pcv.mprodname,
                 mtermp=prodws.warrantyterm,
                 mprice=prodws.warrantyprice,
                 subwarrantid=prodws.subwarrantid,
                 mtermf=pcv.mtermf,
                 mqty=pcv.mqty,
                 mbaseprice=prodws.warrantyprice,
             }).FirstOrDefault();

            return p;
        }
        public static bool checkmaxWarranty(int? warrantterm, int? prodid,LeasingDbEntities db)
        {
            //var p = new WarrantyCartViewModel();
            var p = (from pcv in db.products
                 where pcv.mprodid == prodid
                 select new 
                 {
                     mprodw1=pcv.msupplierwarranty.HasValue?pcv.msupplierwarranty.Value:0,
                     mprodw2 = pcv.msupplierwarranty2.HasValue ? pcv.msupplierwarranty2.Value : 0,
                     mprodw3 = pcv.msupplierwarranty3.HasValue ? pcv.msupplierwarranty3.Value : 0,
                     mprodw4 = pcv.msupplierwarranty4.HasValue ? pcv.msupplierwarranty4.Value : 0,
                     mprodw5 = pcv.msupplierwarranty5.HasValue ? pcv.msupplierwarranty5.Value : 0,

                 }).FirstOrDefault();
            if (warrantterm == 1 && (p.mprodw2 > 0 || p.mprodw3 > 0 || p.mprodw4 > 0 || p.mprodw5 > 0))
                return true;
            else if (warrantterm == 2 && (p.mprodw3 > 0 || p.mprodw4 > 0 || p.mprodw5 > 0))
                return true;
            else if (warrantterm == 3 && (p.mprodw4 > 0 || p.mprodw5 > 0))
                return true;
            else if (warrantterm == 4 && (p.mprodw5 > 0))
                return true;
            else
                return false;
            
        }
        public static int getNumberOfOffer(int? prodid, int? msupplierid, LeasingDbEntities db)
        {


            var ctx = db;
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
        [HttpGet]
        [Route("api/CookieApi/GetCookieCartOrder")]
        public IHttpActionResult GetCookieCartOrder(string id,int mschoolid)
        {

            var seed = string.Empty;
            List<CartOrderViewModel> products = null;
            List<CartOrderViewModel> inactiveproducts = null;
            var ctx = new LeasingDbEntities();
            products = (from ab in ctx.SaveCarts
                        join prodsub in ctx.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                        join p in ctx.products on prodsub.mprodid equals p.mprodid
                        join subcart in ctx.SubCartAddresses on ab.Id equals subcart.mcartid into addressdata
                        from subcart in addressdata.DefaultIfEmpty()
                        join users in ctx.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                        from users in usersdata.DefaultIfEmpty()
                        where ab.Session_Id == id && users.mstatus == "active" && ab.mwarrantyflag == 0
                        select new CartOrderViewModel()
                        {
                            mprodid = ab.mprodid,
                            msubquoteid = ab.Id,
                            mproductname = ab.mprodname,
                            mprice = ab.mprice,
                            mtermf = ab.mtermf,
                            mtermp = ab.mtermp,
                            quantity = ab.mqty,
                            mprodvarid = ab.mprodvarid,
                            msupplierid = ab.msupplierid,
                            suppliername =p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                            maddressflag = ab.maddressflag,
                            Address = subcart.maddress,
                            moffer = ab.moffer,
                            minstallationflag=ab.minstallationflag==1?"yes":"no",
                        }).OrderByDescending(x => x.msubquoteid).ToList();




            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                var vat = ctx.featuremasters.FirstOrDefault(z => z.featurename == "vat");

                x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                x.mflexible = GetPreviousPayment(mschoolid);
                x.mleadtime = GetLeadTime(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + x.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
                if (x.moffer > 0)
                {
                    var pwdata = ctx.productweekdeals.FirstOrDefault(z => z.mprodid == x.mprodid);
                    if (pwdata != null)
                    {

                        x.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.mprice.HasValue ? x.mprice.Value : 0;
                        x.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                        x.mofferdetail = pwdata.mofferdetail;
                    }
                }
                if (x.maddressflag == 1)
                {

                    x.AddressDetail = getDifferentAddress(int.Parse(x.Address));
                }
            });
            inactiveproducts = (from ab in ctx.SaveCarts
                                join prodsub in ctx.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                                join p in ctx.products on prodsub.mprodid equals p.mprodid
                                join subcart in ctx.SubCartAddresses on ab.Id equals subcart.mcartid into addressdata
                                from subcart in addressdata.DefaultIfEmpty()
                                join users in ctx.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                                from users in usersdata.DefaultIfEmpty()
                                where ab.Session_Id == id && users.mstatus != "active" && ab.mwarrantyflag == 0
                                select new CartOrderViewModel()
                                {
                                    mprodid = ab.mprodid,
                                    msubquoteid = ab.Id,
                                    mproductname = ab.mprodname,
                                    mprice = ab.mprice,
                                    mtermf = ab.mtermf,
                                    mtermp = ab.mtermp,
                                    quantity = ab.mqty,
                                    mprodvarid = ab.mprodvarid,
                                    msupplierid = ab.msupplierid,
                                    suppliername =p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                                    maddressflag = ab.maddressflag,
                                    Address = subcart.maddress,
                                    moffer = ab.moffer,
                                    minstallationflag=ab.minstallationflag==1?"yes":"no"
                                }).OrderByDescending(x => x.msubquoteid).ToList();
            if (inactiveproducts.Count > 0)
            {
                inactiveproducts.ForEach(x =>
                {
                    var vat = ctx.featuremasters.FirstOrDefault(z => z.featurename == "vat");

                    x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                    x.mflexible = GetPreviousPayment(mschoolid);
                    x.mleadtime = GetLeadTime(x.mprodvarid, x.msupplierid);
                    x.mproducturl = "productname=" + x.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                    x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                    x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
                    if (x.moffer > 0)
                    {
                        var pwdata = ctx.productweekdeals.FirstOrDefault(z => z.mprodid == x.mprodid);
                        if (pwdata != null)
                        {

                            x.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.mprice.HasValue ? x.mprice.Value : 0;
                            x.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                            x.mofferdetail = pwdata.mofferdetail;
                        }
                    }
                    if (x.maddressflag == 1)
                    {

                        x.AddressDetail = getDifferentAddress(int.Parse(x.Address));
                    }
                });
            }
            return Json(new { data = products, inactiveproduct = inactiveproducts });
        }
        [HttpGet]
        [Route("api/CookieApi/GetCookieCartOrder2")]
        public IHttpActionResult GetCookieCartOrder2(string id, int mschoolid)
        {
            var db = new LeasingDbEntities();
            var seed = string.Empty;
            List<CartOrderViewModel> products = null;
            List<CartOrderViewModel> products2 = null;
            List<CartOrderViewModel> inactiveproducts = null;
            var ctx = new LeasingDbEntities();
            products = (from ab in ctx.SaveCarts
                        join prodsub in ctx.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                        join p in ctx.products on prodsub.mprodid equals p.mprodid
                        join subcart in ctx.SubCartAddresses on ab.Id equals subcart.mcartid into addressdata
                        from subcart in addressdata.DefaultIfEmpty()
                        join users in ctx.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                        from users in usersdata.DefaultIfEmpty()
                        where ab.Session_Id == id && users.mstatus == "active" && ab.mwarrantyflag==0
                        select new CartOrderViewModel()
                        {
                            
                            mprodid = ab.mprodid,
                            msubquoteid = ab.Id,
                            mproductname = ab.mprodname,
                            mprice = ab.mprice,
                            mtermf = ab.mtermf,
                            mtermp = ab.mtermp,
                            quantity = ab.mqty,
                            mprodvarid = ab.mprodvarid,
                            msupplierid = ab.msupplierid,
                            suppliername =p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                            maddressflag = ab.maddressflag,
                            Address = subcart.maddress,
                            moffer = ab.moffer,
                            mwarrantyflag=0,
                            minstallationflag = ab.minstallationflag == 1 ? "yes" : "no"
                        }).OrderByDescending(x => x.msubquoteid).ToList();




            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                var vat = ctx.featuremasters.FirstOrDefault(z => z.featurename == "vat");

                x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                x.mflexible = GetPreviousPayment(mschoolid);
                x.mleadtime = GetLeadTime(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + x.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
                x.warrantydetails = getWarranty(x.msubquoteid, x.mproductname, db);
                if (x.moffer > 0)
                {
                    var pwdata = ctx.productweekdeals.FirstOrDefault(z => z.mprodid == x.mprodid);
                    if (pwdata != null)
                    {

                        x.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.mprice.HasValue ? x.mprice.Value : 0;
                        x.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                        x.mofferdetail = pwdata.mofferdetail;
                    }
                }
                if (x.maddressflag == 1)
                {

                    x.AddressDetail = getDifferentAddress(int.Parse(x.Address));
                }
            });
            products2 = products.ToList();
            /*products.ForEach(x =>
            {
                var product3 = (from ab in ctx.SaveCarts
                            where ab.mprodvarid == x.msubquoteid && ab.mwarrantyflag==1
                            select new CartOrderViewModel()
                            {
                                mprodid = ab.mprodid,
                                msubquoteid = ab.Id,
                                mproductname = ab.mprodname,
                                mprice = ab.mprice,
                                mtermf = ab.mtermf,
                                mtermp = ab.mtermp,
                                quantity = ab.mqty,
                                mprodvarid = ab.mprodvarid,
                                msupplierid = ab.msupplierid,
                                moffer = ab.moffer,
                                mwarrantyflag=1,
                            }).OrderByDescending(z => z.msubquoteid).FirstOrDefault();
                if (product3 != null)
                {
                    products2.Add(product3);
                }
            });*/

            inactiveproducts = (from ab in ctx.SaveCarts
                                join prodsub in ctx.productsubs on ab.mprodvarid equals prodsub.mprodsubid
                                join p in ctx.products on prodsub.mprodid equals p.mprodid
                                join subcart in ctx.SubCartAddresses on ab.Id equals subcart.mcartid into addressdata
                                from subcart in addressdata.DefaultIfEmpty()
                                join users in ctx.supplier_registration on ab.msupplierid equals users.msuid into usersdata
                                from users in usersdata.DefaultIfEmpty()
                                where ab.Session_Id == id && users.mstatus != "active"
                                select new CartOrderViewModel()
                                {
                                    mprodid = ab.mprodid,
                                    msubquoteid = ab.Id,
                                    mproductname = ab.mprodname,
                                    mprice = ab.mprice,
                                    mtermf = ab.mtermf,
                                    mtermp = ab.mtermp,
                                    quantity = ab.mqty,
                                    mprodvarid = ab.mprodvarid,
                                    msupplierid = ab.msupplierid,
                                    suppliername =p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                                    maddressflag = ab.maddressflag,
                                    Address = subcart.maddress,
                                    moffer = ab.moffer,
                                    minstallationflag = ab.minstallationflag == 1 ? "yes" : "no"
                                }).OrderByDescending(x => x.msubquoteid).ToList();
            if (inactiveproducts.Count > 0)
            {
                inactiveproducts.ForEach(x =>
                {
                    var vat = ctx.featuremasters.FirstOrDefault(z => z.featurename == "vat");

                    x.mvat = (vat != null ? vat.featureval.HasValue ? vat.featureval.Value : 0 : 0);
                    x.mflexible = GetPreviousPayment(mschoolid);
                    x.mleadtime = GetLeadTime(x.mprodvarid, x.msupplierid);
                    x.mproducturl = "productname=" + x.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                    x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                    x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
                    if (x.moffer > 0)
                    {
                        var pwdata = ctx.productweekdeals.FirstOrDefault(z => z.mprodid == x.mprodid);
                        if (pwdata != null)
                        {

                            x.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : x.mprice.HasValue ? x.mprice.Value : 0;
                            x.mofferqty = pwdata.mqty.HasValue ? pwdata.mqty.Value : 0;
                            x.mofferdetail = pwdata.mofferdetail;
                        }
                    }
                    if (x.maddressflag == 1)
                    {

                        x.AddressDetail = getDifferentAddress(int.Parse(x.Address));
                    }
                });
            }
            return Json(new { data = products2, inactiveproduct = inactiveproducts });

        }
        public string getDefaultAddress(int mschoolid)
        {
            var db = new LeasingDbEntities();
            var addr = "";
            var maddress = db.user_registration_addr.FirstOrDefault(x => x.muid == mschoolid);
            if (maddress != null)
            {
                addr = maddress.maddress1;
                if (maddress.maddress2 != null)
                {
                    addr = addr + "<br/>" + maddress.maddress2;
                }
                if (maddress.mlandmark != null)
                {
                    addr = addr + "<br/>" + maddress.mlandmark;

                }
                addr = addr + "<br/>" + maddress.mcity+(maddress.mpostcode!=null?" - "+maddress.mpostcode:"");
                return addr;
            }
            return addr;
        }
        public string getDifferentAddress(int museraddrid)
        {
            var db = new LeasingDbEntities();
            var addr = "";
            var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == museraddrid);
            if (maddress != null)
            {
                addr = maddress.maddress1;
                if (maddress.maddress2 != null)
                {
                    addr = addr + "<br/>" + maddress.maddress2;
                }
                if (maddress.mlandmark != null)
                {
                    addr = addr + "<br/>" + maddress.mlandmark;

                }
                addr = addr + "<br/>" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                return addr;
            }
            return "";
        }
        public string getDifferentAddressOld(int mcartid)
        {
            var db = new LeasingDbEntities();
            var maddress = db.SubCartAddresses.FirstOrDefault(x => x.mcartid ==mcartid);
            if (maddress != null)
            {
                return maddress.maddress;
            }
            return "";
        }
        public byte GetPreviousPayment(int mschoolid)
        {
            var db = new LeasingDbEntities();
            var mpayment = db.userleaseagreements.OrderByDescending(x=>x.mleaseid).FirstOrDefault(x => x.muserid == mschoolid && x.mpayment > 0);
            if (mpayment != null)
            {
                return (byte)mpayment.mpayment;
            }
            return 0;
        }
        public int GetLeadTime(int? mprodvarid, int? msupplierid)
        {
            var db = new LeasingDbEntities();
            int leadtime = 0;
            var prodsub = (from pcv in db.productsubs
                           join psupp in db.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                           from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                           where pcv.mprodsubid == mprodvarid && psupp.SupplierId == msupplierid
                           select new
                           {
                               psupp.mleadtime
                           }).FirstOrDefault();
            if (prodsub != null)
            {
                leadtime = prodsub.mleadtime.HasValue ? prodsub.mleadtime.Value : 3;
            }
            return leadtime;
        }
        /*[HttpGet]
        [Route("api/CookieApi/GetSubCart")]
        public IHttpActionResult GetSubCart(string id)
        {
            var ctx = new LeasingDbEntities();

            var d = new List<SubCartDisplayViewModel>();
            if (id != null)
            {
                var data = ctx.SaveCarts.Where(x => x.Session_Id == id).ToList();
                if (data.Any())
                {
                    foreach (var datas in data)
                    {

                        var d2 = (from ab in ctx.SaveSubCarts
                                  join pcv in ctx.productsubs on ab.msubquoteid equals pcv.mprodsubid
                                  join p in ctx.products on pcv.mprodid equals p.mprodid
                                  join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                  join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                  join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                                  join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                                  from users in userdata.DefaultIfEmpty()
                                  let msupplierid=psupp.SupplierId.HasValue?psupp.SupplierId.Value:0
                                  let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * datas.mtermf) / (datas.mtermp * 0.9) : 0
                                  where ab.mcompareprodid == datas.Id && ab.msupplierid == msupplierid && pcv.mprodsubid == ab.msubquoteid
                                  select new SubCartDisplayViewModel()
                                  {
                                      Id=ab.Id,
                                      mprodid = datas.Id,

                                      mprodname = p.mprodname,
                                      mtermf = datas.mtermf.HasValue ? datas.mtermf.Value : 1,
                                      mtermp = datas.mtermp.HasValue ? datas.mtermp.Value : 60,
                                      suppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                                      //msubpicname = y.Key.pic,
                                      mprice = mprice.HasValue ? mprice.Value : 0,
                                      mbasicprice = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                                      mprodvarid = pcv.mprodsubid,
                                      msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                                  }).Take(2).ToList<SubCartDisplayViewModel>();
                        if (d2.Any() && d2.Count > 0)
                        {
                            d2.ForEach(x => d.Add(x));
                            
                        }
                        if (d2.Count < 2)
                        {
                            var d3 = (from ab in ctx.SaveSubCarts
                                      join pcv in ctx.productsubs on ab.msubquoteid equals pcv.mprodsubid
                                      join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                                      join p in ctx.products on pcv.mprodid equals p.mprodid
                                      join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                      join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                      
                                      join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                                      from users in userdata.DefaultIfEmpty()
                                      let supplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0
                                      let mprice = psub.mofferprice.HasValue ? (psub.mofferprice.Value * datas.mtermf) / (datas.mtermp * 0.9) : 0
                                      where ab.mcompareprodid == datas.Id && pcv.mprodsubid == ab.msubquoteid && pcv.moresupplier == 1 && supplierid == ab.msupplierid
                                      select new SubCartDisplayViewModel()
                                      {
                                          Id = ab.Id,
                                          mprodid = datas.Id,
                                          mprodname = p.mprodname,
                                          mtermf = datas.mtermf.HasValue ? datas.mtermf.Value : 1,
                                          mtermp = datas.mtermp.HasValue ? datas.mtermp.Value : 60,
                                          suppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                                          //msubpicname = y.Key.pic,
                                          mprice = mprice.HasValue ? mprice.Value : 0,
                                          mbasicprice = psub.mofferprice.HasValue ? psub.mofferprice.Value : 0,
                                          mprodvarid = pcv.mprodsubid,
                                          msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                                      }).Take(2).ToList<SubCartDisplayViewModel>();
                            if (d3.Count > 0)
                            {
                                d3.ForEach(x => d.Add(x));

                            }

                        }



                    }

                }
                //foreach(var datas in datas2)
                //{
                d.ForEach(x =>
                {
                    x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                    x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
                });

                return Ok(d.ToList());
            }
            return Ok("failed");

        }*/
        [HttpGet]
        [Route("api/CookieApi/GetCartDelete")]
        public IHttpActionResult GetCartDelete(int id)
        {
            var db = new LeasingDbEntities();
            List<SaveCartNewViewModel> d = null;

            if (id != 0)
            {
                //foreach(var datas in datas2)
                //{
                var search = db.SaveCarts.FirstOrDefault(x => x.Id == id);
                if (search != null)
                {
                    var searchsub = db.SaveSubCarts.Where(x => x.mcompareprodid == id);
                    if (searchsub.Any())
                    {
                        db.SaveSubCarts.RemoveRange(searchsub);
                        db.SaveChanges();
                    }
                    var searchsub2 = db.SaveCarts.Where(x => x.mprodvarid == id && x.mwarrantyflag==1);
                    if (searchsub2.Any())
                    {
                        db.SaveCarts.RemoveRange(searchsub2);
                        db.SaveChanges();
                    }
                    db.Entry(search).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return Ok("delete");
            }
            return Ok("failed");

        }
        /*
        [HttpGet]
        [Route("api/CookieApi/GetSubCartDelete")]
        public IHttpActionResult GetSubCartDelete(int id)
        {
            var db = new LeasingDbEntities();
            List<SaveCartNewViewModel> d = null;

            if (id != 0)
            {
                //foreach(var datas in datas2)
                //{
                 var searchsub = db.SaveSubCarts.FirstOrDefault(x => x.Id == id);
                if (searchsub != null)
                {


                    db.Entry(searchsub).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return Ok("delete");
            }
                
            
            return Ok("failed");

        }
        */
        [HttpGet]
        [Route("api/CookieApi/CartEmpty")]
        public IHttpActionResult CartEmpty(string id)
        {
            var db = new LeasingDbEntities();
            List<SaveCartNewViewModel> d = null;

            if (id != null)
            {
                //foreach(var datas in datas2)
                //{
                var search = db.SaveCarts.Where(x => x.Session_Id == id);
                if (search.Any())
                {
                    db.SaveCarts.RemoveRange(search);
                    //db.Entry(search).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return Ok("delete");
            }
            return Ok("failed");

        }
        [HttpPost]
        public IHttpActionResult SaveCart2(int datas2)
        {
            var db = new LeasingDbEntities();
            List<string> d = new List<string>();

            /* if (datas2.Any())
             {
                 foreach(var datas in datas2)
                 {
                     var sessionid = System.Web.HttpContext.Current.Session.SessionID;
                     sessionid = sessionid + "-" + (DateTime.Now.Ticks);
                     var obj2 = new SaveCart
                     {
                         mprodid = datas.mprodid,
                         mprice = datas.mprice,
                         mproducturl = datas.mproducturl,
                         mprodvarid = datas.mprodvarid,
                         msubpicname = datas.msubpicname,
                         msupplierid = datas.msupplierid,
                         mtermf = datas.mtermf,
                         mtermp = datas.mtermp,
                         Session_Id = sessionid
                     };
                     db.SaveCarts.Add(obj2);
                     db.SaveChanges();
                     d.Add("Success");
                     d.Add(sessionid);

                 }
                 return Ok(d);
             }*/
            return Ok(datas2);
            return Ok("failed");
        }
    }
}
