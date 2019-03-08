using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Workflows
{
    public class TrackMessage : EntityBase<long>, IAggregateRoot
    {
        public long TrackMessageID { get; set; }
        public Guid MessageID { get; set; }
        public byte LeadScoreConditionType { get; set; }
        public int EntityId { get; set; }
        public int LinkedEntityId { get; set; }
        public int ContactId { get; set; }
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public string ConditionValue { get; set; }
        public DateTime CreatedOn { get; set; }
        public TrackMessageProcessStatus MessageProcessStatusId { get; set; }
        protected override void Validate()
        {
            //for future use
        }    
    }
}
