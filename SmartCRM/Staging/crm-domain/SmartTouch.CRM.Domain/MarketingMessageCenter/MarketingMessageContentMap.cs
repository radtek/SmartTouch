using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.MarketingMessageCenter
{
   public class MarketingMessageContentMap
    {
        public int MarketingMessageContentMapID { get; set; }
        public int MarketingMessageID { get; set; }
        public string Subject { get; set; }
        public string Icon { get; set; }
        public string Content { get; set; }
        public byte TimeInterval { get; set; }
    }
}
