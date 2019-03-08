using SmartTouch.CRM.Domain.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityBuyerRequest:ServiceRequestBase
    {
        public int OpportunityId { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetOpportunityBuyerResponse:ServiceResponseBase
    {
        public IEnumerable<OpportunityBuyer> OpportunityBuyers { get; set; }
    }
}
