using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetMyCommunicationContactsRequest:ServiceRequestBase
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Activity { get; set; }
        public string ActivityType { get; set; }
    }

    public class GetMyCommunicationContactsResponse : ServiceResponseBase
    {
        public IList<int> ContactIdList { get; set; }
    }
}
