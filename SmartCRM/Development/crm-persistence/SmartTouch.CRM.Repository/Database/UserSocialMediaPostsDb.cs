using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class UserSocialMediaPostsDb
    {
        [Key]
        public int UserSocialMediaPostID { get; set; }
        [ForeignKey("Campaign")]
        public int CampaignID { get; set; }
        public CampaignsDb Campaign { get; set; }
        [ForeignKey("User")]
        public int UserID { get; set; }
        public UsersDb User { get; set; }

        public string Post { get; set; }
        public string AttachmentPath { get; set; }
        public string CommunicationType { get; set; }
    }
}
