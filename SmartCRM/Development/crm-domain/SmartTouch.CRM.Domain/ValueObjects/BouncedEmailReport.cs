using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class BouncedEmailReport : ValueObjectBase
    {
        public string Email { get; set; }
        public string Account { get; set; }
        public string BounceType { get; set; }
        public DateTime Date { get; set; }
        public int TotalCount { get; set; }
        public string BouncedReason { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
