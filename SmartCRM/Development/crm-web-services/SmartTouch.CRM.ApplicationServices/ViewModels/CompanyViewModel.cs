using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ICompanyViewModel
    {
        string CompanyName { get; set; }

        int ContactID { get; set; }
        string ContactImageUrl { get; set; }
        Guid? ProfileImageKey { get; set; }
        int AccountID { get; set; }
        string WorkPhone { get; set; }
        string PrimaryEmail { get; set; }
        bool DoNotEmail { get; set; }
        int? LastUpdatedBy { get; set; }
        DateTime? LastUpdatedOn { get; set; }

        ICommunicationViewModel Communication { get; set; }
        IEnumerable<IAddressViewModel> Addresses { get; set; }
        IEnumerable<ActionViewModel> Actions { get; set; }
        Dictionary<int, string> AddressTypes { get; set; }
    }

    public class CompanyViewModel //: ICompanyViewModel
    {
        public string CompanyName { get; set; }

        public virtual int ContactID { get; set; }
        public int? CompanyID { get; set; }
        public string ContactImageUrl { get; set; }
        public Guid? ProfileImageKey { get; set; }
        public int AccountID { get; set; }
        public string OwnerName { get; set; }
        public int? OwnerId { get; set; }
        //public string PrimaryEmail { get; set; }
        public bool DoNotEmail { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastUpdatedBy { get; set; }
        public int? PreviousOwnerId { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string ContactType { get; set; }
        public string DateFormat { get; set; }
        public ContactSource? FirstContactSource { get; set; } 

        public ICommunicationViewModel Communication { get; set; }
        public IList<AddressViewModel> Addresses { get; set; }
        public string Address { get; set; }
        public IEnumerable<dynamic> AddressTypes { get; set; }
        public IEnumerable<dynamic> PhoneTypes { get; set; }
       
        public IList<Phone> Phones { get; set; }
        public IList<Phone> PhoneList { get; set; }
        public IList<Email> Emails { get; set; }
        public IList<Email> EmailList { get; set; }
        public IList<Url> SocialMediaUrls { get; set; }
        public IEnumerable<Tag> TagsList { get; set; }

        //public IEnumerable<string> SecondaryEmails { get; set; }
        public IList<dynamic> SecondaryEmails { get; set; }
        public RelationshipViewModel RelationshipViewModel;
        public ActionViewModel Action { get; set; }
        public IEnumerable<OpportunityViewModel> Opportunities { get; set; }
        public IEnumerable<ActionViewModel> Actions { get; set; }
        public IEnumerable<NoteViewModel> Notes { get; set; }
        public ImageViewModel Image { get; set; }
        public IEnumerable<DropdownValueViewModel> LeadSources { get; set; }
        public IList<DropdownValueViewModel> SelectedLeadSource { get; set; }

        public IEnumerable<ContactCustomFieldMapViewModel> CustomFields { get; set; }
        public IEnumerable<CustomFieldTabViewModel> CustomFieldTabs { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
        public Guid ReferenceId { get; set; }
        public string LastNote { get; set; }
        public DateTime? LastNoteDate { get; set; }
        public string ContactSummary { get; set; }
    }
}
