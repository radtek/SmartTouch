using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CalenderTimeSlot : ValueObjectBase
    {
        public DateTime? start { get; set; }
        public DateTime? end { get; set; }
        public string title { get; set; }
        public string description { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
