using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class BulkoperationConactsInsertionRequest:ServiceRequestBase
    {
       public IEnumerable<BulkContact> ConactsData { get; set; }
    }

    public class BulkoperationConactsInsertionResponse : ServiceResponseBase
    {

    }
}
