using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ParentWorkflowViewModel
    {
        public int WorkflowID { get; set; }
        public int ParentWorkflowID { get; set; }
        public string WorkflowName { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Status { get; set; }
        public WorkFlowViewModel workflowViewModel { get; set; }
    }
}
