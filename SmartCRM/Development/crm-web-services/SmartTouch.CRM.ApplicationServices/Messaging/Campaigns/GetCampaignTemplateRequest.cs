using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignTemplateRequest : ServiceRequestBase
    {
        public int CampaignTemplateID { get; set; }
        public CampaignTemplateType TemplateType { get; set; }
    }

    public class GetCampaignTemplateResponse : ServiceResponseBase
    {
        public CampaignTemplateViewModel CampaignTemplateViewModel { get; set; }
        public string HTMLContent { get; set; }
    }
}
