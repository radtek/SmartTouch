using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class CancelCampaignRequest : IntegerIdRequest
    {
        public CancelCampaignRequest(int id) : base(id) { }
    }

    public class CancelCampaignResponse : ServiceResponseBase
    {
        public Campaign CanceledCampaign { get; set; }
    }
}
