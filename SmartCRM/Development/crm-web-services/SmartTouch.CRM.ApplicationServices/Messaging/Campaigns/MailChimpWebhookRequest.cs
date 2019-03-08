using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class MailChimpWebhookRequest : ServiceRequestBase
    {
        public CampaignResponseViewModel CampaignResponse { get; set; }
    }

    public class MailChimpWebhookResponse : ServiceResponseBase
    {
    }
}
