using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class UpdateLeadScoreRuleRequest : ServiceRequestBase
    {
        public LeadScoreRuleViewModel LeadScoreRuleViewModel { get; set; }
    }

    public class UpdateLeadScoreRuleResponse : ServiceResponseBase
    {
        public virtual LeadScoreRuleViewModel LeadScoreRuleViewModel { get; set; }
    }
}
