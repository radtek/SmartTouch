using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
   public class RefreshAnalyticViewModel
    {
        public int RefreshAnalyticsID { get; set; }
        public int EntityID { get; set; }
        public byte EntityType { get; set; }
        public byte Status { get; set; }
        public DateTime? LastModifiedOn { get; set; }
    }
}
