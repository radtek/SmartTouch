using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class LeadadapterSubmittedDetails
    {
        public string Title { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public LeadAdapterSubmittedDataItem DataItem { get; set; }
    }
    public class LeadAdapterSubmittedDataItem
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        
    }
}
