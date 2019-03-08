using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class ViewLeadAdapterViewModel
    {
       public string FileName { get; set; }
       public DateTime ImportDate { get; set; }
       public LeadAdapterJobStatus LeadAdapterJobStatus { get; set; }
       public int LeadAdapterJobID { get; set; }
       public int LeadAdapterID { get; set; }
       public string LeadAdapterName { get; set; }
       public bool SuccessRecords { get; set; }
       public bool FailureRecords { get; set; }
    }
}
