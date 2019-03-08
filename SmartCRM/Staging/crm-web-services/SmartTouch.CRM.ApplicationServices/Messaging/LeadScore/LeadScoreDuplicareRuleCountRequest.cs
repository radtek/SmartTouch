using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class LeadScoreDuplicareRuleCountRequest : ServiceRequestBase
    {
        public int ConditionId { get; set; }
        public string ConditionValue { get; set; }
    }

    public class LeadScoreDuplicareRuleCountResponse : ServiceResponseBase
    {
        public int Count { get; set; }
    }
}
