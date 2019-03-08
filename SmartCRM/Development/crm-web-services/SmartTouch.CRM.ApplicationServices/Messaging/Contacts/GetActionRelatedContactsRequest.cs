using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetActionRelatedContactsRequest : ServiceRequestBase
    {
        public int ActionID { get; set; }
    }

    public class GetActionRelatedContactsResponce : ServiceResponseBase
    {
        public IList<int> ContactIdList { get; set; }
    }
}
