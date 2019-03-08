using System;
using System.Collections.Generic;
using System.Linq;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Domain.Tours
{
    public class Tour : EntityBase<int>, IAggregateRoot
    {
        private string tourDetails;
        public string TourDetails { get { return tourDetails; } set { tourDetails = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public int? AccountId { get; set; }
        public virtual short CommunityID { get; set; }
        public virtual DateTime TourDate { get; set; }
        public virtual short TourType { get; set; }
        public IEnumerable<ReminderType> ReminderTypes { get;set; }
        public Nullable<DateTime> ReminderDate { get; set; }
        public NotificationStatus? NotificationStatus { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }

        public virtual ICollection<Contacts.Contact> Contacts { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        public virtual User User { get; set; }
        public bool IsTourCreate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IcsCanlender { get; set; }
        public bool SelectAll { get; set; }
        public IEnumerable<TourContact> TourContacts { get; set; }
        public IList<int> OwnerIds { get; set; }
        public string UserName { get; set; }
        public IEnumerable<int> ContactIDS { get; set; }
        public IDictionary<int,Guid> EmailGuid { get; set; }
        public IDictionary<int,Guid> TextGuid { get; set; }


        protected override void Validate()
        {
            if (!IsTourDetailsValid()) AddBrokenRule(TourBusinessRule.DetailsMaxLengthReached);
            if (!IsCommunityValid()) AddBrokenRule(TourBusinessRule.CommunityRequired);
            if (!IsTourTypeValid()) AddBrokenRule(TourBusinessRule.TourTypeRequired);
            if (!IsContactsCountValid()) AddBrokenRule(TourBusinessRule.ContactsRequired);
            if (!IsReminderTypeValid()) AddBrokenRule(TourBusinessRule.ReminderNotApplicable);
            if ((ReminderTypes != null && ReminderTypes.Any()) && !IsReminderDateValid()) AddBrokenRule(TourBusinessRule.ReminderDateNotValid);
            if (!IsTourDateValid()) AddBrokenRule(TourBusinessRule.TourDateNotValid);
        }

        public IList<BusinessRule> OutlookValidation()
        {
            var brokenRules = new List<BusinessRule>();
            if (!IsTourDetailsValid()) brokenRules.Add(TourBusinessRule.DetailsMaxLengthReached);
            if (!IsCommunityValid()) brokenRules.Add(TourBusinessRule.CommunityRequired);
            if (!IsTourTypeValid()) brokenRules.Add(TourBusinessRule.TourTypeRequired);
            if (!IsContactsCountValid()) brokenRules.Add(TourBusinessRule.ContactsRequired);
            return brokenRules;
        }

        public bool IsTourDetailsValid()
        {
            bool result = (!string.IsNullOrEmpty(TourDetails) && TourDetails.Length > 1000) ? false : true;
            return result;
        }

        public bool IsCommunityValid()
        {
            bool result = CommunityID == 0 ? false : true;
            return result;
        }

        public bool IsTourTypeValid()
        {
            bool result = TourType == 0 ? false : true;
            return result;
        }

        public bool IsContactsCountValid()
        {
            bool result = Contacts == null || Contacts.Count == 0 ? false : true;
            return result;
        }

        public bool IsReminderTypeValid()
        {
            bool result = ((ReminderTypes != null && ReminderTypes.Any()) && TourDate <= DateTime.Now.ToUniversalTime()) ? false : true;
            return result;
        }

        public bool IsReminderDateValid()
        {
            if (ReminderDate != null && ReminderDate.Value != null)
            {
                bool result = (ReminderDate == DateTime.MinValue || ReminderDate > TourDate.AddMinutes(5) 
                    || ReminderDate < DateTime.Now.ToUniversalTime().AddMinutes(-5) || ReminderDate == null) ? false : true;
                return result;
            }
            else
                return false;
        }

        public bool IsTourDateValid()
        {
            bool result = TourDate == null || TourDate == new DateTime(1970, 1, 1) ? false : true;
            return result;
        }
    }

    public class TourContactsSummary
    {
        public int ContactId { get; set; }
        public ContactType ContactType { get; set; }
        public string ContactName { get; set; }
        public bool Status { get; set; }
        public string PrimaryEmail { get; set; }
        public string PrimaryPhone { get; set; }
        public string Lifecycle { get; set; }
        public string PhoneCountryCode { get; set; }
        public string PhoneExtension { get; set; }
    }

}
