﻿using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class TourTypeMessageHandler : MessageHandler, IMessageHandler
    {        
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactTourType)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRules(
                 new GetLeadScoreRuleByConditionRequest()
                 {
                     AccountId = message.AccountId,
                     Condition = LeadScoreConditionType.ContactTourType,
                     ConditionValue = message.LinkedEntityId.ToString()
                 });

                //LeadScoreRule rule = leadScoreRuleResponse.Rule;

                //if (rule == null)
                //    return MessageHandlerStatus.LeadScoreRuleNotDefined;

                var response = leadScoreService.IsScoreAudited(
                    new LeadScoreAuditCheckRequest()
                    {
                        AccountId = message.AccountId,
                        ContactId = message.ContactId,
                        Condition = LeadScoreConditionType.ContactTourType,
                        ConditionValue = message.LinkedEntityId.ToString(),
                        EntityId = message.EntityId
                    });

                if (response.IsAudited)
                    return MessageHandlerStatus.DuplicateLeadScoreRequest;

                Logger.Current.Informational("Auditing the lead score for contact note tag for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactTourType,
                    ContactId = message.ContactId,
                    ConditionValue = message.LinkedEntityId.ToString(),
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
