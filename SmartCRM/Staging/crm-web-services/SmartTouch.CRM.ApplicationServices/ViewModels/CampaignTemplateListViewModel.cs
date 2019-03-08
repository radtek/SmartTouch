using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignTemplateListEntry
    {
        public byte TemplateId { get; set; }
        public string Name { get; set; }
        public CampaignTemplateType Type { get; set; }
        public Image ThumbnailImage { get; set; }
    }

    public interface iCampaignTemplateListViewModel 
    {
        IEnumerable<CampaignTemplateListEntry> CampaignTemplates { get; set; }
    }

    public class CampaignTemplateListViewModel : iCampaignTemplateListViewModel
    {
        public IEnumerable<CampaignTemplateListEntry> CampaignTemplates { get; set; }
    }
}
