using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetTagContactsRequest: ServiceRequestBase
    {
        public int TagID { get; set; }
        public int TagType { get; set; }
    }

    public class GetTagContactsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
