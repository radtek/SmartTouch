using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class SendEmailState : State
    {
        int WorkflowId;
        string Body;
        string Subject;
        int FromEmailId;

        readonly IWorkflowService workflowService;
        readonly ICommunicationService communicationService;

        public SendEmailState(int workflowId, int stateId, string body, string subject, int from, IWorkflowService workflowService, ICommunicationService communicationService)
            : base(stateId)
        {
            this.WorkflowId = workflowId;
            this.Body = body;
            this.Subject = subject;
            this.FromEmailId = from;
            this.workflowService = workflowService;
            this.communicationService = communicationService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            Logger.Current.Informational("Send an email state entered" + ", message: " + message.MessageId);
            if (message.LeadScoreConditionType != (int)LeadScoreConditionType.TriggerWorkflow && !canEnterState(message))                
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            try
            {
                Logger.Current.Verbose("Inside Send an email state for contactId " + message.ContactId + " in " + WorkflowId + ", message: " + message.MessageId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error occured inside send an email state for contact " + message.ContactId + " in workflow : " + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return this.TransitionState.Transit(message);
        }

        bool canEnterState(Message message)
        {
            Logger.Current.Informational("Request received to check whether contact can enter Send an email state" + ", message: " + message.MessageId);
            if (AllowedTriggers.Count == 0)
                return true;

            int entityId = default(int);
            if (message.LeadScoreConditionType == (int)Entities.LeadScoreConditionType.ContactClicksLink)
                entityId = message.LinkedEntityId;
            else
                entityId = message.EntityId;

            return AllowedTriggers.IsMatchTrigger(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("send an email state entered." + ", message: " + message.MessageId);
            Logger.Current.Informational("Request received for sending an email from workflow." + ", message: " + message.ToString());
            ScheduleMailResponse response = communicationService.ScheduleEmail(new ScheduleMailRequest()
            {
                ContactId = message.ContactId,
                RequestedBy = FromEmailId,
                AccountId = message.AccountId,
                SendMailViewModel = new ApplicationServices.ViewModels.SendMailViewModel()
                {
                    AccountID = message.AccountId,
                    Body = Body,
                    Subject = Subject,
                }
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
            Console.WriteLine("Campaign sent state exiting." + ", message: " + message.MessageId);
        }
    }
}
