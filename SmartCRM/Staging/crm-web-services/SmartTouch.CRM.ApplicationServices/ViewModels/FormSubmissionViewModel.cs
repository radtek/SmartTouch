using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class FormSubmissionEntryViewModel
    {
        public int FormSubmissionId { get; set; }
        public FormViewModel Form { get; set; }
        public PersonViewModel Person { get; set; }
        public string IPAddress { get; set; }
        public DateTime SubmittedOn { get; set; }
        public FormSubmissionStatus Status { get; set; }
        public string SubmittedData { get; set; }
    }
}
