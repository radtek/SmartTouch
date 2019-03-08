using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class LeadAdapterJobLogDeailsViewModel
    {
        public int LeadAdapterJobLogDetailID { get; set; }
        public string LeadAdapterRecordStatus { get; set; }
        public string Remarks { get; set; }
        public int LeadAdapterJobLogID { get; set; }
        public Guid? ReferenceID { get; set; }
        public string RowData { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
