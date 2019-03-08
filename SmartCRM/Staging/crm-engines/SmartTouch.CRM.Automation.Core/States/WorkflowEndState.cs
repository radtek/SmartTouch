using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class WorkflowBeginState : State
    {
        public WorkflowBeginState()
            : base(0)
        { }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            this.OnEntry(message);
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Workflow begins." + ", message: " + message.MessageId);
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Workflow begin state exiting." + ", message: " + message.MessageId);
        }
    }

    public class WorkflowEndState : State
    {
        int WorkflowId;
        readonly IWorkflowService workflowService;
   
        public WorkflowEndState(int workflowId,int stateId, IWorkflowService workflowService)
            : base(stateId)
        {
            this.WorkflowId = workflowId;
            this.workflowService = workflowService;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            try
            {
                Logger.Current.Informational("Inside workflow end state for contact " + message.ContactId + " in workflowId :" + WorkflowId);
                this.OnEntry(message);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured inside workflow end state for contact " + message.ContactId + " with workflowId :" + WorkflowId + ", messageId : " + message.MessageId);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return WorkflowStateTransitionStatus.TransitionFailed;
            }
            return WorkflowStateTransitionStatus.TransitedSuccessfully;
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Workflow ended." + ", message: " + message.MessageId);
            Logger.Current.Informational("Update contact workflow audit - workflow end state." + ", message: " + message.ToString());
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
            Console.WriteLine("Workflow end state exiting." + ", message: " + message.MessageId);
        }
    }
}
