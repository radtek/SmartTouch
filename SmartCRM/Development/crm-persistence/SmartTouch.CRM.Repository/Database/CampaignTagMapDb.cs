using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
   public class CampaignTagMapDb
    {
       [Key]
       public int CampaignTagMapID { get; set; }

       [ForeignKey("Campaign")]
       public virtual int CampaignID { get; set; }
       public virtual CampaignsDb Campaign { get; set; }

       [ForeignKey("Tag")]
       public virtual int TagID { get; set; }
       public virtual TagsDb Tag { get; set; }
    }
}
