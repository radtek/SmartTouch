using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
        public class GetContactTypeRequest : IntegerIdRequest
        {
            public GetContactTypeRequest(int id) : base(id) { }
        }

        public class GetContactTypeResponse : ServiceResponseBase
        {
            public ContactType ContactType { get; set; }
        }
}
