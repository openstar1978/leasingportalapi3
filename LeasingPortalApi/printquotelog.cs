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
    
    public partial class printquotelog
    {
        public int Id { get; set; }
        public Nullable<int> prod_id { get; set; }
        public Nullable<int> supplier_id { get; set; }
        public Nullable<int> user_id { get; set; }
        public Nullable<byte> valid { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<double> mvat { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> quantity { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string quotenumber { get; set; }
    }
}
