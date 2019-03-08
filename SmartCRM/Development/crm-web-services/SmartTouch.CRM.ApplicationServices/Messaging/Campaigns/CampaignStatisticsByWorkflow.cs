using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class CampaignStatisticsByWorkflowRequest: ServiceRequestBase
    {
        public short WorkflowID { get; set; }
        public int CampaignID { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class CampaignStatisticsByWorkflowResponse : ServiceResponseBase
    {
        public IEnumerable<WorkflowCampaignStatistics> CampaignStatistics { get; set; }
    }
}
