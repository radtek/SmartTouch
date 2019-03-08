using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class TagSummaryRequestViewModel
    {
        public int AccountId { get; set; }
        public TagViewModel Tag { get; set; }
        public TagViewModel[] AllTags { get; set; }
    }
}
