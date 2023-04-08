using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace LeasingPortalApi.Models
{
    public class ProductDao
    {
        public static List<ShopByCategoryViewModel> getshopbycategory()
        {
            List <ShopByCategoryViewModel> lstdata= new List<ShopByCategoryViewModel>();
            var obj = new ShopByCategoryViewModel();
            var db = new LeasingDbEntities();
            var catdata = db.productcats.Where(x => x.mpreviouscat == 0).ToList();
            if (catdata.Any())
            {
                foreach (var ab in catdata)
                {

                    var subcatdatas = (from abs in db.productcats
                                       where abs.mpreviouscat == ab.mprodcatid && abs.mfeatured==1
                                       select new ShopByCategoryViewModel
                                       {
                                           mcatimg=abs.mcatimage,
                                           mcatname=abs.mcatname,
                                           mcaturl="",
                                           mcatid=abs.mprodcatid
                                       }).ToList();
                    if (subcatdatas.Any())
                    {
                        subcatdatas.ForEach(x=>lstdata.Add(x));
                    }
                }

            }
            if (lstdata.Count == 0)
            {
                return lstdata;
            }
            lstdata.ForEach(x => x.mcaturl = "Category=" + x.mcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mcatid.ToString()))+"&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mcatid.ToString())));
            return lstdata;
        }
        public static List<ShopByAreaViewModel> getshopbyareas()
        {
            List<ShopByAreaViewModel> lstdata = new List<ShopByAreaViewModel>();
            var db = new LeasingDbEntities();
            lstdata = (from abs in db.tblschoolareas
                           select new ShopByAreaViewModel
                           {
                               mareaid = abs.mareaid,
                               mareaname = abs.mareaname,
                               massigncategory = abs.massigncategory,
                               mphoto = abs.mphoto
                           }).ToList();
            if (lstdata.Count == 0)
            {
                return lstdata;
            }
            lstdata.ForEach(x => x.mcaturl = "Dept=area&a=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mareaid.ToString())) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mareaname.ToString())));
            
            return lstdata;
        }

        public static ProductDisplayViewModel GetProductById(int id)
        {

            using (var ctx = new LeasingDbEntities())
            {
             var   products = (
                    from pvar in ctx.productsubs
                    join p in ctx.products on pvar.mprodid equals p.mprodid
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pimg in ctx.productsubpics on p.mprodid equals pimg.mprodsubid into pimgdata
                            from pimg in pimgdata.DefaultIfEmpty()
                            where pvar.mprodsubid==id
                            group new
                            {

                                pimg.msubprodpicname
                            }
                            by new
                            {
                                p.mprodid,
                                p.mprodname,
                                pbrand.mbrandid,
                                pbrand.mbrandname,
                                pc.mcatname
                            } into y
                            orderby y.Key.mprodid
                            select new ProductDisplayViewModel()
                            {
                                mprodid = y.Key.mprodid,
                                mprodname = y.Key.mprodname,
                                mprodcatname = y.Key.mcatname,
                                mbrandid = y.Key.mbrandid,
                                mbrand=y.Key.mbrandname,
                                msubpicname = y.FirstOrDefault().msubprodpicname,
                                mprice = 500
                            }).FirstOrDefault<ProductDisplayViewModel>();

                return products;
            }
            
        }
        
        public static List<ProductSubPicViewModel> GetProductImages(int id)
        {
            var imggallery = new List<ProductSubPicViewModel>();
            using (var ctx = new LeasingDbEntities())
            {
                imggallery = (from pv in ctx.productsubs
                              join p in ctx.productsubpics on pv.mprodsubid equals p.mprodsubid
                             where pv.mprodsubid == id
                             select new ProductSubPicViewModel()
                             {
                                 msubprodpicname=p.msubprodpicname,

                             }).ToList();
            }
                return imggallery;
        }
        public static List<SingleProductDetailViewModel> GetSpecs(int prodid)
        {
            var ctx = new LeasingDbEntities();
            var productdetail = new List<SingleProductDetailViewModel>();
            var productid = ctx.productsubs.FirstOrDefault(x => x.mprodsubid == prodid).mprodid;
            productdetail = (from p in ctx.products
                             join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                             join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                             join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                             from pu in pudata.DefaultIfEmpty()
                             where p.mprodid == productid && pc.mdetailvalue != null && pc.mdetailvalue != ""
                             select new SingleProductDetailViewModel()
                             {
                                 mprodid = prodid,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).Take(4).ToList();

            return productdetail;
        }
    }
}