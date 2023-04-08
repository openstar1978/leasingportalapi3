using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace LeasingPortalApi.Controllers.Api
{
    public class OrderApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetOrders()
        {
            //var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<UserQuotationMasterViewModeel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from q in ctx.userordermasters
                            select new UserQuotationMasterViewModeel
                            {
                                mquoteid = q.morderid,
                                createddate = q.createddate,

                            }).ToList();

            }
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/OrderApi/GetOldOrderById")]
        public IHttpActionResult GetOldOrderById(int getid)
        {
            var id = getid;
            var seed = string.Empty;

            List<UserQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var orderdatas = ctx.userordermasters.FirstOrDefault(x => x.morderid == id);
            var proddatas = orderdatas.mprodlist.Split(',');
            products = (from pc in ctx.usersubquotations
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where pc.type == "o" && proddatas.Any(s => s == pc.msubquoteid.ToString())
                        select new UserQuotationViewModel()
                        {

                            type = pc.type,
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            mquotestatus = pc.mstatus,
                            mnotes = pc.mnotes,

                        }).ToList<UserQuotationViewModel>();



            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(z =>
            {

                z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";

            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/OrderApi/GetLeaseAggreementId")]
        public IHttpActionResult GetLeaseAggreementId(int getid)
        {
            var id = getid;
            var seed = string.Empty;

            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.userleaseagreements
                        where pc.muserid == getid
                        select new LeaseAggreementViewModel()
                        {
                            total = pc.total,
                            mleaseno = pc.mleaseno,
                            createddate = pc.createddate,
                        }).ToList<LeaseAggreementViewModel>();



            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(z =>
            {

                z.displaydate = z.createddate.HasValue ? z.createddate.Value.ToString("dd/MM/yyyy") : "";

            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/OrderApi/GetOrderById")]
        public IHttpActionResult GetOrderById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<UserOrdersViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        where p.muserid == id && pc.type == "o"
                        select new UserOrdersViewModel()
                        {

                            type = pc.type,
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            mquotestatus = pc.mstatus,
                            mnotes = pc.mnotes,
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                        }).ToList<UserOrdersViewModel>();




            if (products.Count == 0)
            {
                return Ok(products);
            }

            products.ForEach(x =>
            {
                var orderdata = GetOrderRef(ctx, x.msubquoteid);
                x.mnotes = (x.mnotes == null ? orderdata.monotes : x.mnotes);
                x.ccreatedate = orderdata.ccreatedate;
                x.orderref = orderdata.orderref;
                x.morderid = orderdata.orderid;
                x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";

            });

            return Ok(products.OrderByDescending(x => x.morderid).ToList());
        }
        public SmallOrderViewModer GetOrderRef(LeasingDbEntities ctx, int id)
        {
            SmallOrderViewModer data2 = null;
            var sid = id.ToString();
            var data = (from a in ctx.userordermasters
                        where a.mprodlist.Contains(sid)
                        select a).ToList().Where(a => a.mprodlist.Split(',').Contains(sid)).FirstOrDefault();
            //ctx..Where(x => x.morderstatus.Contains("," + sid) || x.morderstatus.Contains(sid+",")).FirstOrDefault();
            if (data != null)
            {
                data2 = new SmallOrderViewModer
                {
                    monotes = data.mnotes,
                    orderid = data.morderid,
                    orderref = data.morderref,
                    ccreatedate = data.createddate.HasValue ? data.createddate.Value.ToString("dd/MM/yyyy") : ""
                };
            }
            return data2;
        }
        [HttpPost]
        [Route("api/OrderApi/updateOrderStatus")]
        public IHttpActionResult updateOrderStatus(DeliveryStatusViewModel data)
        {
            var db = new LeasingDbEntities();
            var searchdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == data.msubquoteid);
            if (searchdata != null)
            {
                searchdata.mstatus = "confirmed";
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                var obj = new deliverystatu
                {
                    morderid = data.morderid,
                    mreceivedate = data.mreceivedate,
                    mcondition = data.mcondition,
                    msubquoteid = data.msubquoteid,
                    muserid = data.muserid,
                    createdate = DateTime.Now,
                };
                db.deliverystatus.Add(obj);
                db.SaveChanges();
                return Ok("Save");
            }
            else
            {
                return Ok("Not Updated");
            }
        }
        [HttpGet]
        [Route("api/OrderApi/GetNewOrder")]
        public IHttpActionResult GetNewOrder(int getid)
        {
            var id = getid;
            var seed = string.Empty;

            var products = new List<DisplayOrderViewModel>();
            var ctx = new LeasingDbEntities();
            products = (from ord in ctx.userordermasters
                        join pc in ctx.usersubquotations on ord.morderref equals pc.morderno
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join sup in ctx.supplier_registration on pc.msupplierid equals sup.msuid into supdatas
                        from sup in supdatas.DefaultIfEmpty()
                        where ord.muserid == id && (pc.magreementno==null || pc.magreementno==0)
                        group new
                        {
                            ord.msorderref,
                            sup.morgname,
                            sup.mcity,
                            sup.mcounty,
                            ord.createddate,
                            pc.magreementno,
                            pc.morderno,
                            pc.type,
                            prod.mprodname,
                            pc.mprice,
                            pc.msubquoteid,
                            pc.mtermf,
                            pc.mtermp,
                            pc.quantity,
                            pc.mprodvarid,
                            pc.mnotes,
                            pc.mstatus
                        }
                               by new
                               {
                                   ord.morderref,
                               } into y
                        select new DisplayOrderViewModel()
                        {
                            msorderref=y.FirstOrDefault().msorderref,
                            morderref = y.Key.morderref,
                            ddate=y.FirstOrDefault().createddate,
                            agreement = (y.Where(x=>x.magreementno==null || x.magreementno==0).Any())?"":"Generated",
                            
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                magreementno=x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                suppliername = x.morgname + (x.mcity != null ? ", " + x.mcity : "") + (x.mcounty != null ? ", " + x.mcounty : ""),
                            }).ToList(),

                        }).ToList();
                      


            if (products.Count == 0)
            {
                return Ok(products);
            }
            products = products.Where(x => x.agreement == "").ToList();
            products.ForEach(z =>
            {
                z.createdate = z.ddate.HasValue ? z.ddate.Value.ToString("dd/MM/yyyy") : "";
                z.Product.ForEach(k =>
                {
                    k.mproductname = k.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == k.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == k.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    k.mproducturl = "productname=" + k.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(k.mprodvarid.ToString()));
                    k.subpicname = ctx.productsubpics.FirstOrDefault(h => h.mprodsubid == k.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(h => h.mprodsubid == k.mprodvarid).msubprodpicname : "";
                });


            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/OrderApi/GetSchoolOrderRef")]
        public IHttpActionResult GetSchoolOrderRef(int getid)
        {
            var id = getid;
            var seed = string.Empty;

            var products = "";
            var ctx = new LeasingDbEntities();
            var productdata = ctx.userordermasters.FirstOrDefault(x => x.morderref == getid);
            if (productdata != null)
            {
                products = productdata.msorderref;
            }


            
            return Ok(products);
        }
    }
}
