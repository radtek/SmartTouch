using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Domain.Tags;
using System;
using System.Collections.Generic;
using II = SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class Campaign : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public byte? CampaignTypeId { get; set; }
        public string Subject { get; set; }
        public string HTMLContent { get; set; }
        public string PlainTextContent { get; set; }
        public bool IncludePlainText { get; set; }
        public CampaignTemplate Template { get; set; }
        public IList<Tag> Tags { get; set; }
        public IList<SearchDefinition> SearchDefinitions { get; set; }
        public IList<CampaignRecipient> CampaignRecipients { get; set; }
        public IList<II.Image> Images { get; set; }
        public IList<Contact> Contacts { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; }
        public Email TestEmail { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public CampaignStatus CampaignStatus { get; set; }
        public int? ServiceProviderID { get; set; }
        public int AccountID { get; set; }
        public int SentCount { get; set; }
        public int DeliveredCount { get; set; }
        public int RecipientCount { get; set; }
        public string DeliveryRate { get; set; }
        public string OpenRate { get; set; }
        public string ClickRate { get; set; }
        public string CompliantRate { get; set; }
        public int UniqueClicks { get; set; }
        public IList<Tag> ContactTags { get; set; }
        public IEnumerable<CampaignLink> Links { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Remarks { get; set; }
        public string LastViewedState { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public bool IsLinkedToWorkflows { get; set; }
        public string ServiceProviderCampaignID { get; set; }
        public IEnumerable<UserSocialMediaPosts> Posts { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public Int16? TagRecipients { get; set; }
        public Int16? SSRecipients { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
        public int ComplaintCount { get; set; }
        public int OptOutCount { get; set; }
        public int TotalCampaignCount { get; set; }
        public bool? IsRecipientsProcessed { get; set; }
        public bool? HasDisclaimer { get; set; }
        protected override void Validate()
        {
            if (string.IsNullOrEmpty(Name)) AddBrokenRule(CampaignBusinessRules.CampaignNameEmpty);
            if (Id > 0 && IsPropValid(Subject)) AddBrokenRule(CampaignBusinessRules.SubjectEmpty);
            if (Id > 0 && IsPropValid(From)) AddBrokenRule(CampaignBusinessRules.FromEmailEmpty);
            if (Id > 0 && IsPropValid(HTMLContent)) AddBrokenRule(CampaignBusinessRules.HTMLContentEmpty);
        }
        public bool IsPropValid(string value)
        {
            if (CampaignStatus != CampaignStatus.Draft && string.IsNullOrEmpty(value)) return true;
            return false;
        }
        public bool IsScheduleTimeValid()
        {
            if (CampaignStatus != CampaignStatus.Draft && ScheduleTime == null) return false;
            else if (CampaignStatus == CampaignStatus.Draft) return true;
            return ScheduleTime >= DateTime.Now.AddMinutes(-5).ToUniversalTime() ? true : false;
        }

        public override string ToString()
        {
            return this.Id.ToString();
        }
    }
}
