using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetOpportunityStageContactsRequest : ServiceRequestBase
    {
        public int StageId { get; set; }
    }

    public class GetOpportunityStageContactsRsponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIds { get; set; }
    }
}
