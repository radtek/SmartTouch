using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class AddNotificationRequest : ServiceRequestBase
    {
        public Notification Notification { get; set; }
    }

    public class AddNotificationResponse : ServiceResponseBase
    {
 
    }
}
