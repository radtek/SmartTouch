using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Search;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignStatistics : ValueObjectBase
    {
        public int CampaignId { get; set; }
        public string CampaignName { get; set; }
        public int RecipientCount { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Clicked { get; set; }
        public int Opened { get; set; }
        public IEnumerable<CampaignLinkContent> Opens { get; set; }
        public IEnumerable<CampaignLinkContent> Clicks { get; set; }
        public IEnumerable<CampaignLinkContent> UniqueClicks { get; set; }
        public int UnSubscribed { get; set; }
        public int Complained { get; set; }
        public int Bounced { get; set; }
        public int Blocked { get; set; }
        public DateTime? SentOn { get; set; }
        public CampaignRecipient CampaignRecipient { get; set; }
        public string MailProvider { get; set; }
        public Guid ServiceProviderGuid { get; set; }
        public int NotViewed { get; set; }

        public IEnumerable<Tag> ContactTags { get; set; }
        public IEnumerable<SearchDefinition> SavedSearches { get; set; }

        public Campaign Campaign { get; set; }
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }

    public class CampaignLinkContent
    {
        public int RecipientId { get; set; }
        public byte? LinkIndex { get; set; }       
        public Url Link { get; set; }
        public int UniqueClickCount { get; set; }
        public int? CampaignLinkID { get; set; }
    }
}
