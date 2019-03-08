using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class LeadScoreConditionValue : ValueObjectBase
    {
        public int LeadScoreConditionValueId { get; set; }
        public int LeadScoreRuleId { get; set; }
        public LeadScoreValueType ValueType { get; set; }
        public string Value { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
