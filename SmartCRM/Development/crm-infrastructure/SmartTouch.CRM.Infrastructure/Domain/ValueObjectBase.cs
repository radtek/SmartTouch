using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Infrastructure.Domain
{
    public abstract class ValueObjectBase
    {
        List<BusinessRule> brokenRules = new List<BusinessRule>();

        public ValueObjectBase()
        { }

        protected abstract void Validate();


        public IEnumerable<BusinessRule> GetBrokenRules()
        {
            brokenRules.Clear();
            Validate();
            return brokenRules;
        }

        public void ThrowExceptionIfInvalid()
        {
            brokenRules.Clear();
            Validate();
            if (brokenRules != null && brokenRules.Any())
            {
                StringBuilder issues = new StringBuilder();
                foreach (BusinessRule businessRule in brokenRules)
                {
                    issues.AppendLine(businessRule.RuleDescription);
                }

                throw new ValueObjectIsInvalidException(issues.ToString());
            }
        }

        protected void AddBrokenRule(BusinessRule businessRule)
        {
            brokenRules.Add(businessRule);
        }
    }
}
