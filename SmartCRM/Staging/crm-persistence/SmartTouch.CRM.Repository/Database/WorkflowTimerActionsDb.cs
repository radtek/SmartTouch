using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using SmartTouch.CRM.Entities;
using System.Linq;


namespace SmartTouch.CRM.Repository.Database
{
    public class WorkflowTimerActionsDb : BaseWorkflowActionsDb
    {
        [Key]
        public int WorkflowTimerActionID { get; set; }    
        public TimerType TimerType { get; set; }
        public int? DelayPeriod { get; set; }
        public DateInterval? DelayUnit { get; set; }
        // 1 for anyday, 2 for weekday
        public RunOn? RunOn { get; set; }        
        public RunType? RunType { get; set; }
        public TimeSpan? RunAt { get; set; }
        public DateTime? RunOnDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string RunOnDay { get; set; }
        public string DaysOfWeek { get; set; }
        public override void Save(CRMDb db)
        {
            if (WorkflowTimerActionID == 0)
            {
                db.WorkflowTimerActions.Add(this);
            }
            else
            {
                db.Entry(this).State = EntityState.Modified;
            }
        }

        public override BaseWorkflowActionsDb Get(int workflowActionId, CRMDb db)
        {            
            return db.WorkflowTimerActions.Where(cl => cl.WorkflowActionID == workflowActionId).FirstOrDefault();
        }

    }
}
