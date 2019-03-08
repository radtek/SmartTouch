using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CampaignReEngagementInfo
    {
        public int CampaignID { get; set; }
        public string Name { get; set; }
        public int CurrentMonthClicks { get; set; }
        public int LastMonthClicks { get; set; }
        public int Last12MonthsClicks { get; set; }
        public int TotalClicks { get; set; }
        public int CurrentMonthContacts { get; set; }
        public int LastMonthContacts { get; set; }
        public int TotalContacts { get; set; }
        public int Last12MonthsContacts { get; set; }
    }
}
