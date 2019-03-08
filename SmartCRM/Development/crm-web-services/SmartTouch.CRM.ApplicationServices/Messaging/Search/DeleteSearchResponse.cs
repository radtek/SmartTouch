using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class DeleteSearchRequest:ServiceRequestBase
    {
        public List<int> SearchIDs { get; set; }
    }

    public class DeleteSearchResponse : ServiceResponseBase
    {
        public string ResponseMessage { get; set; }
    }
}
