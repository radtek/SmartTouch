using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class WebVisitMessageHandler : MessageHandler, IMessageHandler
    {
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactVisitsWebsite)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRule(
                new GetLeadScoreRuleByConditionRequest()
                {
                    AccountId = message.AccountId,
                    Condition = LeadScoreConditionType.ContactVisitsWebsite,
                    ConditionValue = message.ConditionValue
                });

                LeadScoreRule rule = leadScoreRuleResponse.Rule;

                if (rule == null)
                    return MessageHandlerStatus.LeadScoreRuleNotDefined;

                //var response = leadScoreService.IsScoreAudited(
                //    new LeadScoreAuditCheckRequest()
                //    {
                //        AccountId = message.AccountId,
                //        ContactId = message.ContactId,
                //        Condition = LeadScoreConditionType.ContactVisitsWebsite,
                //        ConditionValue = message.ConditionValue,
                //        EntityId = message.EntityId
                //    });

                //if (response.IsAudited)
                //    return MessageHandlerStatus.DuplicateLeadScoreRequest;

                List<LeadScoreRule> rules = new List<LeadScoreRule>();
                rules.Add(rule);
                Logger.Current.Informational("Auditing the lead score for web visit for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactVisitsWebsite,
                    ContactId = message.ContactId,
                    ConditionValue = message.ConditionValue,
                    AccountId = message.AccountId,
                    RequestedBy = message.UserId,
                    EntityId = message.EntityId,
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
