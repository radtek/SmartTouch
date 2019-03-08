using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class SuppressedDomainsDb
    {
        [Key]
        public int SuppressedDomainID { get; set; }
        public string Domain { get; set; }

        [ForeignKey("Accounts")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }
    }
}
