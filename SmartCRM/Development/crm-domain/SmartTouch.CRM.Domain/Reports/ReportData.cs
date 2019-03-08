using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.Domain.Reports
{
    public class ReportData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int CurrentTotal { get; set; }
        public int PreviousTotal { get; set; }
        public IEnumerable<DropDownValue> DropdownValues { get; set; }
    }


    public class CustomReportData
    {
        public IEnumerable<CustomColumns> CustomData { get; set; }
    }

    public class CustomColumns
    {
        public string ColumnName {get; set;}
        public object ColumnValue {get; set;}
    }


    public class DropDownValue
    {
        public int DropdownValueId { get; set; }
        public string DropdownValueName { get; set; }
        public int DropdownValue { get; set; }
        public string DropdownType { get; set; }
        //public int TotalCount { get; set; }
        //public decimal? Potential { get; set; }
    }

    public class AreaChartData
    {
        public int ID { get; set; }       
        public int P { get; set; }
        public int C { get; set; }
    }

    public class PieChartData
    {
        public string GridValue { get; set; }
        public int P { get; set; }
        public int C { get; set; }
    }

    public class ReportResult
    {
        public IEnumerable<ReportData> GridData { get; set; }
        public IEnumerable<AreaChartData> AreaChartData { get; set; }
        public IEnumerable<PieChartData> PieChartData { get; set; }
        public IEnumerable<DatabasePieChartData> DatabasePiesChartData { get; set; }
        public IEnumerable<CustomReportData> CustomReportData { get; set; }
        public IEnumerable<AllLeadSourceReportGrid> AllLeadSourceData { get; set; }
        public IEnumerable<DatabaseLifeCycleGridData> AllDatabaseLifeCycleData { get; set; }
        public IEnumerable<DashboardPieChartDetails> DashboardPieCharData { get; set; }
        public IEnumerable<ReportData> TourGridData { get; set; }
        public int PreviousValue { get; set; }
    }


    public class ForsCountSummary
    {
        public int LeadAdapterAndAccountMapID { get; set; }
        public int FormID { get; set; }
        public string Name { get; set; }
        public string DropdownValue { get; set; }
        public short DropdownValueID { get; set; }
        public int UniqueSubmissions { get; set; }
        public int Total { get; set; }
        public bool IsAPIForm { get; set; }
    }

    public class HotlistData
    {
        public int ContactId { get; set; }
        public string FullName{ get; set; }
        public string AccountExec { get; set; }
        public string LeadSource { get; set; }
        public string Lifecycle { get; set; }
        public string Email { get; set; }
        public short? EmailStatus { get; set; }
        public int? ContactEmailId { get; set; }
        public string PhoneNumber { get; set; }
        public int? ContacPhoneNumberID { get; set; }
        public int LeadScore { get; set; }
        public int NewPoints { get; set; }
        public int? OwnerID { get; set; }
        public short? LeadSourceId { get; set; }
        public short? LifeCycleStageId { get; set; }
    }

    public class HotlistGridData
    {
        public IEnumerable<HotlistData> HotlistData { get; set; }
        public int TotalHits { get; set; }
        public IEnumerable<int> Contacts { get; set; }
    }

    public class CampaignReportData
    {
        public int CampaignID { get; set; }
        public string Name { get; set; }
        public string Subject { get; set; }
        public int TotalSends { get; set; }
        public int TotalOpens { get; set; }
        public int TotalClicks { get; set; }
        public int TotalDelivered { get; set; }
        public int TotalCompliants { get; set; }
        public string ProviderName { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string JobId { get; set; }
        public int WorkflowsCount { get; set; }
    }

    public class AllLeadSourceReportGrid
    {
        public int LeadSourceID { get; set; }
        public string Value { get; set; }
        public int PrimaryLSCount { get; set; }
        public int SecondaryLSCount { get; set; }
        public double PrimaryLSPercent { get; set; }
        public double SecondaryLSPercent { get; set; }
    }

    public class ReportAdvancedViewContact
    {
        public int ContactID { get; set; }
    }

    public class DatabaseLifeCycleGridData
    {
        public int LifecycleStageId { get; set; }
        public int ContactsCount { get; set; }
    }

    public class DatabasePieChartData
    {
        public int LifecycleStageId { get; set; }
        public int ContactsCount { get; set; }
    }
}
