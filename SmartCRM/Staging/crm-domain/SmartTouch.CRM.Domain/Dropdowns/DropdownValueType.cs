using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Dropdowns
{
    public class DropdownValueType : EntityBase<byte>, IAggregateRoot
    {

        public Int16 DropdownValueTypeName {get;set;}
        public string DefaultDescription { get; set; }
        protected override void Validate()
        {
        }
    }
}
