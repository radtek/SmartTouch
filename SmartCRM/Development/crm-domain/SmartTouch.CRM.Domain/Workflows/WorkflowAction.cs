using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class WorkflowAction : BaseWorkflowAction
    {
        public short WorkflowID { get; set; }
        public int OrderNumber { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsSubAction { get; set; }
        public BaseWorkflowAction Action { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
