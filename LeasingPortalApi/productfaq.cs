//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LeasingPortalApi
{
    using System;
    using System.Collections.Generic;
    
    public partial class productfaq
    {
        public int mfaqid { get; set; }
        public Nullable<int> mprodid { get; set; }
        public string mquestion { get; set; }
        public string manswer { get; set; }
        public Nullable<System.DateTime> mpostdate { get; set; }
        public Nullable<byte> mactive { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
    }
}
