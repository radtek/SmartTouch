using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class FindMatchedSavedSearchQueryRequest : ServiceRequestBase
    {
        public Contact Contact { get; set; }
    }

    public class FindMatchedSavedSearchQueryResponse : ServiceRequestBase
    {
        public int MatchedQueries { get; set; }
    }
}
