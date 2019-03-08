using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class InsertCampaignOpenOrClickEntryRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public int ContactId { get; set; }
        public byte? LinkId { get; set; }
        public string IpAddress { get; set; }
        public int CampaignRecipientID { get; set; }
    }

    public class InsertCampaignOpenOrClickEntryResponse : ServiceResponseBase
    {
        public CampaignRecipient Recipient { get; set; }
    }
}
