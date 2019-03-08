using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ZillowAddressLookup : ValueObjectBase
    {
        public string HomeDetails { get; set; }
        public string GraphsAndData { get; set; }
        public string MapThisHome { get; set; }
        public string Comparables { get; set; }

        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
