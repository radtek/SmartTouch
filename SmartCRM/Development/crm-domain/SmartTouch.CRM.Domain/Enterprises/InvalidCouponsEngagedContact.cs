using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Enterprises
{
    public class InvalidCouponsEngagedContact: EntityBase<int>, IAggregateRoot
    {
        public int FormSubmissionID { get; set; }
        public int ContactID { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
