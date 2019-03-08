using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetNotificationsCountByDateRequest : ServiceRequestBase
    {
        public byte Status { get; set; }
        public IEnumerable<byte> ModuleIds { get; set; }
    }

    public class GetNotificationsCountByDateResponse : ServiceResponseBase
    {
        public int[] CountByDate { get; set; }
        public List<byte> PermissionModuleIds { get; set; }

        public NotificationsCount Count { get; set; }
    }
}
