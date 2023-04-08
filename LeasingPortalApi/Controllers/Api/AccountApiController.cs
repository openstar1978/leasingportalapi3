using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;

namespace LeasingPortalApi.Controllers.Api
{
    public class AccountApiController : ApiController
    {
        [Route("api/AccountApi/GetCountries")]
        public IHttpActionResult GetCountries()
        {
            var db = new LeasingDbEntities();
            var countrydatas = (from con in db.CountryMasters
                                select new CountryStateCityViewModel.SelectModelForCountrStateCity
                                {
                                    Code = con.ID,
                                    Name = con.Name
                                }).ToList();
            if (countrydatas.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(countrydatas);
        }
        [Route("api/AccountApi/GetStates")]
        public IHttpActionResult GetStates(int cnid)
        {
            var db = new LeasingDbEntities();
            var countrydatas = (from st in db.StateMasters
                                join con in db.CountryMasters on st.CountryID equals con.ID
                                where con.ID==cnid
                                select new CountryStateCityViewModel.SelectModelForCountrStateCity
                                {
                                    Code = st.ID,
                                    Name = st.Name
                                }).ToList();
            if (countrydatas.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(countrydatas);
        }
        [Route("api/AccountApi/GetSchool")]
        public IHttpActionResult GetSchool()
        {
            var db = new LeasingDbEntities();
            var countrydatas = (from st in db.UKSchools
                                select new CountryStateCityViewModel.SelectModelForCountrStateCity
                                {
                                    Code = st.id,
                                    Name = st.SchoolName,
                                }).ToList();
            if (countrydatas.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(countrydatas);
        }
        [Route("api/AccountApi/GetSchoolByVal")]
        public IHttpActionResult GetSchoolByVal(string val)
        {
            var db = new LeasingDbEntities();
            var countrydatas = (from st in db.UKSchools
                                where st.SchoolName.Contains(val)
                                select new CountryStateCityViewModel.SelectModelForCountrStateCity
                                {
                                    Code = st.id,
                                    Name = st.SchoolName,
                                }).Take(8).ToList();
            if (countrydatas.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(countrydatas);
        }
        [Route("api/AccountApi/GetSchoolDetail")]
        public IHttpActionResult GetSchoolDetail(int detailid)
        {
            var db = new LeasingDbEntities();
            var countrydatas = (from st in db.UKSchools
                                where st.id==detailid
                                select st).FirstOrDefault();
            if (countrydatas==null)
            {
                return Ok("empty");
            }
            return Ok(countrydatas);
        }
    }

}
