using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class MarketingMessageContentMapDb
    {
        [Key]
        public int MarketingMessageContentMapID { get; set; }
        [ForeignKey("MarketingMessage")]
        public virtual int MarketingMessageID { get; set; }
        public virtual MarketingMessagesDb MarketingMessage { get; set; }
        public string Subject { get; set; }
        public string Icon { get; set; }
        public string Content { get; set; }
        [NotMapped]
        public byte TimeInterval { get; set; }

    }
}
