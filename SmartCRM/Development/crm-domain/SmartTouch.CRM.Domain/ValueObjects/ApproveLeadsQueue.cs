using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ApproveLeadsQueue : ValueObjectBase
    {
        public string LeadSourceType { get; set; }
        public int TotalCount { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        //public string MobilePhone { get; set; }
        public int SubmittedFormDataID { get; set; }
        public string Remarks { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
