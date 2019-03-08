using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Automation.Core;
using SmartTouch.CRM.Automation.Core.States;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.MessageQueues;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartTouch.CRM.JobProcessor
{
    public class ActionProcessor : CronJobProcessor
    {
        readonly IWorkflowService workflowService;
        readonly ICampaignService campaignService;
        readonly ITagService tagService;
        readonly ILeadScoreService leadScoreService;
        readonly IContactService contactService;
        readonly IMessageService messageService;
        readonly ICommunicationService communicationService;
        readonly IAccountService accountService;

        public ActionProcessor(CronJobDb cronJob, JobService jobService, string cacheName)
            : base(cronJob, jobService, cacheName)
        {
            this.workflowService = IoC.Container.GetInstance<IWorkflowService>();
            this.campaignService = IoC.Container.GetInstance<ICampaignService>();
            this.tagService = IoC.Container.GetInstance<ITagService>();
            this.leadScoreService = IoC.Container.GetInstance<ILeadScoreService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.messageService = IoC.Container.GetInstance<IMessageService>();
            this.communicationService = IoC.Container.GetInstance<ICommunicationService>();
            this.accountService = IoC.Container.GetInstance<IAccountService>();
        }

        protected override void Execute()
        {

            try
            {
                //Get the items to start processing to start processing

                var items = this.workflowService.GetNextBatchToProcess().TrackActions;

                if (items.IsAny())
                {
                    while (true)
                    {
                        var errorMessages = new List<TrackActionLog>();
                        foreach (var item in items.OrderBy(a=>a.TrackActionID))
                        {
                            try
                            {
                                var state = default(State);
                                bool hasMatched = false;
                                if(item !=null && item.WorkflowAction != null)
                                {
                                    Logger.Current.Informational("TrackActionID :" + item.WorkflowAction.WorkflowActionID +" ContactID:" +item.TrackMessage.ContactId);
                                }
                                hasMatched = workflowService.HasContactMatchedEndTrigger(item.TrackMessage.ContactId, item.WorkflowID, item.TrackMessageID);
                                if (!hasMatched)
                                {
                                    switch ((Entities.WorkflowActionType)item.WorkflowActionTypeID)
                                    {
                                        case Entities.WorkflowActionType.SendCampaign:
                                            var workflowCampaignAction = (item.WorkflowAction as WorkflowCampaignAction);
                                            state = new SendCampaignState(workflowCampaignAction.CampaignID, item.WorkflowID, item.Workflow.CreatedBy, item.ActionID,
                                                    workflowCampaignAction.Links != null && workflowCampaignAction.Links.Any() ? workflowCampaignAction.Links.Select(l => l.LinkID) : new List<int>(),
                                                    campaignService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.SendEmail:
                                            var action = item.WorkflowAction as WorkflowEmailNotificationAction;
                                            state = new SendEmailState(item.WorkflowID, action.WorkflowActionID, action.Body, action.Subject, action.FromEmailID, workflowService, communicationService);
                                            break;
                                        case Entities.WorkflowActionType.NotifyUser:
                                            //Added 5 milli seconds wait time in-order to make notify emails pull fresh data. NEXG-2697
                                            System.Threading.Thread.Sleep(5000);
                                            var workflowNotifyUserAction = (item.WorkflowAction as WorkflowNotifyUserAction);
                                            state = new NotifyUserState(workflowNotifyUserAction.UserID, workflowNotifyUserAction.NotificationFieldID, item.WorkflowID, item.Workflow.WorkflowName, item.Workflow.CreatedBy, item.ActionID,
                                                 workflowNotifyUserAction.MessageBody, workflowNotifyUserAction.NotifyType, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.AddTag:
                                            state = new TagAddedState((item.WorkflowAction as WorkflowTagAction).TagID, item.WorkflowID, item.Workflow.CreatedBy, item.ActionID, tagService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.RemoveTag:
                                            state = new TagRemovedState((item.WorkflowAction as WorkflowTagAction).TagID, item.WorkflowID, item.Workflow.CreatedBy, item.ActionID, tagService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.AdjustLeadScore:
                                            state = new AdjustLeadScoreState((item.WorkflowAction as WorkflowLeadScoreAction).LeadScoreValue, item.WorkflowID, item.Workflow.CreatedBy, item.TrackMessage.AccountId, item.ActionID, leadScoreService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.ChangeLifecycle:
                                            state = new LifecycleChangedState((item.WorkflowAction as WorkflowLifeCycleAction).LifecycleDropdownValueID, item.WorkflowID, item.Workflow.CreatedBy, item.ActionID, contactService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.UpdateField:
                                            var workflowContactFieldAction = item.WorkflowAction as WorkflowContactFieldAction;
                                            state = new UpdateFieldState(workflowContactFieldAction.FieldID, item.WorkflowID, item.ActionID, workflowContactFieldAction.FieldValue, (int)workflowContactFieldAction.FieldInputTypeId, contactService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.AssignToUser:
                                            var assignUser = (item.WorkflowAction as WorkflowUserAssignmentAction);
                                            state = new AssignUserState(assignUser.WorkflowUserAssignmentActionID, assignUser.ScheduledID, item.WorkflowID, item.Workflow.CreatedBy, item.ActionID, contactService, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.TriggerWorkflow:
                                            state = new TriggerWorkflowState(item.WorkflowID, (item.WorkflowAction as TriggerWorkflowAction).SiblingWorkflowID, item.ActionID, workflowService, messageService);
                                            break;
                                        case Entities.WorkflowActionType.WorkflowEndState:
                                        case Entities.WorkflowActionType.LinkActions:
                                            state = new WorkflowEndState(item.WorkflowID, item.ActionID, workflowService);
                                            break;
                                        case Entities.WorkflowActionType.SetTimer:
                                            state = new WaitingPeriodState(item.ActionID, workflowService);
                                            break;
                                        default:
                                            //Handle exception here
                                            break;
                                    }
                                    if (state != null)
                                    {
                                        state.OnEntry(ToMessage(item.TrackMessage, item.WorkflowID));
                                    }
                                    item.ActionProcessStatusID = Entities.TrackActionProcessStatus.Executed;
                                    item.ExecutedOn = DateTime.UtcNow;
                                }
                                else
                                {
                                    item.ActionProcessStatusID = Entities.TrackActionProcessStatus.Termintaed;
                                    item.ExecutedOn = DateTime.UtcNow;
                                }
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Error while executing {0} workflow - {1} action id - {2} track message, {4} - action type. Error - {3}", item.WorkflowID, item.TrackActionID, item.TrackMessage.TrackMessageID, ex.Message, ((Entities.WorkflowActionType)item.WorkflowActionTypeID).ToString());
                                Logger.Current.Error(message, ex);
                                item.ActionProcessStatusID = Entities.TrackActionProcessStatus.Error;
                                item.ExecutedOn = DateTime.UtcNow;
                                errorMessages.Add(new TrackActionLog { TrackActionID = item.TrackActionID, ErrorMessage = ex.Message });
                            }
                            try
                            {
                                accountService.ScheduleAnalyticsRefresh(item.WorkflowID, (byte)IndexType.Workflows);
                            }
                            catch (Exception wex)
                            {
                                Logger.Current.Error("Error while inserting to refresh analytics - workflows", wex);
                            }
                        }
                        workflowService.UpdateActionBatchStatus(new ApplicationServices.Messaging.WorkFlow.UpdateActionBatchStatusRequest { TrackActions = items, TrackActionLogs = errorMessages });

                        //this.UpdateLastNotifyDateTime();
                        //Check for next iteration
                        items = this.workflowService.GetNextBatchToProcess().TrackActions;
                        if (items.IsAny() == false) break;
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error("Automation Error - ", ex);
            }
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
