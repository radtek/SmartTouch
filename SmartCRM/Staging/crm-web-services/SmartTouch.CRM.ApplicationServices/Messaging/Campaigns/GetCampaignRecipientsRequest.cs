using System.Collections.Generic;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    public class GetCampaignRecipientsRequest : ServiceRequestBase
    {
        public int CampaignId { get; set; }
        public bool IsLinkedToWorkflow { get; set; }
    }

    public class GetCampaignRecipientsResponse : ServiceResponseBase
    {
        public IEnumerable<CampaignRecipient> Recipients { get; set; }
        public IDictionary<int, IDictionary<string, string>> RecipientsInfo { get; set; }
    }

    public class GetCampaignUniqueRecipientsCountRequest : ServiceRequestBase
    {
        public IEnumerable<TagViewModel> Tags { get; set; }
        public IEnumerable<int> ContactIdsFromSearch { get; set; }
        public long ToTagStatus { get; set; }
    }

    public class GetCampaignUniqueRecipientsCountResponse : ServiceResponseBase
    {
        public long CampaignRecipientsCount { get; set; }
        public long CountBySearchDefinition { get; set; }
        public long CountByTag { get; set; }
        public CampaignRecipientTypes Recipients { get; set; }
        public IDictionary<int, int> TagCounts { get; set; }
        public IDictionary<int, int> SDefinitionCounts { get; set; }
    }

    public class GetCampaignRecipientRequest : ServiceRequestBase
    {
        public int CampaignRecipientId { get; set; }
    }
    public class GetCampaignRecipientResponse : ServiceResponseBase
    {
        public CampaignRecipient CampaignRecipient { get; set; }
    }

    public class UpdateCampaignRecipientsStatusRequest : ServiceRequestBase
    {
        public List<int> CampaignRecipientIDs { get; set; }
        public string Remarks { get; set; }
        public DateTime DeliveredOn { get; set; }
        public DateTime SentOn { get; set; }
        public CampaignDeliveryStatus DeliveryStatus { get; set; }
    }

    public class GetUniqueRecipientsCountRequest : ServiceRequestBase
    {
        public IEnumerable<int> Tags { get; set; }
        public IEnumerable<int> SDefinitions { get; set; }

        public int SelectedSearhDefinitionID { get; set; }
        public int SelectedTagID { get; set; }
    }
    public class GetUniqueRecipientsCountResponse : ServiceResponseBase
    {
        public IDictionary<int, int> TagCounts { get; set; }
        public IDictionary<int, int> SDefinitionCounts { get; set; }
        
        public int TagAllCount { get; set;}
        public int TagActiveCount { get; set; }
        public int SDefinitionAllCount { get; set; }
        public int SDefinitionActiveCount { get; set; }

        public int TagsAllSDsActiveCount { get; set; }
        public int TagsActiveSdsAllCount { get; set; }
        public int TotalAllUniqueCount { get; set; }
        public int TotalActiveUniqueCount { get; set; }
    }
}
