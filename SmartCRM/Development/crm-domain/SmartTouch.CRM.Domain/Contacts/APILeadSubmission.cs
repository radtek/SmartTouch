using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class APILeadSubmission : EntityBase<int>, IAggregateRoot
    {
        public int APILeadSubmissionID { get; set; }
        public int? ContactID { get; set; }
        public int AccountID { get; set; }
        public string SubmittedData { get; set; }
        public DateTime? SubmittedOn { get; set; }
        public byte IsProcessed { get; set; }
        public string Remarks { get; set; }
        public int OwnerID { get; set; }
        public int FormID { get; set; }
        public string IPAddress { get; set; }


        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
