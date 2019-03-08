using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class LeadScoreConditionValueViewModel
    {
        public int LeadScoreConditionValueId { get; set; }
        public int LeadScoreRuleId { get; set; }
        public LeadScoreValueType ValueType { get; set; }
        public string Value { get; set; }
    }
}
