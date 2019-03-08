using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetCampaignContactsRequest:ServiceRequestBase
    {
        public int CampaignID { get; set; }
        public CampaignDrillDownActivity CampaignDrillDownActivity { get; set; }
        public int? CampaignLinkID { get; set; }
    }

    public class GetCampaignContactsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
