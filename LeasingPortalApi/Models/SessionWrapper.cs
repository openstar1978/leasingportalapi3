using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class SessionWrapper
    {
        /*public static List<Items> Cart
        {
            get
            {
                if (HttpContext.Current.Session["Cart"] != null)
                {
                    if (!((List<Items>)HttpContext.Current.Session["Cart"]).Any())
                    {
                        return ((List<Items>)HttpContext.Current.Session["Cart"]);
                    }
                }
                return null;
            }
            set
            {

                HttpContext.Current.Session["Cart"] = value;
            }
        }
        */
        public static List<Items> Cart
        {
            get
            {
                if (HttpContext.Current.Session["Cart"] != null)
                {
                    if (!((List<Items>)HttpContext.Current.Session["Cart"]).Any())
                    {
                        return ((List<Items>)HttpContext.Current.Session["Cart"]);
                    }
                }
                return null;
            }
            set
            {

                HttpContext.Current.Session["Cart"] = value;
            }
        }
        public static List<int> agnos
        {
            get
            {
                if (HttpContext.Current.Session["agnos"] != null)
                {
                    return ((List<int>)HttpContext.Current.Session["agnos"]);
                    
                }
                return null;
            }
            set
            {
                //HttpContext.Current.Session["agnos"] = new List<int>();
                HttpContext.Current.Session.Add("agnos",value);
            }
        }
        public static string CustomerName
        {
            get
            {
                if (HttpContext.Current.Session["CustomerName"] != null)
                {
                    return (HttpContext.Current.Session["CustomerName"].ToString());
                }
                return null;
            }
            set
            {

                HttpContext.Current.Session["CustomerName"] = value;
            }
        }
        public static string CustomerType
        {
            get
            {
                if (HttpContext.Current.Session["CustomerType"] != null)
                {
                    return (HttpContext.Current.Session["CustomerType"].ToString());
                }
                return null;
            }
            set
            {

                HttpContext.Current.Session["CustomerType"] = value;
            }
        }
        public static string CustomerEmail
        {
            get
            {
                if (HttpContext.Current.Session["CustomerEmail"] != null)
                {
                    return (HttpContext.Current.Session["CustomerEmail"].ToString());
                }
                return null;
            }
            set
            {

                HttpContext.Current.Session["CustomerEmail"] = value;
            }
        }
        public static int CustomerId
        {
            get
            {
                if (HttpContext.Current.Session["CustomerId"] != null)
                {
                    return (int.Parse(HttpContext.Current.Session["CustomerName"].ToString()));
                }
                return 0;
            }
            set
            {

                HttpContext.Current.Session["CustomerId"] = value;
            }
        }
        public static int Testemail
        {
            get
            {
                if (HttpContext.Current.Session["Testemail"] != null)
                {
                    return (int.Parse(HttpContext.Current.Session["Testemail"].ToString()));
                }
                return 0;
            }
            set
            {

                HttpContext.Current.Session["Testemail"] = value;
            }
        }
    }
}