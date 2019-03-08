using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class OpportunityViewModel
    {
        public int OpportunityID { get; set; }
        public string OpportunityName { get; set; }
        public decimal Potential { get; set; }
        public string Stage { get; set; }
        public short StageID { get; set; }
        public short PreviousStageID { get; set; }
        public int AccountID { get; set; }
        public string People { get; set; }
        public int OwnerId { get; set; }
        public int PreviousOwnerId { get; set; }
        public string Description { get; set; }
        public int CreatedBy { get; set; }
        public string Currency { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public string DateFormat { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public string UserName { get; set; }
        public bool IsDeleted { get; set; }
        public string OpportunityType { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public string Comments { get; set; }
        public int UserID { get; set; }
        public int ContactCount { get; set; }
        public string ContactName { get; set; }
        public int ContactID { get; set; }
        public byte ContactType { get; set; }
        public int TotalCount { get; set; }
        public ImageViewModel Image { get; set; }
        public IEnumerable<PeopleInvolvedViewModel> PeopleInvolved { get; set; }
        public IEnumerable<ContactEntry> Contacts { get; set; }
        public IEnumerable<TagViewModel> OpportunityTags { get; set; }
        public IEnumerable<Tag> TagsList { get; set; }
        public IEnumerable<ActionViewModel> Actions { get; set; }
        public IEnumerable<dynamic> RelationshipTypes { get; set; }
        public IEnumerable<UserViewModel> Users { get; set; }
        public IEnumerable<dynamic> Stages { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }
    }
}
