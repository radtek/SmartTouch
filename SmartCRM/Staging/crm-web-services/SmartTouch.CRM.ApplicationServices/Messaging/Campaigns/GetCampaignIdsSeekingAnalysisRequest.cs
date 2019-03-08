using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignsSeekingAnalysisRequest :ServiceRequestBase
    {
    }

    public class GetCampaignsSeekingAnalysisResponse : ServiceResponseBase
    {
        public IEnumerable<Campaign> Campaigns { get; set; }
    }
}
