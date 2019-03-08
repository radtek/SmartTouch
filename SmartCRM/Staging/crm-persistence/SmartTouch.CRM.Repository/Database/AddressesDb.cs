using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class AddressesDb
    {
        [Key]
        public int AddressID { get; set; }
        
        //[ForeignKey("AddressType")]
        public short AddressTypeID { get;set; }
        // public virtual AddressType AddressType { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string ZipCode { get; set; }
        public bool? IsDefault { get; set; }

        [ForeignKey("State")]
        public virtual string StateID { get; set; }
        public virtual StatesDb State { get; set; }

        [ForeignKey("Country")]
        public virtual string CountryID { get; set; }
        public virtual CountriesDb Country { get; set; }
    }
}