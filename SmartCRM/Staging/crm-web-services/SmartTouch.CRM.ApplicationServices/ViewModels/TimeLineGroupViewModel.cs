using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class TimeLineGroupViewModel
    {
        public int Year { get; set; }
        public int YearCount { get; set; }
        public IEnumerable<TimeLineMonthGroupViewModel> Months { get; set; }
    }

    public class TimeLineMonthGroupViewModel
    {
        public int Month { get; set; }
        public int MonthCount { get; set; }        
    }
}
