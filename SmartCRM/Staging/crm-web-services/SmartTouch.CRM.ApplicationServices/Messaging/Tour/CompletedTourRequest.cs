using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Tour
{
    public class CompletedTourRequest : ServiceRequestBase
    {
        public int tourId { get; set; }
        public bool isCompleted { get; set; }
        public bool CompletedForAll { get; set; }
        public int? contactId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool AddToContactSummary { get; set; }
    }

    public class CompletedTourResponse : ServiceResponseBase
    {
    }
}
