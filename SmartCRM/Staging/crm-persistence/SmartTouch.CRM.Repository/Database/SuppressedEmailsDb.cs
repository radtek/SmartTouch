using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SuppressedEmailsDb
    {
        [Key]
        public int SuppressedEmailID { get; set; }
        public string Email { get; set; }

        [ForeignKey("Accounts")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }
    }
}
