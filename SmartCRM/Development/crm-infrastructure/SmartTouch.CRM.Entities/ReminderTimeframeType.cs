using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ReminderTimeframeType: byte
    {
        Today = 1,
        Tomorrow = 2,
        [Description("A week from now")] AWeekFromNow = 3,
        [Description("Two weeks from now")] TwoWeeksFromNow = 4,
        [Description("A month from now")] AMonthFromNow = 5,
        [Description("On a date")] OnADate = 6,
    }
}
