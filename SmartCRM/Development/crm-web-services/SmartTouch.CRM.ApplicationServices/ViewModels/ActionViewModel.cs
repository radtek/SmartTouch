using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface IActionViewModel
    {
        int ActionId { get; set; }
        string ActionMessage { get; set; }
        int? AccountId { get; set; }
        IEnumerable<ReminderType> SelectedReminderTypes { get; set; }
        IEnumerable<dynamic> ReminderTypes { get; set; }
        IEnumerable<dynamic> ReminderMethods { get; set; }
        DateTime? RemindOn { get; set; }
        bool IsCompleted { get; set; }
        bool MarkAsCompleted { get; set; }
        IEnumerable<ContactEntry> Contacts { get; set; }
        IEnumerable<TagViewModel> TagsList { get; set; }
        int CreatedBy { get; set; }
        int OppurtunityId { get; set; }
        DateTime CreatedOn { get; set; }
        string DateFormat { get; set; }
        string ToEmail { get; set; }
        Guid? EmailRequestGuid { get; set; }
        Guid? TextRequestGuid { get; set; }
        IEnumerable<string> FromEmails { get; set; }
    }

    public class ActionViewModel : IActionViewModel
    {
        public virtual int ActionId { get; set; }
        public string ActionMessage { get; set; }
        //public int ReminderType { get; set; }
        public int? AccountId { get; set; }
        public IEnumerable<ReminderType> SelectedReminderTypes { get; set; }
        //public int[] ReminderTypeMulti { get; set; }
        public IEnumerable<dynamic> ReminderTypes { get; set; }
        public IEnumerable<dynamic> ReminderMethods { get; set; }
        public DateTime? RemindOn { get; set; }
        public bool IsCompleted { get; set; }
        public bool PreviousCompletedStatus { get; set; }
        public bool MarkAsCompleted { get; set; }
        public int LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public bool IsFromDashboard { get; set; }

        public IEnumerable<ContactEntry> Contacts { get; set; }
        public IEnumerable<TagViewModel> TagsList { get; set; }
        public int CreatedBy { get; set; }
        public int OppurtunityId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string DateFormat { get; set; }
        public string ToEmail { get; set; }
        public Guid? EmailRequestGuid { get; set; }
        public Guid? TextRequestGuid { get; set; }
        public IEnumerable<string> FromEmails { get; set; }
        public string ContactName { get; set; }
        public bool IcsCanlender { get; set; }
        public bool IcsCanlenderToContact { get; set; }

        public IEnumerable<DropdownValueViewModel> ActionTypes { get; set; }
        public short? ActionType { get; set; }
        public short? PreviousActionType { get; set; }
        public DateTime? ActionDate { get; set; }
        public DateTime? ActionStartTime { get; set; }
        public DateTime? ActionEndTime { get; set; }
        public string ActionTypeName { get; set; }

        public DateTime? ActionDateOn { get; set; }
        public DateTime? ActionStartTimeOn { get; set; }
        public DateTime? ActionEndTimeOn { get; set; }

        public string ActionTypeValue { get; set; }
        public DateTime ActionDateTime { get; set; }
        public IList<int> OwnerIds { get; set; }
        public string UserName { get; set; }

        public virtual CRMOutlookSyncViewModel OutlookSync { get; set; }

        public IEnumerable<TagViewModel> PopularTags { get; set; }
        public IEnumerable<TagViewModel> RecentTags { get; set; }

        public bool SelectAll { get; set; }
        public string ActionTemplateHtml { get; set; }
        public int? MailBulkId { get; set; }
        public Guid? GROUPID { get; set; }
        public bool IsHtmlSave { get; set; }
        public bool AddToNoteSummary { get; set; }
        public int ContactId { get; set; }
        public byte ContactType { get; set; }

    }
}
