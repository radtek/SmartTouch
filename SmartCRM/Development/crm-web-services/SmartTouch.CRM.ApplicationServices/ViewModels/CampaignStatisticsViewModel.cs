using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignStatisticsViewModel
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public int Bounced { get; set; }
        public int Blocked { get; set; }
        public dynamic Opens { get; set; }
        public dynamic Clicks { get; set; }
        public dynamic UniqueClicks { get; set; }
        public int UnSubscribed { get; set; }
        public int Complained { get; set; }
        public DateTime? SentOn { get; set; }
        public string MailProvider { get; set; }
        public int NotViewed { get; set; }
        public bool IsLinkedToWorkflows { get; set; }

        public IEnumerable<TagViewModel> ContactTags { get; set; }
        public IEnumerable<AdvancedSearchViewModel> SearchDefinitions { get; set; }

        public CampaignViewModel CampaignViewModel { get; set; }
    }
}
