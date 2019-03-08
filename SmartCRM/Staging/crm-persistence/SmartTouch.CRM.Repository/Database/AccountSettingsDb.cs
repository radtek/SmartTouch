
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class AccountSettingsDb
   {
        [Key]
        public int AccountSettingsID { get; set; }
        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
        [ForeignKey("Status")]
        public virtual short StatusID { get; set; }
        public virtual StatusesDb Status { get; set; }
        public string ViewName { get; set; }
       
    }
}
