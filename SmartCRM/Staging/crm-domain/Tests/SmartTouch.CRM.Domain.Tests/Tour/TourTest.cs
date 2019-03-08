using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using Moq;
using DT = SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Tests.Tour;
namespace SmartTouch.CRM.Domain.Tests
{
    [TestClass]
    public class TourTest
    {
        #region Constants Declaration
        public const int USER_LOGGED_IN = 1;
        public const int QUARTER = 15;

        public const int DAY_OF_TOUR = 1;
        public const int ONE_DAY_BEFORE = 2;
        public const int TWO_DAYS_BEFORE = 3;
        public const int ON_SELECTED_DATE = 4;

        public const int NO_REMINDER = 0;
        public const int EMAIL = 1;
        public const int POP_UP = 2;
        public const int TEXT_MESSAGE = 4;
        #endregion

        #region Initialization
        public TourData tourdata = new TourData();

        [TestInitialize]
        public void Initialize()
        {

        }
        #endregion

        #region Tests

        #region TourDetails
        [TestMethod]
        public void IsTourDetailsValid_DetailsLengthIs1000_ValidTourDetails()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDetails = new string('a', 1000);
            Assert.IsTrue(tour.IsTourDetailsValid());
        }

        [TestMethod]
        public void IsTourDetailsValid_DetailsIsEmpty_ValidTourDetails()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDetails = null;
            Assert.IsTrue(tour.IsTourDetailsValid());
        }

        [TestMethod]
        public void IsTourDetailsValid_DetailsLengthGreaterThan1000_InvalidTourDetailsException()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDetails = new string('a', 1001);
            Assert.IsFalse(tour.IsTourDetailsValid());
        }
        #endregion

        #region Community
        [TestMethod]
        public void IsCommunityValid_CommunityIDEqualsZero_InvalidCommunityException()
        {
            DT.Tour tour = new DT.Tour();
            tour.CommunityID = 0;
            Assert.IsFalse(tour.IsCommunityValid());
        }

        [TestMethod]
        public void IsCommunityValid_CommunityIdGreaterThanZero_ValidCommunity()
        {
            DT.Tour tour = new DT.Tour();
            tour.CommunityID = 1;
            Assert.IsTrue(tour.IsCommunityValid());
        }
        #endregion

        #region Contacts
        [TestMethod]
        public void IsContactsCountValid_ContactsCountMoreThanZero_ValidContacts()
        {
            DT.Tour tour = new DT.Tour();
            tour.Contacts = tourdata.CreateContactsList(2);
            Assert.IsTrue(tour.IsContactsCountValid());
        }

        [TestMethod]
        public void IsContactsCountValid_ContactsNull_AtleastOneContactRequiredException()
        {
            DT.Tour tour = new DT.Tour();
            tour.Contacts = null;
            Assert.IsFalse(tour.IsContactsCountValid());
        }

        [TestMethod]
        public void IsContactsCountValid_ContactsCountZero_AtleastOneContactRequiredException()
        {
            DT.Tour tour = new DT.Tour();
            tour.Contacts = tourdata.CreateContactsList(0);
            Assert.IsFalse(tour.IsContactsCountValid());
        }


        #endregion

        #region TourType
        [TestMethod]
        public void IsTourTypeValid_TourTypeIsNotZero_ValidTourType()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourType = 3;
            Assert.IsTrue(tour.IsTourTypeValid());
        }

        [TestMethod]
        public void IsTourTypeValid_TourTypeIsZero_ValidContacts()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourType = 0;
            Assert.IsFalse(tour.IsTourTypeValid());
        }
        #endregion

        #region ReminderType
        [TestMethod]
        public void IsReminderTypeValid_TourDateIsCurrentTime_ReminderNotApplicable()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDate = DateTime.Now;
            tour.ReminderTypes = new List<ReminderType>() { ReminderType.Email };
            Assert.IsFalse(tour.IsReminderTypeValid());
        }
        #endregion

        #region ReminderDate
        [TestMethod]
        public void IsReminderDateValid_ReminderDateIsPastTime_InvalidReminder()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDate = new DateTime(2015, 1, 1);
            tour.ReminderTypes = new List<ReminderType>(){ ReminderType.Email};
            tour.ReminderDate = new DateTime(2014, 1, 1);
            Assert.IsFalse(tour.IsReminderDateValid());
        }

        public void IsReminderDate_ReminderDateIsCurrentTime_InvalidReminder()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDate = new DateTime(2015, 1, 1);
            tour.ReminderDate = DateTime.Now;
            Assert.IsFalse(tour.IsReminderDateValid());
        }

        public void IsReminderDate_ReminderDateIsFutureDateBeforeTourDate_ValidReminder()
        {
            DT.Tour tour = new DT.Tour();
            tour.TourDate = new DateTime(2015, 1, 1);
            tour.ReminderDate = new DateTime(2014, 12, 31);
            Assert.IsTrue(tour.IsReminderDateValid());
        }
        #endregion
        #endregion
    }
}
