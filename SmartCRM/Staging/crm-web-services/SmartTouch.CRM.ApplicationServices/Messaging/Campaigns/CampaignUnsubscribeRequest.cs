using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
   public class CampaignUnsubscribeRequest:ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public int ContactId { get; set; }
        public string Email { get; set; }
        public DateTime SnoozeUntil { get; set; }
    }
   public class CampaignUnsubscribeResponse : ServiceResponseBase
   {
       public int? contactId { get; set; }
   }
}
