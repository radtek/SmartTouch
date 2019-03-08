using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public  class CampaignLogDetailsDb
    {
        [Key]
        public int CampaignLogDetailsID { get; set; }
        public string CampaignId { get; set; }
        public int? CampaignRecipientId { get; set; }
        public string Recipient { get; set; }
        public short? DeliveryStatus { get; set; }
        public short? OptOutStatus { get; set; }
        public int? BounceCategory { get; set; }
        public DateTime? TimeLogged { get; set; }
        public string Remarks { get; set; }
        public DateTime CreatedOn { get; set; }
        public byte? Status { get; set; }
        public int? FileType { get; set; }
    }
}
