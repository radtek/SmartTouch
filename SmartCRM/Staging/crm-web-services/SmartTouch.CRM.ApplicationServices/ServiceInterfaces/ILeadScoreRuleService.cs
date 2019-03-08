using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface ILeadScoreRuleService
    {
        InsertLeadScoreRuleResponse CreateRule(InsertLeadScoreRuleRequest request);
        GetLeadScoreListResponse GetLeadScoresList(GetLeadScoreListRequest request);
        UpdateLeadScoreRuleResponse UpdateRule(UpdateLeadScoreRuleRequest request);
        DeleteLeadScoreResponse DeleteRule(DeleteLeadScoreRequest request);
        GetLeadScoreResponse GetLeadScoreRule(GetLeadScoreRequest request);
        GetCategoriesResponse GetCategories(GetCategoriesRequest request);
        GetConditionsResponse GetConditions(GetConditionsRequest request);
        DeleteLeadScoreResponse UpdateLeadScoreStatus(DeleteLeadScoreRequest request);
        GetCampaignsResponse GetCampaigns(GetCampaignsRequest request);
        //GetLeadSourceResponse GetLeadSources(GetLeadSourceRequest request);
        GetFormResponse GetForms(GetFormsRequest request);
        GetLeadScoreRuleByConditionResponse GetLeadScoreRule(GetLeadScoreRuleByConditionRequest request);
        GetLeadScoreRuleByConditionResponse GetLeadScoreRules(GetLeadScoreRuleByConditionRequest request);
        GetLeadScoreRuleByConditionResponse GetCampaignClickLeadScoreRule(GetLeadScoreRuleByConditionRequest request);
        GetLeadScoreCategoriesResponse GetLeadScoreCategories(GetLeadScoreCategoriesRequest request);
        GetLeadScoreConditionsResponse GetLeadScoreConditions(GetLeadScoreConditionsRequest request);
    }
}
