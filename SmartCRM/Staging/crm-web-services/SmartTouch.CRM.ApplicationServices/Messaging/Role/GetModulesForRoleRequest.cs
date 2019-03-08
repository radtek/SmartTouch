using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Role
{
    public class GetModulesForRoleRequest : ServiceRequestBase
    {
        public short roleId { get; set; }
    }

    public class GetModulesForRoleResponse : ServiceResponseBase
    {
        public List<byte> moduleIds { get; set; } 
    }
}
