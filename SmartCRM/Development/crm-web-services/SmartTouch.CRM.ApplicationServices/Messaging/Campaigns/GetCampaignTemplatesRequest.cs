using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignTemplatesRequest : ServiceRequestBase
    {
        public CampaignTemplateType? CampaignTemplateType { get; set; }
    }

    public class GetCampaignTemplatesResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignTemplateViewModel> Templates { get; set; }
    }
}
