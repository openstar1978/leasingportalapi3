using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using LeasingPortalApi.Models;

namespace LeasingPortalApi.Controllers.Api
{
    public class WarrantyApiController : ApiController
    {
        [HttpGet]
        [Route("api/WarrantyApi/GetWarrantyDetails")]
        public IHttpActionResult GetWarrantyDetails(int msupplierwarranty1)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductSubWarrantyViewModel> warranties = null;
            var ctx = new LeasingDbEntities();
            warranties = (from ws in ctx.productsubwarrantybysuppliers
                          join w2 in ctx.productwarrantybysuppliers on ws.warrantyid equals w2.WarrantyId
                          where ws.warrantyid==msupplierwarranty1 
                          select new ProductSubWarrantyViewModel
                          {
                              subwarrantid=ws.subwarrantid,
                              WarrantyTitle=w2.WarrantyTitle,
                              warrantyprice=ws.warrantyprice,
                              warrantyterm=ws.warrantyterm,
                              warrantyid=w2.WarrantyId,
                              WType=w2.Type==2?"RTB":w2.Type==3?"CollectReturn":"On-Site",
                          }).ToList();
            


            if (warranties.Count == 0)
            {
                return Ok("");
            }
            
            return Ok(warranties);
        }
        [Route("api/WarrantyApi/GetSingleWarrantyDetails")]
        public IHttpActionResult GetSingleWarrantyDetails(int mprodid,int supid,int term)
        {
            //var id = int.Parse(Encryption.Decrypt(getid));
            //var id = getid;
            var seed = string.Empty;
            List<ProductSubWarrantyViewModel> warranties = null;
            var ctx = new LeasingDbEntities();
            Nullable<int> p = 0;
            var pdata = ctx.products.FirstOrDefault(x => x.mprodid==mprodid);

            if (term == 2)
            {
                if (pdata.msupplierwarranty2>0)
                {
                    p = pdata.msupplierwarranty2;
                }
                else
                {
                    p = switchfunction(pdata,3);
                }
            }
            else if(term==3)
            {
                if (pdata.msupplierwarranty3 > 0)
                {
                    p = pdata.msupplierwarranty3;
                }
                else
                {
                    p = switchfunction(pdata, 2);
                }
            }
            else if (term == 4)
            {
                if (pdata.msupplierwarranty4 > 0)
                {
                    p = pdata.msupplierwarranty4;
                }
                else
                {
                    p = switchfunction(pdata, 1);
                }
            }
            else if (term == 5)
            {
                if (pdata.msupplierwarranty5 > 0)
                {
                    p = pdata.msupplierwarranty5;
                }
                
            }

            warranties = (from ws in ctx.productsubwarrantybysuppliers
                          join w2 in ctx.productwarrantybysuppliers on ws.warrantyid equals w2.WarrantyId
                          where ws.warrantyid == p
                          select new ProductSubWarrantyViewModel
                          {
                              subwarrantid = ws.subwarrantid,
                              WarrantyTitle = w2.WarrantyTitle,
                              warrantyprice = ws.warrantyprice,
                              warrantyterm = ws.warrantyterm,
                              warrantyid = w2.WarrantyId,
                              WType = w2.Type == 2 ? "RTB" : w2.Type == 3 ? "CollectReturn" : "On-Site",
                          }).ToList();



            if (warranties.Count == 0)
            {
                return Ok("");
            }

            return Ok(warranties);
        }
        public static int switchfunction(product pdata,int cnt)
        {
            if (cnt == 3)
            {
                if (pdata.msupplierwarranty3 > 0)
                {
                    return pdata.msupplierwarranty3.HasValue ? pdata.msupplierwarranty3.Value : 0;
                }
                else if (pdata.msupplierwarranty4 > 0)
                {
                    return pdata.msupplierwarranty4.HasValue ? pdata.msupplierwarranty4.Value : 0;
                }
                else if (pdata.msupplierwarranty5 > 0)
                {
                    return pdata.msupplierwarranty5.HasValue ? pdata.msupplierwarranty5.Value : 0;
                }
            }
            else if (cnt == 2)
            {
                if (pdata.msupplierwarranty4 > 0)
                {
                    return pdata.msupplierwarranty4.HasValue ? pdata.msupplierwarranty4.Value : 0;
                }
                else if (pdata.msupplierwarranty5 > 0)
                {
                    return pdata.msupplierwarranty5.HasValue ? pdata.msupplierwarranty5.Value : 0;
                }
            }
            else if (cnt == 1)
            {
                if (pdata.msupplierwarranty5 > 0)
                {
                    return pdata.msupplierwarranty5.HasValue ? pdata.msupplierwarranty5.Value : 0;
                }
            }
            return 0;
        }
    }
}
