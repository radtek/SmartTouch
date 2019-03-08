//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Stateless;
//using SmartTouch.CRM.Automation.Core.States;
//using SmartTouch.CRM.MessageQueues;
//using SmartTouch.CRM.Domain.Workflows;
//using SmartTouch.CRM.Entities;

//namespace SmartTouch.CRM.Automation.Core
//{
//    public class AutomationWorkflow
//    {
//        public int AccountId { get; set; }
//        public int WorkflowId { get; set; }

//        StateMachine<State, WorkflowTriggerType> stateMachine;
//        public delegate bool GuardClauseDelegate();
//        public delegate void UnhandledTriggerDelegate(State state, WorkflowTrigger trigger);

//        public GuardClauseDelegate IsHandledBySubState = null;
//        public GuardClauseDelegate IsHandledBySiblingState = null;
//        public UnhandledTriggerDelegate OnUnhandledTrigger = null;

//        IList<StateMachine<State, WorkflowTriggerType>.TriggerWithParameters<Message>> automationTriggers =
//            new List<StateMachine<State, WorkflowTriggerType>.TriggerWithParameters<Message>>();

//        public AutomationWorkflow(Workflow workflow)
//        {
//            Configure(workflow.WorkflowActions, workflow.Triggers);
//        }

//        void Configure(IEnumerable<WorkflowAction> workflowActions, IEnumerable<WorkflowTrigger> workflowTriggers)
//        {
//            IList<State> beginStates = new List<State>();
//            IList<State> transitionStates = new List<State>();
//            var initialState = new WorkflowBeginState();

//            foreach (var workflowTrigger in workflowTriggers.Where(t => t.IsStartTrigger))
//            {
//                State state = null;

//                if (workflowTrigger.TriggerTypeID == WorkflowTriggerType.FormSubmitted)
//                    state = new FormSubmittedState(workflowTrigger.FormID.Value);
//                else if (workflowTrigger.TriggerTypeID == WorkflowTriggerType.TagApplied)
//                    state = new FormSubmittedState(workflowTrigger.TagID.Value);

//                state.AssociatedTriggerType = workflowTrigger.TriggerTypeID;

//                if (beginStates.Count == 0)
//                    stateMachine = new StateMachine<State, WorkflowTriggerType>(initialState);

//                var autoTrigger = stateMachine.SetTriggerParameters<Message>(workflowTrigger.TriggerTypeID);
//                automationTriggers.Add(autoTrigger);

//                beginStates.Add(state);
//            }

//            foreach (var workflowAction in workflowActions.OrderBy(a => a.OrderNumber))
//            {
//                State state = null;

//                if (workflowAction.WorkflowActionTypeID == WorkflowActionType.AddTag)
//                {
//                    state = new TagAddedState((workflowAction.Action as WorkflowTagAction).TagID);
//                    state.AssociatedTriggerType = WorkflowTriggerType.TagApplied;
//                }
//                else if (workflowAction.WorkflowActionTypeID == WorkflowActionType.RemoveTag)
//                {
//                    state = new TagRemovedState((workflowAction.Action as WorkflowTagAction).TagID);
//                    state.AssociatedTriggerType = WorkflowTriggerType.TagRemoved;
//                }

//                State lastState = null;
//                if (transitionStates.Count == 0)
//                {
//                    lastState = initialState;
//                    foreach (var beginState in beginStates)
//                    {
//                        stateMachine.Configure(beginState)
//                            .PermitIf(beginState.AssociatedTriggerType, state, () => beginState.EntityId == message.EntityId);
//                    }
//                }
//                else
//                {
//                    lastState = transitionStates.Last();
//                    stateMachine.Configure(lastState)
//                        .OnEntryFrom(automationTriggers.Last(), m => stateMachine.State.OnEntry(m))
//                        .Permit(lastState.AssociatedTriggerType, state);
//                }

//                var autoTrigger = stateMachine.SetTriggerParameters<Message>(state.AssociatedTriggerType);
//                automationTriggers.Add(autoTrigger);

//                transitionStates.Add(state);
//            }
//        }

//        Message message = null;
//        public bool TryFireTrigger(WorkflowTriggerType trigger, Message message)
//        {
//            this.message = message;
//            //if (!stateMachine.CanFire(trigger))
//            //{
//            //    return false;
//            //}
//            foreach (var automationTrigger in automationTriggers.Where(a => a.Trigger == trigger))
//            {
//                stateMachine.Fire<Message>(automationTrigger, message);
//            }
//            return true;
//        }
//    }
//}
