using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace LeasingPortalApi.Models
{
    public class SupplierDao
    {
        public static bool OldAdduser(SupplierViewModel model, supplierequipmentviewmodel equipment, suppliermoreviewmodel more, List<suppliercontactsViewmodel> contacts)
        {
            using (var db = new LeasingDbEntities())
            {

                var obj = new supplier_registration
                {
                    museridemail = model.museridemail,
                    mstatus = "pending",
                    musertype = "supplier",
                    mpwd = Encryption.Encrypt(model.mpwd),
                    mname = model.mname,
                    morgname = model.morgname,
                    mphone = model.mphone,
                    maddress1 = model.maddress1,
                    mcounty = model.mtown,
                    mcity = model.mcity,
                    mcountry = model.mcountry,
                    mpostcode = model.mpostcode,
                    createddate = DateTime.Now,
                    createdby = 1,
                    maddress2 = model.maddress2,
                    morgregno = model.morgregno,
                    mvatno = model.mvatno,
                    use_lease = model.use_lease,
                };

                db.supplier_registration.Add(obj);
                db.SaveChanges();

                var supplierid = obj.msuid;
                var objre = new supplier_registration_detail
                {
                    com_logo = model.com_logo,
                    date_established = model.date_established,
                    com_type = model.com_type,
                    source_enquiry = model.source_enquiry,
                    target_places = model.target_places,
                    trading_as = model.trading_as,
                    website = model.website
                };
                db.supplier_registration_detail.Add(objre);
                db.SaveChanges();
                if (equipment != null)
                {
                    var obj2 = new supplier_equipment
                    {
                        supplierid = supplierid,
                        equipment_brand = equipment.equipment_brand,
                        equipment_sold = equipment.equipment_sold,
                        warrantly_length = equipment.warrantly_length,
                        warrranty_provider = equipment.warrranty_provider
                    };
                    db.supplier_equipment.Add(obj2);
                    db.SaveChanges();

                }
                if (more != null)
                {
                    var obj2 = new supplier_more
                    {
                        supplierid = supplierid,
                        monthly_lease = more.monthly_lease,
                        monthly_sales = more.monthly_sales,

                    };
                    db.supplier_more.Add(obj2);
                    db.SaveChanges();

                }
                if (contacts.Count > 0)
                {
                    foreach (var ab in contacts)
                    {
                        if (ab.susername != null && ab.susername != "")
                        {
                            var obj2 = new supplier_contacts
                            {
                                supplierid = supplierid,
                                susername = ab.susername,
                                semailaddress = ab.semailaddress,
                                sphone = ab.sphone,
                                sposition = ab.sposition

                            };
                            db.supplier_contacts.Add(obj2);
                            db.SaveChanges();
                        }

                    }
                }
                return true;

            }
            return false;
        }
        public static bool Adduser(SupplierViewModel model)
        {
            using (var db = new LeasingDbEntities())
            {

                var obj = new supplier_registration
                {
                    museridemail = model.museridemail,
                    mstatus = "pending",
                    musertype = "supplier",
                    mpwd = Encryption.Encrypt(model.mpwd),
                    mname = model.mname,
                    mlname = model.mlname,
                    morgname = model.morgname,
                    mphone = model.mphone,
                    maddress1 = model.maddress1,
                    mcounty = model.mtown,
                    mcity = model.mcity,
                    mcountry = model.mcountry,
                    mpostcode = model.mpostcode,
                    createddate = DateTime.Now,
                    createdby = 1,
                    maddress2 = model.maddress2,
                    morgregno = model.morgregno,
                    mvatno = model.mvatno,
                    use_lease = model.use_lease,
                    

                };
                db.supplier_registration.Add(obj);
                db.SaveChanges();
                var objre = new supplier_registration_detail
                {
                    com_logo = model.com_logo,
                    date_established = model.date_established,
                    com_type = model.com_type,
                    source_enquiry = model.source_enquiry,
                    target_places = model.target_places,
                    trading_as = model.trading_as,
                    website = model.website
                };
                db.supplier_registration_detail.Add(objre);
                db.SaveChanges();
                var d = RegistrationSuccessEmail(model.museridemail);

                return true;

            }
            return false;
        }
        public static SupplierViewModel GetSupplierById(int id)
        {
            using (var db = new LeasingDbEntities())
            {
                var obj = (from s in db.supplier_registration
                           join sa in db.supplier_registration_detail on s.msuid equals sa.msuid into sadata
                           from sa in sadata.DefaultIfEmpty()
                           where s.msuid == id
                           select new SupplierViewModel
                           {
                               com_logo=sa.com_logo,
                               morgname=s.morgname,
                               maddress1=s.maddress1,
                               mcity=s.mcity,
                               maddress2=s.maddress2,
                               mpostcode=s.mpostcode,
                               mphone=s.mphone,
                               museridemail=s.museridemail,
                           }).FirstOrDefault();
                return obj;

            }
            //return false;
        }
        public static string RegistrationSuccessEmail(string emailto)
        {
            var db = new LeasingDbEntities();
            var fmaster = db.featuremasters.FirstOrDefault(x => x.featurename == "testemail");
            var testemail = 0;
            if (fmaster != null)
            {
                testemail = 1;
            }
            UrlHelper url = new UrlHelper();
            var body = "Dear Supplier,<br/><br/>";
            body += "Thank you for registered with EYS. <br/>Your application is under process and  our team will get back to you soon until that you can check your application by clicking<a href='http://equipyourschool.co.uk/supplier'> here</a>.<br/><br/>";
            body += "<br/>Regards,<br/>";
            body += "EYS Team";
            byte[] b = { };
            if (MailDao.MailSend("Registration successful on EYS", body,emailto, b) == "done")
            {
                return ("done");
            }
            else
            {
                return ("");
            }
            return "done";

        }



    }
}
