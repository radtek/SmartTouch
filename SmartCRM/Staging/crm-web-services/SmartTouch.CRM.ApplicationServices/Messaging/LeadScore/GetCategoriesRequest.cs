using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadScore
{
    public class GetCategoriesRequest : ServiceRequestBase
    {
        public int accountId { get; set; }
    }

    public class GetCategoriesResponse : ServiceResponseBase
    {
        public GetCategoriesResponse() { }

        public IEnumerable<dynamic> Categories { get; set; }
    }
}
