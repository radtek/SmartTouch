using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class SendCampaignState : State
    {
        int WorkflowId;
        int CreatedBy;
        IEnumerable<int> links;

        readonly ICampaignService campaignService;
        readonly IWorkflowService workflowService;

        public SendCampaignState(int campaignId, int workflowId, int createdBy, int stateId, IEnumerable<int> links,
            ICampaignService campaignService, IWorkflowService workflowService)
            : base(stateId)
        {
            this.EntityId = campaignId;
            this.WorkflowId = workflowId;
            this.CreatedBy = createdBy;
            this.campaignService = campaignService;
            this.workflowService = workflowService;
            this.links = links;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Send a campaign state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !CanEnterState(message))                
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside Send campaign state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside send a campaign state for contact " + message.ContactId + " in workflow :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }
        public override void OnEntry(Message message)
        {
            Console.WriteLine("Campaign sent state entered." + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for sending a campaign from workflow." + ", message: " + message.ToString());
            InsertCampaignRecipientResponse response = campaignService.InsertCampaignRecipients(new InsertCampaignRecipientRequest()
            {
                ContactId = message.ContactId,
                WorkflowId = WorkflowId,
                CampaignId = EntityId,
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
            Console.WriteLine("Send campaign state exiting." + ", message: " + message.MessageId);
        }
    }
}
