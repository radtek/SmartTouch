using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactsDb
    {
        [Key]
        public int ContactID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string ContactImageUrl { get; set; }
        public Guid? ProfileImageKey { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public int LeadScore { get; set; }

        //public string LeadSource { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string WorkPhone { get; set; }
        public string PrimaryEmail { get; set; }
        public string SSN { get; set; }
        public bool? DoNotEmail { get; set; }

        public DateTime? LastContacted { get; set; }
        public byte? LastContactedThrough { get; set; }

        [ForeignKey("Owner")]
        public int? OwnerID { get; set; }
        public virtual UsersDb Owner { get; set; }

        [NotMapped]
        public ContactCreatorInfo CreaterInfo { get; set; }

        public int? CompanyID { get; set; }

        public int AccountID { get; set; }
        public ContactType ContactType { get; set; }
        //public LifecycleStage LifecycleStage { get; set; }
        //public PartnerType? PartnerType { get; set; }
        public short? LifecycleStage { get; set; }
        public short? PartnerType { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        //public ICollection<LeadAdapterDb> LeadAdapters { get; set; }

        public bool IsDeleted { get; set; }
        public ICollection<AddressesDb> Addresses { get; set; }
        public ICollection<ContactEmailsDb> ContactEmails { get; set; }
        public List<ContactRelationshipDb> ContactRelations { get; set; }
        public List<ContactRelationshipDb> RelatedContactRelations { get; set; }
        [ForeignKey("Communication")]
        public virtual int? CommunicationID { get; set; }
        public virtual CommunicationsDb Communication { get; set; }

        public virtual ICollection<ActionsDb> Actions { get; set; }
        public virtual ICollection<NotesDb> Notes { get; set; }
        public virtual ICollection<TourDb> Tours { get; set; }


        public ContactSource? ContactSource { get; set; }
        public ContactSource? FirstContactSource { get; set; }
        public int? SourceType { get; set; }
        public int? FirstSourceType { get; set; }


        [ForeignKey("Image")]
        public virtual int? ImageID { get; set; }
        public virtual ImagesDb Image { get; set; }
        public virtual IList<CampaignsDb> Campaign { get; set; }

        public Guid? ReferenceId { get; set; }

        public ICollection<ContactPhoneNumbersDb> ContactPhones { get; set; }
        public ICollection<ContactCustomFieldsDb> CustomFields { get; set; }
        public virtual ICollection<ContactLeadSourceMapDb> ContactLeadSources { get; set; }
        public virtual ICollection<ContactCommunityMapDb> Communities { get; set; }
        public ICollection<DropdownValueDb> DropDownValues { get; set; }


        //Elastic 
        public ICollection<WebVisitsDb> WebVisits { get; set; }
        public ICollection<ContactTagMapDb> Tags { get; set; }
        public ICollection<FormSubmissionDb> FormSubmissions { get; set; }
        [NotMapped]
        public IEnumerable<ContactTourCommunityMap> TourCommunity { get; set; }
        public IEnumerable<ContactActionMap> ContactActions { get; set; }
        [NotMapped]
        public string LastNote { get; set; }
        [NotMapped]
        public bool IncludeInReports { get; set; }
        [NotMapped]
        public IEnumerable<ContactNoteMap> ContactNotes { get; set; }

        [NotMapped]
        public bool IsActive { get; set; }
    }
}
