using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class vCampaignStatisticsDb
    {
        [Key]
        public int CampaignTrackerID { get; set; }       
        public int CampaignID { get; set; }      

        public CampaignContactActivity ActivityType { get; set; }      
        public int? CampaignLinkID { get; set; }      

        public byte? LinkIndex { get; set; }
        public DateTime ActivityDate { get; set; }      
        public int CampaignRecipientId { get; set; }
         
       
    }
}
