using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignTypeDb
    {
        [Key]
        public byte CampaignTypeID { get; set; }
        public string CampaignType { get; set; }
    }
}
