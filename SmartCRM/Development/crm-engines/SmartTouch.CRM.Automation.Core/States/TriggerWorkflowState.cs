using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class TriggerWorkflowState : State
    {
        int WorkflowId;

        readonly IWorkflowService workflowService;
        IMessageService messageService;

        public TriggerWorkflowState(int workflowId, int siblingWorkflowId, int stateId, IWorkflowService workflowService, IMessageService messageService)
            : base(stateId)
        {
            this.EntityId = siblingWorkflowId;
            this.WorkflowId = workflowId;
            this.workflowService = workflowService;
            this.messageService = messageService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Trigger a workflow state entered" + ", message: " + message.MessageId);
            if (!CanEnterState(message))
                return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside trigger a workflow state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside trigger a workflow state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Trigger a workflow enter." + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for processing trigger a workflow state." + ", message: " + message.ToString());

            scheduleMessage(message);

            workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest()
            {
                WorkflowId = WorkflowId,
                WorkflowActionId = StateId,
                ContactId = message.ContactId,
                AccountId = message.AccountId,
                MessageId = message.MessageId
            });
        }

        void scheduleMessage(Message message)
        {
            TrackMessage newMessage = new TrackMessage();
            newMessage.LeadScoreConditionType = (int)LeadScoreConditionType.TriggerWorkflow;
            newMessage.AccountId = message.AccountId;
            newMessage.ContactId = message.ContactId;
            newMessage.EntityId = EntityId;
            messageService.SendMessages(new ApplicationServices.Messaging.Messages.SendMessagesRequest()
            {
                Message = newMessage
            });
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Exit notify user." + ", message: " + message.MessageId);
        }
    }
}
