using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetRoleRequest : ServiceRequestBase
    {
        public GetRoleRequest() { }
        public int AccountID { get; set; }
    }

    public class GetRoleResponse : ServiceResponseBase
    {
        public GetRoleResponse() { }

        public IEnumerable<dynamic> Roles { get; set; }
        public byte SubscriptionId { get; set; }
    }
}
