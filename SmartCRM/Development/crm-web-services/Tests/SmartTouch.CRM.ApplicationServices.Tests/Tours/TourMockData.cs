using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.Domain.Tours;

using Moq;
using Moq.Linq;
using TestCT=SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using CT=SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Tests.Tours
{
    public class TourMockData
    {
        public static IEnumerable<Tour> GetMockTours(MockRepository mockRepository, int objectCount)
        {
            IList<Tour> mockTours = new List<Tour>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTour = mockRepository.Create<Tour>();
                mockTours.Add(mockTour.Object);
            }
            return mockTours;
        }

        public static Mock<Tour> CreateMockTourWithId(MockRepository mockRepository, int Id)
        {
            var mockTour = mockRepository.Create<Tour>();
            mockTour.Setup(mt => mt.Id).Returns(Id);
            mockTour.Setup<ICollection<CT.Contact>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContacts(mockRepository, 2).ToList());
            mockTour.Setup(mt => mt.TourType).Returns(3);
            mockTour.Setup(mt => mt.CommunityID).Returns(1);
            mockTour.Setup(mt => mt.TourDate).Returns(new DateTime());
            return mockTour;
        }


        public static IList<Tour> GetMockToursWithId(MockRepository mockRepository, int objectCount)
        {
            IList<Tour> mockTours= new List<Tour>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTour = mockRepository.Create<Tour>();
                mockTour.Setup(mt => mt.Id).Returns(i);
                mockTour.Setup<ICollection<CT.Contact>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContacts(mockRepository, 2).ToList());
                mockTour.Setup(mt => mt.TourType).Returns(3);
                mockTour.Setup(mt => mt.CommunityID).Returns(1);
                mockTour.Setup(mt => mt.TourDate).Returns(new DateTime());
                mockTours.Add(mockTour.Object);
            }
            return mockTours;
        }

        public static IEnumerable<Mock<Tour>> GetMockToursWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<Tour>> mockTours = new List<Mock<Tour>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockTour = mockRepository.Create<Tour>();
                mockTour.Setup<int>(c => c.Id).Returns(i);
                mockTour.Setup<ICollection<CT.Contact>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContacts(mockRepository, 2).ToList());
                mockTour.Setup(mt => mt.TourType).Returns(3);
                mockTour.Setup(mt => mt.CommunityID).Returns(1);
                mockTour.Setup(mt => mt.TourDate).Returns(new DateTime());
                mockTours.Add(mockTour);
            }
            return mockTours;
        }

        public static Mock<TourViewModel> GetMockTourViewModelWithId(MockRepository mockRepository, int id)
        {
            var mockTourViewModel = mockRepository.Create<TourViewModel>();
            mockTourViewModel.Setup(mt => mt.TourID).Returns(id);
            mockTourViewModel.Setup<ICollection<ContactEntry>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContactEntrys(mockRepository, 2).ToList());
            mockTourViewModel.Setup(mt => mt.TourType).Returns(1);
            mockTourViewModel.Setup(mt => mt.CommunityID).Returns(1);
            mockTourViewModel.Setup(mt => mt.TourDate).Returns(new DateTime());
            return mockTourViewModel;
        }

        /// <summary>
        /// Returns a new TourViewModel based on the values passed.
        /// </summary>
        /// <param name="mockRepository"></param>
        /// <param name="tourId">Business rule: New = 0, Existing > 0</param>
        /// <param name="contactsCount">Business rule: Count > 0</param>
        /// <param name="tourType">Business rule: Mandatory</param>
        /// <param name="communityId">Business rule: Mandatory</param>
        /// <param name="tourDate">Business rule: Mandatory</param>
        /// <returns></returns>
        public static Mock<TourViewModel> CreateMockTourWithCustomValues(MockRepository mockRepository, int tourId,int contactsCount, short tourType, short communityId, DateTime tourDate)
        {
            var mockTourViewModel = mockRepository.Create<TourViewModel>();
            mockTourViewModel.Setup(mt => mt.TourID).Returns(tourId);
            mockTourViewModel.Setup<ICollection<ContactEntry>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContactEntrys(mockRepository, contactsCount).ToList());
            mockTourViewModel.Setup(mt => mt.TourType).Returns(tourType);
            mockTourViewModel.Setup(mt => mt.CommunityID).Returns(communityId);
            mockTourViewModel.Setup(mt => mt.TourDate).Returns(tourDate);
            return mockTourViewModel;
        }

        public static TourViewModel GetTourViewModel()
        {
            ICollection<ContactEntry> contact = new List<ContactEntry>() { new ContactEntry() { Id = 1, FullName = "Unit-test" } };
            IEnumerable<TagViewModel> tag = new List<TagViewModel>() { new TagViewModel() { TagID = 1001 } };
            TourViewModel tourViewModel = new TourViewModel();
            tourViewModel.TourDetails = "Sample";
            tourViewModel.Contacts = contact;
            tourViewModel.CommunityID = 1;
            tourViewModel.TourDate = new DateTime(14, 8, 15);
            tourViewModel.ReminderTypes = new List<dynamic>() { ReminderType.Email };
            return tourViewModel;
        }

        /// <summary>
        /// Returns a new <i>TourViewModel</i> with reminder based on the values passed.
        /// </summary>
        /// <param name="mockRepository"></param>
        /// <param name="tourId">Business rule: New = 0, Existing > 0</param>
        /// <param name="contactsCount">Business rule: Count > 0</param>
        /// <param name="tourType">Business rule: Mandatory</param>
        /// <param name="communityId">Business rule: Mandatory</param>
        /// <param name="tourDate">Business rule: Mandatory</param>
        /// <param name="reminderType">Business rule: Mandatory</param>
        /// <param name="reminderDate">Business rule: Mandatory</param>
        /// <returns></returns>
        public static Mock<TourViewModel> CreateMockTourWithCustomValues(MockRepository mockRepository, int tourId, int contactsCount, short tourType, short communityId, DateTime tourDate,IEnumerable<ReminderType> reminderTypes,DateTime? reminderDate)
        {
            var mockTourViewModel = mockRepository.Create<TourViewModel>();
            mockTourViewModel.Setup(mt => mt.TourID).Returns(tourId);
            mockTourViewModel.Setup<ICollection<ContactEntry>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContactEntrys(mockRepository, contactsCount).ToList());
            mockTourViewModel.Setup(mt => mt.TourType).Returns(tourType);
            mockTourViewModel.Setup(mt => mt.CommunityID).Returns(communityId);
            mockTourViewModel.Setup(mt => mt.TourDate).Returns(tourDate);
            mockTourViewModel.Setup(mt => mt.SelectedReminderTypes).Returns(reminderTypes);
            mockTourViewModel.Setup(mt => mt.ReminderDate).Returns(reminderDate);
            return mockTourViewModel;
        }
    }
}
