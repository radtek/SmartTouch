using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ImportListViewModel 
    {
        public int LeadAdapterJobLogID { get; set; }
        public int LeadAdapterAndAccountMapID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public byte LeadAdapterJobStatusID { get; set; }
        public string Remarks { get; set; }
        public string FileName { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int RecordCreated { get; set; }
        public int RecordUpdated { get; set; }
        public int TotalRecords { get; set; }
        public string LeadAdapterJobStatus { get; set; }

        public bool IsValidated { get; set; }
        public string BadEmailsData { get; set; }
        public string GoodEmailsData { get; set; }
        public int NeverBounceRequestID { get; set; }
    }
}
