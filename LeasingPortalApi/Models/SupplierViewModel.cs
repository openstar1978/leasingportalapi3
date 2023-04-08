using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class SupplierViewModel
    {
        public int msuid { get; set; }
        public string com_logo { get; set; }
        public string museridemail { get; set; }
        public string mpwd { get; set; }
        public string mname { get; set; }
        public string mlname { get; set; }
        public string mphone { get; set; }
        public string morgname { get; set; }
        public string morgregno { get; set; }
        public string mvatno { get; set; }
        public string mpostcode { get; set; }
        public string maddress1 { get; set; }
        public string maddress2 { get; set; }
        public string mtown { get; set; }
        public string mcity { get; set; }
        public string mcountry { get; set; }
        public string mstatus { get; set; }
        public Nullable<System.DateTime> createddate { get; set; }
        public Nullable<int> createdby { get; set; }
        public string musertype { get; set; }
        public Nullable<byte> use_lease { get; set; }
        public string website { get; set; }
        public string source_enquiry { get; set; }
        public string trading_as { get; set; }
        public Nullable<System.DateTime> date_established { get; set; }
        public string target_places { get; set; }
        public Nullable<byte> com_type { get; set; }
        public string owner { get; set; }
        public string com_email { get; set; }
    }
    public class suppliermoreviewmodel
    {
        public int supplier_more_id { get; set; }
        public Nullable<int> supplierid { get; set; }
        public string monthly_sales { get; set; }
        public string monthly_lease { get; set; }
        public Nullable<double> education { get; set; }
        public Nullable<double> commercial { get; set; }
        public Nullable<double> publicsector { get; set; }
    }
    public class supplierequipmentviewmodel
    {
        public int supplier_equipment_id { get; set; }
        public Nullable<int> supplierid { get; set; }
        public string equipment_sold { get; set; }
        public string equipment_brand { get; set; }
        public Nullable<double> warrantly_length { get; set; }
        public string warrranty_provider { get; set; }
    }
    public class suppliercontactsViewmodel
    {
        public int supplier_contact_id { get; set; }
        public Nullable<int> supplierid { get; set; }
        public string susername { get; set; }
        public string sposition { get; set; }
        public string semailaddress { get; set; }
        public string sphone { get; set; }
    }
}