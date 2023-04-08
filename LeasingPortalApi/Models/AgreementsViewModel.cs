using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class AgreementsViewModel
    {
    }
    public class GetAgreementDataViewModel
    {
        public int muserid { get; set; }
        public int msubquoteid { get; set; }
    }
    public class AgreementDataDisplayViewModel
    {
        public string Make { get; set; }
        public string Condition { get; set; }
        public string Model { get; set; }
    }
    public class PassAgreementDataDisplayViewModel
    {
        public int msubquoteid { get; set; }
        public AgreementDataDisplayViewModel AgreementData { get; set; }
    }
    public class LeaseAggreementViewModel
    {
        public string menvelopid { get; set; }
        public int mleaseid { get; set; }
        public string mlessorno { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mleaseno { get; set; }
        public Nullable<int> morderref { get; set; }
        public string msorderref { get; set; }
        public Nullable<double> ffee { get; set; }
        public Nullable<double> sfee { get; set; }
        public Nullable<double> total { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
        public List<UserQuotationViewModel> Product { get; set; }
        public string subquoteids { get; set; }
        public string displaydate { get; set; }
        public string morgname { get; set; }
        public Nullable<byte> msend { get; set; }
        public string mprintby { get; set; }
        public Nullable<DateTime> mprintdate { get; set; }
        public Nullable<int> mpayment { get; set; }
        public string mdocname { get; set; }
        public Nullable<DateTime> mest { get; set; }
        public string malloc { get; set; }
        public Nullable<int> defaultaddress { get; set; }
        public string maddress { get; set; }
    }

    public class LeaseAggreementNewViewModel
    {
        public Nullable<DateTime> mest { get; set; }
        public string msorderref { get; set; }
        public Nullable<byte> mpayment { get; set; }

        public int mleaseid { get; set; }
        public string mlessorno { get; set; }
        public Nullable<int> muserid { get; set; }
        public Nullable<int> mleaseno { get; set; }
        public Nullable<byte> msend { get; set; }
        public Nullable<double> total { get; set; }
        public Nullable<DateTime> mprintdate { get; set; }
        public Nullable<int> morderno { get; set; }
        public Nullable<int> magreementno { get; set; }
        public string type { get; set; }
        public int msubquoteid { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<double> mvat { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public string mnotes { get; set; }

        public Nullable<System.DateTime> createddate { get; set; }
        public string mname { get; set; }
        public string mprodname { get; set; }
        public string mstatus { get; set; }
        public string morgname { get; set; }
        public string mtown { get; set; }
        public string mcity { get; set; }
    }
    public class SmallLeaseAggreementViewModel
    {
        public int muserid { get; set;  }
        public List<int> mleasenos { get; set; }
    }
    public class UserLeaseDocumentViewModel
    {
        public int Id { get; set; }
        public Nullable<int> mleaseno { get; set; }
        public string mlessorno { get; set; }
        public string mdocname { get; set; }
        public int muserid { get; set; }
    }
    public class AgreementAddressViewModel
    {
        public int id { get; set; }
        public string address { get; set; }
    }
    public class SmallAgreementAddressViewModel
    {
        public int? suppid { get; set; }
        public int? address { get; set; }
    }
}