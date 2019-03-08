using System;
using System.Collections.Generic;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Infrastructure.Domain;
using System.Linq;

namespace SmartTouch.CRM.Domain.Opportunities
{
    public class Opportunity : EntityBase<int>, IAggregateRoot
    {
        public int OpportunityID { get; set; }
        public IEnumerable<int> Contacts { get; set; }
        public string OpportunityName { get; set; }
        public decimal? Potential { get; set; }
        public short StageID { get; set; }
        public string Stage { get; set; }
        public DateTime? ExpectedClosingDate { get; set; }
        public string Description { get; set; }
        public int OwnerId { get; set; }
        public string UserName { get; set; }
        public int AccountID { get; set; }
        public int CreatedBy { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        public int? LastModifiedBy { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public IList<Tag> OpportunityTags { get; set; }
        public IList<PeopleInvolved> PeopleInvolved { get; set; }
        public string OpportunityType { get; set; }
        public string ProductType { get; set; }
        public string Address { get; set; }
        public int? ImageID { get; set; }
        public string Comments { get; set; }
        public string ContactName { get; set; }
        public int ContactID { get; set; }
        public byte ContactType { get; set; }
        public int TotalCount { get; set; }
        public IEnumerable<OpportunityBuyer> Buyers { get; set; }


        protected override void Validate()
        {
            if (string.IsNullOrEmpty(OpportunityName)) AddBrokenRule(OpportunityBusinessRule.OpportunityRequired);
            if (!string.IsNullOrEmpty(OpportunityName) && OpportunityName.Length > 75) AddBrokenRule(OpportunityBusinessRule.OpportunityNameMaxLength);
           // if (Contacts == null || !Contacts.Any()) AddBrokenRule(OpportunityBusinessRule.ContactsRequired);
            //if (StageID == 0) AddBrokenRule(OpportunityBusinessRule.StageRequired);
            if (Potential == 0) AddBrokenRule(OpportunityBusinessRule.PotentialRequired);
            if (!string.IsNullOrEmpty(Description) && Description.Length > 1000) AddBrokenRule(OpportunityBusinessRule.OpportunityDescriptionMaxLength);
            if (OwnerId == 0) AddBrokenRule(OpportunityBusinessRule.OwnerRequired);
            if (Potential > 922337203685477) AddBrokenRule(OpportunityBusinessRule.PotentialMaxValueValidation);
            if (Potential < 1) AddBrokenRule(OpportunityBusinessRule.PotentialMinValueValidation);
        }
    }
}
