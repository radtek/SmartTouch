using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Dropdowns
{
    public class Dropdown : EntityBase<byte>, IAggregateRoot
    {
        public string Name { get; set; }
        public IEnumerable<DropdownValue> DropdownValues { get; set; }
        public int AccountID { get; set; }
        public int TotalDropdownCount { get; set; }
        protected override void Validate()
        {
        }
    }
}
