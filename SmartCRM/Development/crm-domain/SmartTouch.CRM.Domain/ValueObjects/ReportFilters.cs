using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Entities;
using System;

namespace SmartTouch.CRM.Domain.ValueObjects
{
    public class ReportFilters : ValueObjectBase
    {
        public int UserId { get; set; }
        public int AccountId { get; set; }
        public int Type { get; set; }
        public int Top5Only { get; set; }
        public char DateRange { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ReportId { get; set; }


        public int[] SelectedOwners { get; set; }
        public int[] SelectedCommunities { get; set; }
        public int[] TrafficType { get; set; }
        public short[] TrafficSource { get; set; }
        public short[] TrafficLifeCycle { get; set; }
        public int[] ModuleIDs { get; set; }
        public int[] FormIDs { get; set; }
        public int[] LeadAdapterIDs { get; set; }
        public int[] AccountIDs { get; set; }
        public byte SubscriptionID { get; set; }
        public DateTime StartDatePrev { get; set; }
        public DateTime EndDatePrev { get; set; }
        public bool IsComparedTo { get; set; }
        public bool IsAdmin { get; set; }
        public int RowId { get; set; }
        public int ColumnIndex { get; set; }

        public int PageNumber { get; set; }
        public int PageLimit { get; set; }
        public string SortField { get; set; }
        public string SortDirection { get; set; }
        public bool IsDasboardView { get; set; }
        public string DropdownType { get; set; }
        public byte ModuleId { get; set; }

        protected override void Validate()
        {
            throw new NotImplementedException();
        }
    }
}
