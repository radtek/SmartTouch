using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DailySummaryEmail : ValueObjectBase
    {
        public int AccountID { get; set; }
        public string AccountName { get; set; }
        public string AccountEmail { get; set; }
        public string DomainURL { get; set; }
        public Address PrimaryAddress { get; set;}
        public string PrimaryPhone { get; set; }
        public List<UserSummary> Users { get; set; }

        protected override void Validate()
        {
        }
    }

    public class UserSummary
    {
        public int AccountId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public List<UserSettingsSummary> UserSettings { get; set; }
    }

    public class UserSettingsSummary
    {
        public List<UserContactActivitySummary> ContactsSummary { get; set; }
        public List<UserOpportunityActivitySummary> OpportunitySummary { get; set; }
        public List<UserContactActivitySummary> OwnerChangedContacts { get; set; }
        public UserActionActivitySummary UserContactActionSummary { get; set; }
    }

    public class UserContactActivitySummary
    {
        public ContactType? ContactType { get; set; }
        public string ContactName { get; set; }
        public int? contactId { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public int PrimaryPhoneTypeValueId { get; set; }
    }

    public class UserOpportunityActivitySummary
    {
        public int? OpportunityId { get; set; }
        public string OpportunityName { get; set; }
    }
    
    public class ContactAccountGroup
    {
        public string Data { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int? ContactsCount { get; set; }
        public int? ElasticCount { get; set; }
    }

    public class UserActionActivitySummary
    {
        public List<string> ActionDetails { get; set; }
        public List<string> ReminderDetails { get; set; }
    }
}
