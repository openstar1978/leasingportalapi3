using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;

namespace LeasingPortalApi.Controllers.Api
{
    public class LeaseBankApiController : ApiController
    {
        [HttpGet]
        [Route("api/LeaseBankApi/GetBankById")]
        public IHttpActionResult GetBankById(int muserid)
        {
            var db = new LeasingDbEntities();
            var datafounds = (from mbank in db.leaseuserbanks
                              where mbank.muserid == muserid
                              select new LeaseBankViewModel
                              {
                                  muserid = muserid,
                                  mbankaccount = mbank.mbankaccount,
                                  maccountholder = mbank.maccountholder,
                                  mbankname = mbank.mbankname,
                                  mbankshort = mbank.mbankshort
                              }).FirstOrDefault();
            if (datafounds == null)
            {
                return Ok("empty");
            }

            return Ok(datafounds);
        }
        [HttpPost]
        [Route("api/LeaseBankApi/BankUpdate")]
        public IHttpActionResult BankUpdate(LeaseBankViewModel bankupdates)
        {
            var db = new LeasingDbEntities();
            if (bankupdates != null)
            {
                var banksearch = db.leaseuserbanks.FirstOrDefault(x => x.muserid == bankupdates.muserid);
                if (banksearch == null)
                {
                    var leasebanks = new leaseuserbank
                    {
                        maccountholder = bankupdates.maccountholder,
                        muserid = bankupdates.muserid,
                        mbankaccount = bankupdates.mbankaccount,
                        mbankname = bankupdates.mbankname,
                        mbankshort = bankupdates.mbankshort,
                        createdby=bankupdates.muserid,
                        createddate=DateTime.Now
                    };
                    db.leaseuserbanks.Add(leasebanks);
                    db.SaveChanges();
                }
                else
                {
                    banksearch.maccountholder = bankupdates.maccountholder;
                    banksearch.mbankaccount = bankupdates.mbankaccount;
                    banksearch.mbankname = bankupdates.mbankname;
                    banksearch.mbankshort = bankupdates.mbankshort;
                    banksearch.updatedby = bankupdates.muserid;
                    banksearch.updateddate = DateTime.Now;
                    db.Entry(banksearch).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return Ok("success");
            }

            return Ok("empty");



        }
    }
}
