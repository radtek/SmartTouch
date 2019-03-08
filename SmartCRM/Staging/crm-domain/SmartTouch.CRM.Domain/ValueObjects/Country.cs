using System;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class Country : ValueObjectBase
    {
        public string Code { get; set; }
        public string Name { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
