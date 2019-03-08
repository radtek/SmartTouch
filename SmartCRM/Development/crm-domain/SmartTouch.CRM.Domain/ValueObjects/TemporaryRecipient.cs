using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class TemporaryRecipient
    {
        public Int64 TemporaryRecipientId { get; set; }
        public int ContactID { get; set; }
        public int CampaignID { get; set; }
    }
}
