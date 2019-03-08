using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class TourByContactsRequest:ServiceRequestBase
    {
       public DateTime FromDate { get; set; }
       public DateTime ToDate { get; set; }
       public int[] TourStatus { get; set; }
       public int[] TourType { get; set; }
       public int[] TourCommunity { get; set; }
       public int pageSize { get; set; }
       public int pageNumber { get; set; }
       public string SortField { get; set; }
       public string SortDirection { get; set; }
    }

    public class TourByContactsReponse : ServiceResponseBase
    {
        public IEnumerable<TourByContactsViewModel> TourByContactsReportData { get; set; }
    }
}
