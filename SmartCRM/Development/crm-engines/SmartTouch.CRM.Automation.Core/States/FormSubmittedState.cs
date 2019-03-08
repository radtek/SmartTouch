using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.Automation.Core.States
{
   public class FormSubmittedState : State
    {
       public FormSubmittedState(int formId, int stateId): base(stateId)
       {
           this.EntityId = formId;
       }

       public override WorkflowStateTransitionStatus Transit(Message message)
       {
           this.OnEntry(message);
           return this.TransitionState.Transit(message);
       }

       public override void OnEntry(Message message)
       {
           Console.WriteLine("Form submitted entered." + ", message: " + message.MessageId);
       }

       public override void OnExit(Message message)
       {
           Console.WriteLine("Form submitted exit." + ", message: " + message.MessageId);
       }
    }
}
