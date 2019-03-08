using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserRoleRequest: ServiceRequestBase
    {
        public int UserId { get; set; }
    }

    public class GetUserRoleResponse : ServiceResponseBase
    {
        public RoleViewModel Role { get; set; }
    }
}
