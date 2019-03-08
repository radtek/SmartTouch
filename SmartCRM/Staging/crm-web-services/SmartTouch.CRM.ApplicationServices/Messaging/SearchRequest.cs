using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging
{
    public class SearchRequest : ServiceRequestBase
    {
        public string Query { get; set; }
        public int PageNumber { get; set; }
    }

    public class SearchResponse<T> : ServiceResponseBase
    {
        public IEnumerable<T> Results { get; set; }
        public int TotalHits { get; set; }
    }
}
