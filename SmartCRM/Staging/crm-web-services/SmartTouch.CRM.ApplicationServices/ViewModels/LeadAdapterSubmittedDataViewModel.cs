using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class LeadAdapterSubmittedDataViewModel
    {
        public int JobLogID { get; set; }
        public string SubmittedData { get; set; }
        public List<LeadadapterSubmittedDetails> submittedDetails { get; set; }

    }
}
