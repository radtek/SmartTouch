using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stateless;
using SmartTouch.CRM.Automation.Core.States;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;

namespace SmartTouch.CRM.Automation.Core
{
    public class AutomationWorkflow
    {
        Workflow workflow;
        public int AccountId { get; private set; }
        public int WorkflowId { get; private set; }
        IWorkflowService workflowService;
        ITagService tagService;
        ICampaignService campaignService;
        IContactService contactService;
        ILeadScoreService leadScoreService;
        IPublishSubscribeService pubSubService;
        ICommunicationService communicationService;
        public IList<State> states = new List<State>();
        public IDictionary<LeadScoreConditionType, WorkflowTrigger> stopTriggers { get; set; }
        public IDictionary<LeadScoreConditionType, WorkflowTrigger> allowedTriggers { get; set; }

        public AutomationWorkflow(Workflow workflow, IWorkflowService workflowService, ITagService tagService,
            ICampaignService campaignService, IContactService contactService, ILeadScoreService leadScoreService, IPublishSubscribeService pubSubService,
            ICommunicationService communicationService)
        {
            this.workflow = workflow;
            this.AccountId = workflow.AccountID;
            this.WorkflowId = workflow.WorkflowID;
            this.workflowService = workflowService;
            this.tagService = tagService;
            this.campaignService = campaignService;
            this.contactService = contactService;
            this.leadScoreService = leadScoreService;
            this.pubSubService = pubSubService;
            this.communicationService = communicationService;
            this.configure();
        }

        void configure()
        {
            var triggersGroup = workflow.Triggers.GroupBy(t => new { t.TriggerTypeID, t.IsStartTrigger }, (key, g) =>
                new { triggerType = key.TriggerTypeID, triggers = g, IsStartTrigger = g.FirstOrDefault().IsStartTrigger });

            allowedTriggers = new Dictionary<LeadScoreConditionType, WorkflowTrigger>();

            stopTriggers = new Dictionary<LeadScoreConditionType, WorkflowTrigger>();

            Logger.Current.Informational("Configuring triggers for workflow :" + WorkflowId);
            foreach (var trigger in triggersGroup)
            {
                if (trigger.triggerType == WorkflowTriggerType.FormSubmitted)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactSubmitsForm, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactSubmitsForm, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.Campaign)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.CampaignSent, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.CampaignSent, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.LifecycleChanged)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactLifecycleChange, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactLifecycleChange, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.SmartSearch)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactMatchesSavedSearch, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactMatchesSavedSearch, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.TagApplied)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactTagAdded, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactTagAdded, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.TagRemoved)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactTagRemoved, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactTagRemoved, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.OpportunityStatusChanged)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.OpportunityStatusChanged, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.OpportunityStatusChanged, trigger.triggers.FirstOrDefault());

                else if (trigger.triggerType == WorkflowTriggerType.LeadAdapterSubmitted)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.LeadAdapterSubmitted, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.LeadAdapterSubmitted, trigger.triggers.FirstOrDefault());
                else if (trigger.triggerType == WorkflowTriggerType.LeadScoreReached)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.LeadscoreReached, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.LeadscoreReached, trigger.triggers.FirstOrDefault());
                else if (trigger.triggerType == WorkflowTriggerType.WebPageVisited)
                    if (trigger.IsStartTrigger)
                        allowedTriggers.Add(LeadScoreConditionType.ContactVisitsWebPage, trigger.triggers.FirstOrDefault());
                    else
                        stopTriggers.Add(LeadScoreConditionType.ContactVisitsWebPage, trigger.triggers.FirstOrDefault());
                else if (trigger.triggerType == WorkflowTriggerType.LinkClicked)
                {
                    if (trigger.IsStartTrigger)
                    {
                        // foreach (var LinkID in trigger.triggers.Select(s => s.SelectedLinks))
                        allowedTriggers.Add(LeadScoreConditionType.ContactClicksLink, trigger.triggers.FirstOrDefault());
                    }
                    else
                    {
                        //foreach (var LinkID in trigger.triggers.Select(s => s.SelectedLinks))
                        stopTriggers.Add(LeadScoreConditionType.ContactClicksLink, trigger.triggers.FirstOrDefault());
                    }
                }
            }

            Logger.Current.Informational("Configuring actions for workflow :" + WorkflowId);
            foreach (var workflowAction in workflow.WorkflowActions.OrderBy(a => a.OrderNumber))
            {
                if (workflowAction != null)
                {
                    State currentState = StateFactory.GetState(workflow, workflowAction.Action, workflowAction.WorkflowActionID, workflowAction.WorkflowActionTypeID,
                        workflowService, tagService, campaignService, contactService, leadScoreService, pubSubService, communicationService);

                    if (states.Count > 0)
                    {
                        var lastState = states.Last();
                        lastState.TransitionState = currentState;
                    }
                    else
                        currentState.AllowedTriggers = allowedTriggers;

                    states.Add(currentState);
                }
            }
        }

        public WorkflowStateTransitionStatus ProcessMessage(Message message, int workflowId)
        {
            //Check if the contact has entered the workflow, verify if a record exists in contactworkflowaudit
            //If already entered, verify if the contact can reenter, else return as false

            //if the message is a form submission, then transit to the next states
            //get last state of the contact.
            //move to next state.

            Logger.Current.Informational("Request for processing message with workflowId :" + workflowId + "AccountId :" + message.AccountId + "and ContactId :" + message.ContactId);

            var endState = states.LastOrDefault();

            bool canReenterResponse = workflow.IsWorkflowAllowedMoreThanOnce.HasValue ? workflow.IsWorkflowAllowedMoreThanOnce.Value : false;

            HasContactEnteredWorkflowResponse contactEnteredResponse = workflowService.HasContactEnteredWorkflow(new HasContactEnteredWorkflowRequest()
            {
                ContactId = message.ContactId,
                WorkflowId = workflowId,
                MessageId = message.MessageId
            });

            HasContactCompletedWorkflowResponse hasCompletedResponse = workflowService.HasContactCompletedWorkflow(new HasContactCompletedWorkflowRequest()
            {
                ContactId = message.ContactId,
                WorkflowId = WorkflowId,
                WorkflowActionId = endState.StateId
            });

            bool matchEndTrigger = stopTriggers.IsMatchTrigger(message);

            if (message.LeadScoreConditionType == (int)LeadScoreConditionType.LeadscoreReached)
            {
                bool matchStartTrigger = allowedTriggers.IsMatchTrigger(message);

                if (matchEndTrigger && !hasCompletedResponse.HasCompleted)
                    return InsertWorkflowAudit(endState.StateId, message.ContactId, message.AccountId);
                else if (!contactEnteredResponse.HasEntered && matchStartTrigger)
                    return ProcessFirstState(message);
                else
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;
            }

            if (contactEnteredResponse.HasEntered && canReenterResponse.Equals(false) && message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactClicksLink)
            {
                if (!((message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactWaitPeriodEnded || matchEndTrigger) && !hasCompletedResponse.HasCompleted))
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;
                //{ }
                //else
                    //return WorkflowStateTransitionStatus.UnAuthorizedTransition;
            }
            else if (contactEnteredResponse.HasEntered && canReenterResponse.Equals(false) && message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactClicksLink
                && allowedTriggers.IsMatchTrigger(message))          //link click as trigger
                return WorkflowStateTransitionStatus.UnAuthorizedTransition;
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactWaitPeriodEnded && hasCompletedResponse.HasCompleted)
                return WorkflowStateTransitionStatus.UnAuthorizedTransition;

            if (matchEndTrigger)
                return InsertWorkflowAudit(endState.StateId, message.ContactId, message.AccountId);

            //Entering into workflow for the first time.
            if ((!contactEnteredResponse.HasEntered || canReenterResponse) && message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactWaitPeriodEnded && message.LeadScoreConditionType != (int)LeadScoreConditionType.ContactClicksLink)
                return ProcessFirstState(message);

            //If link click is sub-action
            else if (message.LeadScoreConditionType == (int)LeadScoreConditionType.ContactClicksLink)
            {
                Dictionary<int, bool> canComplete = new Dictionary<int, bool>();
                //Substate processing won't have impact on 'Allow more than once' condition
                foreach (var state in states)
                {
                    if (state.SubState != null && state.SubState.AllowedTriggers.ContainsKey(LeadScoreConditionType.ContactClicksLink) &&
                            state.SubState.AllowedTriggers.Any(s => s.Value.SelectedLinks.Contains(message.LinkedEntityId)))
                    {
                        
                            var result = state.SubState.Transit(message);
                            if (result == WorkflowStateTransitionStatus.TransitedSuccessfully)
                                canComplete.Add(state.StateId, true);
                            else
                                canComplete.Add(state.StateId, false);
                        
                    }
                    if (state.SubState == null && (!contactEnteredResponse.HasEntered || canReenterResponse) &&
                            state.AllowedTriggers.ContainsKey(LeadScoreConditionType.ContactClicksLink) &&
                            state.AllowedTriggers.Any(s => s.Value.SelectedLinks.Contains(message.LinkedEntityId)))
                    {                        
                         var firstState = states.First();
                         return firstState.Transit(message);                       
                    }
                }
                if (canComplete.All(d => d.Value == true))
                    return WorkflowStateTransitionStatus.TransitedSuccessfully;
                else
                    return WorkflowStateTransitionStatus.TransitionFailed;
            }
            else if ((LeadScoreConditionType)message.LeadScoreConditionType == LeadScoreConditionType.ContactWaitPeriodEnded)
            {
                if (message.WorkflowId == workflow.WorkflowID)
                    return HandleUnfinishedWorkflowMessage(message, workflowId);
                else
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;
            }
            else
            {
                if (!allowedTriggers.IsMatchTrigger(message))
                    return WorkflowStateTransitionStatus.UnAuthorizedTransition;

                return HandleUnfinishedWorkflowMessage(message, workflowId);
            }
        }

        private WorkflowStateTransitionStatus HandleUnfinishedWorkflowMessage(Message message, int workflowId)
        {
            try
            {
                GetContactLastStateResponse lastStateResponse = workflowService.GetLastState(new GetContactLastStateRequest() { ContactId = message.ContactId, WorkflowId = workflowId });
                if (lastStateResponse.Exception == null)
                {
                    var lastState = states.SingleOrDefault(s => s.StateId == (int)lastStateResponse.WorkflowActionId);
                    if (lastState != null && lastState.TransitionState != null)
                        return lastState.TransitionState.Transit(message);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }

            return WorkflowStateTransitionStatus.TransitionFailed;
        }

        WorkflowStateTransitionStatus InsertWorkflowAudit(int endStateId, int contactId, int accountId)
        {
            workflowService.InsertContactWorkflowAudit(new InsertContactWorkflowAuditRequest()
            {
                WorkflowId = WorkflowId,
                WorkflowActionId = endStateId,
                ContactId = contactId,
                AccountId = accountId
            });
            return WorkflowStateTransitionStatus.TransitedSuccessfully;
        }

        WorkflowStateTransitionStatus ProcessFirstState(Message message)
        {
            var firstState = states.First();
            return firstState.Transit(message);
        }
    }
}
