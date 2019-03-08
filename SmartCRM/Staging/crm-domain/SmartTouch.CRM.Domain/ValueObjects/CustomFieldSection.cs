using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Fields;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CustomFieldSection : ValueObjectBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CustomFieldSectionStatus StatusId { get; set; }
        public byte SortId { get; set; }
        public int TabId { get; set; }
        public int AccountId { get; set; }

        public IEnumerable<CustomField> CustomFields { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
