using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignTemplateViewModel
    {
        public int TemplateId { get; set; }
        public string Name { get; set; }
        public CampaignTemplateType Type { get; set; }
        public string Description { get; set; }
        public string HTMLContent { get; set; }
        public string ThumbnailImageUrl { get; set; }
        public string OriginalName { get; set; }
        public string ImageContent { get; set; }
        public string ImageType { get; set; }
       // public ImageViewModel Image { get; set; }
    }
}
