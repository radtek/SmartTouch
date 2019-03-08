using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class MomentaryCampaignRecipientsDb
    {
        [Key]
        public Int64 MomentaryRecipientID { get; set; }
        public int CampaignID { get; set; }
        public int ContactID { get; set; }
    }
}
