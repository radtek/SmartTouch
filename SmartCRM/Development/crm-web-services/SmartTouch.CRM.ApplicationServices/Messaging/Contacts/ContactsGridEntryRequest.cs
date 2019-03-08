using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class ContactsGridEntryRequest:ServiceRequestBase
    {
    }

    public class ContactsGridEntryResponse : ServiceResponseBase
    {
       public IEnumerable<ContactGridEntry> Contacts { get; set; }
        public int TotalHits { get; set; }
    }
}
