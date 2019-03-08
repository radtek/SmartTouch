using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class MarketingMessageAccountMapDb
    {
        [Key]
        public int MarketingMessageAccountMapID { get; set; }
        [ForeignKey("MarketingMessage")]
        public virtual int MarketingMessageID { get; set; }
        public virtual MarketingMessagesDb MarketingMessage { get; set; }
        [ForeignKey("Account")]
        public virtual int AccountID { get; set; }
        public virtual AccountsDb Account { get; set; }
        [NotMapped]
        public string AccountName { get; set; }
    }
}
