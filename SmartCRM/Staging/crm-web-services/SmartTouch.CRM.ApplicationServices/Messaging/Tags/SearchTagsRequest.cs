using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tags
{
    public class SearchTagsRequest : ServiceRequestBase
    {
        public string Query { get; set; }
        public int Limit { get; set; }
    }

    public class SearchTagsResponse : ServiceResponseBase
    {
        public IEnumerable<TagViewModel> Tags { get; set; }
    }
}
