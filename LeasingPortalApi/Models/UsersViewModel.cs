using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class UsersViewModel
    {
        public int muid { get; set; }
        public int muserid { get; set; }
        public string museridemail { get; set; }
        public string mstatus { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
        public string musertype { get; set; }
        public string mpwd { get; set; }
        public string mname { get; set; }
        public string mphone { get; set; }
        public string morgname { get; set; }
        public Nullable<byte> mverify { get; set; }
        public string Captcha_Response { get; set; }
    }
    public class UsersAddressViewModel
    {
        public int museraddrid { get; set; }
        public Nullable<int> muid { get; set; }
        public string mpostcode { get; set; }
        public string maddress1 { get; set; }
        public string maddress2 { get; set; }
        public string mcity { get; set; }
        public string mcountry { get; set; }
        public string mlandmark { get; set; }
        public string maddresstype { get; set; }
    }
    public class UsersMoreDetailViewModel
    {
        public int musermoreid { get; set; }
        public Nullable<int> muid { get; set; }
        public string mpwd { get; set; }
        public string mname { get; set; }
        public string mphone { get; set; }
        public string morgname { get; set; }
        public string morgregno { get; set; }
        public string mvatno { get; set; }
        public string mleasestatus { get; set; }
        public Nullable<System.DateTime> mregdate { get; set; }
    }
    public class schoolmasterviewmodel
    {
        public int mschoolid { get; set; }
        public string mschoolname { get; set; }
        public Nullable<int> createdby { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
    }
    public class UserContactsMoreViewModel
    {
        public int Id { get; set; }
        public Nullable<int> usermoreid { get; set; }
        public string cusername { get; set; }
        public string csurname { get; set; }
        public string cposition { get; set; }
        public string cemailaddress { get; set; }
        public string cphone { get; set; }
        public Nullable<byte> cdefault { get; set; }
    }
    public class UsersAllDetailViewModel
    {
        public int muid { get; set; }
        public int musermoreid { get; set; }
        public string museridemail { get; set; }
        public string mstatus { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
        public string musertype { get; set; }
        public string mpwd { get; set; }
        public string mname { get; set; }
        public string msname { get; set; }
        public string mposition { get; set; }
        public string mphone { get; set; }
        public string morgname { get; set; }
        public Nullable<int> mauid { get; set; }
        public string mpostcode { get; set; }
        public string maddress1 { get; set; }
        public string maddress2 { get; set; }
        public string mcity { get; set; }
        public int? mcountry { get; set; }
        public string mlandmark { get; set; }
        public string maddresstype { get; set; }
        public string morgregno { get; set; }
        public string mvatno { get; set; }
        public string mleasestatus { get; set; }
        public int mschoolid { get; set; }
        public Nullable<System.DateTime> mregdate { get; set; }
        public string manotherusername { get; set; }
        public string manotheruseremail { get; set; }

        public string maccountantemail { get; set; }
        public Nullable<int> subscribe { get; set; }
        public List<UserContactsMoreViewModel> morecontacts { get; set; }
        public UsersAddressViewModel moreaddress { get; set; }
    }
    public class ProductRequestViewModel
    {
        public string morgname { get; set; }
        public string museridemail { get; set; }
        public int lookingfor { get; set; }
                public bool interested { get; set; }
                public string specificproduct { get; set; }
                public int muserid { get; set; }
        public string searchproduct { get; set; }
        public int category { get; set; }
    }
}