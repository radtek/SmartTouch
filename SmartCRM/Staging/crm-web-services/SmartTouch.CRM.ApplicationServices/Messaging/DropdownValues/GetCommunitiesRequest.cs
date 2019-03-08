using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.DropdownValues
{
    public class GetCommunitiesRequest : ServiceRequestBase
    {
        public int DropdownId { get; set; }
    }

    public class GetCommunitiesResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Communities { get; set; }
    }
}
