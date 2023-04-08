using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class UserQuotationMasterViewModeel
    {
        public int mquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public string mquoteref { get; set; }
        public Nullable<double> mquotetotal { get; set; }
        public string mquotestatus { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public string ccreatedate { get; set; }
        public string encmquoteid { get; set; }
    }
    public class UserQuotationViewModel
    {
        public int msubquoteid { get; set; }
        public int mquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<double> mvat { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mquotestatus { get; set; }
        public string mproductname { get; set; }
        public string subpicname { get; set; }
        public string type { get; set; }
        public string mnotes { get; set; }
        public Nullable<int> morderno { get; set; }
        public string msorderno { get; set; }
        public Nullable<int> magreementno { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public string ccreatedate { get; set; }
        public string suppliername { get; set; }
        public string mproducturl { get; set; }
        public string status { get; set; }
        public string minstallationflag { get; set; }
        public WarrantyCartViewModel warrantydetails { get; set; }
    }
    public class CartOrderViewModel
    {
        
        public int msubquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mquotestatus { get; set; }
        public string mproductname { get; set; }
        public string subpicname { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public string ccreatedate { get; set; }
        
        public bool Selected { get; set; }
        public string suppliername { get; set; }
        public string Address { get; set; }
        public string AddressDetail { get; set; }
        public Nullable<byte> maddressflag { get; set; }
        public Nullable<byte> mflexible { get; set; }
        public double mvat { get; set; }
        public string mproducturl { get; set; }
        public int mleadtime { get; set; }
        public Nullable<byte> noncancellable { get; set; }
        public int mofferqty { get; set; }
        public string mofferdetail { get; set; }
        public double mofferprice { get; set; }
        public bool mofferapplied { get; set; }
        public Nullable<byte> moffer { get; set; }
        public Nullable<byte> mwarrantyflag { get; set; }
        public WarrantyCartViewModel warrantydetails { get; set; }
        public string minstallationflag { get; set; }
    }
    public class UserQuotationNewViewModel
    {
        public int msubquoteid { get; set; }
        public int mquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<double> mvat { get; set; }
        public Nullable<int> quantity { get; set; }
    
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mquotestatus { get; set; }
        public string mproductname { get; set; }
        public string subpicname { get; set; }
        public string type { get; set; }
        public string mnotes { get; set; }
        public string mstatus { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public string ccreatedate { get; set; }
        public string encmquoteid { get; set; }
        public bool Selected { get; set; }
        public string suppliername { get; set; }
        public string Make { get; set; }
        public string Condition { get; set; }
        public string Model { get; set; }
        public int Address { get; set; }
        public string AddressDetail { get; set; }
        public string DefaultAddress { get; set; }
        public Nullable<byte> maddressflag { get; set; }
        public Nullable<byte> mflexible { get; set; }
        public string termdata { get; set; }
        public string mproducturl { get; set; }
        public int mleadtime { get; set;}
        public Nullable<byte> noncancellable { get; set; }
        public string mofferdetail { get; set; }
        public WarrantyCartViewModel warrantydetails { get; set; }
        public string minstallationflag { get; set; }
    }
    public class SubSingleQuotationViewModel
    {
        public List<UserQuotationNewViewModel> Product { get; set; }
        public UsersAllDetailViewModel UserData { get; set; }
        public List<AgreementProductData> AgreementProductData { get; set; }
        public int muserid { get; set; }
        public int mschoolid { get; set; }
        public string mnotes { get; set; }
        public string msorderref { get; set; }
        public float mquotetotal{get;set;}
        public byte mpayment { get; set; }
        public Nullable<int> mcontactid { get; set; }
        public Nullable<int> mdefaultadd { get; set; }
        
    }
    public class AgreementProductData
    {
        public string Itemlist { get; set; }
    }
    public class QuotationDetailViewModel
    {
        public UserQuotationViewModel Product { get; set; }
        public int Quantity { get; set; }
    }
    public class QuotationMoreDetailViewModel
    {
        public List<QuotationDetailViewModel> Product { get; set; }
        public int muserid { get; set; }
        public  Nullable<double> mquotetotal { get; set; }
        public int mquoteid { get; set; }
        public string SessionId { get; set; }
    }
    public class SingleQuotationViewModel
    {
        public IQueryable<ProductSubVariationTwoViewModel> productsubvariationtwo { get; set; }
        public List<UserQuotationViewModel> Product { get; set; }
        public int muserid { get; set; }
        public Nullable<double> mquotetotal { get; set; }
        public int mquoteid { get; set; }
        public string mquoteref { get; set; }
        public Nullable<DateTime> mquotedate { get; set; }
        public string mstatus { get; set; }
        public string mnotes { get; set; }
        public string encmquoteid { get; set; }
        public List<int> SelectedQuote { get; set; }
    }
    public class SupplierWiseQuotationViewModel
    {
        public List<UserQuotationViewModel> Product { get; set; }
       
        public Nullable<double> mquotetotal { get; set; }
        public int mquoteid { get; set; }
        public Nullable<int> msuid { get; set; }
        public string suppliername { get; set; }

    }
    public class RelatedProductViewModel
    {
        public List<UserQuotationViewModel> Product { get; set; }
    }
    public class DisplayOrderViewModel
    {
        public List<UserQuotationViewModel> Product { get; set; }
        public Nullable<DateTime> ddate { get; set; }
        public string createdate { get; set; }
        public Nullable<int> morderref { get; set; }
        public string msorderref { get; set; }
        public string agreement { get; set; }
        public int mleaseid { get; set; }
        public Nullable<double> mquotetotal { get; set; }
        
    }
    public class UserDisplayQuotationViewModel
    {
        public string itemcode { get; set; }
        public int msubquoteid { get; set; }
        public int mquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mquotestatus { get; set; }
        public string mproductname { get; set; }
        public string subpicname { get; set; }
        public string type { get; set; }
        public string period { get; set; }
        public string suppliername { get; set; }
        public string suppliernote { get; set; }
        public string supplierstatus { get; set; }
        public Nullable<DateTime> supplierdate { get; set; }
        public string morderdate { get; set; }
        public string Make { get; set; }
        public string Condition { get; set; }
        public string DifferentAddress { get; set; }
        public double? mvat { get; set; }
    }
    public partial class printquotelogviewmodel
    {
        public int Id { get; set; }
        public Nullable<int> prod_id { get; set; }
        public Nullable<int> supplier_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<byte> valid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public Nullable<double> mvat { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
    }
}