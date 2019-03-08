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
    public class TagAddedState : State
    {
        int WorkflowId;
        int CreatedBy;
        readonly ITagService tagService;
        readonly IWorkflowService workflowService;

        public TagAddedState(int tagId, int workflowId, int createdBy, int stateId, ITagService tagService, IWorkflowService workflowService)
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
            Logger.Current.Informational("AddTag state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))               
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside add tag state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside tag add state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Logger.Current.Informational("Request received for processing TagAdd state." + ", message: " + message.MessageId);
            WorkflowAddTagResponse response = tagService.AddTag(new WorkflowAddTagRequest()
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
            Console.WriteLine("Tag added exit." + ", message: " + message.MessageId);
        }
    }
}
