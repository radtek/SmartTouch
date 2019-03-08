using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class InsertEmailOpenOrClickEntryRequest:ServiceRequestBase
    {
        public int SentMailDetailID { get; set; }
        public int ContactID { get; set; }
        public int? EmailLinkID { get; set; }
        public byte ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public string IPAddress { get; set; }

    }

    public class InsertEmailOpenOrClickEntryResponse : ServiceResponseBase
    {

    }
}
