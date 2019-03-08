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
    public class TourDb
    {
        [Key]
        public int TourID { get; set; }

        [ForeignKey("Accounts")]
        public virtual int? AccountID { get; set; }
        public virtual AccountsDb Accounts { get; set; }

        public string TourDetails { get; set; }
        //public int Community { get; set; }
        public DateTime TourDate { get; set; }
        public short TourType { get; set; }
        //public ReminderType ReminderType { get; set; }
        public Nullable<DateTime> ReminderDate { get; set; }
        public bool? RemindbyText { get; set; }
        public bool? RemindbyEmail { get; set; }
        public bool? RemindbyPopup { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
        [ForeignKey("User")]
        public int CreatedBy { get; set; }
        public virtual UsersDb User { get; set; }
        
        public DateTime CreatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public bool SelectAll { get; set; }

        //[ForeignKey("Community")]
        public short CommunityID { get; set; }
        public IList<int> OwnerIds { get; set; }
        public ICollection<ContactsDb> Contacts { get; set; }

        public NotificationStatus? NotificationStatus { get; set; }

        public virtual ICollection<ContactTourMapDb> TourContacts { get; set; }

        public virtual ICollection<UserTourMapDb> TourUsers { get; set; }
    }
}
