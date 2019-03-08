using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class TagRemovedState : State
    {
        int WorkflowId;
        int CreatedBy;
        readonly ITagService tagService;
        readonly IWorkflowService workflowService;
        public TagRemovedState(int tagId, int workflowId, int createdBy, int stateId, ITagService tagService, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityId = tagId;
            this.WorkflowId = workflowId;
            this.CreatedBy = createdBy;
            this.tagService = tagService;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("RemoveTag state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))              
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside tag remove state for contactId " + message.ContactId + " in " + WorkflowId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside tag remove state for contact " + message.ContactId + " in workflow : " + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Removed tag entered" + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for processing TagRemove state." + ", message: " + message.ToString());
            WorkflowRemoveTagResponse response = tagService.RemoveTag(new WorkflowRemoveTagRequest()
            {
                TagId = EntityId,
                ContactId = message.ContactId,
                CreatedBy = CreatedBy,
                AccountId = message.AccountId
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
            Console.WriteLine("Removed tag exit" + ", message: " + message.MessageId);
        }
    }
}
