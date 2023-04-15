using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class ProductViewModel
    {
        public int mprodid { get; set; }
        public string mprodname { get; set; }
        public Nullable<int> mprodregid { get; set; }
        public string mprodregvalue { get; set; }
        public string mactive { get; set; }
        public Nullable<byte> mfeatured { get; set; }
        public Nullable<int> mprodcatid { get; set; }
        public string msearchtags { get; set; }
        public string mbrand { get; set; }
        public string mmanufacturer { get; set; }
        public string mwarrantyterm { get; set; }
        public string mmanufacturerwarrantdesc { get; set; }
        public Nullable<System.DateTime> mmanufacturerdate { get; set; }
        public string mreplacementterm { get; set; }
        public Nullable<int> mreturndays { get; set; }
        public string mreviewed { get; set; }
        public string mquestion { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public string mtaxtype { get; set; }
        public Nullable<double> matxperct { get; set; }
        public Nullable<double> mpriceincltax { get; set; }
        public string mdescription { get; set; }
        public string mprocurementtype { get; set; }
        public string mdraft { get; set; }
        public Nullable<int> mdisplayportal { get; set; }
        public string mpopular { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
    }

    public class ProductSubViewModel
    {
        public int mprodsubid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public string sellerskuid { get; set; }
        public string mvariation1 { get; set; }
        public string mvariation2 { get; set; }
        public Nullable<double> mvariationprice { get; set; }
        public string mdescription { get; set; }
        public Nullable<double> mlengthcm { get; set; }
        public Nullable<double> mdepthcm { get; set; }
        public Nullable<double> mwidthcm { get; set; }
        public Nullable<double> mheightcm { get; set; }
        public Nullable<double> mweightcm { get; set; }
        public Nullable<double> mrp { get; set; }
        public Nullable<double> mofferprice { get; set; }
        public Nullable<int> mleasetermid { get; set; }
        public Nullable<double> mleasetermprice { get; set; }
        public Nullable<double> mstock { get; set; }

        public List<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
    }
    public class ProductSubTwoViewModel
    {
        public int mprodusubtwoid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mdetailheadid { get; set; }
        public string mdetailvalue { get; set; }
        public Nullable<int> mdetailunit { get; set; }
    }
    public class ProductSubVariationTwoViewModel
    {
        public int mprodsubvariationtwoid { get; set; }
        public Nullable<int> mprodsubid { get; set; }
        public Nullable<int> mprodvariationid { get; set; }
        public string mprodvariation { get; set; }
        public string mprodvariationvalue { get; set; }
        public IEnumerable<string> mprodvariationvalue2 { get; set; }
    }
    public class ProductSubVariationDisplayViewModel
    {

        public Nullable<int> mprodsubid { get; set; }
        public string mprodvariation { get; set; }
        public string mproducturl { get; set; }
        public string mprodname { get; set; }
        public string optionname { get; set; }
        public Nullable<double> mrp { get; set; }
        public int msupplierid { get; set; }
        public string mprodvariationvalue { get; set; }
        public IEnumerable<string> mprodvariationvalue2 { get; set; }

    }
    public class variationsubclass
    {
        public Nullable<int> mprodsubid { get; set; }
        public string mvariationvalue { get; set; }
        public string mproducturl { get; set; }
        public string mprodname { get; set; }

    }
    public class ProductSubPicViewModel
    {
        public int mpicid { get; set; }
        public Nullable<int> mprodsubid { get; set; }
        public string msubprodpicname { get; set; }

    }
    public class ProductCategoryDisplayViewModel
    {
        public int mprodid { get; set; }
        public int mcatid { get; set; }
        public string mproducturl { get; set; }
        public string mcatpic { get; set; }
        public string mcatname { get; set; }

    }
    public class ProductDisplayViewModel
    {
        public int mprodid { get; set; }
        public int mtermf { get; set; }
        public int mtermp { get; set; }
        public int mprodvarid { get; set; }
        public string mprodname { get; set; }
        public string mprodcatname { get; set; }
        public int mbrandid { get; set; }
        public string mbrand { get; set; }
        public string mbrandlogo { get; set; }
        public string msubpicname { get; set; }
        public double mprice { get; set; }
        public double dprice { get; set; }
        public double mbasicprice { get; set; }
        public string mproducturl { get; set; }
        public int msupplierid { get; set; }
        public string msuppliername { get; set; }
        public int ProductOffer { get; set; }
        public int mleadtime { get; set; }
        public Nullable<int> manufacturewarranty { get; set; }
        public Nullable<int> msupplierwarranty1 { get; set; }
        public Nullable<int> msupplierwarranty2 { get; set; }
        public Nullable<int> msupplierwarranty3 { get; set; }
        public Nullable<int> msupplierwarranty4 { get; set; }
        public Nullable<int> msupplierwarranty5 { get; set; }
        public IQueryable<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
        public List<SingleProductDetailViewModel> productspecs { get; set; }
        public string minstallflag { get; set; }
        public string avgrating { get; set; }
        public string totalrating { get; set; }
        public string MPN { get; set; }
        public string EYS { get; set; }
        public double msstock { get; set; } 
        public string supcode { get; set; }
        public string distributor { get; set; }
    }
    public class FeatureProductDisplayViewModel
    {
        public Nullable<int> mleadtime { get; set; }
        public int mprodid { get; set; }
        public int mtermf { get; set; }
        public int mtermp { get; set; }
        public int mprodvarid { get; set; }
        public string mprodname { get; set; }
        public string mprodcatname { get; set; }
        public int mbrandid { get; set; }
        public string mbrand { get; set; }
        public string mbrandlogo { get; set; }
        public string msubpicname { get; set; }
        public double mprice { get; set; }
        public double mbasicprice { get; set; }
        public double mdiscount { get; set; }
        public string mproducturl { get; set; }
        public string mprodcaturl { get; set; }
        public int mproductcatid { get; set; }
        public int msupplierid { get; set; }
        public string msuppliername { get; set; }
        public int ProductOffer { get; set; }
        public Nullable<byte> variableproduct { get; set; }
        public Nullable<byte> mfeatured { get; set; }
        public Nullable<byte> mnewarrival { get; set; }
        public Nullable<byte> mweekdeal { get; set; }
        public Nullable<int> manufacturewarranty { get; set; }
        public Nullable<int> msupplierwarranty1 { get; set; }
        public Nullable<int> msupplierwarranty2 { get; set; }
        public Nullable<int> msupplierwarranty3 { get; set; }
        public Nullable<int> msupplierwarranty4 { get; set; }
        public Nullable<int> msupplierwarranty5 { get; set; }
        public string minstallflag { get; set; }
        public string avgrating { get; set; }
        public string totalrating { get; set; }
        public string MPN { get; set; }
        public string EYS { get; set; }
        public string supcode { get; set; }
        public string distributor { get; set; }
        public IQueryable<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
        public List<SingleProductDetailViewModel> productspecs { get; set; }
    }
    public class WeekdealProductDisplayViewModel
    {
        public Nullable<int> mleadtime { get; set; }
        public int mprodid { get; set; }
        public int mtermf { get; set; }
        public int mtermp { get; set; }
        public int mprodvarid { get; set; }
        public string mprodname { get; set; }
        public string mprodcatname { get; set; }
        public int mbrandid { get; set; }
        public string mbrand { get; set; }
        public string mbrandlogo { get; set; }
        public string msubpicname { get; set; }
        public double mprice { get; set; }
        public double mbasicprice { get; set; }
        public double mdiscount { get; set; }
        public string mproducturl { get; set; }
        public string mprodcaturl { get; set; }
        public int mproductcatid { get; set; }
        public int msupplierid { get; set; }
        public string msuppliername { get; set; }
        public Nullable<byte> variableproduct { get; set; }
        public Nullable<byte> mfeatured { get; set; }
        public Nullable<byte> mnewarrival { get; set; }
        public Nullable<byte> mweekdeal { get; set; }
        public Nullable<int> mqty { get; set; }
        public string mofferdetail { get; set; }
        public Nullable<int> manufacturewarranty { get; set; }
        public Nullable<int> msupplierwarranty1 { get; set; }
        public Nullable<int> msupplierwarranty2 { get; set; }
        public Nullable<int> msupplierwarranty3 { get; set; }
        public Nullable<int> msupplierwarranty4 { get; set; }
        public Nullable<int> msupplierwarranty5 { get; set; }
        public string minstallflag { get; set; }
        public string avgrating { get; set; }
        public string totalrating { get; set; }
        public string MPN { get; set; }
        public string EYS { get; set; }
        public IQueryable<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
        public List<SingleProductDetailViewModel> productspecs { get; set; }
    }
    public class SingleProductDisplayViewModel
    {
        public int ProductOffer { get; set; }
        public int mprodid { get; set; }
        public int mtermf { get; set; }
        public int mtermp { get; set; }
        public int mprodvarid { get; set; }
        public int mprodcatid { get; set; }
        public string mprodname { get; set; }
        public string mprodcatname { get; set; }
        public int mbrandid { get; set; }
        public string mbrand { get; set; }
        public string mbrandlogo { get; set; }
        public string msubpicname { get; set; }
        public double mprice { get; set; }
        public double mbasicprice { get; set; }
        public string mproducturl { get; set; }
        public string mproductcaturl { get; set; }
        public string sellerskuid { get; set; }
        public double mstock { get; set; }
        public double mlengthcm { get; set; }
        public double mdepthcm { get; set; }
        public double mwidthcm { get; set; }
        public double mheightcm { get; set; }
        public double mweightcm { get; set; }
        public string mdescription { get; set; }
        public double mvariationprice { get; set; }
        public int msupplierid { get; set; }
        public string msuppliername { get; set; }
        public byte moresupplier { get; set; }
        public double mrp { get; set; }
        public double mmrpbasicprice { get; set; }
        public int mproductcatid { get; set; }
        public int mofferqty { get; set; }
        public string mofferdetail { get; set; }
        public double mofferprice { get; set; }
        public string minstallflag { get; set; }
        public Nullable<int> mleadtime { get; set; }
        public Nullable<int> manufacturewarranty { get; set; }
        public Nullable<int> msupplierwarranty1 { get; set; }
        public Nullable<int> msupplierwarranty2 { get; set; }
        public Nullable<int> msupplierwarranty3 { get; set; }
        public Nullable<int> msupplierwarranty4 { get; set; }
        public Nullable<int> msupplierwarranty5 { get; set; }
        public IQueryable<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
        public string mpn { get; set; }
        public string supcode { get; set; }
        public string distributor { get; set; }
    }
    public class SingleProductDetailViewModel
    {
        public int mprodid { get; set; }
        public int mproddetailid { get; set; }
        public string mdetailhead { get; set; }
        public string mdetailvalue { get; set; }
        public string muname { get; set; }
        public int morder { get; set; }
    }
    public class ProductSearchViewModel
    {
        public int id { get; set; }
        public string mprodname { get; set; }
        public string bname { get; set; }
        public string namewithflag { get; set; }
    }
    public class ShopByCategoryViewModel
    {
        public string mcatimg { get; set; }
        public int mcatid { get; set; }
        public string mcatname { get; set; }
        public string mcaturl { get; set; }
    }
    public class ShopByAreaViewModel
    {
        public int mareaid { get; set; }
        public string mareaname { get; set; }
        public string massigncategory { get; set; }
        public string mphoto { get; set; }
        public string mcaturl { get; set; }
    }
    public class ProdCatViewModel
    {
        public int mcatid { get; set; }
        public string mcatname { get; set; }
        public int mpreviouscat { get; set; }
        public List<ProdCatViewModel> Children { get; set; }
    }
    public class SearchProductViewModel
    {
        public string getproduct { get; set; }
        public int category { get; set; }
        public string posttype { get; set; }
        public int row { get; set; }
        public int rowperpage { get; set; }
        public bool filter { get; set; }
        public List<ProductFilterDetailViewModel> filterdata { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public int sortby { get; set; }
    }
    public class ProductFilterViewModel
    {
        public int Id { get; set; }
        public string FilterHead { get; set; }
        public List<ProductFilterDetailViewModel> FilterData { get; set; }
    }
    public class ProductFilterDetailViewModel
    {
        public int FilterId { get; set; }
        public string FilterValue { get; set; }
        public string FilterUrl { get; set; }
        public string FilterUnit { get; set; }
        public string FilterName { get; set; }
        public bool range { get; set; }
        public string rangevalues { get; set; }
        public bool display { get; set; }
    }
    public class ProductParameterViewModel
    {
        public int getid { get; set; }
        public int row { get; set; }
        public int rowperpage { get; set; }
        public bool filter { get; set; }
        public List<ProductFilterDetailViewModel> filterdata { get; set; }
        public double min { get; set; }
        public double max { get; set; }
        public int sortby { get; set; }
    }
    public class QueryListViewModel
    {
        public List<int> mdetailhead { get; set; }
        public List<string> mdetailvalue { get; set; }
    }
}