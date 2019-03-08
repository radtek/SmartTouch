using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class SubmittedFormFieldViewModel
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class SubmittedFormViewModel
    {
        public IList<SubmittedFormFieldViewModel> SubmittedFormFields { get; set; }
        public int FormId { get; set; }
        public int AccountId { get; set; }
        public string IPAddress { get; set; }
        public DateTime SubmittedOn { get; set; }
        public SubmittedFormStatus Status { get; set; }
        public int OwnerId { get; set; }
        public string SubmittedData { get; set; }
        public short LeadSourceID { get; set; }
        public string STITrackingID { get; set; }
        public int? CreatedBy { get; set; }
        public int SubmittedFormDataID { get; set; }
    }

    public class SubmittedFormAPIViewModel
    {
        public Dictionary<string,string> SubmittedFormFields { get; set; }
        public int FormId { get; set; }
        public int AccountId { get; set; }
    }
}
