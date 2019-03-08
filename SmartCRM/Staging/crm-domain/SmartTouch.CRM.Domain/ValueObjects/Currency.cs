using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Currency : ValueObjectBase
    {
        public int CurrencyId { get; set; }
        public string Symbol { get; set; }
        public string Format { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
