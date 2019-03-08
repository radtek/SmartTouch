using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.MarketingMessageCenter
{
   public class MarketingMessageAccountMap
    {
        public int MarketingMessageID { get; set; }
        public int AccountID { get; set; }
        public string AccountName { get; set; }
    }
}
