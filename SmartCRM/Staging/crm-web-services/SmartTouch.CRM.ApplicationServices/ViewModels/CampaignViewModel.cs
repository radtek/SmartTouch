using II = SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Users;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ICampaignViewModel
    {
    }

    public class CampaignViewModel : ICampaignViewModel
    {
        public int CampaignID { get; set; }
        public string Name { get; set; }
        public byte? CampaignTypeId { get; set; }
        public string Subject { get; set; }
        public string HTMLContent { get; set; }
        public string PlainTextContent { get; set; }
        public bool IncludePlainText { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public DateTime? ScheduleTimeUTC { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; }
        public IList<ContactEntry> Contacts { get; set; }
        public CampaignStatus CampaignStatus { get; set; }
        public Int16? ToTagStatus { get; set; }
        public Int16? SSContactsStatus { get; set; }
        public CampaignTemplateViewModel CampaignTemplate { get; set; }
        public int? ServiceProviderID { get; set; }
        public string ServiceProviderCampaignID { get; set; }
        public IList<TagViewModel> ContactTags { get; set; }
        public IList<II.Image> Images { get; set; }
        public Email TestEmail { get; set; }
        public int AccountID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RecipientCount { get; set; }

        public int SentCount { get; set; }
        public string DeliveryRate { get; set; }
        public string OpenRate { get; set; }
        public string ClickRate { get; set; }
        public string CompliantRate { get; set; }
        public int DeliveredCount { get; set; }
        public int OpenCount { get; set; }
        public int ClickCount { get; set; }
        public int ComplaintCount { get; set; }
        public int OptOutCount { get; set; }
        public int TotalCampaignCount { get; set; }
        public int UniqueClicks { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public IEnumerable<CampaignLinkViewModel> Links { get; set; }

        public IList<AdvancedSearchViewModel> SearchDefinitions { get; set; }
        public string DateFormat { get; set; }

        public int TotalUniqueContacts { get; set; }
        public int TotalActiveUniqueContacts { get; set; }
        public int TotalAllAndActiveUniqueContacts { get; set; }
        public int TotalActiveAndAllUniqueContacts { get; set; }

        public int To_All { get; set; }
        public int To_Active { get; set; }
        public int SS_All { get; set; }
        public int SS_Active { get; set; }

        public IEnumerable<CampaignTemplateViewModel> CampaignTemplates { get; set; }
        public string Remarks { get; set; }
        public string LastViewedState { get; set; }
        public int? LastUpdatedBy { get; set; }
        public bool CampaignUnsubscribeStatus { get; set; }
        public string UnsubscribeLink { get; set; }
        public string AccountPrivacyPolicy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public bool IsLinkedToWorkflows { get; set; }
        public IEnumerable<UserSocialMediaPostsViewModel> Posts { get; set; }

        public IEnumerable<FieldViewModel> ContactFields { get; set; }
        public IEnumerable<FieldViewModel> CustomFields { get; set; }

        public bool EnablePostToFacebook { get; set; }
        public bool EnablePostToTwitter { get; set; }
        public string FacebookPost { get; set; }
        public string TwitterPost { get; set; }
        public string FacebookAttachmentPath { get; set; }
        public bool EnableFacebook { get; set; }
        public bool EnableTwitter { get; set; }
        public DateTime? ProcessedDate { get; set; }        

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }

        public int ParentCampaignId { get; set; }
        public bool? IsRecipientsProcessed { get; set; }
        public bool? HasDisclaimer { get; set; }
        public string LitmusGuid { get; set; }
        public bool IsLitmusTestPerformed { get; set; }
        public string MailTesterGuid { get; set; }
        public bool PerformLitmusTest { get; set; }
        public bool PerformMailTester { get; set; }

        public IEnumerable<UserSocialMediaPostsViewModel> GetPosts()
        {
            var posts = new List<UserSocialMediaPostsViewModel>();
            var fbPost = new UserSocialMediaPostsViewModel()
            {
                Post = this.FacebookPost,
                AttachmentPath = this.FacebookAttachmentPath,
                CommunicationType = "Facebook"
            };
            var twitterPost = new UserSocialMediaPostsViewModel()
            {
                Post = this.TwitterPost,
                CommunicationType = "Twitter"
            };
            if (this.EnablePostToFacebook)
                posts.Add(fbPost);
            if (this.EnablePostToTwitter)
                posts.Add(twitterPost);
            return posts;
        }

        public void SetPosts()
        {
            this.Posts.ToList().ForEach(post =>
                {
                    if (post.CommunicationType == "Facebook")
                    {
                        this.EnablePostToFacebook = true;
                        this.FacebookAttachmentPath = post.AttachmentPath;
                        this.FacebookPost = post.Post;
                    }
                    if (post.CommunicationType == "Twitter")
                    {
                        this.EnablePostToTwitter = true;
                        this.TwitterPost = post.Post;
                    }
                });
        }
    }
    public class UserSocialMediaPostsViewModel
    {
        public int UserSocialMediaPostID { get; set; }
        public int CampaignID { get; set; }
        public int UserID { get; set; }
        public string Post { get; set; }
        public string AttachmentPath { get; set; }
        public string CommunicationType { get; set; }
    }
}
