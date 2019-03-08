using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DateFormat : ValueObjectBase
    {
        public int DateFormatId { get; set; }
        public string FormatName { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
