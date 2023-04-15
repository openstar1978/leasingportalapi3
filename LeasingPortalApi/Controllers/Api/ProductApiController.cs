using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;
using System.Web;
using System.Data.SqlClient;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
namespace LeasingPortalApi.Controllers.Api
{
    public class ProductApiController : ApiController
    {
        [HttpGet]
        public IHttpActionResult GetAllFilterForProduct(int getid, string page)
        {
            var id = getid;
            var seed = string.Empty;
            List<ProductFilterViewModel> filters = null;
            return Ok("");
        }
        public static string PadNumbers(string input)
        {
            return Regex.Replace(input, "[0-9]+", match => match.Value.PadLeft(10, '0'));
        }
        [Route("api/productapi/GetAllProductById")]
        [HttpPost]
        public IHttpActionResult GetAllProductById(ProductParameterViewModel param)
        {
            //bool filterd = filter;
            //var id = int.Parse(Encryption.Decrypt(getid));
            var getid = param.getid;
            var row = param.row;
            var rowperpage = param.rowperpage;
            var filter = param.filter;
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetProducts(getid).ToList();
            var getprod = getallproducts;
            if (param.min > 0 && param.max > 0)
            {
                getprod = (from p in getallproducts
                           where p.mprodcatid == id && p.mrp >= param.min && p.mrp <= param.max
                           select p).ToList();
            }
            if (param.filterdata.Any(x => x.FilterUrl == "bn"))
            {
                getprod = (from p in getprod
                           where p.mprodcatid == id && param.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                           select p).ToList();
            }

            if (param.filterdata.Any(x => x.FilterUrl == "a"))
            {
                List<int> productid = new List<int>();
                List<int> getproducts = new List<int>();
                var index = 0;
                foreach (var ab in param.filterdata.Where(x => x.FilterUrl == "a"))
                {
                   
                    var ss = ab.FilterValue.Split(',');
                    List<spGetFiltersQuery_Result> getfilteredsubtwoes = null;
                    if (ab.FilterName=="Screen Size")
                    {
                        getfilteredsubtwoes = ctx.spGetFiltersQuery("Select mdetailheadid,mprodusubtwoid,mprodid,mdetailunit,(CASE WHEN Isnumeric(mdetailvalue)=1 and mdetailvalue is not null THEN cast(Floor(cast(mdetailvalue as float)) as nvarchar(max)) else mdetailvalue End) as mdetailvalue from [productsubtwo] where mdetailheadid=" + ab.FilterId).ToList();
                    }
                    else
                    {
                        getfilteredsubtwoes = ctx.spGetFiltersQuery("Select mdetailheadid,mprodusubtwoid,mprodid,mdetailvalue,mdetailunit from [productsubtwo] where mdetailheadid=" + ab.FilterId).ToList();
                    }
                    

                    if (getproducts.Count<=0 || !getproducts.Any())
                    {

                        
                        getproducts = (from c in getfilteredsubtwoes
                                       
                                       where c.mdetailheadid == ab.FilterId && ss.Any(z => z==c.mdetailvalue)
                                       select c.mprodid.Value).ToList();

                    }
                    else
                    {
                        getproducts = (from c in getfilteredsubtwoes
                                       where c.mdetailheadid == ab.FilterId && ss.Any(z => z==c.mdetailvalue) && getproducts.Any(z=>z==c.mprodid)
                                       select c.mprodid.Value).ToList();
                    }
                    //if (getproducts.Any())
                    //{
                        productid=getproducts;
                    //}
                }
                getprod = (from p in getprod
                           join pids in productid on p.mprodid equals pids
                           where p.mprodcatid == id
                           select p).ToList();

            }
            /*if (getprod == null)
            {
                getprod = (from p in getallproducts
                           where p.mprodcatid == id
                           select p).ToList();

            }*/
            var finalprod = getprod;//.ToList();
            products = (from p in finalprod
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mbrandlogo = p.mbrandlogo,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor=p.sellername!=null && p.sellername!=""?p.sellername.Substring(0,2):"00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msstock = p.msstock.HasValue ? p.msstock.Value : 0,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList();
                        /*.GroupBy(x => x.mprodvarid).Select(y => new ProductDisplayViewModel
                        {
                            mprodid = y.FirstOrDefault().mprodid,
                            mprodname = y.FirstOrDefault().mprodname,
                            mprodcatname = y.FirstOrDefault().mprodcatname,
                            mbrandid = y.FirstOrDefault().mbrandid,
                            mbrand = y.FirstOrDefault().mbrand,
                            mbrandlogo = y.FirstOrDefault().mbrandlogo,
                            mtermf = y.FirstOrDefault().mtermf,
                            mtermp = y.FirstOrDefault().mtermp,
                            msuppliername = y.FirstOrDefault().msuppliername,
                            //msubpicname = y.Key.pic,
                            msstock=y.FirstOrDefault().msstock,
                            mprice = y.FirstOrDefault().mprice,
                            mbasicprice = y.FirstOrDefault().mbasicprice,
                            mprodvarid = y.FirstOrDefault().mprodvarid,
                            minstallflag = y.FirstOrDefault().minstallflag,
                            msupplierid = y.FirstOrDefault().msupplierid,
                            mleadtime = y.FirstOrDefault().mleadtime,
                            msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty1,
                            msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2,
                            msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3,
                            msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4,
                            msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5,
                            manufacturewarranty = y.FirstOrDefault().manufacturewarranty,
                            MPN = y.FirstOrDefault().MPN,
                            EYS = y.FirstOrDefault().EYS
                        }).ToList<ProductDisplayViewModel>();*/

            if (products.Count == 0)
            {
                return Ok("");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand,
                    display=true,
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });
                var attrfilter = (from g in ctx.spGetFilters(getid).ToList()
                                  select g).GroupBy(x => x.mdetailheadid).Select(y => new ProductFilterViewModel
                                  {
                                      FilterHead = y.FirstOrDefault().mdetailhead,
                                      FilterData = y.GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(k => new ProductFilterDetailViewModel
                                      {
                                          FilterName = y.FirstOrDefault().mdetailhead,
                                          FilterUnit = k.FirstOrDefault().munitname,
                                          FilterId = k.FirstOrDefault().mdetailheadid.Value,
                                          FilterUrl = "a",
                                          FilterValue = k.Key,
                                          range = false,
                                          rangevalues=k.Key,
                                          display=true,
                                      }).OrderBy(x => PadNumbers(x.FilterValue)).ToList()
                                  }) ;
                filters.AddRange(attrfilter);
                /*var getfiltercat = ctx.productcomparativefields.Where(x => x.Filter == "Y" && x.CategoryId == getid).ToList();
                if (getfiltercat.Any())
                {
                    foreach (var ab in getfiltercat)
                    {
                        var getdetail = ctx.productdetailmasters.FirstOrDefault(x => x.mprodetailid == ab.DetailId);
                        var attributefilters = (from g in products
                                                join subtwo in ctx.productsubtwoes on g.mprodid equals subtwo.mprodid
                                                where subtwo.mdetailheadid == ab.DetailId && subtwo.mdetailvalue != "" && subtwo.mdetailvalue != null
                                                select subtwo).GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(y => new ProductFilterDetailViewModel
                                                {
                                                    FilterId = y.FirstOrDefault().mdetailheadid.Value,
                                                    FilterUrl = "a",
                                                    FilterValue = y.Key
                                                }).OrderBy(x => x.FilterValue).ToList();
                        if (attributefilters.Any())
                        {
                            filters.Add(new ProductFilterViewModel
                            {
                                FilterHead = getdetail.mdetailhead,
                                FilterData = attributefilters
                            });
                        }
                    }
                }*/
            }

            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            if (param.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).ToList();
            }
            else if (param.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).ToList();
            }
            else if (param.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).ToList();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "xx="+x.supcode+"&yy="+x.distributor+"&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                var subpicdata = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });
            filters = customisefilter(filters);
            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
        }
        public static List<ProductFilterViewModel> customisefilter(List<ProductFilterViewModel> filters)
        {
            filters.ForEach(x =>
            {
                if (x.FilterHead == "Main Storage")
                {
                    var cnt = 0;
                    var index = -1;
                    x.FilterData.ForEach(z =>
                    {
                        float re = 0;
                        if (float.TryParse(z.FilterValue, out re))
                        {
                            if (re >= 1000 && index==-1)
                            {
                                z.FilterValue = "1 TB and above";
                                z.FilterUnit = null;
                                z.range = true;
                                index = cnt;
                            }else if(re>=1000 && index != -1)
                            {
                                x.FilterData[index].rangevalues = x.FilterData[index].rangevalues + "," + z.FilterValue;
                                z.display = false;
                            }
                            

                        }
                        cnt++;
                    });
                }
                else if (x.FilterHead == "Memory RAM")
                {
                    var cnt = 0;
                    var index = -1;
                    x.FilterData.ForEach(z =>
                    {
                        float re = 0;
                        if (float.TryParse(z.FilterValue, out re))
                        {
                            if (re <= 8 && index == -1)
                            {
                                z.FilterValue = "Upto 8 GB";
                                z.FilterUnit = null;
                                z.range = true;
                                index = cnt;
                            }
                            else if (re <=8 && index != -1)
                            {
                                x.FilterData[index].rangevalues = x.FilterData[index].rangevalues + "," + z.FilterValue;
                                z.display = false;
                            }


                        }
                        cnt++;
                    });
                }
            });
            return filters;
        }
        [HttpPost]
        [Route("api/ProductApi/GetSearchProduct")]
        public IHttpActionResult GetSearchProduct(SearchProductViewModel prodsearch)
        {
            var getproduct = HttpUtility.HtmlDecode(prodsearch.getproduct);
            var category = prodsearch.category;
            var posttype = prodsearch.posttype;
            var row = prodsearch.row;
            var rowperpage = prodsearch.rowperpage;
            //var id = int.Parse(Encryption.Decrypt(getid));
            var filters = new List<ProductFilterViewModel>();
            var seed = string.Empty;
            var sdata = getproduct.Split(' ');
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filter = prodsearch.filter;
            bool pricefilter = false;
            var getprods = getproduct;
            //IQueryable<product> productdata = null;
            if (posttype == null || posttype.ToLower() == "product" || posttype=="")
            {
                posttype = "-1";
                if (getproduct.EndsWith("s"))
                {
                    posttype = "st";
                    getprods = getproduct.Substring(0, getproduct.LastIndexOf("s"));
                }
            }
            if (category != 0)
            {
                var productdata = ctx.spGetSearchProductsInCategory(category, posttype, getproduct,getprods).ToList();
                var getprod = productdata;
                if (posttype == "pt" || posttype == "bpt")
                {
                    var ptype = getproduct;
                    var brand = "";
                    if (posttype == "bpt")
                    {
                        var splitstring = getproduct.Split('$');
                        brand = splitstring[0];
                        ptype = splitstring[1];
                    }
                    var getdetaildata = (from dm in ctx.productdetailmasters
                                         join ps in ctx.productsubtwoes on dm.mprodetailid equals ps.mdetailheadid
                                         where dm.mdetailhead == "Product Type" && ps.mdetailvalue.StartsWith(ptype)
                                         select new
                                         {
                                             ps.mprodid
                                         }).AsQueryable();
                    if (getdetaildata.Any())
                    {
                        if (posttype == "pt")
                        {
                            getprod = (from p in productdata
                                       join pt in getdetaildata on p.mprodid equals pt.mprodid
                                       select p).ToList();
                        }
                        else
                        {
                            getprod = (from p in productdata
                                       join pt in getdetaildata on p.mprodid equals pt.mprodid
                                       where p.mbrandname == brand
                                       select p).ToList();
                        }
                    }

                }
                if (prodsearch.min > 0 && prodsearch.max > 0)
                {
                    getprod = (from p in getprod
                               where p.mrp >= prodsearch.min && p.mrp <= prodsearch.max
                               select p).ToList();
                    pricefilter = true;
                }
                if (prodsearch.filterdata.Any(x => x.FilterUrl == "bn"))
                {
                    getprod = (from p in getprod
                               where prodsearch.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                               select p).ToList();
                    pricefilter = true;
                }
                if (prodsearch.filterdata.Any(x => x.FilterUrl == "a"))
                {
                    List<int> productid = new List<int>();
                    List<int> getproducts = new List<int>();
                    foreach (var ab in prodsearch.filterdata.Where(x => x.FilterUrl == "a"))
                    {
                        var ss = ab.FilterValue.Split(',');
                        if (getproducts.Count <= 0 || !getproducts.Any())
                        {
                            getproducts = (from c in ctx.productsubtwoes
                                           where c.mdetailheadid == ab.FilterId && ss.Any(z => z == c.mdetailvalue)
                                           select c.mprodid.Value).ToList();

                        }
                        else
                        {
                            getproducts = (from c in ctx.productsubtwoes
                                           where c.mdetailheadid == ab.FilterId && ss.Any(z => z == c.mdetailvalue) && getproducts.Any(z => z == c.mprodid)
                                           select c.mprodid.Value).ToList();
                        }
                        //if (getproducts.Any())
                        //{
                        productid = getproducts;
                        //}
                    }
                    getprod = (from p in getprod
                               join pids in productid on p.mprodid equals pids
                               select p).ToList();
                    pricefilter = true;

                }
                if (posttype != "-1" && getprod.Count() <= 0 && !pricefilter) 
                {
                    if (getprod.Any())
                    {
                        //var getprod2 =ctx.spGetSearchProducts("-1", getproduct).ToList();
                        getprod.AddRange(ctx.spGetSearchProductsInCategory(category, "-1", getproduct,getprods).ToList());
                    }
                    else
                    {
                        getprod = ctx.spGetSearchProductsInCategory(category, "-1", getproduct,getprods).ToList();
                    }
                }
                var finalprod = getprod.ToList();
                products = (from p in finalprod
                            select new ProductDisplayViewModel()
                            {
                                mprodid = p.mprodid,
                                mbrandlogo = p.mbrandlogo,
                                mprodname = p.mprodname,
                                mprodcatname = p.mcatname,
                                mbrandid = p.mbrandid,
                                mbrand = p.mbrandname,
                                mtermf = 1,
                                mtermp = 60,
                                supcode = p.morgname.Substring(0, 3),
                                distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                                msuppliername =p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                                //msubpicname = y.Key.pic,
                                msstock = p.msstock.HasValue ? p.msstock.Value : 0,
                                mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                                mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                                mprodvarid = p.mprodsubid,
                                minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                                msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                                mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                                msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                MPN = p.mmanufacturercode,
                                EYS = p.muniquecode
                            }).ToList();
                            /*GroupBy(x=>x.mprodvarid).Select(y=>new ProductDisplayViewModel
                            {
                                mprodid = y.FirstOrDefault().mprodid,
                                mprodname = y.FirstOrDefault().mprodname,
                                mprodcatname = y.FirstOrDefault().mprodcatname,
                                mbrandid = y.FirstOrDefault().mbrandid,
                                mbrand = y.FirstOrDefault().mbrand,
                                mbrandlogo = y.FirstOrDefault().mbrandlogo,
                                mtermf = y.FirstOrDefault().mtermf,
                                mtermp = y.FirstOrDefault().mtermp,
                                msuppliername = y.FirstOrDefault().msuppliername,
                                //msubpicname = y.Key.pic,
                                msstock=y.FirstOrDefault().msstock,
                                mprice = y.FirstOrDefault().mprice,
                                mbasicprice = y.FirstOrDefault().mbasicprice,
                                mprodvarid = y.FirstOrDefault().mprodvarid,
                                minstallflag = y.FirstOrDefault().minstallflag,
                                msupplierid = y.FirstOrDefault().msupplierid,
                                mleadtime = y.FirstOrDefault().mleadtime,
                                msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty1,
                                msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2,
                                msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3,
                                msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4,
                                msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5,
                                manufacturewarranty = y.FirstOrDefault().manufacturewarranty,
                                MPN = y.FirstOrDefault().MPN,
                                EYS = y.FirstOrDefault().EYS
                            }).ToList<ProductDisplayViewModel>();*/

            }
            else
            {
                var productdata = ctx.spGetSearchProducts(posttype, getproduct,getprods).ToList();
                var getprod = productdata;
                if (posttype == "pt" || posttype == "bpt")
                {
                    var ptype = getproduct;
                    var brand = "";
                    if (posttype == "bpt")
                    {
                        var splitstring = getproduct.Split('$');
                        brand = splitstring[0];
                        ptype = splitstring[1];
                    }
                    var getdetaildata = (from dm in ctx.productdetailmasters
                                         join ps in ctx.productsubtwoes on dm.mprodetailid equals ps.mdetailheadid
                                         where dm.mdetailhead == "Product Type" && ps.mdetailvalue.StartsWith(ptype)
                                         select new
                                         {
                                             ps.mprodid
                                         }).AsQueryable();
                    if (getdetaildata.Any())
                    {
                        if (posttype == "pt")
                        {
                            getprod = (from p in productdata
                                       join pt in getdetaildata on p.mprodid equals pt.mprodid
                                       select p).ToList();
                        }
                        else
                        {
                            getprod = (from p in productdata
                                       join pt in getdetaildata on p.mprodid equals pt.mprodid
                                       where p.mbrandname == brand
                                       select p).ToList();
                        }
                    }

                }
                if (prodsearch.min > 0 && prodsearch.max > 0)
                {
                    getprod = (from p in getprod
                               where p.mrp >= prodsearch.min && p.mrp <= prodsearch.max
                               select p).ToList();
                    pricefilter = true;
                }
                if (prodsearch.filterdata.Any(x => x.FilterUrl == "bn"))
                {
                    getprod = (from p in getprod
                               where prodsearch.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                               select p).ToList();
                    pricefilter = true;
                }
                if (prodsearch.filterdata.Any(x => x.FilterUrl == "a"))
                {
                    List<int> productid = new List<int>();
                    foreach (var ab in prodsearch.filterdata.Where(x => x.FilterUrl == "a"))
                    {
                        var ss = ab.FilterValue.Split(',');
                        var getproducts = (from c in ctx.productsubtwoes
                                           where c.mdetailheadid == ab.FilterId && ss.Any(z => z == c.mdetailvalue)
                                           select c.mprodid.Value).AsEnumerable();
                        if (getproducts.Any())
                        {
                            productid.AddRange(getproducts);
                        }
                    }

                    getprod = (from p in getprod
                               join pids in productid on p.mprodid equals pids
                               select p).ToList();
                    pricefilter = true;
                }
                if (posttype != "-1" && getprod.Count()<=0 && !pricefilter)
                {
                    if (getprod.Any())
                    {
                        //var getprod2 =ctx.spGetSearchProducts("-1", getproduct).ToList();
                        getprod.AddRange(ctx.spGetSearchProducts("-1", getproduct,getprods).ToList());
                    }
                    else
                    {
                        getprod = ctx.spGetSearchProducts("-1", getproduct,getprods).ToList();
                    }

                }
                var finalprod = getprod.ToList();
                products = (from p in finalprod
                            select new ProductDisplayViewModel()
                            {
                                mprodid = p.mprodid,
                                mbrandlogo = p.mbrandlogo,
                                mprodname = p.mprodname,
                                mprodcatname = p.mcatname,
                                mbrandid = p.mbrandid,
                                mbrand = p.mbrandname,
                                mtermf = 1,
                                mtermp = 60,
                                supcode = p.morgname.Substring(0, 3),
                                distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                                msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                                //msubpicname = y.Key.pic,
                                mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                                mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                                mprodvarid = p.mprodsubid,
                                msstock = p.msstock.HasValue ? p.msstock.Value : 0,
                                minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                                msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                                mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                                msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                MPN = p.mmanufacturercode,
                                EYS = p.muniquecode
                            }).ToList();
                            /*.GroupBy(x => x.mprodvarid).Select(y => new ProductDisplayViewModel
                            {
                                mprodid = y.FirstOrDefault().mprodid,
                                mprodname = y.FirstOrDefault().mprodname,
                                mprodcatname = y.FirstOrDefault().mprodcatname,
                                mbrandid = y.FirstOrDefault().mbrandid,
                                mbrand = y.FirstOrDefault().mbrand,
                                mbrandlogo = y.FirstOrDefault().mbrandlogo,
                                mtermf = y.FirstOrDefault().mtermf,
                                mtermp = y.FirstOrDefault().mtermp,
                                msuppliername = y.FirstOrDefault().msuppliername,
                                //msubpicname = y.Key.pic,
                                msstock=y.FirstOrDefault().msstock,
                                mprice = y.FirstOrDefault().mprice,
                                mbasicprice = y.FirstOrDefault().mbasicprice,
                                mprodvarid = y.FirstOrDefault().mprodvarid,
                                minstallflag = y.FirstOrDefault().minstallflag,
                                msupplierid = y.FirstOrDefault().msupplierid,
                                mleadtime = y.FirstOrDefault().mleadtime,
                                msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty1,
                                msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2,
                                msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3,
                                msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4,
                                msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5,
                                manufacturewarranty = y.FirstOrDefault().manufacturewarranty,
                                MPN = y.FirstOrDefault().MPN,
                                EYS = y.FirstOrDefault().EYS
                            }).ToList<ProductDisplayViewModel>();*/
            }



            if (products.Count == 0)
            {
                return Ok("empty");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });
                if (category != 0 || posttype == "c")
                {
                    var cattid = 0;
                    if (category != 0)
                    {
                        cattid = category;
                    }
                    else
                    {
                        var getcat = ctx.productcats.FirstOrDefault(x => x.mcatname == getproduct);
                        if (getcat != null)
                            cattid = getcat.mprodcatid;
                    }

                    var attrfilter = (from g in ctx.spGetFilters(cattid).ToList()
                                      select g).GroupBy(x => x.mdetailheadid).Select(y => new ProductFilterViewModel
                                      {
                                          FilterHead = y.FirstOrDefault().mdetailhead,
                                          FilterData = y.GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(k => new ProductFilterDetailViewModel
                                          {
                                              FilterId = k.FirstOrDefault().mdetailheadid.Value,
                                              FilterUrl = "a",
                                              FilterValue = k.Key
                                          }).OrderBy(x => x.FilterValue).ToList()
                                      });
                    filters.AddRange(attrfilter);
                    /*var getfiltercat = ctx.productcomparativefields.Where(x => x.Filter == "Y" && x.CategoryId == cattid).ToList();
                    if (getfiltercat.Any())
                    {
                        foreach (var ab in getfiltercat)
                        {
                            var getdetail = ctx.productdetailmasters.FirstOrDefault(x => x.mprodetailid == ab.DetailId);
                            var attributefilters = (from g in products
                                                    join subtwo in ctx.productsubtwoes on g.mprodid equals subtwo.mprodid
                                                    where subtwo.mdetailheadid == ab.DetailId && subtwo.mdetailvalue != "" && subtwo.mdetailvalue != null
                                                    select subtwo).GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(y => new ProductFilterDetailViewModel
                                                    {
                                                        FilterId = y.FirstOrDefault().mdetailheadid.Value,
                                                        FilterUrl = "a",
                                                        FilterValue = y.Key
                                                    }).OrderBy(x => x.FilterValue).ToList();
                            if (attributefilters.Any())
                            {
                                filters.Add(new ProductFilterViewModel
                                {
                                    FilterHead = getdetail.mdetailhead,
                                    FilterData = attributefilters
                                });
                            }
                        }
                    }*/
                }

            }
            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;
            if (prodsearch.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (prodsearch.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (prodsearch.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            //products = products.Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                var subpicdata = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });
            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
            //return Ok(products);
        }
        [Route("api/productapi/GetFiltersById")]
        [HttpGet]
        public IHttpActionResult GetFiltersById(int getid, int row, int rowperpage)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetProducts(getid).ToList();
            products = (from p in getallproducts
                        where p.mprodcatid == id
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).OrderBy(x => x.mprice).ToList();




            if (products.Count == 0)
            {
                return Ok("");
            }
            var brandfilters = getallproducts.GroupBy(x => x.mbrandname).Select(y => new ProductFilterDetailViewModel
            {
                FilterId = y.FirstOrDefault().mbrandid,
                FilterUrl = "bn",
                FilterValue = y.FirstOrDefault().mbrandname
            }).Distinct().ToList();
            filters.Add(new ProductFilterViewModel
            {
                FilterHead = "Brand",
                FilterData = brandfilters


            });
            var getfiltercat = ctx.productcomparativefields.Where(x => x.Filter == "Y" && x.CategoryId == getid).ToList();
            if (getfiltercat.Any())
            {
                foreach (var ab in getfiltercat)
                {
                    var getdetail = ctx.productdetailmasters.FirstOrDefault(x => x.mprodetailid == ab.DetailId);
                    var attributefilters = (from g in getallproducts
                                            join subtwo in ctx.productsubtwoes on g.mprodid equals subtwo.mprodid
                                            where subtwo.mdetailheadid == ab.DetailId && subtwo.mdetailvalue != "" && subtwo.mdetailvalue != null
                                            select subtwo).GroupBy(x => x.mdetailvalue.ToUpperInvariant()).Select(y => new ProductFilterDetailViewModel
                                            {
                                                FilterId = y.FirstOrDefault().mdetailheadid.Value,
                                                FilterUrl = "a",
                                                FilterValue = y.Key
                                            }).OrderBy(x => x.FilterValue).ToList();
                    if (attributefilters.Any())
                    {
                        filters.Add(new ProductFilterViewModel
                        {
                            FilterHead = getdetail.mdetailhead,
                            FilterData = attributefilters
                        });
                    }
                }
            }
            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            return Json(new { cnt = cnt, minprice = minprice, maxprice = maxprice, allFilter = filters });
        }

        [HttpGet]
        [Route("api/ProductApi/GetSearchList")]
        public IHttpActionResult GetSearchList(string name, int cat)
        {
            name = name.Trim();
            var db = new LeasingDbEntities();
            var List = new List<ProductSearchViewModel>();
            var alist = new List<ProductSearchViewModel>();
            var blist = new List<ProductSearchViewModel>();
            var ptlist = new List<ProductSearchViewModel>();
            var brandlist = new List<ProductSearchViewModel>();
            var mpnlist = new List<ProductSearchViewModel>();

            if (cat == 0 && name != "")
            {

                alist = (from p in db.productcats
                         where (p.mcatname.StartsWith(name))
                         select new ProductSearchViewModel
                         {
                             mprodname = p.mcatname,
                             namewithflag = "c"
                         }).Take(10).ToList();
                List.AddRange(alist);
                if (List.Count < 10)
                {
                    ////if (psdata.Any())
                    ////{
                    //ptlist = (from pt in db.productdetailmasters
                    //          join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                    //          join p1 in db.products on ps.mprodid equals p1.mprodid
                    //          join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                    //          join psub in db.productsubs on p1.mprodid equals psub.mprodid
                    //          join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                    //          join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                    //          where pt.mdetailhead == "Product Type" && ps.mdetailvalue != null && ps.mdetailvalue.Trim().ToLower().Contains(name.ToLower()) && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                    //          group new
                    //          {
                    //              ps.mdetailheadid,
                    //          } by new
                    //          {
                    //              ps.mdetailvalue
                    //          } into y
                    //          select new ProductSearchViewModel()
                    //          {
                    //              mprodname = y.Key.mdetailvalue,
                    //              namewithflag = "pt"
                    //          }).Take(10 - List.Count).ToList();
                    //List.AddRange(ptlist);
                    ////}

                    name = name.ToLower();
                    ptlist = (from pt in db.productdetailmasters
                              join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                              join p1 in db.products on ps.mprodid equals p1.mprodid
                              join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                              join psub in db.productsubs on p1.mprodid equals psub.mprodid
                              join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                              join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                              let detailvalue = ps.mdetailvalue != null && ps.mdetailvalue != "" ? ps.mdetailvalue.ToLower().Trim() : ""
                              where pt.mdetailhead == "Product Type" && detailvalue.StartsWith(name) && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                              group new
                              {
                                  ps.mdetailheadid,
                              } by new
                              {
                                  ps.mdetailvalue
                              } into y
                              select new ProductSearchViewModel()
                              {
                                  mprodname = y.Key.mdetailvalue,
                                  namewithflag = "pt"
                              }).Take(10 - List.Count).ToList();
                    List.AddRange(ptlist);
                }
                if (List.Count < 10)
                {
                    var getbrand = db.productbrands.Where(x => x.mbrandname.StartsWith(name)).ToList();
                    if (getbrand.Any())
                    {
                        foreach (var ab in getbrand)
                        {
                            var searchtype = ab.mbrandname;

                            brandlist = (from pt in db.productdetailmasters
                                         join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                                         join p1 in db.products on ps.mprodid equals p1.mprodid
                                         join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                                         join psub in db.productsubs on p1.mprodid equals psub.mprodid
                                         join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                                         join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                                         let newname = ps.mdetailvalue != null && ps.mdetailvalue != "" ? searchtype + " " + ps.mdetailvalue : ""
                                         where pt.mdetailhead == "Product Type" && newname.StartsWith(searchtype) && ab.mbrandid == p1.mbrand && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                                         group new
                                         {
                                             newname,
                                             ps.mdetailheadid,
                                         } by new
                                         {
                                             ps.mdetailvalue
                                         } into y
                                         select new ProductSearchViewModel()
                                         {
                                             bname = searchtype + "$" + y.Key.mdetailvalue,
                                             mprodname = searchtype + " " + y.Key.mdetailvalue,
                                             namewithflag = "bpt"
                                         }).Take(10 - List.Count).ToList();
                            List.AddRange(brandlist);
                        }

                    }
                    else
                    {
                        var splitdata = name.Split(' ');
                        var bname = splitdata[0];
                        var getbrandnew = db.productbrands.Where(x => x.mbrandname.StartsWith(bname)).ToList();
                        if (getbrandnew.Any())
                        {
                            foreach (var ab in getbrandnew)
                            {
                                var searchtype = ab.mbrandname;

                                brandlist = (from pt in db.productdetailmasters
                                             join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                                             join p1 in db.products on ps.mprodid equals p1.mprodid
                                             join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                                             join psub in db.productsubs on p1.mprodid equals psub.mprodid
                                             join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                                             join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                                             let newname = ps.mdetailvalue != null && ps.mdetailvalue != "" ? searchtype + " " + ps.mdetailvalue : ""
                                             where pt.mdetailhead == "Product Type" && newname.StartsWith(searchtype) && ab.mbrandid == p1.mbrand && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                                             group new
                                             {
                                                 newname,
                                                 ps.mdetailheadid,
                                             } by new
                                             {
                                                 ps.mdetailvalue
                                             } into y
                                             select new ProductSearchViewModel()
                                             {
                                                 bname = searchtype + "$" + y.Key.mdetailvalue,
                                                 mprodname = searchtype + " " + y.Key.mdetailvalue,
                                                 namewithflag = "bpt"
                                             }).Take(10 - List.Count).ToList();
                                List.AddRange(brandlist);
                            }

                        }
                    }

                }
                if (List.Count < 10)
                {
                    blist = (from p in db.products
                             where p.mprodname.StartsWith(name)
                             select new ProductSearchViewModel
                             {
                                 mprodname = p.mprodname,
                                 namewithflag = "p"
                             }).Take(10 - List.Count).ToList();
                    List.AddRange(blist);
                }
                if (List.Count < 10)
                {
                    blist = (from p in db.products
                             where p.mmanufacturercode.StartsWith(name)
                             select new ProductSearchViewModel
                             {
                                 mprodname = p.mmanufacturercode,
                                 namewithflag = "mpn"
                             }).Take(10 - List.Count).ToList();
                    List.AddRange(blist);
                }

            }
            else
            {
                if (name != "")
                {
                    alist = (from p in db.productcats
                             where (p.mcatname.StartsWith(name) && (p.mpreviouscat == cat || p.mprodcatid == cat))
                             select new ProductSearchViewModel
                             {
                                 mprodname = p.mcatname,
                                 namewithflag = "c"
                             }).Take(10).ToList();
                    List.AddRange(alist);
                    if (List.Count < 10)
                    {
                        ptlist = (from pt in db.productdetailmasters
                                  join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                                  join p1 in db.products on ps.mprodid equals p1.mprodid
                                  join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                                  join psub in db.productsubs on p1.mprodid equals psub.mprodid
                                  join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                                  join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                                  where pt.mdetailhead == "Product Type" && ps.mdetailvalue.StartsWith(name) && (pc1.mpreviouscat == cat || pc1.mprodcatid == cat) && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)

                                  group new
                                  {
                                      ps.mdetailheadid,
                                  } by new
                                  {
                                      ps.mdetailvalue
                                  } into y
                                  select new ProductSearchViewModel()
                                  {
                                      mprodname = y.Key.mdetailvalue,
                                      namewithflag = "pt"
                                  }).Take(10 - List.Count).ToList();
                        List.AddRange(ptlist);
                    }
                    if (List.Count < 10)
                    {
                        var getbrand = db.productbrands.Where(x => x.mbrandname.StartsWith(name)).ToList();
                        if (getbrand.Any())
                        {
                            foreach (var ab in getbrand)
                            {
                                var searchtype = ab.mbrandname;
                                brandlist = (from pt in db.productdetailmasters
                                             join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                                             join p1 in db.products on ps.mprodid equals p1.mprodid
                                             join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                                             join psub in db.productsubs on p1.mprodid equals psub.mprodid
                                             join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                                             join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                                             where pt.mdetailhead == "Product Type" && ps.mdetailvalue.StartsWith(name) && p1.mbrand == ab.mbrandid && (pc1.mpreviouscat == cat || pc1.mprodcatid == cat) && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                                             group new
                                             {
                                                 ps.mdetailheadid,
                                             } by new
                                             {
                                                 ps.mdetailvalue
                                             } into y
                                             select new ProductSearchViewModel()
                                             {
                                                 bname = searchtype + "$" + y.Key.mdetailvalue,
                                                 mprodname = searchtype + " " + y.Key.mdetailvalue,
                                                 namewithflag = "bpt"
                                             }).Take(10 - List.Count).ToList();
                                List.AddRange(brandlist);
                            }

                        }
                        else
                        {
                            var splitdata = name.Split(' ');
                            var getbrandnew = db.productbrands.Where(x => x.mbrandname.StartsWith(splitdata[0])).ToList();
                            if (getbrandnew.Any())
                            {
                                foreach (var ab in getbrandnew)
                                {
                                    var searchtype = ab.mbrandname;

                                    brandlist = (from pt in db.productdetailmasters
                                                 join ps in db.productsubtwoes on pt.mprodetailid equals ps.mdetailheadid
                                                 join p1 in db.products on ps.mprodid equals p1.mprodid
                                                 join pc1 in db.productcats on p1.mprodcatid equals pc1.mprodcatid
                                                 join psub in db.productsubs on p1.mprodid equals psub.mprodid
                                                 join psupps in db.ProductSubSuppliers on psub.mprodsubid equals psupps.ProdSubId
                                                 join userss in db.supplier_registration on psupps.SupplierId equals userss.msuid
                                                 let newname = ps.mdetailvalue != null && ps.mdetailvalue != "" ? searchtype + " " + ps.mdetailvalue : ""
                                                 where pt.mdetailhead == "Product Type" && newname.StartsWith(searchtype) && ab.mbrandid == p1.mbrand && (pc1.mpreviouscat == cat || pc1.mprodcatid == cat) && p1.papprove == 1 && p1.mpactive == "yes" && psupps.mrp > 0 && userss.mstatus == "active" && psupps.mactive == "yes" && ((pc1.minstallation == 1 && psupps.minstallation > 0) || pc1.minstallation == 0 || pc1.minstallation == null)
                                                 group new
                                                 {
                                                     newname,
                                                     ps.mdetailheadid,
                                                 } by new
                                                 {
                                                     ps.mdetailvalue
                                                 } into y
                                                 select new ProductSearchViewModel()
                                                 {
                                                     bname = searchtype + "$" + y.Key.mdetailvalue,
                                                     mprodname = searchtype + " " + y.Key.mdetailvalue,
                                                     namewithflag = "bpt"
                                                 }).Take(10 - List.Count).ToList();
                                    List.AddRange(brandlist);
                                }

                            }
                        }
                    }
                    if (List.Count < 10)
                    {
                        blist = (from p in db.products
                                 join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                                 where p.mprodname.StartsWith(name) && (pc.mpreviouscat == cat || pc.mprodcatid == cat)
                                 select new ProductSearchViewModel
                                 {
                                     mprodname = p.mprodname,
                                     namewithflag = "p"
                                 }).Take(10 - List.Count).ToList();
                        List.AddRange(blist);
                    }
                    if (List.Count < 10)
                    {
                        blist = (from p in db.products
                                 join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                                 where p.mmanufacturercode.StartsWith(name) && (pc.mpreviouscat == cat || pc.mprodcatid == cat)
                                 select new ProductSearchViewModel
                                 {
                                     mprodname = p.mmanufacturercode,
                                     namewithflag = "mpn"
                                 }).Take(10 - List.Count).ToList();
                        List.AddRange(blist);
                    }
                    //alist = (from p in db.productcats
                    //         where (p.mcatname.StartsWith(name) || p.mcatname.Contains(name)) && (p.mpreviouscat == cat || p.mprodcatid == cat)
                    //         select new ProductSearchViewModel
                    //         {
                    //             mprodname = p.mcatname,
                    //         }).Take(10).ToList();
                    //List.AddRange(alist);
                    //blist = (from p in db.products
                    //         join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                    //         where p.mprodname.StartsWith(name) || p.mprodname.Contains(name) && pc.mprodcatid == cat
                    //         select new ProductSearchViewModel
                    //         {
                    //             mprodname = p.mprodname,
                    //         }).Take(10).ToList();
                    //List.AddRange(alist);
                    //List.AddRange(blist);
                }
            }
            if (List.Count() > 1)
            {
                var newlist = List.GroupBy(x => x.mprodname).Select(y => new ProductSearchViewModel
                {
                    mprodname = y.FirstOrDefault().mprodname,
                    id = y.FirstOrDefault().id,
                    namewithflag = y.FirstOrDefault().namewithflag,
                    bname = y.FirstOrDefault().bname
                }).ToList();
                List = newlist;
                //return Ok(newlist.Take(10).Distinct().ToList());
            }

            List = List.Take(10).ToList();
            return Ok(List.Distinct().ToList());


        }

        [HttpGet]
        [Route("api/ProductApi/GetWeekDealProduct")]
        public IHttpActionResult GetWeekDealProduct()
        {
            var db = new LeasingDbEntities();

            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<WeekdealProductDisplayViewModel> products = null;
            //string cnnString = System.Configuration.ConfigurationManager.ConnectionStrings["BackgroundConString"].ConnectionString;
            //SqlConnection cnn = new SqlConnection(cnnString);
            //SqlCommand cmd = new SqlCommand();
            //cmd.Connection = cnn;
            //cmd.CommandType = System.Data.CommandType.StoredProcedure;
            //cmd.CommandText = "spGetWeekDealProducts";
            ////add any parameters the stored procedure might require
            //cnn.Open();
            //object o = cmd.ExecuteScalar();
            //cnn.Close();
            var pnew = db.spGetWeekDealProducts().ToList();
            products = (from p in pnew
                        group new
                        {
                            p.sellername,
                            p.mmanufacturer,
                            p.mbrandlogo,
                            p.mleadtime,
                            p.mprodsubid,
                            p.mrp,
                            p.mofferprice,
                            p.msupplierid,
                            p.mqty,
                            p.mofferdetail,
                            p.msupplierwarranty,
                            p.msupplierwarranty2,
                            p.msupplierwarranty3,
                            p.msupplierwarranty4,
                            p.msupplierwarranty5,
                            p.mwarrantyterm,
                            p.minstallation,
                            p.mprodcatid,
                            p.mcatname,
                            p.mbrand,
                            p.mprodname,
                            p.mbrandname,
                            p.mfeatured,
                            p.mnewarrival,
                            p.mpactive,
                            p.mweekdeal,
                            p.muniquecode,
                            p.mmanufacturercode,
                        } by new
                        {
                            p.mprodid,

                        } into y
                        orderby y.Key.mprodid
                        select new WeekdealProductDisplayViewModel
                        {
                            mprodvarid = y.FirstOrDefault().mprodsubid.HasValue ? y.FirstOrDefault().mprodsubid.Value : 0,
                            mproductcatid = y.FirstOrDefault().mprodcatid,
                            mprodid = y.Key.mprodid,
                            mprodname = y.FirstOrDefault().mprodname,
                            mbrandlogo=y.FirstOrDefault().mbrandlogo,
                            mprodcatname = y.FirstOrDefault().mcatname,
                            mfeatured = y.FirstOrDefault().mfeatured,
                            mnewarrival = y.FirstOrDefault().mnewarrival,
                            mweekdeal = y.FirstOrDefault().mweekdeal,
                            mprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mbasicprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mleadtime = y.FirstOrDefault().mleadtime.HasValue ? y.FirstOrDefault().mleadtime.Value : 3,
                            msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                            mqty = y.FirstOrDefault().mqty.HasValue ? y.FirstOrDefault().mqty.Value : 0,
                            mofferdetail = y.FirstOrDefault().mofferdetail,
                            minstallflag = y.FirstOrDefault().minstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty.HasValue ? y.FirstOrDefault().msupplierwarranty.Value : 0,
                            msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2.HasValue ? y.FirstOrDefault().msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3.HasValue ? y.FirstOrDefault().msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4.HasValue ? y.FirstOrDefault().msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5.HasValue ? y.FirstOrDefault().msupplierwarranty5.Value : 0,
                            manufacturewarranty = y.FirstOrDefault().mwarrantyterm.HasValue ? y.FirstOrDefault().mwarrantyterm.Value : 1,
                            MPN = y.FirstOrDefault().mmanufacturercode,
                            EYS = y.FirstOrDefault().muniquecode
                        }).ToList<WeekdealProductDisplayViewModel>();


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
                var getrating = getStarRating(x.mprodvarid, db);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mdiscount = 100 - ((x.mprice / x.mbasicprice) * 100);
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
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
            var pnew = db.spGetFeatureProducts().ToList();
            products = (from p in pnew
                        select new FeatureProductDisplayViewModel
                        {
                            mbrandlogo=p.mbrandlogo,
                            mprodvarid = p.mprodsubid.HasValue ? p.mprodsubid.Value : 0,
                            mproductcatid = p.mprodcatid.Value,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mfeatured = p.mfeatured,
                            mnewarrival = p.mnewarrival,
                            mweekdeal = p.mweekdeal,
                            minstallflag = p.minstallation == 1 ? "yes" : "no",
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<FeatureProductDisplayViewModel>();


            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, db);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                var subpicdata = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Ok(products);
        }

        [HttpPost]
        [Route("api/ProductApi/GetAllFeatureProduct")]
        public IHttpActionResult GetAllFeatureProduct(ProductParameterViewModel param)
        {
            //var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var getid = param.getid;
            var row = param.row;
            var rowperpage = param.rowperpage;
            var filter = param.filter;
            var id = getid;
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetFeatureProducts().Where(x => x.mfeatured == 1).ToList();
            IEnumerable<spGetFeatureProducts_Result> getprod = getallproducts;
            if (param.min > 0 && param.max > 0)
            {
                getprod = (from p in getallproducts
                           where  p.mrp >= param.min && p.mrp <= param.max
                           select p);
            }
            if (param.filterdata.Any(x => x.FilterUrl == "bn"))
            {
                getprod = (from p in getprod
                           where param.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                           select p);
            }
            //if (getprod == null)
            //{
            //    getprod = (from p in getallproducts
            //               select p);

            //}
            var finalprod = getprod.ToList();
            products = (from p in finalprod
                        select new ProductDisplayViewModel()
                        {
                            mbrandlogo=p.mbrandlogo,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid.Value,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });

            }

            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            if (param.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            products.ForEach(x =>
            {
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                var subpicdata = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
        }

        /****************************************End Feature Product*****************************************/

        /***************************************New Arrival Product***********************************************/
        [HttpPost]
        [Route("api/ProductApi/GetAllNewArrivalProduct")]
        public IHttpActionResult GetAllNewArrivalProduct(ProductParameterViewModel param)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var getid = param.getid;
            var row = param.row;
            var rowperpage = param.rowperpage;
            var filter = param.filter;
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetNewArrivalProducts().ToList();
            IEnumerable<spGetNewArrivalProducts_Result> getprod = null;
            if (param.min > 0 && param.max > 0)
            {
                getprod = (from p in getallproducts
                           where p.mrp >= param.min && p.mrp <= param.max
                           select p);
            }
            if (param.filterdata.Any(x => x.FilterUrl == "bn"))
            {
                getprod = (from p in getallproducts
                           where param.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                           select p);
            }

            if (getprod == null)
            {
                getprod = (from p in getallproducts
                           select p);

            }
            var finalprod = getprod.ToList();
            products = (from p in finalprod
                        select new ProductDisplayViewModel()
                        {
                            mbrandlogo = p.mbrandlogo,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid.Value,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });

            }

            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            if (param.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                var subpicdata = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
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
            var pnew = db.spGetOnSaleProducts().ToList();
            products = (from p in pnew
                        select new FeatureProductDisplayViewModel
                        {
                            mbrandlogo = p.mbrandlogo,
                            mprodvarid = p.mprodsubid.HasValue ? p.mprodsubid.Value : 0,
                            mproductcatid = p.mprodcatid.Value,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mfeatured = p.mfeatured,
                            mnewarrival = p.mnewarrival,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            minstallflag = p.minstallation == 1 ? "yes" : "no",
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mofferprice.HasValue ? p.mofferprice.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mofferprice.HasValue ? p.mofferprice.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mofferprice.HasValue ? p.mofferprice.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mofferprice.HasValue ? p.mofferprice.Value : 0),
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<FeatureProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok(products);
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, db);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                var subpicdata = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Ok(products);
        }
        [HttpPost]
        [Route("api/ProductApi/GetAllOnSaleProduct")]
        public IHttpActionResult GetAllOnSaleProduct(ProductParameterViewModel param)
        {
            var db = new LeasingDbEntities();
            var getid = param.getid;
            var row = param.row;
            var rowperpage = param.rowperpage;
            var filter = param.filter;
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var filters = new List<ProductFilterViewModel>();
            var getallproducts = ctx.spGetOnSaleProducts().ToList();
            IEnumerable<spGetOnSaleProducts_Result> getprod = null;
            if (param.min > 0 && param.max > 0)
            {
                getprod = (from p in getallproducts
                           where p.mrp >= param.min && p.mrp <= param.max
                           select p);
            }
            if (param.filterdata.Any(x => x.FilterUrl == "bn"))
            {
                getprod = (from p in getallproducts
                           where param.filterdata.Any(x => x.FilterUrl == "bn" && x.FilterValue.ToUpper() == p.mbrandname.ToUpper())
                           select p);
            }

            if (getprod == null)
            {
                getprod = (from p in getallproducts

                           select p);

            }
            var finalprod = getprod.ToList();
            products = (from p in finalprod
                        select new ProductDisplayViewModel()
                        {
                            mbrandlogo = p.mbrandlogo,
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mofferprice.HasValue ? p.mofferprice.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mofferprice.HasValue ? p.mofferprice.Value : 0),
                            mbasicprice = (p.pcinstallation == 1 && p.minstallation > 0 ? ((p.mofferprice.HasValue ? p.mofferprice.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mofferprice.HasValue ? p.mofferprice.Value : 0),
                            mprodvarid = p.mprodsubid.Value,
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            mleadtime = p.mleadtime.HasValue ? p.mleadtime.Value : 3,
                            minstallflag = p.pcinstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            if (filter)
            {
                var brandfilters = products.GroupBy(x => x.mbrand).Select(y => new ProductFilterDetailViewModel
                {
                    FilterId = y.FirstOrDefault().mbrandid,
                    FilterUrl = "bn",
                    FilterValue = y.FirstOrDefault().mbrand
                }).Distinct().ToList();
                filters.Add(new ProductFilterViewModel
                {
                    FilterHead = "Brand",
                    FilterData = brandfilters


                });

            }

            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;

            if (param.sortby == 2)
            {
                products = products.OrderByDescending(x => x.mprodname).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 3)
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else if (param.sortby == 4)
            {
                products = products.OrderByDescending(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            else
            {
                products = products.OrderBy(x => x.mprice).Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, db);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                var subpicdata = db.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid);
                x.msubpicname = subpicdata != null ? subpicdata.msubprodpicname : "";
            });

            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
        }

        /***************************************On Sale Product***********************************************/

        [HttpGet]
        public IHttpActionResult OldGetAllProductById(int getid, int row, int rowperpage)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from p in ctx.products
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid
                        //let psupp= ctx.ProductSubSuppliers.Where(x=>x.ProdSubId==pcv.mprodsubid && x.approve == 1 && x.mactive == "yes") .OrderBy(x=>x.mrp).FirstOrDefault()
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.mprodcatid == id && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && users.mstatus == "active" && psupp.mactive == "yes" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).OrderBy(x => x.mprice).ToList();

            products = products.GroupBy(x => x.mprodvarid).Select(y => new ProductDisplayViewModel()
            {
                mprodid = y.FirstOrDefault().mprodid,
                mprodname = y.FirstOrDefault().mprodname,
                mprodcatname = y.FirstOrDefault().mprodcatname,
                mbrandid = y.FirstOrDefault().mbrandid,
                mbrand = y.FirstOrDefault().mbrand,
                mtermf = 1,
                mtermp = 60,
                msuppliername = y.FirstOrDefault().msuppliername,
                //msubpicname = y.Key.pic,
                minstallflag = y.FirstOrDefault().minstallflag,
                mprice = y.FirstOrDefault().mprice,
                mbasicprice = y.FirstOrDefault().mbasicprice,
                mprodvarid = y.FirstOrDefault().mprodvarid,
                msupplierid = y.FirstOrDefault().msupplierid,
                mleadtime = y.FirstOrDefault().mleadtime,
                msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty1,
                msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2,
                msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3,
                msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4,
                msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5,
                manufacturewarranty = y.FirstOrDefault().manufacturewarranty,
                MPN = y.FirstOrDefault().MPN,
                EYS = y.FirstOrDefault().EYS,
            }).Distinct().OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;
            products = products.Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice });
        }
        [Route("api/productapi/GetCategoryProductById")]
        [HttpGet]
        public IHttpActionResult GetCategoryProductById(int getid, int row, int rowperpage)
        {

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
                        from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where pc.mpreviouscat == id && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return Ok("");
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        [Route("api/productapi/GetCategoryByAreaId")]
        [HttpGet]
        public IHttpActionResult GetCategoryByAreaId(int getid)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductCategoryDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var getcats = ctx.tblschoolareas.FirstOrDefault(x => x.mareaid == id);
            if (getcats != null)
            {
                var catlist = getcats.massigncategory.Split(',').ToList();
                foreach (var a in catlist)
                {
                    var catid = int.Parse(a);
                    var subprod = ctx.productcats.FirstOrDefault(x => x.mprodcatid == catid);
                    if (subprod != null)
                    {
                        if (products == null)
                        {
                            products = new List<ProductCategoryDisplayViewModel>();
                        }
                        var mproducturl = "";
                        var getsubcat = ctx.productcats.FirstOrDefault(x => x.mpreviouscat == catid);
                        if (getsubcat == null)
                        {

                            mproducturl = "/Leasing/Products?Category=" + HttpUtility.UrlEncode(subprod.mcatname) + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(subprod.mprodcatid.ToString()));
                        }
                        else
                        {
                            mproducturl = "/Leasing/ShopByArea?Dept=cat&a=" + HttpUtility.UrlEncode(Encryption.Encrypt(subprod.mprodcatid.ToString())) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(subprod.mcatname.ToString()));
                        }
                        products.Add(new ProductCategoryDisplayViewModel
                        {
                            mcatid = catid,
                            mcatname = subprod.mcatname,
                            mcatpic = subprod.mcatimage,
                            mproducturl = mproducturl
                        });
                    }
                }
            }

            return Ok(products);
        }
        [Route("api/productapi/GetCategoryByCategoryId")]
        [HttpGet]
        public IHttpActionResult GetCategoryByCategoryId(int getid)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductCategoryDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var subprod = ctx.productcats.Where(x => x.mpreviouscat == getid).ToList();
            if (subprod.Any())
            {
                foreach (var ab in subprod)
                {
                    if (products == null)
                    {
                        products = new List<ProductCategoryDisplayViewModel>();
                    }
                    var mproducturl = "";
                    var getsubcat = ctx.productcats.FirstOrDefault(x => x.mpreviouscat == ab.mprodcatid);
                    if (getsubcat == null)
                    {

                        mproducturl = "/Leasing/Products?Category=" + HttpUtility.UrlEncode(ab.mcatname) + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(ab.mprodcatid.ToString()));
                    }
                    else
                    {
                        mproducturl = "/Leasing/ShopByArea?Dept=cat&a=" + HttpUtility.UrlEncode(Encryption.Encrypt(ab.mprodcatid.ToString())) + "&p=" + HttpUtility.UrlEncode(Encryption.Encrypt(ab.mcatname.ToString()));
                    }
                    products.Add(new ProductCategoryDisplayViewModel
                    {
                        mcatid = getid,
                        mcatname = ab.mcatname,
                        mcatpic = ab.mcatimage,
                        mproducturl = mproducturl
                    });

                }
            }

            return Ok(products);
        }
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
        [Route("api/ProductApi/GetRelatedProductById")]
        public IHttpActionResult GetRelatedProductById(int getid, int exceptid)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            var getallproducts = ctx.spGetProducts(getid).ToList();
            products = (from p in getallproducts
                        where p.mprodsubid != exceptid
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = p.mcatname,
                            mbrandid = p.mbrandid,
                            mbrand = p.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = p.morgname.Substring(0, 3),
                            distributor = p.sellername != null && p.sellername != "" ? p.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (p.minstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mbasicprice = (p.minstallation == 1 && p.minstallation > 0 ? ((p.mrp.HasValue ? p.mrp.Value : 0) + (p.minstallation.HasValue ? p.minstallation.Value : 0)) : p.mrp.HasValue ? p.mrp.Value : 0),
                            mprodvarid = p.mprodsubid,
                            minstallflag = p.minstallation == 1 ? "yes" : "no",
                            msupplierid = p.SupplierId.HasValue ? p.SupplierId.Value : 0,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = p.muniquecode
                        }).ToList<ProductDisplayViewModel>();


            if (products.Count == 0)
            {
                return NotFound();
            }
            products = products.Take(5).ToList();
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }
        [HttpGet]
        [Route("api/ProductApi/GetAddOnProductById")]
        public IHttpActionResult GetAddOnProductById(int getid)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = getid;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = new List<ProductDisplayViewModel>();
            var ctx = new LeasingDbEntities();
            var paddon = ctx.productaddondetails.Where(x => x.mselecteprod == id);
            if (paddon.Any())
            {
                foreach (var ab in paddon)
                {
                    var p2 = (from pcv in ctx.productsubs
                              join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                              join p in ctx.products on pcv.mprodid equals p.mprodid
                              join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                              join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                              join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                              from users in userdata.DefaultIfEmpty()
                              where psupp.Id == ab.maddonprodid && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                              select new ProductDisplayViewModel()
                              {
                                  mprodid = p.mprodid,

                                  mprodname = p.mprodname,
                                  mprodcatname = pc.mcatname,
                                  mbrandid = pbrand.mbrandid,
                                  mbrand = pbrand.mbrandname,
                                  mtermf = 1,
                                  mtermp = 60,
                                  supcode = users.morgname.Substring(0, 3),
                                  distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                                  msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                                  //msubpicname = y.Key.pic,
                                  mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                  mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                  mprodvarid = pcv.mprodsubid,
                                  minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                  msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                  msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                  msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                  msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                  msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                  msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                  manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                  MPN = p.mmanufacturercode,
                                  EYS = pcv.muniquecode
                              }).FirstOrDefault();
                    if (p2 != null)
                    {
                        products.Add(p2);
                    }


                }

                if (products.Count == 0)
                {
                    return Ok("failed");
                }
                products = products.ToList();
                products.ForEach(x =>
                {
                    var getrating = getStarRating(x.mprodvarid, ctx);
                    if (getrating.Any())
                    {
                        x.avgrating = getrating[0];
                        x.totalrating = getrating[1];

                    }
                    x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                    x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                    x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
                });

                return Ok(products);
            }
            return Ok("failed");
        }

        [HttpGet]
        [Route("api/ProductApi/GetMoreProductById")]
        public IHttpActionResult GetMoreProductById(int getid)
        {

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
                        from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        where p.mprodid == id && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = 1,
                            mtermp = 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }
        [HttpPost]
        [Route("api/ProductApi/GetOfferProduct")]
        public IHttpActionResult GetOfferProduct(CartProductViewModel item)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = 0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            List<int> ProdIds = new List<int>();
            List<ProductSubTwoViewModel> pd = new List<ProductSubTwoViewModel>();
            var ctx = new LeasingDbEntities();

            /*products = (from pcv in ctx.productsubs
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where p.mprodid == item.mprodid && pcv.mprodsubid != item.mprodvarid && psupp.SupplierId != item.msupplierid && psupp.mrp > 0 && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mbrandlogo=pbrand.mphoto,
                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).ToList<ProductDisplayViewModel>();
            */
            //if (products.Count <= 5)
            //{
            products = (from pcv in ctx.productsubs
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where p.mprodid == item.mprodid && pcv.mprodsubid == item.mprodvarid && psupp.SupplierId == item.msupplierid && psupp.mrp > 0 && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,
                            mbrandlogo = pbrand.mphoto,
                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).ToList<ProductDisplayViewModel>();
            var pcat = (from pcv in ctx.productsubs
                            join p in ctx.products on pcv.mprodid equals p.mprodid
                            where pcv.mprodsubid == item.mprodvarid
                            select new
                            {
                                catid = p.mprodcatid
                            }).FirstOrDefault().catid;
                var getdetaildatas = ctx.productcomparativefields.Where(x => x.Compare > 0 && x.CategoryId==pcat.Value).OrderBy(x => x.Compare).ToList();
                if (getdetaildatas.Any())
                {
                    try
                    {
                        foreach (var ab in getdetaildatas)
                        {
                            var proddata = ctx.productsubtwoes.FirstOrDefault(x => x.mprodid == item.mprodid && x.mdetailheadid == ab.DetailId);
                            if (proddata != null && proddata.mdetailvalue != null)
                            {
                                pd.Add(new ProductSubTwoViewModel
                                {
                                    mdetailheadid = ab.DetailId,
                                    mdetailvalue = proddata.mdetailvalue
                                });
                            }

                        }
                        //var prodids = ctx.productsubtwoes.SqlQuery(query);
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                }
                var cnt = 0;
            /*if (pd.Count > 0)
            {
                var getall = getallquery(pd, 0);
                var pids = (from ps in ctx.productsubtwoes.Where(getall)
                            select ps).GroupBy(x => x.mprodid).ToList();
                
            }*/
            List<int> mb = new List<int>();
            while (ProdIds.Count < 4 && pd.Count > cnt)
            {
                var query = getquery(pd, cnt);
                var pids = (from ps in ctx.productsubtwoes.Where(q=>query.mdetailhead.Any(x=>x==q.mdetailheadid) && query.mdetailvalue.Any(x=>x==q.mdetailvalue))
                            join p in ctx.products on ps.mprodid equals p.mprodid
                            where p.mbrand!=item.mbrandid.Value && !mb.Any(x=>x==p.mbrand)
                            select p).GroupBy(x=>x.mprodid).Where(g=>g.Count()>=(pd.Count-cnt)).SelectMany(x=>x).Take(10).ToList();
                pids.ForEach(x =>
                {
                    if (!ProdIds.Any(z => z == x.mprodid) && !mb.Any(q=>q==x.mbrand))
                    {
                        ProdIds.Add(x.mprodid);
                        mb.Add(x.mbrand.Value);
                    }
                        
                });
                cnt++;
            }
            while (ProdIds.Count < 4 && pd.Count > cnt)
                {
                    //var query = "mprodid!= " + item.mprodid + " and " + getquery(pd, cnt);
                    //var pids = ctx.productsubtwoes.Where(query).Take(10).ToList();
                var query = getquery(pd, cnt);
                var pids = (from ps in ctx.productsubtwoes.Where(q => query.mdetailhead.Any(x => x == q.mdetailheadid) && query.mdetailvalue.Any(x => x == q.mdetailvalue))
                            join p in ctx.products on ps.mprodid equals p.mprodid
                            where p.mprodid != item.mprodid && !mb.Any(x => x == p.mbrand)
                            select p).GroupBy(x => x.mprodid).Where(g => g.Count() >= (pd.Count - cnt)).SelectMany(x => x).Take(10).ToList();
                pids.ForEach(x =>
                    {
                        if (!ProdIds.Any(z => z == x.mprodid))
                            ProdIds.Add(x.mprodid);
                    });
                    cnt++;
                }
                if (ProdIds.Count > 0)
                {
                    foreach (var ap in ProdIds.Take(4).ToList())
                    {
                        var pdatas = (from p in ctx.products
                                      join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                      join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                      join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid
                                      join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                                      from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                                      join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                      from users in userdata.DefaultIfEmpty()
                                      let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                                      where p.mprodid == ap && p.mprodcatid == pcat && pcv.mprodsubid != item.mprodvarid && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                                      select new ProductDisplayViewModel()
                                      {
                                          mprodid = p.mprodid,
                                          mbrandlogo = pbrand.mphoto,
                                          mprodname = p.mprodname,
                                          mprodcatname = pc.mcatname,
                                          mbrandid = pbrand.mbrandid,
                                          mbrand = pbrand.mbrandname,
                                          mtermf = 1,
                                          mtermp = 60,
                                          supcode = users.morgname.Substring(0, 3),
                                          distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                                          msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                                          //msubpicname = y.Key.pic,
                                          mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                          mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                          mprodvarid = pcv.mprodsubid,
                                          minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                          msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                          mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                          msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                          msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                          msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                          msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                          msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                          manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                          MPN = p.mmanufacturercode,
                                          EYS = pcv.muniquecode
                                      }).FirstOrDefault<ProductDisplayViewModel>();
                        if (pdatas != null)
                        {
                            products.Add(pdatas);
                        }
                    }
                }
                if (products.Count < 4)
                {
                    if (pcat > 0)
                    {
                        var d = (from p in ctx.products
                                 join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                 join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                 join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                 from pcv in pcvdata.DefaultIfEmpty()
                                 join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                                 from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                                 join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                 from users in userdata.DefaultIfEmpty()
                                 let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                                 where p.mprodcatid == pcat && pcv.mprodsubid != item.mprodvarid && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                                 select new ProductDisplayViewModel()
                                 {
                                     mprodid = p.mprodid,
                                     mbrandlogo = pbrand.mphoto,
                                     mprodname = p.mprodname,
                                     mprodcatname = pc.mcatname,
                                     mbrandid = pbrand.mbrandid,
                                     mbrand = pbrand.mbrandname,
                                     mtermf = 1,
                                     mtermp = 60,
                                     supcode = users.morgname.Substring(0, 3),
                                     distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                                     msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                                     //msubpicname = y.Key.pic,
                                     mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                     mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                     mprodvarid = pcv.mprodsubid,
                                     minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                     msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                     mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                     msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                     msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                     msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                     msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                     msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                     manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                     MPN = p.mmanufacturercode,
                                     EYS = pcv.muniquecode
                                 }).Take(4).ToList<ProductDisplayViewModel>();
                        if (d.Count > 0)
                        {
                            if (products.Count > 0)
                            {
                                d.ForEach(x => products.Add(x));

                            }
                            else
                            {
                                products = d;
                            }

                        }
                    }
                }


            //}
            if (products.Count < 0)
            {
                return Ok("empty");
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetOfferSpecs(x.mprodid);
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            return Ok(products.Take(5).ToList());
        }
        public QueryListViewModel getquery(List<ProductSubTwoViewModel> pds, int cnt)
        {
            var query = new QueryListViewModel() ;
            var pddata = pds;
            var pdcnt = pddata.Count;
            var mdetailhead = new List<int>();
            var mdetailvalue = new List<string>();
            //pddata.RemoveRange(pds.Count - cnt - 1, cnt);
            if (pddata.Any())
            {

                for (var i = 0; i < pdcnt - cnt; i++)
                {

                    /*if (mdetailhead != "")
                    {
                        mdetailhead += ",";
                        mdetailvalue += ",";
                    }*/
                    mdetailhead.Add(pddata[i].mdetailheadid.Value);
                    mdetailvalue.Add(pddata[i].mdetailvalue);
                   
                    //query += "mdetailheadid==" + pddata[i].mdetailheadid + " and mdetailvalue.Contains(\"" + pddata[i].mdetailvalue + "\")";

                }
                query.mdetailhead = mdetailhead;
                query.mdetailvalue = mdetailvalue;
                //mdetailhead.Any(x=>x==)
                //query=mdetailhead+ ".Any(x=>x==mdetailheadid) &&"+ mdetailvalue+ ".Any(x=>x==mdetailvalue)";
            }
            return query;
        }
        public string getallquery(List<ProductSubTwoViewModel> pds, int cnt)
        {
            var query = "";
            var pddata = pds;
            var pdcnt = pddata.Count;
            //pddata.RemoveRange(pds.Count - cnt - 1, cnt);
            if (pddata.Any())
            {
                for (var i = 0; i < pdcnt - cnt; i++)
                {

                    if (query != "")
                    {
                        query += " or ";
                    }
                    query += "mdetailheadid==" + pddata[i].mdetailheadid + " and mdetailvalue.Contains(\"" + pddata[i].mdetailvalue + "\")";

                }
            }
            return query;
        }
        [HttpPost]
        [Route("api/ProductApi/GetOfferSameProduct")]
        public IHttpActionResult GetOfferSameProduct(CartProductViewModel item)
        {

            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = 0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pcv in ctx.productsubs
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where pcv.mprodsubid == item.mprodvarid && pcv.moresupplier == 1 && psupp.SupplierId != item.msupplierid && psupp.mrp > 0
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).ToList<ProductDisplayViewModel>();

            if (products.Count < 0)
            {
                return Ok("empty");
            }
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.mproducturl = "xx=" + x.supcode + "&yy=" + x.distributor + "&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid && k.mprodvariationvalue != null).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            return Ok(products.OrderBy(x => x.mprice).Take(5).ToList());
        }
        [HttpPost]
        [Route("api/ProductApi/GetCompareProduct")]
        public IHttpActionResult GetCompareProduct(CartProductViewModel item)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            var id = 0;
            var seed = string.Empty;
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            products = (from pcv in ctx.productsubs
                        join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId
                        join p in ctx.products on pcv.mprodid equals p.mprodid
                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                        from users in userdata.DefaultIfEmpty()
                        let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                        where pcv.mprodsubid == item.mprodvarid && pcv.moresupplier == 1 && psupp.SupplierId != item.msupplierid && mprice > item.mprice && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        select new ProductDisplayViewModel()
                        {
                            mprodid = p.mprodid,

                            mprodname = p.mprodname,
                            mprodcatname = pc.mcatname,
                            mbrandid = pbrand.mbrandid,
                            mbrand = pbrand.mbrandname,
                            mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                            mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                            supcode = users.morgname.Substring(0, 3),
                            distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                            msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                            //msubpicname = y.Key.pic,
                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                            mprodvarid = pcv.mprodsubid,
                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                            MPN = p.mmanufacturercode,
                            EYS = pcv.muniquecode
                        }).ToList<ProductDisplayViewModel>();
            if (products.Count == 0 || products.Count == 1)
            {
                var category = (from pcv in ctx.productsubs
                                join p in ctx.products on pcv.mprodid equals p.mprodid
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                where pcv.mprodsubid == item.mprodvarid
                                select new
                                {
                                    category = pc.mprodcatid,
                                    brand = pbrand.mbrandid,
                                }).FirstOrDefault();
                var d = (from pcv in ctx.productsubs
                         join p in ctx.products on pcv.mprodid equals p.mprodid
                         join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                         join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                         join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
                         from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                         join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                         from users in userdata.DefaultIfEmpty()

                         let mprice = psupp.mrp.HasValue ? (psupp.mrp.Value * item.mtermf) / (item.mtermp * 0.9) : 0
                         where pcv.mprodsubid != item.mprodvarid && (pcv.msupplierid != item.msupplierid) && p.mprodcatid == category.category && mprice > item.mprice && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                         select new ProductDisplayViewModel()
                         {
                             mprodid = p.mprodid,

                             mprodname = p.mprodname,
                             mprodcatname = pc.mcatname,
                             mbrandid = pbrand.mbrandid,
                             mbrand = pbrand.mbrandname,
                             mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                             mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                             supcode = users.morgname.Substring(0, 3),
                             distributor = psupp.sellername != null && psupp.sellername != "" ? psupp.sellername.Substring(0, 2) : "00",
                             msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                             //msubpicname = y.Key.pic,
                             mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                             mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                             mprodvarid = pcv.mprodsubid,
                             minstallflag = pc.minstallation == 1 ? "yes" : "no",
                             msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                             msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                             msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                             msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                             msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                             msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                             manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                             MPN = p.mmanufacturercode,
                             EYS = pcv.muniquecode
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
                         let mprice = (psub.mrp.Value * item.mtermf) / (item.mtermp * 0.9)
                         where pcv.mprodsubid != item.mprodvarid && (psub.SupplierId != item.msupplierid) && p.mprodcatid == category.category && mprice > item.mprice && ((pc.minstallation == 1 && psub.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                         select new ProductDisplayViewModel()
                         {
                             mprodid = p.mprodid,

                             mprodname = p.mprodname,
                             mprodcatname = pc.mcatname,
                             mbrandid = pbrand.mbrandid,
                             mbrand = pbrand.mbrandname,
                             mtermf = item.mtermf.HasValue ? item.mtermf.Value : 1,
                             mtermp = item.mtermp.HasValue ? item.mtermp.Value : 60,
                             supcode = users.morgname.Substring(0, 3),
                             distributor = psub.sellername != null && psub.sellername != "" ? psub.sellername.Substring(0, 2) : "00",
                             msuppliername = p.mmanufacturer, //p.morgname + (p.mcity != null && p.mcity.Length > 2 ? ", " + p.mcity : "") + (p.mcounty != null && p.mcounty.Length >= 2 ? ", " + p.mcounty : ""),
                             //msubpicname = y.Key.pic,
                             mprice = (pc.minstallation == 1 && psub.minstallation > 0 ? ((psub.mrp.HasValue ? psub.mrp.Value : 0) + (psub.minstallation.HasValue ? psub.minstallation.Value : 0)) : psub.mrp.HasValue ? psub.mrp.Value : 0),
                             mbasicprice = (pc.minstallation == 1 && psub.minstallation > 0 ? ((psub.mrp.HasValue ? psub.mrp.Value : 0) + (psub.minstallation.HasValue ? psub.minstallation.Value : 0)) : psub.mrp.HasValue ? psub.mrp.Value : 0),
                             mprodvarid = pcv.mprodsubid,
                             minstallflag = pc.minstallation == 1 ? "yes" : "no",
                             msupplierid = psub.SupplierId.HasValue ? psub.SupplierId.Value : 0,
                             msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                             msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                             msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                             msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                             msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                             manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                             MPN = p.mmanufacturercode,
                             EYS = pcv.muniquecode
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
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.mproducturl = "xx="+x.supcode+"&yy="+x.distributor+"&productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products.Take(2).ToList());
        }

        [HttpPost]
        [Route("api/ProductApi/oldGetSearchProduct")]
        public IHttpActionResult oldGetSearchProduct(SearchProductViewModel prodsearch)
        {
            var getproduct = HttpUtility.HtmlDecode(prodsearch.getproduct);
            var category = prodsearch.category;
            var posttype = prodsearch.posttype;
            var row = prodsearch.row;
            var rowperpage = prodsearch.rowperpage;
            //var id = int.Parse(Encryption.Decrypt(getid));
            var filters = new List<ProductFilterViewModel>();
            var seed = string.Empty;
            var sdata = getproduct.Split(' ');
            List<ProductDisplayViewModel> products = null;
            var ctx = new LeasingDbEntities();
            IQueryable<product> productdata = null;
            if (category != 0)
            {
                productdata = (from p in ctx.products
                               join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                               where pc.mprodcatid == category && pc.mpreviouscat == category
                               select p).AsQueryable();
            }
            else
            {
                productdata = ctx.products.AsQueryable();
            }

            if (productdata.Any())
            {
                if (posttype == "c")
                {
                    products = (from p in productdata
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                from pcv in pcvdata.DefaultIfEmpty()
                                    //let psupp = (ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").Count()>0)? ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault():null
                                let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                from users in userdata.DefaultIfEmpty()
                                    //sdata.Any(p=>pc.mcatname.Contains(p)
                                    //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                where (pc.mcatname.StartsWith(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp != null && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                    mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mprodvarid = pcv.mprodsubid,
                                    minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                    mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                    msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                    msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                    msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                    msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                    msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                    manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                    MPN = p.mmanufacturercode,
                                    EYS = pcv.muniquecode
                                }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                }
                else if (posttype == "p")
                {
                    products = (from p in productdata
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                from pcv in pcvdata.DefaultIfEmpty()
                                let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                from users in userdata.DefaultIfEmpty()
                                    //sdata.Any(p=>pc.mcatname.Contains(p)
                                    //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                where (p.mprodname.StartsWith(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                    mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mprodvarid = pcv.mprodsubid,
                                    minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                    mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                    msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                    msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                    msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                    msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                    msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                    manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                    MPN = p.mmanufacturercode,
                                    EYS = pcv.muniquecode
                                }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                }
                else if (posttype == "mpn")
                {
                    products = (from p in productdata
                                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                from pcv in pcvdata.DefaultIfEmpty()
                                let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                from users in userdata.DefaultIfEmpty()
                                    //sdata.Any(p=>pc.mcatname.Contains(p)
                                    //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                where (p.mmanufacturercode.StartsWith(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                    mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                    mprodvarid = pcv.mprodsubid,
                                    minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                    mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                    msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                    msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                    msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                    msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                    msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                    manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                    MPN = p.mmanufacturercode,
                                    EYS = pcv.muniquecode
                                }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                }
                else if (posttype == "pt" || posttype == "bpt")
                {
                    var ptype = getproduct;
                    var brand = "";
                    if (posttype == "bpt")
                    {
                        var splitstring = getproduct.Split('$');
                        brand = splitstring[0];
                        ptype = splitstring[1];
                    }
                    var getdetaildata = (from dm in ctx.productdetailmasters
                                         join ps in ctx.productsubtwoes on dm.mprodetailid equals ps.mdetailheadid
                                         where dm.mdetailhead == "Product Type" && ps.mdetailvalue == ptype
                                         select new
                                         {
                                             ps.mprodid
                                         }).AsQueryable();
                    if (getdetaildata.Any())
                    {
                        if (posttype == "pt")
                        {
                            products = (from p in productdata
                                        join pt in getdetaildata on p.mprodid equals pt.mprodid
                                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                        from pcv in pcvdata.DefaultIfEmpty()
                                        let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                        from users in userdata.DefaultIfEmpty()
                                            //sdata.Any(p=>pc.mcatname.Contains(p)
                                            //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                        where p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                            mprodvarid = pcv.mprodsubid,
                                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                            MPN = p.mmanufacturercode,
                                            EYS = pcv.muniquecode
                                        }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                        }
                        else
                        {
                            products = (from p in productdata
                                        join pt in getdetaildata on p.mprodid equals pt.mprodid
                                        join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                        join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                        join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                        from pcv in pcvdata.DefaultIfEmpty()
                                        let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                        join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                        from users in userdata.DefaultIfEmpty()
                                            //sdata.Any(p=>pc.mcatname.Contains(p)
                                            //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                        where pbrand.mbrandname == brand && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                            mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                            mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                            mprodvarid = pcv.mprodsubid,
                                            minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                            msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                            mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                            msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                            msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                            msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                            msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                            msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                            manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                            MPN = p.mmanufacturercode,
                                            EYS = pcv.muniquecode
                                        }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                        }
                    }
                    else
                    {
                        products = (from p in ctx.products
                                    join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                                    join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                                    join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                                    from pcv in pcvdata.DefaultIfEmpty()
                                    let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                                    join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                                    from users in userdata.DefaultIfEmpty()
                                        //sdata.Any(p=>pc.mcatname.Contains(p)
                                        //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                                    where (p.mprodname.Contains(getproduct) || pc.mcatname.Contains(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                        mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                        mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                        mprodvarid = pcv.mprodsubid,
                                        minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                        msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                        mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                        msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                        msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                        msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                        msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                        msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                        manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                        MPN = p.mmanufacturercode,
                                        EYS = pcv.muniquecode
                                    }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();
                    }
                }

            }
            //if (category == 0)
            //{
            //    products = (from p in ctx.products
            //                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
            //                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
            //                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
            //                from pcv in pcvdata.DefaultIfEmpty()
            //                let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
            //                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
            //                from users in userdata.DefaultIfEmpty()
            //                    //sdata.Any(p=>pc.mcatname.Contains(p)
            //                    //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
            //                where (p.mprodname.Contains(getproduct) || pc.mcatname.Contains(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
            //                select new ProductDisplayViewModel()
            //                {
            //                    mprodid = p.mprodid,

            //                    mprodname = p.mprodname,
            //                    mprodcatname = pc.mcatname,
            //                    mbrandid = pbrand.mbrandid,
            //                    mbrand = pbrand.mbrandname,
            //                    mtermf = 1,
            //                    mtermp = 60,
            //                    msuppliername = users.morgname,
            //                    //msubpicname = y.Key.pic,
            //                    mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
            //                    mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
            //                    mprodvarid = pcv.mprodsubid,
            //                    minstallflag = pc.minstallation == 1 ? "yes" : "no",
            //                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
            //                    mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
            //                    msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
            //                    msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
            //                    msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
            //                    msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
            //                    msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
            //                    manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
            //                    MPN = p.mmanufacturercode,
            //                    EYS = pcv.muniquecode
            //                }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();


            //}
            //else
            //{
            //    products = (from p in ctx.products
            //                join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
            //                join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
            //                join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
            //                from pcv in pcvdata.DefaultIfEmpty()
            //                join psupp in ctx.ProductSubSuppliers on pcv.mprodsubid equals psupp.ProdSubId into psuppdata
            //                from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
            //                join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
            //                from users in userdata.DefaultIfEmpty()
            //                where (p.mprodname.Contains(getproduct) || sdata.Any(p => pbrand.mbrandname.Contains(p))) && (pc.mprodcatid == category || pc.mpreviouscat == category) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp != 0 && psupp.approve == 1 && psupp.mactive == "yes" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
            //                select new ProductDisplayViewModel()
            //                {
            //                    mprodid = p.mprodid,

            //                    mprodname = p.mprodname,
            //                    mprodcatname = pc.mcatname,
            //                    mbrandid = pbrand.mbrandid,
            //                    mbrand = pbrand.mbrandname,
            //                    mtermf = 1,
            //                    mtermp = 60,
            //                    msuppliername = users.morgname,
            //                    //msubpicname = y.Key.pic,
            //                    mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
            //                    mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
            //                    mprodvarid = pcv.mprodsubid,
            //                    minstallflag = pc.minstallation == 1 ? "yes" : "no",
            //                    msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
            //                    mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
            //                    msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
            //                    msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
            //                    msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
            //                    msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
            //                    msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
            //                    manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
            //                    MPN = p.mmanufacturercode,
            //                    EYS = pcv.muniquecode
            //                }).OrderBy(x => x.mprodname).Skip(row).Take(rowperpage).ToList<ProductDisplayViewModel>();


            //}
            if (products == null || products.Count == 0)
            {
                products = (from p in ctx.products
                            join pc in ctx.productcats on p.mprodcatid equals pc.mprodcatid
                            join pbrand in ctx.productbrands on p.mbrand equals pbrand.mbrandid
                            join pcv in ctx.productsubs on p.mprodid equals pcv.mprodid into pcvdata
                            from pcv in pcvdata.DefaultIfEmpty()
                            let psupp = ctx.ProductSubSuppliers.Where(x => x.ProdSubId == pcv.mprodsubid && x.approve == 1 && x.mactive == "yes").OrderBy(x => x.mrp).FirstOrDefault()
                            join users in ctx.supplier_registration on psupp.SupplierId equals users.msuid into userdata
                            from users in userdata.DefaultIfEmpty()
                                //sdata.Any(p=>pc.mcatname.Contains(p)
                                //|| sdata.Any(p => pbrand.mbrandname.Contains(p))
                            where (p.mprodname.Contains(getproduct) || pc.mcatname.Contains(getproduct)) && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null) && users.mstatus == "active"
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
                                mprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mbasicprice = (pc.minstallation == 1 && psupp.minstallation > 0 ? ((psupp.mrp.HasValue ? psupp.mrp.Value : 0) + (psupp.minstallation.HasValue ? psupp.minstallation.Value : 0)) : psupp.mrp.HasValue ? psupp.mrp.Value : 0),
                                mprodvarid = pcv.mprodsubid,
                                minstallflag = pc.minstallation == 1 ? "yes" : "no",
                                msupplierid = psupp.SupplierId.HasValue ? psupp.SupplierId.Value : 0,
                                mleadtime = psupp.mleadtime.HasValue ? psupp.mleadtime.Value : 3,
                                msupplierwarranty1 = p.msupplierwarranty.HasValue ? p.msupplierwarranty.Value : 0,
                                msupplierwarranty2 = p.msupplierwarranty2.HasValue ? p.msupplierwarranty2.Value : 0,
                                msupplierwarranty3 = p.msupplierwarranty3.HasValue ? p.msupplierwarranty3.Value : 0,
                                msupplierwarranty4 = p.msupplierwarranty4.HasValue ? p.msupplierwarranty4.Value : 0,
                                msupplierwarranty5 = p.msupplierwarranty5.HasValue ? p.msupplierwarranty5.Value : 0,
                                manufacturewarranty = p.mwarrantyterm.HasValue ? p.mwarrantyterm.Value : 1,
                                MPN = p.mmanufacturercode,
                                EYS = pcv.muniquecode
                            }).OrderBy(x => x.mprodname).ToList<ProductDisplayViewModel>();

            }
            if (products.Count == 0)
            {
                return Ok("empty");
            }
            var cnt = products.Count;
            var minprice = products.OrderBy(x => x.mprice).FirstOrDefault().mprice;
            var maxprice = products.OrderByDescending(x => x.mprice).FirstOrDefault().mprice;
            products = products.Skip(row).Take(rowperpage).Take(rowperpage).ToList();
            products.ForEach(x =>
            {
                var getrating = getStarRating(x.mprodvarid, ctx);
                if (getrating.Any())
                {
                    x.avgrating = getrating[0];
                    x.totalrating = getrating[1];

                }
                x.productspecs = GetSpecs(x.mprodid);
                x.ProductOffer = getNumberOfOffer(x.mprodvarid, x.msupplierid);
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });
            return Json(new { cnt = cnt, datap = products, minprice = minprice, maxprice = maxprice, allFilter = filters });
            //return Ok(products);
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
                             where p.mprodid == prodid && pc.mdetailvalue != null && pc.mdetailvalue != ""
                             select new SingleProductDetailViewModel()
                             {
                                 morder=pd.morder.HasValue?pd.morder.Value:100,
                                 mprodid = prodid,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).OrderBy(x=>x.morder).Take(4).ToList();

            return productdetail;
        }
        public static List<SingleProductDetailViewModel> GetOfferSpecs(int prodid)
        {
            var ctx = new LeasingDbEntities();
            var productdetail = new List<SingleProductDetailViewModel>();
            productdetail = (from p in ctx.products
                             join pc in ctx.productsubtwoes on p.mprodid equals pc.mprodid
                             join pd in ctx.productdetailmasters on pc.mdetailheadid equals pd.mprodetailid
                             join pu in ctx.productunits on pc.mdetailunit equals pu.munitid into pudata
                             from pu in pudata.DefaultIfEmpty()
                             where p.mprodid == prodid && pc.mdetailvalue != null && pc.mdetailvalue != ""
                             select new SingleProductDetailViewModel()
                             {
                                 morder = pd.morder.HasValue ? pd.morder.Value : 100,
                                 mprodid = prodid,
                                 mdetailhead = pd.mdetailhead,
                                 mdetailvalue = pc.mdetailvalue,
                                 muname = pu.munitname
                             }).OrderBy(x => x.morder).Take(15).ToList();

            return productdetail;
        }
        [HttpGet]
        [Route("api/ProductApi/GetProductMenu")]
        public IHttpActionResult GetAllProductMenuById(int getid)
        {
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
                        from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                        where p.mprodcatid == id && p.papprove == 1 && p.mpactive == "yes" && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
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
                            mprice = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mbasicprice = psupp.mrp.HasValue ? psupp.mrp.Value : 0,
                            mprodvarid = pcv.mprodsubid,
                            msupplierid = pcv.msupplierid.HasValue ? pcv.msupplierid.Value : 0,
                        }).ToList<ProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
                x.mprodname = x.mprodname + " " + (ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).Any() ? ctx.prodsubvariationtwoes.Where(k => k.mprodsubid == x.mprodvarid).AsEnumerable().Select(y => y.mprodvariationvalue).Aggregate((c, n) => $"{c},{n}") : "");
                x.msubpicname = ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid) != null ? ctx.productsubpics.FirstOrDefault(z => z.mprodsubid == x.mprodvarid).msubprodpicname : "";
            });

            return Ok(products);
        }

        [HttpGet]
        [Route("api/ProductApi/oGetFeatureProduct")]
        public IHttpActionResult oGetFeatureProduct()
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<FeatureProductDisplayViewModel> products = null;
            products = (from p in db.products
                        join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                        join b in db.productbrands on p.mbrand equals b.mbrandid
                        join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                        from psub in psubdata.DefaultIfEmpty()
                        join psupp in db.ProductSubSuppliers.OrderBy(x => x.mrp) on psub.mprodsubid equals psupp.ProdSubId
                        join users in db.supplier_registration on psupp.SupplierId equals users.msuid
                        join pimg in db.productsubpics on psub.mprodsubid equals pimg.mprodsubid into pimgdata
                        from pimg in pimgdata.DefaultIfEmpty()
                        where p.mpactive == "yes" && p.papprove == 1 && (p.mfeatured == 1 || p.mnewarrival == 1) && psupp.mrp > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        group new
                        {
                            psupp.mleadtime,
                            psub.mprodsubid,
                            psupp.mrp,
                            psupp.mofferprice,
                            pimg.msubprodpicname,
                            psupp.SupplierId,
                            p.msupplierwarranty,
                            p.msupplierwarranty2,
                            p.msupplierwarranty3,
                            p.msupplierwarranty4,
                            p.msupplierwarranty5,
                            p.mwarrantyterm,
                            pc.minstallation,
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
                            p.mpactive,
                            p.mweekdeal
                        } into y
                        orderby y.Key.mprodid
                        select new FeatureProductDisplayViewModel
                        {
                            mprodvarid = y.FirstOrDefault().mprodsubid,
                            mproductcatid = y.Key.mprodcatid,
                            mprodid = y.Key.mprodid,
                            mprodname = y.Key.mprodname,
                            mprodcatname = y.Key.mcatname,
                            msubpicname = y.FirstOrDefault().msubprodpicname,
                            mfeatured = y.Key.mfeatured,
                            mnewarrival = y.Key.mnewarrival,
                            mweekdeal = y.Key.mweekdeal,
                            minstallflag = y.FirstOrDefault().minstallation == 1 ? "yes" : "no",
                            mprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mbasicprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mleadtime = y.FirstOrDefault().mleadtime.HasValue ? y.FirstOrDefault().mleadtime.Value : 3,
                            msupplierid = y.FirstOrDefault().SupplierId.HasValue ? y.FirstOrDefault().SupplierId.Value : 0,
                            msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty.HasValue ? y.FirstOrDefault().msupplierwarranty.Value : 0,
                            msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2.HasValue ? y.FirstOrDefault().msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3.HasValue ? y.FirstOrDefault().msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4.HasValue ? y.FirstOrDefault().msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5.HasValue ? y.FirstOrDefault().msupplierwarranty5.Value : 0,
                            manufacturewarranty = y.FirstOrDefault().mwarrantyterm.HasValue ? y.FirstOrDefault().mwarrantyterm.Value : 1
                        }).ToList<FeatureProductDisplayViewModel>();


            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
            });

            return Ok(products);
        }
        
        [HttpGet]
        [Route("api/ProductApi/oGetWeekDealProduct")]
        public IHttpActionResult oGetWeekDealProduct()
        {
            var db = new LeasingDbEntities();

            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<WeekdealProductDisplayViewModel> products = null;

            products = (from pw in db.productweekdeals
                        join p in db.products on pw.mprodid equals p.mprodid
                        join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                        join b in db.productbrands on p.mbrand equals b.mbrandid
                        join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                        from psub in psubdata.DefaultIfEmpty()
                        join psupp in db.ProductSubSuppliers on psub.mprodsubid equals psupp.ProdSubId into psuppdata
                        from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                        join users in db.supplier_registration on psupp.SupplierId equals users.msuid
                        where p.mpactive == "yes" && p.papprove == 1 && p.mweekdeal == 1 && psupp.mrp > 0 && pw.mfeatured == 1 && pw.mqty > 0 && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        group new
                        {
                            psupp.mleadtime,
                            psub.mprodsubid,
                            psupp.mrp,
                            psupp.mofferprice,
                            psub.msupplierid,
                            pw.mqty,
                            pw.mofferdetail,
                            p.msupplierwarranty,
                            p.msupplierwarranty2,
                            p.msupplierwarranty3,
                            p.msupplierwarranty4,
                            p.msupplierwarranty5,
                            p.mwarrantyterm,
                            pc.minstallation,
                            pc.mprodcatid,
                            pc.mcatname,
                            p.mbrand,
                            p.mprodname,
                            b.mbrandname,
                            p.mfeatured,
                            p.mnewarrival,
                            p.mpactive,
                            p.mweekdeal,

                        } by new
                        {
                            p.mprodid,

                        } into y
                        orderby y.Key.mprodid
                        select new WeekdealProductDisplayViewModel
                        {
                            mprodvarid = y.FirstOrDefault().mprodsubid,
                            mproductcatid = y.FirstOrDefault().mprodcatid,
                            mprodid = y.Key.mprodid,
                            mprodname = y.FirstOrDefault().mprodname,
                            mprodcatname = y.FirstOrDefault().mcatname,
                            mfeatured = y.FirstOrDefault().mfeatured,
                            mnewarrival = y.FirstOrDefault().mnewarrival,
                            mweekdeal = y.FirstOrDefault().mweekdeal,
                            mprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mbasicprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mrp.Value) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mrp.HasValue ? y.FirstOrDefault().mrp.Value : 0),
                            mleadtime = y.FirstOrDefault().mleadtime.HasValue ? y.FirstOrDefault().mleadtime.Value : 3,
                            msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                            mqty = y.FirstOrDefault().mqty.HasValue ? y.FirstOrDefault().mqty.Value : 0,
                            mofferdetail = y.FirstOrDefault().mofferdetail,
                            minstallflag = y.FirstOrDefault().minstallation == 1 ? "yes" : "no",
                            msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty.HasValue ? y.FirstOrDefault().msupplierwarranty.Value : 0,
                            msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2.HasValue ? y.FirstOrDefault().msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3.HasValue ? y.FirstOrDefault().msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4.HasValue ? y.FirstOrDefault().msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5.HasValue ? y.FirstOrDefault().msupplierwarranty5.Value : 0,
                            manufacturewarranty = y.FirstOrDefault().mwarrantyterm.HasValue ? y.FirstOrDefault().mwarrantyterm.Value : 1
                        }).ToList<WeekdealProductDisplayViewModel>();


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
                x.mdiscount = 100 - ((x.mprice / x.mbasicprice) * 100);
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
            });

            return Ok(products);
        }


        [HttpGet]
        [Route("api/ProductApi/oGetOnSaleProduct")]
        public IHttpActionResult oGetOnSaleProduct()
        {
            var db = new LeasingDbEntities();
            //var id = int.Parse(Encryption.Decrypt(getid));

            var seed = string.Empty;
            List<FeatureProductDisplayViewModel> products = null;
            products = (from p in db.products
                        join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                        //join b in db.productbrands on p.mbrand equals b.mbrandid
                        join psub in db.productsubs on p.mprodid equals psub.mprodid into psubdata
                        from psub in psubdata.DefaultIfEmpty()
                        join psupp in db.ProductSubSuppliers on psub.mprodsubid equals psupp.ProdSubId into psuppdata
                        from psupp in psuppdata.OrderBy(x => x.mrp).DefaultIfEmpty()
                        join users in db.supplier_registration on psupp.SupplierId equals users.msuid
                        join pimg in db.productsubpics on psub.mprodsubid equals pimg.mprodsubid into pimgdata
                        from pimg in pimgdata.DefaultIfEmpty()
                        where p.mpactive == "yes" && p.papprove == 1 && (psupp.mofferprice < psupp.mrp && psupp.mofferprice > 0) && psupp.approve == 1 && psupp.mactive == "yes" && users.mstatus == "active" && ((pc.minstallation == 1 && psupp.minstallation > 0) || pc.minstallation == 0 || pc.minstallation == null)
                        group new
                        {
                            psupp.mleadtime,
                            psub.mprodsubid,
                            pimg.msubprodpicname,
                            psupp.mofferprice,
                            psub.msupplierid,
                            p.msupplierwarranty,
                            p.msupplierwarranty2,
                            p.msupplierwarranty3,
                            p.msupplierwarranty4,
                            p.msupplierwarranty5,
                            p.mwarrantyterm,
                            pc.minstallation,

                        } by new
                        {
                            p.mprodid,
                            pc.mprodcatid,
                            pc.mcatname,
                            p.mbrand,
                            p.mprodname,
                            //b.mbrandname,
                            p.mfeatured,
                            p.mnewarrival,
                            p.mpactive,


                        } into y
                        orderby y.Key.mprodid
                        select new FeatureProductDisplayViewModel
                        {
                            mprodvarid = y.FirstOrDefault().mprodsubid,
                            mproductcatid = y.Key.mprodcatid,
                            mprodid = y.Key.mprodid,
                            mprodname = y.Key.mprodname,
                            mprodcatname = y.Key.mcatname,
                            msubpicname = y.FirstOrDefault().msubprodpicname,
                            mfeatured = y.Key.mfeatured,
                            mnewarrival = y.Key.mnewarrival,
                            mprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : 0) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : 0),
                            mbasicprice = (y.FirstOrDefault().minstallation == 1 && y.FirstOrDefault().minstallation > 0 ? ((y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : 0) + (y.FirstOrDefault().minstallation.HasValue ? y.FirstOrDefault().minstallation.Value : 0)) : y.FirstOrDefault().mofferprice.HasValue ? y.FirstOrDefault().mofferprice.Value : 0),
                            mleadtime = y.FirstOrDefault().mleadtime.HasValue ? y.FirstOrDefault().mleadtime.Value : 0,
                            minstallflag = y.FirstOrDefault().minstallation == 1 ? "yes" : "no",
                            msupplierid = y.FirstOrDefault().msupplierid.HasValue ? y.FirstOrDefault().msupplierid.Value : 0,
                            msupplierwarranty1 = y.FirstOrDefault().msupplierwarranty.HasValue ? y.FirstOrDefault().msupplierwarranty.Value : 0,
                            msupplierwarranty2 = y.FirstOrDefault().msupplierwarranty2.HasValue ? y.FirstOrDefault().msupplierwarranty2.Value : 0,
                            msupplierwarranty3 = y.FirstOrDefault().msupplierwarranty3.HasValue ? y.FirstOrDefault().msupplierwarranty3.Value : 0,
                            msupplierwarranty4 = y.FirstOrDefault().msupplierwarranty4.HasValue ? y.FirstOrDefault().msupplierwarranty4.Value : 0,
                            msupplierwarranty5 = y.FirstOrDefault().msupplierwarranty5.HasValue ? y.FirstOrDefault().msupplierwarranty5.Value : 0,
                            manufacturewarranty = y.FirstOrDefault().mwarrantyterm.HasValue ? y.FirstOrDefault().mwarrantyterm.Value : 1
                        }).ToList<FeatureProductDisplayViewModel>();



            if (products.Count == 0)
            {
                return NotFound();
            }
            products.ForEach(x =>
            {
                x.mprodcaturl = "Category=" + x.mprodcatname + "&getproducts=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mproductcatid.ToString()));
                x.mproducturl = "productname=" + HttpUtility.UrlEncode(x.mprodname) + "&getproduct=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.mprodvarid.ToString())) + "&getsup=" + HttpUtility.UrlEncode(Encryption.Encrypt(x.msupplierid.ToString()));
            });

            return Ok(products);
        }



        public List<string> getStarRating(int prodid, LeasingDbEntities db)
        {
            var getdata = db.productreviews.Where(x => x.mprodid == prodid);
            List<string> ratedata = new List<string>();
            if (getdata.Any())
            {
                var ratestar = getdata.Average(x => x.mrankno);
                var cssper = (ratestar * 100) / 5;
                ratedata.Add(cssper.ToString());
                ratedata.Add(getdata.Count().ToString());
            }
            return ratedata;
        }
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
                    foreach (var d in datas.Where(x => x.Selected == true).ToList())
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
                foreach (var d in datas.Where(x => x.Selected == true).ToList())
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
            if (datas != null)
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
            var prodlist = new List<PassAgreementDataDisplayViewModel>();
            if (datas.Any())
            {

                foreach (var d in datas)
                {
                    var products = (from us in db.usersubquotations
                                    join pcv in db.productsubs on us.mprodvarid equals pcv.mprodsubid
                                    join p in db.products on pcv.mprodid equals p.mprodid
                                    join pc in db.productcats on p.mprodcatid equals pc.mprodcatid
                                    join pbrand in db.productbrands on p.mbrand equals pbrand.mbrandid
                                    where us.msubquoteid == d.msubquoteid
                                    select new AgreementDataDisplayViewModel()
                                    {
                                        Make = pbrand.mbrandname,
                                        Condition = pcv.mcondition,

                                    }).FirstOrDefault();
                    var p2 = new PassAgreementDataDisplayViewModel
                    {
                        msubquoteid = d.msubquoteid,
                        AgreementData = products,
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
                        var orderref = 101;
                        var ordproducts = datas.Product.Where(x => x.Selected == true).ToList();
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
                                muserid = datas.muserid,
                                total = Math.Round(datas.mquotetotal, 2),
                                mleaseno = orderref,
                                msorderref = datas.msorderref,
                                ffee = Math.Round(ffee, 2),
                                sfee = Math.Round(sfee, 2),
                                msend = 1,
                                mpayment = datas.mpayment,
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
            List<SmallAgreementAddressViewModel> addresses = new List<SmallAgreementAddressViewModel>();
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
                                mprintby = datas.muserid.ToString(),
                                mprintdate = DateTime.Now,
                                muserid = datas.mschoolid,
                                total = Math.Round(datas.mquotetotal, 2),
                                mleaseno = orderref,
                                msorderref = datas.msorderref,
                                ffee = Math.Round(ffee, 2),
                                sfee = Math.Round(sfee, 2),
                                msend = 2,
                                mpayment = datas.mpayment,
                                mcontactid = datas.mcontactid,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                                mdefaultadd = datas.mdefaultadd,
                                mest = DateTime.Now.AddDays(7)
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            mleaseno.Add(orderref);
                            int? oldsuppid = 0;
                            int ordrno = 0;
                            int i = 1;
                            var ord = ordproducts.OrderBy(x => x.msupplierid).OrderBy(x => x.Address).ToList();
                            foreach (var d in ord)
                            {
                                if (oldsuppid != d.msupplierid)
                                {
                                    i = 1;
                                    var obj4 = new supplierpurchaseorder
                                    {
                                        msupplierid = d.msupplierid,
                                        magreementno = obj2.mleaseno,
                                        mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                        createddate = DateTime.Now,
                                    };

                                    db.supplierpurchaseorders.Add(obj4);
                                    db.SaveChanges();
                                    ordrno = obj4.mspoid;
                                    addresses.Add(new SmallAgreementAddressViewModel
                                    {
                                        suppid = d.msupplierid,
                                        address = d.Address
                                    });
                                    oldsuppid = d.msupplierid;
                                    i = 2;
                                }
                                else
                                {
                                    if (addresses.Where(x => x.address == d.Address && x.suppid == d.msupplierid).FirstOrDefault() == null)
                                    {
                                        var obj4 = new supplierpurchaseorder
                                        {
                                            msupplierid = d.msupplierid,
                                            magreementno = obj2.mleaseno,
                                            mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                            createddate = DateTime.Now,
                                        };
                                        db.supplierpurchaseorders.Add(obj4);
                                        db.SaveChanges();
                                        ordrno = obj4.mspoid;
                                        addresses.Add(new SmallAgreementAddressViewModel
                                        {
                                            suppid = d.msupplierid,
                                            address = d.Address
                                        });
                                        i++;
                                    }
                                }
                                byte ovisible = 0;
                                var ordervisible = db.supplier_registration.FirstOrDefault(x => x.msuid == d.msupplierid);
                                if (ordervisible != null)
                                {
                                    ovisible = (byte)(ordervisible.order_visible.HasValue ? ordervisible.order_visible.Value : 0);
                                }

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
                                    mofferdetail = d.mofferdetail,
                                    mexpdeliverydate = DateTime.Now.AddDays(7),
                                    order_visible = ovisible,
                                    minstallationflag = (byte)(d.minstallationflag == "yes" ? 1 : 0),
                                };
                                db.usersubquotations.Add(obj3);
                                db.SaveChanges();
                                if (d.warrantydetails != null)
                                {
                                    var w = new usersubquotationwarranty
                                    {
                                        mprodname = d.warrantydetails.mprodname,
                                        msubquoteid = obj3.msubquoteid,
                                        mprice = d.warrantydetails.mprice,
                                        mtermf = d.warrantydetails.mtermf,
                                        mtermp = d.warrantydetails.mtermp,
                                        msubwarrantid = d.warrantydetails.subwarrantid,
                                    };
                                    db.usersubquotationwarranties.Add(w);
                                    db.SaveChanges();
                                }
                                if (d.Address.ToString() != null && d.Address.ToString() != "")
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
        [Route("api/ProductApi/SaveAllOrderFromCart")]
        public IHttpActionResult SaveAllOrderFromCart(List<SubSingleQuotationViewModel> multipledatas)
        {
            var db = new LeasingDbEntities();
            var prodlist = "";
            string content = "";
            float ffee = 0;
            float sfee = 0;
            var muserid = 0;
            List<int> products = new List<int>();
            List<SmallAgreementAddressViewModel> addresses = new List<SmallAgreementAddressViewModel>();
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
                    var orderref = 101;
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
                                msend = 2,
                                mpayment = datas.mpayment,
                                mcontactid = datas.mcontactid,
                                mdefaultadd=datas.mdefaultadd,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                                mest = DateTime.Now.AddDays(7)
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            if (datas.mpayment == 2)
                            {
                                var bankupdates = db.leaseuserbanks.FirstOrDefault(x => x.muserid == datas.mschoolid);
                                if (bankupdates != null)
                                {
                                    var leasebanks = new agreementbank
                                    {
                                        maccountholder = bankupdates.maccountholder,
                                        agreementid = orderref,
                                        mbankaccount = bankupdates.mbankaccount,
                                        mbankname = bankupdates.mbankname,
                                        mbankshort = bankupdates.mbankshort,
                                        
                                    };
                                    db.agreementbanks.Add(leasebanks);
                                    db.SaveChanges();
                                }
                            }
                            
                            mleaseno.Add(orderref);
                            int? oldsuppid = 0;
                            int ordrno = 0;
                            int i = 1;
                            var ord = ordproducts.OrderBy(x => x.msupplierid).OrderBy(x => x.Address).ToList();
                            foreach (var d in ord)
                            {
                                if (oldsuppid != d.msupplierid)
                                {
                                    i = 1;
                                    var obj4 = new supplierpurchaseorder
                                    {
                                        msupplierid = d.msupplierid,
                                        magreementno = obj2.mleaseno,
                                        mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                        createddate = DateTime.Now,
                                    };

                                    db.supplierpurchaseorders.Add(obj4);
                                    db.SaveChanges();
                                    ordrno = obj4.mspoid;
                                    addresses.Add(new SmallAgreementAddressViewModel
                                    {
                                        suppid = d.msupplierid,
                                        address = d.Address
                                    });
                                    oldsuppid = d.msupplierid;
                                    i = 2;
                                }
                                else
                                {
                                    if (addresses.Where(x => x.address == d.Address && x.suppid == d.msupplierid).FirstOrDefault() == null)
                                    {
                                        var obj4 = new supplierpurchaseorder
                                        {
                                            msupplierid = d.msupplierid,
                                            magreementno = obj2.mleaseno,
                                            mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                            createddate = DateTime.Now,
                                        };
                                        db.supplierpurchaseorders.Add(obj4);
                                        db.SaveChanges();
                                        ordrno = obj4.mspoid;
                                        addresses.Add(new SmallAgreementAddressViewModel
                                        {
                                            suppid = d.msupplierid,
                                            address = d.Address
                                        });
                                        i++;
                                    }
                                }
                                byte ovisible = 0;
                                var ordervisible = db.supplier_registration.FirstOrDefault(x => x.msuid == d.msupplierid);
                                if (ordervisible != null)
                                {
                                    ovisible = (byte)(ordervisible.order_visible.HasValue ? ordervisible.order_visible.Value : 0);
                                }
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
                                    msendtosupplier = 1,
                                    morderno = ordrno,
                                    order_visible = ovisible,
                                    minstallationflag = (byte)(d.minstallationflag == "yes" ? 1 : 0),
                                };
                                db.usersubquotations.Add(obj3);
                                db.SaveChanges();
                                if (d.warrantydetails != null)
                                {
                                    var w = new usersubquotationwarranty
                                    {
                                        mprodname = d.warrantydetails.mprodname,
                                        msubquoteid = obj3.msubquoteid,
                                        mprice = d.warrantydetails.mprice,
                                        mtermf = d.warrantydetails.mtermf,
                                        mtermp = d.warrantydetails.mtermp,
                                        msubwarrantid = d.warrantydetails.subwarrantid,
                                    };
                                    db.usersubquotationwarranties.Add(w);
                                    db.SaveChanges();
                                }
                                if (d.Address.ToString() != null && d.Address.ToString() != "")
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
                            products.Add(orderref);
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
                return Ok(products);

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

            List<SmallAgreementAddressViewModel> addresses = new List<SmallAgreementAddressViewModel>();
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
            if (multipledatas != null)
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
                                msend = 2,
                                mpayment = datas.mpayment,
                                mcontactid = datas.mcontactid,
                                createddate = DateTime.Now,
                                createdby = datas.muserid,
                                mest = DateTime.Now.AddDays(7)
                            };
                            db.userleaseagreements.Add(obj2);
                            db.SaveChanges();
                            mleaseno.Add(orderref);
                            int? oldsuppid = 0;
                            int ordrno = 0;
                            int i = 1;
                            var ord = ordproducts.OrderBy(x => x.msupplierid).OrderBy(x => x.Address).ToList();
                            foreach (var d in ord)
                            {
                                if (oldsuppid != d.msupplierid)
                                {
                                    i = 1;
                                    var obj4 = new supplierpurchaseorder
                                    {
                                        msupplierid = d.msupplierid,
                                        magreementno = obj2.mleaseno,
                                        mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                        createddate = DateTime.Now,
                                    };

                                    db.supplierpurchaseorders.Add(obj4);
                                    db.SaveChanges();
                                    ordrno = obj4.mspoid;
                                    addresses.Add(new SmallAgreementAddressViewModel
                                    {
                                        suppid = d.msupplierid,
                                        address = d.Address
                                    });
                                    oldsuppid = d.msupplierid;
                                    i = 2;
                                }
                                else
                                {
                                    if (addresses.Where(x => x.address == d.Address && x.suppid == d.msupplierid).FirstOrDefault() == null)
                                    {
                                        var obj4 = new supplierpurchaseorder
                                        {
                                            msupplierid = d.msupplierid,
                                            magreementno = obj2.mleaseno,
                                            mponumber = "EYS" + obj2.mleaseno + d.msupplierid + d.Address + "0" + i,
                                            createddate = DateTime.Now,
                                        };
                                        db.supplierpurchaseorders.Add(obj4);
                                        db.SaveChanges();
                                        ordrno = obj4.mspoid;
                                        addresses.Add(new SmallAgreementAddressViewModel
                                        {
                                            suppid = d.msupplierid,
                                            address = d.Address
                                        });
                                        i++;
                                    }
                                }
                                byte ovisible = 0;
                                var ordervisible = db.supplier_registration.FirstOrDefault(x => x.msuid == d.msupplierid);
                                if (ordervisible != null)
                                {
                                    ovisible = (byte)(ordervisible.order_visible.HasValue ? ordervisible.order_visible.Value : 0);
                                }
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
                                    msendtosupplier = 1,
                                    morderno = ordrno,
                                    order_visible = ovisible,
                                    minstallationflag = (byte)(d.minstallationflag == "yes" ? 1 : 0),
                                };
                                db.usersubquotations.Add(obj3);
                                db.SaveChanges();
                                if (d.warrantydetails != null)
                                {
                                    var w = new usersubquotationwarranty
                                    {
                                        mprodname = d.warrantydetails.mprodname,
                                        msubquoteid = obj3.msubquoteid,
                                        mprice = d.warrantydetails.mprice,
                                        mtermf = d.warrantydetails.mtermf,
                                        mtermp = d.warrantydetails.mtermp,
                                        msubwarrantid = d.warrantydetails.subwarrantid,
                                    };
                                    db.usersubquotationwarranties.Add(w);
                                    db.SaveChanges();
                                }
                                if (d.Address.ToString() != null && d.Address.ToString() != "")
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
                                     mreviewid = pr.mreviewid,
                                     mrankno = pr.mrankno,
                                     mreviewtitle = pr.mreviewtitle,
                                     mcomments = pr.mcomments,

                                 }).FirstOrDefault();

                if (productrating != null)
                {
                    List<productreviewpic> productreview = null;
                    productreview = db.productreviewpics.Where(x => x.mreviewid == productrating.mreviewid).ToList();
                    return Ok(new { productrating = productrating, reviewimage = productreview });
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
