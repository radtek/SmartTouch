using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Opportunity
{
    public class GetAllUsersRequest: ServiceRequestBase
    {
        public int AccountID { get; set; }
    }

    public class GetAllUsersResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> Users { get; set; }
    }
}
