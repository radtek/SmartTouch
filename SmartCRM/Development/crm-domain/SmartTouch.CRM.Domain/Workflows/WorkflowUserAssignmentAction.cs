using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowUserAssignmentAction : WorkflowAction
    {
        [Key]
        public int WorkflowUserAssignmentActionID { get; set; }

        public byte ScheduledID { get; set; }
        public string UserName { get; set; }
        public IEnumerable<string> UserNames { get; set; }

        public IEnumerable<RoundRobinContactAssignment> RoundRobinContactAssignments { get; set; }

        protected override void Validate()
        {
            base.Validate();
            //if (this.UserID == 0)
                //AddBrokenRule(WorkflowBusinessRules.UserRequired);
        }
    }
}
