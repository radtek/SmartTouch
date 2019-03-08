using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class DeleteOpportunityRequest:ServiceRequestBase
    {
        public int[] OpportunityIDs { get; set; }
    }

    public class DeleteOpportunityResponse : ServiceResponseBase
    {
        
    }
}
