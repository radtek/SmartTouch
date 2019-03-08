using AutoMapper;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using System.Collections.Generic;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Tests.Tours;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Accounts;

namespace SmartTouch.CRM.ApplicationServices.Tests.Tours
{
    [TestClass]
    public class TourServiceTest
    {
        #region Constants Declaration
        public const int USER_LOGGED_IN = 1;
        public const int QUARTER = 15;

        public const int DAY_OF_TOUR = 1;
        public const int ONE_DAY_BEFORE = 2;
        public const int TWO_DAYS_BEFORE = 3;
        public const int ON_SELECTED_DATE = 4;

        public const int ACTIVE_CONTACT_ID = 1;
        public const int DELETED_CONTACT_ID = 2;

        public const int LEGAL_TOUR_ID = 1;
       
        public const int NEW_TOUR_ID = 5;
        #endregion

        #region Declare TourService Contructor Elements
        MockRepository mockRepository;
        ITourService tourService;
        Mock<IUnitOfWork> mockUnitOfWork;
        Mock<ITourRepository> mockTourRepository;
        Mock<IContactRepository> mockContactRepository;
        Mock<IIndexingService> mockIndexingService;
        Mock<IServiceProviderRepository> mockServiceProviderRepository;
        Mock<IUserRepository> mockUserRepository;
        Mock<IMessageService> mockMessageService;
        Mock<ICachingService> mockCachingService;
        Mock<IOpportunityRepository> mockOpportunityRepository;
       // Mock<IAccountService> mockAccountService;
        Mock<IAccountRepository> mockAccountRepository;
        Mock<IUrlService> mockUrlService;
        
        #endregion

        #region TestInitialize
        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUnitOfWork = mockRepository.Create<IUnitOfWork>();
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockTourRepository = mockRepository.Create<ITourRepository>();
            mockIndexingService = mockRepository.Create<IIndexingService>();
            mockServiceProviderRepository = mockRepository.Create<IServiceProviderRepository>();
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockMessageService = mockRepository.Create<IMessageService>();
            mockCachingService = mockRepository.Create<ICachingService>();
            mockOpportunityRepository = mockRepository.Create<IOpportunityRepository>();
            //mockAccountService = mockRepository.Create<IAccountService>();
            mockAccountRepository = mockRepository.Create<IAccountRepository>();
            mockUrlService = mockRepository.Create<IUrlService>();

            tourService = new TourService(mockTourRepository.Object, 
                mockContactRepository.Object, mockUnitOfWork.Object, 
                mockIndexingService.Object, mockServiceProviderRepository.Object,mockUserRepository.Object,
                mockMessageService.Object, mockCachingService.Object, mockOpportunityRepository.Object, mockUrlService.Object, mockAccountRepository.Object);
        }
        #endregion

        #region TestCleanUp
        [TestCleanup]
        public void Cleanup()
        {
        }
        #endregion

        #region GetTours
        //[TestMethod]//Pending
        //public void GetTourList_ForAnExistingContact_SuccessfulRetrieval()
        //{
        //    var mockTours = TourMockData.GetMockTours(mockRepository, 5);
        //    mockTourRepository.Setup(tr => tr.FindByContact(It.IsAny<int[]>())).Returns(mockTours);

        //    GetTourListResponse response = tourService.GetTourList(new GetTourListRequest() { Id = LEGAL_TOUR_ID });

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(response.ToursListViewModel.Count(), mockTours.Count());
        //}

        [TestMethod]
        public void GetTour_ForAnExistingContact_TourObjectFound()
        {
            var mockTour = TourMockData.CreateMockTourWithId(mockRepository, ACTIVE_CONTACT_ID);
            mockTourRepository.Setup(tr => tr.GetTourByID(It.IsAny<int>())).Returns(mockTour.Object);
            GetTourResponse response = new GetTourResponse();

            response = tourService.GetTour(ACTIVE_CONTACT_ID);

            mockRepository.VerifyAll();
            Assert.AreEqual(mockTour.Object.Id, response.TourViewModel.TourID);
        }
        #endregion

        #region Insert Tour
        //[TestMethod]
        public void InsertTour_WithValidDetailsWithoutReminder_SuccessfullyInserted()
        {
            TourViewModel tourViewModel = TourMockData.GetTourViewModel();
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            mockUnitOfWork.Setup(tr => tr.Commit()).Returns(new Tour() { Id = NEW_TOUR_ID });
            InsertTourRequest request = new InsertTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 10, 2, 1, 5, new DateTime()).Object };
            InsertTourResponse response = new InsertTourResponse();
            //InsertTourResponse response = tourService.InsertTour(new InsertTourRequest() { TourViewModel = tourViewModel });
            //response = tourService.InsertTour(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithValidDetailsWithReminder_SuccessfullyInserted()
        {
            TourViewModel tourViewModel = TourMockData.GetTourViewModel();
            tourViewModel.ReminderDate = DateTime.Now.AddDays(1);
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            mockUnitOfWork.Setup(tr => tr.Commit()).Returns(new Tour() { Id = NEW_TOUR_ID });
            InsertTourRequest request = new InsertTourRequest()
            {
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 1, 2, new DateTime(2016, 1, 1), new List<ReminderType>() { ReminderType.Email }, new DateTime(2015, 6, 1)).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithValidDetailsWithFutureReminder_NotInserted()
        {
            TourViewModel tourViewModel = TourMockData.GetTourViewModel();
            tourViewModel.ReminderDate = new DateTime(14, 8, 14);
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            mockUnitOfWork.Setup(tr => tr.Commit()).Returns(new Tour() { Id = NEW_TOUR_ID});
            InsertTourRequest request = new InsertTourRequest()
            {
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 1, 2, new DateTime(2015, 1, 31), new List<ReminderType>() { ReminderType.Email }, new DateTime(2014, 12, 31)).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithoutTourType_NotInserted()
        {
            TourViewModel tourViewModel = TourMockData.GetTourViewModel();
            tourViewModel.TourType = 2;
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 0, 2, 0, 10, new DateTime()).Object };
            InsertTourResponse response = new InsertTourResponse();
            //InsertTourResponse response = tourService.InsertTour(new InsertTourRequest() { TourViewModel = actionViewModel });
            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithoutContacts_NotInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 0, 0, 3, 10, new DateTime()).Object };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithCurrentTourTimeWithReminder_NotInserted()  //Business Rule: Reminder cannot be set for past and current times.
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(), new List<ReminderType>() { ReminderType.Email }, new DateTime()).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithCurrentTourTimeWithoutReminder_SuccessfullyInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>()));
            mockUnitOfWork.Setup(tr => tr.Commit()).Returns(new Tour() { Id = NEW_TOUR_ID});
            InsertTourRequest request = new InsertTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 0, 2, 3, 10, new DateTime()).Object };
            InsertTourResponse response = new InsertTourResponse();
            response = tourService.InsertTour(request);
            //Assert.AreEqual(request.TourViewModel.ReminderDate, new DateTime(2015, 4, 1, 12, 0, 0));
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertTour_ReminderTypeSelectedWithReminderDate_SuccessfullyInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            mockUnitOfWork.Setup(tr => tr.Commit()).Returns(new Tour() { Id = NEW_TOUR_ID });
            InsertTourRequest request = new InsertTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(2015, 4, 3, 12, 0, 0),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime(2015, 4, 1, 12, 0, 0)).Object
            };
            InsertTourResponse response = new InsertTourResponse();
            response = tourService.InsertTour(request);
            Assert.AreEqual(request.TourViewModel.ReminderDate, new DateTime(2015, 4, 1, 12, 0, 0));
            //Assert.AreEqual(request.TourViewModel.TourID, NEW_TOUR_ID);
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertTour_ReminderTypeSelectedWithoutReminderDate_NotInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() {
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(2014, 4, 1), new List<ReminderType>() { ReminderType.Email }, null).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertTour_ReminderTypeSelectedWithPastReminderDate_NotInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(2014, 4, 1),
                new List<ReminderType>() { ReminderType.Email }, new DateTime(2014, 3, 8)).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertTour_WithoutCommunity_NotInserted()
        {
            mockTourRepository.Setup(tr => tr.Insert(It.IsAny<Tour>())).Verifiable("Unable to insert Tour");
            InsertTourRequest request = new InsertTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 1, 0, new DateTime(2015, 1, 1),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime()).Object
            };
            InsertTourResponse response = new InsertTourResponse();

            response = tourService.InsertTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }
        #endregion

        #region Update Tour
        [TestMethod]
        public void UpdateTour_WithValidDetailsWithoutReminder_SuccessfullyUpdated()
        {
            var mockTour = TourMockData.CreateMockTourWithId(mockRepository, 0);
            mockTourRepository.Setup(tr => tr.Update(mockTour.Object)).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 10, 2, 1, 5, new DateTime()).Object };
            UpdateTourResponse response = new UpdateTourResponse();

            request.TourViewModel.Contacts.Add(new ContactEntry() { Id = 10 });
            response = tourService.UpdateTour(request);

            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithValidDetailsWithValidReminder_SuccessfullyUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                1, 2, 1, 2, new DateTime(2015, 1, 1),
                new List<ReminderType>() { ReminderType.Email }, new DateTime(2014, 12, 31)).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithValidDetailsWithFutureReminder_NotUpdateed()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() {
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 1, 2, new DateTime(12, 1, 1),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime(2013, 1, 1)).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithoutTourType_NotUpdated()
        {
            var mockTour = TourMockData.CreateMockTourWithId(mockRepository, 1);
            mockTourRepository.Setup(tr => tr.Update(mockTour.Object)).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 0, 2, 0, 10, new DateTime()).Object };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithoutContacts_NotUpdated()
        {
            var mockTour = TourMockData.CreateMockTourWithId(mockRepository, 0);
            mockTourRepository.Setup(tr => tr.Update(mockTour.Object)).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 10, 2, 1, 5, new DateTime()).Object };
            UpdateTourResponse response = new UpdateTourResponse();

            request.TourViewModel.Contacts.Clear();
            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithCurrentTourTimeWithReminder_NotUpdated()  //Business Rule: Reminder cannot be set for past and current times.
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() {
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime()).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithCurrentTourTimeWithoutReminder_SuccessfullyUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository, 0, 2, 3, 10, new DateTime()).Object };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_ReminderTypeSelectedWithReminderDate_SuccessfullyUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(2015, 4, 1, 12, 0, 0),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime(2015, 4, 1, 11, 0, 0)).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_ReminderTypeSelectedWithoutReminderDate_NotUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(14, 4, 1),
                 new List<ReminderType>() { ReminderType.Email }, null).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_ReminderTypeSelectedWithPastReminderDate_NotUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 3, 10, new DateTime(2014, 4, 1),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime(2014, 3, 8)).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateTour_WithoutCommunity_NotUpdated()
        {
            mockTourRepository.Setup(tr => tr.Update(It.IsAny<Tour>())).Verifiable("Unable to update Tour");
            UpdateTourRequest request = new UpdateTourRequest() { 
                TourViewModel = TourMockData.CreateMockTourWithCustomValues(mockRepository,
                0, 2, 1, 0, new DateTime(2015, 1, 1),
                 new List<ReminderType>() { ReminderType.Email }, new DateTime()).Object
            };
            UpdateTourResponse response = new UpdateTourResponse();

            response = tourService.UpdateTour(request);

            Assert.AreNotEqual(null, response.Exception);
        }
        #endregion

        #region Delete Tour
        [TestMethod]
        public void DeleteTour_NoRuntimeExceptions_SuccessfullyDeleted()
        {
            mockTourRepository.Setup(tr => tr.DeleteTour(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Verifiable("Unable to delete Tour");
            DeleteTourResponse response = new DeleteTourResponse();

            response = tourService.DeleteTour(LEGAL_TOUR_ID, LEGAL_TOUR_ID, LEGAL_TOUR_ID);

            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void DeleteTour_RuntimeExceptions_NotDeleted()
        {
            mockTourRepository.Setup(tr => tr.DeleteTour(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).Throws(new InvalidOperationException());
            DeleteTourResponse response = new DeleteTourResponse();

            response = tourService.DeleteTour(LEGAL_TOUR_ID, LEGAL_TOUR_ID, LEGAL_TOUR_ID);

            Assert.AreNotEqual(null, response.Exception);
        }

        #endregion

        #region AutoMapper
        [TestMethod]
        public void TourAutoMap_ViewModelToEntity_SuccessfulMapping()
        {
            TourViewModel viewModel = new TourViewModel()
            {
                TourID = 1,
                CommunityID = 2,
                TourDetails = "Test"
            };

            Tour tour = Mapper.Map<TourViewModel, Tour>(viewModel);

            Assert.AreEqual(tour.Id, viewModel.TourID);
            Assert.AreEqual(tour.TourDetails, viewModel.TourDetails);
            Assert.AreEqual(tour.CommunityID, viewModel.CommunityID);
        }
        #endregion
    }
}
