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
    
    public partial class SaveCart
    {
        public int Id { get; set; }
        public string Session_Id { get; set; }
        public Nullable<int> mprodid { get; set; }
        public Nullable<int> mprodvarid { get; set; }
        public string msubpicname { get; set; }
        public Nullable<int> msupplierid { get; set; }
        public string mproducturl { get; set; }
        public Nullable<double> mprice { get; set; }
        public Nullable<int> mtermf { get; set; }
        public Nullable<int> mtermp { get; set; }
        public string mprodname { get; set; }
        public Nullable<int> mqty { get; set; }
        public Nullable<byte> maddressflag { get; set; }
        public Nullable<byte> moffer { get; set; }
        public Nullable<byte> mwarrantyflag { get; set; }
        public Nullable<int> mwarrantid { get; set; }
        public Nullable<byte> minstallationflag { get; set; }
        public Nullable<System.DateTime> mcreatedtime { get; set; }
    }
}
