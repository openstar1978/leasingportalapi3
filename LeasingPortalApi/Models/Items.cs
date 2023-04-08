using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class Items
    {
        public ProductDisplayViewModel Product
        {
            get;
            set;
        }

        public int Quantity
        {
            get;
            set;
        }
    }
}