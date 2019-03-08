using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetFormContactsRequest : ServiceRequestBase
    {
        public int FormID { get; set; }
    }

    public class GetFormContactsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
