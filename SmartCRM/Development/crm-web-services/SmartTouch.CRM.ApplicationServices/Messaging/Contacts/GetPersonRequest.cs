using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetPersonRequest : IntegerIdRequest
    {
        public GetPersonRequest(int id) : base(id) { }
        public bool IncludeLastTouched { get; set; }
        public bool IncludeCustomFieldTabs { get; set; }
    }

    public class GetPersonResponse : ServiceResponseBase
    {
        public PersonViewModel PersonViewModel { get; set; }
    }
}
