using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowTimerAction : BaseWorkflowAction
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
        public IEnumerable<DayOfWeek> DaysOfWeek { get; set; }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
