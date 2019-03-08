using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Infrastructure.Domain;
using System;


namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignRecipientTypes
    {
        public long CampaignRecipientsCount { get; set; }
        public long CampaignActiveRecipientsCount { get; set; }
        public long CampaignALLandACTIVERecipientsCount { get; set; }
        public long CampaignACTIVEandALLRecipientsCount { get; set; }

        public long AllContactsByTag { get; set; }
        public long ActiveContactsByTag { get; set; }
        public long AllContactsBySS { get; set; }
        public long ActiveContactsBySS { get; set; }
    }
}
