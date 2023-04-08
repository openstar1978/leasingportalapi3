using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class ProductRatingViewModel
    {
        public int mreviewid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mrankno { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<System.DateTime> mreviewdate { get; set; }
        public string mcomments { get; set; }
        public string mreviewtitle { get; set; }
        public Nullable<byte> mactive { get; set; }
        public List<string> files { get; set; }
        public string emprodid { get; set; }
    }
    public class ProductRatingViewModelWithImage
    {
        public int mreviewid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mrankno { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<System.DateTime> mreviewdate { get; set; }
        public string mdreviewdate { get; set; }
        public string mcomments { get; set; }
        public string musername { get; set; }

        public string mreviewtitle { get; set; }
        public Nullable<byte> mactive { get; set; }
        public List<ProductReviewImage> mimages { get; set; }
    }
    public class ProductReviewImage
    {
        public string mpicname { get; set; }

    }
    public partial class ProductSupplierRatingViewModelWithImage

    {
        public int mid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public string mpicname { get; set; }
        public Nullable<System.DateTime> mcreateddate { get; set; }
    }
}