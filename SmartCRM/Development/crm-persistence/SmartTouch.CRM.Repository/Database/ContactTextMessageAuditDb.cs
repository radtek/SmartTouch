using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ContactTextMessageAuditDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ContactTextMessageAuditID { get; set; }
         [ForeignKey("ContactPhones")]
        public int ContactPhoneNumberID { get; set; }
         [ForeignKey("Users")]
        public int SentBy { get; set; }
        public DateTime SentOn { get; set; }
        public byte Status { get; set; }
        public UsersDb Users { get; set; }
        public ContactPhoneNumbersDb ContactPhones { get; set; }
        public Guid RequestGuid { get; set; }
    }
}
