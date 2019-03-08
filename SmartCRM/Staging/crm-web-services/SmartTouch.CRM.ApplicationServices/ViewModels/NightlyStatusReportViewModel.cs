using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class NightlyStatusReportViewModel
    {
        public string AccountName { get; set; }
        public int SenderReputationCount { get; set; }
        public int CampaignsSent { get; set; }
        public int Recipients { get; set; }
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public string Bounced { get; set; }
        public string Opened { get; set; }
        public string Clicked { get; set; }
        public int TagsAll { get; set; }
        public int TagsActive { get; set; }
        public int SSAll { get; set; }
        public int SSActive { get; set; }
    }
}
