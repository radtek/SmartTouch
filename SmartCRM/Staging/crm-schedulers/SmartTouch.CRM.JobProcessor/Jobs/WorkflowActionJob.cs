using System;
using System.Collections.Generic;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.WorkFlow;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Automation.Core;
using SmartTouch.CRM.Automation.Core.States;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.MessageQueues;

namespace SmartTouch.CRM.JobProcessor.Jobs
{
    public class WorkflowActionJob : BaseJob
    {
        private readonly IWorkflowService _workflowService;
        private readonly ICampaignService _campaignService;
        private readonly ITagService _tagService;
        private readonly ILeadScoreService _leadScoreService;
        private readonly IContactService _contactService;
        private readonly IMessageService _messageService;
        private readonly ICommunicationService _communicationService;
        private readonly IAccountService _accountService;
        private readonly JobServiceConfiguration _jobConfig;

        public WorkflowActionJob(
            IWorkflowService workflowService,
            ICampaignService campaignService,
            ITagService tagService,
            ILeadScoreService leadScoreService,
            IContactService contactService,
            IMessageService messageService,
            ICommunicationService communicationService,
            IAccountService accountService,
            JobServiceConfiguration jobConfig)
        {
            _workflowService = workflowService;
            _campaignService = campaignService;
            _tagService = tagService;
            _leadScoreService = leadScoreService;
            _contactService = contactService;
            _messageService = messageService;
            _communicationService = communicationService;
            _accountService = accountService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            //Get the items to start processing to start processing
            var trackActions = _workflowService.GetNextBatchToProcess()
                .TrackActions
                .OrderBy(a => a.TrackActionID)
                .Take(_jobConfig.WorkflowActionChunkSize)
                .ToArray();

            if (!trackActions.IsAny())
                return;

            var errorMessages = new List<TrackActionLog>();
            foreach (var trackAction in trackActions)
            {
                try
                {
                    if (trackAction.WorkflowAction != null && trackAction.TrackMessage != null)
                        Log.Informational("TrackActionID :" + trackAction.WorkflowAction.WorkflowActionID + " ContactID:" + trackAction.TrackMessage.ContactId);
                    var hasMatched = _workflowService.HasContactMatchedEndTrigger(trackAction.TrackMessage.ContactId, trackAction.WorkflowID, trackAction.TrackMessageID);
                    if (!hasMatched)
                    {
                        State state = null;
                        switch ((WorkflowActionType)trackAction.WorkflowActionTypeID)
                        {
                            case WorkflowActionType.SendCampaign:
                                var workflowCampaignAction = (trackAction.WorkflowAction as WorkflowCampaignAction);
                                state = new SendCampaignState(workflowCampaignAction.CampaignID, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.ActionID,
                                        workflowCampaignAction.Links != null && workflowCampaignAction.Links.Any() ? workflowCampaignAction.Links.Select(l => l.LinkID) : new List<int>(),
                                        _campaignService, _workflowService);
                                break;
                            case WorkflowActionType.SendEmail:
                                var action = trackAction.WorkflowAction as WorkflowEmailNotificationAction;
                                state = new SendEmailState(trackAction.WorkflowID, action.WorkflowActionID, action.Body, action.Subject, action.FromEmailID, _workflowService, _communicationService);
                                break;
                            case WorkflowActionType.NotifyUser:
                                //Added 5 seconds wait time in-order to make notify emails pull fresh data. NEXG-2697
                                System.Threading.Thread.Sleep(5000);
                                var workflowNotifyUserAction = (trackAction.WorkflowAction as WorkflowNotifyUserAction);
                                state = new NotifyUserState(workflowNotifyUserAction.UserID, workflowNotifyUserAction.NotificationFieldID, trackAction.WorkflowID, trackAction.Workflow.WorkflowName, trackAction.Workflow.CreatedBy, trackAction.ActionID,
                                        workflowNotifyUserAction.MessageBody, workflowNotifyUserAction.NotifyType, _workflowService);
                                break;
                            case WorkflowActionType.AddTag:
                                state = new TagAddedState((trackAction.WorkflowAction as WorkflowTagAction).TagID, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.ActionID, _tagService, _workflowService);
                                break;
                            case WorkflowActionType.RemoveTag:
                                state = new TagRemovedState((trackAction.WorkflowAction as WorkflowTagAction).TagID, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.ActionID, _tagService, _workflowService);
                                break;
                            case WorkflowActionType.AdjustLeadScore:
                                state = new AdjustLeadScoreState((trackAction.WorkflowAction as WorkflowLeadScoreAction).LeadScoreValue, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.TrackMessage.AccountId, trackAction.ActionID, _leadScoreService, _workflowService);
                                break;
                            case WorkflowActionType.ChangeLifecycle:
                                state = new LifecycleChangedState((trackAction.WorkflowAction as WorkflowLifeCycleAction).LifecycleDropdownValueID, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.ActionID, _contactService, _workflowService);
                                break;
                            case WorkflowActionType.UpdateField:
                                var workflowContactFieldAction = trackAction.WorkflowAction as WorkflowContactFieldAction;
                                state = new UpdateFieldState(workflowContactFieldAction.FieldID, trackAction.WorkflowID, trackAction.ActionID, workflowContactFieldAction.FieldValue, (int)workflowContactFieldAction.FieldInputTypeId, _contactService, _workflowService);
                                break;
                            case WorkflowActionType.AssignToUser:
                                var assignUser = trackAction.WorkflowAction as WorkflowUserAssignmentAction;
                                state = new AssignUserState(assignUser.WorkflowUserAssignmentActionID, assignUser.ScheduledID, trackAction.WorkflowID, trackAction.Workflow.CreatedBy, trackAction.ActionID, _contactService, _workflowService);
                                break;
                            case WorkflowActionType.TriggerWorkflow:
                                state = new TriggerWorkflowState(trackAction.WorkflowID, (trackAction.WorkflowAction as TriggerWorkflowAction).SiblingWorkflowID, trackAction.ActionID, _workflowService, _messageService);
                                break;
                            case WorkflowActionType.WorkflowEndState:
                            case WorkflowActionType.LinkActions:
                                state = new WorkflowEndState(trackAction.WorkflowID, trackAction.ActionID, _workflowService);
                                break;
                            case WorkflowActionType.SetTimer:
                                state = new WaitingPeriodState(trackAction.ActionID, _workflowService);
                                break;
                            case WorkflowActionType.SendText:
                                //Not implemented
                                break;
                            default:
                                //Handle exception here
                                throw new InvalidOperationException($"Handling of WorkflowActionType {(WorkflowActionType)trackAction.WorkflowActionTypeID} not implemented");
                        }
                        state?.OnEntry(ToMessage(trackAction.TrackMessage, trackAction.WorkflowID));
                        trackAction.ActionProcessStatusID = TrackActionProcessStatus.Executed;
                        trackAction.ExecutedOn = DateTime.UtcNow;
                    }
                    else
                    {
                        trackAction.ActionProcessStatusID = TrackActionProcessStatus.Termintaed;
                        trackAction.ExecutedOn = DateTime.UtcNow;
                    }
                }
                catch (Exception ex)
                {
                    var message = string.Format("Error while executing {0} workflow - {1} action id - {2} track message, {4} - action type. Error - {3}", trackAction.WorkflowID, trackAction.TrackActionID, trackAction.TrackMessage.TrackMessageID, ex.Message, ((Entities.WorkflowActionType)trackAction.WorkflowActionTypeID));
                    Log.Error(message, ex);
                    trackAction.ActionProcessStatusID = TrackActionProcessStatus.Error;
                    trackAction.ExecutedOn = DateTime.UtcNow;
                    errorMessages.Add(new TrackActionLog { TrackActionID = trackAction.TrackActionID, ErrorMessage = ex.Message });
                }
                try
                {
                    _accountService.ScheduleAnalyticsRefresh(trackAction.WorkflowID, (byte)IndexType.Workflows);
                }
                catch (Exception wex)
                {
                    Log.Error("Error while inserting to refresh analytics - workflows", wex);
                }
            }
            _workflowService.UpdateActionBatchStatus(new UpdateActionBatchStatusRequest { TrackActions = trackActions, TrackActionLogs = errorMessages });
            Log.Informational($"Updated status for TrackActionIDs {string.Join(",", trackActions.Select(x => x.TrackActionID))}");
        }

        private Message ToMessage(TrackMessage trackMessage, int workflowId)
        {
            return new Message
            {
                MessageId = trackMessage.MessageID.ToString(),
                EntityId = trackMessage.EntityId,
                UserId = trackMessage.UserId,
                LeadScoreConditionType = trackMessage.LeadScoreConditionType,
                ContactId = trackMessage.ContactId,
                AccountId = trackMessage.AccountId,
                LinkedEntityId = trackMessage.LinkedEntityId,
                ConditionValue = trackMessage.ConditionValue,
                WorkflowId = workflowId
            };
        }
    }
}
