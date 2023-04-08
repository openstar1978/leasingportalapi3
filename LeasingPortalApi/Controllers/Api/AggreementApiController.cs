using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Net.Http.Headers;
using System.Web;
using System.Data.Common;

namespace LeasingPortalApi.Controllers.Api
{
    public class AggreementApiController : ApiController
    {
        [HttpGet]
        [Route("api/AggreementApi/GetAggreementById")]
        public IHttpActionResult GetAggreementById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            /*var p = ctx.Database.SqlQuery<LeaseAggreementNewViewModel>("EXECUTE dbo.GetAgreementsById "+getid).ToList();
            products = p
        .GroupBy(x => x.mleaseno)
        .Select(y => new LeaseAggreementViewModel
        {
            mest = y.FirstOrDefault().mest,
            msorderref = y.FirstOrDefault().msorderref,
            mpayment = y.FirstOrDefault().mpayment,
            mprintby = y.FirstOrDefault().mname,
            mprintdate = y.FirstOrDefault().mprintdate,
            mleaseid = y.FirstOrDefault().mleaseid,
            mleaseno = y.FirstOrDefault().mleaseno,
            total = y.FirstOrDefault().total,

            createddate = y.FirstOrDefault().createddate,
            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
            Product = y.Select(x => new UserQuotationViewModel
            {

                morderno = x.morderno,
                magreementno = x.magreementno,
                type = x.type,
                mproductname = x.mprodname,
                mprice = x.mprice,
                msubquoteid = x.msubquoteid,
                mtermf = x.mtermf,
                mtermp = x.mtermp,
                mvat = x.mvat.HasValue ? x.mvat.Value : 0,
                quantity = x.quantity,
                mprodvarid = x.mprodvarid,
                mquotestatus = x.mstatus,
                mnotes = x.mnotes,
                suppliername = x.morgname + (x.mtown != null ? ", " + x.mtown : "") + (x.mcity != null ? " - " + x.mcity : ""),
            }).ToList(),
        }).ToList();*/
            products = (from ag in ctx.GetAgreementViews
                where ag.muserid == getid && (ag.magreementno > 0) && (ag.signstatus=="partial") && ag.menvelopeid != null
                        group new
                        {
                            ag.mmanufacturer,
                            ag.menvelopeid,
                            ag.signstatus,
                            ag.mest,
                            ag.msorderref,
                            ag.mpayment,
                            ag.mleaseid,
                            ag.msend,
                            ag.total,
                            ag.mname,
                            ag.mleaseno,
                            ag.mprintdate,
                            ag.morgname,
                            ag.mcity,
                            ag.town,
                            ag.createddate,
                            ag.magreementno,
                            ag.morderno,
                            ag.type,
                            ag.mprodname,
                            ag.mprice,
                            ag.msubquoteid,
                            ag.mtermf,
                            ag.mtermp,
                            ag.mvat,
                            ag.quantity,
                            ag.mprodvarid,
                            ag.mnotes,
                            ag.mstatus,
                            ag.msupplierid,
                            createdby=ag.Expr1,
                            ag.minstallationflag,
                            ag.warranty,
                            ag.warrantyterm,
                            ag.warrrantyprice,
                        }
                       by new
                       {
                           ag.mleaseno,
                       } into y
                        select new LeaseAggreementViewModel
                        {
                            mest = y.FirstOrDefault().mest,
                            msorderref = y.FirstOrDefault().msorderref,
                            mpayment = y.FirstOrDefault().mpayment,
                            //mprintby = y.FirstOrDefault().mname,
                            mprintby=y.FirstOrDefault().createdby,
                            mprintdate = y.FirstOrDefault().mprintdate,
                            mleaseid = y.FirstOrDefault().mleaseid,
                            mleaseno = y.Key.mleaseno,
                            total = y.FirstOrDefault().total,
                            menvelopid = y.FirstOrDefault().menvelopeid,
                            createddate = y.FirstOrDefault().createddate,
                            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                msupplierid=x.msupplierid,
                                morderno = x.morderno,
                                magreementno = x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                mvat = x.mvat.HasValue ? x.mvat.Value : 0,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                minstallationflag=x.minstallationflag.HasValue?x.minstallationflag.Value==1?"yes":"no":"no",
                                warrantydetails=new WarrantyCartViewModel
                                {
                                    mprodname=x.warranty,
                                    mtermp=x.warrantyterm,
                                    mprice=x.warrrantyprice,
                                },
                                suppliername =x.mmanufacturer, //x.morgname + (x.mcity != null ? ", " + x.mcity : "")+(x.town != null ? ", " + x.town : ""),
                            }).ToList(),
                        }).ToList();
                        
            products.ForEach(x =>
            {
                x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                x.mdocname = getDocDetail(x.mleaseno);
                //x.malloc = getAllocatSt(x.Product, ctx);
                x.Product.ForEach(z => {
                    z.status = getAllocatStProd(z.msubquoteid, z.msupplierid, ctx);
                    z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : "");
                    z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                    z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                    z.warrantydetails = getWarranty(z.msubquoteid, z.mproductname, ctx);
                });
                x.malloc = x.Product.Where(k => k.status == "p").Any() ? "p" : x.Product.Where(k => k.status == "a" || k.status == "sd" || k.status=="rd").Any() ? "a" : x.Product.Where(k => k.status == "di").Any() ? "di" : x.Product.Where(k => k.status == "de").Any() ? "de":x.Product.Where(k => k.status == "c").Any() ? "c" : "p";
            });

            return Ok(products.OrderByDescending(x=>x.createddate).ToList());
        }
        public static WarrantyCartViewModel getWarranty(int cartid, string mprodname, LeasingDbEntities db)
        {
            var p = new WarrantyCartViewModel();
            p = (from pcv in db.usersubquotationwarranties
                 join prodws in db.productsubwarrantybysuppliers on pcv.msubwarrantid equals prodws.subwarrantid
                 join prodw in db.productwarrantybysuppliers on prodws.warrantyid equals prodw.WarrantyId
                 where pcv.msubquoteid == cartid
                 select new WarrantyCartViewModel
                 {
                     mprodname = pcv.mprodname,
                     mtermp = pcv.mtermp,
                     mprice = prodws.warrantyprice,
                     subwarrantid = prodws.subwarrantid,
                     mtermf = pcv.mtermf,
                     
                 }).FirstOrDefault();
            return p;
        }
        [HttpPost]
        [Route("api/AggreementApi/GetPendingSignAggreementByMultipleIds")]
        public IHttpActionResult GetPendingSignAggreementByMultipleIds(List<int> getids)
        {
            if (getids!=null && getids.Count > 0)
            {
                var ctx = new LeasingDbEntities();
                List<LeaseAggreementViewModel> products = null;
                //foreach (var getid in getids)
                //{
                    //var id = getid;
                    var seed = string.Empty;
                    
                    products = (from ag in ctx.GetAgreementViews
                                where getids.Any(x=>x==ag.mleaseno) && (ag.magreementno > 0) && (ag.signstatus != "partial" && ag.signstatus != "completed" && ag.menvelopeid==null)
                                group new
                                {
                                    ag.mmanufacturer,
                                    ag.maddress,
                                    ag.mdefaultadd,
                                    ag.menvelopeid,
                                    ag.signstatus,
                                    ag.mest,
                                    ag.msorderref,
                                    ag.mpayment,
                                    ag.mleaseid,
                                    ag.msend,
                                    ag.total,
                                    ag.mname,
                                    ag.mleaseno,
                                    ag.mprintdate,
                                    ag.morgname,
                                    ag.mcity,
                                    ag.town,
                                    ag.createddate,
                                    ag.magreementno,
                                    ag.morderno,
                                    ag.type,
                                    ag.mprodname,
                                    ag.mprice,
                                    ag.msubquoteid,
                                    ag.mtermf,
                                    ag.mtermp,
                                    ag.mvat,
                                    ag.quantity,
                                    ag.mprodvarid,
                                    ag.mnotes,
                                    ag.mstatus,
                                    ag.msupplierid,
                                    createdby = ag.Expr1,
                                    ag.minstallationflag,
                                    ag.warranty,
                                    ag.warrantyterm,
                                    ag.warrrantyprice,
                                }
                               by new
                               {
                                   ag.mleaseno,
                               } into y
                                select new LeaseAggreementViewModel
                                {
                                    defaultaddress=(y.FirstOrDefault().maddress.HasValue?y.FirstOrDefault().maddress.Value>0?y.FirstOrDefault().maddress.Value:y.FirstOrDefault().mdefaultadd: y.FirstOrDefault().mdefaultadd),
                                    mest = y.FirstOrDefault().mest,
                                    msorderref = y.FirstOrDefault().msorderref,
                                    mpayment = y.FirstOrDefault().mpayment,
                                    //mprintby = y.FirstOrDefault().mname,
                                    mprintby = y.FirstOrDefault().createdby,
                                    mprintdate = y.FirstOrDefault().mprintdate,
                                    mleaseid = y.FirstOrDefault().mleaseid,
                                    mleaseno = y.Key.mleaseno,
                                    total = y.FirstOrDefault().total,
                                    menvelopid = y.FirstOrDefault().menvelopeid,
                                    createddate = y.FirstOrDefault().createddate,
                                    msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                                    Product = y.Select(x => new UserQuotationViewModel
                                    {
                                        msupplierid = x.msupplierid,
                                        morderno = x.morderno,
                                        magreementno = x.magreementno,
                                        type = x.type,
                                        mproductname = x.mprodname,
                                        mprice = x.mprice,
                                        msubquoteid = x.msubquoteid,
                                        mtermf = x.mtermf,
                                        mtermp = x.mtermp,
                                        mvat = x.mvat.HasValue ? x.mvat.Value : 0,
                                        quantity = x.quantity,
                                        mprodvarid = x.mprodvarid,
                                        mquotestatus = x.mstatus,
                                        mnotes = x.mnotes,
                                        minstallationflag = x.minstallationflag.HasValue ? x.minstallationflag.Value == 1 ? "yes" : "no" : "no",
                                        warrantydetails = new WarrantyCartViewModel
                                        {
                                            mprodname = x.warranty,
                                            mtermp = x.warrantyterm,
                                            mprice = x.warrrantyprice,
                                        },
                                        suppliername = x.mmanufacturer,
                                        //suppliername = x.morgname + (x.mcity != null ? ", " + x.mcity : "") + (x.town != null ? ", " + x.town : ""),
                                    }).ToList(),
                                }).ToList();


                //}
                if (products.Count < 0)
                {
                    return Ok("");
                }
                products.ForEach(x =>
                {
                    x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                    x.mdocname = getDocDetail(x.mleaseno);
                    //x.malloc = getAllocatSt(x.Product, ctx);
                    x.maddress = GetUserAddress(x.defaultaddress.Value,ctx);
                    x.Product.ForEach(z => {
                        z.status = getAllocatStProd(z.msubquoteid, z.msupplierid, ctx);
                        z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : "");
                        z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                        z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                        z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                    });
                    x.malloc = x.Product.Where(k => k.status == "p").Any() ? "p" : x.Product.Where(k => k.status == "a").Any() ? "a" : x.Product.Where(k => k.status == "di").Any() ? "di" : x.Product.Where(k => k.status == "rd").Any() ? "rd" : "p";
                });

                return Ok(products.OrderByDescending(x => x.createddate).ToList());
            }
            return Ok("");
        }
        public string GetUserAddress(int dadd,LeasingDbEntities db)
        {
            string addr = "";

            if (dadd != 0)
            {
                //var dadd = int.Parse(ab.DifferentAddress);
                var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == dadd);
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
                }
                return addr;
            }
            return addr;
        }
        [HttpGet]
        [Route("api/AggreementApi/GetPendingSignAggreementById")]
        public IHttpActionResult GetPendingSignAggreementById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from ag in ctx.GetAgreementViews
                        where ag.muserid == getid && (ag.magreementno > 0) && (ag.signstatus!="partial" && ag.signstatus!="completed") 
                        group new
                        {
                            ag.mmanufacturer,
                            ag.menvelopeid,
                            ag.signstatus,
                            ag.mest,
                            ag.msorderref,
                            ag.mpayment,
                            ag.mleaseid,
                            ag.msend,
                            ag.total,
                            ag.mname,
                            ag.mleaseno,
                            ag.mprintdate,
                            ag.morgname,
                            ag.mcity,
                            ag.town,
                            ag.createddate,
                            ag.magreementno,
                            ag.morderno,
                            ag.type,
                            ag.mprodname,
                            ag.mprice,
                            ag.msubquoteid,
                            ag.mtermf,
                            ag.mtermp,
                            ag.mvat,
                            ag.quantity,
                            ag.mprodvarid,
                            ag.mnotes,
                            ag.mstatus,
                            ag.msupplierid,
                            createdby = ag.Expr1,
                            ag.minstallationflag,
                            ag.warranty,
                            ag.warrantyterm,
                            ag.warrrantyprice,
                        }
                       by new
                       {
                           ag.mleaseno,
                       } into y
                        select new LeaseAggreementViewModel
                        {
                            mest = y.FirstOrDefault().mest,
                            msorderref = y.FirstOrDefault().msorderref,
                            mpayment = y.FirstOrDefault().mpayment,
                            //mprintby = y.FirstOrDefault().mname,
                            mprintby = y.FirstOrDefault().createdby,
                            mprintdate = y.FirstOrDefault().mprintdate,
                            mleaseid = y.FirstOrDefault().mleaseid,
                            mleaseno = y.Key.mleaseno,
                            total = y.FirstOrDefault().total,
                            menvelopid=y.FirstOrDefault().menvelopeid,
                            createddate = y.FirstOrDefault().createddate,
                            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                msupplierid = x.msupplierid,
                                morderno = x.morderno,
                                magreementno = x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                mvat = x.mvat.HasValue ? x.mvat.Value : 0,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                minstallationflag = x.minstallationflag.HasValue ? x.minstallationflag.Value == 1 ? "yes" : "no" : "no",
                                warrantydetails = new WarrantyCartViewModel
                                {
                                    mprodname = x.warranty,
                                    mtermp = x.warrantyterm,
                                    mprice = x.warrrantyprice,
                                },
                                suppliername = x.mmanufacturer,
                                //suppliername = x.morgname +  (x.mcity != null ? ", " + x.mcity : "")+(x.town != null ? ", " + x.town : ""),
                            }).ToList(),
                        }).ToList();

            products.ForEach(x =>
            {
                x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                x.mdocname = getDocDetail(x.mleaseno);
                //x.malloc = getAllocatSt(x.Product, ctx);
                x.Product.ForEach(z => {
                    z.status = getAllocatStProd(z.msubquoteid, z.msupplierid, ctx);
                    z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : "");
                    z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                    z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                });
                x.malloc = x.Product.Where(k => k.status == "p").Any() ? "p" : x.Product.Where(k => k.status == "a").Any() ? "a" : x.Product.Where(k => k.status == "di").Any() ? "di" : x.Product.Where(k => k.status == "rd").Any() ? "rd" : "p";
            });

            return Ok(products.OrderByDescending(x => x.createddate).ToList());
        }

        public string getAllocatStProd(int msubquoteid,Nullable<int> msupplierid, LeasingDbEntities ctx)
        {
            var allocflag = false;
            var disflag = false;
            var deliflag = false;
            var fdata = ctx.supplierstatus.FirstOrDefault(x => x.SubQuoteId == msubquoteid && x.SupplierId == msupplierid);
            if (fdata == null)
            {
                return "p";
            }
            else
            {
                if (fdata.Status == null || fdata.Status == "na" || fdata.Status == "")
                {
                    return "p";
                }
                else
                {
                    return fdata.Status;
                }
            }
        }
        public string getAllocatSt(List<UserQuotationViewModel> Products,LeasingDbEntities ctx)
        {
            var allocflag = false;
            var disflag = false;
            var deliflag = false;
            foreach(var ab in Products)
            {
                var fdata = ctx.supplierstatus.FirstOrDefault(x => x.SubQuoteId == ab.msubquoteid && x.SupplierId == ab.msupplierid);
                if (fdata == null)
                {
                    return "p";
                }
                else
                {
                    if(fdata.Status==null || fdata.Status == "na" || fdata.Status=="")
                    {
                        return "p";
                    }
                    else if (fdata.Status=="a")
                    {
                        allocflag = true;
                    }
                    else if (fdata.Status == "di")
                    {
                        disflag = true;
                    }
                    else if (fdata.Status == "rd")
                    {
                        deliflag = true;
                    }
                    
                }
            }
            if (allocflag)
            {
                return "a";
            }else if (disflag)
            {
                return "di";
            }
            else if (deliflag)
            {
                return "rd";
            }else
            {
                return "p";
            }
            
        }
        [HttpGet]
        [Route("api/AggreementApi/GetOldAggreementById")]
        public IHttpActionResult GetOldAggreementById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from la in ctx.userleaseagreements
                        join pc in ctx.usersubquotations on la.mleaseno equals pc.magreementno
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join sup in ctx.supplier_registration on pc.msupplierid equals sup.msuid into supdatas
                        from sup in supdatas.DefaultIfEmpty()
                        join um in ctx.user_registration on la.mprintby equals um.muid.ToString() into umdatas
                        from um in umdatas.DefaultIfEmpty()
                        where la.muserid == getid && (pc.magreementno > 0) 
                        group new
                        {
                           prod.mmanufacturer,
                            la.mest,
                            la.msorderref,
                            la.mpayment,
                            la.mleaseid,
                            la.msend,
                            la.total,
                            um.mname,
                            la.mleaseno,
                            la.mprintdate,
                            sup.morgname,
                            sup.mcity,
                            sup.mcounty,
                            la.createddate,
                            pc.magreementno,
                            pc.morderno,
                            pc.type,
                            prod.mprodname,
                            pc.mprice,
                            pc.msubquoteid,
                            pc.mtermf,
                            pc.mtermp,
                            pc.mvat,
                            pc.quantity,
                            pc.mprodvarid,
                            pc.mnotes,
                            pc.mstatus
                        }
                               by new
                               {
                                   la.mleaseno,
                               } into y
                        select new LeaseAggreementViewModel
                        {
                            mest=y.FirstOrDefault().mest,
                            msorderref = y.FirstOrDefault().msorderref,
                            mpayment = y.FirstOrDefault().mpayment,
                            mprintby = y.FirstOrDefault().mname,
                            mprintdate = y.FirstOrDefault().mprintdate,
                            mleaseid = y.FirstOrDefault().mleaseid,
                            mleaseno = y.Key.mleaseno,
                            total = y.FirstOrDefault().total,

                            createddate = y.FirstOrDefault().createddate,
                            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                            Product = y.Select(x => new UserQuotationViewModel
                            {

                                morderno = x.morderno,
                                magreementno = x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                mvat=x.mvat.HasValue?x.mvat.Value:0,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                suppliername = x.mmanufacturer,
                                //suppliername = x.morgname  + (x.mcity != null ? ", " + x.mcity : "") + (x.mcounty != null ? ", " + x.mcounty : ""),
                            }).ToList(),
                        }).ToList();
            
            products.ForEach(x =>
            {
                x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                x.mdocname = getDocDetail(x.mleaseno);
                x.Product.ForEach(z => {
                    z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : "");
                    z.mproductname= z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                    z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                });
            });
            
            return Ok(products);
        }

        [HttpPost]
        [Route("api/AggreementApi/GetListAggreementById")]
        public IHttpActionResult GetListAggreementById(SmallLeaseAggreementViewModel mleaseids)
        {
          
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            products= AgreementDAO.GetListAggreementById(mleaseids);

            return Ok(products);
        }
        public string getDocDetail(int? mleaseno)
        {
            var db = new LeasingDbEntities();
            var str = "";
            var searchdata = db.userleasedocuments.FirstOrDefault(x => x.mleaseno == mleaseno);
            if (searchdata != null)
            {
                str = searchdata.mdocname;
            }
            return str;
        }
        [HttpGet]
        [Route("api/AggreementApi/GetActiveAggreementById")]
        public IHttpActionResult GetActiveAggreementById(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from ag in ctx.GetAgreementViews
                        where ag.muserid == getid && (ag.magreementno > 0) && (ag.signstatus == "completed") && ag.menvelopeid != null
                        group new
                        {
                            ag.mmanufacturer,
                            ag.menvelopeid,
                            ag.signstatus,
                            ag.mest,
                            ag.msorderref,
                            ag.mpayment,
                            ag.mleaseid,
                            ag.msend,
                            ag.total,
                            ag.mname,
                            ag.mleaseno,
                            ag.mprintdate,
                            ag.morgname,
                            ag.mcity,
                            ag.town,
                            ag.createddate,
                            ag.magreementno,
                            ag.morderno,
                            ag.type,
                            ag.mprodname,
                            ag.mprice,
                            ag.msubquoteid,
                            ag.mtermf,
                            ag.mtermp,
                            ag.mvat,
                            ag.quantity,
                            ag.mprodvarid,
                            ag.mnotes,
                            ag.mstatus,
                            ag.msupplierid,
                            createdby = ag.Expr1,

                        }
                       by new
                       {
                           ag.mleaseno,
                       } into y
                        select new LeaseAggreementViewModel
                        {
                            mest = y.FirstOrDefault().mest,
                            msorderref = y.FirstOrDefault().msorderref,
                            mpayment = y.FirstOrDefault().mpayment,
                            //mprintby = y.FirstOrDefault().mname,
                            mprintby = y.FirstOrDefault().createdby,
                            mprintdate = y.FirstOrDefault().mprintdate,
                            mleaseid = y.FirstOrDefault().mleaseid,
                            mleaseno = y.Key.mleaseno,
                            total = y.FirstOrDefault().total,
                            menvelopid = y.FirstOrDefault().menvelopeid,
                            createddate = y.FirstOrDefault().createddate,
                            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                msupplierid = x.msupplierid,
                                morderno = x.morderno,
                                magreementno = x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                mvat = x.mvat.HasValue ? x.mvat.Value : 0,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                suppliername = x.mmanufacturer,
                                //suppliername = x.morgname + (x.mcity != null ? ", " + x.mcity : "") + (x.town != null ? ", " + x.town : ""),
                            }).ToList(),
                        }).ToList();

            products.ForEach(x =>
            {
                x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                x.mdocname = getDocDetail(x.mleaseno);
                //x.malloc = getAllocatSt(x.Product, ctx);
                x.Product.ForEach(z => {
                    z.status = getAllocatStProd(z.msubquoteid, z.msupplierid, ctx);
                    z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : "");
                    z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                    z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                });
                x.malloc = x.Product.Where(k => k.status == "p").Any() ? "p" : x.Product.Where(k => k.status == "a").Any() ? "a" : x.Product.Where(k => k.status == "di").Any() ? "di" : x.Product.Where(k => k.status == "de").Any() ? "de" : x.Product.Where(k => k.status == "c").Any() ? "c" : "p";
            });

            return Ok(products.OrderByDescending(x => x.createddate).ToList());
        }
        public IHttpActionResult GetActiveAggreementByIdold(int getid)
        {
            var id = getid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from la in ctx.userleaseagreements
                        join pc in ctx.usersubquotations on la.mleaseno equals pc.magreementno
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join doctin in ctx.userleasedocuments on la.mleaseno equals doctin.mleaseno
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join sup in ctx.supplier_registration on pc.msupplierid equals sup.msuid into supdatas
                        from sup in supdatas.DefaultIfEmpty()
                        join um in ctx.user_registration on la.mprintby equals um.muid.ToString() into umdatas
                        from um in umdatas.DefaultIfEmpty()
                        where (la.mlessorno != null && la.mlessorno != "")
                        group new
                        {
                            docdate = doctin.createddate,
                            la.msorderref,
                            la.mleaseid,
                            la.mlessorno,
                            la.msend,
                            la.total,
                            um.mname,
                            la.mleaseno,
                            la.mprintdate,
                            sup.morgname,
                            sup.mcity,
                            sup.mcounty,
                            la.createddate,
                            pc.magreementno,
                            pc.morderno,
                            pc.type,
                            prod.mprodname,
                            pc.mprice,
                            pc.msubquoteid,
                            pc.mtermf,
                            pc.mtermp,
                            pc.mvat,
                            pc.quantity,
                            pc.mprodvarid,
                            pc.mnotes,
                            pc.mstatus
                        }
                               by new
                               {
                                   la.mleaseno,
                               } into y
                        select new LeaseAggreementViewModel
                        {
                            msorderref=y.FirstOrDefault().msorderref,
                            mlessorno = y.FirstOrDefault().mlessorno,
                            mprintby = y.FirstOrDefault().mname,
                            mprintdate = y.FirstOrDefault().mprintdate,
                            mleaseid = y.FirstOrDefault().mleaseid,
                            mleaseno = y.Key.mleaseno,
                            total = y.FirstOrDefault().total,
                            createddate = y.FirstOrDefault().docdate,
                            msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                morderno = x.morderno,
                                magreementno = x.magreementno,
                                type = x.type,
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                mvat=x.mvat.HasValue?x.mvat.Value:0,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus,
                                mnotes = x.mnotes,
                                suppliername = x.morgname  + (x.mcity != null ? ", " + x.mcity : "") + (x.mcounty != null ? ", " + x.mcounty : ""),
                            }).ToList(),
                        }).ToList();
            products.ForEach(x =>
            {
                x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                x.Product.ForEach(z => z.msorderno = (ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno) != null ? ctx.userordermasters.FirstOrDefault(k => k.morderref == z.morderno).msorderref : ""));
            });

            return Ok(products);
        }
        [HttpPost]
        [Route("api/AggreementApi/updateLastPrint")]
        public IHttpActionResult updateLastPrint(int mleaseid, int muserid)
        {
            var db = new LeasingDbEntities();
            var id = muserid.ToString();
            var searchdata = db.userleaseagreements.FirstOrDefault(x => x.mleaseid == mleaseid);
            if (searchdata != null)
            {
                searchdata.mprintby = id;
                searchdata.mprintdate = DateTime.Now;
                db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Ok("Save");
            }
            else
            {
                return Ok("Not Updated");
            }
        }
        [HttpPost]
        [Route("api/AggreementApi/updateallLastPrint")]
        public IHttpActionResult updateallLastPrint(SmallLeaseAggreementViewModel mleaseids)
        {
            var db = new LeasingDbEntities();
            var f = false;
            if (mleaseids != null)
            {
                var id = mleaseids.muserid.ToString();
                if (mleaseids.mleasenos.Any())
                {
                    foreach (var ab in mleaseids.mleasenos)
                    {
                        var searchdata = db.userleaseagreements.FirstOrDefault(x => x.mleaseno == ab);
                        if (searchdata != null)
                        {
                            searchdata.mprintby = id;
                            searchdata.mprintdate = DateTime.Now;
                            db.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            f = true;
                        }

                        /*OrderController o = new OrderController();
                        var html2 = o.LoadOrder(ab);
                        
                        HtmlNode.ElementsFlags["br"] = HtmlElementFlag.Closed;
                        HtmlNode.ElementsFlags["img"] = HtmlElementFlag.Closed;
                        HtmlNode.ElementsFlags["input"] = HtmlElementFlag.Closed;
                        HtmlDocument doc = new HtmlDocument();
                        doc.OptionFixNestedTags = true;
                        doc.LoadHtml(html);
                        
                       
                        html = doc.DocumentNode.OuterHtml;

                        using (MemoryStream stream = new System.IO.MemoryStream())
                        {
                            Encoding unicode = Encoding.UTF8;
                            StringReader sr = new StringReader(html);
                            Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                            PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                            pdfDoc.Open();
                            XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                            pdfDoc.Close();
                            var result = new HttpResponseMessage(HttpStatusCode.OK)
                            {
                                Content = new ByteArrayContent(stream.ToArray())
                            };
                            result.Content.Headers.ContentDisposition =
                                new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment")
                                {
                                    FileName = "OrderStatus.pdf"
                                };
                            result.Content.Headers.ContentType =
                                new MediaTypeHeaderValue("application/pdf");

                            return result;
                            
                        }*/
                    }
                    if (f)
                    {
                        return Ok("success");
                    }
                    else
                    {
                        return Ok("failed");
                    }
                }

            }
            return Ok("failed");
        }
        [HttpPost]
        [Route("api/AgreementApi/UploadFile")]
        public IHttpActionResult UploadFile()
        {
            if (HttpContext.Current.Request.Files.AllKeys.Any())
            {
                string Url = HttpContext.Current.Request.Url.Host;
                if (Url == "www.saicomputer.com" || Url == "www.saicomputers.com")
                {
                    Url = "G:/PleskVhosts/saicomputers.com/httpdocs/saicomputer.com/LeaseAdmin/Content/LeaseDoc";
                }
                else if(Url == "www.ticklease.co.uk" || Url == "equipyourschool.co.uk")
                {
                    Url = HttpContext.Current.Server.MapPath("/LeaseAdmin/Content/LeaseDoc");
                }
                else
                {
                    Url = "F:/palak/bhadreshkaka/openstar/websites/leasing/project/LeasingPortalAdmin/LeasingPortalAdmin/LeasingPortalAdmin/Content/LeaseDoc/";
                    //HttpContext.Current.Server.MapPath("~/Content/LeaseDoc/")
                }
                var httpPostedFile = HttpContext.Current.Request.Files["file"];
                string fname;



                fname = httpPostedFile.FileName;
                var uniquename = DateTime.Now.Ticks;
                var fileName = httpPostedFile.FileName;
                var ext = Path.GetExtension(fname);
                var myUniqueFileName = string.Format(@"{0}" + ext, DateTime.Now.Ticks);
                string createpath = "LeaseDoc";
                bool exists = System.IO.Directory.Exists(Url);

                if (!exists)
                {
                    System.IO.Directory.CreateDirectory(Url);
                }
                fname = Path.Combine(Url, myUniqueFileName);
                httpPostedFile.SaveAs(fname);


                return Ok(myUniqueFileName);



            }
            return Ok("empty");
        }
        [HttpPost]
        [Route("api/AgreementApi/SaveDocIn")]
        public IHttpActionResult SaveDocIn(UserLeaseDocumentViewModel ordersdata)
        {
            var ctx = new LeasingDbEntities();
            if (ordersdata != null)
            {
                var searchdoc = ctx.userleasedocuments.FirstOrDefault(x => x.mleaseno == ordersdata.mleaseno);
                if (searchdoc == null)
                {
                    var obj = new userleasedocument
                    {
                        mleaseno = ordersdata.mleaseno,
                        mdocname = ordersdata.mdocname,
                        createddate = DateTime.UtcNow,
                        createdby = ordersdata.muserid,
                    };
                    ctx.userleasedocuments.Add(obj);
                    ctx.SaveChanges();
                    return Ok("Success");
                }
                else
                {
                    var olddoc = searchdoc.mdocname;
                    string Url = HttpContext.Current.Request.Url.Host;
                    if (Url == "www.saicomputer.com" || Url == "www.saicomputers.com")
                    {
                        Url = "G:/PleskVhosts/saicomputers.com/httpdocs/saicomputer.com/LeaseAdmin/Content/LeaseDoc/olddoc";
                    }
                    else if (Url == "www.ticklease.co.uk" || Url == "equipyourschool.co.uk")
                    {
                        Url = HttpContext.Current.Server.MapPath("/LeaseAdmin/Content/LeaseDoc/olddoc");
                    }
                    else
                    {
                        Url = "F:/palak/bhadreshkaka/openstar/websites/leasing/project/LeasingPortalAdmin/LeasingPortalAdmin/LeasingPortalAdmin/Content/LeaseDoc/olddoc";
                        //HttpContext.Current.Server.MapPath("~/Content/LeaseDoc/")
                    }
                    bool exists = System.IO.File.Exists(Url);
                    if (exists)
                    {
                        System.IO.File.Delete(Url);
                    }
                    searchdoc.mdocname = ordersdata.mdocname;
                    ctx.Entry(searchdoc).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                    return Ok("Success");
                }
                /*var searchdata = ctx.userleaseagreements.FirstOrDefault(x => x.mleaseno == ordersdata.mleaseno);
                if (searchdata != null)
                {
                    searchdata.mlessorno = ordersdata.mlessorno;
                    ctx.Entry(searchdata).State = System.Data.Entity.EntityState.Modified;
                    ctx.SaveChanges();
                }*/
                
            }
            return Ok("Failed");
        }
        [HttpGet]
        [Route("api/AgreementApi/MarkSend")]
        public IHttpActionResult MarkSend(int mleaseno)
        {
            var ctx = new LeasingDbEntities();
            var searchlease = ctx.userleaseagreements.FirstOrDefault(x => x.mleaseno == mleaseno);
            if (searchlease != null)
            {
                searchlease.msend = 2;
                ctx.Entry(searchlease).State = System.Data.Entity.EntityState.Modified;
                ctx.SaveChanges();
            }
            return Ok("Success");

        }
    }
}
