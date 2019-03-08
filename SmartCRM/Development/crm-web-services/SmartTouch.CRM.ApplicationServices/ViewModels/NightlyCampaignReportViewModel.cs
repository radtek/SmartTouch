using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class NightlyCampaignReportViewModel
    {
        public int Day { get; set; }
        public string AccountName { get; set; }
        public int CampaignId { get; set; }
        public string CampaignSubject { get; set; }
        public string Vmta { get; set; }
        public int Recipients { get; set; }
        public int Sent { get; set; }
        public string Delivered { get; set; }
        public string Bounced { get; set; }
        public string Opened { get; set; }
        public string Clicked { get; set; }
        public string Complained { get; set; }
        public int TagsAll { get; set; }
        public int TagsActive { get; set; }
        public int SavedSearchAll { get; set; }
        public int SavedSearchActive { get; set; }
    }
}
