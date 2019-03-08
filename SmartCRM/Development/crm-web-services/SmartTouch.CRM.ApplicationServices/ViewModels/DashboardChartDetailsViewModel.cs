using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
  public  class DashboardChartDetailsViewModel
    {
      public IEnumerable<dynamic> Chart1Details { get; set; }
      public IEnumerable<dynamic> Chart2Details { get; set; }
      public int? PreviousCount { get; set; }
      public int? PresentCount { get; set; }
      public int MaxValue { get; set; }
      public int? ReportId { get; set; }
    }

}
