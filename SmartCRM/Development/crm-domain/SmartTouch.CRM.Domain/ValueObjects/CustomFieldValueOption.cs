using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class FieldValueOption : ValueObjectBase
    {
        public int Id { get; set; }

        /// <summary>
        /// Country or State code.
        /// </summary>
        public string Code { get; set; }
        public int FieldId { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
        public int? Order { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
