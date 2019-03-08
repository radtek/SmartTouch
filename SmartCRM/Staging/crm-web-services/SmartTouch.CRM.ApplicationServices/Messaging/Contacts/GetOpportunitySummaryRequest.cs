using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetOpportunitySummaryRequest : ServiceRequestBase
    {
        public int ContactId { get; set; }
        public string Period { get; set; }
    }
    public class GetOpportunitySummaryResponse : ServiceResponseBase
    {
      public OpportunitySummary OpportunitySummary { get; set; }
    }
}
