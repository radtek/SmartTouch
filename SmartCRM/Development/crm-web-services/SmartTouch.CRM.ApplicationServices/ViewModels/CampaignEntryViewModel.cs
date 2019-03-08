using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class CampaignEntryViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public string OpenRate { get; set; }
        public string ClickRate { get; set; }
        public string ComplaintRate { get; set; }
        public string ProviderName { get; set; }
        public int ClickCount { get; set; }
        public int CampaignDeliveryCount { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string JobId { get; set; }
        public int TotalSends { get; set; }
        public int WorkflowsCount { get; set; }
    }
}
