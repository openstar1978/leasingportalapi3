using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class CountryStateCityViewModel
    {
        public partial class CountryViewModel
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string CountryCode { get; set; }
        }
        public partial class StateViewModel
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Nullable<int> CountryID { get; set; }
        }
        public partial class CityViewModel
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public Nullable<int> StateID { get; set; }
        }
        public partial class SelectModelForCountrStateCity
        {
            public int Code { get; set; }
            public  string Name { get; set; }
        }
    }
}