using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class DeactivateUserRequest : ServiceRequestBase
    {
        public int[] UserID { get; set; }
    }

    public class DeactivateUserResponse : ServiceResponseBase
    {
    }


}
