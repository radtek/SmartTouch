using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class ReceivedMailAuditDb
    {
        [Key]
        public int ReceivedMailAuditID { get; set; }

        [ForeignKey("Users")]
        public int UserID { get; set; }
        public UsersDb Users { get; set; }

        [ForeignKey("Contacts")]
        public int SentByContactID { get; set; }
        public ContactsDb Contacts { get; set; }

        public DateTime ReceivedOn { get; set; }
        public Guid ReferenceID { get; set; }
    }
}
