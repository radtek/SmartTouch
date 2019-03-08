using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
   public class CampaignContactTagMapDb
    {
       [Key]
       public int CampaignContactTagMapID { get; set; }
       
       [ForeignKey("Campaign")]
       public int CampaignID { get; set; }
       
       public CampaignsDb Campaign { get; set; }      
              
       [ForeignKey("Tag")]
       public int TagID { get; set; }
       
       public TagsDb Tag { get; set; }
    }
}
