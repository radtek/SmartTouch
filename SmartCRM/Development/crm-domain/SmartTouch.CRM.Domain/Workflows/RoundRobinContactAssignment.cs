using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class RoundRobinContactAssignment
    {
        public int RoundRobinContactAssignmentID { get; set; }
        public byte DayOfWeek { get; set; }
        public string UserID { get; set; }
        public bool IsRoundRobinAssignment { get; set; }
        public IEnumerable<string> UserNames { get; set; }
        public int WorkFlowUserAssignmentActionID { get; set; }
        public WorkflowUserAssignmentAction UserAction { get; set; }
    }
}
