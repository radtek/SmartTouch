using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactEmailAuditDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContactEmailAuditID { get; set; }
         [ForeignKey("ContactEmails")]
        public int ContactEmailID { get; set; }
         [ForeignKey("Users")]
        public int SentBy { get; set; }
        public DateTime SentOn { get; set; }
        public byte Status { get; set; }
        public UsersDb Users { get; set; }
        public ContactEmailsDb ContactEmails { get; set; }
        public Guid RequestGuid { get; set; }
    }
}
