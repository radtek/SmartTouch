using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetLeadScoreRuleByConditionRequest:ServiceRequestBase
    {
        public LeadScoreConditionType Condition { get; set; }
        public string ConditionValue { get; set; }
        public int EntityID { get; set; }
        public IEnumerable<LeadScoreConditionValueViewModel> AdditionalConditionValues { get; set; }
    }

    public class GetLeadScoreRuleByConditionResponse : ServiceResponseBase
    {
        public LeadScoreRule Rule { get; set; }
        public IEnumerable<LeadScoreRule> Rules { get; set; }
    }
}
