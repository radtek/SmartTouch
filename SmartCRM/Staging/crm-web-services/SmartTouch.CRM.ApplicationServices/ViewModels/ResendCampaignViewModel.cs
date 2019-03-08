using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ResendCampaignViewModel
    {
        public int CampaignId { get; set; }
        public int ParentCampaignId { get; set; }
        public string From { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }      
        public CampaignResentTo CampaignResentTo { get; set; }
    }
}
