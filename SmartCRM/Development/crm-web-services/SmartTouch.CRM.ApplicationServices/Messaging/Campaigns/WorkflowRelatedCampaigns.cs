using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetWorkflowRelatedCampaignsRequest : ServiceRequestBase
    {
        public short WorkflowID { get; set; }
    }
    public class GetWorkflowRelatedCampaignsResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignViewModel> Campaigns {get;set;}
    }
    
}
