using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class GetContactsCountRequest : ServiceRequestBase
    {
        public int Id { get; set; }
    }
    public class GetContactsCountResponse : ServiceResponseBase
    {
        public int Count { get; set; }
        public bool SelectAll { get; set; }
    }
}
