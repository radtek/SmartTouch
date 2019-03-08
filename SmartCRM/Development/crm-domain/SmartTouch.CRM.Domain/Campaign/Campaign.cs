using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Campaign
{
    public class Campaign : EntityBase<int>, IAggregateRoot
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public Email From { get; set; }
        public CampaignStatus CampaignStatus { get; set; }
        public CampaignTemplate CampaignTemplateID { get; set; }
        public byte EmailProviderID { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
