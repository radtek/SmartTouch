using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetUsersRequest : ServiceRequestBase
    {
        public int AccountID { get; set; }
        public int UserId { get; set; }
        public Boolean IsSTadmin { get; set; }
        public GetUsersRequest() {
          
        }
    }

    public class GetUsersResponse : ServiceResponseBase
    {
        public GetUsersResponse() { }

        public IEnumerable<Owner> Owner { get; set; }
    }
}
