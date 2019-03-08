using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class UserStatusRequest : ServiceRequestBase
    {

        public int[] UserID { get; set; }
        public byte Status { get; set; }
    }

    public class UserStatusResponse : ServiceResponseBase
    {
    }

}
