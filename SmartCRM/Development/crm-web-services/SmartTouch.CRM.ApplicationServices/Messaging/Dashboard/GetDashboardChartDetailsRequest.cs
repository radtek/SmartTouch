using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Dashboard
{
    public class GetDashboardChartDetailsRequest : ServiceRequestBase
    {
        public bool IsSTadmin { get; set; }
        public int UserId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
    public class GetDashboardChartDetailsResponse : ServiceResponseBase
    {
        public DashboardChartDetailsViewModel ChartDetailsViewModel { get; set; }
    }
}
