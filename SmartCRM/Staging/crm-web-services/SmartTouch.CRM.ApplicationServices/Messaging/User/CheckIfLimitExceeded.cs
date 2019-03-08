using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class CheckIfLimitExceededRequest : ServiceRequestBase
    {

    }

    public class CheckIfLimitExceededResponse : ServiceResponseBase
    {
        public bool IsLimitExceeded { get; set; }
    }
}
