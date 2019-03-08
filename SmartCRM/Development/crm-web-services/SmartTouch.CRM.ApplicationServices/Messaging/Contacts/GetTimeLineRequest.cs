using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.ViewModels;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Contacts
{
    public class GetTimeLineRequest : ServiceRequestBase
    {        
        public int? ContactID { get; set; }
        public int? OpportunityID { get; set; }
        public int Limit { get; set; }
        public string Module { get; set; }
        public string Period { get; set; }
        public string PageName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int PageNumber { get; set; }
        public string TimeZone { get; set; }
        public string DateFormat { get; set; }
        public string[] Activities { get; set; }        
    }

    public class GetTimeLineResponse : ServiceResponseBase
    {
        public int TotalRecords { get; set; }
        public IList<TimeLineEntryViewModel> lsttimelineViewModel { get; set; }
        public IEnumerable<TimeLineGroupViewModel> timeLineGroup { get; set; }
    }
}
