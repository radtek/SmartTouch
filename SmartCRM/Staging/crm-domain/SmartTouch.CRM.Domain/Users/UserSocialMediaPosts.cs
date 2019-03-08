using SmartTouch.CRM.Domain.Campaigns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Users
{
    public class UserSocialMediaPosts
    {
        public int UserSocialMediaPostID { get; set; }
        public int CampaignID { get; set; }
        public int UserID { get; set; }
        public string Post { get; set; }
        public string AttachmentPath { get; set; }
        public string CommunicationType { get; set; }
    }
}
