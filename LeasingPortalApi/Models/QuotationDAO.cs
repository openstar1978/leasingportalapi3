using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class QuotationDAO
    {

        public static List<SupplierWiseQuotationViewModel> LoadSubQuotationDetails(List<int> subquotesid)
        {
            List<SupplierWiseQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where subquotesid.Any(s=>s==pc.msubquoteid)
                        group new
                        {
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
                                pc.msupplierid,


                            } into y
                        select new SupplierWiseQuotationViewModel()
                        {
                            msuid = y.Key.msupplierid,
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus
                            }).ToList()
                        }).ToList();
            products.ForEach(x =>
            {

                x.Product.ForEach(
                        z =>
                        {
                            z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                            z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                        });

            });
            return products;
        }
        public static List<SupplierWiseQuotationViewModel> LoadQuotationDetails(int quoteid)
        {
            List<SupplierWiseQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where pc.mquoteid == quoteid
                        group new
                        {
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
                                pc.msupplierid,


                            } into y
                        select new SupplierWiseQuotationViewModel()
                        {
                            msuid=y.Key.msupplierid,
                            Product = y.Select(x => new UserQuotationViewModel
                            {
                                mproductname = x.mprodname,
                                mprice = x.mprice,
                                msubquoteid = x.msubquoteid,
                                mtermf = x.mtermf,
                                mtermp = x.mtermp,
                                quantity = x.quantity,
                                mprodvarid = x.mprodvarid,
                                mquotestatus = x.mstatus
                            }).ToList()
                        }).ToList();
            products.ForEach(x =>
            {

                x.Product.ForEach(
                        z =>
                        {
                            z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                            z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                        });

            });
            return products;

        }
        
        public static List<SubCartDisplayViewModel> LoadRelatedQuotationProduct(int id)
        {
            var ctx = new LeasingDbEntities();
            var d = new List<SubCartDisplayViewModel>();

                    var ps = ctx.usersubquotations.FirstOrDefault(x => x.msubquoteid == id);
                    if (ps != null)
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
                                  where ab.mcompareprodid == ps.msubquoteid && ab.msupplierid == pcv.msupplierid && pcv.mprodsubid == ab.msubquoteid
                                  select new SubCartDisplayViewModel()
                                  {
                                      Id = ab.Id,
                                      mprodid = ps.msubquoteid,

                                      mprodname = p.mprodname,
                                      mtermf = ps.mtermf.HasValue ? ps.mtermf.Value : 1,
                                      mtermp = ps.mtermp.HasValue ? ps.mtermp.Value : 60,
                                      suppliername = users.morgname + (users.mcity != null ? ", " + users.mcity : "") + (users.mcounty != null ? ", " + users.mcounty : ""),
                                      //msubpicname = y.Key.pic,
                                      mqty=ps.quantity,
                                      mprice = mprice.HasValue ? mprice.Value : 0,
                                      mbasicprice = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                                      mprodvarid = pcv.mprodsubid,
                                      msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                                  }).ToList<SubCartDisplayViewModel>();
                        if (d2.Any() && d2.Count > 0)
                        {
                            d2.ForEach(x => d.Add(x));

                        }

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
                                      mqty = ps.quantity,
                                      mprice = mprice.HasValue ? mprice.Value : 0,
                                      mbasicprice = psub.mofferprice.HasValue ? psub.mofferprice.Value : 0,
                                      mprodvarid = pcv.mprodsubid,
                                      msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                                  }).ToList<SubCartDisplayViewModel>();
                        if (d3.Count > 0)
                        {
                            d3.ForEach(x => d.Add(x));

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
                return d;
            }
        // return Ok("failed");


        /*
        public static List<UserQuotationViewModel> LoadQuotationDetails(int quoteid)
        {
            List<UserQuotationViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        where pc.mquoteid == quoteid
                        
                        select new UserQuotationViewModel()
                        {
                            mproductname = prod.mprodname,
                                mprice = pc.mprice,
                                msubquoteid = pc.msubquoteid,
                                mtermf = pc.mtermf,
                                mtermp = pc.mtermp,
                                quantity = pc.quantity,
                                mprodvarid = pc.mprodvarid,
                                mquotestatus = pc.mstatus
                            
                        }).ToList();
            products.ForEach(
                    z => {
                        z.mproductname = z.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == z.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                        z.subpicname = ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(k => k.mprodsubid == z.mprodvarid).msubprodpicname : "";
                    });
            return products;
        }
*/
        public static string getPeriodInWords(int? mtermf)
        {
            var msg = "";
            string Number = mtermf.ToString();
            int _Number = Convert.ToInt32(mtermf);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
            //item.mprice=5;
            //return msg;
        }
        private static String ones(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = "";
            switch (_Number)
            {

                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }
        private static String tens(String Number)
        {
            int _Number = Convert.ToInt32(Number);
            String name = null;
            switch (_Number)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (_Number > 0)
                    {
                        name = tens(Number.Substring(0, 1) + "0") + " " + ones(Number.Substring(1));
                    }
                    break;
            }
            return name;
        }
        public static LeaseBankViewModel GetBanks(int agreementid,LeasingDbEntities db)
        {
            var getbanks = (from b in db.agreementbanks
                            where b.agreementid == agreementid
                            select new LeaseBankViewModel
                            {
                                maccountholder=b.maccountholder,
                                mbankaccount=b.mbankaccount,
                                mbankname=b.mbankname,
                                mbankshort=b.mbankshort
                            }).FirstOrDefault();
            return getbanks;
        }
        public static List<UserDisplayQuotationViewModel> LoadNewSubQuotationDetails(List<int> subquotesid)
        {
            var products = new List<UserDisplayQuotationViewModel>();
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join paddress in ctx.AgreementAddresses on pc.msubquoteid equals paddress.msubquoteid into paddressdata
                        from paddress in paddressdata.DefaultIfEmpty()
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join brand in ctx.productbrands on prod.mbrand equals brand.mbrandid into branddata
                        from brand in branddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        join supst in ctx.supplierstatus on pc.msubquoteid equals supst.SubQuoteId into supstdatas
                        from supst in supstdatas.DefaultIfEmpty()
                        join supsubst in ctx.subsupplierstatus on supst.Id equals supsubst.SSId into supsubstdatas
                        from supsubst in supsubstdatas.DefaultIfEmpty()
                        where subquotesid.Any(s => s == pc.msubquoteid)
                        select new UserDisplayQuotationViewModel()
                        {
                            itemcode=pcv.muniquecode,
                            Make = brand.mbrandname,
                            Condition = (pcv.mcondition != null ? pcv.mcondition : "new"),
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            mvat=pc.mvat,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            supplierstatus = supst.Status,
                            suppliernote = supsubst.KeyNotes,
                            supplierdate = supsubst.KeyDate,
                            mquotestatus = pc.mstatus,
                            DifferentAddress = paddress.maddress.ToString(),
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                        }).ToList();
            if (products.Any())
            {
                products.ForEach(x => {

                    x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                });

            }
            return products;
        }
        public static List<UserDisplayQuotationViewModel> LoadSingleNewSubQuotationDetails(int subquotesid)
        {
            var products = new List<UserDisplayQuotationViewModel>();
            var ctx = new LeasingDbEntities();
            products = (from pc in ctx.usersubquotations
                        join paddress in ctx.AgreementAddresses on pc.msubquoteid equals paddress.msubquoteid into paddressdata
                        from paddress in paddressdata.DefaultIfEmpty()
                        join pcv in ctx.productsubs on pc.mprodvarid equals pcv.mprodsubid
                        join prod in ctx.products on pcv.mprodid equals prod.mprodid into proddata
                        from prod in proddata.DefaultIfEmpty()
                        join brand in ctx.productbrands on prod.mbrand equals brand.mbrandid into branddata
                        from brand in branddata.DefaultIfEmpty()
                        join puser in ctx.supplier_registration on pc.msupplierid equals puser.msuid into puserdata
                        from puser in puserdata.DefaultIfEmpty()
                        join supst in ctx.supplierstatus on pc.msubquoteid equals supst.SubQuoteId into supstdatas
                        from supst in supstdatas.DefaultIfEmpty()
                        join supsubst in ctx.subsupplierstatus on supst.Id equals supsubst.SSId into supsubstdatas
                        from supsubst in supsubstdatas.DefaultIfEmpty()
                        where subquotesid == pc.msubquoteid
                        select new UserDisplayQuotationViewModel()
                        {
                            itemcode = pcv.muniquecode,
                            Make = brand.mbrandname,
                            Condition = (pcv.mcondition != null ? pcv.mcondition : "new"),
                            mproductname = prod.mprodname,
                            mprice = pc.mprice,
                            mvat = pc.mvat,
                            msubquoteid = pc.msubquoteid,
                            mtermf = pc.mtermf,
                            mtermp = pc.mtermp,
                            quantity = pc.quantity,
                            mprodvarid = pc.mprodvarid,
                            supplierstatus = supst.Status,
                            suppliernote = supsubst.KeyNotes,
                            supplierdate = supsubst.KeyDate,
                            mquotestatus = pc.mstatus,
                            DifferentAddress = paddress.maddress.ToString(),
                            suppliername = puser.morgname + (puser.mcity != null ? ", " + puser.mcity : "") + (puser.mcounty != null ? ", " + puser.mcounty : ""),
                        }).ToList();
            if (products.Any())
            {
                products.ForEach(x => {

                    x.mproductname = x.mproductname + " " + (ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(n => n.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                });

            }
            return products;
        }
        public static WarrantyCartViewModel getWarranty(int cartid, string mprodname, LeasingDbEntities db)
        {
            var p = new WarrantyCartViewModel();
            p = (from pcv in db.SaveCarts
                 join prodws in db.productsubwarrantybysuppliers on pcv.mwarrantid equals prodws.subwarrantid
                 join prodw in db.productwarrantybysuppliers on prodws.warrantyid equals prodw.WarrantyId
                 where pcv.mprodvarid == cartid && pcv.mwarrantyflag == 1
                 select new WarrantyCartViewModel
                 {
                     id = pcv.Id,
                     mprodname = pcv.mprodname,
                     mtermp = prodws.warrantyterm,
                     mprice = prodws.warrantyprice,
                     subwarrantid = prodws.subwarrantid,
                     mtermf = pcv.mtermf,
                     mqty = pcv.mqty,
                     mbaseprice = prodws.warrantyprice,
                 }).FirstOrDefault();

            return p;
        }
        public static string ImageToBase64(int status)
        {
            System.Drawing.Image image;
            System.Drawing.Imaging.ImageFormat format;
            if (status == 1)
            {
                image = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~/Content/images/logo.png"), true);
                format = System.Drawing.Imaging.ImageFormat.Png;
            }
            else
            {
                image = System.Drawing.Image.FromFile(HttpContext.Current.Server.MapPath("~/Content/logo/felixfull.png"), true);
                format = System.Drawing.Imaging.ImageFormat.Png;
            }
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String.ToString();
            }
        }
        public static string SavePrintQuoteLog(printquotelogviewmodel model)
        {
            var db = new LeasingDbEntities();
            var today = DateTime.Now;
            var tilldate = today.AddDays(21);
            var id = 101;
            var getids = db.printquotelogs.OrderByDescending(x => x.Id).FirstOrDefault();
            if (getids != null)
            {
                id = getids.Id + 101;
            }
            var quotenumber = "TL" + id + DateTime.Now.Year;
            var Quotelog = new printquotelog
            {
                prod_id=model.prod_id,
                user_id=model.user_id,
                supplier_id=model.supplier_id,
                mprice=model.mprice,
                mtermf=model.mtermf,
                mtermp=model.mtermp,
                quantity=model.quantity,
                createddate=today,
                valid=1,
               quotenumber=quotenumber
            };
            db.printquotelogs.Add(Quotelog);
            db.SaveChanges();
            return quotenumber;
        }
             
    }
}