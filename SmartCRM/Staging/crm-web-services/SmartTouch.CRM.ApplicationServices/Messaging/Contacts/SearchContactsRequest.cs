using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using System.ComponentModel;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class SearchContactsRequest : ServiceRequestBase
    {
        public int Limit { get; set; }
        public string Query { get; set; }
        public int PageNumber { get; set; }
        public int[] ContactIDs { get; set; }
        public AppModules Module { get; set; }
        public ContactSortFieldType SortFieldType { get; set; }
        public ContactShowingFieldType ShowingFieldType { get; set; }

        public string SortField { get; set; }
        public ListSortDirection SortDirection { get; set; }
        public bool IsResultsGrid { get; set; }
    }

    public class SearchContactsResponse<T> : ServiceResponseBase where T: IShallowContact
    {
        public long TotalHits { get; set; }
        //public IEnumerable<ContactListEntry> Contacts { get; set; }
        public IEnumerable<T> Contacts { get; set; }
    }
}
