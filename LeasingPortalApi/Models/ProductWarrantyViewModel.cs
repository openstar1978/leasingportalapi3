using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class ProductWarrantyViewModel
    {
        public int WarrantyId { get; set; }
        public string WarrantyTitle { get; set; }
        public string WarrantyDesc { get; set; }
        public Nullable<int> SupplierId { get; set; }
        public Nullable<int> ProdId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<int> ParentId { get; set; }
        public Nullable<int> BrandId { get; set; }
        public Nullable<byte> Type { get; set; }
        public string DesignedFor { get; set; }
        public string ServiceSupport { get; set; }
        public string WarrantyCode { get; set; }
        public string ServiceAvailability { get; set; }
    }
    public partial class ProductSubWarrantyViewModel
    {
        public int subwarrantid { get; set; }
        public Nullable<int> warrantyid { get; set; }
        public string WarrantyTitle { get; set;}
        public Nullable<int> warrantyterm { get; set; }
        public Nullable<double> warrantyprice { get; set; }
        public string WType { get; set; }
    }
}