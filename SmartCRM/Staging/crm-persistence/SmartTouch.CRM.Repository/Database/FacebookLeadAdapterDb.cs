using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class FacebookLeadAdapterDb
    {
        [Key]
        public int FacebookLeadAdapterID { get; set; }
        public string PageAccessToken { get; set; }
        public long AddID { get; set; }
        public int LeadAdapterAndAccountMapID { get; set; }
        public string Name { get; set; }
        public long PageID { get; set; }
        public DateTime? TokenUpdatedOn { get; set; }
    }
}
