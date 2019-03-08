using System;
using System.Collections.Generic;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ITourViewModel
    {
        int TourID { get; set; }
        string TourDetails { get; set; }
        short CommunityID { get; set; }
        int? AccountId { get; set; }
        DateTime TourDate { get; set; }
        short TourType { get; set; }
        IEnumerable<ReminderType> SelectedReminderTypes { get; set; }
        DateTime? ReminderDate { get; set; }
        int CreatedBy { get; set; }
        DateTime CreatedOn { get; set; }
        int LastUpdatedBy { get; set; }
        DateTime LastUpdatedOn { get; set; }
        DateTime currentTime { get; set; }
        ICollection<ContactEntry> Contacts { get; set; }
        IEnumerable<dynamic> Communities { get; set; }
        IEnumerable<dynamic> ReminderTypes { get; set; }
        IEnumerable<dynamic> TourTypes { get; set; }
        string DateFormat { get; set; }
        string ToEmail { get; set; }
        DateTime UtcTourDate { get; set; }
        DateTime UtcReminderDate { get; set; }
        Guid? EmailRequestGuid { get; set; }
        Guid? TextRequestGuid { get; set; }
    }

    public class TourViewModel : ITourViewModel
    {
        public TourViewModel()
        {
            //Get the list of communities. This has to be developed once service layer for communities is created.
        }
        public virtual int TourID { get; set; }
        public string TourDetails { get; set; }
        public int? AccountId { get; set; }
        public virtual short CommunityID { get; set; }
        public virtual string Community { get; set; }
        public virtual DateTime TourDate { get; set; }
        public virtual short TourType { get; set; }
        public virtual IEnumerable<ReminderType> SelectedReminderTypes { get; set; }
        public virtual DateTime? ReminderDate { get; set; }
        public virtual int LastUpdatedBy { get; set; }
        public virtual DateTime LastUpdatedOn { get; set; }
        public int CreatedBy { get; set; }
        public bool IsCompleted { get; set; }
        public bool PreviousCompletedStatus { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime currentTime { get; set; }
        public IEnumerable<dynamic> Communities { get; set; }
        public virtual ICollection<ContactEntry> Contacts { get; set; }
        public virtual IEnumerable<dynamic> ReminderTypes { get; set; }
        public IEnumerable<dynamic> TourTypes { get; set; }
        public string DateFormat { get; set; }
        public string ToEmail { get; set; }
        public DateTime UtcTourDate { get; set; }
        public DateTime UtcReminderDate { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
        public bool IcsCanlender { get; set; }
        public bool SelectAll { get; set; }
        public IList<int> OwnerIds { get; set; }
        public string UserName { get; set; }
        public bool AddToContactSummary { get; set; }
    }

    public interface IToursListViewModel
    {
        IEnumerable<TourViewModel> Tours { get; set; }
    }

    public class ToursListViewModel : IToursListViewModel
    {
        public IEnumerable<TourViewModel> Tours { get; set; }
    }
}
