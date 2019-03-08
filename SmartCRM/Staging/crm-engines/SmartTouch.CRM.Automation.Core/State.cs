using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Entities;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Workflows;

namespace SmartTouch.CRM.Automation.Core
{
    public abstract class State
    {
        protected State(int stateId)
        {
            this.StateId = stateId;
            this.AllowedTriggers = new Dictionary<LeadScoreConditionType, WorkflowTrigger>();
        }

        /// <summary>
        /// Sibling state.
        /// </summary>
        public State TransitionState { get; set; }

        /// <summary>
        /// Sub or child workflow state.
        /// </summary>
        public State SubState { get; set; }

        public abstract WorkflowStateTransitionStatus Transit(Message message);

        public int StateId { get; set; }
        public int EntityId { get; set; }
        public IEnumerable<int> EntityIds { get; set; }

        public abstract void OnEntry(Message message);
        public abstract void OnExit(Message message);

        /// <summary>
        /// LeadScoreConditionType indicates the action performed on the contact. 
        /// Todo: Need to change this enum to be more generically used across application whereever messages are being consumed.
        /// </summary>
        public IDictionary<LeadScoreConditionType, WorkflowTrigger> AllowedTriggers { get; set; }

        public virtual bool CanEnterState(Message message)
        {
            Logger.Current.Informational("Request received to check whether contact can enter change leadscore state, message: " + message.MessageId);
            if (AllowedTriggers.Count == 0) return true;

            //var entityId = (message.LeadScoreConditionType == (int)Entities.LeadScoreConditionType.ContactClicksLink) ? message.LinkedEntityId : message.EntityId;

            return AllowedTriggers.IsMatchTrigger(message);
        }
    }
}
