using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.LeadScore;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class AdjustLeadScoreState : State
    {
        int WorkflowId;
        int CreatedBy;
        int accountId;
        readonly ILeadScoreService leadScoreService;
        readonly IWorkflowService workflowService;

        public AdjustLeadScoreState(int leadScore, int workflowId, int createdBy, int accountId, int stateId,
            ILeadScoreService leadScoreService, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityId = leadScore;
            this.WorkflowId = workflowId;
            this.CreatedBy = createdBy;
            this.accountId = accountId;
            this.leadScoreService = leadScoreService;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Adjust leadscore state entered" + ", message: " + message.MessageId);

            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))               
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside adjust leadscore state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside adjust leadscore state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Adjust leadscore state entered");
            Logger.Current.Informational("Request received to adjust leadscore for contact, message: " + message.ToString());
            AdjustLeadscoreResponse response = leadScoreService.AdjustLeadscore(new AdjustLeadscoreRequest()
            {
                ContactId = message.ContactId,
                LeadScore = (short)EntityId,
                WorkflowActionId = StateId,
                AccountId = accountId
            });
            if (response.Exception == null)
                workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest() { WorkflowId = WorkflowId, WorkflowActionId = StateId, ContactId = message.ContactId, MessageId = message.MessageId });
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Adjust leadscore state exit");
        }
    }
}
