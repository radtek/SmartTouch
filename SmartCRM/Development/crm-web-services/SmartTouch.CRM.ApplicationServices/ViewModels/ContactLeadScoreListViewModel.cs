using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ContactLeadScoreListViewModel : IShallowContact
    {
        public int Id { get; set; }
        public int LeadScore { get; set; }
        public string FullName { get; set; }
        public short LifecycleStage { get; set; }
        public string LifecycleName { get; set; }
        public string PrimaryEmail { get; set; }
    }
}
