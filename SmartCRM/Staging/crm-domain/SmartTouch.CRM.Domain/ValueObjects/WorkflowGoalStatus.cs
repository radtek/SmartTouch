using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WorkflowGoalStatus : ValueObjectBase
    {
        public bool HasMatched { get; set; }
        public byte LeadScoreConditionType { get; set; }
        public string SearchDefinitionIds { get; set; }
        public int AccountId { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
