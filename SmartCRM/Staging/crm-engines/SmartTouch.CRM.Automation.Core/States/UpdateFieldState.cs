using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class UpdateFieldState : State
    {
        int WorkflowId;

        string fieldValue;

        int fieldInputType;

        readonly IContactService contactService;

        readonly IWorkflowService workflowService;

        public UpdateFieldState(int fieldId, int workflowId, int stateId, string fieldValue, int fieldInputType, IContactService contactService, IWorkflowService workflowService) : base(stateId)
        {
            this.EntityId = fieldId;
            this.WorkflowId = workflowId;
            this.fieldInputType = fieldInputType;
            this.fieldValue = fieldValue;
            this.contactService = contactService;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Update a field state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))
                return WorkflowStateTransitionStatus.UnAuthorizedTransition;
            try
            {
                Logger.Current.Verbose("Inside update a field state for contactId " + message.ContactId + " in " + WorkflowId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside update a field state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Update a field enter.");
            Logger.Current.Informational("Request received for processing update a field state." + ", message: " + message.ToString());
            var response = workflowService.UpdateContactField(new UpdateContactFieldRequest()
            {
                FieldID = EntityId,
                FieldValue = fieldValue,
                AccountId = message.AccountId,
                ContactID = message.ContactId,
                FieldInputTypeID = fieldInputType
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
            Console.WriteLine("Exit update a field state." + ", message: " + message.MessageId);
        }
    }
}