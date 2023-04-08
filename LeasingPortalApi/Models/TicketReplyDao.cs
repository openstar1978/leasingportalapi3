using LeasingPortalApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class TicketReplyDao
    {
        public static bool Save(List<TicketReplyViewModel> model, string details)
        {
            try
            {
                if (model != null)
                {
                    foreach (var a in model)
                    {
                        var ticketreplydata = new TicketReply
                        {
                            TicketId = a.TicketId,
                            Detail = details,
                            UserId = a.UserId,
                            CreatedDate = DateTime.UtcNow
                        };

                        using (var db = new LeasingDbEntities())
                        {
                            db.TicketReplies.Add(ticketreplydata);
                            db.SaveChanges();
                        }
                    }
                    return true;
                }
                return false;
            }         
            catch(Exception ex)
            {
                return false;                
            }
        }
    }
}