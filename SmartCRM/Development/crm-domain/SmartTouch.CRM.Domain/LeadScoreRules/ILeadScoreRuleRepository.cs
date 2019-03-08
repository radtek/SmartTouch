using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.LeadScoreRules
{
    public interface ILeadScoreRuleRepository : IRepository<LeadScoreRule, int>
    {
        IEnumerable<LeadScoreRule> FindAll(string name);
        void DeleteRule(int leadScoreId);
        IEnumerable<Condition> GetConditions(int categoryID);
        IEnumerable<ScoreCategories> GetScoreCategories();
        bool IsDuplicateRule(int accountID, LeadScoreConditionType conditionId, string conditionValue, int leadScoreRuleMapID);
        IEnumerable<LeadScoreRule> FindAll(string name, int limit, int pageNumber, int accountId,string sortField, ListSortDirection direction,IEnumerable<byte> Modules);
        IEnumerable<LeadScoreRule> FindAll(string name, int accountId);
        IEnumerable<Tag> GetTags(int[] values);
        void DeactivateRules(int[] ruleID);
        IEnumerable<LeadSource> GetLeadSources();
        IEnumerable<Campaign> GetCampaigns(int accountId);
        IEnumerable<Form> GetForms(int accountId);
        IEnumerable<Form> GetForms(int accountId, int? userID);
        LeadScoreRule GetLeadScoreRuleByCondition(int conditionId, string conditionValue, int accountId);
        IEnumerable<LeadScoreRule> GetLeadScoreRulesByCondition(int conditionId, string conditionValue, int accountId);
        IEnumerable<LeadScoreRule> GetLeadScoreRulesForLinkClicked(string conditionValue, int accountId,int EntityID);
        bool GetFormLeadScore(int[] formId, int accountID);
        int LeadScoreRulesCount(string name, int accountID,IEnumerable<byte> Modules);
        int GetMaxLeadScoreRuleID();
        bool InsertLeadScoreRules(LeadScoreRule leadScore,int LeadScoreRuleMapID);
        void UpdateLeadScoreRules(LeadScoreRule leadScoreRule);
        bool IsDuplicate(LeadScoreRule rule);
        List<byte> GetConditionsFromModules(IEnumerable<byte> UserModules);
        int[] GetLeadScoreTagIDs(int AccountID);
        IEnumerable<LeadScoreRule> GetPageDurationRules(int conditionId, string conditionValue, int accountId, int duration);
        IEnumerable<ScoreCategories> GetLeadScoreCategeories();
        IEnumerable<Condition> GetLeadScoreConditions();
    }
}
