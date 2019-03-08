using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class MarketingMessageContentMapViewModel
    {
        public int MarketingMessageContentMapID { get; set; }
        public int MarketingMessageID { get; set; }
        public string Subject { get; set; }
        public string Icon { get; set; }
        public string Content { get; set; }
        public bool IsDeleted { get; set; }
        public byte TimeInterval { get; set; }

    }
}
