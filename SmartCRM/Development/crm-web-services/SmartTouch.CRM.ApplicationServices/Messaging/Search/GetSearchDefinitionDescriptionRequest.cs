using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class GetSearchDefinitionDescriptionRequest : ServiceRequestBase
    {
        public int SearchDefinitionId { get; set; }
    }

    public class GetSearchDefinitionDescriptionRespone : ServiceResponseBase
    {
        public string Title { get; set; }
    }
}
