using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class TicketDao
    {
        public static bool Add(TicketViewModel model)
        {
            var ticketdata = new TicketDetail
            {
                FullName = model.FullName,
                ReplyEmail = model.ReplyEmail,
                Mobile = model.Mobile,
                Department = model.Department,
                Subject = model.Subject,
                Detail = model.Detail,
                Attachment = model.Attachment,
                Priority = model.Priority,
                Status = 1,
                CreatedDate = DateTime.UtcNow,
                LastModify = DateTime.UtcNow,
                Type = model.Type,
                TypeId = model.TypeId,
                UserId = model.UserId,
                ModuleType = "General"
            };

            using (var db = new LeasingDbEntities())
            {
                db.TicketDetails.Add(ticketdata);

                db.SaveChanges();

                return true;
            }

        }
        public static bool UpdateAction(List<TicketViewModel> model)
        {
            try
            {
                using (var db = new LeasingDbEntities())
                {
                    if (model != null)
                    {
                        foreach (var a in model)
                        {
                            var search = db.TicketDetails.FirstOrDefault(x => x.Id == a.Id);
                            if (search != null)
                            {
                                search.Status = a.Status;
                            }
                            db.SaveChanges();
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool DeleteAction(List<TicketViewModel> model)
        {
            try
            {
                using (var db = new LeasingDbEntities())
                {
                    if (model != null)
                    {
                        foreach (var a in model)
                        {
                            var search = db.TicketDetails.FirstOrDefault(x => x.Id == a.Id);
                            if (search != null)
                            {
                                string filePath = System.Web.HttpContext.Current.Server.MapPath("~/Uploads/TicketAttachment" + search.Attachment);
                                if (System.IO.File.Exists(filePath))
                                {
                                    System.IO.File.Delete(filePath);
                                }
                                var searchprod = db.TicketReplies.Where(x => x.TicketId == a.Id);
                                if (searchprod != null)
                                {
                                    foreach (var b in searchprod)
                                    {
                                        db.Entry(b).State = System.Data.Entity.EntityState.Deleted;

                                    }
                                    db.SaveChanges();
                                }
                                db.Entry(search).State = System.Data.Entity.EntityState.Deleted;
                                db.SaveChanges();
                            }
                        }
                        return true;
                    }
                }
                return false;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public static TicketViewModel GetById(int id)
        {
            using (var db = new LeasingDbEntities())
            {
                var search = db.TicketDetails.FirstOrDefault(x => x.Id == id);
                if(search != null)
                {
                    var GetReply = GetTicketReply(search.Id);
                    
                    return new TicketViewModel()
                    {
                        Id = search.Id,
                        FullName = search.FullName,
                        ReplyEmail = search.ReplyEmail,
                        Mobile = search.Mobile,
                        Department = search.Department,
                        Subject = search.Subject,
                        Detail = search.Detail,
                        VarCreatedDate = search.CreatedDate.HasValue ? search.CreatedDate.Value.ToString("dd/MM/yyyy hh:mm tt") : "",
                        Attachment = search.Attachment,
                        Priority = search.Priority,    
                        GetReply = GetReply,
                    };
                }
                else
                {
                    return null;
                }
            }
        }

        public static List<TicketViewModel> getdata(int id)
        {
            var list = new List<TicketViewModel>();
            using (var db = new LeasingDbEntities())
            {
                foreach (var search in db.TicketDetails.Where(x => x.Id == id).OrderBy(x => x.Id))
                {
                    list.Add(new TicketViewModel
                    {
                        Id = search.Id,
                        FullName = search.FullName,
                        ReplyEmail = search.ReplyEmail,
                        Mobile = search.Mobile,
                        Department = search.Department,
                        Subject = search.Subject,
                        Detail = search.Detail,
                        CreatedDate = search.CreatedDate,
                        Attachment = search.Attachment,
                        UserId = search.UserId,
                    });
                }
                return list;
            }
        }
        public static List<TicketReplyViewModel> GetTicketReply(int Id)
        {
            var list = new List<TicketReplyViewModel>();
            var data = new List<TicketReplyViewModel>();
            using (var db = new LeasingDbEntities())
            {
                foreach (var a in db.TicketReplies.Where(x => x.TicketId == Id))
                {
                    var check_username = db.user_registration.FirstOrDefault(x=>x.muid == a.UserId);
                    list.Add(new TicketReplyViewModel
                    {
                        Id = a.Id,
                        UserId = a.UserId,
                        Detail = a.Detail,
                        UserName = check_username != null ? check_username.mname + " " + check_username.msname : "",
                        VarCreatedDate = a.CreatedDate.HasValue ? a.CreatedDate.Value.ToString("dd/MM/yyyy hh:mm tt") : "Data Not Available"
                    });
                }
                if(list != null)
                {
                    data = list.OrderByDescending(x => x.Id).ToList();
                }
            }
            return data;
        }
        public static List<TicketViewModel> Gets()
        {
            var list = new List<TicketViewModel>();
            using (var db = new LeasingDbEntities())
            {
                foreach (var a in db.TicketDetails.Where(x => x.Status == 1))
                {

                    list.Add(new TicketViewModel
                    {
                        Id = a.Id,
                        FullName = a.FullName,
                        ReplyEmail = a.ReplyEmail,
                        Mobile = a.Mobile,
                        Department = a.Department,
                        Subject = a.Subject,
                        ModuleType = a.ModuleType,
                        TypeId = a.TypeId,
                        Detail = a.Detail,
                        Priority = a.Priority,
                        Type = a.Type
                    });
                }
            }
            return list;

        }
    }


}