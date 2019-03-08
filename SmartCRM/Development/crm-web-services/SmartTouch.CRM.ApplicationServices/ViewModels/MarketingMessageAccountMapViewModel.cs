using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class MarketingMessageAccountMapViewModel
    {
        public int MarketingMessageAccountMapID { get; set; }
        public int MarketingMessageID { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
    }
}
