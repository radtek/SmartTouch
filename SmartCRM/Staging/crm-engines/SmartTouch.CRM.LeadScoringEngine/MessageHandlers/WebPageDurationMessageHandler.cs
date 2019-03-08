using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public class WebPageDurationMessageHandler : MessageHandler, IMessageHandler
    {
        public MessageHandlerStatus Handle(MessageQueues.Message message)
        {
            try
            {
                if (message.LeadScoreConditionType != (int)LeadScoreConditionType.PageDuration)
                    return MessageHandlerStatus.InvalidMessageHandler;
                var conditionValues = new List<LeadScoreConditionValueViewModel>();
                conditionValues.Add(new LeadScoreConditionValueViewModel()
                {
                    Value = message.LinkedEntityId.ToString(),
                    ValueType = LeadScoreValueType.PageDuration
                });
                var leadScoreRuleResponse = leadScoreRuleService.GetLeadScoreRules(
                    new GetLeadScoreRuleByConditionRequest()
                    {
                        AccountId = message.AccountId,
                        Condition = LeadScoreConditionType.PageDuration,
                        ConditionValue = message.ConditionValue
                    });
                Logger.Current.Informational("Auditing the lead score for A PAGE DURATION OF for message:" + message.MessageId);
                var insertScoreResponse = leadScoreService.InsertLeadScore(new InsertLeadScoreRequest()
                {
                    Condition = LeadScoreConditionType.PageDuration,
                    ContactId = message.ContactId,
                    ConditionValue = message.EntityId.ToString(),
                    AccountId = message.AccountId,
                    RequestedBy = message.UserId,
                    EntityId = message.EntityId,
                    Rules = leadScoreRuleResponse.Rules
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
