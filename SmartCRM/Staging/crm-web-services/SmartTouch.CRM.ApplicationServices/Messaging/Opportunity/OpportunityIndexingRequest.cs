using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class OpportunityIndexingRequest : ServiceRequestBase
    {
        public IList<int> OpportunityIds { get; set; }
    }
    public class OpportunityIndexingResponce : ServiceResponseBase
    {

    }
}
