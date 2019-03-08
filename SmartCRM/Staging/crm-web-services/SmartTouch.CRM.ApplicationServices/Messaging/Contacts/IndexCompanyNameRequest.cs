using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class IndexCompanyNameRequest : ServiceRequestBase
    {
    }

    public class IndexCompanyNameResponse : ServiceResponseBase
    {
        public int IndexedCount { get; set; }
    }
}
