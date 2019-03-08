using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class CampaignSearchDefinitionsDb
    {
        [Key]
        public int CampaignSearchDefinitionMapID { get; set; }

        [ForeignKey("Campaign")]
        public int CampaignID { get; set; }
        public CampaignsDb Campaign { get; set; }

        [ForeignKey("SearchDefinition")]
        public int SearchDefinitionID { get; set; }
        public SearchDefinitionsDb SearchDefinition { get; set; }
    }
}
