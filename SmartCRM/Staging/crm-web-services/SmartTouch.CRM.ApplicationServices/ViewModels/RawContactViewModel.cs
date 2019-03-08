using System;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class RawContactViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Title { get; set; }
        public string LeadSource { get; set; }        
        public string LifecycleStage { get; set; }
        public string PartnerType { get; set; }
        public bool DoNotEmail { get;set; }

        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public int AccountID { get; set; }

        public string PrimaryEmail { get; set; }
        public string SecondaryEmails { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string BlogUrl { get; set; }
        public string WebSiteUrl { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
        public bool IsDefault { get; set; }
        public int ImportDataId { get; set; }
        public int LeadAdapterRecordStatusId { get; set; }
        public Guid ReferenceId { get; set; }
        public int ContactID { get; set; }
        public bool IsDeleted { get; set; }
        public string CustomField { get; set; }
        public string PhoneData { get; set; }      
        public byte ContactStatusID { get; set; }
        public Guid ReferenceID { get; set; }
        public byte ContactTypeID { get; set; }
        public int OwnerID { get; set; }
        public int JobID { get; set; }
        public int LeadSourceID { get; set; }
        public string LeadAdapterRowData { get; set; }
        public int SerialId { get; set; }
        public bool IsDuplicate { get; set; }
    }
}
