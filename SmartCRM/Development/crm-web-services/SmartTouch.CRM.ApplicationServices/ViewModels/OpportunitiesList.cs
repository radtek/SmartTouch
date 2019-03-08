using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class OpportunitiesList
    {
        public int OpportunityID { get; set; }
        public string OpportunityName { get; set; }
        public IEnumerable<ContactEntry> Contacts { get; set; }
    }
}
