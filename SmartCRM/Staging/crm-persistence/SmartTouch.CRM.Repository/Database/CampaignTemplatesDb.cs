using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignTemplatesDb
    {
        [Key]
        public int CampaignTemplateID { get; set; }
        public string HTMLContent { get; set; }
        public string Name { get; set; }
        public CampaignTemplateType Type { get; set; }
        public string Description { get; set; }
        [ForeignKey("Statuses")]
        public short Status { get; set; }
        public StatusesDb Statuses { get; set; }
        [ForeignKey("Image")]
        public virtual int? ThumbnailImage { get; set; }
        public virtual ImagesDb Image { get; set; }
        public int? AccountId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
    }
}
