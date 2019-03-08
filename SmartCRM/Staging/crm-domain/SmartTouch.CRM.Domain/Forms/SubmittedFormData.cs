using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Forms
{
   public class SubmittedFormData
    {
        public int SubmittedFormDataID { get; set; }

        public int FormID { get; set; }
        public int AccountID { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreatedOn { get; set; }
        public int Status { get; set; }
        public Int16 LeadSourceID { get; set; }
        public string STITrackingID { get; set; }
        public int? CreatedBy { get; set; }
        public int? OwnerID { get; set; }
        public string SubmittedData { get; set; }
        public string Remarks { get; set; }
    }
}
