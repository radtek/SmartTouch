using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.Domain.Tag;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Tests;

using Moq;

namespace SmartTouch.CRM.ApplicationServices.Tests.Tags
{
    [TestClass]
    public class TagServiceTest
    {
        MockRepository mockRepository;
        ITagService tagService;
        IUnitOfWork mockUnitOfWork;
        Mock<ITagRepository> mockTagRepository;

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;
            mockTagRepository = mockRepository.Create<ITagRepository>();
            tagService = new TagService(mockTagRepository.Object, mockUnitOfWork);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        //[TestMethod]
        //public void GetTags_ValidTags_Succeed()
        //{
        //    var mockTags = TagMockData.GetMockTagsWithSetups(mockRepository, 10).Select(c => c.Object).ToList();
        //    mockTagRepository.Setup(tr => tr.FindAll()).Returns(mockTags);

        //    GetTagsResponse response = tagService.GetTags();

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(mockTags.Count(), response.TagsListViewModel.Tags.Count());
        //    Assert.AreEqual(null, response.Exception);
        //}

        //[TestMethod]
        //public void GetTags_NoTags_EmptyTags()
        //{
        //    var mockTags = TagMockData.GetMockTagsWithSetups(mockRepository, 0).Select(c => c.Object).ToList();
        //    mockTagRepository.Setup(tr => tr.FindAll()).Returns(mockTags);

        //    GetTagsResponse response = tagService.GetTags();

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(0, response.TagsListViewModel.Tags.Count());
        //    Assert.AreEqual(null, response.Exception);
        //}

        [TestMethod]
        public void GetAllTags_ValidTags_Succeed()
        {
            var mockTags = TagMockData.GetMockTags(mockRepository, 10).ToList();
            mockTagRepository.Setup(cr => cr.Search(It.IsAny<string>())).Returns(mockTags);

            GetTagListResponse response = tagService.GetAllTags(new GetTagListRequest() { Name = "" });
            ITagsListViewModel viewModel = response.TagsListViewModel;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockTags.Count, viewModel.Tags.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetAllTags_RunTimeException_ExceptionDetails()
        {
            mockTagRepository.Setup(cr => cr.Search(It.IsAny<string>())).Throws(new InvalidOperationException());

            GetTagListResponse response = tagService.GetAllTags(new GetTagListRequest() { Name = "" });

            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        //[TestMethod]
        //public void GetTags_RunTimeException_ExceptionDetails()
        //{
        //    mockTagRepository.Setup(tr => tr.FindAll()).Throws(new InvalidOperationException());

        //    GetTagsResponse response = tagService.GetTags();

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
        //    Assert.AreNotEqual(null, response.Exception);
        //}

        //[TestMethod]
        //public void GetTag_ValidTag_Succeed()
        //{
        //    var mockTags = TagMockData.GetMockTagsWithSetups(mockRepository, 1).Select(c => c.Object).FirstOrDefault();
        //    mockTagRepository.Setup(tr => tr.FindBy(It.IsAny<int>())).Returns(mockTags);

        //    GetTagResponse response = tagService.GetTag(new GetTagRequest(1));
        //    int id = response.TagViewModel.TagID;

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(mockTags.Id, id);
        //    Assert.AreEqual(null, response.Exception);
        //}

        //[TestMethod]
        //public void GetTag_TagNotFound_ResourceNotFoundException()
        //{
        //    Tag tag = null;
        //    mockTagRepository.Setup(tr => tr.FindBy(It.IsAny<int>())).Returns(tag);

        //    GetTagResponse response = tagService.GetTag(new GetTagRequest(1));

        //    mockRepository.VerifyAll();
        //    Assert.AreEqual(typeof(ResourceNotFoundException), response.Exception.GetType());
        //}

        //[TestMethod]
        //public void GetTagByName_ValidTag_Succeed()
        //{
        //    var mockTags = TagMockData.GetMockTagsWithSetups(mockRepository, 1).Select(c => c.Object).FirstOrDefault();
        //    mockTagRepository.Setup(tr => tr.Search(It.IsAny<string>)).Returns(mockTags);

        //    GetTagsResponse response = tagService.GetTags(new GetTagsRequest());
        //    var tagName = response.TagsListViewModel.Tags;
        //    mockRepository.VerifyAll();

        //    Assert.AreEqual(mockTags, tagName);
        //    Assert.AreEqual(null, response.Exception);
        //    //int id = response.TagViewModel.TagID;

        //    //mockRepository.VerifyAll();
        //    //Assert.AreEqual(mockTags.Id, id);
        //    //Assert.AreEqual(null, response.Exception);
        //}

        [TestMethod]
        public void Save_ValidTag_Succeed()
        {
            mockTagRepository.Setup(t => t.Insert(It.IsAny<Tag>()));

            InsertTagResponse insertTagResponse = tagService.InsertTag(new InsertTagRequest());

            mockTagRepository.VerifyAll();
            mockTagRepository.Verify(t => t.Insert(It.IsAny<Tag>()), Times.Once);

            //ITagService tagServiceInsert = new TagService(mockTagRepository.Object, mockUnitOfWork);
            //tagServiceInsert.CreateMany(tagViewModels);
            //mockTagRepository.VerifyAll();

            //mockTagRepository.Verify(t => t.Insert(It.IsAny<Tag>()), Times.Exactly(tagViewModels.Count));
        }

        [TestMethod]
        public void DeleteTag_ValidTag_Succeed()
        {
            int itemToDelete = 3;
            var mockTags = TagMockData.GetMockTagsWithSetups(mockRepository, 1).Select(c => c.Object).FirstOrDefault();
            mockTagRepository.Setup(x => x.FindBy(It.IsAny<int>())).Returns(mockTags);
            //mockTagRepository.Setup(x => x.Delete(mockTags));

            DeleteTagResponse response = tagService.DeleteTag(new DeleteTagRequest(itemToDelete));

            mockRepository.VerifyAll();
            mockTagRepository.Verify(t => t.Delete(It.IsAny<Tag>()), Times.Once);
        }

        [TestMethod]
        public void DeleteTag_TagNotFound_ResourceNotFoundException()
        {
            var itemToDelete = TagMockData.GetMockTagsWithSetups(mockRepository, 1).Select(c => c.Object).FirstOrDefault();
            mockTagRepository.Setup(tr => tr.FindBy(It.IsAny<int>())).Returns(itemToDelete);

            DeleteTagResponse response = tagService.DeleteTag(new DeleteTagRequest(1));

            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(ResourceNotFoundException), response.Exception.GetType());
        }


    }
}
