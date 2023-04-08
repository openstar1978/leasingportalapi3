using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Models
{
    public class UsersDao
    {
        public static List<int> Adduser(UsersAllDetailViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {
                var ids = new List<int>();
                var mschoolid = model.mschoolid;
                if (mschoolid > 0)
                {
                    var schooldata = db.UKSchools.FirstOrDefault(x => x.id == mschoolid && x.SchoolName.Trim() == model.morgname.Trim());
                    if (schooldata == null)
                    {
                        mschoolid = 0;
                    }
                }
                var obj2 = new user_registration_more
                {
                    morgname = model.morgname,
                    mphone = model.mphone,
                    mschoolid=mschoolid
                };
                db.user_registration_more.Add(obj2);
                db.SaveChanges();
                var musermoreid = obj2.musermoreid;
                var st = "active";
                if (model.mschoolid == 0)
                {
                    st = "deactive";
                }
                var obj = new user_registration
                {
                    musermoreid = musermoreid,
                    mpwd = Encryption.Encrypt(model.mpwd),
                    mname = model.mname,
                    msname=model.msname,
                    museridemail = model.museridemail,
                    mstatus = st,
                    musertype = "Approver",
                    createddate = DateTime.Now,
                    createdby = 1,
                    mposition=model.mposition,
                    mverify=0,
                };

                db.user_registration.Add(obj);

                db.SaveChanges();
                var muid = obj.muid;
                if (model.subscribe == 1)
                {
                    var sub = new user_subscriptions
                    {
                        emailid = model.museridemail,
                        active = 1
                    };
                    db.user_subscriptions.Add(sub);
                    db.SaveChanges();
                }
                var obj3 = new user_registration_addr
                {
                    maddress1 = model.maddress1,
                    maddress2 = model.maddress2,
                    maddresstype = (model.moreaddress.maddress1==null || model.moreaddress.mcity==null)?"Default":null,
                    mcity = model.mcity,
                    mcountry = model.mcountry,
                    mlandmark = model.mlandmark,
                    mpostcode = model.mpostcode,
                    muid = musermoreid
                };
                db.user_registration_addr.Add(obj3);
                db.SaveChanges();
                if (model.manotherusername != "" && model.manotherusername != null && model.manotheruseremail != null && model.manotheruseremail != "")
                {
                    var pass = RandomPassword();
                    var musertype = "";
                    if (model.musertype == "Buyer")
                    {
                        musertype = "Approver";
                    }
                    else
                    {
                        musertype = "Buyer";
                    }
                    if (SendEmail(model.manotheruseremail, musertype, pass))
                    {
                        var obj4 = new user_registration
                        {
                            musermoreid = musermoreid,
                            mpwd = Encryption.Encrypt(pass),
                            mname = model.manotherusername,
                            museridemail = model.manotheruseremail,
                            mstatus = "active",
                            musertype = musertype,
                            createddate = DateTime.Now,
                            createdby = muid
                        };
                        db.user_registration.Add(obj4);
                        db.SaveChanges();
                    }

                }

                if (model.maccountantemail != "" && model.maccountantemail != null)
                {
                    var pass = RandomPassword();
                    var musertype = "Accountant";
                    if (SendEmail(model.manotheruseremail, musertype, pass))
                    {
                        var obj4 = new user_registration
                        {
                            musermoreid = musermoreid,
                            mpwd = Encryption.Encrypt(pass),
                            mname = "Accountant",
                            museridemail = model.manotheruseremail,
                            mstatus = "active",
                            musertype = musertype,
                            createddate = DateTime.Now,
                            createdby = muid
                        };
                        db.user_registration.Add(obj4);
                        db.SaveChanges();
                    }
                }
                byte cd = 1;
                if (model.morecontacts.Count > 0)
                {
                    cd = 0;
                }
                var obj6 = new user_contact_more
                {
                    usermoreid = musermoreid,
                    cusername = model.mname,
                    csurname = model.msname,
                    cemailaddress = model.museridemail,
                    cphone = model.mphone,
                    cposition = model.mposition,
                    cdefault = cd
                };
                db.user_contact_more.Add(obj6);
                db.SaveChanges();
                if (model.morecontacts.Count > 0)
                {
                    foreach(var cn in model.morecontacts)
                    {
                        if(cn.cusername!=null && cn.cusername != "")
                        {
                            var obj5 = new user_contact_more
                            {
                                usermoreid = musermoreid,
                                cusername = cn.cusername,
                                cemailaddress = cn.cemailaddress,
                                cphone = cn.cphone,
                                cposition = cn.cposition,
                                csurname=cn.csurname,
                                cdefault=1,
                            };
                            db.user_contact_more.Add(obj5);
                            db.SaveChanges();
                        }
                    }
                }

                if(model.moreaddress.maddress1!=null && model.moreaddress.mcity != null)
                {
                    obj3 = new user_registration_addr
                    {
                        maddress1 = model.moreaddress.maddress1,
                        maddress2 = model.moreaddress.maddress2,
                        maddresstype = "Default",
                        mcity = model.moreaddress.mcity,
                        mpostcode = model.moreaddress.mpostcode,
                        muid = musermoreid
                    };
                    db.user_registration_addr.Add(obj3);
                    db.SaveChanges();
                }
                ids.Add(musermoreid);
                ids.Add(muid);
                string Url = "equipyourschool.co.uk";
                var emailreturn = VerifyEmail(obj, Url);
                //var emailreturn=SendThankYouemail(obj,Url);
                return ids;

            }
            return null;
        }
        public static bool VerifyAccount(string ag)
        {
            var db = new LeasingDbEntities();
            var agid = int.Parse(Encryption.Decrypt(ag));
            var obj = (from ab in db.user_registration
                       where ab.muid == agid
                       select ab).FirstOrDefault();
            if (obj != null)
            {
                obj.mverify = 1;
                db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                string Url = "equipyourschool.co.uk";
                var emailreturn = SendThankYouemail(obj, Url);
                return true;
            }
            return false;
        }
        //public static List<schoolmasterviewmodel> Getsschool()
        //{
        //    var list = new List<schoolmasterviewmodel>();
        //    using (var db = new LeasingDbEntities())
        //    {
        //        foreach (var a in db.tbl_school_master)
        //        {
        //            list.Add(new Models.schoolmasterviewmodel
        //            {
        //                mschoolid = a.mschoolid,
        //                mschoolname = a.mschoolname,
                        
        //            });
        //        }
        //    }
        //    return list;
        //}
        public static bool SendThankYouemail(user_registration obj, string requestedurl)
        {
        
            var body = "";
            body = "<table><tr height=50></tr></table>";

            body += "<table width='750' align='center' border='0' cellpadding='0' cellspacing='0' style='border: 1px solid #44266c;padding: 23px;'><tr>" +
                "<td align=center>  <img src='https://equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log-dark.png' id='eyslogo'  height='60' border='0' alt='' align='center'></td>" +
                "</tr>" +
                "<tr height=10></tr>" +
                  "<tr align=center>" +
                "<td><font size='3' color='#9900ff' face=calibri><strong>A portal strictly for Education only</strong></font></td>" +
                "</tr>" +
                "<tr height=30></tr>" +
                "<tr align=left >" +
                "<td><font size='3' color='' face=calibri>Welcome to EYS (equipyourschool.co.uk). A platform designed only for the schools</ font></td>" +
                "</tr>" +
                "<tr height=20></tr>" +
                "<tr align=left >" +
                "<td><font size='3' color='' face=calibri>Your account has been successfully created.</ font></td>" +
                "</tr>" +
                "<tr height=20></tr>" +
                "<tr align=left >" +
                "<td><font size='3' color='' face=calibri>You can now starting using EYS to save your school huge sums of money, ensuring your school stays upto datewith the latest technology at all tiems without hurting your budget.  </ font></td>" +
                "</tr>" +
                "<tr height=20></tr>" +
                "<tr width=100%>" +
            "<td><font size='3' color='' face=calibri>If you have any queries in regards to using EYS and how it all works, please request a callback by clicking HERE</ font></td>" +
            "</tr>" +
                "<tr height=20></tr>" +
                "<tr width=100%>" +
            "<td><font size='3' color='' face=calibri>Yours sincerely</font></td>" +
            "</tr>" +
            "<tr height=20 width=50%>" +
            "<td><font size='3' color='' face=calibri>EYS Admin</font></td>" +
            "</tr>" +
                "</table>";
            byte[] b = { };
            if (MailDao.MailSend("Welcome to EYS (equipyourschool.co.uk). A platform designed only for the schools", body, obj.museridemail, b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");



        }
        /*body += "<!DOCTYPE html>";
            body += "<html lang='en' xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office'>";
            body += "<head>";
            body += "<meta charset='utf-8'>";
            body += "<meta name='viewport' content='width=device-width,initial-scale=1'>";
            body += "<meta name='x-apple-disable-message-reformatting'>";
            body += "<style>";
            body += ".box_border { border: 0px solid #0fb5de !important; padding: 20px; background-color: #7140be; border-radius: 20px; } .box_border .txt_clr { color: #fff; } .box_border .clr { color: #0fb5de; } .box_border .mb-10 { margin-bottom: 10px; } .box_border .mb-20 { margin-bottom: 20px; } .box_border .mb-30 { margin-bottom: 30px; } .box_border .mb-60 { margin-bottom: 60px; }";
            body += ".col-md-1, .col-md-2, .col-md-3, .col-md-4, .col-md-5, .col-md-6, .col-md-7, .col-md-8, .col-md-9, .col-md-10, .col-md-11{ position: relative; width: 100%; padding-right: 15px; padding-left: 15px; }";
            body += ".col-md-12 { position: relative; width: 100%; }";
            body += "b, strong { font-weight: bolder; }";
            body += "@media (min-width: 768px) { .col-md-2 { flex: 0 0 16.666667%; max-width: 16.666667%; } }";
            body += "@media (min-width: 768px) { .col-md-3 { flex: 0 0 20%; max-width: 20%; } }";
            body += "@media (min-width: 768px) { .col-md-4 { flex: 0 0 33.333333%; max-width: 33.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-6 { flex: 0 0 60%; max-width: 60%; } }";
            body += "@media (min-width: 768px) { .col-md-8 { flex: 0 0 66.666667%; max-width: 66.666667%;} }";
            body += "@media (min-width: 768px) { .col-md-10 { flex: 0 0 83.333333%; max-width: 83.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-12 { flex: 0 0 100%; max-width: 100%; } }";
            body += ".black_clr { color: #000; }";
            body += "p { display: block; margin-block-start: 1em; margin-block-end: 1em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += "h1, h2, h3, h4, h5, h6, p { margin: 0; padding: 0; }";
            body += "h4 { font-size: 1.2rem; font-family: inherit; font-weight: 500; display: block; margin-block-start: 1.33em; margin-block-end: 1.33em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += ".redeembox_img img{ height: 100% !important; max-width: none !important; } ";
            body += "table, td, div, h1, p { font-family:Arial, sans-serif; } ";
            body += "@media (max-width: 1200px) and (min-width: 992px) { .redeembox .mainbox.col-md-3 {flex: 0 0 20%;max-width: 20%;} .redeembox .mainbox.col-md-6 {flex: 0 0 60%;max-width: 60%;}} ";
            body += "@media (max-width: 991px) and (min-width: 767px) { .redeembox .mainbox.col-md-3 { flex: 0 0 10%; max-width: 10%; } .redeembox .mainbox.col-md-6 { flex: 0 0 80%; max-width: 80%; } }";
            body += ".section1_code { padding: 20px; background: #fff; border-radius: 15px; }";
            body += "*, ::after, ::before { box-sizing: border-box; }";
            body += ".row { display: flex; flex-wrap: wrap; margin-right: -15px; margin-left: -15px; }";
            body += "html, body { font-family: 'Open Sans'; text-align: left; font-weight: 400; font-size: 13px; line-height: 1.7; width: 100%; color: #ffffff; }";
            body += ".redeembox_img img { height: 100% !important; max-width: none !important; }";
            body += "img { vertical-align: middle; border-style: none; max-width: 100%; height: auto; }";
            body += ".box_border .txt_clr { color: #fff; }";
            body += "div {display:block;}";
            body += ".section1_code { width:100%;}";
            body += "a { text-decoration: none; }";
            body += ".btn:not(:disabled):not(.disabled) { cursor: pointer; }";
            body += ".btn { display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; user-select: none; border: 1px solid transparent; padding: .375rem .75rem; font-size: 1rem; line-height: 1.5; border-radius: .25rem; }";
            body += "@media (max-width: 767px) { .col-md-8 { max-width: 100% !important; width: 100% !important; } .col-md-4 { max-width: 100% !important; width: 100% !important; } }";
            body += "@media (max-width: 767px) { .redeembox .mainbox.col-md-3 { display:none; } }";
            body += "</style>";
            body += "</head>";
            body += "<body>";

            body += "<div class='redeembox row'>";
            //Column 1 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 1 End

            //Column 2 Start
            body += "<div class='mainbox col-md-6 box_border'>";

            //Logo Start
            body += "<div style='text-align:center;'>";
            body += "<img class='mb-30' width='260' style='-webkit-user-drag:none !important;' src='https://www.equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' />";
            body += "</div>";
            //Logo End           

            //Section 1 Start
            body += "<div class='row'>";
            body += "<div class='col-md-12 section1_code'>";
            body += "<font size='3' color='' face=calibri>Welcome to EYS (equipyourschool.co.uk). A platform designed only for the schools</font>";
            body += "<div style='text-align:center;'>";
            body += "<img class='mb-30' width='260' style='-webkit-user-drag:none !important;' src='https://www.equipyourschool.co.uk/Content/images/hownew.jpg' />";
            body += "</div>";
            body += "<span style='display:block; color:#333;'>Please write to us if you face any difficulty</span>";
            body += "<span style='display:block; margin-top:15px; color:#333;'>Warm regards,</span>";
            body += "<span style='display:block; color:#333;'><strong>EYS Team</strong></span>";
            body += "</div>";
            body += "</div>";
            //Section 1 End          

            body += "</div>";
            //Column 2 End

            //Column 3 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 3 End           

            body += "</body>";
            body += "</html>";
            */
        public static bool VerifyEmail(user_registration obj, string requestedurl)
        {
            UrlHelper url = new UrlHelper();
            var body = "";
            body += "<!DOCTYPE html>";
            body += "<html lang='en' xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office'>";
            body += "<head>";
            body += "<meta charset='utf-8'>";
            body += "<meta name='viewport' content='width=device-width,initial-scale=1'>";
            body += "<meta name='x-apple-disable-message-reformatting'>";
            body += "<style>";
            body += ".box_border { border: 0px solid #0fb5de !important; padding: 20px; background-color: #7140be; border-radius: 20px; } .box_border .txt_clr { color: #fff; } .box_border .clr { color: #0fb5de; } .box_border .mb-10 { margin-bottom: 10px; } .box_border .mb-20 { margin-bottom: 20px; } .box_border .mb-30 { margin-bottom: 30px; } .box_border .mb-60 { margin-bottom: 60px; }";
            body += ".col-md-1, .col-md-2, .col-md-3, .col-md-4, .col-md-5, .col-md-6, .col-md-7, .col-md-8, .col-md-9, .col-md-10, .col-md-11{ position: relative; width: 100%; padding-right: 15px; padding-left: 15px; }";
            body += ".col-md-12 { position: relative; width: 100%; }";
            body += "b, strong { font-weight: bolder; }";
            body += "@media (min-width: 768px) { .col-md-2 { flex: 0 0 16.666667%; max-width: 16.666667%; } }";
            body += "@media (min-width: 768px) { .col-md-3 { flex: 0 0 20%; max-width: 20%; } }";
            body += "@media (min-width: 768px) { .col-md-4 { flex: 0 0 33.333333%; max-width: 33.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-6 { flex: 0 0 60%; max-width: 60%; } }";
            body += "@media (min-width: 768px) { .col-md-8 { flex: 0 0 66.666667%; max-width: 66.666667%;} }";
            body += "@media (min-width: 768px) { .col-md-10 { flex: 0 0 83.333333%; max-width: 83.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-12 { flex: 0 0 100%; max-width: 100%; } }";
            body += ".black_clr { color: #000; }";
            body += "p { display: block; margin-block-start: 1em; margin-block-end: 1em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += "h1, h2, h3, h4, h5, h6, p { margin: 0; padding: 0; }";
            body += "h4 { font-size: 1.2rem; font-family: inherit; font-weight: 500; display: block; margin-block-start: 1.33em; margin-block-end: 1.33em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += ".redeembox_img img{ height: 100% !important; max-width: none !important; } ";
            body += "table, td, div, h1, p { font-family:Arial, sans-serif; } ";
            body += "@media (max-width: 1200px) and (min-width: 992px) { .redeembox .mainbox.col-md-3 {flex: 0 0 20%;max-width: 20%;} .redeembox .mainbox.col-md-6 {flex: 0 0 60%;max-width: 60%;}} ";
            body += "@media (max-width: 991px) and (min-width: 767px) { .redeembox .mainbox.col-md-3 { flex: 0 0 10%; max-width: 10%; } .redeembox .mainbox.col-md-6 { flex: 0 0 80%; max-width: 80%; } }";
            body += ".section1_code { padding: 20px; background: #fff; border-radius: 15px; }";
            body += "*, ::after, ::before { box-sizing: border-box; }";
            body += ".row { display: flex; flex-wrap: wrap; margin-right: -15px; margin-left: -15px; }";
            body += "html, body { font-family: 'Open Sans'; text-align: left; font-weight: 400; font-size: 13px; line-height: 1.7; width: 100%; color: #ffffff; }";
            body += ".redeembox_img img { height: 100% !important; max-width: none !important; }";
            body += "img { vertical-align: middle; border-style: none; max-width: 100%; height: auto; }";
            body += ".box_border .txt_clr { color: #fff; }";
            body += "div {display:block;}";
            body += ".section1_code { width:100%;}";
            body += "a { text-decoration: none; }";
            body += ".btn:not(:disabled):not(.disabled) { cursor: pointer; }";
            body += ".btn { display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; user-select: none; border: 1px solid transparent; padding: .375rem .75rem; font-size: 1rem; line-height: 1.5; border-radius: .25rem; }";
            body += "@media (max-width: 767px) { .col-md-8 { max-width: 100% !important; width: 100% !important; } .col-md-4 { max-width: 100% !important; width: 100% !important; } }";
            body += "@media (max-width: 767px) { .redeembox .mainbox.col-md-3 { display:none; } }";
            body += "</style>";
            body += "</head>";
            body += "<body>";

            body += "<div class='redeembox row'>";
            //Column 1 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 1 End

            //Column 2 Start
            body += "<div class='mainbox col-md-6 box_border'>";

            //Logo Start
            body += "<div style='text-align:center;'>";
            body += "<a href='https://www.equipyourschool.co.uk'><img class='mb-30' width='260' style='-webkit-user-drag:none !important;' src='https://www.equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' /></a>";
            body += "</div>";
            //Logo End           

            //Section 1 Start
            body += "<div class='row'>";
            body += "<div class='col-md-12 section1_code'>";
            body += "<h4 style='color:#333'><strong>Welcome to EYS, a leasing platform.</strong></h4>";
            body += "<div style = 'margin:0 auto;text-align:left;' ><p> Please verify your account by clicking on below link.</p><br/><a style = 'text-decoration:none;text-align:left;' href = 'http://" + requestedurl + "/User/VerifyAccount?ag=" + url.Encode(Encryption.Encrypt(obj.muid.ToString())) + "&status=verify' target = '_blank' data-saferedirecturl='#'><span style='text-align:center;display:inline-block;border:solid 1px #017fbe;color:#017fbe;padding:8px 10px;font-family:Lato,Arial;font-size:14px;text-decoration:none;font-weight:400;margin-right:10px'>Verify Now</span></a>" +
                                    "</div>";
                                   
           
            body += "<span style='display:block; color:#333;'>Please write to us if you face any difficulty</span>";
            body += "<span style='display:block; margin-top:15px; color:#333;'>Warm regards,</span>";
            body += "<span style='display:block; color:#333;'><strong>EYS Team</strong></span>";
            body += "</div>";
            body += "</div>";
            //Section 1 End          

            body += "</div>";
            //Column 2 End

            //Column 3 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 3 End           

            body += "</body>";
            body += "</html>";
            byte[] b = { };
            if (MailDao.MailSend("Verify your account on EYS", body, obj.museridemail, b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");



        }
        public static bool RequestEmail(ProductRequestViewModel obj, string requestedurl)
        {
            var ctx = new LeasingDbEntities();
            UrlHelper url = new UrlHelper();
            var body = "";
            body += "<!DOCTYPE html>";
            body += "<html lang='en' xmlns='http://www.w3.org/1999/xhtml' xmlns:o='urn:schemas-microsoft-com:office:office'>";
            body += "<head>";
            body += "<meta charset='utf-8'>";
            body += "<meta name='viewport' content='width=device-width,initial-scale=1'>";
            body += "<meta name='x-apple-disable-message-reformatting'>";
            body += "<style>";
            body += ".box_border { border: 1px solid #0fb5de !important;background-color: #7140be; padding: 20px; border-radius: 20px; } .box_border .txt_clr { color: #fff; } .box_border .clr { color: #0fb5de; } .box_border .mb-10 { margin-bottom: 10px; } .box_border .mb-20 { margin-bottom: 20px; } .box_border .mb-30 { margin-bottom: 30px; } .box_border .mb-60 { margin-bottom: 60px; }";
            body += ".col-md-1, .col-md-2, .col-md-3, .col-md-4, .col-md-5, .col-md-6, .col-md-7, .col-md-8, .col-md-9, .col-md-10, .col-md-11{ position: relative; width: 100%; padding-right: 0px; padding-left: 0px; }";
            body += ".col-md-12 { position: relative; width: 100%; }";
            body += "b, strong { font-weight: bolder; }";
            body += "@media (min-width: 768px) { .col-md-2 { flex: 0 0 16.666667%; max-width: 16.666667%; } }";
            body += "@media (min-width: 768px) { .col-md-3 { flex: 0 0 20%; max-width: 20%; } }";
            body += "@media (min-width: 768px) { .col-md-4 { flex: 0 0 33.333333%; max-width: 33.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-6 { flex: 0 0 60%; max-width: 60%; } }";
            body += "@media (min-width: 768px) { .col-md-8 { flex: 0 0 66.666667%; max-width: 66.666667%;} }";
            body += "@media (min-width: 768px) { .col-md-10 { flex: 0 0 83.333333%; max-width: 83.333333%; } }";
            body += "@media (min-width: 768px) { .col-md-12 { flex: 0 0 100%; max-width: 100%; } }";
            body += ".black_clr { color: #000; }";
            body += "p { display: block; margin-block-start: 1em; margin-block-end: 1em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += "h1, h2, h3, h4, h5, h6, p { margin: 0; padding: 0; }";
            body += "h4 { font-size: 1.2rem; font-family: inherit; font-weight: 500; display: block; margin-block-start: 1.33em; margin-block-end: 1.33em; margin-inline-start: 0px; margin-inline-end: 0px; }";
            body += ".redeembox_img img{ height: 100% !important; max-width: none !important; } ";
            body += "table, td, div, h1, p { font-family:Arial, sans-serif; } ";
            body += "@media (max-width: 1200px) and (min-width: 992px) { .redeembox .mainbox.col-md-3 {flex: 0 0 20%;max-width: 20%;} .redeembox .mainbox.col-md-6 {flex: 0 0 60%;max-width: 60%;}} ";
            body += "@media (max-width: 991px) and (min-width: 767px) { .redeembox .mainbox.col-md-3 { flex: 0 0 10%; max-width: 10%; } .redeembox .mainbox.col-md-6 { flex: 0 0 80%; max-width: 80%; } }";
            body += ".section1_code { padding: 20px; background: #fff; border-radius: 15px; }";
            body += "*, ::after, ::before { box-sizing: border-box; }";
            body += ".row { display: flex; flex-wrap: wrap; margin-right: -15px; margin-left: -15px; }";
            body += "html, body { font-family: 'Open Sans'; text-align: left; font-weight: 400; font-size: 13px; line-height: 1.7; width: 100%; color: #ffffff; }";
            body += ".redeembox_img img { height: 100% !important; max-width: none !important; }";
            body += "img { vertical-align: middle; border-style: none; max-width: 100%; height: auto; }";
            body += ".box_border .txt_clr { color: #fff; }";
            body += "div {display:block;}";
            body += ".section1_code { width:100%;}";
            body += "a { text-decoration: none; }";
            body += ".btn:not(:disabled):not(.disabled) { cursor: pointer; }";
            body += ".btn { display: inline-block; font-weight: 400; text-align: center; vertical-align: middle; user-select: none; border: 1px solid transparent; padding: .375rem .75rem; font-size: 1rem; line-height: 1.5; border-radius: .25rem; }";
            body += "@media (max-width: 767px) { .col-md-8 { max-width: 100% !important; width: 100% !important; } .col-md-4 { max-width: 100% !important; width: 100% !important; } }";
            body += "@media (max-width: 767px) { .redeembox .mainbox.col-md-3 { display:none; } }";
            body += "</style>";
            body += "</head>";
            body += "<body>";

            body += "<div class='redeembox row'>";
            //Column 1 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 1 End

            //Column 2 Start
            body += "<div class='mainbox col-md-6 box_border'>";

            //Logo Start
            
            body += "<div style='text-align:center;'>";
            body += "<a href='https://www.equipyourschool.co.uk'><img class='mb-30' width='260' style='-webkit-user-drag:none !important;' src='https://www.equipyourschool.co.uk/Content/images/logo/openstar-school-leasing-log.png' /></a>";
            body += "</div>";
            //Logo End           

            //Section 1 Start
            body += "<div class='row' style='background:#fff;padding:5px;'>";
            body += "<table border=1 style=width:100%;>";
            if (obj.muserid > 0)
            {
                var getschool = (from u in ctx.user_registration
                                 join uc in ctx.user_contact_more on u.muid equals uc.usermoreid
                                 join um in ctx.user_registration_more  on u.musermoreid equals um.musermoreid
                                 where u.muid==obj.muserid
                                 select new
                                 {
                                     um.morgname,
                                     uc.cusername,
                                     uc.csurname,
                                     uc.cposition,
                                     uc.cemailaddress,
                                     uc.cphone
                                 }).FirstOrDefault();

                if (getschool != null)
                {
                    body += "<tr>";
                    body += "<td style=width:30%;>School Detail : </td>";
                    body += "<td>" + getschool.morgname + "<br/>"+getschool.cusername+" "+getschool.csurname +",<br/>"+getschool.cposition+"<br/>"+getschool.cphone+"<br/>"+getschool.cemailaddress+"</td>";
                    body += "</tr>";
                }
                
            }
            body += "<tr>";
            body += "<td style=width:30%;>Name : </td>";
            body += "<td>"+obj.morgname+"</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<td style=width:30%;>Email address : </td>";
            body += "<td>" + obj.museridemail + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<td style=width:30%;>Interested category / Product : </td>";
            body += "<td>" + obj.searchproduct + "</td>";
            body += "</tr>";
            body += "<tr>";
            body += "<td style=width:30%;>Interested to see the product range : </td>";
            body += "<td>" + (obj.interested==true?"Yes":"No") + "</td>";
            body += "</tr>";
            
            if (obj.specificproduct!=null && obj.specificproduct != "")
            {
                body += "<tr>";
                body += "<td style=width:30%;>Specific products : </td>";
                body += "<td>" + obj.specificproduct + "</td>";
                body += "</tr>";

            }
            body += "</table>";

            body += "</div>";
            body += "</div>";
            //Section 1 End          

            body += "</div>";
            //Column 2 End

            //Column 3 Start
            body += "<div class='mainbox col-md-3'></div>";
            //Column 3 End           

            body += "</body>";
            body += "</html>";
            byte[] b = { };
            if (MailDao.MailSend("A Product Request", body, "shahpalakh@gmail.com", b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            //return File(stream.ToArray(), "application/pdf", "OrderStatus.pdf");



        }
        public static bool Adduseraddress(UsersAddressViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {
                var muid = model.muid;
                var maddr = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == model.museraddrid);
                if (maddr != null)
                {
                    maddr.maddress1 = model.maddress1;
                    maddr.maddress2 = model.maddress2;
                    maddr.maddresstype = model.maddresstype;
                    maddr.mcity = model.mcity;
                    maddr.mcountry = int.Parse(model.mcountry);
                    maddr.mlandmark = model.mlandmark;
                    maddr.mpostcode = model.mpostcode;
                    db.Entry(maddr).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();       
                }
                else
                {
                    var obj3 = new user_registration_addr
                    {
                        maddress1 = model.maddress1,
                        maddress2 = model.maddress2,
                        maddresstype = model.maddresstype,
                        mcity = model.mcity,
                        mcountry = int.Parse(model.mcountry),
                        mlandmark = model.mlandmark,
                        mpostcode = model.mpostcode,
                        muid = muid,
                    };
                    db.user_registration_addr.Add(obj3);

                }
                db.SaveChanges();
                return true;
            }
        }

        public static bool Addusercontact(UserContactsMoreViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {
                var id = model.Id;
                var maddr = db.user_contact_more.FirstOrDefault(x => x.Id == model.Id);
                if (maddr != null)
                {
                    maddr.cusername = model.cusername;
                    maddr.csurname = model.csurname;
                    maddr.cposition = model.cposition;
                    maddr.cemailaddress = model.cemailaddress;
                    maddr.cphone = model.cphone;
                    maddr.usermoreid = model.usermoreid;
                    db.Entry(maddr).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var obj3 = new user_contact_more
                    {
                        cusername = model.cusername,
                        csurname = model.csurname,
                        cposition = model.cposition,
                        cemailaddress = model.cemailaddress,
                        cphone = model.cphone,
                        usermoreid = model.usermoreid,
                };
                    db.user_contact_more.Add(obj3);

                }
                db.SaveChanges();
                return true;
            }
        }
        public static UserContactsMoreViewModel getusercontact(int contactid)
        {
            using (var db = new LeasingDbEntities())
            {
                var userc = new UserContactsMoreViewModel();
                var maddr = db.user_contact_more.FirstOrDefault(x => x.Id == contactid);
                if (maddr != null)
                {
                    userc.cusername = maddr.cusername;
                    userc.csurname = maddr.csurname;
                    userc.cposition = maddr.cposition;
                    userc.cemailaddress=maddr.cemailaddress;
                    userc.cphone = maddr.cphone;
                    return userc;
                }
                return userc;
            }
        }
        public static UsersAddressViewModel getuseraddress(int contactid)
        {
            using (var db = new LeasingDbEntities())
            {
                var userc = new UsersAddressViewModel();
                var maddr = db.user_registration_addr.FirstOrDefault(x => x.museraddrid == contactid);
                if (maddr != null)
                {
                    userc.maddress1 = maddr.maddress1;
                    userc.maddress2 = maddr.maddress2;
                    userc.mcity = maddr.mcity;
                    userc.mlandmark = maddr.mlandmark;
                    userc.mpostcode = maddr.mpostcode;
                    return userc;
                }
                return userc;
            }
        }
        public static UsersAllDetailViewModel GetByOrgId(int muid)
        {
            using (var db = new LeasingDbEntities())
            {
                var users = (from po in db.user_registration_more.Where(x => x.musermoreid == muid)
                             join user in db.user_registration on po.musermoreid equals user.musermoreid
                             join c in db.user_registration_addr on po.musermoreid equals c.muid into cdatas
                             from c in cdatas.DefaultIfEmpty()
                             join uc in db.user_contact_more on po.musermoreid equals uc.usermoreid into ucdatas
                             from uc in ucdatas.DefaultIfEmpty()
                             select new UsersAllDetailViewModel
                             {
                                 mposition=uc.cposition,
                                museridemail=user.museridemail,
                                mname=user.mname,
                                 muid = po.musermoreid,
                                 morgname = po.morgname,
                                 mphone = po.mphone,
                                 maddress1=c.maddress1,
                                 maddress2=c.maddress2,
                                 mcity=c.mcity,
                                 mlandmark=c.mlandmark,
                                 mpostcode=c.mpostcode
                             }).FirstOrDefault();
                return users;
            }
        }
        public static UsersViewModel IsUserLogin(UsersViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {
                var search = db.user_registration.FirstOrDefault(x => x.museridemail == model.museridemail);
                if (search != null)
                {
                    if(search.mstatus == "active")
                    {
                        var pwd = Encryption.Encrypt(model.mpwd);
                        var searchnew = (from u in db.user_registration
                                         join um in db.user_registration_more on u.musermoreid equals um.musermoreid into umdatas
                                         from um in umdatas.DefaultIfEmpty()
                                         where u.museridemail == model.museridemail && u.mpwd == pwd
                                         select new UsersViewModel
                                         {
                                             mstatus=u.mstatus,
                                             morgname = um.morgname,
                                             museridemail = model.museridemail,
                                             muid = search.musermoreid.HasValue ? search.musermoreid.Value : 0,
                                             muserid = search.muid,
                                             mname = u.mname,
                                             musertype = search.musertype,
                                             mverify = (byte)(search.mverify.HasValue ? search.mverify.Value : 0),
                                         }).FirstOrDefault();
                        if (searchnew != null)
                        {
                            return searchnew;
                        }
                    }
                    else
                    {
                        var searchnew = new UsersViewModel()
                        {
                            mstatus=search.mstatus
                        };
                        return searchnew;
                    }
                    
                }


            }
            return null;
        }

        public static UsersAllDetailViewModel GetUserById(int id)
        {
            var db = new LeasingDbEntities();
            var users = (from u in db.user_registration
                         join ua in db.user_registration_addr on u.musermoreid equals ua.muid into uadata
                         from ua in uadata.DefaultIfEmpty()
                         join um in db.user_registration_more on u.musermoreid equals um.musermoreid into umdata
                         from um in umdata.DefaultIfEmpty()
                         where u.muid == id
                         select new UsersAllDetailViewModel
                         {
                             mname = u.mname,
                             morgname = um.morgname,
                             maddress1 = ua.maddress1,
                             maddress2 = ua.maddress2,
                             mcity = ua.mcity,
                             mcountry = ua.mcountry,
                             mpostcode = ua.mpostcode,
                             mlandmark = ua.mlandmark,
                             mphone = um.mphone,


                         }).FirstOrDefault();
            return users;
        }
        public static int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        // Generate a random string with a given size and case.   
        // If second parameter is true, the return string is lowercase  
        public static string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }
        public static string RandomPassword(int size = 0)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(2, false));
            return builder.ToString();
        }
        public static int Updateuser(UsersAllDetailViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {
                var ids = new List<int>();
                var obj2 = db.user_registration_more.FirstOrDefault(x => x.musermoreid == model.musermoreid);
                if (obj2 != null)
                {
                    obj2.morgname = model.morgname;
                    obj2.mphone = model.mphone;
                    db.Entry(obj2).State=System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                var musermoreid = obj2.musermoreid;
                var obj = db.user_registration.FirstOrDefault(x => x.musermoreid == model.musermoreid);
                if (obj != null)
                {
                    obj.mname = model.mname;
                    obj.msname = model.msname;
                    obj.mposition = model.mposition;
                    db.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                /*var obj3 = db.user_registration_addr.FirstOrDefault(x => x.muid == model.musermoreid);
                if (obj3 != null) { 
                    obj3.maddress1 = model.maddress1;
                    obj3.maddress2 = model.maddress2;
                    obj3.maddresstype = (model.moreaddress.maddress1 == null || model.moreaddress.mcity == null) ? "Default" : null;
                    obj3.mcity = model.mcity;
                    obj3.mcountry = model.mcountry;
                    obj3.mlandmark = model.mlandmark; 
                    obj3.mpostcode = model.mpostcode;
                    db.Entry(obj3).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                };*/
                return 1;

            }
            return 0;
        }
        public static int updatepwd(int mid,string pwd)
        {
            using (var db = new LeasingDbEntities())
            {
                var ids = new List<int>();
                var obj2 = db.user_registration.FirstOrDefault(x => x.muid == mid);
                if (obj2 != null)
                {
                    obj2.mpwd = pwd;
                    db.Entry(obj2).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges(); ;
                    if (sendUpdateEmail(obj2.museridemail, pwd))
                    {
                        return 1;
                    }
                    //db.SaveChanges();
                }
                else
                {
                    return 0;
                }

            }
            return 0;
        }
        public static int resetPwd(string email)
        {
            using (var db = new LeasingDbEntities())
            {
                var ids = new List<int>();
                var obj2 = db.user_registration.FirstOrDefault(x => x.museridemail == email);
                if (obj2 != null)
                {
                    string pwd = Encryption.Decrypt(obj2.mpwd);
                    if (sendForgetEmail(email, pwd))
                    {
                        return 1;
                    }
                    //db.SaveChanges();
                }
                else
                {
                    return 0;
                }
                
            }
            return 0;
        }
        public static bool sendForgetEmail(string sendto,string pass)
        {
            
            var body = "";
            body += "Hi,<br/>";
            body += "Below is your registered password which is used for login to equipyourschool.co.uk.<br/>";
            body += "For login to our portal <a href='http://equipyourschool.co.uk/Leasing'> Click here</a> and use below details:<br/>";
            body += "Password : " + pass + "<br/>";
            body += "<hr/>";
            body += "Thanks";
            //var cc = new MailAddress(model.SendCc);
            byte[] b = { };
            if (MailDao.MailSend("Verify your account on EYS", body,sendto, b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        public static bool sendUpdateEmail(string sendto, string pass)
        {
            var body = "";
            body += "Hi,<br/>";
            body += "Your password has been updated successfully.<br/>";
            body += "<hr/>";
            body += "Thanks";
            byte[] b = { };
            if (MailDao.MailSend("You password changed on EYS", body, sendto, b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        public static bool SendEmail(string sendto, string usertype, string pass)
        {
            var body = "";
            body += "Hi,<br/>";
            body += "Your are successfully registered as a " + usertype + " to our portal";
            body += "For login to our portal <a href='http://equipyourschool.co.uk/Leasing'> Click here</a> and use below details:<br/>";
            body += "UserName : " + sendto + "<br/>";
            body += "Password : " + pass + "<br/>";
            body += "<hr/>";
            body += "Thanks";
            byte[] b = { };
            //var cc = new MailAddress(model.SendCc);
            if (MailDao.MailSend("Welcome to EYS", body, sendto, b) == "done")
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}