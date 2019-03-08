using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowDelayTimerActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowDelayTimerActionID { get; set; }

        public int WaitValue { get; set; }
        public byte WaitUnit { get; set; }
        public DateTime RunOn { get; set; }
        
        public override void Save(CRMDb db)
        {
            if (WorkflowDelayTimerActionID == 0)
            {
                db.WorkFlowDelayTimerActions.Add(this);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
            
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {
            return db.WorkFlowDelayTimerActions.Where(dt => dt.WorkflowActionID == workflowActionId).FirstOrDefault();
        }
    }
}
