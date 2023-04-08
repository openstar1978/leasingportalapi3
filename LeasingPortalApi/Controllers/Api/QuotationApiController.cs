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
    public class QuotationApiController : ApiController
    {
        [HttpGet]
        [Route("api/QuotationApi/GetTotalNumberId")]
        public IHttpActionResult GetTotalNumberId(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<string> products = new List<string>() ;
            var ctx = new LeasingDbEntities();
            var d = "0";var d2 = "0";var d3 = "0";var d4 = "0"; var d5="0";var d6 = "";var d7 = "";
            var data = (from pc in ctx.usersubquotations
                        join p in ctx.userleaseagreements on pc.magreementno equals p.mleaseno
                       
                        where p.muserid == id && (pc.mstatus == "pending") 
                        select pc).ToList();
            if (data.Any())
            {
                d = data.Count.ToString();
            }
            /*d2= (from pc in ctx.usersubquotations
                     join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                     where p.muserid == id && (pc.mstatus == "printed")
                     select pc).ToList().Count.ToString();

            d3 = (from pc in ctx.usersubquotations
                     join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                     where p.muserid == id && (pc.mstatus == "approved")
                     select pc).ToList().Count.ToString();
            d4 = (from pc in ctx.usersubquotations
                      join p in ctx.userordermasters on pc.morderno equals p.morderref
                      where p.muserid == id && pc.type=="o" && (pc.magreementno==0 || pc.magreementno==null)
                      select pc ).GroupBy(x=>x.morderno).ToList().Count.ToString();
            */
            var leasedata = (from pc in ctx.userleaseagreements
                             where pc.muserid == id
                             select pc).ToList();
            d5= leasedata.Where(x=>x.mstatus=="partial").ToList().Count.ToString();
            d6 = leasedata.Where(x => x.mstatus != "partial" && x.mstatus!="completed").ToList().Count.ToString();
            d7 = leasedata.Where(x => x.mstatus == "completed").ToList().Count.ToString();
            products.Add(d5);
            products.Add(d);
            products.Add(d4);
            products.Add(d6);
            products.Add(d7);
            //products.Add(d2);

            //products.Add(d3);
            //

            return Ok(products);
        }
        [HttpGet]
        [Route("api/QuotationApi/GetTotalPendingId")]
        public IHttpActionResult GetTotalPendingId(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<string> products = new List<string>();
            var ctx = new LeasingDbEntities();
            var d = "0"; var d2 = "0"; var d3 = "0"; var d4 = "0"; var d5 = "0";
            var data = (from pc in ctx.usersubquotations
                        join p in ctx.userleaseagreements on pc.magreementno equals p.mleaseno
                        where p.muserid == id && (pc.mstatus == "pending")
                        select pc).ToList();
            if (data.Any())
            {
                d = data.Count.ToString();
            }
            products.Add(d);
            return Ok(products);
        }
        [HttpGet]
        public IHttpActionResult GetQuotes()
        {
            //var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<UserQuotationMasterViewModeel> products = null;
            using (var ctx = new LeasingDbEntities())
            {
                products = (from q in ctx.userquotationmasters
                            where q.mquotestatus == "pending"
                            select new UserQuotationMasterViewModeel
                            {
                                mquoteid = q.mquoteid,
                                mquoteref = q.mquoteref,
                                mquotestatus = q.mquotestatus,
                                mquotetotal = q.mquotetotal,
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
        [Route("api/QuotationApi/GetQuoteById")]
        public IHttpActionResult GetQuotesById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<UserQuotationNewViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        where p.muserid == id && (pc.mstatus == "pending")
                        select new UserQuotationNewViewModel()
                        {
                            muserid=id,
                            mquoteid = p.mquoteid,
                            type = pc.type,
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            mquotestatus = pc.mstatus,
                            msupplierid=pc.msupplierid,                                   
                            createddate = p.createddate,
                            noncancellable=pcv.noncancellable,
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                        }).OrderByDescending(x => x.msubquoteid).ToList();
                        



            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.mleadtime = GetLeadTime(x.mprodvarid, x.msupplierid);
                x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + x.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";
                    
            });

            return Ok(products);
        }
        public int GetLeadTime(int? mprodvarid,int? msupplierid)
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
        [HttpGet]
        [Route("api/QuotationApi/GetPrintedQuoteById")]
        public IHttpActionResult GetPrintedQuotesById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<UserQuotationNewViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        where p.muserid == id && (pc.mstatus == "printed")
                        select new UserQuotationNewViewModel()
                        {

                            mquoteid = p.mquoteid,
                            type = pc.type,
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            mquotestatus = pc.mstatus,
                            msupplierid=pc.msupplierid,
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                            createddate = p.createddate,
                            
                        }).OrderByDescending(x => x.msubquoteid).ToList();




            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";

            });

            return Ok(products);
        }
        /************************OLD CODE OF QUOTES*********************************/
        [HttpGet]
        [Route("api/QuotationApi/GetQuoteOldById")]
        public IHttpActionResult GetQuotesOldById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<SingleQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from p in ctx.userquotationmasters
                        join pc in ctx.usersubquotations on p.mquoteid equals pc.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where p.muserid == id  
                        group new
                        {
                            pc.type,
                            p.mquotetotal,
                            pc.msubquoteid,
                            pc.mprice,
                            pc.mtermf,
                            pc.mtermp,
                            pc.quantity,
                            pc.mstatus,
                            pc.mprodvarid,
                            prod.mprodname
                        }
                        by new
                        {
                            p.mquoteid,
                        } into y
                        orderby y.Key.mquoteid
                        select new SingleQuotationViewModel()
                        {
                            mquoteid = y.Key.mquoteid,
                            mquotetotal = y.FirstOrDefault().mquotetotal,
                            mstatus = y.Where(z => z.mstatus == "approved").FirstOrDefault().mstatus,
                                Product = y.Select(x => new UserQuotationViewModel
                                {
                                    type = x.type,
                                    mproductname =x.mprodname,
                                    mprice=x.mprice,
                                    msubquoteid=x.msubquoteid,
                                    mtermf=x.mtermf,
                                    mtermp=x.mtermp,
                                    quantity=x.quantity,
                                    mprodvarid=x.mprodvarid,
                                    mquotestatus=x.mstatus
                                }).ToList()
                            }).ToList<SingleQuotationViewModel>();


            
            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.Product.ForEach(

                    z => {
                       z.mproductname= z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n=>n.mprodsubid==z.mprodvarid).Any()? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}"):"");
                        z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                        });
                
            });
            
            return Ok(products);
        }
        [HttpGet]
        [Route("api/QuotationApi/deleteSubQuoteById")]
        public IHttpActionResult deleteSubQuoteById(int getid)
        {
            var db = new LeasingDbEntities();
            var searchdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == getid && (x.mstatus=="pending" || x.mstatus=="printed"));
            if (searchdata != null)
            {
                var searchsub = db.usercomparesubquotations.Where(x => x.mcompareprodid == getid);
                if (searchsub.Any())
                {
                    db.usercomparesubquotations.RemoveRange(searchsub);
                    db.SaveChanges();
                }
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return Ok("Delete");
            }
            else
            {
                return Ok("Not Delete");
            }  
        }
        [HttpGet]
        [Route("api/QuotationApi/DeleteCompareProduct")]
        public IHttpActionResult DeleteCompareProduct(int id)
        {
            var db = new LeasingDbEntities();
            

            if (id != 0)
            {
                //foreach(var datas in datas2)
                //{
                var searchsub = db.usercomparesubquotations.FirstOrDefault(x => x.Id == id);
                if (searchsub != null)
                {


                    db.Entry(searchsub).State = System.Data.Entity.EntityState.Deleted;
                    db.SaveChanges();
                }
                return Ok("delete");
            }


            return Ok("failed");

        }
        [HttpPost]
        [Route("api/QuotationApi/updateQuoteDatas")]
        public IHttpActionResult updateQuoteDatas(UserQuotationNewViewModel datas)
        {
            var db = new LeasingDbEntities();
            if (datas != null)
            {
                var searchdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == datas.msubquoteid);
                if (searchdata != null)
                {
                    searchdata.mprodvarid = datas.mprodvarid;
                    searchdata.mtermf = datas.mtermf;
                    searchdata.mtermp = datas.mtermp;
                    searchdata.mprice = datas.mprice.HasValue ? Math.Round(datas.mprice.Value, 2) : 0;
                    searchdata.quantity = datas.quantity;
                    searchdata.msupplierid = datas.msupplierid;
                    
                    db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return Ok("Updated");
                }
                else
                {
                    return Ok("Not Updated");
                }
            }
            else
            {
                return Ok("Not Updated");
            }
            
        }
        [HttpGet]
        [Route("api/QuotationApi/updateQuoteStatus")]
        public IHttpActionResult updateQuoteStatus(int getid,string status,byte? flexible,int muserid)
        {
            var db = new LeasingDbEntities();
            var searchdata = db.usersubquotations.FirstOrDefault(x => x.msubquoteid == getid);
            if (searchdata != null)
            {
                searchdata.mstatus = status;
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Ok("Updated");
            }
            else
            {
                return Ok("Not Updated");
            }
        }

        /*******************************Get Compare Product data***********************************************************/
        [HttpGet]
        [Route("api/QuotationApi/GetCompareProductData")]
        public IHttpActionResult GetCompareProductData(int id)
        {
            var ctx = new LeasingDbEntities();
            var d = new List<SubCartDisplayViewModel>();

            var products = (from pc in ctx.usersubquotations
                        join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                        where p.muserid == id && (pc.type == "q")
                        select new 
                        {
                            msubquoteid = pc.msubquoteid,
                            mtermf=pc.mtermf,
                            mtermp=pc.mtermp   
                        }).OrderByDescending(x => x.msubquoteid).ToList();
            if (products.Any())
            {
                foreach(var ps in products)
                {
                    //var datas = ctx.usersubquotations.FirstOrDefault(x => x.msubquoteid == ps.msubquoteid);
                    //if (datas != null)
                    {
                        var d2 = (from ab in ctx.usercomparesubquotations
                                  join pcv in ctx.productsubs on ab.msubquoteid equals pcv.mprodsubid
                                  join p in ctx.products on pcv.mprodid equals p.mprodid
                                  join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                  join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                  join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                                  join users in ctx.supplier_registration on pcv.msupplierid equals users.msuid into userdata
                                  from users in userdata.DefaultIfEmpty()
                                  let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * ps.mtermf) / (ps.mtermp * 0.9) : 0
                                  where ab.mcompareprodid ==ps.msubquoteid && ab.msupplierid == psupp.SupplierId && pcv.mprodsubid == ab.msubquoteid
                                  select new SubCartDisplayViewModel()
                                  {
                                      Id = ab.Id,
                                      mprodid = ps.msubquoteid,

                                      mprodname = p.mprodname,
                                      mtermf = ps.mtermf.HasValue ? ps.mtermf.Value : 1,
                                      mtermp = ps.mtermp.HasValue ? ps.mtermp.Value : 60,
                                      suppliername = users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
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
                            var d3 = (from ab in ctx.usercomparesubquotations
                                      join pcv in ctx.productsubs on ab.msubquoteid equals pcv.mprodsubid
                                      join psub in ctx.ProductSubSuppliers on pcv.mprodsubid equals psub.ProdSubId
                                      join p in ctx.products on pcv.mprodid equals p.mprodid
                                      join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                      join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                      join users in ctx.supplier_registration on psub.SupplierId equals users.msuid into userdata
                                      from users in userdata.DefaultIfEmpty()
                                      let mprice = psub.mofferprice.HasValue ? (psub.mofferprice.Value * ps.mtermf) / (ps.mtermp * 0.9) : 0
                                      where ab.mcompareprodid == ps.msubquoteid && pcv.mprodsubid == ab.msubquoteid && pcv.moresupplier == 1 && psub.SupplierId == ab.msupplierid
                                      select new SubCartDisplayViewModel()
                                      {
                                          Id = ab.Id,
                                          mprodid = ps.msubquoteid,
                                          mprodname = p.mprodname,
                                          mtermf = ps.mtermf.HasValue ? ps.mtermf.Value : 1,
                                          mtermp = ps.mtermp.HasValue ? ps.mtermp.Value : 60,
                                          suppliername = users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
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

                    //foreach(var datas in datas2)
                    //{
                   

                }
                d.ForEach(x =>
                {
                    x.mproducturl = "productname=" + x.mprodname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString()));
                    x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
                });
                return Ok(d.ToList());
            }
            return Ok("failed");

        }
        [HttpPost]
        [Route("api/QuotationApi/updateRelatedProduct")]
        public IHttpActionResult updateRelatedProduct(List<SubCartDisplayViewModel> datas)
        {
            var db = new LeasingDbEntities();
            if (datas.Any())
            {
                foreach(var data in datas)
                {
                    var searchdata = db.usercomparesubquotations.FirstOrDefault(x => x.Id == data.Id);
                    if (searchdata != null)
                    {
                        searchdata.msubquoteid = data.mprodvarid;
                        searchdata.msupplierid = data.msupplierid;
                        db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return Ok("Updated");
                    }
                    else
                    {
                        return Ok("Not Updated");
                    }

                }
                return Ok("Updated");
            }
            else
            {
                return Ok("Not Updated");
            }

        }
        /***********************************************************************************/
        [HttpGet]
        [Route("api/QuotationApi/GetApproveQuoteById")]
        public IHttpActionResult GetApproveQuoteById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<UserQuotationNewViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join p in ctx.userquotationmasters on pc.mquoteid equals p.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join pbrand in ctx.productbrands on prod.mbrand equals pbrand.mbrandid into pbranddata
                        from pbrand in pbranddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        where p.muserid == id &&  pc.mstatus == "approved"
                        select new UserQuotationNewViewModel()
                        {
                            Make = pbrand.mbrandname,
                            Condition = pcv.mcondition,
                            mquoteid = p.mquoteid,
                            type = pc.type,
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            mquotestatus = pc.mstatus,
                            Selected=false,
                            createddate = p.createddate,
                            msupplierid=pc.msupplierid,
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                            
                        }).OrderByDescending(x => x.msubquoteid).ToList();




            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == x.mprodvarid).msubprodpicname : "";
                x.ccreatedate = x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "";

            });

            return Ok(products);
        }

        /****************************************OLd CODE *******************************************/
        [HttpGet]
        [Route("api/QuotationApi/GetApproveQuoteByOlddId")]
        public IHttpActionResult GetApproveQuoteByOldId(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<SingleQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from p in ctx.userquotationmasters
                        join pc in ctx.usersubquotations on p.mquoteid equals pc.mquoteid
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where p.mquoteid == id && pc.mstatus == "approved"
                        group new
                        {
                            p.muserid,
                            pc.type,
                            p.mquotetotal,
                            pc.msubquoteid,
                            pc.mprice,
                            pc.mtermf,
                            pc.mtermp,
                            pc.quantity,
                            pc.mstatus,
                            pc.mprodvarid,
                            prod.mprodname
                        }
                        by new
                        {
                            p.mquoteid,
                        } into y
                        orderby y.Key.mquoteid
                        select new SingleQuotationViewModel()
                        {
                            muserid = y.FirstOrDefault().muserid.HasValue ? y.FirstOrDefault().muserid.Value : 0,
                            mquoteid = y.Key.mquoteid,
                            mquotetotal = y.FirstOrDefault().mquotetotal,
                            mstatus = y.Where(z => z.mstatus == "approved").FirstOrDefault().mstatus,
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus
                            }).ToList()
                        }).ToList<SingleQuotationViewModel>();



            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.encmquoteid = Encryption.Encrypt(x.mquoteid.ToString());
                x.Product.ForEach(

                    z => {
                        z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                        z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                    });

            });

            return Ok(products);
        }
    }
}
