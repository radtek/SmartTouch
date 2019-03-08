using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using System.Text.RegularExpressions;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class DropdownValues : EntityBase<int>, IAggregateRoot
    {
        public Int16 DropdownValueID { get; set; }
        public byte DropdownID { get; set; }

        public int AccountID { get; set; }

        string dropdownValue;
        public string DropdownValue { get { return dropdownValue; } set { dropdownValue = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }
        public bool IsDefault { get; set; }
        string dropdownName;
        public string DropdownName { get { return dropdownName; } set { dropdownName = !string.IsNullOrEmpty(value) ? value.Trim() : null; } }

        public IList<Contacts.Contact> Contacts { get; set; }

        protected override void Validate()
        {
            throw new System.NotImplementedException();
        }
    }
}
