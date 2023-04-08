using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class TicketReplyViewModel
    {
        public int Id { get; set; }
        public Nullable<int> TicketId { get; set; }
        public string Detail { get; set; }
        public string UserName { get; set; }
        public string VarCreatedDate { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
    }
}