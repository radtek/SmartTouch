using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class ChangeRoleRequest : ServiceRequestBase
    {
        public short RoleID { get; set; }
        public int[] UserID { get; set; }
    }

    public class ChangeRoleResponse : ServiceResponseBase
    { }
}
