using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactAddressMapDb
    {
        [Key]
        public int ContactAddressMapID { get;set; }
        
        [ForeignKey("Contact")]
        public virtual int ContactID { get;set;}
        public virtual ContactsDb Contact { get;set;}

        [ForeignKey("Address")]
        public virtual int AddressID { get; set; }
        public virtual AddressesDb Address { get; set; }
    }
}
