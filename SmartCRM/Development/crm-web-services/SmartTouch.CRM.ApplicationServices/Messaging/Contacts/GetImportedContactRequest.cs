using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetImportedContactRequest: ServiceRequestBase
    {
        public int LeadAdapterJobID { get; set; }
        public string recordStatus { get; set; }
    }

    public class GetImportedContactResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
