using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class LifecycleChangedState : State
    {
        int WorkflowId;
        int CreatedBy;
        readonly IContactService contactService;
        readonly IWorkflowService workflowService;

        public LifecycleChangedState(int lifeCycleTypeId, int workflowId, int createdBy, int stateId, IContactService contactService, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityId = lifeCycleTypeId;
            this.WorkflowId = workflowId;
            this.CreatedBy = createdBy;
            this.contactService = contactService;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Lifecycle change state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))              
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside lifecycle change state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside life-cycle change state for contact " + message.ContactId + "  workflow : " + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Logger.Current.Informational("Request received for changing lifeycle for a contact" + ", message: " + message.ToString());
            ChangeLifecycleResponse response = contactService.ChangeLifecycle(new ChangeLifecycleRequest() { ContactId = message.ContactId, dropdownValueId = (short)EntityId, AccountId = message.AccountId });
            if (response.Exception == null)
                workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest() { WorkflowId = WorkflowId, WorkflowActionId = StateId, ContactId = message.ContactId, MessageId = message.MessageId });
        }

        public override void OnExit(Message message)
        {
        }
    }
}
