using System.Collections.Generic;
using System.Linq;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.Automation.Core.States
{
    public static class StateFactory
    {
        public static State GetState(Workflow workflow, BaseWorkflowAction workflowAction, int worklflowActionId,
            WorkflowActionType workflowActionType, IWorkflowService workflowService,
            ITagService tagService, ICampaignService campaignService, IContactService contactService,
            ILeadScoreService leadScoreService, IPublishSubscribeService pubSubService, ICommunicationService communicationService)
        {
            State currentState = null;
            //if (workflowActionType == WorkflowActionType.AddTag)
            //    currentState = new TagAddedState((workflowAction as WorkflowTagAction).TagID,
            //        workflow.WorkflowID, workflow.CreatedBy, workflowAction.WorkflowActionID, tagService, workflowService);
            //else if (workflowActionType == WorkflowActionType.RemoveTag)
            //    currentState = new TagRemovedState((workflowAction as WorkflowTagAction).TagID,
            //        workflow.WorkflowID, workflow.CreatedBy, workflowAction.WorkflowActionID, tagService, workflowService);
            //else if (workflowActionType == WorkflowActionType.SendCampaign)
            //{
            //    var action = (workflowAction as WorkflowCampaignAction);
            //    var linkIds = action.Links != null && action.Links.Count() > 0 ? action.Links.Select(l => l.LinkID) : new List<int>();

            //    currentState = new SendCampaignState(action.CampaignID,
            //        workflow.WorkflowID, workflow.CreatedBy, workflowAction.WorkflowActionID, linkIds,
            //        campaignService, workflowService);

            //    if (linkIds.Count() > 0)
            //    {
            //        //var linkAction = action.LinkAction;
            //        //var act = linkAction as WorkflowAction;
            //        currentState.SubState = StateFactory.GetState(workflow, action, action.WorkflowActionID, action.WorkflowActionTypeID,
            //            workflowService, tagService, campaignService, contactService, leadScoreService, pubSubService, communicationService);
            //        var allowedTriggers = new Dictionary<LeadScoreConditionType, WorkflowTrigger>();
            //        WorkflowTrigger trigger = new WorkflowTrigger() { SelectedLinks = linkIds };
            //        allowedTriggers.Add(LeadScoreConditionType.ContactClicksLink, trigger);
            //        currentState.SubState.AllowedTriggers = allowedTriggers;
            //        currentState.SubState.TransitionState = new SubWorkflowEndState();
            //    }
            //}
            //else if (workflowActionType == WorkflowActionType.ChangeLifecycle)
            //    currentState = new LifecycleChangedState((workflowAction as WorkflowLifeCycleAction).LifecycleDropdownValueID,
            //        workflow.WorkflowID, workflow.CreatedBy, workflowAction.WorkflowActionID, contactService, workflowService);
            //else if (workflowActionType == WorkflowActionType.TriggerWorkflow)
            //    currentState = new TriggerWorkflowState(workflow.WorkflowID, (workflowAction as TriggerWorkflowAction).SiblingWorkflowID, workflowAction.WorkflowActionID,
            //        workflowService, pubSubService);
            //else if (workflowActionType == WorkflowActionType.NotifyUser)
            //    currentState = new NotifyUserState((workflowAction as WorkflowNotifyUserAction).UserID,
            //        workflow.WorkflowID, workflow.WorkflowName, workflow.CreatedBy, workflowAction.WorkflowActionID,
            //        (workflowAction as WorkflowNotifyUserAction).MessageBody,
            //        (workflowAction as WorkflowNotifyUserAction).NotifyType, workflowService);
            //else if (workflowActionType == WorkflowActionType.AssignToUser)
            //    currentState = new AssignUserState((workflowAction as WorkflowUserAssignmentAction).UserID,
            //        workflow.WorkflowID, workflow.CreatedBy, workflowAction.WorkflowActionID, contactService, workflowService);
            //else if (workflowActionType == WorkflowActionType.AdjustLeadScore)
            //    currentState = new AdjustLeadScoreState((workflowAction as WorkflowLeadScoreAction).LeadScoreValue,
            //        workflow.WorkflowID, workflow.CreatedBy, workflow.AccountID, workflowAction.WorkflowActionID, leadScoreService, workflowService);
            //else if (workflowActionType == WorkflowActionType.SendEmail)
            //{
            //    var action = workflowAction as WorkflowEmailNotificationAction;
            //    currentState = new SendEmailState(workflow.WorkflowID, action.WorkflowActionID, action.Body, action.Subject, action.FromEmailID, workflowService, communicationService);
            //}

            //else if (workflowActionType == WorkflowActionType.SetTimer)
            //{
            //    var action = workflowAction as WorkflowTimerAction;
            //    currentState = new WaitingPeriodState(action.WorkflowActionID, action.TimerType, action.DelayPeriod, action.DelayUnit,
            //        action.RunOn, action.RunAt, action.RunType, action.RunOnDate, action.StartDate, action.EndDate, action.DaysOfWeek, workflow.WorkflowID,
            //        workflowService, pubSubService);
            //}
            //else if (workflowActionType == WorkflowActionType.UpdateField)
            //{
            //    var action = workflowAction as WorkflowContactFieldAction;
            //    currentState = new UpdateFieldState(action.FieldID, workflow.WorkflowID, action.WorkflowActionID, action.FieldValue, (int)action.FieldInputTypeId, contactService, workflowService);
            //}
            //else if (workflowActionType == WorkflowActionType.WorkflowEndState)
            //    currentState = new WorkflowEndState(workflow.WorkflowID, worklflowActionId, workflowService);

            return currentState;
        }
    }
}
