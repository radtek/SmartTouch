using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class AdjustLeadscoreRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int LeadScore { get; set; }
        public int WorkflowActionId { get; set; }
    }

    public class AdjustLeadscoreResponse : ServiceResponseBase
    { 
    
    }
}
