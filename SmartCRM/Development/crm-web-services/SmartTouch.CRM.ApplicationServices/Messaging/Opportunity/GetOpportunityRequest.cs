using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityRequest:ServiceRequestBase
    {
        public int OpportunityID { get; set; }
        public bool IncludeTags { get; set; }
    }

    public class GetOpportunityResponse : ServiceResponseBase {
        public OpportunityViewModel OpportunityViewModel { get; set; }
    }
}
