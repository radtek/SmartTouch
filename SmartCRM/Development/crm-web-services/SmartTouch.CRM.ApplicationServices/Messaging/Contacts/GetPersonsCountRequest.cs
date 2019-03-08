using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetPersonsCountRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
    }

    public class GetPersonsCountResponse : ServiceRequestBase
    {
        public int PersonsCount { get; set; }
        public List<int> Persons { get; set; }
    }
}
