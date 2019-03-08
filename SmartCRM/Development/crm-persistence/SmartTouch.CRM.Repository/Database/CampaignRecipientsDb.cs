using SmartTouch.CRM.Domain.ValueObjects;
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
    public class CampaignRecipientsDb
    {
        [Key]
        public int CampaignRecipientID { get; set; }
        
        [ForeignKey("Campaign")]
        public int CampaignID { get; set; }
        public CampaignsDb Campaign { get; set; }
        
        [ForeignKey("Contact")]
        public int ContactID { get; set; }
        public ContactsDb Contact { get; set; }

        public int AccountID { get; set; }

        public DateTime CreatedDate { get; set; }
        public string To { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public DateTime? SentOn { get; set; }
        public string GUID { get; set; }
        public int? ServiceProviderID { get; set; }
        public DateTime? LastModifiedOn { get; set; }

        [ForeignKey("OptOutStatuses")]
        public short? OptOutStatus { get; set; }
        public StatusesDb OptOutStatuses { get; set; }

        public DateTime? DeliveredOn { get; set; }
        [ForeignKey("Statuses")]
        public short? DeliveryStatus { get; set; }
        public StatusesDb Statuses { get; set; }
        public string Remarks { get; set; }
        public bool HasUnsubscribed { get; set; }
        public DateTime? UnsubscribedOn { get; set; }

        public bool HasComplained { get; set; }
        
        public DateTime? ComplainedOn { get; set; }
        [ForeignKey("Workflow")]
        public short? WorkflowID { get; set; }
        public WorkflowsDb Workflow { get; set; }
    }
}
