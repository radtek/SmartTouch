using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class TimeLineViewModel
    {
        public int PageNumber { get; set; }
        public string PageName { get; set; }
        public DateTime CustomStartDate { get; set; }
        public DateTime CustomEndDate { get; set; }
        public string[] Activities { get; set; }
        public int? ContactID { get; set; }
        public int? OpportunityID { get; set; }
        public int Limit { get; set; }
    }
}
