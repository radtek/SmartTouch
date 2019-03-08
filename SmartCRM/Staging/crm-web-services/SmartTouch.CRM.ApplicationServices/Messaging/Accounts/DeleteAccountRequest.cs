using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class DeleteAccountRequest : IntegerIdRequest
    {
        public DeleteAccountRequest(int id) : base(id) { }
    }

    public class DeleteAccountResponse : ServiceResponseBase
    {
    }
}
