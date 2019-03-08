using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetImportedContactsRequest : ServiceRequestBase
    {
        public DateTime LastModifiedOn { get; set; }
    }
    public class GetImportedContactsResponse : ServiceResponseBase
    {
        public List<int> ContactIds { get; set; }
        public List<int> TagIds { get; set; }
    }
}
