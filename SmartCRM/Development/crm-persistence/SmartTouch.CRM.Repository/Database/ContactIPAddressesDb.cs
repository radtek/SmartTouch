using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactIPAddressesDb
    {
        [Key]
        public int ContactIPAddressID { get; set; }
        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }
        public string IPAddress { get; set; }
        public DateTime IdentifiedOn { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string ISPName { get; set; }
        public string UniqueTrackingID { get; set; }
    }
}
