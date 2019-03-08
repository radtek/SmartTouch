using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class CampaignTracker : ValueObjectBase
    {
        public int CampaignTrackerId { get; set; }
        public int CampaignRecipientId { get; set; }

        public int CampaignId { get; set; }
        public int ContactId { get; set; }

        public CampaignContactActivity ActivityType { get; set; }

        public int? CampaignLinkId { get; set; }
        public byte? LinkIndex { get; set; }
        public DateTime ActivityDate { get; set; }
        public int AccountId { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}