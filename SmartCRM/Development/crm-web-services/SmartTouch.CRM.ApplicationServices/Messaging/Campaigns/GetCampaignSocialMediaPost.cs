using SmartTouch.CRM.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignSocialMediaPostRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public int UserId { get; set; }
        public string CommunicationType { get; set; }
    }
    public class GetCampaignSocialMediaPostResponse : ServiceResponseBase
    {
        public IEnumerable<UserSocialMediaPosts> Posts { get; set; }
    }
}
