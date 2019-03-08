using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class CheckContactDuplicateRequest : ServiceRequestBase
    {
        public PersonViewModel PersonVM { get; set; }
        public CompanyViewModel CompanyVM { get; set; }
        public Person Person { get; set; }
        public Company Company { get; set; }
    }

    public class CheckContactDuplicateResponse : ServiceResponseBase
    {
        public IEnumerable<Contact> Contacts { get; set; }
    }
}
