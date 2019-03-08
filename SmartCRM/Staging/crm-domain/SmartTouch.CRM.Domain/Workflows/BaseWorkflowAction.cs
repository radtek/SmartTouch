using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class BaseWorkflowAction : ValueObjectBase
    {
        public int WorkflowActionID { get; set; }
        public WorkflowActionType WorkflowActionTypeID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
