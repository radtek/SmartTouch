using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class ContactTableType
    {
        public int ContactID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public int CommunicationID { get; set; }
        public string Title { get; set; }
        public string ContactImageUrl { get; set; }
        public int AccountID { get; set; }
        public string LeadSource { get; set; }
        public string HomePhone { get; set; }
        public string WorkPhone { get; set; }
        public string MobilePhone { get; set; }
        public string PrimaryEmail { get; set; }
        public byte ContactType { get; set; }
        public string SSN { get; set; }
        public short LifecycleStage { get; set; }
        public bool DoNotEmail { get; set; }
        public DateTime? LastContacted { get; set; }
        public bool IsDeleted { get; set; }
        public Guid? ProfileImageKey { get; set; }
        public int ImageID { get; set; }
        public Guid ReferenceID { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? OwnerID { get; set; }
        public short? PartnerType { get; set; }
        public byte? ContactSource { get; set; }
        public int? SourceType { get; set; }
        public int? CompanyID { get; set; }
        public byte? LastContactedThrough { get; set; }
        public byte? FirstContactSource { get; set; }
        public int FirstSourceType { get; set; }
        public int LeadScore { get; set; }
        public IEnumerable<ContactCustomField> CustomFields { get; set; }
        public IEnumerable<DropdownValue> LeadSources { get; set; }
        public IEnumerable<DropdownValue> Communities { get; set; }
        public IEnumerable<Address> Addresses { get; set; }
        public IEnumerable<Phone> Phones { get; set; }
        public IEnumerable<Email> Emails { get; set; }
        public Image ContactImage { get; set; }
        public IList<Url> SocialMediaUrls { get; set; }
        public bool IncludeInReports { get; set; }
    }
}
