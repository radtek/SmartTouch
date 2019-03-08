using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.LeadAdapters
{
    public class GetLeadAdapterSubmissionRequest : ServiceRequestBase
    {
        public int JobLogDetailID { get; set; }
    }

    public class GetLeadAdapterSubmissionResponse : ServiceResponseBase
    {
        public LeadAdapterSubmittedDataViewModel LeadAdapterSubmission { get; set; }
    }
}
