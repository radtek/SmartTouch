using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.LeadScoreRules;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class InsertLeadScoreRequest : ServiceRequestBase
    {
        public LeadScoreConditionType Condition { get; set; }
        public int ContactId { get; set; }
        public string ConditionValue { get; set; }
        public int EntityId { get; set; }
        public LeadScoreRule Rule { get; set; }
        public IEnumerable<LeadScoreRule> Rules { get; set; }
    }

    public class InsertLeadScoreResponse : ServiceResponseBase
    {

    }
}
