using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetWorkflowContactsRequest : ServiceRequestBase
    {
        public short WorkflowID { get; set; }
        public int? CampaignID { get; set; }
        public WorkflowContactsState WorkflowContactState { get; set; }
        public CampaignDrillDownActivity CampaignDrillDownsActivity { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class GetWorkflowContactsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
