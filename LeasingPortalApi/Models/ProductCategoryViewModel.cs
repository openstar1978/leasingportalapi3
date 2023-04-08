using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class ProductCategoryViewModel
    {
       
            public int mprodcatid { get; set; }
            public string mcatname { get; set; }
            public string mcatimage { get; set; }
            public int mpreviouscat { get; set; }
            public string mprodcaturl { get; set; }
            public string mcaturl { get; set; }
    }
}