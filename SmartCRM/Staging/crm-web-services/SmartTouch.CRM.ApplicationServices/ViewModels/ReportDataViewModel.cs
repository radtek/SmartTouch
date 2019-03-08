using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ReportDataViewModel
    {
        public string Data { get; set; }
        public string Module { get; set; }
        public string AccountName { get; set; }
        public int AccountID { get; set; }
        public int Count { get; set; }
    }
}
