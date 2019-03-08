using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync
{
    public class GetLoginInfoByUsernameRequest : ServiceRequestBase
    {
        public string UserName { get; set; }
    }

    public class GetLoginInfoByUsernameResponse : ServiceResponseBase
    {
        public KeyValuePair<int,string> UserInfo { get; set; }
    }
}
