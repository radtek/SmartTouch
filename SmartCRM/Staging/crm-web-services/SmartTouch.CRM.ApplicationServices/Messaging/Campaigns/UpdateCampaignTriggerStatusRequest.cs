using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;

namespace SmartTouch.CRM.ApplicationServices.Messaging.Campaigns
{
    /// <summary>
    /// 
    /// </summary>
    public class UpdateCampaignTriggerStatusRequest : IntegerIdRequest
    {
        public UpdateCampaignTriggerStatusRequest(int id) : base(id) { }
        public CampaignStatus Status { get; set; }
        public DateTime SentDateTime { get; set; }
        public string Remarks { get; set; }
        public int SentCount { get; set; }
        public int? ServiceProviderID { get; set; }
        public string ServiceProviderCampaignId { get; set; }
        public IList<string> Recipients { get; set; }
        public IEnumerable<int> RecipientIds { get; set; }
        public bool IsRelatedToWorkFlow { get; set; }
        public bool IsDelayedCampaign { get; set; }
    }
}
