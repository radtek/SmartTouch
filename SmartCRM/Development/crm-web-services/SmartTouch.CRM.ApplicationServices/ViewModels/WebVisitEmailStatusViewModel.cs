using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class WebVisitEmailViewModel
    {
        public string ContactName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string VisitTime { get; set; }
        public short PageViews { get; set; }
        public int Duration { get; set; }
        public string TopPage1 { get; set; }
        public string TopPage2 { get; set; }
        public string TopPage3 { get; set; }
        public string Source { get; set; }
        public string Location { get; set; }
    }
}
