using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LeasingPortalApi.Models;

namespace LeasingPortalApi.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public ActionResult GetCart()
        {
            return Json(SessionWrapper.Cart, JsonRequestBehavior.AllowGet);
            
        }
        [HttpPost]
        public ActionResult Remove(int index)
        {
            List<Items> cart = (List<Items>)SessionWrapper.Cart;
            //int index = isExist(id);
            cart.RemoveAt(index);
            SessionWrapper.Cart = cart;
            return Content("success");
        }
        public ActionResult Empty()
        {
            List<Items> cart = (List<Items>)SessionWrapper.Cart;
            //int index = isExist(id);

            SessionWrapper.Cart = null;
            return Content("succcess");
        }
        
        /*[HttpGet]
        public ActionResult GetCart()
        {
            return Json(Session["cart"],JsonRequestBehavior.AllowGet);
        }*/

        [HttpPost]
        public ActionResult Buy(ProductDisplayViewModel item)
        {
            ProductDisplayViewModel productModel = new ProductDisplayViewModel();
            if (Session["cart"] == null)
            {
                List<Items> cart = new List<Items>();
                cart.Add(new Items { Product = ProductDao.GetProductById(item.mprodid), Quantity = 1 });
                Session["cart"] = cart;
                return Json(Session["cart"]); 
            }
            else
            {
                List<Items> cart = (List<Items>)Session["cart"];
                int index = isExist(item.mprodid);
                if (index != -1)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    cart.Add(new Items { Product = ProductDao.GetProductById(item.mprodid), Quantity = 1 });
                }
                Session["cart"] = cart;
                return Json(Session["cart"]);
            }
            return RedirectToAction("Index");
        }
        /*[HttpPost]
        public ActionResult Buy(Items item)
        {
            ProductDisplayViewModel productModel = new ProductDisplayViewModel();
            if (Session["cart"] == null)
            {
                List<Items> cart = new List<Items>();
                cart.Add(new Items { Product =item.Product, Quantity = item.Quantity });
                Session["cart"] = cart;
                return Json(Session["cart"]);
            }
            else
            {
                List<Items> cart = (List<Items>)Session["cart"];
                int index = isExist(item.Product.mprodid);
                if (index != -1)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    cart.Add(new Items { Product = item.Product, Quantity = item.Quantity });
                }
                Session["cart"] = cart;
                return Json(Session["cart"]);
            }
            return RedirectToAction("Index");
        }*/
        /*[HttpPost]
        public ActionResult Remove(int index)
        {
            List<Items> cart = (List<Items>)Session["cart"];
            //int index = isExist(id);
            cart.RemoveAt(index);
            Session["cart"] = cart;
            return Content("success");
        }*/
       /* public ActionResult Empty()
        {
            List<Items> cart = (List<Items>)Session["cart"];
            //int index = isExist(id);
            
            Session["cart"] =null;
            return Content("succcess");
        }*/
        private int isExist(int id)
        {
            List<Items> cart = (List<Items>)Session["cart"];
            for (int i = 0; i < cart.Count; i++)
                if (cart[i].Product.mprodid.Equals(id))
                    return i;
            return -1;
        }

        [HttpGet]
        public JsonResult GetProductById(int getid)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            SingleProductDisplayViewModel products = null;
            var ctx = new LeasingDbEntities();

            products = (
                        from pcv in ctx.productsubs
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

            products.mproducturl = "Category=" + products.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(products.mprodcatid.ToString()));
            products.mprodname = products.mprodname + " " + (ctx.prodsubvariationtwoes.Where(x => x.mprodsubid == products.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}"));

            var prodid = products.mprodid;
            // List<SingleProductDetailViewModel> productdetail = null;

            /*    productdetail = (from p in ctx.products
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
                            */
            var subid = id;
            //List<ProductSubVariationTwoViewModel> productvariation = null;
            /*productvariation = (from pcv in ctx.productsubs
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
            */
            return Json(new { products }, JsonRequestBehavior.AllowGet);
            //return Ok(new { products,productdetail,productvariation });
        }
    }
}