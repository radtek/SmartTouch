using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class DeleteNotificationRequest : ServiceRequestBase
    {
        public IEnumerable<int> NotificationIds { get; set; }
        //public bool IsActionType { get; set; }
        //public bool IsTourType { get; set; }
        public bool IsBulkDelete { get; set; }

        //In case of bulk remove
        public byte ModuleId { get; set; }
        public bool ArePreviousNotifications { get; set; }
    }

    public class DeleteNotificationResponse : ServiceResponseBase
    {
        public int DeletedCount { get; set; }
    }
}
