using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactListEntry : IShallowContact
    {
        public int AccountID { get; set; }
        public int ContactID { get; set; }
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public int? CompanyID { get; set; }
        public int ContactType { get; set; }
        public string PrimaryEmail { get; set; }
        public int PrimaryContactEmailID { get; set; }
        public int PrimaryContactPhoneNumberID { get; set; }
        public EmailStatus PrimaryEmailStatus { get; set; }
        public bool DoNotEmail { get; set; }
        public short LifecycleStage { get; set; }
        public string LifecycleName { get; set; }
        public short PartnerType { get; set; }
        public string PartnerTypeName { get; set; }
        public string ContactImageUrl { get; set; }
        public Guid? ProfileImageKey { get; set; }
        public DateTime? LastTouched { get; set; }
        public string LastTouchedThrough { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string Address { get; set; }
        public int LeadScore { get; set; }
        public IEnumerable<dynamic> ContactSortTypes { get; set; }

        public string ImagePath { get { return string.Empty; } }
        public DateTime? LastContactedDate { get { return this.LastTouched; } }

        public string Phone { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FacebookUrl { get; set; }
        public string TwitterUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string GooglePlusUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string BlogUrl { get; set; }
        public IList<Phone> Phones { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }

        public IEnumerable<Int16> LeadSourceIds { get; set; }
        public string LeadSources { get; set; } //all leadsources
        public string LeadSourceDate { get; set; }
        public Int16 FirstLeadSourceId { get; set; }
        public string FirstLeadSource { get; set; }
        public DateTime? FirstLeadSourceDate { get; set; }

        public AddressViewModel PrimaryAddress { get; set; }     //  Advanced search
        public string Title { get; set; }
        public int? CreatedBy { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string DateFormat { get; set; }
        public IList<ContactCustomFieldMapViewModel> CustomFields { get; set; }
        public bool IsDelete { get; set; }
        public string SourceType { get; set; }
        public DateTime? LastNoteDate { get; set; }
        public string NoteSummary { get; set; }
        public int? FirstSourceType { get; set; }
        public string LastNote { get; set; }
    }

    public interface IContactListViewModel
    {
        IEnumerable<ContactListEntry> Contacts { get; set; }
    }

    public class ContactListViewModel : IContactListViewModel
    {
        public IEnumerable<ContactListEntry> Contacts { get; set; }
    }

    public interface IShallowContact
    { }
}
