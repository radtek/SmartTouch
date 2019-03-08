using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetSavedSearchContactIdsRequest : ServiceRequestBase
    {
        public int SearchDefinitionId { get; set; }
        public int[] SearchDefinitionIds { get; set; }
        public bool IsActiveContactsSearch { get; set; }
    }

    public class GetSavedSearchContactIdsResponse : ServiceResponseBase
    {
        public IEnumerable<int> ContactIds { get; set; }
    }
}
