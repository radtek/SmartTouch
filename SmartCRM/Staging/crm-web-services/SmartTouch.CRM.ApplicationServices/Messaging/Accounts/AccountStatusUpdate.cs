using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class AccountStatusUpdateRequest : ServiceRequestBase
    {
        public int[] AccountID { get; set; }
        public byte StatusID { get; set; }
        public string[] AccountNames { get; set; }
    }

    public class AccountStatusUpdateResponse : ServiceResponseBase
    {
        public List<string> Toemails { get; set; }
    }
}
