using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactCampaignStatisticsRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public DateTime Period { get; set; }
    }
    public class GetContactCampaignStatisticsResponse :ServiceRequestBase
    {
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
    }
}
