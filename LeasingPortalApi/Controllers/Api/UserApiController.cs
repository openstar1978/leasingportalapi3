using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;

namespace LeasingPortalApi.Controllers.Api
{
    public class UserApiController : ApiController
    {
        [HttpGet]
        [Route("api/UserApi/getUserDatas")]
        public IHttpActionResult getUserDatas(int getid)
        {
            var db = new LeasingDbEntities();
            var datafounds = (from u in db.user_registration
                              join ur in db.user_registration_addr on u.musermoreid equals ur.muid
                              join um in db.user_registration_more on u.musermoreid equals um.musermoreid
                              where u.muid == getid
                              select new UsersAllDetailViewModel
                              {
                                  museridemail = u.museridemail,
                                  maddress1 = ur.maddress1,
                                  maddress2 = ur.maddress2,
                                  maddresstype = ur.maddresstype,
                                  mcity = ur.mcity,
                                  mcountry = ur.mcountry,
                                  mlandmark = ur.mlandmark,
                                  mpostcode = ur.mpostcode,
                                  mname = u.mname,
                                  msname=u.msname,
                                  mposition=u.mposition,
                                  morgname = um.morgname,
                                  mphone = um.mphone
                              }).FirstOrDefault();
            if (datafounds == null)
            {
                return Ok("empty");
            }

            return Ok(datafounds);
        }
        [HttpGet]
        [Route("api/UserApi/getUserAllDatas")]
        public IHttpActionResult getUserAllDatas(int getid)
        {
            var db = new LeasingDbEntities();
            var datafounds = (from um in db.user_registration_more
                              join ur in db.user_registration_addr on um.musermoreid equals ur.muid
                              join u in db.user_registration on um.musermoreid equals u.musermoreid
                              where um.musermoreid == getid
                              select new UsersAllDetailViewModel
                              {
                                  museridemail = u.museridemail,
                                  maddress1 = ur.maddress1,
                                  maddress2 = ur.maddress2,
                                  maddresstype = ur.maddresstype,
                                  mcity = ur.mcity,
                                  mcountry = ur.mcountry,
                                  mlandmark = ur.mlandmark,
                                  mpostcode = ur.mpostcode,
                                  mname = u.mname,
                                  msname = u.msname,
                                  mposition = u.mposition,
                                  morgname = um.morgname,
                                  mphone = um.mphone,
                                  musermoreid=um.musermoreid,
                              }).FirstOrDefault();
            if (datafounds == null)
            {
                return Ok("empty");
            }

            return Ok(datafounds);
        }
        [HttpGet]
        [Route("api/UserApi/GetUserAddress")]
        public IHttpActionResult GetUserAddress(int getid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            var maddress = db.user_registration_addr.FirstOrDefault(x => x.muid == getid && x.maddresstype=="Default");
            if (maddress != null)
            {
                addr = maddress.maddress1;
                if (maddress.maddress2 != null)
                {
                    addr = addr + "<br/>" + maddress.maddress2;
                }
                if (maddress.mlandmark != null)
                {
                    addr = addr + "<br/>" + maddress.mlandmark;

                }
                addr = addr + "<br/>" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                var data = new AgreementAddressViewModel
                {
                    id = maddress.museraddrid,
                    address = addr,
                };
                return Ok(data);
            }else
            {
                maddress = db.user_registration_addr.FirstOrDefault(x => x.muid == getid);
                if (maddress != null)
                {
                    addr = maddress.maddress1;
                    if (maddress.maddress2 != null)
                    {
                        addr = addr + "<br/>" + maddress.maddress2;
                    }
                    if (maddress.mlandmark != null)
                    {
                        addr = addr + "<br/>" + maddress.mlandmark;

                    }
                    addr = addr + "<br/>" + maddress.mcity + (maddress.mpostcode != null ? " - " + maddress.mpostcode : "");
                    var data = new AgreementAddressViewModel
                    {
                        id = maddress.museraddrid,
                        address = addr,
                    };
                    return Ok(data);
                }
            }
            return Ok(addr);
        }
        [HttpGet]
        [Route("api/UserApi/GetUserAddresses")]
        public IHttpActionResult GetUserAddresses(int getid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            List<UsersAddressViewModel> maddresses = new List<UsersAddressViewModel>();
           
                maddresses = (from us in db.user_registration_addr
                              where us.muid==getid
                            select new UsersAddressViewModel
                            {
                                museraddrid=us.museraddrid,
                                maddress1=us.maddress1,
                                maddress2=us.maddress2,
                                mcity=us.mcity,
                                mpostcode=us.mpostcode,
                                maddresstype=us.maddresstype,
                                mlandmark=us.mlandmark,
                            }).ToList();
                if (maddresses.Count < 0)
                {
                    return Ok("empty");
                }
                return Ok(maddresses);
          
        }
        [HttpGet]
        [Route("api/UserApi/UpdateDefaultAddress")]
        public IHttpActionResult UpdateDefaultAddress(int getid,int schoolid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            var maddress = db.user_registration_addr.FirstOrDefault(x => x.muid == schoolid && x.maddresstype=="Default");
            if (maddress != null)
            {
                maddress.maddresstype = "";
                db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid==getid);
            if (maddress != null)
            {
                maddress.maddresstype = "Default";
                db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Ok("Done");
            }
            return Ok("empty");

        }
        [HttpGet]
        [Route("api/UserApi/DeleteAddress")]
        public IHttpActionResult DeleteAddress(int getid, int schoolid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            var maddress = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == getid);
            if (maddress != null)
            {
                db.Entry(maddress).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return Ok("Done");
            }
            maddress = db.user_registration_addr.FirstOrDefault(x => x.muid == schoolid && x.maddresstype == "Default");
            if (maddress == null)
            {
                var maddress2 = db.user_registration_addr.FirstOrDefault(x => x.muid == schoolid);
                if (maddress2 != null)
                {
                    maddress.maddresstype = "Default";
                    db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                
            }

            return Ok("empty");

        }
        [HttpGet]
        [Route("api/UserApi/GetUserContacts")]
        public IHttpActionResult GetUserContacts(int getid)
        {
            var db = new LeasingDbEntities();
            var ucontactmore = new List<UserContactsMoreViewModel>();
            var ucontacts = (from uc in db.user_contact_more
                             where uc.usermoreid == getid
                             select new UserContactsMoreViewModel
                             {
                                 cusername = uc.cusername,
                                 cemailaddress = uc.cemailaddress,
                                 cphone = uc.cphone,
                                 cposition = uc.cposition,
                                 Id = uc.Id,
                                 cdefault=uc.cdefault,   
                                 csurname=uc.csurname,
                             }).ToList();
            if (ucontacts.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(ucontacts);
        }
        [HttpGet]
        [Route("api/UserApi/UpdateDefaultContact")]
        public IHttpActionResult UpdateDefaultContact(int getid, int schoolid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            var maddress = db.user_contact_more.FirstOrDefault(x => x.usermoreid == schoolid && x.cdefault == 1);
            if (maddress != null)
            {
                maddress.cdefault = 0;
                db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            maddress = db.user_contact_more.FirstOrDefault(x => x.Id == getid);
            if (maddress != null)
            {
                maddress.cdefault = 1;
                db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return Ok("Done");
            }
            return Ok("empty");

        }
        [HttpGet]
        [Route("api/UserApi/DeleteContact")]
        public IHttpActionResult DeleteContact(int getid, int schoolid)
        {
            var db = new LeasingDbEntities();

            var addr = "";
            var maddress = db.user_contact_more.FirstOrDefault(x => x.Id == getid);
            if (maddress != null)
            {
                db.Entry(maddress).State = System.Data.Entity.EntityState.Deleted;
                db.SaveChanges();
                return Ok("Done");
            }
            maddress = db.user_contact_more.FirstOrDefault(x => x.usermoreid == schoolid && x.cdefault == 1);
            if (maddress == null)
            {
                var maddress2 = db.user_contact_more.FirstOrDefault(x => x.usermoreid == schoolid);
                if (maddress2 != null)
                {
                    maddress.cdefault =1;
                    db.Entry(maddress).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

            }

            return Ok("empty");

        }
        [HttpGet]
        [Route("api/UserApi/ForgetUser")]
        public IHttpActionResult ForgetUser(string email)
        {
                var re = UsersDao.resetPwd(email);
                if (re == 1)
                {
                    return Json(new { Data = "", msg = "Success", muid = 1 });
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            
            
        }
    }
}
