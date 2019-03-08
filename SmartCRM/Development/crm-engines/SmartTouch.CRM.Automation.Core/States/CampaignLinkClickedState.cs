using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.Automation.Core.States
{
    public class CampaignLinkClickedState : State
    {
        public CampaignLinkClickedState(int campaignLinkId, int stateId) : base(stateId)        
        {
            this.EntityId = campaignLinkId;
        }

        public override WorkflowStateTransitionStatus Transit(Message message)
        {
            this.OnEntry(message);
            return this.TransitionState.Transit(message);
        }

        public override void OnEntry(Message message)
        {
            Console.WriteLine("Link Clicked entered."+ ", message: " + message.MessageId);
        }

        public override void OnExit(Message message)
        {
            Console.WriteLine("Link clicked state exit." + ", message: " + message.MessageId);
        }
    }
}
