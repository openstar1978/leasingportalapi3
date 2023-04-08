using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LeasingPortalApi.Controllers.Api
{
    public class ExtraPagesController : ApiController
    {
        [HttpGet]
        [Route("api/ExtraPageApi/GetSchoolFaqs")]
        public IHttpActionResult GetSchoolFaqs()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.faq_school.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetSupplierFaqs")]
        public IHttpActionResult GetSupplierFaqs()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.faq_supplier.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetDisclaimer")]
        public IHttpActionResult GetDisclaimer()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.tbl_disclaimer.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetPrivacyPolicy")]
        public IHttpActionResult GetPrivacyPolicy()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.tbl_privacy_policy.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetReturnPolicy")]
        public IHttpActionResult GetReturnPolicy()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.tbl_return_refund_policy.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetTermsConditions")]
        public IHttpActionResult GetTermsConditions()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.tbl_terms_conditions.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
        [HttpGet]
        [Route("api/ExtraPageApi/GetAboutUs")]
        public IHttpActionResult GetAboutUs()
        {
            var ctx = new LeasingDbEntities();
            var faqs = ctx.tbl_aboutus.ToList();
            if (faqs.Count == 0)
            {
                return Ok("empty");
            }
            return Ok(faqs);
        }
    }
}
