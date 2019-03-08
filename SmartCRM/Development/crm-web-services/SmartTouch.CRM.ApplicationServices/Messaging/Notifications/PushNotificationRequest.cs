using SmartTouch.CRM.Domain.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Notifications
{
    public class PushNotificationRequest:ServiceRequestBase
    {
        public PushNotification PushNotification { get; set; }
    }

    public class PushNotificationResponse : ServiceResponseBase
    {
        public string Message { get; set; }
    }
}
