using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LeasingPortalApi.Controllers.Api
{
    public class SingleProductApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetProductById(int getid,string ptype,string getsup)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            SingleProductDisplayViewModel products = null;
            var ctx = new LeasingDbEntities();
            if (getsup == "0" || getsup==null)
            {
                products = (from pcv in ctx.productsubs
                            let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid).OrderBy(x => x.mrp).FirstOrDefault()
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where pcv.mprodsubid == id && psupp.mrp > 0 && users.mstatus == "active"
                            select new SingleProductDisplayViewModel()
                            {
                                mpn=p.mmanufacturercode,
                                mbrandlogo=pbrand.mphoto,
                                mprodvarid = pcv.mprodsubid,
                                mprodid = p.mprodid,
                                mprodname = p.mprodname,
                                mprodcatid = pc.mprodcatid,
                                mprodcatname = pc.mcatname,
                                mbrandid = pbrand.mbrandid,
                                mbrand = pbrand.mbrandname,
                                sellerskuid = pcv.muniquecode,
                                mstock = psupp.msstock.HasValue ? psupp.msstock.Value : 0,
                                mheightcm = p.mheightcm.HasValue ? p.mheightcm.Value : 0,
                                mlengthcm = p.mlengthcm.HasValue ? p.mlengthcm.Value : 0,
                                mwidthcm = p.mwidthcm.HasValue ? p.mwidthcm.Value : 0,
                                mdepthcm = p.mdepthcm.HasValue ? p.mdepthcm.Value : 0,
                                mweightcm = p.mweightcm.HasValue ? p.mweightcm.Value : 0,
                                mrp = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mmrpbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mvariationprice = (psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mdescription = p.mdescription,
                                supcode=users.morgname.Substring(0,3),
                                distributor=psupp.sellername!=null && psupp.sellername!=""?psupp.sellername.Substring(0,2):"00",
                                msuppliername =p.mmanufacturer, //users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                                mtermf = 1,
                                mtermp = 60,
                                msupplierid = (psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0),
                                moresupplier = (byte)(pcv.moresupplier.HasValue ? pcv.moresupplier.Value : 0),
                                minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1
                                /*productsubvariationtwo = y.Select(x => new ProductSubVariationTwoViewModel
                                {
                                    mprodvariationvalue = x.mprodvariationvalue,
                                }).AsQueryable()*/
                            }).OrderBy(x => x.mrp).SingleOrDefault();
            }else
            {
                int sid = int.Parse(getsup);
                products = (from pcv in ctx.productsubs
                            join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where pcv.mprodsubid == id && psupp.mrp > 0 && psupp.SupplierId==sid && users.mstatus == "active"
                            select new SingleProductDisplayViewModel()
                            {
                                mpn = p.mmanufacturercode,
                                mbrandlogo =pbrand.mphoto,
                                mprodvarid = pcv.mprodsubid,
                                mprodid = p.mprodid,
                                mprodname = p.mprodname,
                                mprodcatid = pc.mprodcatid,
                                mprodcatname = pc.mcatname,
                                mbrandid = pbrand.mbrandid,
                                mbrand = pbrand.mbrandname,
                                sellerskuid = pcv.muniquecode,
                                mstock = psupp.msstock.HasValue ? psupp.msstock.Value : 0,
                                mheightcm = p.mheightcm.HasValue ? p.mheightcm.Value : 0,
                                mlengthcm = p.mlengthcm.HasValue ? p.mlengthcm.Value : 0,
                                mwidthcm = p.mwidthcm.HasValue ? p.mwidthcm.Value : 0,
                                mdepthcm = p.mdepthcm.HasValue ? p.mdepthcm.Value : 0,
                                mweightcm = p.mweightcm.HasValue ? p.mweightcm.Value : 0,
                                mrp = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mmrpbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mvariationprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mdescription = p.mdescription,
                                supcode = users.morgname.Substring(0, 3),
                                distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                                msuppliername = p.mmanufacturer,
                                //msuppliername = users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                                mtermf = 1,
                                mtermp = 60,
                                minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                msupplierid = (psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0),
                                moresupplier = (byte)(pcv.moresupplier.HasValue ? pcv.moresupplier.Value : 0),
                                mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1
                                /*productsubvariationtwo = y.Select(x => new ProductSubVariationTwoViewModel
                                {
                                    mprodvariationvalue = x.mprodvariationvalue,
                                }).AsQueryable()*/
                            }).OrderBy(x => x.mrp).SingleOrDefault();

            }

            //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))

            if (products == null)
            {
                return Ok("failed");
            }
            if (ptype == "3")
            {
                var pwdata = ctx.productweekdeals.FirstOrDefault(x => x.mprodid == products.mprodid);
                if (pwdata != null)
                {
                    
                    products.mofferprice = pwdata.mweekprice.HasValue ? pwdata.mweekprice.Value : products.mbasicprice;
                    products.mofferqty = pwdata.mqty.HasValue?pwdata.mqty.Value:0;
                    products.mofferdetail = pwdata.mofferdetail;
                }
            }
            products.ProductOffer= getNumberOfOffer(products.mprodvarid, products.msupplierid);
            products.mproductcaturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            products.mproducturl = "xx=" + products.supcode + "&yy=" + products.distributor + "&productname=" + HttpUtility.UrlEncode(products.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodvarid.ToString()));
            products.mprodname = products.mprodname + " " + (ctx.prodsubvariationtwoes.Where(x => x.mprodsubid == products.mprodvarid && x.mprodvariationvalue!=null).Any() ? ctx.prodsubvariationtwoes.Where(x => x.mprodsubid == products.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
            products.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == products.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == products.mprodvarid).msubprodpicname : "";
            var prodid = products.mprodid;
            List<SingleProductDetailViewModel> productdetail = null;

            productdetail = (from p in ctx.products
                             join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                             join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                             join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                             from pu in pudata.DefaultIfEmpty()
                             where p.mprodid == prodid && pc.mdetailvalue!=null && pc.mdetailvalue!=""
                             select new SingleProductDetailViewModel()
                             {
                                 morder=pd.morder.HasValue?pd.morder.Value:100,
                                 mprodid = id,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).OrderBy(x=>x.morder).OrderBy(x=>x.mdetailhead).ToList();

            var subid = id;
            var prodname = products.mprodname;
            List<ProductSubVariationDisplayViewModel> productvariation = null;
            productvariation = (from pcv in ctx.productsubs
                                join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                                join p in ctx.products on pcv.mprodid equals p.mprodid
                                join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid
                                join pv in ctx.productvariationmasters on pcv2.mprodvariationid equals pv.mprodvariationid
                                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                from users in userdata.DefaultIfEmpty()
                                where pcv.mprodsubid != subid && p.mprodid == prodid && users.mstatus == "active" && psupp.mactive=="yes" && pcv2.mprodvariationvalue!=null
                                select new ProductSubVariationDisplayViewModel()
                                {
                                    mprodsubid = pcv.mprodsubid,
                                    mrp = psupp.mrp,
                                    mprodname = p.mprodname,
                                    mprodvariation = pv.mvariation,
                                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                    // pcv2.mprodvariationid,
                                    mprodvariationvalue = pcv2.mprodvariationvalue
                                }).OrderBy(x => x.mrp).ToList().GroupBy(x => x.mprodsubid).Select(y => new ProductSubVariationDisplayViewModel()
                                {

                                    mprodname = y.FirstOrDefault().mprodname,
                                    mprodsubid = y.Key.HasValue?y.Key.Value:0,
                                    mprodvariation = y.FirstOrDefault().mprodvariation,
                                    msupplierid=y.FirstOrDefault().msupplierid,
                                    mprodvariationvalue2 = y.Select(x => x.mprodvariationvalue).AsQueryable()
                                }).ToList();
            /*group new
            {
                psupp.mrp,
                p.mprodname,
                pv.mvariation,
                pcv2.mprodvariationid,
                pcv2.mprodvariationvalue
            }
            by new
            {
                pcv.mprodsubid,

            } into y
            select new ProductSubVariationDisplayViewModel()
            {

                mprodname = y.FirstOrDefault().mprodname,
                mprodsubid = y.Key.mprodsubid,
                mprodvariation = y.FirstOrDefault().mvariation,
                mprodvariationvalue2 = y.Select(x => x.mprodvariationvalue).AsQueryable()
            }).ToList();
            */
            //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            /*if (productvariation != null)
            {
                productvariation.ForEach(x => x.mprodvariationvalue = x.mprodvariationvalue2.AsEnumerable().Select(y => y).Aggregate((c, n) => $"{c},{n}"));
            }*/
            if (productvariation != null)
            {
                productvariation.ForEach(x => {
                    x.optionname = x.optionname + " " + (x.mprodvariationvalue2.AsEnumerable().Select(y => y).Aggregate((c, n) => $"{c},{n}"));
                    x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodsubid && x.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodsubid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodsubid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));

                });
            }
            //return Ok(new { products });
            return Ok(new { products, productdetail, productvariation });
        }
        /*public static int getNumberOfOffer(int prodid, int msupplierid)
        {


            var ctx = new LeasingDbEntities();
            var products = (from pcv in ctx.productsubs
                            join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                            where psub.ProdSubId == prodid && psub.SupplierId != msupplierid
                            select psub).ToList();
            if (products.Count <= 0)
            {
                return 0;

            }
            else
            {
                return products.Count;
            }
        }*/
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
        [HttpGet]
        [Route("api/SingleApi/GetProductDetail")]
        public IHttpActionResult GetProductDetails(int getid2)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid2;
            List<SingleProductDetailViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from p in ctx.products
                                 join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                                 join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                                 join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                                 from pu in pudata.DefaultIfEmpty()
                                 where p.mprodid == id
                                 select new SingleProductDetailViewModel()
                                 {
                                     mprodid = id,
                                     mdetailhead = pd.mdetailhead,
                                     mdetailvalue = pc.mdetailvalue,
                                     muname = pu.munitname
                                 }).ToList();

                //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            }
            if (products == null)
            {
                return NotFound();
            }
            //products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            return Ok(products);
        }
        [HttpGet]
        [Route("api/SingleApi/GetVariationDetail")]
        public IHttpActionResult GetVariationDetails(int getid2,int subid)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid2;
            //var subid = subid;
            List<ProductSubVariationTwoViewModel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from pcv in ctx.productsubs
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid into pcv2data
                            from pcv2 in pcv2data.DefaultIfEmpty()
                            where pcv.mprodsubid != subid && p.mprodid == id
                            group new
                            {
                                pcv2.mprodvariationvalue
                            }
                            by new
                            {
                                pcv.mprodsubid
                            } into y
                            select new ProductSubVariationTwoViewModel()
                            {
                                mprodsubid = y.Key.mprodsubid,
                                mprodvariationvalue2 = y.Select(x => x.mprodvariationvalue).AsQueryable()
                            }).ToList();

                //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            }
            if (products == null)
            {
                return NotFound();
            }
            products.ForEach(x=>x.mprodvariationvalue=x.mprodvariationvalue2.AsEnumerable().Select(y => y).Aggregate((c, n) => $"{c},{n}"));
            //products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            return Ok(products);
        }


        [HttpGet]
        [Route("api/SingleApi/GetMoreSupplierDetail")]
        public IHttpActionResult GetMoreSupplierDetails(int subid,string getprodname,int msupplierid)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid2;
            //var subid = subid;
            List<SingleProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            
                products = (from pcv in ctx.productsubs
                            join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                            where pcv.mprodsubid == subid && psupp.SupplierId!=msupplierid
                            select new SingleProductDisplayViewModel()
                            {
                                mprodvarid = pcv.mprodsubid,
                                mprodid =p.mprodid,
                                mprodname = getprodname,
                                mprodcatid = pc.mprodcatid,
                                mprodcatname = pc.mcatname,
                                mbrand = pbrand.mbrandname,
                                sellerskuid = psupp.sellerskuid,
                                msupplierid=psupp.SupplierId.HasValue?psupp.SupplierId.Value:0,
                                msuppliername =(users.morgname!=null?users.morgname:"Demo Supplier"),
                                mprice=psupp.mrp.HasValue?psupp.mrp.Value:0,
                                mbasicprice= psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                                mtermf=1,
                                mtermp=60

                            }).ToList();

                //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            
            if (products == null)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.msubpicname =ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            //products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            return Ok(products);
        }

        /*[HttpGet]
        [Route("api/SingleApi/OldGetProductById2")]
        public IHttpActionResult OldGetProductById(int getid)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            SingleProductDisplayViewModel products = null;
            var ctx = new LeasingDbEntities();

            products = (
                        from pcv in ctx.productsubs
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid into pcv2data
                        from pcv2 in pcv2data.DefaultIfEmpty()
                        join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where pcv.mprodsubid == id
                        group new
                        {
                            pcv.mprodid,
                            pcv.sellerskuid,
                            pcv.mstock,
                            pcv.mdescription,
                            p.mlengthcm,
                            p.mheightcm,
                            p.mweightcm,
                            p.mwidthcm,
                            p.mdepthcm,
                            pcv.variableproduct,
                            pcv.mrp,
                            pcv.mofferprice,
                            pcv.mvariationprice,
                            pcv2.mprodvariationvalue,
                            pcv.moresupplier,
                            pcv.msupplierid,
                            users.mtown,
                            users.mcity,
                        }
                        by new
                        {
                            pcv.mprodsubid,
                            p.mprodid,
                            p.mprodname,
                            pbrand.mbrandid,
                            pbrand.mbrandname,
                            pc.mcatname,
                            pc.mprodcatid,
                            p.mdescription,
                            users.morgname,
                        } into y
                        orderby y.Key.mprodid
                        select new SingleProductDisplayViewModel()
                        {
                            mprodvarid = y.Key.mprodsubid,
                            mprodid = y.Key.mprodid,
                            mprodname = y.Key.mprodname,
                            mprodcatid = y.Key.mprodcatid,
                            mprodcatname = y.Key.mcatname,
                            mbrandid = y.Key.mbrandid,
                            mbrand = y.Key.mbrandname,
                            sellerskuid = y.FirstOrDefault().sellerskuid,
                            mstock = y.FirstOrDefault().mstock.HasValue ? y.FirstOrDefault().mstock.Value : 0,
                            mheightcm = y.FirstOrDefault().mheightcm.HasValue ? y.FirstOrDefault().mheightcm.Value : 0,
                            mlengthcm = y.FirstOrDefault().mlengthcm.HasValue ? y.FirstOrDefault().mlengthcm.Value : 0,
                            mwidthcm = y.FirstOrDefault().mwidthcm.HasValue ? y.FirstOrDefault().mwidthcm.Value : 0,
                            mdepthcm = y.FirstOrDefault().mdepthcm.HasValue ? y.FirstOrDefault().mdepthcm.Value : 0,
                            mweightcm = y.FirstOrDefault().mweightcm.HasValue ? y.FirstOrDefault().mweightcm.Value : 0,
                            mrp = y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                            mmrpbasicprice = y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                            mprice = y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                            mbasicprice = y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0,
                            mvariationprice = y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : 0,
                            mdescription = y.Key.mdescription,
                            msuppliername = y.Key.morgname + (y.FirstOrDefault().mtown != null ? ", " + y.FirstOrDefault().mtown : "") + (y.FirstOrDefault().mcity != null ? " - " + y.FirstOrDefault().mcity : ""),
                            mtermf = 1,
                            mtermp = 60,
                            msupplierid = (y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0),
                            moresupplier = (byte)(y.FirstOrDefault().moresupplier.HasValue ? y.FirstOrDefault().moresupplier.Value : 0),
                            productsubvariationtwo = y.Select(x => new ProductSubVariationTwoViewModel
                            {
                                mprodvariationvalue = x.mprodvariationvalue,
                            }).AsQueryable()
                        }).SingleOrDefault();

            //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))

            if (products == null)
            {
                return NotFound();
            }
            products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            products.mprodname = products.mprodname + " " + (products.productsubvariationtwo.AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}"));

            var prodid = products.mprodid;
            List<SingleProductDetailViewModel> productdetail = null;

            productdetail = (from p in ctx.products
                             join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                             join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                             join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                             from pu in pudata.DefaultIfEmpty()
                             where p.mprodid == prodid
                             select new SingleProductDetailViewModel()
                             {
                                 mprodid = id,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).ToList();

            var subid = id;
            List<ProductSubVariationTwoViewModel> productvariation = null;
            productvariation = (from pcv in ctx.productsubs
                                join p in ctx.products on pcv.mprodid equals p.mprodid
                                join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid into pcv2data
                                from pcv2 in pcv2data.DefaultIfEmpty()
                                where pcv.mprodsubid != subid && p.mprodid == prodid
                                group new
                                {
                                    pcv2.mprodvariationvalue
                                }
                                by new
                                {
                                    pcv.mprodsubid
                                } into y
                                select new ProductSubVariationTwoViewModel()
                                {
                                    mprodsubid = y.Key.mprodsubid,
                                    mprodvariationvalue2 = y.Select(x => x.mprodvariationvalue).AsQueryable()
                                }).ToList();

            //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            if (productvariation != null)
            {
                productvariation.ForEach(x => x.mprodvariationvalue = x.mprodvariationvalue2.AsEnumerable().Select(y => y).Aggregate((c, n) => $"{c},{n}"));
            }

            //return Ok(new { products });
            return Ok(new { products, productdetail, productvariation });
        }
        */
        [HttpGet]
        [Route("api/SingleApi/GetProductById2")]
        public IHttpActionResult GetProductById2(int getid)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            SingleProductDisplayViewModel products = null;
            var ctx = new LeasingDbEntities();

            products = (from pcv in ctx.productsubs
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where pcv.mprodsubid == id
                        select new SingleProductDisplayViewModel()
                        {
                            mprodvarid = pcv.mprodsubid,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatid = pc.mprodcatid,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            sellerskuid = psupp.sellerskuid,
                            mstock = psupp.msstock.HasValue ? psupp.msstock.Value : 0,
                            mheightcm = p.mheightcm.HasValue ? p.mheightcm.Value : 0,
                            mlengthcm = p.mlengthcm.HasValue ? p.mlengthcm.Value : 0,
                            mwidthcm = p.mwidthcm.HasValue ? p.mwidthcm.Value : 0,
                            mdepthcm = p.mdepthcm.HasValue ? p.mdepthcm.Value : 0,
                            mweightcm = p.mweightcm.HasValue ? p.mweightcm.Value : 0,
                            mrp = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mmrpbasicprice = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mprice = psupp.mofferprice.HasValue ? psupp.mofferprice.Value : psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mbasicprice = psupp.mofferprice.HasValue ? psupp.mofferprice.Value : psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mvariationprice = psupp.mofferprice.HasValue ? psupp.mofferprice.Value : 0,
                            mdescription = pcv.mdescription,
                            msuppliername = users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                            mtermf = 1,
                            mtermp = 60,
                            msupplierid = (pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0),
                            moresupplier = (byte)(pcv.moresupplier.HasValue ? pcv.moresupplier.Value : 0),
                            /*productsubvariationtwo = y.Select(x => new ProductSubVariationTwoViewModel
                            {
                                mprodvariationvalue = x.mprodvariationvalue,
                            }).AsQueryable()*/
                        }).SingleOrDefault();

            //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))

            if (products == null)
            {
                return NotFound();
            }
            products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            products.mprodname = products.mprodname + " " + (ctx.prodsubvariationtwoes.Where(x => x.mprodsubid == products.mprodvarid).Any()? ctx.prodsubvariationtwoes.Where(x => x.mprodsubid == products.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}"):"");

            var prodid = products.mprodid;
            List<SingleProductDetailViewModel> productdetail = null;

            productdetail = (from p in ctx.products
                            join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                            join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                            join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                            from pu in pudata.DefaultIfEmpty()
                            where p.mprodid == prodid
                            select new SingleProductDetailViewModel()
                            {
                                mprodid = id,
                                mdetailhead = pd.mdetailhead,
                                mdetailvalue = pc.mdetailvalue,
                                muname = pu.munitname
                            }).ToList();
                            
            var subid = id;
            List<ProductSubVariationTwoViewModel> productvariation = null;
            productvariation = (from pcv in ctx.productsubs
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            join pcv2 in ctx.prodsubvariationtwoes on pcv.mprodsubid equals pcv2.mprodsubid into pcv2data
                            from pcv2 in pcv2data.DefaultIfEmpty()
                            where pcv.mprodsubid != subid && p.mprodid == prodid
                            group new
                            {
                                pcv2.mprodvariationvalue
                            }
                            by new
                            {
                                pcv.mprodsubid
                            } into y
                            select new ProductSubVariationTwoViewModel()
                            {

                                mprodsubid = y.Key.mprodsubid,
                                mprodvariationvalue2 = y.Select(x => x.mprodvariationvalue).AsQueryable()
                            }).ToList();

                //x => x.mprodcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodcatid.ToString()))
            if (productvariation != null)
            {
                productvariation.ForEach(x => x.mprodvariationvalue = x.mprodvariationvalue2.AsEnumerable().Select(y => y).Aggregate((c, n) => $"{c},{n}"));
            }
            
            //return Ok(new { products });
            return Ok(new { products,productdetail,productvariation });
        }
        [HttpGet]
        [Route("api/SingleProductApi/GetProductRatings")]
        public IHttpActionResult GetProductRatings(int getid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<ProductRatingViewModelWithImage> productrating = null;
            var ctx = new LeasingDbEntities();
                productrating = (from pr in ctx.productreviews
                                 join pri in ctx.productreviewpics on pr.mreviewid equals pri.mreviewid into pridata
                                 from pri in pridata.DefaultIfEmpty()
                                 join usr in ctx.user_registration_more on pr.muserid equals usr.musermoreid into usrdata
                                 from usr in usrdata.DefaultIfEmpty()
                                 where pr.mprodid == getid && pr.mactive==1
                                 group new{
                                     pri.mpicname,
                                     pr.mrankno,
                                     pr.mreviewtitle,
                                     pr.mcomments,
                                     usr.morgname,
                                     pr.mreviewdate
                                 } by new
                                {
                                    pr.mreviewid
                                    
                                } into y
                                orderby y.Key.mreviewid
                                 select new ProductRatingViewModelWithImage
                                 {
                                     mreviewid = y.Key.mreviewid,
                                     mrankno = y.FirstOrDefault().mrankno,
                                     mreviewtitle = y.FirstOrDefault().mreviewtitle,
                                     mcomments = y.FirstOrDefault().mcomments,
                                     mreviewdate=y.FirstOrDefault().mreviewdate,
                                     musername=y.FirstOrDefault().morgname,
                                     mimages=y.Select(x=>new ProductReviewImage
                                     {
                                         mpicname=x.mpicname,
                                     }).ToList(),
                                 }).ToList();

                if (productrating.Any())
                {
                    productrating.ForEach(x => x.mdreviewdate = x.mreviewdate.HasValue?x.mreviewdate.Value.ToString("dd/MM/yyyy"):"");
                    return Ok(new { productrating = productrating});
                }
                else
                {
                    return Ok("empty");
                }


        }
        [HttpGet]
        [Route("api/SingleProductApi/GetProductSupplierRatings")]
        public IHttpActionResult GetProductSupplierRatings(int getid,int supid)
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<ProductSupplierRatingViewModelWithImage> productsupplierrating = null;
            var ctx = new LeasingDbEntities();
            productsupplierrating = (from pr in ctx.productreviewbysuppliers
                             where pr.mprodid == getid && pr.msupplierid == supid
                             select new ProductSupplierRatingViewModelWithImage
                             {
                                 mpicname = pr.mpicname
                             }).ToList();

            if (productsupplierrating.Any())
            {
                //productrating.ForEach(x => x.mdreviewdate = x.mreviewdate.HasValue ? x.mreviewdate.Value.ToString("dd/MM/yyyy") : "");
                return Ok(new { productsupplierrating = productsupplierrating });
            }
            else
            {
                return Ok("empty");
            }


        }
    }
}
