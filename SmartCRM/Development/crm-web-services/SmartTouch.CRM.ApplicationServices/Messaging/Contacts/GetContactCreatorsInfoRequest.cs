using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetContactCreatorsInfoRequest : ServiceRequestBase
    {
        public IEnumerable<int> ContactIDs { get; set; }
    }

    public class GetContactCreatorsInfoResponse : ServiceResponseBase
    {
        public IEnumerable<ContactCreatorInfo> GetContactCreatorsInfo { get; set; }
    }
}
