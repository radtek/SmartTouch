using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class AssignUserState : State
    {
        int WorkflowId;
        int CreatedBy;
        byte ScheduledID;
        readonly IContactService contactService;
        readonly IWorkflowService workflowService;

        public AssignUserState(int userAssignmentActionID, byte scheduledID, int workflowId, int createdBy, int stateId, IContactService contactService, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityId = userAssignmentActionID;
            this.WorkflowId = workflowId;
            this.ScheduledID = scheduledID;
            this.CreatedBy = createdBy;
            this.contactService = contactService;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Assign a user state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))              
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside assign user state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside assign user state for contact " + message.ContactId + " in workflow : " + WorkflowId + ", messageId : " + message.MessageId);
                //Logger.Current.Error("An error occured while adjusting leadscore for a contact from workflow " + ex);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Assign a user state entered" + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for assigning a user for a contact" + ", message: " + message.ToString());
            AssignUserResponse response = workflowService.AssignUser(new AssignUserRequest() { ContactId = message.ContactId, AccountId = message.AccountId, userAssignmentActionID = EntityId, ScheduledID = ScheduledID });
            if (response.Exception == null)
                workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest() { WorkflowId = WorkflowId, WorkflowActionId = StateId, ContactId = message.ContactId, MessageId = message.MessageId });
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Assign a user state exit" + ", message: " + message.MessageId);
        }
    }
}
