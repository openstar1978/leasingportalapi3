using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeasingPortalApi.Models
{
    public class EUploadViewModel
    {
        public List<EeUploadViewModel> uploads { get; set; }
    }
    public class EeUploadViewModel
    {
        public string id { get; set; }
        public string title { get; set; }
        public string base64 { get; set; }
        public string extension { get; set; }
        public string  type { get; set; }
        public bool contains_tags { get; set; }

    }
    public class EResponseUploadViewModel
    {
        public List<EeResponseUploadViewModel> uploads { get; set; }
        public List<ErrorViewModel> Error { get; set; }
    }
    public class ErrorViewModel
    {
        public int code { get; set; }
        public string type { get; set; }
        public string details { get; set; }
        public string href { get; set; }
    }

    public class EeResponseUploadViewModel
    {
        public string id { get; set; }
        public string file_name { get; set; }
        public bool processing { get; set; }
        public string type { get; set; }
        public bool contains_tags { get; set; }

    }
    public class ESignViewModel
    {
        public EnvelopeViewModel envelope { get; set; }
        public string redirect_uri { get; set; }
    }
    public class ESignerrorViewModel
    {
        public List<ErrorViewModel> error { get; set; }
    }
    
    public class EnvelopeViewModel
    {
        public string code { get; set; }
        public string details { get; set; }
        public string id { get; set; }
        public string title { get; set; }
        public string subject { get; set; }
        public string description { get; set; }
        public string language { get; set; }
        public string envelope_status { get; set; }
        public string signer_redirect_uri { get; set; }
        public List<DocumentViewModel> documents { get; set; }
        public List<signersViewModel> signers { get; set; }
        public envelopeoptionsViewMoel envelope_options { get; set; }
        public List<CarbonCopiesViewModel> carbon_copies { get; set; }
        public string redirect_uri { get; set; }
    }
    public class DocumentViewModel
    {
        public string title { get; set; }
        public documentFileViewModel document_file { get; set; }
        public UploadFileViewModel upload_file { get; set; }
        public List<AttachmentViewModel> attachment_files { get; set; }
        public List<documentfieldsViewModel> document_fields { get; set; }
        public List<CarbonCopiesViewModel> carbon_copies { get; set; }
    }
    public class documentFileViewModel
    {
        public string uri { get; set; }
        public int? pages { get; set; }
        public int? signature_certificate_page { get; set; }
    }
    public class UploadFileViewModel
    {
        public string id { get; set; }
    }
    public class AttachmentViewModel
    {
        public string id { get; set; }
    }
    public class documentfieldsViewModel
    {
        public string signer_email { get; set; }
        public string signer_idx { get; set; }
        public string field_type { get; set; }
        public bool field_required { get; set; }
        public string field_placeholder { get; set; }
        public string field_amount { get; set; }
        public string field_value { get; set; }
        public List<string> field_dropdown_options { get; set; }
        public DocumentPositionViewModel document_position { get; set; }
    }
    public class DocumentPositionViewModel
    {
        public string x { get; set; }
        public string y { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string size { get; set; }
        public int page { get; set; }


    }
    public class CarbonCopiesViewModel
    {
        public string name { get; set; }
        public string email { get; set; }
    }
    public class signersViewModel
    {
        public string id { get; set; }
        public string  name { get; set; }
        public string email { get; set; }
        public envelopeStatusViewModel envelope_status { get; set; }
        public envelopeAuthViewModel envelope_authentication { get; set; }
        public signeroptViewModel signer_options { get; set; }
        public signerdetViewModel signer_details { get; set; }
    }
    public class envelopeStatusViewModel
    {
        public string status { get; set; }
    }
    public class envelopeAuthViewModel
    {
        public string country_code { get; set; }
        public string  phone { get; set; }
        public string passcode { get; set; }

    }
    public class signeroptViewModel
    {
        public int auto_reminder_frequency { get; set; }
        public bool id_check_required { get; set; }
    }
    public class signerdetViewModel
    {
        public string primary_sequential_email { get; set; }
    }
    public class envelopeoptionsViewMoel
    {
        public bool dont_send_signing_emails { get; set; }
        public bool sign_in_sequential_order { get; set; }
        public string days_envelope_expires { get; set; }
        public bool validate_signer_mailboxes { get; set; }

    }

}