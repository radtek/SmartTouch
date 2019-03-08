using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Action
{
    public class CompletedActionRequest : ServiceRequestBase
    {
        public int actionId { get; set; }
        public bool isCompleted { get; set; }
        public bool CompletedForAll { get; set; }
        public int? contactId { get; set; }
        public int opportunityId { get; set; }
        public DateTime UpdatedOn { get; set; }
        public bool IsSchedule { get; set; }
        public int? MailBulkId { get; set; }
        public Guid? GroupId { get; set; }
        public bool AddToNoteSummary { get; set; }

    }

    public class CompletedActionResponse : ServiceResponseBase
    { 
    }
}
