using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignsDb
    {
        [Key]
        public int CampaignID { get; set; }

        public StatusesDb Statuses { get; set; }
        [ForeignKey("Statuses")]
        public virtual short CampaignStatusID { get; set; }

        [ForeignKey("CampaignTemplate")]
        public virtual int CampaignTemplateID { get; set; }
        public CampaignTemplatesDb CampaignTemplate { get; set; }

        public string Name { get; set; }
        
        [ForeignKey("CampaignType")]
        public byte? CampaignTypeID { get; set; }        
        public virtual CampaignTypeDb CampaignType { get; set; }
        
        public string Subject { get; set; }
        public string HTMLContent { get; set; }
        [NotMapped]
        public string PlainTextContent { get; set; }
        public bool IncludePlainText { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; }

        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public UsersDb User { get; set; }
        public virtual int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        public DateTime CreatedDate { get; set; }

        [ForeignKey("ServiceProvider")]
        public int? ServiceProviderID { get; set; }
        public ServiceProvidersDb ServiceProvider { get; set; }

        public IList<ContactsDb> Contacts { get; set; }
        public ICollection<TagsDb> Tags { get; set; }
        public IEnumerable<CampaignLinksDb> Links { get; set; }
        [ForeignKey("Account")]
        public int AccountID { get; set; }
        public AccountsDb Account { get; set; }

        public bool IsDeleted { get; set; }

        public IList<CampaignRecipientsDb> CampaignRecipients { get; set; }
        public IEnumerable<SearchDefinitionsDb> SearchDefinitions { get; set; }

        public string Remarks { get; set; }
        public string LastViewedState { get; set; }
        public string ServiceProviderCampaignID { get; set; }

        public bool IsLinkedToWorkflows { get; set; }
        public ICollection<UserSocialMediaPostsDb> Posts { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public Int16? TagRecipients { get; set; }
        public Int16? SSRecipients { get; set; }
        public bool? IsRecipientsProcessed { get; set; }
        public bool? HasDisclaimer { get; set; }

    }
}
