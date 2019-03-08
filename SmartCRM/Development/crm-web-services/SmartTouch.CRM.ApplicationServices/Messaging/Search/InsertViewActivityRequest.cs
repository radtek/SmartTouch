using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class InsertViewActivityRequest : ServiceRequestBase
    {
        public int SearchDefinitionId { get; set; }
        public string SearchName { get; set; }
    }

    public class InsertViewActivityResponse : ServiceResponseBase
    {

    }
}
