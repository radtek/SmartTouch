using SmartTouch.CRM.ApplicationServices.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Reports
{
    public class InsertDefaultReportsRequest : ServiceRequestBase
    {

    }

    public class InsertDefaultReportsResponse : ServiceResponseBase
    {

    }

    public class InsertUserDashboardSettingsRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
        public IEnumerable<DashboardSettingViewModel> DashboardViewModel { get; set; }
    }

    public class InsertUserDashboardSettingsResponse : ServiceResponseBase
    {

    }

    public class GetDashboardItemsRequest : ServiceRequestBase
    {
        public int UserId { get; set; }
    }

    public class GetDashboardItemsResponse : ServiceResponseBase
    {
        public IEnumerable<dynamic> DashboardSettingViewModel { get; set; }
    }
}
