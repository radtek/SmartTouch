using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class NotifyUserState : State
    {
        IEnumerable<int> NotificationFieldIds;
        int WorkflowId;
        string WorkflowName;
        int CreatedBy;
        string messageBody;
        byte notifyType;
        readonly IWorkflowService workflowService;

        public NotifyUserState(IEnumerable<int> userIds, IEnumerable<int> NotificationFieldIds, int workflowId, string workflowName, int createdBy, int stateId, string message, byte notifyType, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityIds = userIds;
            this.NotificationFieldIds = NotificationFieldIds;
            this.WorkflowId = workflowId;
            this.WorkflowName = workflowName;
            this.CreatedBy = createdBy;
            this.messageBody = message;
            this.notifyType = notifyType;
            this.workflowService = workflowService;
            
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Notify a user state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))                
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside notify user state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside notify user state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Notify user enter." + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for processing notify a user state." + ", message: " + message.ToString());

            NotifyUserResponse response = workflowService.NotifyUser(new NotifyUserRequest()
            {
                AccountId = message.AccountId,
                Message = messageBody,
                NotifyType = notifyType,
                UserIds = EntityIds,
                NotificationFieldIds = NotificationFieldIds,
                ContactId = message.ContactId,
                trigger = (Entities.LeadScoreConditionType)message.LeadScoreConditionType,
                LinkEntityId = (Entities.LeadScoreConditionType)message.LeadScoreConditionType == LeadScoreConditionType.PageDuration ? message.EntityId : message.LinkedEntityId,  //In case of webvisit trigger we get Contactwebvisitid in to entityid
                WorkflowName = WorkflowName,
                EntityId = message.EntityId,
                WorkflowActionId = StateId,
                WorkflowId = WorkflowId
            });

            if (response.Exception == null)
                workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest()
                {
                    WorkflowId = WorkflowId,
                    WorkflowActionId = StateId,
                    ContactId = message.ContactId,
                    AccountId = message.AccountId,
                    MessageId = message.MessageId
                });
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Exit notify user." + ", message: " + message.MessageId);
        }
    }
}
