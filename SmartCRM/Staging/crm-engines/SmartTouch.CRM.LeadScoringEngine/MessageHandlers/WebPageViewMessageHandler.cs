using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class WebPageViewMessageHandler : MessageHandler, IMessageHandler
    {
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactVisitsWebPage)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRule(
                new GetLeadScoreRuleByConditionRequest()
                {
                    AccountId = message.AccountId,
                    Condition = LeadScoreConditionType.ContactVisitsWebPage,
                    ConditionValue = message.ConditionValue
                });

                LeadScoreRule rule = leadScoreRuleResponse.Rule;

                if (rule == null)
                    return MessageHandlerStatus.LeadScoreRuleNotDefined;

                List<LeadScoreRule> rules = new List<LeadScoreRule>();
                rules.Add(rule);

                Logger.Current.Informational("Auditing the lead score for web page view for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactVisitsWebPage,
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
