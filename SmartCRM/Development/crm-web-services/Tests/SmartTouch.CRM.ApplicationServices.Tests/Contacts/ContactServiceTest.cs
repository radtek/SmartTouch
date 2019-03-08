using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Tests;
using Moq;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.ApplicationServices.Tests.Contacts
{
    [TestClass]
    public class ContactServiceTest
    {
        public const int ACCOUNT_ID = 5;
        public const int OWNER_ID = 3;
        public const int CONTACTID = 1;
        public const int USER_ID = 3; 
        public const short DROPDOWN_VALUE_ID = 1;
        //public const int[] CONTACT_IDS = new int[] {4, 5, 6 };

        MockRepository mockRepository;
        IContactService contactService;
        Mock<IImageService> imageService;
        Mock<IContactRepository> mockContactRepository;        
        Mock<ITagRepository> mockTagRepository;
        Mock<IUrlService> mockUrlService;
        Mock<ICustomFieldService> customFieldService;
        Mock<IIndexingService> mockIndexingService;
        Mock<ISearchService<Contact>> mockSearchService;
        Mock<IDropdownRepository> dropdownRepository;
       // Mock<IMessageQueuingService> queuingService;
        Mock<ICachingService> cachingService;
        Mock<IAccountService> accountService;
        Mock<IUserRepository> mockUserRepository;
        Mock<IFormRepository> mockFormRepository;
        Mock<IMailGunService> mailGunService;
        Mock<ITagService> tagService;
        Mock<IMessageService> mockMessageService;
        Mock<IAdvancedSearchRepository> advancedSearchRepository;
        Mock<ICommunicationService> communicationService;
        Mock<IUserService> userService;
        Mock<IServiceProviderRepository> serviceProvider;
        Mock<IFormSubmissionRepository> formSubmissionRepository; //NEXG-3014
        Mock<IFindSpamService> findSpamService; //NEXG-3014

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            IUnitOfWork mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockTagRepository = mockRepository.Create<ITagRepository>();
            mockUrlService = mockRepository.Create<IUrlService>();
            imageService = mockRepository.Create<IImageService>();
            customFieldService = mockRepository.Create<ICustomFieldService>();
            mockIndexingService = mockRepository.Create<IIndexingService>();
            mockSearchService = mockRepository.Create<ISearchService<Contact>>();
            cachingService = mockRepository.Create<ICachingService>();
            dropdownRepository = mockRepository.Create<IDropdownRepository>();
            accountService = mockRepository.Create<IAccountService>();
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockMessageService = mockRepository.Create<IMessageService>();
            mockFormRepository = mockRepository.Create<IFormRepository>();
            mailGunService = mockRepository.Create<IMailGunService>();
            tagService = mockRepository.Create<ITagService>();
            advancedSearchRepository = mockRepository.Create<IAdvancedSearchRepository>();
            communicationService = mockRepository.Create<ICommunicationService>();
            userService = mockRepository.Create<IUserService>();
            serviceProvider = mockRepository.Create<IServiceProviderRepository>();
            formSubmissionRepository = mockRepository.Create<IFormSubmissionRepository>(); //NEXG-3014
            findSpamService = mockRepository.Create<IFindSpamService>(); //NEXG-3014
            contactService = new ContactService(mockContactRepository.Object, 
                mockTagRepository.Object, mockUnitOfWork, mockUrlService.Object, 
                customFieldService.Object, cachingService.Object, 
                dropdownRepository.Object,  
                mockIndexingService.Object, mockSearchService.Object, 
                accountService.Object, mockUserRepository.Object,
                mockMessageService.Object, imageService.Object, mockFormRepository.Object, mailGunService.Object, advancedSearchRepository.Object, communicationService.Object, userService.Object, serviceProvider.Object
                ,formSubmissionRepository.Object //NEXG-3014
                , findSpamService.Object //NEXG-3014
                );
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void GetAllContacts_ValidPersonsAndCompanies_Succeed()
        {
            var mockContacts = ContactMockData.GetMockContacts(mockRepository, 10).ToList();
            mockContactRepository.Setup(cr => cr.FindAll(It.IsAny<string>())).Returns(mockContacts);
            //mockSearchService.Setup(c => c.Search("Adam", new SearchParameters() { AutoCompleteFieldName = "Adam"})).Returns();

            SearchContactsResponse<ContactListEntry> response = contactService.GetAllContacts<ContactListEntry>(new SearchContactsRequest() { Query = "" });
            var contacts = response.Contacts;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockContacts.Count, contacts.Count());
            Assert.AreEqual(null, response.Exception);
        }


        [TestMethod]
        public void GetAllContacts_RunTimeException_ExceptionHandled()
        {            
            SearchContactsResponse<ContactListEntry> response = contactService.GetAllContacts<ContactListEntry>(new SearchContactsRequest() { Query = "Adam" });
            Assert.AreEqual(null, response.Exception);

        }


        [TestMethod]
        public void GetPerson_ValidContact_Succeed()
        {
            var mockContact = ContactMockData.GetMockPersonsWithSetups(mockRepository, 1).FirstOrDefault();
            mockContactRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(mockContact.Object);
            mockTagRepository.Setup(c => c.FindByContact(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Tag>());

            GetPersonResponse response = contactService.GetPerson(new GetPersonRequest(1));
            int id = response.PersonViewModel.ContactID;

            mockRepository.VerifyAll();
            mockTagRepository.VerifyAll();
            Assert.AreEqual(mockContact.Object.Id, id);
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetUsers_ValidUsers_Succeed()
        {
            var mockUsers = ContactMockData.GetMockUsers(mockRepository, 5).ToList();
            mockContactRepository.Setup(c => c.GetUsers(ACCOUNT_ID, OWNER_ID, false)).Returns(mockUsers);

            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest() { AccountID = ACCOUNT_ID, UserId = USER_ID });
            var users = response.Owner;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockUsers.Count, users.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetUsers_ValidUsers_RaiseException()
        {
            var mockUsers = ContactMockData.GetMockUsers(mockRepository, 5).ToList();
            mockContactRepository.Setup(c => c.GetUsers(ACCOUNT_ID, OWNER_ID, false)).Throws(new NullReferenceException());

            GetUsersResponse response = contactService.GetUsers(new GetUsersRequest() { AccountID = ACCOUNT_ID, UserId = USER_ID });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        //[TestMethod]
        //public void ChangeOwner_ValidOwner_ReturnSuccess()
        //{
        //    var mockUsers = ContactMockData.GetMockUsers(mockRepository, 5).ToList();
        //    mockContactRepository.Setup(c => c.ChangeOwner(OWNER_ID, OWNER_STATUS))
        //}

        [TestMethod]
        public void DeleteContact_ContactDelete_ReturnSuccess()
        {
            mockContactRepository.Setup(a => a.FindBy(CONTACTID));
            DeleteContactResponse response = contactService.DeleteContact(CONTACTID);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        //[TestMethod]
        public void DeleteContact_ContactDelete_RaiseException()
        {
            mockContactRepository.Setup(a => a.FindBy(CONTACTID));
            DeleteContactResponse response = contactService.DeleteContact(CONTACTID);            
            mockRepository.VerifyAll();
            Assert.AreNotEqual(null, response.Exception);
        }

        //[TestMethod]
        //public void GetPerson_ValidPerson_ReturnSuccess()
        //{
        //    var mockContacts = ContactMockData.GetMockPersons(mockRepository, 5).ToList();
        //    var mockCustomFields = ContactMockData.GetContactField(mockRepository, 5).ToList();
        //    mockContactRepository.Setup(c => c.FindBy(CONTACTID)).Returns(mockContacts[0]);
        //    dropdownRepository.Setup(c => c.GetDropdownFieldValueBy(DROPDOWN_VALUE_ID)).Returns("HOME");
            
        //    GetContactCustomFieldsRequest request = new GetContactCustomFieldsRequest(CONTACTID);

        //    var mockCustomeFieldTabs = ContactMockData.GetCustomeFieldTabs(mockRepository, 1).ToList();
        //    GetAllCustomFieldTabsRequest customeFieldRequest = new GetAllCustomFieldTabsRequest(ACCOUNT_ID);           

        //    customFieldService.Setup(c => c.GetContactCustomFields(request).ContactCustomFields).Returns(mockCustomFields);

            
        //    customFieldService.Setup(c => c.GetAllCustomFieldTabs(customeFieldRequest).CustomFieldsViewModel).Returns(mockCustomeFieldTabs);
        //    GetPersonResponse response = contactService.GetPerson(new GetPersonRequest(CONTACTID));
        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(null, response.Exception);
        //    Assert.AreNotEqual(typeof(ArgumentNullException), response.PersonViewModel);
        //}

        [TestMethod]
        public void DeactivateContacts_ContactsoftDelete_ReturnSuccess()
        {
            //var mockContacts = ContactMockData.GetMockContacts(mockRepository, 10).ToList();
            mockContactRepository.Setup(a => a.DeactivateContact(CONTACTID, USER_ID, ACCOUNT_ID));
            mockIndexingService.Setup(c => c.RemoveContact(CONTACTID, ACCOUNT_ID));
            DeactivateContactRequest request = new DeactivateContactRequest(CONTACTID);
            DeactivateContactResponse response = contactService.Deactivate(new DeactivateContactRequest(CONTACTID));
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
            Assert.AreNotEqual(typeof(ArgumentNullException), response.Exception);
        }

        [TestMethod]
        public void DeactivateContacts_ContactsoftDelete_RaiseException()
        {
            //var mockContacts = ContactMockData.GetMockContacts(mockRepository, 10).ToList();
            //mockContactRepository.Setup(a => a.DeactivateContact(CONTACTID)).th;
            DeactivateContactRequest request = new DeactivateContactRequest(CONTACTID);
            DeactivateContactResponse response = contactService.Deactivate(new DeactivateContactRequest(CONTACTID));
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception);
            Assert.AreNotEqual(null, response.Exception);
        }











































        /*
       [TestMethod]
       public void GetContact_ContactNotFound_ResourceNotFoundException()
       {
           Contact contact = null;
           mockContactRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(contact);

           GetContactResponse response = contactService.GetContact(new GetPersonRequest(1));

           mockRepository.VerifyAll();
           Assert.AreEqual(typeof(ResourceNotFoundException), response.Exception.GetType());
       }
        * */
    }
}
