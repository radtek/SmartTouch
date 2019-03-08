using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class WebVisitViewModel
    {
        public int ContactWebVisitID { get; set; }
        public int ContactID { get; set; }
        public DateTime VisitedOn { get; set; }
        public string PageVisited { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ISPName { get; set; }
        public short Duration { get; set; }
        public string IPAddress { get; set; }
        public bool IsVisit { get; set; }
        public string VisitReference { get; set; }
        public string ContactReference { get; set; }
    }
}
