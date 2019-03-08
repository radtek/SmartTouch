using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor.QuartzScheduler
{
    public class CampaignSendEmailRequest : IJobRequest
    {
        public long CampaignId { get; set; }

        public CampaignSendEmailRequest(int Id)
        {
            CampaignId = Id;
        }
    }
}
