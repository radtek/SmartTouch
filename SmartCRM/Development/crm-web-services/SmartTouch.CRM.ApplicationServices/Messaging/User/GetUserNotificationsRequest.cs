using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserNotificationsRequest : ServiceRequestBase
    {
        public IEnumerable<int> UserIds { get; set; }
        public IEnumerable<int> ModuleIds { get; set; }
        public bool TodayNotifications { get; set; }
    }

    public class GetUserNotificationsResponse : ServiceResponseBase
    {
        public IEnumerable<Notification> Notifications { get; set; }
        public IEnumerable<Notification> WebVisitNotifications { get; set; }

    }
}
