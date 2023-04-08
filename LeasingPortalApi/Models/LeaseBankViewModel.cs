using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class LeaseBankViewModel
    {
        public int Id { get; set; }
        public Nullable<int> muserid { get; set; }
        public string mbankname { get; set; }
        public string mbankaccount { get; set; }
        public string maccountholder { get; set; }
        public string mbankshort { get; set; }
    }
}