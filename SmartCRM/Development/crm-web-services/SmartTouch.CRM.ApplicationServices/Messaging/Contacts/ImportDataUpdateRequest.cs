using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ImportDataUpdateRequest : ServiceRequestBase
    {
        public IList<int> ContactIds { get; set; }
        public IList<int> TagIds { get; set; }
    }
    public class ImportDataUpdateResponce : ServiceResponseBase
    {

    }
}
