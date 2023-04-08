using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;
using System.Web;

namespace LeasingPortalApi.Controllers.Api
{
    public class ProductApiControllerold : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetAllProductById(int getid, int row, int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();

            products = (from p in ctx.products
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                        from pcv in pcvdata.DefaultIfEmpty()
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                        from psupp in psuppdata.OrderByDescending(x=>x.OfferPrice).DefaultIfEmpty()
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.mprodcatid == id && p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.Take(rowperpage).ToList().ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }
        [Route("api/productapi/GetCategoryProductById")]
        [HttpGet]
        public IHttpActionResult GetCategoryProductById(int getid, int row, int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();

            products = (from pc in ctx.productcats
                        join p in ctx.products on pc.mprodcatid equals p.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                        from pcv in pcvdata.DefaultIfEmpty()
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                        from psupp in psuppdata.OrderByDescending(x => x.OfferPrice).DefaultIfEmpty()
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where pc.mpreviouscat==id && p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        public static int getNumberOfOffer(int prodid,int msupplierid)
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


        [HttpGet]
        [Route("Api/ProductApi/OldGetAllProductById")]
        public IHttpActionResult OldGetAllProductById(int getid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid into pcv2data
                            from pcv2 in pcv2data.DefaultIfEmpty()
                            join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                            from psupp in psuppdata.OrderByDescending(x => x.OfferPrice).DefaultIfEmpty()
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where p.mprodcatid == id && p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0



                            group new
                            {
                                pcv2.mprodvariationvalue,
                                pcv.mrp,
                                pcv.mofferprice,
                                pcv.msupplierid,
                                users.morgname,
                                users.mtown,
                                users.mcity
                            }
                            by new
                            {

                                pcv.mprodsubid,
                                p.mprodid,
                                p.mprodname,
                                pbrand.mbrandid,
                                pbrand.mbrandname,
                                pc.mcatname,

                            } into y
                            orderby y.Key.mprodid
                            select new ProductDisplayViewModel()
                            {
                                mprodid = y.Key.mprodid,

                                mprodname = y.Key.mprodname,
                                mprodcatname = y.Key.mcatname,
                                mbrandid = y.Key.mbrandid,
                                mbrand = y.Key.mbrandname,
                                mtermf = 1,
                                mtermp = 60,
                                msuppliername = y.FirstOrDefault().morgname + (y.FirstOrDefault().mtown != null ? ", " + y.FirstOrDefault().mtown : "") + (y.FirstOrDefault().mcity != null ? " - " + y.FirstOrDefault().mcity : ""),
                                //msubpicname = y.Key.pic,
                                mprice = y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                                mbasicprice = y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                                mprodvarid = y.Key.mprodsubid,
                                msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                                productsubvariationtwo = y.Select(x => new ProductSubVariationTwoViewModel
                                {
                                    mprodvariationvalue = x.mprodvariationvalue,
                                }).AsQueryable()
                            }).ToList<ProductDisplayViewModel>();


            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (x.productsubvariationtwo.AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}"));
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/ProductApi/GetRelatedProductById")]
        public IHttpActionResult GetRelatedProductById(int getid,int exceptid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                            from psupp in psuppdata.OrderByDescending(x => x.OfferPrice).DefaultIfEmpty()
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where p.mprodcatid == id && pcv.mprodsubid!=exceptid && p.approve==1 && p.mactive=="yes" && pcv.mrp > 0
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
                                mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            }).ToList<ProductDisplayViewModel>();


            if (products.Count == 0)
            {
                return NotFound();
            }
            products = products.Take(5).ToList();
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }


        [HttpGet]
        [Route("api/ProductApi/GetMoreProductById")]
        public IHttpActionResult GetMoreProductById(int getid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                            from psupp in psuppdata.OrderByDescending(x => x.OfferPrice).DefaultIfEmpty()
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where p.mprodid == id && p.approve==1 && p.mactive=="yes" && pcv.mrp > 0
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
                                mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            }).ToList<ProductDisplayViewModel>();


            
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }
        [HttpPost]
        [Route("api/ProductApi/GetOfferProduct")]
        public IHttpActionResult GetOfferProduct(CartProductViewModel item)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = 0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pcv in ctx.productsubs
                        join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psub.OfferPrice.HasValue ? (psub.OfferPrice.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where pcv.mprodsubid == item.mprodvarid && pcv.moresupplier == 1 && psub.SupplierId != pcv.msupplierid && psub.OfferPrice > 0
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            
                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = mprice.HasValue ? mprice.Value : 0,
                            mbasicprice = psub.OfferPrice.HasValue ? psub.OfferPrice.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).ToList<ProductDisplayViewModel>();
            if (products.Count <= 5)
            {
                var pcat = (from pcv in ctx.productsubs
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            where pcv.mprodsubid == item.mprodvarid
                            select new
                            {
                                catid = p.mprodcatid
                            }).FirstOrDefault().catid;
                if (pcat > 0)
                {
                    var d = (from p in ctx.products
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                from pcv in pcvdata.DefaultIfEmpty()
                                join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                                from users in userdata.DefaultIfEmpty()
                             let mprice = pcv.mrp.HasValue ? (pcv.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                             where p.mprodcatid == pcat && pcv.mprodsubid != item.mprodvarid && p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0
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
                                    mprice = mprice.HasValue ? mprice.Value : 0,
                                    mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                    mprodvarid = pcv.mprodsubid,
                                    msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                                    mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                                }).Take(5).ToList<ProductDisplayViewModel>();
                    if (d.Count > 0)
                    {
                        if (products.Count > 0)
                        {
                            d.ForEach(x => products.Add(x));
                            
                        }
                        else
                        {
                            products=d;
                        }

                    }
                }
                
            }
            if (products.Count < 0)
            {
                return Ok("empty");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            return Ok(products.Take(5).ToList());
        }
        [HttpPost]
        [Route("api/ProductApi/GetOfferSameProduct")]
        public IHttpActionResult GetOfferSameProduct(CartProductViewModel item)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = 0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pcv in ctx.productsubs
                        join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psub.OfferPrice.HasValue ? (psub.OfferPrice.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where pcv.mprodsubid == item.mprodvarid && pcv.moresupplier == 1 && psub.SupplierId != pcv.msupplierid && psub.OfferPrice > 0
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = mprice.HasValue ? mprice.Value : 0,
                            mbasicprice = psub.OfferPrice.HasValue ? psub.OfferPrice.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).ToList<ProductDisplayViewModel>();
            
            if (products.Count < 0)
            {
                return Ok("empty");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            return Ok(products.OrderBy(x=>x.mprice).Take(5).ToList());
        }
        [HttpPost]
        [Route("api/ProductApi/GetCompareProduct")]
        public IHttpActionResult GetCompareProduct(CartProductViewModel item)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id =0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pcv in ctx.productsubs
                      join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                      join p in ctx.products on pcv.mprodid equals p.mprodid
                      join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                      join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                      join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                      from users in userdata.DefaultIfEmpty()
                      let mprice= psub.OfferPrice.HasValue?(psub.OfferPrice.Value * item.mtermf) / (item.mtermp * 0.9):0
                      where pcv.mprodsubid == item.mprodvarid && pcv.moresupplier==1 && psub.SupplierId!=pcv.msupplierid && mprice>item.mprice
                      select new ProductDisplayViewModel()
                      {
                          mprodid = p.mprodid,

                          mprodname = p.mprodname,
                          mprodcatname = pc.mcatname,
                          mbrandid = pbrand.mbrandid,
                          mbrand = pbrand.mbrandname,
                          mtermf = item.mtermf.HasValue?item.mtermf.Value:1,
                          mtermp = item.mtermp.HasValue?item.mtermp.Value:60,
                          msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                          //msubpicname = y.Key.pic,
                          mprice = mprice.HasValue ? mprice.Value: 0,
                          mbasicprice = psub.OfferPrice.HasValue ? psub.OfferPrice.Value : 0,
                          mprodvarid = pcv.mprodsubid,
                          msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                      }).ToList<ProductDisplayViewModel>();
            if(products.Count == 0 || products.Count==1)
            {
                var category = (from pcv in ctx.productsubs
                                join p in ctx.products on pcv.mprodid equals p.mprodid
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                where pcv.mprodsubid == item.mprodvarid
                                select new
                                {
                                    category=pc.mprodcatid,
                                    brand=pbrand.mbrandid,
                                }).FirstOrDefault();
                var d=(from pcv in ctx.productsubs
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            let mprice = pcv.mofferprice.HasValue ? (pcv.mofferprice.Value * item.mtermf) / (item.mtermp * 0.9) : pcv.mrp.HasValue ? (pcv.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                            where pcv.mprodsubid != item.mprodvarid && (pcv.msupplierid!=item.msupplierid) && p.mprodcatid==category.category && mprice>item.mprice
                            select new ProductDisplayViewModel()
                            {
                                mprodid = p.mprodid,

                                mprodname = p.mprodname,
                                mprodcatname = pc.mcatname,
                                mbrandid = pbrand.mbrandid,
                                mbrand = pbrand.mbrandname,
                                mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                                mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                                msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                                //msubpicname = y.Key.pic,
                                mprice = mprice.HasValue ? mprice.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            }).ToList<ProductDisplayViewModel>();
                if (d.Count > 0)
                {
                    d = d.Take(2).ToList();
                    if (products.Count == 1)
                    {
                        d.ForEach(x => products.Add(x));
                    }
                    else
                    {
                        products = d;
                    }
                }
                if (products.Count <= 1)
                {
                    d = (from pcv in ctx.productsubs
                             join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                             join p in ctx.products on pcv.mprodid equals p.mprodid
                             join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                             join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                             join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                             from users in userdata.DefaultIfEmpty()
                             let mprice = (psub.OfferPrice.Value * item.mtermf) / (item.mtermp * 0.9)
                             where pcv.mprodsubid != item.mprodvarid && ( psub.SupplierId != item.msupplierid) && p.mprodcatid == category.category && mprice>item.mprice
                             select new ProductDisplayViewModel()
                             {
                                 mprodid = p.mprodid,

                                 mprodname = p.mprodname,
                                 mprodcatname = pc.mcatname,
                                 mbrandid = pbrand.mbrandid,
                                 mbrand = pbrand.mbrandname,
                                 mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                                 mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                                 msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                                 //msubpicname = y.Key.pic,
                                 mprice = mprice.HasValue ? mprice.Value : 0,
                                 mbasicprice = psub.OfferPrice.HasValue ? psub.OfferPrice.Value : 0,
                                 mprodvarid = pcv.mprodsubid,
                                 msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                             }).ToList<ProductDisplayViewModel>();

                }
                if (d.Count > 0)
                {
                    d = d.Take(2).ToList();
                    if (products.Count == 1)
                    {
                        d.ForEach(x => products.Add(x));
                    }
                    else
                    {
                        products = d;
                    }
                }

            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products.Take(2).ToList());
        }
        [HttpGet]
        [Route("api/ProductApi/GetSearchProduct")]
        public IHttpActionResult GetSearchProduct(string getproduct, int category,int row,int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            if (category == 0)
            {
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where (p.mprodname.Contains(getproduct) || pc.mcatname == getproduct) && p.approve == 1 && p.mactive == "yes" && pcv.mrp>0
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
                                mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                                mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                            }).OrderBy(x=>x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();


            }
            else
            {
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where (p.mprodname.Contains(getproduct)  || pc.mcatname == getproduct) && (pc.mprodcatid==category || pc.mpreviouscat==category) && p.approve == 1 && p.mactive == "yes" && pcv.mrp!=0
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
                                mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                                mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                            }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();


            }

            if (products.Count == 0)
            {
                return Ok("empty");
            }
            products.ForEach(x =>
            {
                x.productspecs=GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
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
                             where p.mprodid == prodid && pc.mdetailvalue!=null && pc.mdetailvalue!=""
                             select new SingleProductDetailViewModel()
                             {
                                 mprodid = prodid,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).Take(4).ToList();
            
            return productdetail;
        }
        [HttpGet]
        [Route("api/ProductApi/GetSearchList")]
        public IHttpActionResult GetSearchList(string name,int cat)
        {
            var db = new LeasingDbEntities();
            var List = new List<ProductSearchViewModel>();
            var alist=new List<ProductSearchViewModel>();
            var blist=new List<ProductSearchViewModel>();
            if (cat == 0)
            {
                alist = (from p in db.productcats
                             where (p.mcatname.StartsWith(name) || p.mcatname.Contains(name))
                             select new ProductSearchViewModel
                             {
                                 mprodname = p.mcatname,
                             }).Take(10).ToList();
                List.AddRange(alist);
                blist = (from p in db.products
                         where p.mprodname.StartsWith(name) || p.mprodname.Contains(name)
                         select new ProductSearchViewModel
                         {
                             mprodname = p.mprodname,
                         }).Take(10).ToList();
                
            }
            else
            {
                alist = (from p in db.productcats
                         where (p.mcatname.StartsWith(name) || p.mcatname.Contains(name)) && (p.mpreviouscat==cat || p.mprodcatid==cat)
                         select new ProductSearchViewModel
                         {
                             mprodname = p.mcatname,
                         }).Take(10).ToList();
                List.AddRange(alist);
                blist = (from p in db.products
                         join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                         where p.mprodname.StartsWith(name) || p.mprodname.Contains(name) && pc.mprodcatid == cat
                         select new ProductSearchViewModel
                         {
                             mprodname = p.mprodname,
                         }).Take(10).ToList();
            }
            List.AddRange(alist);
            List.AddRange(blist);
            List = List.Take(10).ToList();
            return Ok(List.Distinct().ToList());
        }
        [HttpGet]
        [Route("api/ProductApi/GetProductMenu")]
        public IHttpActionResult GetAllProductMenuById(int getid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                
                            where p.mprodcatid == id && p.approve==1 && p.mactive=="yes" && pcv.mrp>0
                            select new ProductDisplayViewModel()
                            {
                                mprodid = p.mprodid,

                                mprodname = p.mprodname,
                                mprodcatname = pc.mcatname,
                                mbrandid = pbrand.mbrandid,
                                mbrand = pbrand.mbrandname,
                                mtermf = 1,
                                mtermp = 60,
                                
                                //msubpicname = y.Key.pic,
                                mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                                mprodvarid = pcv.mprodsubid,
                                msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            }).ToList<ProductDisplayViewModel>();


            
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        /***************************************Feature Product***********************************************/
        [HttpGet]
        [Route("api/ProductApi/GetFeatureProduct")]
        public IHttpActionResult GetFeatureProduct()
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            
            var seed = string.Empty;
            List<FeatureProductDisplayViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from p in db.products
                            join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                            join b in db.productbrands on p.mbrand equals b.mbrandid
                            join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                            from psub in psubdata.DefaultIfEmpty()
                            join pimg in db.productsubpics on psub.mprodsubid equals pimg.mprodsubid into pimgdata
                            from pimg in pimgdata.DefaultIfEmpty()
                            where p.mactive=="yes" && p.approve==1 && (p.mfeatured==1 || p.mnewarrival==1) && psub.mrp>0
                            group new
                            {
                                psub.mleadetime,
                                psub.mprodsubid,
                                psub.mrp,
                                psub.mofferprice,
                                pimg.msubprodpicname,
                                psub.msupplierid,
                            } by new
                            {
                                p.mprodid,
                                pc.mprodcatid,
                                pc.mcatname,
                                p.mbrand,
                                p.mprodname,
                                b.mbrandname,
                                p.mfeatured,
                                p.mnewarrival,
                                p.mactive,
                                p.mweekdeal
                            } into y
                            orderby y.Key.mprodid
                            select new FeatureProductDisplayViewModel
                            {
                                mprodvarid=y.FirstOrDefault().mprodsubid,
                                mproductcatid=y.Key.mprodcatid,
                                mprodid = y.Key.mprodid,
                                mprodname = y.Key.mprodname,
                                mprodcatname = y.Key.mcatname,
                                msubpicname = y.FirstOrDefault().msubprodpicname,
                                mfeatured = y.Key.mfeatured,
                                mnewarrival = y.Key.mnewarrival,
                                mweekdeal=y.Key.mweekdeal,
                                mprice =  y.FirstOrDefault().mrp.HasValue?y.FirstOrDefault().mrp.Value:0,
                                mleadtime = y.FirstOrDefault().mleadetime.HasValue ? y.FirstOrDefault().mleadetime.Value : 3,
                                msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                            }).ToList<FeatureProductDisplayViewModel>();


            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
            });

            return Ok(products);
        }

        [HttpGet]
        [Route("api/ProductApi/GetWeekDealProduct")]
        public IHttpActionResult GetWeekDealProduct()
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<WeekdealProductDisplayViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from pw in db.productweekdeals
                            join p in db.products on pw.mprodid equals p.mprodid
                            join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                            join b in db.productbrands on p.mbrand equals b.mbrandid
                            join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                            from psub in psubdata.DefaultIfEmpty()
                            where p.mactive == "yes" && p.approve == 1 && p.mweekdeal == 1 && psub.mrp > 0 && pw.mfeatured==1 && pw.mqty>0
                            group new
                            {
                                psub.mleadetime,
                                psub.mprodsubid,
                                psub.mrp,
                                psub.mofferprice,
                                psub.msupplierid,
                                pw.mqty,
                                pw.mofferdetail
                            } by new
                            {
                                p.mprodid,
                                pc.mprodcatid,
                                pc.mcatname,
                                p.mbrand,
                                p.mprodname,
                                b.mbrandname,
                                p.mfeatured,
                                p.mnewarrival,
                                p.mactive,
                                p.mweekdeal,
                                
                            } into y
                            orderby y.Key.mprodid
                            select new WeekdealProductDisplayViewModel
                            {
                                mprodvarid = y.FirstOrDefault().mprodsubid,
                                mproductcatid = y.Key.mprodcatid,
                                mprodid = y.Key.mprodid,
                                mprodname = y.Key.mprodname,
                                mprodcatname = y.Key.mcatname,
                                mfeatured = y.Key.mfeatured,
                                mnewarrival = y.Key.mnewarrival,
                                mweekdeal = y.Key.mweekdeal,
                                mprice = y.FirstOrDefault().mrp.Value,
                                mbasicprice=y.FirstOrDefault().mrp.HasValue?y.FirstOrDefault().mrp.Value:0,
                                mleadtime = y.FirstOrDefault().mleadetime.HasValue ? y.FirstOrDefault().mleadetime.Value : 3,
                                msupplierid=y.FirstOrDefault().msupplierid.HasValue?y.FirstOrDefault().msupplierid.Value:0,
                                mqty= y.FirstOrDefault().mqty.HasValue ? y.FirstOrDefault().mqty.Value : 0,
                                mofferdetail=y.FirstOrDefault().mofferdetail,
                            }).ToList<WeekdealProductDisplayViewModel>();


            }
            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                /*var dealimgdata = db.productdealimages.FirstOrDefault(z => z.ProdId == x.mprodid);
                if (dealimgdata != null)
                {
                    x.msubpicname = "product/"+dealimgdata.Imagename;
                }
                else
                {*/
                    x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname;
                //}
                x.mdiscount = 100-((x.mprice / x.mbasicprice) * 100);
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
            });

            return Ok(products);
        }

        [HttpGet]
        [Route("api/ProductApi/GetAllFeatureProduct")]
        public IHttpActionResult GetAllFeatureProduct(int row,int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();

            products = (from p in ctx.products
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                        from pcv in pcvdata.DefaultIfEmpty()
                        join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0 && (p.mfeatured == 1)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        /****************************************End Feature Product*****************************************/

        /***************************************New Arrival Product***********************************************/
        [HttpGet]
        [Route("api/ProductApi/GetAllNewArrivalProduct")]
        public IHttpActionResult GetAllNewArrivalProduct(int row,int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();

            products = (from p in ctx.products
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                        from pcv in pcvdata.DefaultIfEmpty()
                        join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.approve == 1 && p.mactive == "yes" && pcv.mrp > 0 && (p.mnewarrival == 1)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mbasicprice = pcv.mrp.HasValue ? pcv.mrp.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        /***************************************New Arrival Product***********************************************/

        /***************************************On Sale Product***********************************************/
        [HttpGet]
        [Route("api/ProductApi/GetOnSaleProduct")]
        public IHttpActionResult GetOnSaleProduct()
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<FeatureProductDisplayViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from p in db.products
                            join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                            join b in db.productbrands on p.mbrand equals b.mbrandid
                            join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                            from psub in psubdata.DefaultIfEmpty()
                            join pimg in db.productsubpics on psub.mprodsubid equals pimg.mprodsubid into pimgdata
                            from pimg in pimgdata.DefaultIfEmpty()
                            where p.mactive == "yes" && p.approve==1 && (psub.mofferprice<psub.mrp && psub.mofferprice>0)
                            group new
                            {
                                psub.mleadetime,
                                psub.mprodsubid,
                                pimg.msubprodpicname,
                                psub.mofferprice,
                                psub.msupplierid,
                            } by new
                            {
                                p.mprodid,
                                pc.mprodcatid,
                                pc.mcatname,
                                p.mbrand,
                                p.mprodname,
                                b.mbrandname,
                                p.mfeatured,
                                p.mnewarrival,
                                p.mactive,
                                

                            } into y
                            orderby y.Key.mprodid
                            select new FeatureProductDisplayViewModel
                            {
                                mprodvarid=y.FirstOrDefault().mprodsubid,
                                mproductcatid = y.Key.mprodcatid,
                                mprodid = y.Key.mprodid,
                                mprodname = y.Key.mprodname,
                                mprodcatname = y.Key.mcatname,
                                msubpicname = y.FirstOrDefault().msubprodpicname,
                                mfeatured = y.Key.mfeatured,
                                mnewarrival = y.Key.mnewarrival,
                                mprice=y.FirstOrDefault().mofferprice.HasValue?y.FirstOrDefault().mofferprice.Value:0,
                                mleadtime=y.FirstOrDefault().mleadetime.HasValue?y.FirstOrDefault().mleadetime.Value:0,
                                msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                            }).ToList<FeatureProductDisplayViewModel>();


            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
            });

            return Ok(products);
        }

        [HttpGet]
        [Route("api/ProductApi/GetAllOnSaleProduct")]
        public IHttpActionResult GetAllOnSaleProduct(int row,int rowperpage)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();

            products = (from p in ctx.products
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                        from pcv in pcvdata.DefaultIfEmpty()
                        join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.approve == 1 && p.mactive == "yes" && pcv.mofferprice > 0 && (pcv.mofferprice < pcv.mrp && pcv.mofferprice > 0)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            msuppliername = users.morgname + (users.mtown != null ? ", " + users.mtown : "") + (users.mcity != null ? " - " + users.mcity : ""),
                            //msubpicname = y.Key.pic,
                            mprice = pcv.mofferprice.HasValue ? pcv.mofferprice.Value : 0,
                            mbasicprice = pcv.mofferprice.HasValue ? pcv.mofferprice.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                            mleadtime = pcv.mleadetime.HasValue ? pcv.mleadetime.Value : 3,
                        }).OrderBy(x=>x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        /***************************************On Sale Product***********************************************/
        /*
        [HttpGet]
        public IHttpActionResult GetAllProduct()
        {
            IList<ProductDisplayViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
               products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pimg in ctx.productsubpics on p.mprodid equals pimg.mprodsubid into pimgdata
                            from pimg in pimgdata.DefaultIfEmpty()
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            group new
                            {
                                pcv.mrp,
                                pimg.msubprodpicname
                            }
                            by new
                            {
                                p.mprodid,
                                p.mprodname,
                                p.mbrand,
                                pc.mcatname

                            } into y
                            orderby y.Key.mprodid
                            select new ProductDisplayViewModel()
                            {
                                mprodid=y.Key.mprodid,
                                mprodname =y.Key.mprodname,
                                mprodcatname=y.Key.mcatname,
                                mbrand=y.Key.mbrand,
                                msubpicname=y.FirstOrDefault().msubprodpicname,
                                mprice= y.FirstOrDefault().mrp.HasValue?y.FirstOrDefault().mrp.Value:0,
                            }).ToList<ProductDisplayViewModel>();
                
                
            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            return Ok(products);
        }*/

        [HttpPost]
        public IHttpActionResult AddToCart(Items item)
        {
            int no = item.Quantity;
            if (SessionWrapper.Cart.Any())
            {

                List<Items> cart = new List<Items>();
                cart.Add(new Items { Product = ProductDao.GetProductById(item.Product.mprodvarid), Quantity = no });
                SessionWrapper.Cart = cart;
            }
            else
            {
                List<Items> cart = (List<Items>)SessionWrapper.Cart;
                int index = isExist1(item.Product.mprodid, item.Product.msupplierid);
                if (index != -1)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    cart.Add(new Items { Product = ProductDao.GetProductById(item.Product.mprodvarid), Quantity = no });
                }
                SessionWrapper.Cart = cart;
            }
            if (SessionWrapper.Cart.Count == 0)
            {
                return NotFound();
            }
            return Ok(SessionWrapper.Cart);
        }
        [HttpGet]
        [Route("api/ProductApi/GetCart")]
        public IHttpActionResult GetCart()
        {
            return Ok(SessionWrapper.Cart);
        }
        [HttpGet]
        [Route("api/ProductApi/GetCategory")]
        public IHttpActionResult GetCategory(int id)
        {
            var db = new LeasingDbEntities();
            var c = "";
            var catdata = (from ps in db.productsubs
                           join p in db.products on ps.mprodid equals p.mprodid
                           join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                           where ps.mprodsubid == id
                           select new
                           {
                               Category = pc.mcatname
                           }).FirstOrDefault();
            if (catdata != null)
            {
                c = catdata.Category;
            }
            return Ok(c);
        }
        private int isExist1(int id, int msupplierid)
        {
            List<Items> cart = (List<Items>)SessionWrapper.Cart;
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].Product.mprodvarid.Equals(id) && cart[i].Product.msupplierid.Equals(msupplierid))
                    return i;
            return -1;
        }

        [Route("api/ProductApi/SaveQuote")]
        public IHttpActionResult SaveQuote(QuotationMoreDetailViewModel datas)
        {
            var db = new LeasingDbEntities();
            var quotedata = db.userquotationmasters.OrderByDescending(x => x.mquoteid).FirstOrDefault();
            var q = 1;
            var quoteref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
            if (quotedata != null)
            {
                q = quotedata.mquoteid;

                quoteref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

            }

            var obj2 = new userquotationmaster
            {

                muserid = datas.muserid,
                mquotetotal = datas.mquotetotal,
                mquotestatus = "pending",
                createddate = DateTime.Now,
                mquoteref = quoteref,
            };
            db.userquotationmasters.Add(obj2);
            db.SaveChanges();
            var mquoteid = obj2.mquoteid;
            var datas2 = db.SaveCarts.Where(x => x.Session_Id == datas.SessionId).ToList();
            if (datas2.Any())
            {
                foreach (var d in datas2)
                {
                    var searchdata = (from us in db.usersubquotations
                                      join u in db.userquotationmasters on us.mquoteid equals u.mquoteid
                                      where us.type == "q" && us.mprodvarid == d.mprodvarid && us.msupplierid == d.msupplierid && us.mtermf == d.mtermf && us.mtermp == d.mtermp && u.muserid == datas.muserid
                                      select us).FirstOrDefault();
                                      //.FirstOrDefault(x => x.type == "q" && && x.mprodvarid == d.mprodvarid && x.msupplierid == d.msupplierid && x.mtermf == d.mtermf && x.mtermp == d.mtermp);
                    if (searchdata != null)
                    {
                        searchdata.quantity = searchdata.quantity + d.mqty;
                        db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                            
                    }
                    else
                    {
                        var obj = new usersubquotation
                        {
                            type = "q",
                            mquoteid = mquoteid,
                            mprodvarid = d.mprodvarid,
                            mtermf = d.mtermf,
                            mtermp = d.mtermp,
                            mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0,
                            quantity = d.mqty,
                            msupplierid = d.msupplierid,
                            mstatus = "pending"
                        };
                        db.usersubquotations.Add(obj);
                        db.SaveChanges();

                    }
                    /*var datasub = db.SaveSubCarts.Where(x => x.mcompareprodid == d.Id).ToList();
                    if (datasub.Any())
                    {
                        foreach(var ds in datasub)
                        {
                            var obj3 = new usercomparesubquotation
                            {
                                mcompareprodid = obj.msubquoteid,
                                msubquoteid = ds.msubquoteid,
                                msupplierid = ds.msupplierid
                            };
                            db.usercomparesubquotations.Add(obj3);
                            db.SaveChanges();
                        }
                        db.SaveSubCarts.RemoveRange(datasub);
                        db.SaveChanges();
                    }*/
                }
                db.SaveCarts.RemoveRange(datas2);
                db.SaveChanges();
            }

            return Ok(mquoteid);
        }
        [Route("api/ProductApi/SaveOldQuote")]
        public IHttpActionResult SaveOldQuote(QuotationMoreDetailViewModel datas)
        {
            var db = new LeasingDbEntities();
            var quotedata = db.userquotationmasters.OrderByDescending(x => x.mquoteid).FirstOrDefault();
            var q = 1;
            var quoteref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
            if (quotedata != null)
            {
                q = quotedata.mquoteid;

                quoteref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

            }

            var obj2 = new userquotationmaster
            {

                muserid = datas.muserid,
                mquotetotal = datas.mquotetotal,
                mquotestatus = "pending",
                createddate = DateTime.Now,
                mquoteref = quoteref,
            };
            db.userquotationmasters.Add(obj2);
            db.SaveChanges();
            var mquoteid = obj2.mquoteid;
            foreach (var d in datas.Product)
            {
                var obj = new usersubquotation
                {
                    type = "q",
                    mquoteid = mquoteid,
                    mprodvarid = d.Product.mprodvarid,
                    mtermf = d.Product.mtermf,
                    mtermp = d.Product.mtermp,
                    mprice = d.Product.mprice.HasValue ? Math.Round(d.Product.mprice.Value, 2) : 0,
                    quantity = d.Quantity,
                    msupplierid = d.Product.msupplierid,
                    mstatus = "pending"
                };
                db.usersubquotations.Add(obj);
                db.SaveChanges();
            }
            return Ok("Save");
        }


        /***********************For Print Email Quotes***************************************************/
        [Route("api/ProductApi/CheckQuoteTerms")]
        public IHttpActionResult CheckQuoteTerms(List<UserQuotationNewViewModel> datas)
        {
            var f = true;
            var db = new LeasingDbEntities();
            if (datas.Any())
            {
                var uid = datas.FirstOrDefault().muserid; 
                var datafound = (from sb in db.usersubquotations
                                 join s in db.userquotationmasters on sb.mquoteid equals s.mquoteid
                                 where ((sb.mstatus == "printed" || sb.mstatus == "approved") && sb.type == "q" && s.muserid == uid)
                                 select sb).ToList();
                if (datafound.Any())
                {
                    foreach (var d in datas)
                    {
                        var subquotes = datafound.Where(x => x.mtermf == d.mtermf && x.mtermp == d.mtermp).FirstOrDefault();
                        if (subquotes != null)
                        {
                            f = true;
                        }
                        else
                        {
                            return Ok(false);
                            
                        }

                    }
                    if (f)
                    {
                        return Ok(true);
                    }
                }
                else
                {
                    return Ok(true);
                }

                
            }
            
            return Ok("failed");
        }

        [Route("api/ProductApi/CheckSelectedQuoteTerms")]
        public IHttpActionResult CheckSelectedQuoteTerms(List<UserQuotationNewViewModel> datas)
        {
            var f = true;
            var db = new LeasingDbEntities();
            if (datas.Any())
            {
                var uid = datas.FirstOrDefault().muserid;
                var datafound = (from sb in db.usersubquotations
                                 join s in db.userquotationmasters on sb.mquoteid equals s.mquoteid
                                 where ((sb.mstatus == "printed" || sb.mstatus == "approved") && sb.type == "q" && s.muserid == uid)
                                 select sb).ToList();
                if (datafound.Any())
                {
                    foreach (var d in datas.Where(x=>x.Selected==true).ToList())
                    {
                        var subquotes = datafound.Where(x => x.mtermf == d.mtermf && x.mtermp == d.mtermp).FirstOrDefault();
                        if (subquotes != null)
                        {
                            f = true;
                        }
                        else
                        {
                            return Ok(false);

                        }

                    }
                    if (f)
                    {
                        return Ok(true);
                    }
                }
                else
                {
                    return Ok(true);
                }


            }

            return Ok("failed");
        }

        [Route("api/ProductApi/UpdateQuote")]
        public IHttpActionResult UpdateQuote(List<UserQuotationNewViewModel> datas)
        {
            var db = new LeasingDbEntities();
            if (datas.Any())
            {
                foreach (var d in datas)
                {
                    var subquotes = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == d.msubquoteid);
                    if (subquotes != null)
                    {
                        subquotes.mquoteid = d.mquoteid;
                        subquotes.mprodvarid = d.mprodvarid;
                        subquotes.mtermf = d.mtermf;
                        subquotes.mtermp = d.mtermp;
                        subquotes.mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0;
                        subquotes.quantity = d.quantity;
                        subquotes.msupplierid = d.msupplierid;
                        subquotes.mstatus = "pending";
                        db.Entry(subquotes).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }
                    
                }
            }
            
            return Ok("Save");
        }
        [Route("api/ProductApi/UpdateSelectedQuote")]
        public IHttpActionResult UpdateSelectedQuote(List<UserQuotationNewViewModel> datas)
        {
            var db = new LeasingDbEntities();
            if (datas.Any())
            {
                foreach (var d in datas.Where(x=>x.Selected==true).ToList())
                {
                    var subquotes = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == d.msubquoteid);
                    if (subquotes != null)
                    {
                        subquotes.mquoteid = d.mquoteid;
                        subquotes.mprodvarid = d.mprodvarid;
                        subquotes.mtermf = d.mtermf;
                        subquotes.mtermp = d.mtermp;
                        subquotes.mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0;
                        subquotes.quantity = d.quantity;
                        subquotes.msupplierid = d.msupplierid;
                        subquotes.mstatus = "printed";
                        db.Entry(subquotes).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                }
            }

            return Ok("Save");
        }
        [Route("api/ProductApi/UpdateSingleQuote")]
        public IHttpActionResult UpdateSingleQuote(UserQuotationNewViewModel datas)
        {
            var db = new LeasingDbEntities();
            if (datas!=null)
            {
                var d = datas;
                    var subquotes = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == datas.msubquoteid);
                    if (subquotes != null)
                    {
                        subquotes.mquoteid = d.mquoteid;
                        subquotes.mprodvarid = d.mprodvarid;
                        subquotes.mtermf = d.mtermf;
                        subquotes.mtermp = d.mtermp;
                        subquotes.mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0;
                        subquotes.quantity = d.quantity;
                        subquotes.msupplierid = d.msupplierid;
                        subquotes.mstatus = "pending";
                        db.Entry(subquotes).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                    }

                
            }

            return Ok("Save");
        }
        [HttpPost]
        [Route("api/ProductApi/GetAgreementData")]
        public IHttpActionResult GetAgreementData(List<GetAgreementDataViewModel> datas)
        {
            var db = new LeasingDbEntities();
            var prodlist =new List<PassAgreementDataDisplayViewModel>();
            if (datas.Any())
            {

                foreach (var d in datas)
                {
                   var products = (from us in db.usersubquotations
                                join pcv in db.productsubs on us.mprodvarid equals pcv.mprodsubid
                                join p in db.products on pcv.mprodid equals p.mprodid
                                join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in db.productbrands on p.mbrand equals pbrand.mbrandid
                                where us.msubquoteid==d.msubquoteid
                                select new AgreementDataDisplayViewModel()
                                {
                                    Make = pbrand.mbrandname,
                                    Condition=pcv.mcondition,
                                    
                                }).FirstOrDefault();
                    var p2 = new PassAgreementDataDisplayViewModel
                    {
                        msubquoteid=d.msubquoteid,
                        AgreementData=products,
                    };
                    prodlist.Add(p2);
                }
                return Ok(prodlist);

            }



            return Ok("failed");
        }

        

        [Route("api/ProductApi/SaveOrder")]
        public IHttpActionResult SaveOrder(List<SubSingleQuotationViewModel> multipledatas)
        {
            var db = new LeasingDbEntities();
            var prodlist = "";
            float ffee = 0;
            float sfee = 0;
            var commonfields = db.CommonFields.ToList();
            List<int> mleaseno = new List<int>();
            if (commonfields.Any())
            {
                foreach(var a in commonfields)
                {
                    if (a.name == "sfee")
                    {
                        sfee = float.Parse(a.value);
                    }
                    if (a.name == "ffee")
                    {
                        ffee = float.Parse(a.value);
                    }
                }
                
            }
            if (multipledatas.Count > 0)
            {
                foreach(var datas in multipledatas)
                {
                    if (datas.Product.Any())
                    {
                        var orderref = 101;
                        var ordproducts = datas.Product.Where(x => x.Selected == true).ToList();
                        if (ordproducts.Any())
                        {
                            var orddata = db.userleaseagreements.OrderByDescending(x => x.mleaseid).FirstOrDefault();
                            var q = 1;

                            //var orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
                            if (orddata != null)
                            {
                                orderref = (orddata.mleaseno.HasValue?orddata.mleaseno.Value:100) + 1;
                                //orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

                            }
                            
                            var obj2 = new userleaseagreement
                            {
                                muserid = datas.muserid,
                                total = Math.Round(datas.mquotetotal,2),
                                mleaseno = orderref,
                                msorderref=datas.msorderref,
                                ffee = Math.Round(ffee,2),
                                sfee = Math.Round(sfee,2),
                                msend = 1,
                                mpayment=datas.mpayment,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            mleaseno.Add(orderref);
                            foreach (var d in ordproducts)
                            {

                                if (prodlist == "")
                                {
                                    prodlist = d.msubquoteid.ToString();
                                }
                                else
                                {
                                    prodlist = prodlist + "," + d.msubquoteid.ToString();
                                }
                                var quotesubdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == d.msubquoteid);
                                if (quotesubdata != null)
                                {
                                    quotesubdata.magreementno = obj2.mleaseno;
                                    quotesubdata.type = "o";
                                    quotesubdata.mtermf = d.mtermf;
                                    quotesubdata.mtermp = d.mtermp;
                                    quotesubdata.mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0;
                                    quotesubdata.quantity = d.quantity;
                                    quotesubdata.mstatus = "approved";
                                    db.Entry(quotesubdata).State = System.Data.Entity.EntityState.Modified;
                                    db.SaveChanges();
                                    if (d.Address != null)
                                    {
                                        var address = new AgreementAddress
                                        {
                                            maddress = d.Address,
                                            msubquoteid = d.msubquoteid
                                        };
                                        db.AgreementAddresses.Add(address);
                                        db.SaveChanges();
                                    }
                                }
                            }

                        }

                    }
                }
                

                return Ok(mleaseno);

            }
            return Ok("failed");
        }

        [Route("api/ProductApi/SaveOrderFromCart")]
        public IHttpActionResult SaveOrderFromCart(List<SubSingleQuotationViewModel> multipledatas)
        {
            var db = new LeasingDbEntities();
            var prodlist = "";
            float ffee = 0;
            float sfee = 0;
            var muserid = 0;
            var commonfields = db.CommonFields.ToList();
            List<int> mleaseno = new List<int>();
            if (commonfields.Any())
            {
                foreach (var a in commonfields)
                {
                    if (a.name == "sfee")
                    {
                        sfee = float.Parse(a.value);
                    }
                    if (a.name == "ffee")
                    {
                        ffee = float.Parse(a.value);
                    }
                }

            }
            if (multipledatas.Count > 0)
            {
                foreach (var datas in multipledatas)
                {
                    if (datas.Product.Any())
                    {
                        muserid = datas.mschoolid;
                        var orderref = 101;
                        var ordproducts = datas.Product.ToList();
                        if (ordproducts.Any())
                        {
                            var orddata = db.userleaseagreements.OrderByDescending(x => x.mleaseid).FirstOrDefault();
                            var q = 1;

                            //var orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
                            if (orddata != null)
                            {
                                orderref = (orddata.mleaseno.HasValue ? orddata.mleaseno.Value : 100) + 1;
                                //orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

                            }

                            var obj2 = new userleaseagreement
                            {
                                mprintby=datas.muserid.ToString(),
                                mprintdate=DateTime.Now,
                                muserid = datas.mschoolid,
                                total = Math.Round(datas.mquotetotal, 2),
                                mleaseno = orderref,
                                msorderref = datas.msorderref,
                                ffee = Math.Round(ffee, 2),
                                sfee = Math.Round(sfee, 2),
                                msend = 1,
                                mpayment = datas.mpayment,
                                mcontactid=datas.mcontactid,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                                mdefaultadd=datas.mdefaultadd,
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            mleaseno.Add(orderref);
                            foreach (var d in ordproducts)
                            {
                                var obj3 = new usersubquotation
                                {
                                    type = "o",
                                    mprodvarid = d.mprodvarid,
                                    mtermf = d.mtermf,
                                    mtermp = d.mtermp,
                                    mvat=d.mvat,
                                    mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0,
                                    quantity = d.quantity,
                                    msupplierid = d.msupplierid,
                                    mstatus = "approved",
                                    magreementno=obj2.mleaseno,
                                    mofferdetail=d.mofferdetail,
                                };
                                db.usersubquotations.Add(obj3);
                                db.SaveChanges();
                                var searchsavecart = db.SaveCarts.FirstOrDefault(x => x.Id == d.msubquoteid);
                                if (searchsavecart != null)
                                {
                                    db.Entry(searchsavecart).State = System.Data.Entity.EntityState.Deleted;
                                    db.SaveChanges();
                                }
                                
                            }

                        }

                    }
                }
                /*SmallLeaseAggreementViewModel mleaseids = new SmallLeaseAggreementViewModel {
                    muserid = muserid,
                    mleasenos = mleaseno
                };*/
                List<LeaseAggreementViewModel> products = null;
                products = (from la in db.userleaseagreements
                            where la.muserid == muserid && mleaseno.Any(s => s == la.mleaseno)
                            select new LeaseAggreementViewModel
                            {
                                mleaseno = la.mleaseno
                            }).ToList();
                //products = AgreementDAO.GetListAggreementById(mleaseids);

                return Ok(products);
               // return Ok(mleaseno);

            }
            return Ok("failed");
        }

        [Route("api/ProductApi/SaveSingleOrderFromCart")]
        public IHttpActionResult SaveSingleOrderFromCart(SubSingleQuotationViewModel multipledatas)
        {
            var db = new LeasingDbEntities();
            var prodlist = "";
            string content = "";
            float ffee = 0;
            float sfee = 0;
            var muserid = 0;
            
            var commonfields = db.CommonFields.ToList();
            List<int> mleaseno = new List<int>();
            if (commonfields.Any())
            {
                foreach (var a in commonfields)
                {
                    if (a.name == "sfee")
                    {
                        sfee = float.Parse(a.value);
                    }
                    if (a.name == "ffee")
                    {
                        ffee = float.Parse(a.value);
                    }
                }

            }
            if (multipledatas!=null)
            {
                var orderref = 101;
                var datas = multipledatas;
                {
                    if (datas.Product.Any())
                    {
                        muserid = datas.mschoolid;
                        var ordproducts = datas.Product.ToList();
                        if (ordproducts.Any())
                        {
                            var orddata = db.userleaseagreements.OrderByDescending(x => x.mleaseid).FirstOrDefault();
                            var q = 1;

                            //var orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
                            if (orddata != null)
                            {
                                orderref = (orddata.mleaseno.HasValue ? orddata.mleaseno.Value : 100) + 1;
                                //orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

                            }

                            var obj2 = new userleaseagreement
                            {
                                muserid = datas.mschoolid,
                                mprintby = datas.muserid.ToString(),
                                mprintdate = DateTime.Now,
                                total = Math.Round(datas.mquotetotal, 2),
                                mleaseno = orderref,
                                msorderref = datas.msorderref,
                                ffee = Math.Round(ffee, 2),
                                sfee = Math.Round(sfee, 2),
                                msend = 1,
                                mpayment = datas.mpayment,
                                mcontactid = datas.mcontactid,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                                mest = DateTime.Now.AddDays(7)
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            mleaseno.Add(orderref);
                            foreach (var d in ordproducts)
                            {
                                var obj3 = new usersubquotation
                                {
                                    type = "o",
                                    mprodvarid = d.mprodvarid,
                                    mtermf = d.mtermf,
                                    mtermp = d.mtermp,
                                    mvat = d.mvat,
                                    mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0,
                                    quantity = d.quantity,
                                    msupplierid = d.msupplierid,
                                    mstatus = "approved",
                                    magreementno = obj2.mleaseno,
                                    mexpdeliverydate = DateTime.Now.AddDays(7),
                                    msendtosupplier=1,
                                };
                                db.usersubquotations.Add(obj3);
                                db.SaveChanges();
                                if (d.Address.ToString() != null && d.Address.ToString() !="")
                                {
                                    var objaddr = new AgreementAddress
                                    {
                                        msubquoteid = obj3.msubquoteid,
                                        maddress = d.Address
                                    };
                                    db.AgreementAddresses.Add(objaddr);
                                    db.SaveChanges();
                                }
                                var searchsavecart = db.SaveCarts.FirstOrDefault(x => x.Id == d.msubquoteid);
                                if (searchsavecart != null)
                                {
                                    db.Entry(searchsavecart).State = System.Data.Entity.EntityState.Deleted;
                                    db.SaveChanges();
                                    var searchsubadress = db.SubCartAddresses.Where(x => x.mcartid == d.msubquoteid).ToList();
                                    db.SubCartAddresses.RemoveRange(searchsubadress);
                                    db.SaveChanges();
                                }

                            }

                        }

                    }
                    /*var products = (from l in db.userleaseagreements
                                    join o in db.usersubquotations on l.mleaseno equals o.magreementno
                                    where l.mleaseno ==orderref
                                select new exportproductviewmodel
                                {
                                    mleaseno = l.mleaseno,
                                    subquoteids = o.msubquoteid,
                                    ffee = l.ffee,
                                    sfee = l.sfee,
                                    muserid = l.muserid
                                }).ToList();
                    
                        
                    OrderController order = new OrderController();
                    string viewContent = order.ConvertViewToString("~/Views/Order/LoadOrders", products);
                    content = content + viewContent;*/
                    return Ok(orderref);

               }
                /*SmallLeaseAggreementViewModel mleaseids = new SmallLeaseAggreementViewModel
                {
                    muserid = muserid,
                    mleasenos = mleaseno
                };
                List<LeaseAggreementViewModel> products = null;
                products = AgreementDAO.GetListAggreementById(mleaseids);
                */
                // return Ok(mleaseno);

            }
            return Ok("failed");
        }
        /*********************************Old Save Order****************************************************/
        [Route("api/ProductApi/OldSaveOrder")]
        public IHttpActionResult OldSaveOrder(SingleQuotationViewModel datas)
        {
            var db = new LeasingDbEntities();
            var ordproducts = datas.Product.Where(x => datas.SelectedQuote.Contains(x.msubquoteid)).ToList();
            var prodlist = "";
            if (ordproducts.Any())
            {
                foreach (var d in ordproducts)
                {
                    if (prodlist == "")
                    {
                        prodlist = d.msubquoteid.ToString();
                    }
                    else
                    {
                        prodlist = prodlist + "," + d.msubquoteid.ToString();
                    }
                    var quotesubdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == d.msubquoteid);
                    if (quotesubdata != null)
                    {
                        quotesubdata.type = "o";
                        quotesubdata.mtermf = d.mtermf;
                        quotesubdata.mtermp = d.mtermp;
                        quotesubdata.mprice = d.mprice.HasValue ? Math.Round(d.mprice.Value, 2) : 0;
                        quotesubdata.quantity = d.quantity;
                        quotesubdata.mstatus = "lpending";
                        db.Entry(quotesubdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                }
                var quotedata = db.userquotationmasters.FirstOrDefault(x => x.mquoteid == datas.mquoteid);
                if (quotedata != null)
                {
                    quotedata.mquotetotal = datas.mquotetotal.HasValue ? Math.Round(datas.mquotetotal.Value, 2) : 0;
                    db.Entry(quotedata).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                var orddata = db.userordermasters.OrderByDescending(x => x.morderid).FirstOrDefault();
                var q = 1;
                var orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + q.ToString();
                if (orddata != null)
                {
                    q = orddata.morderid;
                    orderref = DateTime.Now.Ticks.ToString() + "" + datas.muserid.ToString() + "" + (q + 1).ToString();

                }

                var obj2 = new userordermaster
                {
                    muserid = datas.muserid,
                    mnotes = datas.mnotes,
                    morderstatus = "pending",
                    mprodlist = prodlist,
                    createddate = DateTime.Now,
                    
                };
                db.userordermasters.Add(obj2);
                db.SaveChanges();
                return Ok("Save");

            }
            return Ok("failed");
        }

        [HttpGet]
        [Route("api/ProductApi/FindandGetRatingProduct")]
        public IHttpActionResult FindandGetRatingProduct(int getproduct, int userid)
        {
            var db = new LeasingDbEntities();
            //getproduct = HttpUtility.UrlDecode(getproduct);
            var id = getproduct;//int.Parse(Encryption.Decrypt(getproduct));

            var seed = string.Empty;
            ProductRatingViewModel productrating = null; 
            var ctx = new LeasingDbEntities();
            var ispurchased = (from sb in ctx.usersubquotations
                               join s in ctx.userleaseagreements on sb.magreementno equals s.mleaseno
                               where sb.mprodvarid == id && s.muserid == userid && sb.mstatus == "ldelivered"
                               select sb
                             ).FirstOrDefault();
            if (ispurchased != null)
            {
                productrating = (from pr in ctx.productreviews
                                 where pr.mprodid == id && pr.muserid == userid
                                 select new ProductRatingViewModel
                                 {
                                     mreviewid=pr.mreviewid,
                                     mrankno=pr.mrankno,
                                     mreviewtitle=pr.mreviewtitle,
                                     mcomments=pr.mcomments,
                                     
                                 }).FirstOrDefault();
                
                if (productrating != null)
                {
                    List<productreviewpic> productreview = null;
                    productreview = db.productreviewpics.Where(x => x.mreviewid == productrating.mreviewid).ToList();
                    return Ok(new { productrating = productrating,reviewimage=productreview });
                }
                else
                {
                    return Ok("empty");
                }
            }
            else
            {
                return Ok("noproduct");
            }
            
            
        }
    }
}
