using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ViewModels
{
    public class ReportViewModel
    {
        public int ReportId { get; set; }
        public int AccountId { get; set; }
        public IEnumerable<DropdownValueViewModel> LifecycleStages { get; set; }
        public int LifecycleStage { get; set; }
        public int OwnerId { get; set; }
        public long ShowTop { get; set; }
        public IEnumerable<Owner> Users { get; set; }
        public string ReportName { get; set; }
        //ComparedTo function
        public bool IsCompared { get; set; }
        public DateTime ComparedToPreviousDate { get; set; }
        public int PreviousPeriodId { get; set; }
        public int LoginFrequencyReportID { get; set; }

        public Entities.Reports ReportType { get; set; }
        public int? LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public int? CreatedOn { get; set; }
        public DateTime? CreatedBy { get; set; }
        public int PeriodId { get; set; }
        public int SearchDefinitionID { get; set; }
        public IEnumerable<SearchFilter> Filters { get; set; }
        public SearchPredicateType PredicateType { get; set; }
        public int SearchPredicateTypeID { get; set; }
        public string CustomPredicateScript { get; set; }
        public DateTime? LastRunDate { get; set; }
        public short PageNumber { get; set; }
        public IList<Field> SelectedColumns { get; set; }
        public IEnumerable<Field> Fields { get; set; }
        public int[] Module { get; set; }

        public DateTime CustomStartDate { get; set; }
        public DateTime CustomEndDate { get; set; }

        public DateTime CustomStartDatePrev { get; set; }
        public DateTime CustomEndDatePrev { get; set; }
        public Char DateRange { get; set; }
       
        public int[] OwnerIds { get; set; }
        public short[] LifeStageIds { get; set; }
        public int[] CommunityIds { get; set; }
        public int[] TourTypeIds { get; set; }
        public int[] TourStatusIds { get; set; }
        public short[] LeadSourceIds { get; set; }
        public int[] ModuleIds { get; set; }
        public int[] FormIds { get; set; }
        public int[] LeadAdapterIds { get; set; }
        public short[] OpportunityStageIds { get; set; }
        public int[] AccountIds { get; set; }
        public byte SubscriptionID { get; set; }
        public bool IsDashboard { get; set; }
        public char Type { get; set; }
        public int GroupId { get; set; }
        public int RowId { get; set; }
        public int ColumnIndex { get; set; }
        public string ActivityModule { get; set; }
        public IEnumerable<DropdownValueViewModel> Communities { get; set; }
        public IEnumerable<DropdownValueViewModel> TourTypes { get; set; }
        public IEnumerable<DropdownValueViewModel> LeadSources { get; set; }
        public IEnumerable<DropdownValueViewModel> OpportunityStages { get; set; }
        public IEnumerable<AccountListViewModel> AccountsList { get; set; }

        public string LeadSource { get; set; }
        public int EntityId { get; set; }
        public IEnumerable<int> ValueObjectIds { get; set; }
        public bool @IsDefaultDateRange { get; set; }
        public bool HasSelectedLinks { get; set; }

        public string SortField { get; set; }
        public string SortDirection { get; set; }
        //public int TourTypeId { get; set; }
        //public int TourCommunityId { get; set; }
        //public int TourStatusId { get; set; }
        public IList<sortFilter> Sorts { get; set; }
        public string DropdownType { get; set; }//Added for LIT Deployment on 06/05/2018
    }

    public class sortFilter
    {
        public string compare { get; set; }
        public string dir { get; set; }
        public string field { get; set; }

    }
}
