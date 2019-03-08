using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FacebookLead : ValueObjectBase
    {
        public DateTime CreatedTime { get; set; }
        public string ID { get; set; }
        public IEnumerable<FacebookFieldData> FieldData { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class FacebookFieldData
    {
        public string Name { get; set; }
        public string[] Values { get; set; }
    }
}
