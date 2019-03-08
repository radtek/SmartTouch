using System;
using SmartTouch.CRM.Entities;

using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.LeadScoreRules;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class FormSubmissionMessageHandler : IMessageHandler
    {
        readonly ILeadScoreRuleService leadScoreRuleService;
        readonly ILeadScoreService leadScoreService;

        public FormSubmissionMessageHandler()
        {
            this.leadScoreRuleService = IoC.Container.GetInstance<ILeadScoreRuleService>();
            this.leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
        }

        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactSubmitsForm)
                    return MessageHandlerStatus.InvalidMessageHandler;

                //var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRule(
                //new GetLeadScoreRuleByConditionRequest()
                //{
                //    AccountId = message.AccountId,
                //    Condition = LeadScoreConditionType.ContactSubmitsForm,
                //    ConditionValue = message.EntityId.ToString()
                //});

                //LeadScoreRule rule = leadScoreRuleResponse.Rule;
                GetLeadScoreRuleByConditionRequest request = new GetLeadScoreRuleByConditionRequest()
                {
                    AccountId = message.AccountId,
                    Condition = LeadScoreConditionType.ContactSubmitsForm,
                    ConditionValue = message.EntityId.ToString()
                };


                GetLeadScoreRuleByConditionResponse response = leadScoreRuleService.GetLeadScoreRules(request);
                var rules = response.Rules;

                Logger.Current.Informational("Auditing the lead score for form submission for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.ContactSubmitsForm,
                    ContactId = message.ContactId,
                    ConditionValue = message.EntityId.ToString(),
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
               // return MessageHandlerStatus.LeadScoreAuditedSuccessfully;
                //var response = leadScoreService.IsScoreAudited(
                //    new LeadScoreAuditCheckRequest()
                //    {
                //        AccountId = message.AccountId,
                //        ContactId = message.ContactId,
                //        Condition = LeadScoreConditionType.ContactSubmitsForm,
                //        ConditionValue = message.EntityId.ToString(),
                //        EntityId = message.EntityId
                //    });

                //if (response.IsAudited)
                //    return MessageHandlerStatus.DuplicateLeadScoreRequest;



            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while handling the Message.", ex);
                return MessageHandlerStatus.FailedToAuditScore;
            }
        }
    }
}
