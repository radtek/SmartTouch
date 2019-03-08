using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class ShowTCRequest : ServiceRequestBase
    {

    }

    public class ShowTCResponse : ServiceResponseBase
    {
        public bool ShowTC { get; set; }
    }
}
