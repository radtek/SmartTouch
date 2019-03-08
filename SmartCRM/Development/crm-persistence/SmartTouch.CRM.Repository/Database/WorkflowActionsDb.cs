using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowActionsDb
    {
        [Key]
        public int WorkflowActionID { get; set; }

        //[ForeignKey("WorkflowActionTypes")]
        public WorkflowActionType WorkflowActionTypeID { get; set; }
        //public WorkflowActionTypesDb WorkflowActionTypes { get; set; }

        [ForeignKey("Workflow")]
        public short WorkflowID { get; set; }
        public WorkflowsDb Workflow { get; set; }

        public int OrderNumber { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsSubAction { get; set; }

        public virtual BaseWorkflowActionsDb Action { get; set; }

        public void Save(CRMDb db)
        {
            try
            {
                if (WorkflowActionID == 0)
                    db.WorkflowActions.Add(this);
                else
                    db.Entry(this).State = EntityState.Modified;
            }
            catch(Exception ex)
            {
                Logger.Current.Error("Error while saving action", ex);
                ex.Data.Clear();
                ex.Data.Add("Workflow", WorkflowID);
            }
            
        }
        public BaseWorkflowActionsDb GetAction(CRMDb db)
        {
            var registeredActions = RegisterActions();
            return registeredActions[(WorkflowActionType)WorkflowActionTypeID]
                        .Get(WorkflowActionID, db);
        }
        /// <summary>
        /// Need to do in more sophisticated way, by applying custom attribute on each action class.
        /// </summary>
        private IDictionary<WorkflowActionType, BaseWorkflowActionsDb> RegisterActions()
        {
            var dictionary = new Dictionary<WorkflowActionType, BaseWorkflowActionsDb>();
            dictionary.Add(WorkflowActionType.SendCampaign, new WorkflowCampaignActionsDb());
            dictionary.Add(WorkflowActionType.SendText, new WorkFlowTextNotificationActionsDb());
            dictionary.Add(WorkflowActionType.SetTimer, new WorkflowTimerActionsDb());
            dictionary.Add(WorkflowActionType.AddTag, new WorkflowTagActionsDb());
            dictionary.Add(WorkflowActionType.RemoveTag, new WorkflowTagActionsDb());
            dictionary.Add(WorkflowActionType.AdjustLeadScore, new WorkflowLeadScoreActionsDb());
            dictionary.Add(WorkflowActionType.ChangeLifecycle, new WorkflowLifeCycleActionsDb());
            dictionary.Add(WorkflowActionType.UpdateField, new WorkflowContactFieldActionsDb());
            dictionary.Add(WorkflowActionType.AssignToUser, new WorkFlowUserAssignmentActionsDb());
            dictionary.Add(WorkflowActionType.NotifyUser, new WorkflowNotifyUserActionsDb());
            dictionary.Add(WorkflowActionType.SendEmail, new WorkflowEmailNotificationActionDb());
            dictionary.Add(WorkflowActionType.TriggerWorkflow, new TriggerWorkflowActionsDb());
            dictionary.Add(WorkflowActionType.LinkActions, new WorkflowCampaignActionsDb());
            return dictionary;
        }
    }
}
