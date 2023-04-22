using LeasingPortalApi.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult UserLogin(UsersViewModel model)
        {
            if (ModelState.IsValid)
            {
                var client = new WebClient();
                var secretKey = "6LfJV9MkAAAAAIDXl8-N0GRju5YOkkcVsZFKMEn6";
                var url = "https://www.google.com/recaptcha/api/siteverify?secret=" + secretKey + "&response=" + model.Captcha_Response;
                var response = client.DownloadString(url);
                var result = JsonConvert.DeserializeObject<ReCaptchaResponse>(response);

                //SessionWrapper.Db = "ERPDbFelixEntities";
                if (result.Success == true) 
                {
                    var re = UsersDao.IsUserLogin(model);

                    if (re != null)
                    {
                        //SessionWrapper.UserId = re.Id;
                        if (re.mstatus != "active")
                        {
                            return Json(new { Data = re, msg = "NotActive" });
                        }
                        else
                        {
                            if (re.mverify == 0)
                            {
                                return Json(new { Data = re, msg = "NotVerify" });

                            }
                            else
                            {
                                SessionWrapper.CustomerName = re.musertype;
                                SessionWrapper.CustomerEmail = re.museridemail;
                                SessionWrapper.CustomerId = re.muid;
                                SessionWrapper.Testemail = 1;
                                return Json(new { Data = re, msg = "Success" });

                            }
                        }
                        
                        //return RedirectToAction("index", "Dashboard", new { area = re.Role });
                    }
                    else
                    {
                        TempData["err"] = "Invalid Username or Password";
                        return Json(new { msg = "Invalid" });
                    }
                }
                else
                {
                    TempData["err"] = "Invalid Username or Password";
                    return Json(new { msg = "InvalidCaptcha" });
                }
            }
            else
            {
                TempData["err"] = "Invalid Username or Password";
                return Json(new { msg = "Invalid" });
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult CheckUser(string userid)
        {
            using (var db = new LeasingDbEntities())
            {
                var search = db.user_registration.FirstOrDefault(x => x.museridemail == userid);
                if (search == null)
                {
                    return Json(new { msg = "Success" });
                }
                else
                {
                    return Json(new { msg = "found" });
                }
            }
            
        }
        [HttpPost]
        public ActionResult CheckCampaignUser(string userid)
        {
            using (var db = new LeasingDbEntities())
            {
                var search = db.campaign_user_registration.FirstOrDefault(x => x.memailaddress == userid);
                if (search == null)
                {
                    return Json(new { msg = "Success" });
                }
                else
                {
                    return Json(new { msg = "found" });
                }
            }

        }
        [HttpPost]
        public ActionResult SaveUser(UsersAllDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var re = UsersDao.Adduser(model);
                if (re.Any())
                {
                    var d= new UsersViewModel
                    {
                        museridemail = model.museridemail,
                        muid = re[0],
                        muserid=re[1],
                        mname = model.mname,
                        musertype = model.musertype,
                        morgname=model.morgname,

                    };
                    return Json(new { Data = d, msg = "Success",muid=re[0] });
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult SaveCampaignUser(UsersAllDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var re = UsersDao.AddCampaignuser(model);
                if (re!=0)
                {
                    return Json(new { Data = "", msg = "Success"});
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult ProductRequest(ProductRequestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ctx = new LeasingDbEntities();
                if (model.category > 0)
                {
                    var catdatas = ctx.productcats.FirstOrDefault(x => x.mprodcatid == model.category);
                    if (catdatas != null)
                    {
                        model.searchproduct = catdatas.mcatname;
                    }
                }
                if(UsersDao.RequestEmail(model, "equipyourschool.co.uk"))
                {

                   return Json(new { msg = "Success"});
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult UpdateUser(UsersAllDetailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var re = UsersDao.Updateuser(model);
                if (re==1)
                {
                    var d = new UsersViewModel
                    {
                        museridemail = model.museridemail,
                        mname = model.mname,
                        musertype = model.musertype,
                        morgname = model.morgname,

                    };
                    return Json(new { Data = d, msg = "Success", muid = 1 });
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult SaveUserAddress(UsersAddressViewModel model)
        {
            if (ModelState.IsValid)
            {
                if(UsersDao.Adduseraddress(model))
                {
                    return Json(new { msg = "Success"});
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }

        public ActionResult SaveUserContact(UserContactsMoreViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (UsersDao.Addusercontact(model))
                {
                    return Json(new { msg = "Success" });
                }
                else
                {
                    return Json(new { msg = "Invalid" });
                }
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult ForgetUser(string email)
        {
            if (ModelState.IsValid)
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
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult PasswordCheck(int id, string oldpwd)
        {
            if (ModelState.IsValid)
            {
                var db = new LeasingDbEntities();
                if (oldpwd != null)
                {
                    var epwd = Encryption.Encrypt(oldpwd);
                    var re = db.user_registration.FirstOrDefault(x => x.muid == id && x.mpwd == epwd);
                    if (re != null)
                    {
                        return Json(new { status = 1, msg = "Success", muid = 1 });
                    }
                    else
                    {
                        return Json(new { status = 2 });
                    }
                }else
                {
                    return Json(new { status = 2 });
                }
                
            }
            return RedirectToAction("index");
        }
        [HttpPost]
        public ActionResult UpdateUserPassword(int mid,string pwd)
        {
            if (ModelState.IsValid)
            {
                if (pwd != null)
                {
                    var epwd = Encryption.Encrypt(pwd);
                    var re = UsersDao.updatepwd(mid, epwd);
                    if (re == 1)
                    {
                        return Json(new { status = 1, msg = "Success", muid = 1 });
                    }
                    else
                    {
                        return Json(new { msg = "Invalid" });
                    }
                }
                
            }
            return RedirectToAction("index");
        }
        public ActionResult VerifyAccount(string ag, string status)
        {
            var db = new LeasingDbEntities();
            var agid = int.Parse(Encryption.Decrypt(ag));
            if (status == "verify")
            {
                if (UsersDao.VerifyAccount(ag))
                {
                    TempData["verify"] = "Success";
                }
                else
                {
                    TempData["verify"] = "Failed";
                }

            }
            else
            {
                TempData["verify"] = "Failed";
            }
            return RedirectToAction("index","Leasing");
        }
    }
    
}
    