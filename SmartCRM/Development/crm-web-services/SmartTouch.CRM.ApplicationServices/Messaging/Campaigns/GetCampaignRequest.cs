using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignRequest : IntegerIdRequest
    {
        public GetCampaignRequest(int id) : base(id) { }
    }

    public class GetCampaignResponse : ServiceResponseBase
    {
        public CampaignViewModel CampaignViewModel { get; set; }
        public Campaign Campaign { get; set; }
    }

    public class GetContactCampaignMapRequest: ServiceRequestBase
    {
        public GetContactCampaignMapRequest(int campaignId, int contactId) { }
    }

    public class GetContactCampaignMapResponse : ServiceResponseBase
    {
        public int ContactCampaignMapResponseId { get; set; }
    }
}
