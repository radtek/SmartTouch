using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class GetUserFullNameRequest : IntegerIdRequest
    {
        public GetUserFullNameRequest(int id) : base(id) { }
    }

    public class GetUserFullNameResponse : ServiceResponseBase
    {
        public string UserFullName { get; set; }
    }
}
