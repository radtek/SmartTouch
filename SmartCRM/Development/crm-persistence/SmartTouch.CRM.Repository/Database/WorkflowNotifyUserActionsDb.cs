using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowNotifyUserActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkFlowNotifyUserActionID { get; set; }

        public string UserID { get; set; }
        [NotMapped]
        public IEnumerable<string> UserNames { get; set; }

        public byte NotifyType { get; set; }
        public string MessageBody { get; set; }
        public string NotificationFields { get; set; }

        public override void Save(CRMDb db)
        {
            
            if(WorkFlowNotifyUserActionID == 0)
            {
                db.WorkflowNotifyUserActions.Add(this);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            var WorkflowNotifyUserActionsDb = db.WorkflowNotifyUserActions.Where(u => u.WorkflowActionID == workflowActionId).FirstOrDefault();
            if (WorkflowNotifyUserActionsDb != null && WorkflowNotifyUserActionsDb.UserID != null) 
            {
                IEnumerable<int> userIds = !string.IsNullOrEmpty(WorkflowNotifyUserActionsDb.UserID) ? WorkflowNotifyUserActionsDb.UserID.Split(',').Select(se => Convert.ToInt32(se)): new List<int>() { };
                if (userIds.IsAny())
                    WorkflowNotifyUserActionsDb.UserNames = db.Users.Where(w => userIds.Contains(w.UserID) && w.IsDeleted == false && w.Status == Status.Active).Select(s => s.FirstName + s.LastName);
                else
                    WorkflowNotifyUserActionsDb.UserNames = new List<string>();

            }
            else if(WorkflowNotifyUserActionsDb != null && WorkflowNotifyUserActionsDb.NotificationFields != null)
            {
                IEnumerable<int> notificationFieldIds = !string.IsNullOrEmpty(WorkflowNotifyUserActionsDb.NotificationFields) ? WorkflowNotifyUserActionsDb.NotificationFields.Split(',').Select(se => Convert.ToInt32(se)): new List<int>() { };
            }
            return WorkflowNotifyUserActionsDb;
        }
    }
}
