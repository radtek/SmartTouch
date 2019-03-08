using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignResponseAuditDb
    {
        [Key]
        public int AuditID { get; set; }

        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }

        [ForeignKey("Campaign")]
        public int CampaignID { get; set; }
        public CampaignsDb Campaign { get; set; }

        public string AuditType { get; set; }

        [ForeignKey("Status")]
        public short Status { get; set; }
        public StatusesDb StatusDb { get; set; }

        public string Source { get; set; }
        public string Reason { get; set; }
        public DateTime FiredDate { get; set; }
        public string IPAddress { get; set; }
    }
}
