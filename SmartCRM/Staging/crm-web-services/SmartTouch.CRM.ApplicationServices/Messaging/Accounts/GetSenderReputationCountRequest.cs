using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class GetSenderReputationCountRequest : ServiceRequestBase
    {
      //  public int AccountId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    public class GetSenderReputationCountResponse : ServiceResponseBase
    {
        public int ReputationCount { get; set; }
    }
}
