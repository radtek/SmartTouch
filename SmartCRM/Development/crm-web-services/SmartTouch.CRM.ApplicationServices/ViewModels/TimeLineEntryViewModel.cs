using System;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public interface ITimeLineEntryViewModel
    {
        int ContactID { get; set; }
        string Module { get; set; }
        string AuditAction { get; set; }
        string Value { get; set; }
        DateTime AuditDate { get; set; }
        int ModuleId { get; set; }
        string UserName { get; set; }
        int CreatedBy { get; set; }

        string TimeLineDate { get; set; }
        string TimeLineTime { get; set; }
        bool AuditStatus { get; set; }

    }

    public class TimeLineEntryViewModel : ITimeLineEntryViewModel
    {
        public int ContactID { get; set; }
        public string Module { get; set; }
        public string AuditAction { get; set; }
        public string Value { get; set; }
        public DateTime AuditDate { get; set; }
        public int ModuleId { get; set; }
        public string UserName { get; set; }
        public int CreatedBy { get; set; }
        public long? TimeLineID { get; set; }
        public string TimeLineDate { get; set; }
        public string TimeLineTime { get; set; }
        public bool AuditStatus { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; }
        public string SortBy { get; set; }
        public bool IsAPIForm { get; set; }
    }
}
