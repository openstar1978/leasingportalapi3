using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class SaveCartViewModel
    {
            public int Id { get; set; }
            public string Session_Id { get; set; }
            public Nullable<int> mprodid { get; set; }
            public Nullable<int> mprodvarid { get; set; }
            public string msubpicname { get; set; }
            public Nullable<int> msupplierid { get; set; }
            public string mproducturl { get; set; }
        public string mprodname { get; set; }
        public Nullable<double> mprice { get; set; }
            public Nullable<int> mtermf { get; set; }
            public Nullable<int> mtermp { get; set; }
            public Nullable<int> mqty { get; set; }
        public Nullable<byte> moffer { get; set; }
        public string page { get; set; }
        public Nullable<int> pindex { get; set; }
        public string offer { get; set; }
        public bool warrantyexist { get; set; }
        public int subwarrantid { get; set; }
        public WarrantyCartViewModel wcart { get; set; }
        public Nullable<byte> minstallationflag { get; set; }
    }
    public class WarrantyCartViewModel
    {
        public int id { get; set; }
        public int subwarrantid { get; set; }
        public string mprodname { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> mtermp { get; set; }
        public Nullable<int> mtermf { get; set; }
        public string Session_Id { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<int> mqty { get; set; }
        public Nullable<double> mbaseprice { get; set; }
    }
    public class SaveCartNewViewModel
    {
        public int Id { get; set; }
        public CartProductViewModel Product { get; set; }
        public string Session_Id { get; set; }
        public Nullable<int> Quantity { get; set; }
        public double mvat { get; set; }
    }
    public class CartProductViewModel
    {
        public Nullable<int> mbrandid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public string msubpicname { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public string mproducturl { get; set; }
        public string mprodname { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string suppliername { get; set; }
        public Nullable<int> mleadtime { get; set; }
        public Nullable<double> mbaseprice { get; set; }
        public int mofferqty { get; set; }
        public string mofferdetail { get; set; }
        public double mofferprice { get; set; }
        public Nullable<byte> moffer { get; set; }
        public int ProductOffer { get; set; }
        public WarrantyCartViewModel warrantydetails { get; set; }
        public string minstallationflag { get; set; }
        public bool maxwarrantyflag { get; set; }
    }

    public class SaveSubCartViewModel
    {
        public int savecartid { get; set; }
        public List<ProductDisplayViewModel> product { get; set; }
    }
    public class SubCartDisplayViewModel
    {
        public int Id { get; set; }
        public string Session_Id { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public string msubpicname { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public string mproducturl { get; set; }
        public string mprodname { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public Nullable<int> mqty { get; set; }
        public string suppliername { get; set; }
        public Nullable<double> mbasicprice { get; set; }
    }
}