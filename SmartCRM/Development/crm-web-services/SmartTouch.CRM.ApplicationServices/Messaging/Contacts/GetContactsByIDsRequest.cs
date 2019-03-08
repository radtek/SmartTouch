using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactsByIDsRequest:ServiceRequestBase
    {
        public IEnumerable<int> ContactIDs{ get; set; }
    }

    public class GetContactsByIDsResponse : ServiceResponseBase
    {
        public IEnumerable<Person> Contacts { get; set; }
    }
}
