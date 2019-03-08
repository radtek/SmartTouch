using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class DeactivateActionContactRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public int ActionId { get; set; }
    }

    public class DeactivateActionContactResponse : ServiceResponseBase
    {
        public bool Result { get; set; }
    }
}
