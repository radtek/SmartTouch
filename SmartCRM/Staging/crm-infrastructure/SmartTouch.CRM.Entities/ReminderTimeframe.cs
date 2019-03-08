using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Entities
{
    public enum ReminderTimeframe : byte
    { 
       Today = 1,
       Tomorrow = 2,
       TwoWeeks = 3,
       NextWeek = 4,
       OnaDate = 5
    }
}
