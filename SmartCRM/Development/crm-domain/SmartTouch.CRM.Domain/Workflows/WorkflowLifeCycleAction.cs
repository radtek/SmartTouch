using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowLifeCycleAction : WorkflowAction
    {
        [Key]
        public int WorkFlowLifeCycleActionID { get; set; }
        public short LifecycleDropdownValueID { get; set; }
        public string LifecycleName { get; set; }

        protected override void Validate()
        {
            base.Validate();
            if (LifecycleDropdownValueID == 0)
                AddBrokenRule(WorkflowBusinessRules.LifeCycleValueRequired);
        }
    }
}
