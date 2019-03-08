using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkFlowUserAssignmentActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkFlowUserAssignmentActionID { get; set; }

        public byte ScheduledID { get; set; }

        public IEnumerable<RoundRobinContactAssignmentDb> RoundRobinContactAssignments { get; set; }

        public override void Save(CRMDb db)
        {
            if (WorkFlowUserAssignmentActionID == 0)
            {
                db.WorkflowUserAssignmentActions.Add(this);
            }
            else
            {
                var assignmentsActions = db.WorkflowUserAssignmentActions.AsNoTracking().Where(w => w.WorkFlowUserAssignmentActionID == WorkFlowUserAssignmentActionID).FirstOrDefault();
                bool isScheduleChanged = assignmentsActions != null ? assignmentsActions.ScheduledID != this.ScheduledID : false;
                if (isScheduleChanged)
                {
                    var oldAssignments = db.RoundRobinAssignment.Where(w => w.WorkFlowUserAssignmentActionID == WorkFlowUserAssignmentActionID);
                    foreach (var oa in oldAssignments)
                        db.Entry(oa).State = System.Data.Entity.EntityState.Deleted;
                }
                db.Entry(this).State = EntityState.Modified;
            }
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            var userAssignment = db.WorkflowUserAssignmentActions.Where(ua => ua.WorkflowActionID == workflowActionId).FirstOrDefault();
            if (userAssignment != null)
                userAssignment.RoundRobinContactAssignments = db.RoundRobinAssignment.Where(w => w.WorkFlowUserAssignmentActionID == userAssignment.WorkFlowUserAssignmentActionID);
            if (userAssignment.RoundRobinContactAssignments != null && userAssignment.RoundRobinContactAssignments.Any())
                foreach (var rr in userAssignment.RoundRobinContactAssignments)
                { 
                    List<string> userIds = rr.UserID.Split(',').ToList();
                    var UserNamesDb = db.Users.Where(w => userIds.Contains(w.UserID.ToString()) && w.IsDeleted == false && w.Status == Status.Active).Select(s => new { Name = s.FirstName + " " + s.LastName, Id = s.UserID });
                    var UserNames = new List<string>();
                    foreach (var ID in userIds)
                        UserNames.Add(UserNamesDb.Where(w => w.Id.ToString() == ID).Select(s => s.Name).FirstOrDefault());
                    rr.UserNames = UserNames;
                }
            return userAssignment;
        }
    }
}
