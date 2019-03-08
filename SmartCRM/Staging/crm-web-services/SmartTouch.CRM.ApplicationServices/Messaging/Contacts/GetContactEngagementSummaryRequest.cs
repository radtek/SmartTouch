using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactEngagementSummaryRequest:ServiceRequestBase
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public int ContactId { get; set; }
        public int Type { get; set; }
    }

    public class GetContactEngagementSummaryResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<ContactWorkflowSummary> ContactWorkflowDetails { get; set; }
        public IEnumerable<ContactEmailSummary> ContactEmailDetails { get; set; }
        public IEnumerable<ContactCampaigSummary> ContactCampaignDetails { get; set; }
    }
}
