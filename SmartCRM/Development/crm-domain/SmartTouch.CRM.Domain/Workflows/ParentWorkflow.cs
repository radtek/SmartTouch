using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class ParentWorkflow
    {
        public int WorkflowID { get; set; }
        public int ParentWorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Status { get; set; }

    }
}
