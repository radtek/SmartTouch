using Quartz;
using System;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler
{
    public class CampaignEmailRequest : IJobRequest
    {
        public long CampaignId { get; set; }

        public CampaignEmailRequest(int Id)
        {
            CampaignId = Id;
        }
    }
}
