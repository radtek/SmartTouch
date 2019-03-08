using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class TriggerWorkflowAction : WorkflowAction
    {
        [Key]
        public int TriggerWorkflowActionID { get; set; }
        public int SiblingWorkflowID { get; set; }
        public string WorkflowName { get; set; }

        protected override void Validate()
        {
            base.Validate();
        }
    }
}
