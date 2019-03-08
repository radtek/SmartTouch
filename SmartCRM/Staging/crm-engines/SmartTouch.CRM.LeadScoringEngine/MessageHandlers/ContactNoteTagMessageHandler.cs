using System;
using SmartTouch.CRM.Entities;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.LeadScoreRules;
using System.Collections.Generic;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    internal class ContactNoteTagMessageHandler : IMessageHandler
    {
        readonly ILeadScoreRuleService leadScoreRuleService;
        readonly ILeadScoreService leadScoreService;

        public ContactNoteTagMessageHandler()
        {
            this.leadScoreRuleService = IoC.Container.GetInstance<ILeadScoreRuleService>();
            this.leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
        }

        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactNoteTagAdded)
                    return MessageHandlerStatus.InvalidMessageHandler;

                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRules(
                   new GetLeadScoreRuleByConditionRequest()
                   {
                       AccountId = message.AccountId,
                       Condition = LeadScoreConditionType.ContactNoteTagAdded,
                       ConditionValue = message.LinkedEntityId.ToString()
                   });

                var rules = leadScoreRuleResponse.Rules;

                var response = leadScoreService.IsScoreAudited(
                   new LeadScoreAuditCheckRequest()
                   {
                       AccountId = message.AccountId,
                       ContactId = message.ContactId,
                       Condition = LeadScoreConditionType.ContactNoteTagAdded,
                       ConditionValue = message.LinkedEntityId.ToString(),
                       EntityId = message.EntityId
                   });

                if (response.IsAudited)
                    return MessageHandlerStatus.DuplicateLeadScoreRequest;  
                              
                Logger.Current.Informational("Auditing the lead score for contact note tag for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactNoteTagAdded,
                    ContactId = message.ContactId,
                    ConditionValue = message.LinkedEntityId.ToString(),
                    AccountId = message.AccountId,
                    RequestedBy = message.UserId,
                    EntityId = message.EntityId,
                    Rules = rules
                });

                if (insertScoreResponse.Exception == null)
                {
                    Logger.Current.Informational("Audited successfully." + message.MessageId);                   
                    return MessageHandlerStatus.LeadScoreAuditedSuccessfully;
                }
                else
                {
                    Logger.Current.Error("Error occurred while auditing the lead score." + message.MessageId, insertScoreResponse.Exception);
                    return MessageHandlerStatus.FailedToAuditScore;
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while handling the Message.", ex);
                return MessageHandlerStatus.FailedToAuditScore;
            }
        }
    }
}
