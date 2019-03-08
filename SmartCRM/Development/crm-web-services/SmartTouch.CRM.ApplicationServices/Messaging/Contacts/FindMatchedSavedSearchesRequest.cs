using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class FindMatchedSavedSearchesRequest : ServiceRequestBase
    {
        public Contact Contact { get; set; }
    }

    public class FindMatchedSavedSearchesResponse : ServiceResponseBase
    {
        public IEnumerable<QueryMatch> MatchedSearches { get; set; }
    }
}
