using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
      
        public string ReplyEmail { get; set; }
        public string Mobile { get; set; }
        public string Department { get; set; }
        public string Subject { get; set; }
        public string Detail { get; set; }
        public string Priority { get; set; }
        public string Attachment { get; set; }
        public string Captcha_Response { get; set; }
        public string VarCreatedDate { get; set; }
        public List<TicketReplyViewModel> GetReply { get; set; }
        public Nullable<byte> Status { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> LastModify { get; set; }
        public HttpPostedFileBase Resume
        {
            get;
            set;
        }
        public string Type { get; set; }
        public int? TypeId { get; set; }
        public Nullable<int> UserId { get; set; }
        public string ModuleType { get; set; }
    }
}