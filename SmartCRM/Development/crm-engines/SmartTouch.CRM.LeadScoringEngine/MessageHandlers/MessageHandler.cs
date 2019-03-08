using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using System;

namespace SmartTouch.CRM.LeadScoringEngine.MessageHandlers
{
    public abstract class MessageHandler
    {
        protected readonly ILeadScoreRuleService leadScoreRuleService;
        protected readonly ILeadScoreService leadScoreService;

        public MessageHandler()
        {
            this.leadScoreRuleService = IoC.Container.GetInstance<ILeadScoreRuleService>();
            this.leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
        }

        protected MessageHandlerStatus GetLeadScoreAuditStatus(MessageQueues.Message message, Exception exception = null)
        {
            if (exception == null)
            {
                Logger.Current.Informational("Audited successfully." + message.MessageId);
                return MessageHandlerStatus.LeadScoreAuditedSuccessfully;
            }
            else
            {
                Logger.Current.Error("Error occurred while auditing the lead score." + message.MessageId, exception);
                return MessageHandlerStatus.FailedToAuditScore;
            }
        }
    }
}
