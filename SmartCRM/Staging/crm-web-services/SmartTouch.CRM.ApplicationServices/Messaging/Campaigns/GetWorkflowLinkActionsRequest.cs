using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetWorkflowLinkActionsRequest: ServiceRequestBase
    {
    }

    public class GetWorkflowLinkActionsResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignLinkInfo> CampaignLinks { get; set; }
    }
}
