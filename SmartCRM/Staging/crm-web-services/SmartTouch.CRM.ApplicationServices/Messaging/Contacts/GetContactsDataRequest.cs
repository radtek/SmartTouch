using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactsDataRequest : ServiceRequestBase
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
    public class GetContactsDataResponce : ServiceResponseBase
    {
        public IEnumerable<Contact> Contacts { get; set; }
    }
}
