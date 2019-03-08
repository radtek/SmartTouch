using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{

    public class WebVisitReportViewModel
    {
        public int ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string VisitReference { get; set; }
        public string VisitedOn { get; set; }
        public short PageViews { get; set; }
        public int Duration { get; set; }
        public string Page1 { get; set; }
        public string Page2 { get; set; }
        public string Page3 { get; set; }
        public string Source { get; set; }
        public string Location { get; set; }
    }
}
