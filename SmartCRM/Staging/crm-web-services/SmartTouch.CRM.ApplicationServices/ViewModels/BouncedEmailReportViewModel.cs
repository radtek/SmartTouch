using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class BouncedEmailReportViewModel
    {
        public string Email { get; set; }
        public string Account { get; set; }
        public string BounceType { get; set; }
        public DateTime Date { get; set; }
        public string BouncedReason { get; set; }
    }
}
