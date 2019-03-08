using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.Domain.LeadScoreRules;

using Moq;
using Moq.Linq;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Tests.LeadScoreRules
{
    public class LeadScoreRuleMockData
    {
        public static IEnumerable<LeadScoreRule> GetMockLeadScoreRules(MockRepository mockRepository, int objectCount)
        {
            IList<LeadScoreRule> mockRules = new List<LeadScoreRule>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockRule = mockRepository.Create<LeadScoreRule>();
                mockRules.Add(mockRule.Object);
            }
            return mockRules;
        }

        public static IEnumerable<Mock<LeadScoreRule>> GetMockLeadScoreRulesWithSetups(MockRepository mockRepository, int objectCount)
        {
            IList<Mock<LeadScoreRule>> mockLeadScoreRules = new List<Mock<LeadScoreRule>>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockLeadScoreRule = mockRepository.Create<LeadScoreRule>();
                mockLeadScoreRule.Setup<int>(c => c.Id).Returns(i);
                mockLeadScoreRules.Add(mockLeadScoreRule);
            }
            return mockLeadScoreRules;
        }

        public static IEnumerable<LeadScoreRuleViewModel> GetMockLeadScoreRulesViewModels(MockRepository mockRepository, int objectCount)
        {
            IList<LeadScoreRuleViewModel> mockLeadScoreRules = new List<LeadScoreRuleViewModel>();
            foreach (int i in Enumerable.Range(1, objectCount))
            {
                var mockLeadScoreRule = mockRepository.Create<LeadScoreRuleViewModel>();
                mockLeadScoreRules.Add(mockLeadScoreRule.Object);
            }
            return mockLeadScoreRules;
        }

        public static IEnumerable<LeadScoreRuleViewModel> ListofLeadScoreRules()
        {
            IList<LeadScoreRuleViewModel> mockRules = new List<LeadScoreRuleViewModel>();
            mockRules.Add(new LeadScoreRuleViewModel() { LeadScoreRuleID = 1, ConditionDescription = "Samsung",Condition = new Condition() { Id = 1 }, Score = 10, ConditionValue = "facebook" });
            mockRules.Add(new LeadScoreRuleViewModel() { LeadScoreRuleID = 2, ConditionDescription = "Sony", Condition = new Condition() { Id = 1 }, Score = 10, ConditionValue = "Twitter" });
            mockRules.Add(new LeadScoreRuleViewModel() { LeadScoreRuleID = 3, ConditionDescription = "Onida",Condition = new Condition() { Id = 1 }, Score = 10, ConditionValue = "GooglePlus" });
            return mockRules;
        }

        public static LeadScoreViewModel GetMockLeadScoreViewModel(MockRepository mockRepository, int Id)
        {
            var mockLeadScoreRule = new LeadScoreViewModel();
            mockLeadScoreRule.LeadScoreRuleID = 1;
            return mockLeadScoreRule;
        }

        public static LeadScoreRuleViewModel GetLeadscoreRuleViewModel()
        {
            CampaignEntryViewModel campaign = new CampaignEntryViewModel() { Id = 1, Name = "This is Sample" };

            LeadScoreRuleViewModel leadScoreViewModel = new LeadScoreRuleViewModel();
            leadScoreViewModel.ConditionDescription = "Sample";
            leadScoreViewModel.Condition = new Condition() { Id = 1 };
            leadScoreViewModel.Campaigns = campaign;            
            leadScoreViewModel.ConditionValue = "1";
            leadScoreViewModel.AccountID = 1;
            leadScoreViewModel.Score = 100;
            return leadScoreViewModel;
        }
    }
}
