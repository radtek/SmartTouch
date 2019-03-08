using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class IsAccountAdminExistRequest : ServiceRequestBase
    {
    }

    public class IsAccountAdminExistResponse : ServiceResponseBase
    {
        public bool AccountAdminExist { get; set; }
    }
}
