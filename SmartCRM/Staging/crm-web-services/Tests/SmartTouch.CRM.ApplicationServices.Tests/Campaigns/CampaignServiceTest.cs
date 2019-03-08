using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Tests.Contacts;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository.Repositories;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.ApplicationServices.Tests.Campaigns
{
    [TestClass]
    public class CampaignServiceTest
    {
        int LEGAL_CAMPAIGN_ID = 5;
        int ILLEGAL_CAMPAIGN_ID = 100000;
        CampaignTemplate sampleLayout = new CampaignTemplate() { Id = 1, Type = Entities.CampaignTemplateType.Layout };
        CampaignTemplate samplePredesigned = new CampaignTemplate() { Id = 1, Type = Entities.CampaignTemplateType.PreDesigned };

        #region Declare CampaignService Constructor elements
        MockRepository mockRepository;
        ICampaignService campaignService;
        Mock<ICachingService> mockCachingService;
        Mock<ICampaignRepository> mockCampaignRepository;
        Mock<IContactRepository> mockContactRepository;
        Mock<IUrlService> mockUrlService;
        Mock<ITagRepository> mockTagRepository;
        Mock<IIndexingService> mockIndexingService;
        Mock<ISearchService<Campaign>> mockSearchService;
        Mock<ISearchService<Contact>> mockContactSearchService;
        Mock<IAdvancedSearchService> mockAdvancedSearchService;
        Mock<IUserService> mockUserService;
        Mock<IUserRepository> mockUserRepository;
        Mock<IAccountService> mockAccountService;
        Mock<IMessageService> mockMessageService;
        Mock<ICommunicationService> mockCommunicaitonService;
        Mock<IServiceProviderRepository> mockServiceProviderRepository;
        #endregion

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            mockCampaignRepository = mockRepository.Create<ICampaignRepository>();
            mockContactRepository = mockRepository.Create<IContactRepository>();
            mockTagRepository = mockRepository.Create<ITagRepository>();
            mockUrlService = mockRepository.Create<IUrlService>();
            mockIndexingService = mockRepository.Create<IIndexingService>();
            mockSearchService = mockRepository.Create<ISearchService<Campaign>>();
            mockContactSearchService = mockRepository.Create<ISearchService<Contact>>();
            mockCachingService = mockRepository.Create<ICachingService>();
            mockAdvancedSearchService = mockRepository.Create<IAdvancedSearchService>();
            mockUserService = mockRepository.Create<IUserService>();
            mockUserRepository = mockRepository.Create<IUserRepository>();
            mockAccountService = mockRepository.Create<IAccountService>();
            mockMessageService = mockRepository.Create<IMessageService>();
            mockCommunicaitonService = mockRepository.Create<ICommunicationService>();
            mockServiceProviderRepository = mockRepository.Create<IServiceProviderRepository>();
            IUnitOfWork mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;

            CampaignMockData campaignMockData = new CampaignMockData();
            campaignService = new CampaignService(mockCampaignRepository.Object,
            mockContactRepository.Object, mockUnitOfWork, mockUrlService.Object, mockTagRepository.Object,
            mockCachingService.Object, mockIndexingService.Object, mockSearchService.Object,
            mockAdvancedSearchService.Object, mockUserService.Object, mockUserRepository.Object, mockMessageService.Object, mockCommunicaitonService.Object,
            mockAccountService.Object, mockServiceProviderRepository.Object, mockContactSearchService.Object);
        }

        #region TestCleanUp
        [TestCleanup]
        public void Cleanup()
        {
        }
        #endregion

        #region GetCampaigns
        //[TestMethod]
        //public void GetCampaignList_SearchWithCampaignId_ReturnCampaignListSuccessfully_NotImplementedYet()
        //{
        //    Assert.Fail();
        //}

        //[TestMethod]
        //public void GetCampaignList_SearchWithCampaignIdAndWhenNoCampaignsFound_ShouldNotThrowException_NotImplementedYet()
        //{
        //    Assert.Fail();
        //}

        [TestMethod]
        public void GetCampaign_SearchWithCampaignId_ReturnCampaignSuccessfully()
        {
            Campaign campaign = CampaignMockData.CreateCampaignWithCustomValues(mockRepository, 5, "Test", new DateTime(2012, 1, 1)
                , "a@b.com", 5, Entities.CampaignStatus.Draft, sampleLayout, "Test campaign", "", 1);
            mockCampaignRepository.Setup(tr => tr.GetCampaignById(It.IsAny<int>())).Returns(campaign);
            GetCampaignResponse response = new GetCampaignResponse();
            GetCampaignRequest request = new GetCampaignRequest(LEGAL_CAMPAIGN_ID);
            response = campaignService.GetCampaign(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(campaign.Id, response.CampaignViewModel.CampaignID);
        }

        [TestMethod]
        public void GetCampaign_SearchWithCampaignIdThatDoesNotExist_CampaignNotFoundException()
        {
            mockCampaignRepository.Setup(tr => tr.GetCampaignById(It.IsAny<int>())).Throws(new InvalidOperationException("Campaign doesn't exist"));
            GetCampaignRequest request = new GetCampaignRequest(ILLEGAL_CAMPAIGN_ID);
            GetCampaignResponse response = new GetCampaignResponse();
            response = campaignService.GetCampaign(request);
            mockRepository.VerifyAll();
            Assert.AreEqual("Campaign doesn't exist", response.Exception.Message);
        }
        #endregion

        #region Insert Campaign
        [TestMethod]
        public void InsertCampaign_PassedValidCampaign_InsertedSuccessfully()
        {
            mockCampaignRepository.Setup(tr => tr.IsCampaignNameUnique(It.IsAny<Campaign>())).Returns(true);
            mockCampaignRepository.Setup(tr => tr.Insert(It.IsAny<Campaign>())).Verifiable("Insert method called");
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 0, DateTime.Now.ToString()
                , "test", DateTime.Now, "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            InsertCampaignRequest request = new InsertCampaignRequest() { CampaignViewModel = campaignViewModel };
            InsertCampaignResponse response = new InsertCampaignResponse();
            response = campaignService.InsertCampaign(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertCampaign_PassedCampaignWithNameThatAlreadyExists_ThrowException()
        {
            mockCampaignRepository.Setup(tr => tr.IsCampaignNameUnique(It.IsAny<Campaign>())).Returns(false);
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 0, DateTime.Now.ToString()
                , "test", DateTime.Now, "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            InsertCampaignRequest request = new InsertCampaignRequest() { CampaignViewModel = campaignViewModel };
            InsertCampaignResponse response = new InsertCampaignResponse();
            response = campaignService.InsertCampaign(request);
            mockRepository.VerifyAll();
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void InsertCampaign_WithNoContactTags_NotInserted()
        {
            mockCampaignRepository.Setup(tr => tr.IsCampaignNameUnique(It.IsAny<Campaign>())).Returns(true);
            mockCampaignRepository.Setup(tr => tr.Insert(It.IsAny<Campaign>())).Verifiable("Insert method called");
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 0, DateTime.Now.ToString()
                , "test", DateTime.Now, "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            InsertCampaignRequest request = new InsertCampaignRequest() { CampaignViewModel = campaignViewModel };
            InsertCampaignResponse response = new InsertCampaignResponse();
            response = campaignService.InsertCampaign(request);
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        #endregion

        #region UpdateCampaign
        [TestMethod]
        public void UpdateCampaign_PassedValidCampaign_UpdatedSuccessfully()
        {
            mockCampaignRepository.Setup(cr => cr.Update(It.IsAny<Campaign>())).Verifiable();
            mockCampaignRepository.Setup(cr => cr.IsCampaignNameUnique(It.IsAny<Campaign>())).Returns(true);
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 1, DateTime.Now.ToString()
                , "test", DateTime.Now, "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            UpdateCampaignRequest request = new UpdateCampaignRequest() { CampaignViewModel = campaignViewModel };
            UpdateCampaignResponse response = campaignService.UpdateCampaign(request);
            mockCampaignRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateCampaign_PassedInvalidCampaign_NotInserted_NotImplementedYet()
        {
            mockCampaignRepository.Setup(cr => cr.IsCampaignNameUnique(It.IsAny<Campaign>())).Returns(true);
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 1, DateTime.Now.ToString()
                , "test", DateTime.Now.AddDays(-1), "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            UpdateCampaignRequest request = new UpdateCampaignRequest() { CampaignViewModel = campaignViewModel };
            UpdateCampaignResponse response = campaignService.UpdateCampaign(request);
            mockCampaignRepository.VerifyAll();
            Assert.AreEqual("Schedule time is invalid\r\n", response.Exception.Message);
        }
        #endregion

        #region DeleteCampaign
        [TestMethod]
        public void DeleteCampaign_PassedValidCampaignId_SoftDeletedSuccessfully_NotImplementedYet()
        {
            mockCampaignRepository.Setup(cr => cr.DeactivateCampaign(It.IsAny<int[]>(), It.IsAny<int>())).Verifiable();
            CampaignViewModel campaignViewModel = CampaignMockData.CreateCampaignViewModelWithCustomValues(mockRepository, 1, DateTime.Now.ToString()
                , "test", DateTime.Now, "a@b.com", 2, Entities.CampaignStatus.Scheduled, 1, 2, 2);
            DeleteCampaignRequest request = new DeleteCampaignRequest() { CampaignID = new int[] { campaignViewModel.CampaignID } };
            DeleteCampaignResponse response = campaignService.Deactivate(request);
            mockCampaignRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }
        #endregion

        #region AutoMapper
        #endregion
    }
}
