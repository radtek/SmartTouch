using System;

namespace SmartTouch.CRM.Infrastructure.Domain
{
    public class BusinessRule
    {
        private readonly string ruleDescription;

        public BusinessRule(string ruleDescription)
        {
            this.ruleDescription = ruleDescription;
        }

        public String RuleDescription
        {
            get
            {
                return ruleDescription;
            }
        }
    }   
}
