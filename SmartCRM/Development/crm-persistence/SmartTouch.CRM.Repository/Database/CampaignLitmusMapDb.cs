using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignLitmusMapDb
    {
        [Key]
        public int CampaignLitmusMapId { get; set; }
        public int? CampaignId { get; set; }
        public string LitmusId { get; set; }
        public int? ProcessingStatus { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string Remarks { get; set; }
    }
}
