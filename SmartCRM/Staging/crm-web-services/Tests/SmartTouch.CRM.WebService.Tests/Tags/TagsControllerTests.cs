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
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Tests;

using Moq;
using Moq.Linq;

namespace SmartTouch.CRM.WebService.Tests.Tags
{
    [TestClass]
    public class TagsControllerTests : ControllerTestBase
    {
        Mock<ITagService> mockTagService;

        public const int SAMPLE_TAG_ID = 1;
        public int[] SAMPLE_TAG_IDS = new int[]{2, 3, 6};
        public const string SAMPLE_TAG_NAME = "Facebook";
        public const int SAMPLE_CONTACT_ID = 2;
        MockRepository mockRepository;

        [TestInitialize]
        public void InitializeTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            var mockService = mockRepository.Create<ITagService>();
            mockTagService = mockService;
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void InsertTag_ValidTag_ReturnSuccess()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Post);
            var mockResponse = mockRepository.Create<SaveTagResponse>();

            TagViewModel newTag = new TagViewModel() { TagID = SAMPLE_TAG_ID };

            mockResponse.Setup(c => c.TagViewModel).Returns(newTag);
            mockTagService.Setup(c => c.SaveTag(It.IsAny<SaveTagRequest>())).Returns(mockResponse.Object);


            var httpResponseMessage = controller.PostTag(It.IsAny<TagViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<SaveTagResponse>().ContinueWith(
                t => { return t.Result; }).Result;
            var tagResponse = postResponse.TagViewModel;

            mockRepository.VerifyAll();
            Assert.IsTrue(postResponse.TagViewModel.TagID > 0, "Id is not greater than zero after insert.");
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void PostTag_RuntimeError_500InternalServerError()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Post);
            var mockResponse = mockRepository.Create<SaveTagResponse>();

            mockTagService.Setup(c => c.SaveTag(It.IsAny<SaveTagRequest>())).
               Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PostTag(It.IsAny<TagViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<SaveTagResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        //[TestMethod]
        //public void UpdateTag_UpdateTag_Succeed()
        //{

        //    TagsController controller = new TagsController(mockTagService.Object);
        //    this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags/1", HttpMethod.Put);
        //    var mockResponse = mockRepository.Create<UpdateTagResponse>();
        //    TagViewModel newTag = new TagViewModel() { TagID = SAMPLE_TAG_ID };

        //    mockResponse.Setup(c => c.TagViewModel).Returns(newTag);
        //    mockTagService.Setup(c => c.UpdateTag(It.IsAny<UpdateTagRequest>())).Returns(mockResponse.Object);

        //    var httpResponseMessage = controller.PutTag(It.IsAny<TagViewModel>());
        //    var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateTagResponse>().ContinueWith(
        //        t => { return t.Result; }).Result;
        //    var tagResponse = postResponse.TagViewModel;

        //    mockRepository.VerifyAll();
        //    Assert.IsTrue(postResponse.TagViewModel.TagID > 0);
        //    Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        //    Assert.AreEqual(postResponse.Exception, null);
        //}
        [TestMethod]
        public void UpdateTag_UpdateTag_RuntimeError_500InternalServerError()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags/1", HttpMethod.Get);
            var mockResponse = mockRepository.Create<UpdateTagResponse>();

            mockTagService.Setup(c => c.UpdateTag(It.IsAny<UpdateTagRequest>())).
              Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());
            var httpResponseMessage = controller.PutTag(It.IsAny<TagViewModel>());
            var postResponse = httpResponseMessage.Content.ReadAsAsync<UpdateTagResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void DeleteTag_DeleteTagByName_Succeed()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Delete);
            var mockResponse = mockRepository.Create<DeleteTagResponse>();

            mockTagService.Setup(c => c.DeleteTag(It.IsAny<DeleteTagRequest>())).
                Returns(mockResponse.Object);

            var httpResponseMessage = controller.Delete(SAMPLE_TAG_NAME, SAMPLE_CONTACT_ID);
            var noteResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTagResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void DeleteTags_DeleteTagsByTagIds_Succeed()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Delete);
            var mockResponse = mockRepository.Create<DeleteTagResponse>();

            mockTagService.Setup(c => c.DeleteTags(It.IsAny<DeleteTagIdsRequest>())).
                Returns(mockResponse.Object);

            var httpResponseMessage = controller.Delete(SAMPLE_TAG_IDS);
            var noteResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTagResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void DeleteTag_DeleteTagByName_RuntimeError_500InternalServerError()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Delete);
            var mockResponse = mockRepository.Create<DeleteTagResponse>();

            mockTagService.Setup(c => c.DeleteTag(It.IsAny<DeleteTagRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.Delete(SAMPLE_TAG_NAME, SAMPLE_CONTACT_ID);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTagResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void DeleteTags_DeleteTagByIds_RuntimeError_500InternalServerError()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Delete);
            var mockResponse = mockRepository.Create<DeleteTagResponse>();

            mockTagService.Setup(c => c.DeleteTags(It.IsAny<DeleteTagIdsRequest>())).
             Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.Delete(SAMPLE_TAG_IDS);
            var postResponse = httpResponseMessage.Content.ReadAsAsync<DeleteTagResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }

        [TestMethod]
        public void ReIndextags_TotalTags_Success()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Get);
            var mockResponse = mockRepository.Create<ReIndexTagsResponse>();

            mockTagService.Setup(c => c.ReIndexTags(It.IsAny<ReIndexTagsRequest>())).Returns(mockResponse.Object);

            var httpResponseMessage = controller.ReIndexTags();
            var actionResponse = httpResponseMessage.Content.ReadAsAsync<ReIndexTagsResponse>().ContinueWith(
              t => { return t.Result; }).Result;

            mockRepository.VerifyAll();
            //Assert.IsFalse(actionResponse.IndexedTags, 1);
            Assert.AreEqual(actionResponse.Exception, null);
            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void ReIndextags_TotalTags_RuntimeError_500InternalServerError()
        {
            TagsController controller = new TagsController(mockTagService.Object);
            this.SetupControllerTests(controller, "http://localhost/STCRMService/api/tags", HttpMethod.Get);
            var mockResponse = mockRepository.Create<ReIndexTagsResponse>();

            mockTagService.Setup(c => c.ReIndexTags(It.IsAny<ReIndexTagsRequest>())).Returns(mockResponse.Object);
            mockResponse.Setup(r => r.Exception).Returns(new InvalidOperationException());

            var httpResponseMessage = controller.ReIndexTags();
            var postResponse = httpResponseMessage.Content.ReadAsAsync<ReIndexTagsResponse>().ContinueWith(
                t => { return t.Result; }).Result;

            mockRepository.VerifyAll();

            Assert.AreEqual(httpResponseMessage.StatusCode, HttpStatusCode.InternalServerError);
            Assert.AreNotEqual(postResponse.Exception, null);
        }
    }
}
