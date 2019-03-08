using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.LeadScoringEngine
{
    public enum MessageHandlerStatus
    {
        LeadScoreAuditedSuccessfully, 
        FailedToAuditScore, 
        LeadScoreRuleNotDefined, 
        InvalidMessageHandler, 
        DuplicateLeadScoreRequest
    }
}
