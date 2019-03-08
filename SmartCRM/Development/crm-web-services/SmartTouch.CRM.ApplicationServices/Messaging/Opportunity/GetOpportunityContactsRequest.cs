using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityContactsRequest : ServiceRequestBase
    {
        public int OpportunityId { get; set; }
    }

    public class GetOpportunityContactsResponse : ServiceResponseBase
    {
        public IList<int> ContactIdList { get; set; }
    }
}
