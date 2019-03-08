using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowDateTimerAction : BaseWorkflowAction
    {
        public int WorkflowDateTimerActionID { get; set; }
        public byte TimerType { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public DateTime? RunOn { get; set; }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
