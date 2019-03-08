using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetEngagementDetailsRequest : ServiceRequestBase
    {
        public int ContactID { get; set; }
        public DateTime Period { get; set; }
    }

    public class GetEngagementDetailsResponse : ServiceResponseBase
    {
        public int LeadScore { get; set; }
        public int WebVisits { get; set; }
        public CampaignStats CampaignInfo { get; set; }
        public EmailStats EmailInfo { get; set; }
        public int WorkflowsCount { get; set; }
        public int CampaignsCount { get; set; }
        public int EmailsCount { get; set; }
    }

    public class CampaignStats
    {
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
    }

    public class EmailStats
    {
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
    }
}
