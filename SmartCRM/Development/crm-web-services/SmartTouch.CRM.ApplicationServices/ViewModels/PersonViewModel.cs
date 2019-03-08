using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Opportunities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class PersonViewModel //: IPersonViewModel
    {
        /// <summary>
        /// First Name of the Contact
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Last Name of the Contact
        /// </summary>
        public string LastName { get; set; }
        /// <summary>
        /// Company Name
        /// </summary>
        public string CompanyName { get; set; }
        /// <summary>
        /// Computed Full Name
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// Title [Mr, Mrs, etc.,]
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Selected Lead Source
        /// </summary>
        public IEnumerable<DropdownValueViewModel> SelectedLeadSource { get; set; }
        /// <summary>
        /// Social Security Number
        /// </summary>
        public string SSN { get; set; }
        /// <summary>
        /// Lifecycle Stage
        /// </summary>
        public short LifecycleStage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short PreviousLifecycleStage { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short? PartnerType { get; set; }
        /// <summary>
        /// Do not email this contact
        /// </summary>
        public bool DoNotEmail { get; set; }
        /// <summary>
        /// Contact Owner name
        /// </summary>
        public string OwnerName { get; set; }
        /// <summary>
        /// Owner ID
        /// </summary>
        public int? OwnerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? CompanyID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? PreviousOwnerId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual int ContactID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContactImageUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid? ProfileImageKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int AccountID { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int LeadScore { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public RelationshipViewModel RelationshipViewModel { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public CommunicationViewModel Communication { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ActionViewModel> Actions { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<OpportunityBuyer> Opportunities { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TourViewModel> Tours { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<AddressViewModel> Addresses { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<AddressViewModel> AddressList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DropdownValueViewModel> PartnerTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DropdownValueViewModel> LifecycleStages { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DropdownValueViewModel> AddressTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DropdownValueViewModel> PhoneTypes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Phone> Phones { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Phone> PhoneList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Email> Emails { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Email> EmailList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<Url> SocialMediaUrls { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<Tag> TagsList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<dynamic> LeadSources { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<DropdownValueViewModel> Communities { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<dynamic> SecondaryEmails { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LifeCycle { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public short DropdownValueTypeId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastContacted { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public byte? LastContactedThrough { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContactType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? CreatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime CreatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? LastUpdatedBy { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastUpdatedOn { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastTouchedDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastTouchedType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<NoteViewModel> Notes { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastContactedString { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ImageViewModel Image { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IList<ContactCustomFieldMapViewModel> CustomFields { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<CustomFieldTabViewModel> CustomFieldTabs { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Guid ReferenceId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DateFormat { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ContactSource? ContactSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public ContactSource? FirstContactSource { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? SourceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? FirstSourceType { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IsLifecycleChanged { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public virtual CRMOutlookSyncViewModel OutlookSync { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TagViewModel> PopularTags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<TagViewModel> RecentTags { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string LastNote { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? LastNoteDate { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ContactSummary { get; set; }
        /// <summary>
        /// 
        /// </summary>
        /// 
        public int FormId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool IncludeInReports { get; set; }
    }
}
