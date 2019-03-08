using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowTextNotificationAction : BaseWorkflowAction
    {
        public int WorkflowTextNotificationActionID { get; set; }
        public int FromMobileID { get; set; }
        public string Message { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (this.FromMobileID == 0)
                AddBrokenRule(WorkflowBusinessRules.FromMobileRequried);
            if (string.IsNullOrEmpty(Message))
                AddBrokenRule(WorkflowBusinessRules.MessageRequried);
        }
    }
}
