using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Role
{
    public class InsertRolePermissionsRequest : ServiceRequestBase
    {
        public RolePermissionsViewModel rolePermissionsViewModel { get; set; } 
    }

    public class InsertRolePermissionsResponse : ServiceResponseBase
    {

    }
}
