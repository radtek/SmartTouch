using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;

namespace SmartTouch.CRM.Domain.Campaigns
{
    public class CampaignTemplate : EntityBase<int>, IAggregateRoot
    {
     
        public string Name { get; set; }
        public CampaignTemplateType Type { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public Image ThumbnailImage { get; set; }
        public string HTMLContent { get; set; }
        public int? AccountId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ThumbnailImageId { get; set; } 
        protected override void Validate()
        {
            throw new NotImplementedException();
        }
       
    }

    public class HTMLTemplate
    {
        public string HTMLContent { get;set; }
        public Images.Image ThumbnailImage { get;set; }
    }
}
