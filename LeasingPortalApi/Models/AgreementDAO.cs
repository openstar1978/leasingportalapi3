using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class AgreementDAO
    {
        public static List<LeaseAggreementViewModel> GetListAggreementById(SmallLeaseAggreementViewModel mleaseids)
        {
            var id = mleaseids.muserid;
            var seed = string.Empty;
            List<LeaseAggreementViewModel> products = null;
            var ctx = new LeasingDbEntities();
            if (mleaseids.mleasenos.Count > 0)
            {
                products = (from la in ctx.userleaseagreements
                            join pc in ctx.usersubquotations on la.mleaseno equals pc.magreementno
                            join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                            join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                            from prod in proddata.DefaultIfEmpty()
                            join sup in ctx.supplier_registration on pc.msupplierid equals sup.msuid into supdatas
                            from sup in supdatas.DefaultIfEmpty()
                            join um in ctx.user_registration on la.mprintby equals um.muid.ToString() into umdatas
                            from um in umdatas.DefaultIfEmpty()
                            where la.muserid == id && mleaseids.mleasenos.Any(s => s == la.mleaseno)
                            group new
                            {
                                la.msorderref,
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
                                pc.quantity,
                                pc.mprodvarid,
                                pc.mnotes,
                                pc.mstatus
                            }
                            by new
                            {
                                la.mleaseno,
                            }
                            into y
                            select new LeaseAggreementViewModel
                            {
                                msorderref = y.FirstOrDefault().msorderref,
                                mprintby = y.FirstOrDefault().mname,
                                mprintdate = y.FirstOrDefault().mprintdate,
                                mleaseid = y.FirstOrDefault().mleaseid,
                                mleaseno = y.Key.mleaseno,
                                total = y.FirstOrDefault().total,
                                createddate = y.FirstOrDefault().createddate,
                                msend = (byte)(y.FirstOrDefault().msend.HasValue ? y.FirstOrDefault().msend.Value : 0),
                                Product = y.Select(x => new UserQuotationViewModel
                                {
                                    msorderno = x.msorderref,
                                    morderno = x.morderno,
                                    magreementno = x.magreementno,
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
                            products.ForEach(x =>
                                {
                                    x.displaydate = (x.createddate.HasValue ? x.createddate.Value.ToString("dd/MM/yyyy") : "");
                                    x.mdocname = getDocDetail(x.mleaseno);
                                    x.Product.ForEach(z => {
                                        z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                                        z.mproducturl = "productname=" + z.mproductname + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(z.mprodvarid.ToString()));
                                        z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                                    });
                                });
            }
            return products;

        }
        public static string getDocDetail(int? mleaseno)
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

    }
}