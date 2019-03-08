using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignMailTestDb
    {
        [Key]
        public int CampaignMailTestID { get; set; }
        public int CampaignID { get; set; }
        public Guid UniqueID { get; set; }
        public int Status { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string RawData { get; set; }
        public int CreatedBy { get; set; }
    }
}
