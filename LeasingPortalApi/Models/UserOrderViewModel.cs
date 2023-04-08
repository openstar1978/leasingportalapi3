using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class UserOrderViewModel
    {

        public int morderid { get; set; }
        public Nullable<int> muserid { get; set; }
        public string morderref { get; set; }
        public Nullable<double> mordertotal { get; set; }
        public string morderstatus { get; set; }
        public string mnotes { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }

    }
    public partial class UsersuborderViewModel
    {
        public int msuborderid { get; set; }
        public Nullable<int> morderid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mstatus { get; set; }
    }
    public class UserOrdersViewModel
    {
        public int msubquoteid { get; set; }
        public int morderid { get; set; }
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
        public string mnotes { get; set; }
        public Nullable<int> orderref { get; set; }

        public Nullable<System.DateTime> createddate { get; set; }
        public string ccreatedate { get; set; }
        public string suppliername { get; set; }
    }
    public partial class SmallOrderViewModer
    {
        public int orderid { get; set; }
        public Nullable<int> orderref { get; set; }
        public string monotes { get; set; }
        public string ccreatedate { get; set; }
    }
    public partial class DeliveryStatusViewModel
    {
        public int Id { get; set; }
        public Nullable<int> morderid { get; set; }
        public Nullable<int> msubquoteid { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<System.DateTime> mreceivedate { get; set; }
        public string msubquotestatus { get; set; }
        public Nullable<byte> mcondition { get; set; }
        public Nullable<System.DateTime> createdate { get; set; }
    }
    public partial class exportproductviewmodel
    {
        public Nullable<int> mleaseno { get; set; }
        public int subquoteids { get; set; }
        public Nullable<double> ffee { get; set; }
        public Nullable<double> sfee { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mcontactid { get; set; }
        public string msorderref { get; set; }
        public Nullable<DateTime> msorderdate { get; set; }
        public int mpayment { get; set; }
        public int mdefaultadd { get; set; }
    }
    
}