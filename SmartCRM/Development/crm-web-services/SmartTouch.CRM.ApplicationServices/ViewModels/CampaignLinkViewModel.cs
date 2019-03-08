using SmartTouch.CRM.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignLinkViewModel
    {
        public int CampaignLinkId { get; set; }
        public int CampaignId { get; set; }
        public Url URL { get; set; }
        public string Name { get; set; }
        public byte LinkIndex { get; set; }
    }
}
