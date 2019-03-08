using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CalenderTimeSlotViewModel
    {
        public DateTime start { get; set; }
        public DateTime end { get; set; }
        public string title { get; set; }
        public string description { get; set; }
    }
}
