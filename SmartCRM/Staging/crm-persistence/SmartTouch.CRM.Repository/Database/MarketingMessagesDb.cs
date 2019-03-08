using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class MarketingMessagesDb
    {
        [Key]
        public int MarketingMessageID { get; set; }
        public string MarketingMessageTitle { get; set; }
        public byte TimeInterval { get; set; }
        public short  Status { get; set; }
        public byte SelectedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public DateTime? ScheduleFrom { get; set; }
        public DateTime? ScheduleTo { get; set; }
        [NotMapped]
        public int TotalCount { get; set; }
        [NotMapped]
        public int? MessageCount { get; set; }
        [NotMapped]
        public IList<int> AccountIDs { get; set; }
        [NotMapped]
        public ICollection<MarketingMessageContentMapDb> Messages { get; set; }
        [NotMapped]
        public ICollection<MarketingMessageAccountMapDb> MarketingMessageAccountMaps { get; set; }
        
    }
}
