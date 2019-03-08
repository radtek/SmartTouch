using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Infrastructure.Domain;
using System;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignRecipient : EntityBase<int>, IAggregateRoot
    {
        public int CampaignRecipientID { get; set; }
        public int CampaignID { get; set; }
        public int AccountID { get; set; }
        public Campaign Campaign { get; set; }
        public int ContactID { get; set; }
        public Contact Contact { get; set; }
        public DateTime CreatedDate { get; set; }
        public string To { get; set; }
        public string GUID { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public DateTime? SentOn { get; set; }

        public short? DeliveryStatus { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public string Remarks { get; set; }
        public Workflow Workflow { get; set; }
        public int WorkflowId { get; set; }
        protected override void Validate()
        {
        }
    }
}
