using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class InsertOpportunityBuyerRequest:ServiceRequestBase
    {
        public OpportunityViewModel opportunityViewModel { get; set; }
        public byte ModuleID { get; set; }
    }

    public class InsertOpportunityBuyerResponse:ServiceResponseBase
    {

    }
}
