using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Contacts
{
    [TestClass]
    public class ContactsControllerTests : ControllerTestBase
    {
        Mock<IContactService> mockContactService;
        Mock<IActionService> mockActionService;
        Mock<ICommunicationService> mockCommunicationservice;
        Mock<IContactRelationshipService> mockContactRelationshipService;
        Mock<IDropdownValuesService> mockdropdownValuesService;
        Mock<IUserService> mockuserService;
        MockRepository mockRepository;

        public const int SAMPLE_PERSON_ID = 5;
        public const int SAMPLE_COMPANY_ID = 5;
        public const int SAMPLE_CONTACT_ID = 4;
        public const int SAMPLE_ACTION_ID = 4;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockService = mockRepository.Create<IContactService>();

            mockContactService = mockService;
            mockActionService = mockRepository.Create<IActionService>();
            mockCommunicationservice = mockRepository.Create<ICommunicationService>();
            mockContactRelationshipService = mockRepository.Create<IContactRelationshipService>();
            mockdropdownValuesService = mockRepository.Create<IDropdownValuesService>();
            mockuserService = mockRepository.Create<IUserService>();
        }

        [TestCleanup]
        public void Cleanup()
        {            
        }
        /// <summary>
        /// Search All contacts/All Persons/ All company TestMethod
        /// </summary>
        [TestMethod]
        public void Search_GetAllContacts_ValidContacts()
        {
            //Arrange
            ContactsController controller = new ContactsController(mockContactService.Object, 
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);

            SearchContactsResponse<ContactListEntry> response = mockRepository.Create<SearchContactsResponse<ContactListEntry>>().Object;
            response.Contacts = MockData.CreateMockList<ContactListEntry>(mockRepository).Select(c => c.Object).ToList();
            mockContactService.Setup(cs => cs.GetAllContacts <ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(response);

            //Act
            var httpResponseMessage = controller.Search("temp", 0, 1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync < SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t=>{return t.Result;}).Result;
            
            //Assert
            mockRepository.VerifyAll();
            Assert.AreEqual(10, contactsResponse.Contacts.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactsResponse.Exception, null);
        }

        [TestMethod]        
        public void Search_GetAllContactsRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object, mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<SearchContactsResponse<ContactListEntry>>();
            mockContactService.Setup(cs => cs.GetAllContacts<ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.Search("temp",0,1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync<SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactsResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }

        [TestMethod]
        public void SearchCompanyNames_SearchCompanybyName_ReturnExistingCompany()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
               mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts/5", HttpMethod.Get);

             var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();
            AutoCompleteSearchResponse response = mockRepository.Create<AutoCompleteSearchResponse>().Object;
            response.Results = MockSuggestionData.GetMockSuggestions(10);
              mockContactService.Setup(cs => cs.SearchCompanyByName(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
          //  mockContactService.Setup(cs=>cs.SearchCompanyByName(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.)

            var httpResponseMessage = controller.SearchCompanyNames("suggest", 1);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(10, response.Results.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }

        [TestMethod]
        public void SearchCompanyNames_SearchCompanybyNameRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();
            mockContactService.Setup(cs => cs.SearchCompanyByName(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.SearchCompanyNames("suggest", 1);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);

        }
        [TestMethod]
        public void SearchCompanyNames_SearchCompanybyNameNotFound_404ResourceNotFoundError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();

            mockContactService.Setup(cs => cs.SearchCompanyByName(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new ResourceNotFoundException());

            var httpResponseMessage = controller.SearchCompanyNames("suggest", 1);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();


            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
            Assert.AreNotEqual(exception, null);
        }
        [TestMethod]
        public void SearchContactTitles_SearchContactTitelsbyTitleorPerson_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();
            AutoCompleteSearchResponse response = mockRepository.Create<AutoCompleteSearchResponse>().Object;
            response.Results = MockSuggestionData.GetMockSuggestions(10);
            mockContactService.Setup(cs => cs.SearchContactTitles(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
            //  mockContactService.Setup(cs=>cs.SearchCompanyByName(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.)

            var httpResponseMessage = controller.SearchContactTitles("suggest");
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(10, response.Results.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);

        }
        [TestMethod]
        public void SearchContactTitles_SearchContactTitelsbyTitleorPersonRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
            mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();
            mockContactService.Setup(cs => cs.SearchContactTitles(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.SearchContactTitles("suggest");
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);

        }
        [TestMethod]
        public void SearchContactTitles_SearchContactTitelsbyTitleorPersonNotFound_404()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<AutoCompleteSearchResponse>();

            mockContactService.Setup(cs => cs.SearchContactTitles(It.IsAny<AutoCompleteSearchRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new ResourceNotFoundException());

            var httpResponseMessage = controller.SearchContactTitles("suggest");
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<AutoCompleteSearchResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();


            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
            Assert.AreNotEqual(exception, null);
        }

        [TestMethod]
        public void SearchPersons_GetAllPersons_ValidPersons()
        {

            ContactsController controller = new ContactsController(mockContactService.Object,
               mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);

            SearchContactsResponse<ContactListEntry> response = mockRepository.Create < SearchContactsResponse<ContactListEntry>>().Object;
            response.Contacts = MockData.CreateMockList<ContactListEntry>(mockRepository).Select(c => c.Object).ToList();
            mockContactService.Setup(cs => cs.GetPersons < ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(response);

            var httpResponseMessage = controller.SearchPersons("temp", 0, 1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync < SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(10, contactsResponse.Contacts.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactsResponse.Exception, null);

        }
        [TestMethod]
        public void SearchPersons_GetAllpersonsRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object, mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<SearchContactsResponse<ContactListEntry>>();
            mockContactService.Setup(cs => cs.GetPersons<ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.SearchPersons("temp", 0, 1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync<SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactsResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }
        [TestMethod]
        public void SearchCompanies_GetAllCompanies_ValidCompanies()
        {

            ContactsController controller = new ContactsController(mockContactService.Object,
               mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);

            SearchContactsResponse<ContactListEntry> response = mockRepository.Create < SearchContactsResponse<ContactListEntry>>().Object;
            response.Contacts = MockData.CreateMockList<ContactListEntry>(mockRepository).Select(c => c.Object).ToList();
            mockContactService.Setup(cs => cs.GetCompanies<ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(response);

            var httpResponseMessage = controller.SearchCompanies("temp", 0, 1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync < SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(10, contactsResponse.Contacts.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactsResponse.Exception, null);
        }
        [TestMethod]
        public void SearchCompanies_GetAllCompaniesRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object, mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<SearchContactsResponse<ContactListEntry>>();
            mockContactService.Setup(cs => cs.GetCompanies<ContactListEntry>(It.IsAny<SearchContactsRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.SearchCompanies("temp", 0, 1);
            var contactsResponse = httpResponseMessage.Content.ReadAsAsync<SearchContactsResponse<ContactListEntry>>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactsResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }


        /// <summary>
        /// Get Person/contact method
        /// </summary>
        [TestMethod]
        public void GetPerson_SearchPersonById_ReturnsExistingPerson()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
            mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts/5", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetPersonResponse>();
            // mockResponse.Object.Contact = MockData.CreateMockList<IContactViewModel>(mockRepository).SingleOrDefault(c => c.Object.Id == 5).Object;
            mockResponse.Object.PersonViewModel = ContactViewModelMockData
                .GetMockPersonWithSetups(mockRepository).SingleOrDefault(c => c.Object.ContactID == SAMPLE_PERSON_ID).Object;

            mockContactService.Setup(c => c.GetPerson(It.IsAny<GetPersonRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.GetPerson(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetPersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(SAMPLE_PERSON_ID, contactResponse.PersonViewModel.ContactID);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }
        
        [TestMethod]
        public void GetPerson_SearchPersonbyIdRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetPersonResponse>();
            mockContactService.Setup(cs => cs.GetPerson(It.IsAny<GetPersonRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetPerson(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetPersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;
            
            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }

        [TestMethod]
        public void GetPerson_SearchPersonByIdPersonNotFound_404ResourceNotFoundError()
        {
           ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetPersonResponse>();

            mockContactService.Setup(cs => cs.GetPerson(It.IsAny<GetPersonRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new ResourceNotFoundException());

            var httpResponseMessage = controller.GetPerson(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetPersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();


            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
            Assert.AreNotEqual(exception, null);
        }
        [TestMethod]
        public void Getcompany_SearchCompanyById_ReturnsExistingCompany()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
            mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts/5", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetCompanyResponse>();
            // mockResponse.Object.Contact = MockData.CreateMockList<IContactViewModel>(mockRepository).SingleOrDefault(c => c.Object.Id == 5).Object;
            mockResponse.Object.CompanyViewModel = ContactViewModelMockData.GetMockCompaniesWithSetups
                (mockRepository).SingleOrDefault(c => c.Object.ContactID == SAMPLE_PERSON_ID).Object;

           
            mockContactService.Setup(c => c.GetCompany(It.IsAny<GetCompanyRequest>())).Returns(mockResponse.Object);


            var httpResponseMessage = controller.GetCompany(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(SAMPLE_PERSON_ID, contactResponse.CompanyViewModel.ContactID);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }
        [TestMethod]
        public void Getcompany_SearchCompanyByIdRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetCompanyResponse>();
            mockContactService.Setup(cs => cs.GetCompany(It.IsAny<GetCompanyRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetCompany(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
        }
        [TestMethod]
        public void Getcompany_SearchCompanyByIdNotFound_404ResourceNotFoundError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
              mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetCompanyResponse>();

            mockContactService.Setup(cs => cs.GetCompany(It.IsAny<GetCompanyRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new ResourceNotFoundException());

            var httpResponseMessage = controller.GetCompany(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();


            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
            Assert.AreNotEqual(exception, null);
        }

        /// <summary>
        /// Post Person/Contact Test
        /// </summary>
        
        [TestMethod]
        public void PostPerson_ValidPerson_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertPersonResponse>();
            PersonViewModel newPerson = new PersonViewModel() { ContactID = SAMPLE_PERSON_ID};

            mockResponse.Setup(c => c.PersonViewModel).Returns(newPerson);
            mockContactService.Setup(c => c.InsertPerson(It.IsAny<InsertPersonRequest>())).Returns(mockResponse.Object);           

            var httpResponseMessage = controller.PostPerson(It.IsAny<PersonViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertPersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.PersonViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.PersonViewModel.ContactID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PostPerson_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertPersonResponse>();

            mockContactService.Setup(c => c.InsertPerson(It.IsAny<InsertPersonRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.PostPerson(It.IsAny<PersonViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertPersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.PersonViewModel;
            var exception = postResponse.Exception;

            mockRepository.VerifyAll();
           
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
        [TestMethod]
        public void PostCompany_ValidCompany_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertCompanyResponse>();
            CompanyViewModel newcompany = new CompanyViewModel() { ContactID = SAMPLE_PERSON_ID };

            mockResponse.Setup(c => c.CompanyViewModel).Returns(newcompany);
            mockContactService.Setup(c => c.InsertCompany(It.IsAny<InsertCompanyRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PostCompany(It.IsAny<CompanyViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.CompanyViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.CompanyViewModel.ContactID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }
        [TestMethod]
        public void PostCompany_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertCompanyResponse>();

            mockContactService.Setup(c => c.InsertCompany(It.IsAny<InsertCompanyRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.PostCompany(It.IsAny<CompanyViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.CompanyViewModel;
            var exception = postResponse.Exception;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
        /// <summary>
        ///  Put aPerson/Contact TestMethod
        /// </summary>
        [TestMethod]
        public void PutPerson_UpdatePerson_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdatePersonResponse>();
            PersonViewModel newPerson = new PersonViewModel() { ContactID = SAMPLE_PERSON_ID };

            mockResponse.Setup(c => c.PersonViewModel).Returns(newPerson);
            mockContactService.Setup(c => c.UpdatePerson(It.IsAny<UpdatePersonRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutPerson(It.IsAny<PersonViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdatePersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.PersonViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.PersonViewModel.ContactID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        } 
        [TestMethod]
        public void PutPerson_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdatePersonResponse>();

            mockContactService.Setup(c => c.UpdatePerson(It.IsAny<UpdatePersonRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutPerson(It.IsAny<PersonViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdatePersonResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PutCompany_UpdateCompany_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateCompanyResponse>();
            CompanyViewModel newCompany = new CompanyViewModel() { ContactID = SAMPLE_PERSON_ID };

            mockResponse.Setup(c => c.CompanyViewModel).Returns(newCompany);
            mockContactService.Setup(c => c.UpdateCompany(It.IsAny<UpdateCompanyRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutCompany(It.IsAny<CompanyViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.CompanyViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.CompanyViewModel.ContactID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }
        [TestMethod]
        public void PutCompany_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateCompanyResponse>();

            mockContactService.Setup(c => c.UpdateCompany(It.IsAny<UpdateCompanyRequest>())).
                Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutCompany(It.IsAny<CompanyViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateCompanyResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);

           
        }

        /// <summary>
        ///  Delete Contact TestMethod
        /// </summary>
        /// 
        [TestMethod]
        public void Delete_ContactDeleted_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateContactResponse>();
           // ContactListViewModel newCompany = new ContactListViewModel();
           
           // mockResponse.Setup(c => c.ContactListViewModel).Returns(newCompany); 
            mockContactService.Setup(c => c.Deactivate(It.IsAny<DeactivateContactRequest>())).
                Returns(mockResponse.Object);

            var httpResponseMessage = controller.Delete(SAMPLE_PERSON_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateContactResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            //Assert.IsFalse(contactResponse.Result,"True");
            Assert.AreEqual(contactResponse.Exception,null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);

        }
        [TestMethod]
        public void Delete_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateContactResponse>();

            mockContactService.Setup(c => c.Deactivate(It.IsAny<DeactivateContactRequest>())).
                Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.Delete(SAMPLE_PERSON_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateContactResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
 
        [TestMethod]
        public void PostAction_validAction_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertActionResponse>();

            ActionViewModel newPerson = new ActionViewModel() { ActionId = SAMPLE_PERSON_ID };

            mockResponse.Setup(c => c.ActionViewModel).Returns(newPerson);
            mockActionService.Setup(c => c.InsertAction(It.IsAny<InsertActionRequest>())).Returns(mockResponse.Object);      


            var httpResponseMessage = controller.PostAction(It.IsAny<ActionViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertActionResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.ActionViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.ActionViewModel.ActionId> 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
     
        }
        [TestMethod]
        public void PostAction_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                             mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertActionResponse>();

            mockActionService.Setup(c => c.InsertAction(It.IsAny<InsertActionRequest>())).
               Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PostAction(It.IsAny<ActionViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertActionResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);

        }
        [TestMethod]
        public void InsertCommunicationTracker_ValidCommunication_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                               mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<CommunicationTrackerResponse>();

            CommunicationTrackerViewModel newTraker = new CommunicationTrackerViewModel() { CommunicationTrackerID = 1 };

            mockResponse.Setup(c => c.CommunicationTrackerViewModel).Returns(newTraker);
           // mockCommunicationservice.Setup(c => c.SaveCommunicationTrackerRequest(It.IsAny<CommunicationTrackerRequest>())).Returns(mockResponse.Object);


            //var httpResponseMessage = controller.InsertCommunicationTracker(It.IsAny<CommunicationTrackerViewModel>());
            //var postResponse = httpResponseMessage.Content.ReadAsAsync<CommunicationTrackerResponse>().ContinueWith(
            //    t => { return t.Result; }).Result;
            //var contactResponse = postResponse.CommunicationTrackerViewModel;

            //mockRepository.VerifyAll();
            //Assert.IsTrue(postResponse.CommunicationTrackerViewModel.CommunicationTrackerID > 0, "Id is not greater than zero after insert.");
            //Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            //Assert.AreEqual(postResponse.Exception, null);

        }
        [TestMethod]
        public void InsertCommunicationTracker_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                               mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<CommunicationTrackerResponse>();

           // mockCommunicationservice.Setup(c => c.SaveCommunicationTrackerRequest(It.IsAny<CommunicationTrackerRequest>())).
                        //   Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            //var httpResponseMessage = controller.InsertCommunicationTracker(It.IsAny<CommunicationTrackerViewModel>());
            //var postResponse = httpResponseMessage.Content.ReadAsAsync<CommunicationTrackerResponse>().ContinueWith(
            //    t => { return t.Result; }).Result;

            //mockRepository.VerifyAll();

            //Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            //Assert.AreNotEqual(postResponse.Exception, null);
        }
        [TestMethod]
        public void PutAction_UpdateAction_Succeed()
        {

            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateActionResponse>();
            ActionViewModel newAction = new ActionViewModel() { ActionId = SAMPLE_PERSON_ID };

            mockResponse.Setup(c => c.ActionViewModel).Returns(newAction);
            mockActionService.Setup(c => c.UpdateAction(It.IsAny<UpdateActionRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutAction(It.IsAny<ActionViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateActionResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.ActionViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.ActionViewModel.ActionId > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }
        [TestMethod]
        public void PutAction_UpdateAction_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateActionResponse>();

            mockActionService.Setup(c => c.UpdateAction(It.IsAny<UpdateActionRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutAction(It.IsAny<ActionViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateActionResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void GetContactActions_GetContactActionbyContactId_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
           
            GetActionListResponse response = mockRepository.Create<GetActionListResponse>().Object;
            response.ActionListViewModel = MockData.CreateMockList<ActionViewModel>(mockRepository).Select(c => c.Object).ToList();
            //mockResponse.Object.ActionListViewModel = ActionViewModelMockData.GetMockActionWithSetups(mockRepository).SingleOrDefault(c => c.Object.ActionId == 3).Object;
              //  .GetMockPersonWithSetups(mockRepository).SingleOrDefault(c => c.Object.ContactID == SAMPLE_PERSON_ID).Object;

            mockActionService.Setup(c => c.GetContactActions(It.IsAny<GetActionListRequest>())).Returns(response);

                var httpResponseMessage = controller.GetContactActions(SAMPLE_CONTACT_ID);
                var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetActionListResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            
            mockRepository.VerifyAll();
            Assert.AreEqual(10,contactResponse.ActionListViewModel.Count());
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }
        [TestMethod]
        public void GetContactActions_GetContactActionbyContactIdRuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
        mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetActionListResponse>();
            mockActionService.Setup(cs => cs.GetContactActions(It.IsAny<GetActionListRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.GetContactActions(SAMPLE_CONTACT_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetActionListResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(exception, null);
           
        }
        [TestMethod]
        public void GetContactActions_GetContactActionbyContactIdNotFound_404ResourceNotFoundError()
        {
         ContactsController controller = new ContactsController(mockContactService.Object,
        mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contact/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetActionListResponse>();
            mockActionService.Setup(cs => cs.GetContactActions(It.IsAny<GetActionListRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new ResourceNotFoundException());

            var httpResponseMessage = controller.GetContactActions(SAMPLE_CONTACT_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetActionListResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var exception = contactResponse.Exception;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.NotFound);
            Assert.AreNotEqual(exception, null);
        }
        [TestMethod]
        public void DeleteAction_DeactivateActionbyActionIdAndContactId_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateActionContactResponse>();
            //mockActionService.Setup(c=>c.)

            mockActionService.Setup(c => c.ContactDeleteForAction(It.IsAny<DeactivateActionContactRequest>())).
               Returns(mockResponse.Object);

            var httpResponseMessage = controller.DeleteAction(SAMPLE_ACTION_ID,SAMPLE_CONTACT_ID);
            var actionResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateActionContactResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.IsFalse(actionResponse.Result, "True");
            Assert.AreEqual(actionResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }
        [TestMethod]
        public void DeleteAction_DeactivateActionbyActionidandContactid_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeactivateActionContactResponse>();

            mockActionService.Setup(c => c.ContactDeleteForAction(It.IsAny<DeactivateActionContactRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.DeleteAction(SAMPLE_ACTION_ID, SAMPLE_CONTACT_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeactivateActionContactResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);


        }
        [TestMethod]
        public void DeleteIndex_DeletedIndexbyIndexValue_Succeed()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteIndexResponse>();

            mockContactService.Setup(c => c.DeleteIndex(It.IsAny<DeleteIndexRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.DeleteIndex("temp");
            var actionResponse = httpResponseMessage.Content.ReadAsAsync<DeleteIndexResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.IsFalse(actionResponse.Result, "True");
            Assert.AreEqual(actionResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);

        }
        [TestMethod]
        public void DeleteIndex_DeletedIndexbyIndexValue_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                 mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteIndexResponse>();

            mockContactService.Setup(c => c.DeleteIndex(It.IsAny<DeleteIndexRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.DeleteIndex("temp");
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeleteIndexResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);

        }
        [TestMethod]
        public void Timelines_GetTimelineDatabyIdandlimitandpagenumber_Succeed()
        {
            
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetTimeLineResponse>();

            mockContactService.Setup((c) => c.GetTimeLinesDataAsync(It.IsAny<GetTimeLineRequest>()).Result).Returns(mockResponse.Object);
            var httpResponseMessage = controller.Timelines(SAMPLE_CONTACT_ID,10,3);
            var actionResponse = httpResponseMessage.Result.Content.ReadAsAsync<GetTimeLineResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
         //   Assert.IsFalse(actionResponse.TotalRecords, "True");
            Assert.AreEqual(0,actionResponse.TotalRecords);
            Assert.AreEqual(actionResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.Result.StatusCode, HttpStatusCode.OK);

        }
        [TestMethod]
        public void Timelines_GetTimelineDatabyIdandlimitandpagenumber_RuntimeError_500InternalServerError()
        {
            ContactsController controller = new ContactsController(mockContactService.Object,
                mockActionService.Object, mockCommunicationservice.Object, mockContactRelationshipService.Object, mockdropdownValuesService.Object, mockuserService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/contacts", HttpMethod.Get);
            var mockResponse = mockRepository.Create<GetTimeLineResponse>();

            mockContactService.Setup(c => c.GetTimeLinesDataAsync(It.IsAny<GetTimeLineRequest>()).Result).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.Timelines(SAMPLE_CONTACT_ID, 10, 3);
            var postResponse = httpResponseMessage.Result.Content.ReadAsAsync<GetTimeLineResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.Result.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
    }
}
