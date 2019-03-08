using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class DeleteContactRequest: IntegerIdRequest
    {
        public DeleteContactRequest(int id) : base(id) { }
    }

    public class DeleteContactResponse: ServiceResponseBase
    {   
    }
}
