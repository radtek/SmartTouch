using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTouch.CRM.Repository.Database
{
    public class ActionAuditDb
    {
        [Key]
        public Int64 AuditId { get; set; }

        public int ActionID { get; set; }

       
        public int? AccountID { get; set; }
       

        public string ActionDetails { get; set; }
      
        public DateTime? RemindOn { get; set; }       
        public bool? RemindbyText { get; set; }
        public bool? RemindbyEmail { get; set; }
        public bool? RemindbyPopup { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
     

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

     
        public int CreatedBy { get; set; }
       
        public DateTime CreatedOn { get; set; }
        public NotificationStatus? NotificationStatus { get; set; }

        public string AuditAction { get; set; }
        public DateTime AuditDate { get; set; }
        public string AuditUser { get; set; }
        public bool AuditStatus { get; set; }
        public string AuditApp { get; set; }
    }
}
