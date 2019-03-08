using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;
using DT = SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Tests.Tour
{
    public class TourData
    {
        public DT.Tour CreateTourWithCustomValues(int tourId, int contactsCount, short tourType, short communityId,
            DateTime tourDate, ReminderType reminderType, DateTime? reminderDate)
        {
            DT.Tour tour = new DT.Tour();
            tour.Id = tourId;
            tour.Contacts = new List<Contacts.Contact>();
            for (int i = 0; i < contactsCount; i++)
            {
                Person person = new Person();
                person.Id = i;
                person.FirstName = "FN" + i;
                person.LastName = "LN";
                tour.Contacts.Add(person);
            }
            tour.CommunityID = communityId;
            tour.TourDate = tourDate;
            tour.ReminderTypes = new List<ReminderType>() { reminderType };
            tour.ReminderDate = reminderDate;

            return tour;
        }

        public IList<Contacts.Contact> CreateContactsList(int contactsCount)
        {
            IList<Contacts.Contact> contacts = new List<Contacts.Contact>();
            for (int i = 0; i < contactsCount; i++)
            {
                Person person = new Person();
                person.Id = i;
                person.FirstName = "FN" + i;
                person.LastName = "LN";
                contacts.Add(person);
            }
            return contacts;
        }
    }
}
