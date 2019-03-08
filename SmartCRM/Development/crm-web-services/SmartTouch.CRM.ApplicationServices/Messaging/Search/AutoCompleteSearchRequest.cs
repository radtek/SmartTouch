using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.SearchEngine.Search;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class AutoCompleteSearchRequest : ServiceRequestBase
    {
        public string Query {get;set;}
    }

    public class AutoCompleteSearchResponse: ServiceResponseBase
    {
        public IEnumerable<Suggestion> Results { get; set; }
    }

    public class AutoCompleteResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Results { get; set; }
    } 
}
