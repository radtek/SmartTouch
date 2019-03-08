using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Domain.Tags;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class ReIndexTagsRequest:ServiceRequestBase
    {
    }

    public class ReIndexTagsResponse:ServiceResponseBase
    {
        public int IndexedTags { get; set; }
    }
}
