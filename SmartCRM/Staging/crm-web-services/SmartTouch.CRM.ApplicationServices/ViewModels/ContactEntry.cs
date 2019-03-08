using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Dropdowns;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactEntry : IShallowContact
    {
        public int Id { get; set; }
        public int ContactType { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public Email Email { get; set; }
        public short? LifeCycleStage { get; set; }
        public int? OwnerId { get; set; }
        public int TourType { get; set; }
        public IEnumerable<DropdownValue> LeadSources { get; set; }
    }

    public class ContactGridEntry : IShallowContact
    {
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public int ContactType { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string PrimaryEmail { get; set; }
        public int? CompanyID { get; set; }
        public int? CampaignLinkID { get; set; }
        public short? LifecycleStage { get; set; }
        public string LifecycleName { get; set; }
        public string Phone { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneExtension { get; set; }
        public int PrimaryContactEmailID { get; set; }
        public int PrimaryContactPhoneNumberID { get; set; }
        public string ContactImageUrl { get; set; }
        public DateTime? LastContactedDate { get; set; }
        public EmailStatus PrimaryEmailStatus { get; set; }
        public bool DoNotEmail { get; set; }
        public bool IsDelete { get; set; }
        public int? CreatedBy { get; set; }
        public string LastTouched { get; set; }
        public string LastTouchedThrough { get; set; }
    }

    public class ContactReportEntry : IShallowContact
    {
        public int ContactID { get; set; }
        public int ContactType { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }        
        public string PrimaryEmail { get; set; }
        public int? CompanyID { get; set; }
        public short? LifecycleStage { get; set; }
        public string LifecycleName { get; set; }
        public int PrimaryContactEmailID { get; set; }
        public int PrimaryContactPhoneNumberID { get; set; }        
        public DateTime? LastContactedDate { get; set; }
        public IEnumerable<short> AllLeadSources { get;set; }
        public short? PrimaryLeadSource { get; set; }
        public string LeadSources { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public int LeadScore { get; set; }

        public string Phone { get; set; }
        public string PhoneTypeName { get; set; }
        public string CountryCode { get; set; }
        public string Extension { get; set; }
    }

    public class ContactAdvancedSearchEntry : IShallowContact
    {
        public int ContactID { get; set; }
        public int ContactType { get; set; }
        public string Name { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string PrimaryEmail { get; set; }
        public int? CompanyID { get; set; }
        public int PrimaryContactEmailID { get; set; }
        public bool DoNotEmail { get; set; }
        public EmailStatus PrimaryEmailStatus { get; set; }
    }

    public class ViewContactEntry : IShallowContact
    {
        public int ContactID { get; set; }
    }

    public class BDXLeadReportEntry : IShallowContact
    {
        public int ContactID { get; set; }
        public string FullName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ContactCreated { get; set; }
        public string PrimaryEmail { get; set; }
        public string FileName { get; set; }
        public string LeadType { get; set; }
        public string CommunityName { get; set; }
        public long LeadAdapterJobLogDetailID { get; set; }
        public string LeadSource { get; set; }

        public string CommunityNumber { get; set; }
        public string MarketName { get; set; }
        public string PlanName { get; set; }
        public string PlanNumber { get; set; }
        public string Comments { get; set; }
        public string Phone { get; set; }
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string BuilderName { get; set; }
        public string BuilderNumber { get; set; }
        public string StateName { get; set; }
    }
}
