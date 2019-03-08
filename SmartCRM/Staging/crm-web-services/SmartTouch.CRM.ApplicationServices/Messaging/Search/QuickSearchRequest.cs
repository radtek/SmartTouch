using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.SearchEngine.Search;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Search
{
    public class QuickSearchRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public int PageNumber { get; set; }
        public string Query { get; set; }
        public IEnumerable<SearchableEntity> SearchableEntities { get; set; }
    }

    public class QuickSearchResponse : ServiceResponseBase
    {
        public SearchResult<Suggestion> SearchResult { get; set; }
    }
}
