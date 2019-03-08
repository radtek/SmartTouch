using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class WorkflowCampaignStatistics
    {
        public string CampaignName { get; set; }
        public int CampaignID { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public int Unsubscribed { get; set; }
        public int OptedOut { get; set; }
        public int Delivered { get; set; }
        public int Complained { get; set; }
    }
}
