using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class SendTestEmailRequest : ServiceRequestBase
    {
        public SendMailViewModel MailViewModel { get; set; }
        public int? ServiceProviderID { get; set; }
        public byte CampaignTypeId { get; set; }
        public Campaign Campaign { get; set; }
        public bool HasDisCliamer { get; set; }
    }

    public class SendTestEmailResponse : ServiceResponseBase
    {
 
    }
}
