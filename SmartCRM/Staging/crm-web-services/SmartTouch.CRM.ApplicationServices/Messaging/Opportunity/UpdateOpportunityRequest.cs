using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class UpdateOpportunityRequest:ServiceRequestBase
    {
        public OpportunityViewModel opportunityViewModel { get; set; }
        public byte ModuleID { get; set; }
    }

    public class UpdateOpportunityResponse : ServiceResponseBase {
        public virtual OpportunityViewModel opportunityViewModel { get; set; }
    }
}
