using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class TimeLineGroup
    {
        public int Year { get; set; }
        public int YearCount { get; set; }
        public IEnumerable<TimeLineMonthGroup> Months { get; set; }
    }

    public class TimeLineMonthGroup
    {
        public int Month { get; set; }
        public int MonthCount { get; set; }
    }
}
