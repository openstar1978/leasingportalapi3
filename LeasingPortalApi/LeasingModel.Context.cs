﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LeasingPortalApi
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class LeasingDbEntities : DbContext
    {
        public LeasingDbEntities()
            : base("name=LeasingDbEntities")
        {
            this.Database.CommandTimeout = 120;
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AgreementAddress> AgreementAddresses { get; set; }
        public virtual DbSet<agreementbank> agreementbanks { get; set; }
        public virtual DbSet<AgreementUserDetail> AgreementUserDetails { get; set; }
        public virtual DbSet<CityMaster> CityMasters { get; set; }
        public virtual DbSet<CommonField> CommonFields { get; set; }
        public virtual DbSet<CountryMaster> CountryMasters { get; set; }
        public virtual DbSet<deliverystatu> deliverystatus { get; set; }
        public virtual DbSet<faq_school> faq_school { get; set; }
        public virtual DbSet<faq_supplier> faq_supplier { get; set; }
        public virtual DbSet<featuremaster> featuremasters { get; set; }
        public virtual DbSet<leasedispatchdate> leasedispatchdates { get; set; }
        public virtual DbSet<leasesupplierpayment> leasesupplierpayments { get; set; }
        public virtual DbSet<leaseuserbank> leaseuserbanks { get; set; }
        public virtual DbSet<lessoractivity> lessoractivities { get; set; }
        public virtual DbSet<login_master> login_master { get; set; }
        public virtual DbSet<mailsetting> mailsettings { get; set; }
        public virtual DbSet<newsletter> newsletters { get; set; }
        public virtual DbSet<printquotelog> printquotelogs { get; set; }
        public virtual DbSet<prodrecent> prodrecents { get; set; }
        public virtual DbSet<prodsubvariationtwo> prodsubvariationtwoes { get; set; }
        public virtual DbSet<product> products { get; set; }
        public virtual DbSet<productaddondetail> productaddondetails { get; set; }
        public virtual DbSet<productblock> productblocks { get; set; }
        public virtual DbSet<productbrand> productbrands { get; set; }
        public virtual DbSet<productcat> productcats { get; set; }
        public virtual DbSet<productcomparativefield> productcomparativefields { get; set; }
        public virtual DbSet<productdealimage> productdealimages { get; set; }
        public virtual DbSet<productdetailmaster> productdetailmasters { get; set; }
        public virtual DbSet<productdetailsub> productdetailsubs { get; set; }
        public virtual DbSet<productfaq> productfaqs { get; set; }
        public virtual DbSet<productincorrectrept> productincorrectrepts { get; set; }
        public virtual DbSet<productlease> productleases { get; set; }
        public virtual DbSet<productquestion> productquestions { get; set; }
        public virtual DbSet<productratepercentage> productratepercentages { get; set; }
        public virtual DbSet<productregid> productregids { get; set; }
        public virtual DbSet<productreview> productreviews { get; set; }
        public virtual DbSet<productreviewbysupplier> productreviewbysuppliers { get; set; }
        public virtual DbSet<productreviewpic> productreviewpics { get; set; }
        public virtual DbSet<productsaledeal> productsaledeals { get; set; }
        public virtual DbSet<productseo> productseos { get; set; }
        public virtual DbSet<productshiping> productshipings { get; set; }
        public virtual DbSet<productsub> productsubs { get; set; }
        public virtual DbSet<productsubpic> productsubpics { get; set; }
        public virtual DbSet<ProductSubSupplier> ProductSubSuppliers { get; set; }
        public virtual DbSet<productsubtwo> productsubtwoes { get; set; }
        public virtual DbSet<productsubwarrantybysupplier> productsubwarrantybysuppliers { get; set; }
        public virtual DbSet<ProductSupplierSpecialOffer> ProductSupplierSpecialOffers { get; set; }
        public virtual DbSet<productunit> productunits { get; set; }
        public virtual DbSet<productvariationmaster> productvariationmasters { get; set; }
        public virtual DbSet<productwarrantybysupplier> productwarrantybysuppliers { get; set; }
        public virtual DbSet<productweekdeal> productweekdeals { get; set; }
        public virtual DbSet<SaveCart> SaveCarts { get; set; }
        public virtual DbSet<SaveSubCart> SaveSubCarts { get; set; }
        public virtual DbSet<StateMaster> StateMasters { get; set; }
        public virtual DbSet<SubCartAddress> SubCartAddresses { get; set; }
        public virtual DbSet<subsupplierstatu> subsupplierstatus { get; set; }
        public virtual DbSet<supplier_bank> supplier_bank { get; set; }
        public virtual DbSet<supplier_contacts> supplier_contacts { get; set; }
        public virtual DbSet<supplier_equipment> supplier_equipment { get; set; }
        public virtual DbSet<supplier_more> supplier_more { get; set; }
        public virtual DbSet<supplier_registration> supplier_registration { get; set; }
        public virtual DbSet<supplier_registration_detail> supplier_registration_detail { get; set; }
        public virtual DbSet<supplier_users_rights> supplier_users_rights { get; set; }
        public virtual DbSet<suppliercancelorder> suppliercancelorders { get; set; }
        public virtual DbSet<supplierpurchaseorder> supplierpurchaseorders { get; set; }
        public virtual DbSet<supplierstatu> supplierstatus { get; set; }
        public virtual DbSet<supplieruploadpermit> supplieruploadpermits { get; set; }
        public virtual DbSet<tbl_aboutus> tbl_aboutus { get; set; }
        public virtual DbSet<tbl_banner> tbl_banner { get; set; }
        public virtual DbSet<tbl_disclaimer> tbl_disclaimer { get; set; }
        public virtual DbSet<tbl_privacy_policy> tbl_privacy_policy { get; set; }
        public virtual DbSet<tbl_return_refund_policy> tbl_return_refund_policy { get; set; }
        public virtual DbSet<tbl_school_department> tbl_school_department { get; set; }
        public virtual DbSet<tbl_terms_conditions> tbl_terms_conditions { get; set; }
        public virtual DbSet<tblschoolarea> tblschoolareas { get; set; }
        public virtual DbSet<TicketDetail> TicketDetails { get; set; }
        public virtual DbSet<TicketReply> TicketReplies { get; set; }
        public virtual DbSet<user_contact_more> user_contact_more { get; set; }
        public virtual DbSet<user_registration> user_registration { get; set; }
        public virtual DbSet<user_registration_addr> user_registration_addr { get; set; }
        public virtual DbSet<user_registration_more> user_registration_more { get; set; }
        public virtual DbSet<user_subscriptions> user_subscriptions { get; set; }
        public virtual DbSet<usercomparesubquotation> usercomparesubquotations { get; set; }
        public virtual DbSet<userleaseagreement> userleaseagreements { get; set; }
        public virtual DbSet<userleasedocument> userleasedocuments { get; set; }
        public virtual DbSet<userordermaster> userordermasters { get; set; }
        public virtual DbSet<userquotationmaster> userquotationmasters { get; set; }
        public virtual DbSet<usersubquotation> usersubquotations { get; set; }
        public virtual DbSet<usersubquotationwarranty> usersubquotationwarranties { get; set; }
        public virtual DbSet<AggregatedCounter> AggregatedCounters { get; set; }
        public virtual DbSet<Hash> Hashes { get; set; }
        public virtual DbSet<Job> Jobs { get; set; }
        public virtual DbSet<JobParameter> JobParameters { get; set; }
        public virtual DbSet<JobQueue> JobQueues { get; set; }
        public virtual DbSet<List> Lists { get; set; }
        public virtual DbSet<Schema> Schemata { get; set; }
        public virtual DbSet<Server> Servers { get; set; }
        public virtual DbSet<Set> Sets { get; set; }
        public virtual DbSet<State> States { get; set; }
        public virtual DbSet<Counter> Counters { get; set; }
        public virtual DbSet<AllProductsView> AllProductsViews { get; set; }
        public virtual DbSet<GetAgreementView> GetAgreementViews { get; set; }
        public virtual DbSet<UKSchool> UKSchools { get; set; }
        public virtual DbSet<UKSchoolType> UKSchoolTypes { get; set; }
        public virtual DbSet<campaign_user_registration> campaign_user_registration { get; set; }
    
        public virtual int GetAgreementsById(Nullable<int> getid)
        {
            var getidParameter = getid.HasValue ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("GetAgreementsById", getidParameter);
        }
    
        public virtual ObjectResult<GetsUser_Result> GetsUser()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetsUser_Result>("GetsUser");
        }
    
        public virtual ObjectResult<spGetFeatureProducts_Result> spGetFeatureProducts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetFeatureProducts_Result>("spGetFeatureProducts");
        }
    
        public virtual ObjectResult<spGetNewArrivalProducts_Result> spGetNewArrivalProducts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetNewArrivalProducts_Result>("spGetNewArrivalProducts");
        }
    
        public virtual ObjectResult<spGetOnSaleProducts_Result> spGetOnSaleProducts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetOnSaleProducts_Result>("spGetOnSaleProducts");
        }
    
        public virtual ObjectResult<spGetProducts_Result> spGetProducts(Nullable<int> getid)
        {
            var getidParameter = getid.HasValue ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetProducts_Result>("spGetProducts", getidParameter);
        }
    
        public virtual ObjectResult<spGetSpecialOfferProducts_Result> spGetSpecialOfferProducts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetSpecialOfferProducts_Result>("spGetSpecialOfferProducts");
        }
    
        public virtual ObjectResult<spGetWeekDealProducts_Result> spGetWeekDealProducts()
        {
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetWeekDealProducts_Result>("spGetWeekDealProducts");
        }
    
        public virtual ObjectResult<spGetSearchProducts_Result> spGetSearchProducts(string getid, string getproduct, string getproducts)
        {
            var getidParameter = getid != null ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(string));
    
            var getproductParameter = getproduct != null ?
                new ObjectParameter("getproduct", getproduct) :
                new ObjectParameter("getproduct", typeof(string));
    
            var getproductsParameter = getproducts != null ?
                new ObjectParameter("getproducts", getproducts) :
                new ObjectParameter("getproducts", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetSearchProducts_Result>("spGetSearchProducts", getidParameter, getproductParameter, getproductsParameter);
        }
    
        public virtual ObjectResult<spGetSearchProductsInCategory_Result> spGetSearchProductsInCategory(Nullable<int> catid, string getid, string getproduct, string getproducts)
        {
            var catidParameter = catid.HasValue ?
                new ObjectParameter("catid", catid) :
                new ObjectParameter("catid", typeof(int));
    
            var getidParameter = getid != null ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(string));
    
            var getproductParameter = getproduct != null ?
                new ObjectParameter("getproduct", getproduct) :
                new ObjectParameter("getproduct", typeof(string));
    
            var getproductsParameter = getproducts != null ?
                new ObjectParameter("getproducts", getproducts) :
                new ObjectParameter("getproducts", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetSearchProductsInCategory_Result>("spGetSearchProductsInCategory", catidParameter, getidParameter, getproductParameter, getproductsParameter);
        }
    
        public virtual ObjectResult<spGetFiltersQuery_Result> spGetFiltersQuery(string getid)
        {
            var getidParameter = getid != null ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetFiltersQuery_Result>("spGetFiltersQuery", getidParameter);
        }
    
        public virtual ObjectResult<spGetFilters_Result> spGetFilters(Nullable<int> getid)
        {
            var getidParameter = getid.HasValue ?
                new ObjectParameter("getid", getid) :
                new ObjectParameter("getid", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<spGetFilters_Result>("spGetFilters", getidParameter);
        }
    }
}
