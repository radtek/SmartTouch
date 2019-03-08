using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetReEngagementInfoRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<int> LinkIds { get; set; }
        public bool IsDefaultDateRange { get; set; }
        public bool HasSelectedLinks { get; set; }
        public byte DrillDownPeriod { get; set; }
        public int ReportId { get; set; }
    }

    public class GetReEngagementInfoResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignReEngagementInfo> CampaignStats { get; set; }
        public IEnumerable<int> ContactIds { get; set; }
    }
}
