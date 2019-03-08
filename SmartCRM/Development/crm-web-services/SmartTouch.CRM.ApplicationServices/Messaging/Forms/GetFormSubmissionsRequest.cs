using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Forms
{
    public class GetFormSubmissionsRequest: ServiceRequestBase
    {
        public int FormId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int PageNumber { get; set; }
        public int PageLimit { get; set; }
    }

    public class GetFormSubmissionsResponse : ServiceResponseBase
    {
        public IEnumerable<FormSubmissionEntryViewModel> FormSubmissions { get; set; }
        public int TotalHits { get; set; }
    }
}
