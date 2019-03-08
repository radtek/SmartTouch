using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.MarketingMessageCenter
{
   public class MarketingMessage : EntityBase<int>, IAggregateRoot
    {
        public int MarketingMessageID { get; set; }
        public string MarketingMessageTitle { get; set; }
        public byte TimeInterval { get; set; }
        public short Status { get; set; }
        public byte SelectedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public int? MessageCount { get; set; }
        public IList<int> AccountIDs { get; set; }
        public DateTime? ScheduleFrom { get; set; }
        public DateTime? ScheduleTo { get; set; }
        public int TotalCount { get; set; }
        public IList<MarketingMessageContentMap> Messages { get; set; }
        public virtual IEnumerable<MarketingMessageAccountMap> MarketingMessageAccountMaps { get; set; }
        public virtual IEnumerable<MarketingMessageContentMap> MarketingMessageContentMaps { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
