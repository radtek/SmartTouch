using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetPrivateModulesRequest : ServiceRequestBase
    {

    }
    public class GetPrivateModulesResponse : ServiceResponseBase
    {
        public IEnumerable<byte> ModuleIds { get; set; }
    }
}
