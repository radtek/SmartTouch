using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Enterprices
{
    public class GetAllReportedCouponsRequest: ServiceRequestBase
    {
        public int Pagesize { get; set; }
        public int PageNumber { get; set; }
    }
   
    public class GetAllReportedCouponsResponse : ServiceResponseBase
    {
        public int TotalHits { get; set; }
        public IEnumerable<ReportedCouponsViewModel> ReportedCouponsViewModel { get; set; }
    }
}
