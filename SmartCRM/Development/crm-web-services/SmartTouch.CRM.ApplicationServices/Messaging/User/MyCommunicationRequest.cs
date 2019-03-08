using SmartTouch.CRM.Domain.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.User
{
    public class MyCommunicationRequest:ServiceRequestBase
    {
        public int UserId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MyCommunicationResponse : ServiceResponseBase
    {
        public IEnumerable<MyCommunication> CommunicationDetails { get; set; }
    }
}
