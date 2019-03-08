using LandmarkIT.Enterprise.Utilities.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class BaseWorkflowActionConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(BaseWorkflowActionViewModel).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            var suitableObjs = GetSuitableAction(obj);
            return suitableObjs[(WorkflowActionType)obj["WorkflowActionTypeID"].Value<int>()];
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Need to do in more sophisticated way, by applying custom attribute on each action class.
        /// </summary>
        private IDictionary<WorkflowActionType, BaseWorkflowActionViewModel> GetSuitableAction(JObject obj)
        {
            var dictionary = new Dictionary<WorkflowActionType, BaseWorkflowActionViewModel>();
            dictionary.Add(WorkflowActionType.SendCampaign, obj.ToObject<WorkflowCampaignActionViewModel>());
            dictionary.Add(WorkflowActionType.SendText, obj.ToObject<WorkflowTextNotificationActionViewModel>());
            dictionary.Add(WorkflowActionType.SetTimer, obj.ToObject<WorkflowTimerActionViewModel>());
            dictionary.Add(WorkflowActionType.AddTag, obj.ToObject<WorkflowTagActionViewModel>());
            dictionary.Add(WorkflowActionType.RemoveTag, obj.ToObject<WorkflowTagActionViewModel>());
            dictionary.Add(WorkflowActionType.AdjustLeadScore, obj.ToObject<WorkflowLeadScoreActionViewModel>());
            dictionary.Add(WorkflowActionType.ChangeLifecycle, obj.ToObject<WorkflowLifeCycleActionViewModel>());
            dictionary.Add(WorkflowActionType.UpdateField, obj.ToObject<WorkflowContactFieldActionViewModel>());
            dictionary.Add(WorkflowActionType.AssignToUser, convertToUserAssignmentModel(obj));
            dictionary.Add(WorkflowActionType.NotifyUser, convertToNotifyUserModel(obj));
            dictionary.Add(WorkflowActionType.SendEmail, obj.ToObject<WorkflowEmailNotifyActionViewModel>());
            dictionary.Add(WorkflowActionType.TriggerWorkflow, obj.ToObject<TriggerWorkflowActionViewModel>());
            dictionary.Add(WorkflowActionType.LinkActions, obj.ToObject<WorkflowCampaignActionViewModel>());
            return dictionary;
        }

        public WorkflowNotifyUserActionViewModel convertToNotifyUserModel(JObject jobject)
        {
            try
            {
                if (jobject["WorkflowActionTypeID"].ToObject<WorkflowActionType>() == WorkflowActionType.NotifyUser)
                {
                    WorkflowNotifyUserActionViewModel viewModel = new WorkflowNotifyUserActionViewModel();
                    viewModel.WorkflowActionTypeID = WorkflowActionType.NotifyUser;
                    viewModel.MessageBody = jobject["MessageBody"].ToObject<string>();
                    viewModel.NotifyType = jobject["NotifyType"].ToObject<int>();

                    var workflowActionId = jobject["WorkflowActionID"];
                    if (workflowActionId != null)
                        viewModel.WorkflowActionID = workflowActionId.ToObject<int>();
                    var notifyUserActionId = jobject["WorkflowNotifyUserActionID"];
                    if (notifyUserActionId != null)
                        viewModel.WorkflowNotifyUserActionID = notifyUserActionId.ToObject<int>();
                    var results = jobject["UserIds"];
                    if (results is JArray)
                        viewModel.UserIds = results.ToObject<List<int>>();
                    var fieldIds = jobject["NotificationFieldIds"];
                    if (fieldIds is JArray)
                        viewModel.NotificationFieldIds = fieldIds.ToObject<List<int>>();

                    return viewModel;
                }
                else
                    return jobject.ToObject<WorkflowNotifyUserActionViewModel>();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while deserializing workflow viewmodel ", ex);
                throw ex;
            }

        }

        public WorkflowUserAssignmentActionViewModel convertToUserAssignmentModel(JObject jobject)
        {
            try
            {
                if (jobject["WorkflowActionTypeID"].ToObject<WorkflowActionType>() == WorkflowActionType.AssignToUser)
                {
                    WorkflowUserAssignmentActionViewModel viewModel = new WorkflowUserAssignmentActionViewModel();
                    viewModel.WorkflowActionTypeID = WorkflowActionType.AssignToUser;
                    var workflowActionId = jobject["WorkflowActionID"];
                    if (workflowActionId != null)
                        viewModel.WorkflowActionID = workflowActionId.ToObject<int>();
                    var assignUserActionId = jobject["WorkflowUserAssignmentActionID"];
                    if (assignUserActionId != null)
                        viewModel.WorkflowUserAssignmentActionID = assignUserActionId.ToObject<int>();
                    var scheduledId = jobject["ScheduledID"];
                    if (scheduledId != null)
                        viewModel.ScheduledID = scheduledId.ToObject<int>();
                    var assignments = jobject["RoundRobinContactAssignments"];
                    if (assignments is JArray)
                        viewModel.RoundRobinContactAssignments = assignments.ToObject<List<RoundRobinContactAssignmentViewModel>>();
                    
                    return viewModel;
                }
                else
                    return jobject.ToObject<WorkflowUserAssignmentActionViewModel>();
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while deserializing workflow viewmodel ", ex);
                throw ex;
            }

        }
    }
}
