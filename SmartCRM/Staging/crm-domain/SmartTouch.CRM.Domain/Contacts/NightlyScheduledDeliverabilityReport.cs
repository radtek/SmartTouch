using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Contacts
{
    public class NightlyScheduledDeliverabilityReport
    {
        public IEnumerable<SenderRecipientInfoNightlyReport> DayReport { get; set; }
        public IEnumerable<SenderRecipientInfoNightlyReport> SevenDaysReport { get; set; }
    }
}
