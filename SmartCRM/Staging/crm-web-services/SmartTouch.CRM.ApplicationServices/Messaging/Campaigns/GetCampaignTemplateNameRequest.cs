using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignTemplateNamesRequest : ServiceRequestBase
    {
       
    }

    public class GetCampaignTemplateNamesResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignTemplate> TemplateNames { get; set; }
    }
}
