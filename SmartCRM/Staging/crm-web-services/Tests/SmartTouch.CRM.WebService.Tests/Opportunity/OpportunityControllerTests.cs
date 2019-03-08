using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.WebService.Controllers;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;
using SmartTouch.CRM.ApplicationServices.Messaging;

namespace SmartTouch.CRM.WebService.Tests.Opportunity
{
    [TestClass]
    public class OpportunityControllerTests : ControllerTestBase
    {
        Mock<IOpportunitiesService> mockOpportunitiesService;
        Mock<IContactService> mockContactService;
        MockRepository mockRepository;

        public const int SAMPLE_OPPORTUNITY_ID = 1;
        public const int SAMPLE_CONTACT_ID = 1;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockService = mockRepository.Create<IOpportunitiesService>();
            var ContactService = mockRepository.Create<IContactService>();
            mockOpportunitiesService = mockService;
            mockContactService = ContactService;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void PostOpportunity_validOpportunity_Succeed()
        {
            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/opportunity", HttpMethod.Post);
            var mockResponse = mockRepository.Create<InsertOpportunityResponse>();

            OpportunityViewModel newOpportunity = new OpportunityViewModel() { OpportunityID = SAMPLE_OPPORTUNITY_ID, CreatedBy = 1, CreatedOn = DateTime.Now };

            mockResponse.Setup(c => c.opportunityViewModel).Returns(newOpportunity);
            mockOpportunitiesService.Setup(c => c.InsertOpportunity(It.IsAny<InsertOpportunityRequest>())).Returns(mockResponse.Object);


            var httpResponseMessage = controller.InsertOpportunity(newOpportunity);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertOpportunityResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.opportunityViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.opportunityViewModel.OpportunityID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PostOpportunity_RuntimeError_500InternalServerError()
        {
            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/opportunity", HttpMethod.Post);
            var mockResponse = mockRepository.Create<InsertOpportunityResponse>();

            OpportunityViewModel newOpportunity = new OpportunityViewModel() { OpportunityID = SAMPLE_OPPORTUNITY_ID, CreatedBy = 1, CreatedOn = DateTime.Now };

            //mockResponse.Setup(c => c.opportunityViewModel).Returns(newOpportunity);
            mockOpportunitiesService.Setup(c => c.InsertOpportunity(It.IsAny<InsertOpportunityRequest>())).Returns(mockResponse.Object);
            //mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.InsertOpportunity(newOpportunity);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertOpportunityResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PutOpportunity_UpdateOpportunity_Succeed()
        {

            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/opportunity/1", HttpMethod.Put);
            var mockResponse = mockRepository.Create<UpdateOpportunityResponse>();
            OpportunityViewModel newOpportunity = new OpportunityViewModel() { OpportunityID = SAMPLE_OPPORTUNITY_ID, CreatedBy = 1 , LastModifiedOn = DateTime.Now };

            mockResponse.Setup(c => c.opportunityViewModel).Returns(newOpportunity);
            mockOpportunitiesService.Setup(c => c.UpdateOpportunity(It.IsAny<UpdateOpportunityRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.UpdateOpportunity(newOpportunity);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateOpportunityResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.opportunityViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.opportunityViewModel.OpportunityID > 0);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PutOpportunity_UpdateOpportunity_RuntimeError_500InternalServerError()
        {
            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/opportunity/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateOpportunityResponse>();
            OpportunityViewModel getOpportunity = GetMock();

            getOpportunity.CreatedBy = 2;
            mockResponse.Setup(c => c.opportunityViewModel).Returns(getOpportunity);
            mockOpportunitiesService.Setup(c => c.UpdateOpportunity(It.IsAny<UpdateOpportunityRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.UpdateOpportunity(getOpportunity);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateOpportunityResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.opportunityViewModel;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void ReIndexOpportunities_TotalOpprtunity_Success()
        {
            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Get);
            var mockResponse = mockRepository.Create<ReIndexDocumentResponse>();

            mockOpportunitiesService.Setup(c => c.ReIndexOpportunities(It.IsAny<ReIndexDocumentRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.ReIndexOpportunities();
            var actionResponse = httpResponseMessage.Content.ReadAsAsync<ReIndexDocumentResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(actionResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void ReIndexOpportunities_TotalTags_RuntimeError_500InternalServerError()
        {
            OpportunitiesController controller = new OpportunitiesController(mockContactService.Object, mockOpportunitiesService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Get);
            var mockResponse = mockRepository.Create<ReIndexDocumentResponse>();

            mockOpportunitiesService.Setup(c => c.ReIndexOpportunities(It.IsAny<ReIndexDocumentRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.ReIndexOpportunities();
            var postResponse = httpResponseMessage.Content.ReadAsAsync<ReIndexDocumentResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        public static OpportunityViewModel GetMock()
        {
            OpportunityViewModel mockOpportunity = new OpportunityViewModel();
            mockOpportunity.OpportunityID = 1;
            mockOpportunity.OpportunityName = "OpportunityName";
            mockOpportunity.CreatedOn = DateTime.Now;
            mockOpportunity.CreatedBy = 1;

            return mockOpportunity;
        }
    }
}
