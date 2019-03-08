using SmartTouch.CRM.Domain.Campaigns;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetNextCampaignToTriggerResponse : ServiceResponseBase
    {
        public Campaign Campaign { get; set; }
    }
}
