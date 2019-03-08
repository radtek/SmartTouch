using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Accounts
{
    public class HealthReportRequest : ServiceRequestBase
    {
        
    }

    public class HealthReportResponse : ServiceResponseBase
    {
        //public HealthReportViewModel HealthReportViewModel { get; set; }
        public List<ReportDataViewModel> Result { get; set; }
    }

    public class HealthReportViewModel
    {
        public List<ReportDataViewModel> Result { get; set; }
        //public ContactCampaignGroupByAccount ContactCampaigns { get; set; }     
    }
}
