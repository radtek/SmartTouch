﻿using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    internal class CampaignOpenMessageHandler : MessageHandler, IMessageHandler
    {        
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactOpensEmail)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRules(
                 new GetLeadScoreRuleByConditionRequest()
                 {
                     AccountId = message.AccountId,
                     Condition = LeadScoreConditionType.ContactOpensEmail,
                     ConditionValue = message.EntityId.ToString()
                 });

                //LeadScoreRule rule = leadScoreRuleResponse.Rule;

                //if (rule == null)
                //    return MessageHandlerStatus.LeadScoreRuleNotDefined;

                var response = leadScoreService.IsScoreAudited(
                    new LeadScoreAuditCheckRequest()
                    {
                        AccountId = message.AccountId,
                        ContactId = message.ContactId,
                        Condition = LeadScoreConditionType.ContactOpensEmail,
                        ConditionValue = message.EntityId.ToString(),
                        EntityId = message.EntityId
                    });

                if (response.IsAudited)
                    return MessageHandlerStatus.DuplicateLeadScoreRequest;              

                Logger.Current.Informational("Auditing the lead score for campaign email open for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactOpensEmail,
                    ConditionValue = message.EntityId.ToString(),
                    ContactId = message.ContactId,
                    AccountId = message.AccountId,
                    RequestedBy = message.UserId,
                    EntityId = message.EntityId,
                    Rules = leadScoreRuleResponse.Rules
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
