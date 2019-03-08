using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class AttachmentDataViewModel
    {
        public int PageNumber { get; set; }
        public string PageName { get; set; }       
        public int? ContactID { get; set; }
        public int? OpportunityID { get; set; }
    }
}
