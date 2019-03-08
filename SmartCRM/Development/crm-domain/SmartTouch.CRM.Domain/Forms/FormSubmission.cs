using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Forms
{
    public class FormSubmission : EntityBase<int>, IAggregateRoot
    {
        public int FormId { get; set; }
        public int ContactId { get; set; }
        public string IPAddress { get; set; }
        public DateTime SubmittedOn { get; set; }
        public FormSubmissionStatus StatusID { get; set; }
        public string SubmittedData { get; set; }
        public short? LeadSourceID { get; set; }
        public int TotalCount { get; set; }
        public virtual Form Form { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FormData
    {
        public string FormName { get; set; }
        public string SubmittedData { get; set; }
    }
}
