using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.ImportData
{
    public class GetContactEmailsRequest : ServiceRequestBase
    {
        //public int JobID { get; set; }
        public string EntityIds { get; set; }
        public NeverBounceEntityTypes EntityType { get; set; }
    }

    public class GetContactEmailsResponse : ServiceResponseBase
    {
        public IEnumerable<ReportContact> Contacts { get; set; }
    }
}
