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
using TestCT = SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using CT = SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Tests.Tours
{
    public class TourData
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

        public static Tour CreateMockTourWithId(int Id)
        {
            var tour = new Tour();
            tour.Id = Id;

            return tour;
        }


        public static IList<Tour> GetMockToursWithId(MockRepository mockRepository, int objectCount)
        {
            IList<Tour> mockTours = new List<Tour>();
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
            //mockTourViewModel.Setup(mt => mt.TourType).Returns(TourType.Agent);
            mockTourViewModel.Setup(mt => mt.CommunityID).Returns(1);
            mockTourViewModel.Setup(mt => mt.TourDate).Returns(new DateTime());
            return mockTourViewModel;
        }

        public static Mock<TourViewModel> CreateMockTourWithCustomValues(MockRepository mockRepository, int id, int contactCount, short tourType, short communityId, DateTime tourDate)
        {
            var mockTourViewModel = mockRepository.Create<TourViewModel>();
            mockTourViewModel.Setup(mt => mt.TourID).Returns(id);
            mockTourViewModel.Setup<ICollection<ContactEntry>>(c => c.Contacts).Returns(TestCT.ContactMockData.GetMockContactEntrys(mockRepository, contactCount).ToList());
            //mockTourViewModel.Setup(mt => mt.TourType).Returns(tourType);
            mockTourViewModel.Setup(mt => mt.CommunityID).Returns(communityId);
            mockTourViewModel.Setup(mt => mt.TourDate).Returns(tourDate);
            return mockTourViewModel;
        }
    }
}
