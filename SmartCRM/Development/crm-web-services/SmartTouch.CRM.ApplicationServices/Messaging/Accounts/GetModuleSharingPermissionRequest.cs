using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetModuleSharingPermissionRequest : ServiceRequestBase
    {
        public byte ModuleId { get; set; }
    }

    public class GetModuleSharingPermissionResponse : ServiceResponseBase
    {
        public bool DataSharing { get; set; }
    }
}
