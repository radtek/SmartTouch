using System;
using System.Collections.Generic;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Tour
{
    public class Tour : EntityBase<int>, IAggregateRoot
    {
        string tourDetails;
        public string TourDetails { get { return tourDetails; } set { tourDetails = !string.IsNullOrEmpty(value)?value.Trim():null;  } }
        public virtual int CommunityID { get; set; }
        public virtual DateTime TourDate { get; set; }
        public virtual TourType TourType { get; set; }
        public ReminderType ReminderType { get; set; }
        public Nullable<DateTime> ReminderDate { get; set; }
        public virtual ICollection<Contact.Contact> Contacts { get; set; }


        protected override void Validate()
        {
            if (!IsTourDetailsValid())
            {
                AddBrokenRule(TourBusinessRule.DetailsMaxLengthReached);
            }
            
            if (!IsCommunityValid())
            {
                AddBrokenRule(TourBusinessRule.CommunityRequired);
            }

            if (!IsTourTypeValid())
            {
                AddBrokenRule(TourBusinessRule.TourTypeRequired);
            }
            
            if (!IsContactsCountValid())
            {
                AddBrokenRule(TourBusinessRule.ContactsRequired);
            }

            if (!IsReminderTypeValid())
            {
                AddBrokenRule(TourBusinessRule.ReminderNotApplicable);
            }

            if (!IsReminderDateValid())
            {
                AddBrokenRule(TourBusinessRule.ReminderDateNotValid);
            }

            if (!IsTourDateValid())
            {
                AddBrokenRule(TourBusinessRule.TourDateNotValid);
            }
        }

        public bool IsTourDetailsValid()
        {
            return (!string.IsNullOrEmpty(TourDetails) && TourDetails.Length > 1000) ? false : true;
        }

        public bool IsCommunityValid()
        {
            return CommunityID == 0 ? false : true;
        }

        public bool IsTourTypeValid()
        {
            return TourType == 0 ? false : true;
        }

        public bool IsContactsCountValid()
        {
            return Contacts == null || Contacts.Count == 0 ? false : true;
        }

        public bool IsReminderTypeValid()
        {
            return ReminderType != 0 && TourDate <= DateTime.Now ? false : true;
        }

        public bool IsReminderDateValid()
        {
            return ReminderType != 0 && (ReminderDate == new DateTime(1970, 1, 1) ||ReminderDate > TourDate|| ReminderDate < DateTime.Now || ReminderDate == null) ? false : true;
        }

        public bool IsTourDateValid()
        {
            return TourDate == null || TourDate == new DateTime(1970, 1, 1) ? false : true;
        }
    }
}
