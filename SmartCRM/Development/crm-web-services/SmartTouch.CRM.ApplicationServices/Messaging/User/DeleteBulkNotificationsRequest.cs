using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class DeleteBulkNotificationsRequest : ServiceRequestBase
    {
        public bool ArePreviousNotifications { get; set; }
        public IEnumerable<int> ModuleIds { get; set; }
    }

    public class DeleteBulkNotificationsResponse : ServiceResponseBase
    {
        public int DeletedCount { get; set; }
    }
}
