using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class DeleteCampaignImageRequest : IntegerIdRequest
    {
        public DeleteCampaignImageRequest(int id) : base(id) { }
    }
    public class DeleteCampaignImageResponse : ServiceResponseBase
    {
    }
}
