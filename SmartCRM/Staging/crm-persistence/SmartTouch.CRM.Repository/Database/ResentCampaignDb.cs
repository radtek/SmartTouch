using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartTouch.CRM.Entities;
using System;


namespace SmartTouch.CRM.Repository.Database
{
    public class ResentCampaignDb
    {
        [Key]
        public int ResentCampaignID { get; set; }

        [ForeignKey("Campaign")]
        public virtual int ParentCampaignID { get; set; }
        public virtual CampaignsDb Campaign { get; set; }

        [ForeignKey("Campaign1")]
        public virtual int CampaignID { get; set; }
        public virtual CampaignsDb Campaign1 { get; set; }

        [ForeignKey("Status")]
        public short? CampaignResentTo { get; set; }
        public StatusesDb Status { get; set; }

        public DateTime? CamapignResentDate { get; set; }
    }
}
