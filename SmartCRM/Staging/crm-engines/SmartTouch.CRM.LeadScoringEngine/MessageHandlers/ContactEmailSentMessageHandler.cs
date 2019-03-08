using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    internal class ContactEmailSentMessageHandler : MessageHandler, IMessageHandler
    {        
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.AnEmailSent)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRule(
                    new GetLeadScoreRuleByConditionRequest()
                    {
                        AccountId = message.AccountId,
                        Condition = LeadScoreConditionType.AnEmailSent,
                        ConditionValue = null
                    });

                LeadScoreRule rule = leadScoreRuleResponse.Rule;
                if (rule == null)
                    return MessageHandlerStatus.LeadScoreRuleNotDefined;

                List<LeadScoreRule> rules = new List<LeadScoreRule>();
                rules.Add(leadScoreRuleResponse.Rule);
                
                Logger.Current.Informational("Auditing the lead score for contact email sent:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.AnEmailSent,
                    ConditionValue = message.LinkedEntityId.ToString(),
                    ContactId = message.ContactId,
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
