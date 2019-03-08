using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetSearchDefinitionContactsRequest : ServiceRequestBase
    {
        public int SearchDefinitionId { get; set; }
        public byte ContactsType { get; set; }
        public List<int> ContactIds { get; set; }
    }
    public class GetSearchDefinitionContactsResponce : ServiceResponseBase
    {
        public IEnumerable<int> ContactIdList { get; set; }
    }
}
