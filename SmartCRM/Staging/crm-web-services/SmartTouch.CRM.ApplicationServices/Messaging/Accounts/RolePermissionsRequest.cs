using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class RolePermissionsRequest : ServiceRequestBase
    {
        public RolePermissionsViewModel RolePermissionsViewModel { get; set; }
    }

    public class RolePermissionsResponse : ServiceResponseBase
    {

    }
}
