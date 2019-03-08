using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Tests;
using Moq;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.SearchEngine.Indexing;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Tests.LeadScoreRules
{
    [TestClass]
    public class LeadScoreRuleServiceTest
    {
        public const int LEAD_SCORE_RULE_ID = 1;
        public readonly int[] leadScoreRulevalues = { 1, 2, 3 };

        MockRepository mockRepository;
        ILeadScoreRuleService leadScoreRuleService;
        Mock<ILeadScoreRuleRepository> mockLeadScoreRuleRepository;
        Mock<ICampaignRepository> mockCampaignRepository;
        Mock<ITagRepository> mockTagRepository;
        Mock<IFormRepository> mockFormRepository;
         Mock<ICachingService> mockCachingService;
         Mock<IIndexingService> indexingService;
         Mock<ITagService> tagService;

        [TestInitialize]
        public void Initialize()
        {
            InitializeAutoMapper.Initialize();
            mockRepository = new MockRepository(MockBehavior.Default);
            IUnitOfWork mockUnitOfWork = mockRepository.Create<IUnitOfWork>().Object;
            mockLeadScoreRuleRepository = mockRepository.Create<ILeadScoreRuleRepository>();
            mockTagRepository = mockRepository.Create<ITagRepository>();
            mockCampaignRepository = mockRepository.Create<ICampaignRepository>();
            mockFormRepository = mockRepository.Create<IFormRepository>();
            mockCachingService=mockRepository.Create<ICachingService>();
            indexingService = mockRepository.Create<IIndexingService>();
            tagService=mockRepository.Create<ITagService>();
            leadScoreRuleService = new LeadScoreRuleService(mockLeadScoreRuleRepository.Object, mockTagRepository.Object, mockCampaignRepository.Object, mockCachingService.Object, mockFormRepository.Object, mockUnitOfWork,tagService.Object, indexingService.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
        }

        [TestMethod]
        public void GetAllLeadScoreRules_ValidRule_Succeed()
        {
            var mockRules = LeadScoreRuleMockData.GetMockLeadScoreRules(mockRepository, 10).ToList();
            mockLeadScoreRuleRepository.Setup(cr => cr.FindAll(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ListSortDirection>(),null)).Returns(mockRules);

            GetLeadScoreListResponse response = leadScoreRuleService.GetLeadScoresList(new GetLeadScoreListRequest() { AccountID = 1, Limit = 10, PageNumber = 1 , Query = ""});
            IEnumerable<LeadScoreRuleViewModel> leadScoreRules = response.LeadScoreViewModel;
            mockRepository.VerifyAll();
            Assert.AreEqual(mockRules.Count, leadScoreRules.Count());
            Assert.AreEqual(null, response.Exception);
        }
        
        [TestMethod]
        public void GetAllLeadScoreRules_RunTimeException_ExceptionHandled()
        {
            mockLeadScoreRuleRepository.Setup(c => c.FindAll(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ListSortDirection>(),null)).Throws(new InvalidOperationException());
            GetLeadScoreListResponse response = leadScoreRuleService.GetLeadScoresList(new GetLeadScoreListRequest() { });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }
        
        [TestMethod]
        public void GetLeadScoreRule_ValidRule_Succeed()
        {
            var mockRule = LeadScoreRuleMockData.GetMockLeadScoreRulesWithSetups(mockRepository, 1).FirstOrDefault();
            mockLeadScoreRuleRepository.Setup(cr => cr.FindBy(It.IsAny<int>())).Returns(mockRule.Object);
            mockTagRepository.Setup(c => c.FindByContact(It.IsAny<int>(), It.IsAny<int>())).Returns(new List<Tag>());

            GetLeadScoreResponse response = leadScoreRuleService.GetLeadScoreRule(new GetLeadScoreRequest(1));
            int id = response.LeadScoreViewModel.LeadScoreRuleID;

            mockLeadScoreRuleRepository.VerifyAll();
            Assert.AreEqual(mockRule.Object.Id, id);
            Assert.AreNotEqual(null, response.Exception);
        }
        
        [TestMethod]
        public void GetLeadScore_RuntimeException_ExceptionDetails()
        {
            mockLeadScoreRuleRepository.Setup(c => c.FindBy(It.IsAny<int>())).Throws(new InvalidOperationException());

            GetLeadScoreResponse response = leadScoreRuleService.GetLeadScoreRule(new GetLeadScoreRequest(LEAD_SCORE_RULE_ID));
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }
        
        [TestMethod]
        public void InsertLeadScoreRule_ValidRule_Succeed()
        {
            LeadScoreRuleViewModel viewModel = LeadScoreRuleMockData.GetLeadscoreRuleViewModel();
            mockLeadScoreRuleRepository.Setup(mnt => mnt.Insert(It.IsAny<LeadScoreRule>())).Verifiable("Error ocuured while calling repository method");

            InsertLeadScoreRuleResponse response = leadScoreRuleService.CreateRule(new InsertLeadScoreRuleRequest() { LeadScoreRuleViewModel = viewModel });
            mockRepository.VerifyAll();
        }
        
        [TestMethod]
        public void InsertLeadScoreRule_RunTimeException_ExceptionDetails()
        {
            LeadScoreRuleViewModel leadScoreViewModel = LeadScoreRuleMockData.GetLeadscoreRuleViewModel();
            mockLeadScoreRuleRepository.Setup(c => c.Insert(It.IsAny<LeadScoreRule>())).Throws(new NullReferenceException());
            InsertLeadScoreRuleResponse response = leadScoreRuleService.CreateRule(new InsertLeadScoreRuleRequest() { LeadScoreRuleViewModel = leadScoreViewModel });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }
       
        [TestMethod]
        public void DeleteLeadScoreRule_ValidRule_Succeed()
        {
            mockLeadScoreRuleRepository.Setup(a => a.DeactivateRules(It.IsAny<int[]>()));
            DeleteLeadScoreResponse response = leadScoreRuleService.UpdateLeadScoreStatus(new DeleteLeadScoreRequest() { RuleID = leadScoreRulevalues });
            mockRepository.VerifyAll();
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void DeleteLeadScoreRule_RunTimeException_Failed()
        {
            mockLeadScoreRuleRepository.Setup(lsr => lsr.DeactivateRules(It.IsAny<int[]>())).Throws(new InvalidOperationException());
            DeleteLeadScoreResponse response = leadScoreRuleService.UpdateLeadScoreStatus(new DeleteLeadScoreRequest() { RuleID = leadScoreRulevalues });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(InvalidOperationException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateLeadScoreRule_ValidRule_Succeed()
        {
            IEnumerable<LeadScoreRuleViewModel> leadScorerulesList = LeadScoreRuleMockData.ListofLeadScoreRules();
            LeadScoreRuleViewModel model = leadScorerulesList.FirstOrDefault(lsr => lsr.LeadScoreRuleID == 1);
            mockLeadScoreRuleRepository.Setup(lsr => lsr.Update(It.IsAny<LeadScoreRule>())).Verifiable("Error ocuured while calling repository method");
            UpdateLeadScoreRuleResponse response = leadScoreRuleService.UpdateRule(new UpdateLeadScoreRuleRequest() { LeadScoreRuleViewModel = model });
            mockRepository.VerifyAll();
        }

        [TestMethod]
        public void GetCampaigns_ValidCampaigns_Success()
        {
            var campaigns = new List<Campaign>();
            campaigns.Add(new Campaign() { Id = 1, Name = "Campaign1", AccountID = 1 });
            campaigns.Add(new Campaign() { Id = 2, Name = "Campaign2", AccountID = 1 });
            mockLeadScoreRuleRepository.Setup(lsr => lsr.GetCampaigns(It.IsAny<int>())).Returns(campaigns);
            GetCampaignsResponse response = leadScoreRuleService.GetCampaigns(new GetCampaignsRequest() { AccountId = 1 });
            mockRepository.VerifyAll();
            Assert.AreEqual(campaigns.Count, response.Campaigns.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void UpdateLeadScoreRule_ExceptionOccured_Failed()
        {
            IEnumerable<LeadScoreRuleViewModel> leadScorerulesList = LeadScoreRuleMockData.ListofLeadScoreRules();
            LeadScoreRuleViewModel model = leadScorerulesList.FirstOrDefault(lsr => lsr.LeadScoreRuleID == 1);
            mockLeadScoreRuleRepository.Setup(lsr => lsr.Update(It.IsAny<LeadScoreRule>())).Throws(new NullReferenceException());
            UpdateLeadScoreRuleResponse response = leadScoreRuleService.UpdateRule(new UpdateLeadScoreRuleRequest() { LeadScoreRuleViewModel = model });
            mockRepository.VerifyAll();
            Assert.AreEqual(typeof(NullReferenceException), response.Exception.GetType());
            Assert.AreNotEqual(null, response.Exception);
        }        

        [TestMethod]
        public void GetForms_ValidForms_Success()
        {
            var forms = new List<Form>();
            forms.Add(new Form() { Id = 1, Name = "form1011", AccountID = 1 });
            forms.Add(new Form() { Id = 2, Name = "form1022", AccountID = 1 });
            mockLeadScoreRuleRepository.Setup(lsr => lsr.GetForms(It.IsAny<int>())).Returns(forms);
            GetFormResponse response = leadScoreRuleService.GetForms(new GetFormsRequest() { });
            mockRepository.VerifyAll();
            Assert.AreEqual(forms.Count, response.Forms.Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetConditions_ValidForms_Success()
        {
            var conditions = new List<Condition>();
            conditions.Add(new Condition() { Id = 1, Name = "form1011", Category = new ScoreCategories { Id = 1 } });
            conditions.Add(new Condition() { Id = 2, Name = "form1022", Category = new ScoreCategories { Id = 2 } });
            mockLeadScoreRuleRepository.Setup(lsr => lsr.GetConditions(It.IsAny<int>())).Returns(conditions);
            GetConditionsResponse response = leadScoreRuleService.GetConditions(new GetConditionsRequest(1));
            mockRepository.VerifyAll();
            //Assert.AreEqual(1, response.Conditions.Where(c => c.cate == 1).Count());
            Assert.AreEqual(null, response.Exception);
        }

        [TestMethod]
        public void GetLeadScoreCategories_ValidCategories_Success()
        {
            var categories = new List<ScoreCategories>();
            categories.Add(new ScoreCategories() { Id = 1, Name = "Campaigns" });
            categories.Add(new ScoreCategories() { Id = 2, Name = "Forms" });
            mockLeadScoreRuleRepository.Setup(lsr => lsr.GetScoreCategories()).Returns(categories);
            GetCategoriesResponse response = leadScoreRuleService.GetCategories(new GetCategoriesRequest() { });
            mockRepository.VerifyAll();
            Assert.AreEqual(categories.Count, response.Categories.Count());
            Assert.AreEqual(null, response.Exception);
        }
    }
}
