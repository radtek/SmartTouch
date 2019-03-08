using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class InsertBulkRecipientsRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public IEnumerable<TemporaryRecipient> Recipients { get; set; }
    }

    public class InsertBulkRecipientsResponse : ServiceResponseBase
    {
        public bool Result { get; set; }
    }


}
