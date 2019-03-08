using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class CampaignLinkClickMessageHandler : MessageHandler, IMessageHandler
    {
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactClicksLink)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetCampaignClickLeadScoreRule(
                 new GetLeadScoreRuleByConditionRequest()
                 {
                     AccountId = message.AccountId,
                     Condition = LeadScoreConditionType.ContactClicksLink,
                     ConditionValue = message.EntityId.ToString(),
                     EntityID = message.LinkedEntityId
                 });

                var rules = leadScoreRuleResponse.Rules;

                var response = leadScoreService.IsScoreAudited(
                    new LeadScoreAuditCheckRequest()
                    {
                        AccountId = message.AccountId,
                        ContactId = message.ContactId,
                        Condition = LeadScoreConditionType.ContactClicksLink,
                        ConditionValue = message.EntityId.ToString(),
                        EntityId = message.LinkedEntityId
                    });

                if (response.IsAudited)
                    return MessageHandlerStatus.DuplicateLeadScoreRequest;

                Logger.Current.Informational("Auditing the lead score for campaign link click for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactClicksLink,
                    ConditionValue = message.EntityId.ToString(),
                    ContactId = message.ContactId,
                    AccountId = message.AccountId,
                    RequestedBy = message.UserId,
                    EntityId = message.LinkedEntityId,
                    Rules = rules
                });

                return GetLeadScoreAuditStatus(message, insertScoreResponse.Exception);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while handling the Message.", ex);
                return MessageHandlerStatus.FailedToAuditScore;
            }
        }
    }
}
