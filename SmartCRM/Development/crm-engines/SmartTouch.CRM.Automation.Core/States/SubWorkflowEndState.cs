using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class SubWorkflowEndState : State
    {
        public SubWorkflowEndState()
            : base(0)
        { }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            this.OnEntry(message);
            return WorkflowStateTransitionStatus.TransitedSuccessfully;
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Sub Workflow ended." + ", message: " + message.MessageId);
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Sub Workflow end state exiting." + ", message: " + message.MessageId);
        }
    }
}
