using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Repository.Repositories;

namespace SmartTouch.CRM.ApplicationServices.Tests.TagTest
{
    //[TestClass]
    //public class TagRepositoryTest
    //{
    //    MockRepository mockRepository;
    //    ITagRepository tagRepository;
    //    Mock<IUnitOfWork> mockUnitofWork=new Mock<IUnitOfWork>();
    //    Mock<IObjectContextFactory> objectContextFactory=new Mock<IObjectContextFactory>();

    //    IList<Tag> tags = new List<Tag>()
    //    {
    //        new Tag(){TagID=1,TagName="facebook",Description="facebook"},
    //        new Tag(){TagID=1,TagName="google+",Description="google+"},
    //        new Tag(){TagID=1,TagName="skype",Description="skype"}
    //    };

    //    [TestInitialize]
    //    public void Initialize()
    //    {
    //        InitializeAutoMapper.Initialize();
    //        mockRepository = new MockRepository(MockBehavior.Default);
    //        objectContextFactory = mockRepository.Create<IObjectContextFactory>();
    //        tagRepository = new TagRepository(mockUnitofWork.Object, objectContextFactory.Object);
    //    }

    //    [TestMethod]
    //    public void FindById_ValidTag_Success()
    //    {
    //        var tag = new Tag(){TagID=1,TagName = "Google+", TagTypeID = TagType.Action};
    //        var tagRepo = tagRepository.FindBy(It.IsAny<int>());
    //        objectContextFactory.Setup(c=>c.Create()).Returns(tag);
    //        mockUnitofWork.Setup(c=>c.RegisterInsertion(tag,)
    //            mockUnitofWork.Verify(m=>m.RegisterInsertion());
    //    }

    //    [TestMethod]
    //    public void DuplicateTag_Insert_()
    //    {
    //        var tag = new Tag()
    //        {
    //            TagName = tags.First().TagName
    //        };

    //        mockUnitofWork.t => t.RegisterInsertion(tag));
    //        mockTagRepository.Verify(t => t.Insert(It.IsAny<Tag>()), Times.Never);
    //        //Assert.AreEqual(mockUnitOfWork.Commit(), Times.Once);
                    
    //    }

    //}
}
