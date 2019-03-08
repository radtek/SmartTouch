using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserNotificationCountRequest : ServiceRequestBase
    {        
    }

    public class GetUserNotificationCountResponse : ServiceResponseBase
    {
        public int Count { get; set; }
        public IEnumerable<int> NotificationIds { get; set; }
    }
}
