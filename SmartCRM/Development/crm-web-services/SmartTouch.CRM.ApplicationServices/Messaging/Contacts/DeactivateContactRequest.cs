using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class DeactivateContactRequest : IntegerIdRequest
    {
        public DeactivateContactRequest(int id) : base(id) { }
    }

    public class DeactivateContactResponse : ServiceResponseBase
    {
       
    }
}
