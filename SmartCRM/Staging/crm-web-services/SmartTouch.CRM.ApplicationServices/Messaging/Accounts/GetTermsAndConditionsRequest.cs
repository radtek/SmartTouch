using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetTermsAndConditionsRequest : ServiceRequestBase
    {

    }

    public class GetTermsAndConditionsResponse : ServiceResponseBase
    {
        public string TC { get; set; }
    }
}
