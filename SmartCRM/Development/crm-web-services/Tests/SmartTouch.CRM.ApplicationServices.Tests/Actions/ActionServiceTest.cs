using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using DA = SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Tests;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Indexing;

using Moq;
using AutoMapper;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Accounts;

namespace SmartTouch.CRM.ApplicationServices.Tests.Action
{
    [TestClass]
    public class ActionServiceTest
    {
        public const int ACTION_ID = 1;
        public const int CONTACT_ID = 1;
        public const int NEW_ACTION_ID = 3;
        MockRepository mockRepository;
        IActionService actionService;
        Mock<IActionRepository> mockActionRepository;
        Mock<IUnitOfWork> mockUnitOfWork;
        Mock<ITagRepository> mockTagRepository;
        Mock<IUserRepository> mockUserRepository;
        Mock<IServiceProviderRepository> mockserviceProviderRepo;
        Mock<IIndexingService> mockIndexingService;
        Mock<IMessageService> mockMessageService;
        Mock<IContactRepository> mockContactRepository;
        Mock<IAccountService> mockAccountService;
        Mock<IUrlService> mockUrlService;
        Mock<IAccountRepository> mockAccountRepository;
        Mock<IContactService> mockContactService;
        Mock<ICachingService> mockCacheService;
        Mock<INoteService> mockNoteService;
        //[MethodUnderTest]_[StateUnderTest]_[ExpectedBehavior]  Naming Testmethods  
        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUnitOfWork = mockRepository.Create<IUnitOfWork>();
            mockActionRepository = mockRepository.Create<IActionRepository>();
            mockTagRepository = mockRepository.Create<ITagRepository>();
            mockIndexingService = mockRepository.Create<IIndexingService>();
            mockserviceProviderRepo = mockRepository.Create<IServiceProviderRepository>();
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockMessageService = mockRepository.Create<IMessageService>();
            mockAccountService = mockRepository.Create<IAccountService>();
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockUrlService = mockRepository.Create<IUrlService>();
            mockAccountRepository = mockRepository.Create<IAccountRepository>();
            mockContactService = mockRepository.Create<IContactService>();
            mockCacheService = mockRepository.Create<ICachingService>();
            mockNoteService = mockRepository.Create<INoteService>();
            actionService = new ActionService(mockActionRepository.Object, mockTagRepository.Object,
                mockUnitOfWork.Object, mockIndexingService.Object,
                mockserviceProviderRepo.Object, mockUserRepository.Object, mockMessageService.Object,
                mockContactRepository.Object, mockUrlService.Object, mockAccountService.Object, mockAccountRepository.Object, mockContactService.Object, mockCacheService.Object, mockNoteService.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void GetContactActions_ValidRequest_Succeed()
        {
            var mockActions = ActionMockData.GetMockActions(mockRepository, 10).ToList();
            mockActionRepository.Setup(cr => cr.FindByContact(It.IsAny<int>())).Returns(mockActions);

            GetActionListResponse response = actionService.GetContactActions(new GetActionListRequest() { Id = ACTION_ID });
            IEnumerable<ActionViewModel> actions = response.ActionListViewModel;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockActions.Count(), actions.Count());
            Assert.AreEqual(null, response.Exception);
        }      //Problem with automapper if we use strict mock 

        [TestMethod]
        public void GetAction_ValidRequest_Succeed()
        {
            var mockActions = ActionMockData.GetMockActions(mockRepository, 10).ToList();
            mockActionRepository.Setup(cr => cr.FindBy(It.IsAny<int>(), It.IsAny<int>())).Returns(mockActions[0]);

            GetActionResponse response = actionService.GetAction(new GetActionRequest() { Id = ACTION_ID, ContactId = CONTACT_ID });
            ActionViewModel action = response.ActionViewModel;
            mockRepository.VerifyAll();
            Assert.AreEqual(0, action.ActionId);
            Assert.AreEqual(null, response.Exception);
        }         //Problem with automapper if we use strict mock 

        [TestMethod]
        public void GetActions_RunTimeException_ExceptionDetails()
        {
            mockActionRepository.Setup(cr => cr.FindBy(It.IsAny<int>(), It.IsAny<int>())).Throws(new InvalidOperationException());

            GetActionResponse response = actionService.GetAction(new GetActionRequest() { Id = ACTION_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertAction_ValidAction_Succeed()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.Insert(new DA.Action())).Verifiable("Unable to insert Action");
            mockUnitOfWork.Setup(a => a.Commit()).Returns(new DA.Action() { Id = NEW_ACTION_ID});
            InsertActionResponse response = actionService.InsertAction(new InsertActionRequest() { ActionViewModel = actionViewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void InsertAction_InValidAction_Exception()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.Insert(new DA.Action())).Throws(new ArgumentNullException());
            InsertActionResponse response = actionService.InsertAction(new InsertActionRequest() { ActionViewModel = actionViewModel });
            //response.ActionViewModel.Contacts = null;
            mockRepository.Verify();
            Assert.AreNotEqual(null, response.Exception);
            Assert.AreEqual(typeof(ArgumentNullException), response.Exception.GetType());
        }

        [TestMethod]
        public void InsertAction_RunTimeException_ExceptionDetails()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.Insert(new DA.Action())).Throws(new NullReferenceException());
            InsertActionResponse response = actionService.InsertAction(new InsertActionRequest() { ActionViewModel = actionViewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void DeleteAction_ValidAction_Succeed()
        {
            mockActionRepository.Setup(a => a.DeleteActionForAll(It.IsAny<int>(), It.IsAny<int>())).Verifiable("Unable to delete Action");
            DeleteActionResponse response = actionService.DeleteAction(new DeleteActionRequest() { ActionId = ACTION_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateAction_ValidAction_Succeed()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.Update(new DA.Action())).Verifiable("Unable to delete Action");
            UpdateActionResponse response = actionService.UpdateAction(new UpdateActionRequest() { ActionViewModel = actionViewModel });
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void UpdateAction_InValidAction_BrokenRules()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.Update(new DA.Action())).Throws(new NullReferenceException());
            UpdateActionResponse response = actionService.UpdateAction(new UpdateActionRequest() { ActionViewModel = actionViewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void ActionStatus_Completed_Succeed()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.ActionCompleted(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<bool>(), It.IsAny<int>())).Verifiable("Unable to call Actioncompleted method");
            CompletedActionResponse response = actionService.ActionStatus(new CompletedActionRequest() { actionId = ACTION_ID, contactId = CONTACT_ID, isCompleted = true });
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void ActionContactsCount_WithContacts_Succeed()
        {
            ActionViewModel actionViewModel = ActionMockData.GetActionViewModel();
            mockActionRepository.Setup(a => a.ContactsCount(It.IsAny<int>())).Returns(It.IsAny<int>());
            GetContactsCountResponse response = actionService.ActionContactsCount(new GetContactsCountRequest() { Id = ACTION_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(It.IsAny<int>(), response.Count);
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void ActionAutoMap_ViewModelToEntity_SuccessfulMap()
        {
            ActionViewModel viewModel = ActionMockData.GetActionViewModel();
            DA.Action action = Mapper.Map<ActionViewModel, DA.Action>(viewModel);

            Assert.AreEqual(action.Id, viewModel.ActionId);
            Assert.AreEqual(action.Details, viewModel.ActionMessage);
            Assert.AreEqual(action.Contacts.Count, viewModel.Contacts.Count());
        }

        [TestMethod]
        public void ActionAutoMap_EntityToViewModel_SuccessfulMap()
        {
            //DA.Action action = new DA.Action() { Id = ACTION_ID, Details = "Sample Action", IsCompleted = true, Contacts = ContactMockData.GetMockContacts(mockRepository, 2).ToList() };
            //ActionViewModel viewModel = Mapper.Map<DA.Action, ActionViewModel>(action);

            //Assert.AreEqual(viewModel.ActionId, action.Id);
            //Assert.AreEqual(viewModel.ActionMessage, action.Details);
            //Assert.AreEqual(viewModel.Contacts.Count(), action.Contacts.Count);
        }
    }
}
