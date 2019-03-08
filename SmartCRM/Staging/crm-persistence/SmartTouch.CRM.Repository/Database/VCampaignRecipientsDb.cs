using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Repository.Database
{
    public class VCampaignRecipientsDb
    {
       [Key]
        public int CampaignRecipientID { get; set; }
        public int CampaignID { get; set; }
        public int ContactID { get; set; }
        public DateTime CreatedDate { get; set; }
        public string To { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public DateTime? SentOn { get; set; }
        public string GUID { get; set; }
        public int? ServiceProviderID { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public short? OptOutStatus { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public short? DeliveryStatus { get; set; }
        public string Remarks { get; set; }
        public bool HasUnsubscribed { get; set; }
        public DateTime? UnsubscribedOn { get; set; }
        public bool HasComplained { get; set; }
        public DateTime? ComplainedOn { get; set; }
        public short? WorkflowID { get; set; }
    }
}
