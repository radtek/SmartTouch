using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class NeverBounceQueue : ValueObjectBase
    {
        public int NeverBounceRequestID { get; set; }
        public string AccountName { get; set; }
        public string FileName { get; set; }
        public DateTime CreatedDate { get; set; }
        public short Status { get; set; }
        public string StatusName { get; set; }
        public int TotalScrubQueCount { get; set; }
        public bool IsAdmin { get; set; }
        public int AccountID { get; set; }
        public int TotalImportedContacts { get; set; }
        public byte EntityType { get; set; }
        public string EntityName { get; set; }
        public int BadEmailsCount { get; set; }
        public string BadEmailsPercentage { get; set; }
        public int GoodEmailsCount { get; set; }
        public string GoodEmailsPercentage { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
