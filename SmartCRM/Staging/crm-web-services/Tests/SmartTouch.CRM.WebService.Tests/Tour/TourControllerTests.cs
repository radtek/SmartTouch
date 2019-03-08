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
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Tour
{
    [TestClass]
    public class TourControllerTests : ControllerTestBase
    {
        Mock<ITourService> mockTourService;
        MockRepository mockRepository;

        public const int SAMPLE_TOUR_ID = 1;
        public const int SAMPLE_CONTACT_ID = 1;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockService = mockRepository.Create<ITourService>();
            mockTourService = mockService;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }


        /// <summary>
        /// Insert valid tour success TestMethod
        /// </summary>
        [TestMethod]
        public void PostTour_validTour_Succeed()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tour", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertTourResponse>();

            TourViewModel newContactTour = new TourViewModel() { TourID = SAMPLE_TOUR_ID };

            mockResponse.Setup(c => c.TourViewModel).Returns(newContactTour);
            mockTourService.Setup(c => c.InsertTour(It.IsAny<InsertTourRequest>())).Returns(mockResponse.Object);


            var httpResponseMessage = controller.PostTour(It.IsAny<TourViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertTourResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.TourViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.TourViewModel.TourID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// Insert tour getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void PostTour_RuntimeError_500InternalServerError()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tour", HttpMethod.Get);
            var mockResponse = mockRepository.Create<InsertTourResponse>();

            mockTourService.Setup(c => c.InsertTour(It.IsAny<InsertTourRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PostTour(It.IsAny<TourViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<InsertTourResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// update tour success TestMethod
        /// </summary>
        [TestMethod]
        public void PutTour_UpdateTour_Succeed()
        {

            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tour/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateTourResponse>();
            TourViewModel newContactTour = new TourViewModel() { TourID = SAMPLE_TOUR_ID };

            mockResponse.Setup(c => c.TourViewModel).Returns(newContactTour);
            mockTourService.Setup(c => c.UpdateTour(It.IsAny<UpdateTourRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.PutTour(It.IsAny<TourViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateTourResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var contactResponse = postResponse.TourViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.TourViewModel.TourID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// update tour getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void PutTour_UpdateTour_RuntimeError_500InternalServerError()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tour/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateTourResponse>();

            mockTourService.Setup(c => c.UpdateTour(It.IsAny<UpdateTourRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutTour(It.IsAny<TourViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateTourResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// delete tour success TestMethod
        /// </summary>
        [TestMethod]
        public void DeleteTour_DeactivateTourforContacts_Succeed()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteTourResponse>();
            mockTourService.Setup(c => c.DeleteTour(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).
               Returns(mockResponse.Object);

            var httpResponseMessage = controller.DeleteTour(SAMPLE_TOUR_ID);
            var noteResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTourResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            //Assert.AreEqual(noteResponse., null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        /// <summary>
        /// delete tour getting exception error occured TestMethod
        /// </summary>
        [TestMethod]
        public void DeleteTour_DeactivateTourforContacts_RuntimeError_500InternalServerError()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/note", HttpMethod.Get);
            var mockResponse = mockRepository.Create<DeleteTourResponse>();

            mockTourService.Setup(c => c.DeleteTour(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.DeleteTour(SAMPLE_TOUR_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTourResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        /// <summary>
        /// Get tour for individual contact TestMethod
        /// </summary>
        [TestMethod]
        public void GetTour_GetTourforContact_ReturnSuccess()
        {
            ToursController controller = new ToursController(mockTourService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tour/1", HttpMethod.Get);

            var mockResponse = mockRepository.Create<GetTourResponse>();

            GetTourResponse response = mockRepository.Create<GetTourResponse>().Object;
            response.TourViewModel = MockData.CreateMockList<TourViewModel>(mockRepository).Select(c => c.Object).FirstOrDefault();
            mockTourService.Setup(c => c.GetTour(It.IsAny<int>())).Returns(response);
            var httpResponseMessage = controller.GetTour(SAMPLE_TOUR_ID);
            var contactResponse = httpResponseMessage.Content.ReadAsAsync<GetTourResponse>().ContinueWith(
            t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(contactResponse.Exception, null);
        }
    }
}
