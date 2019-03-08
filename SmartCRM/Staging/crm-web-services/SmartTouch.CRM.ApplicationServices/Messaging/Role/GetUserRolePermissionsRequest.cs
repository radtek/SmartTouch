using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Role
{
    public class GetUserRolePermissionsRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
    }

    public class GetUserRolePermissionsResponse : ServiceResponseBase
    {
        public List<UserPermission> UserPermissions { get; set; }
    }
}
