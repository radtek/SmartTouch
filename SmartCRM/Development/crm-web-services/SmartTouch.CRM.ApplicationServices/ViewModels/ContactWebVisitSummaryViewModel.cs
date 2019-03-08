using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactWebVisitSummaryViewModel
    {
        public string VisitReference { get; set; }
        public DateTime VisitedOn { get; set; }
        public short PageViews { get; set; }
        public int Duration { get; set; }
        public string Page1 { get; set; }
        public string Page2 { get; set; }
        public string Page3 { get; set; }
        public string Source { get; set; }
        public string Location { get; set; }
        public int? OwnerID { get; set; }
    }
}
