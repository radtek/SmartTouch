using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Domain.ValueObjects;

namespace SmartTouch.CRM.Repository.Database
{
    public class ActionsDb 
    {
        [Key]
        public int ActionID { get; set; }

        [ForeignKey("Accounts")]
        public virtual int? AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        public string ActionDetails { get; set; }
        //public ReminderType ReminderType { get; set; }
        public DateTime? RemindOn { get; set; }
        public ICollection<ContactsDb> Contacts { get; set; }
        public bool? RemindbyText { get; set; }
        public bool? RemindbyEmail { get; set; }
        public bool? RemindbyPopup { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
        public ICollection<TagsDb> Tags { get; set; }

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }

        [ForeignKey("User")]
        public virtual int CreatedBy { get; set; }
        public virtual UsersDb User { get; set; }

      
        public short? ActionType { get; set; }
        public DateTime? ActionDate { get; set; }
        public DateTime? ActionStartTime { get; set; }
        public DateTime? ActionEndTime { get; set; }

        [NotMapped]
        public string UserName { get; set; }

        public DateTime CreatedOn { get; set; }
        public NotificationStatus? NotificationStatus { get; set; }

        public IList<int> OwnerIds { get; set; }

        public virtual ICollection<ContactActionMapDb> ActionContacts { get; set; }
        public virtual IList<ActionTagsMapDb> ActionTags { get; set; }
        public virtual ICollection<UserActionMapDb> ActionUsers { get; set; }
        public int? MailBulkId { get; set; }
        [NotMapped]
        public string ActionTemplateHtml { get; set; }

        public bool? SelectAll { get; set; }
    }
}
