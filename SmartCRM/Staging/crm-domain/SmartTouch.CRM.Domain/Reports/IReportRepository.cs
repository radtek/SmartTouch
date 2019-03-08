using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Infrastructure.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using RE = SmartTouch.CRM.Entities;

namespace SmartTouch.CRM.Domain.Reports
{
    public interface IReportRepository : IRepository<Report, int>
    {
        IEnumerable<Report> FindAll(string name, int accountId, int userId, int pageNumber, int pageSize, out int totalHits, string sortField, string filter, IEnumerable<byte> moduleIds, ListSortDirection direction);

        Report FindReportByType(RE.Reports ReportType, int accountId);

        void UpdateLastRunActivity(int reportId, int userId, int accountId, string reportName);

        void InsertDefaultReports(int accountId);

        ReportResult GetFirstLeadSourceReport(ReportFilters filters);

        IEnumerable<ReportContact> GetFirstLEadSourceReportContacts(ReportFilters filters);

        ReportResult GetAllLeadSourceReport(ReportFilters filters);

        IEnumerable<ReportContact> GetAllLeadSourceReportContacts(ReportFilters filters);
        IEnumerable<ReportContact> GetAllDatabaseLifeCycleReportContacts(ReportFilters filters);

        ReportResult GetTrafficBySource(ReportFilters filters);

        IEnumerable<ReportContact> GetTrafficBySourceContacts(ReportFilters filters);

        ReportResult GetTrafficByLifecycle(ReportFilters filters);

        IEnumerable<ReportContact> GetTrafficByLifeCycleContacts(ReportFilters filters);

        ReportResult GetTrafficByType(ReportFilters filters);

        IEnumerable<ReportContact> GetTrafficByTypeContacts(ReportFilters filters);

        ReportResult GetOpportunityPipeline(ReportFilters filters);

        IEnumerable<ReportContact> GetOpportunityPipelineContacts(ReportFilters filters);

        ReportResult GetActivitiesByModule(ReportFilters filters);

        List<int> GetActivityReportContactIds(ReportFilters filters);

        List<int> GetActivityReportTourContactIds(ReportFilters filters);

        List<int> GetActivityReportNoteContactsIds(ReportFilters filters);

        List<int> GetActivityReportDrildownModuleContactIds(ReportFilters filters);

        IEnumerable<ForsCountSummary> GetFormsCountSummaryData(DateTime startDate, DateTime endDate, int[] formIds, int[] leadAdapterIds, int groupId, int accountId, int userId, bool isAdmin);

        IEnumerable<ReportContactInfo> GetBDXFreemiumCustomLeadReportData(DateTime startDate, DateTime endDate, int accountId, bool isAdmin, int userID);

        ReportResult GetTrafficByTypeAndLifeCycle(ReportFilters filters);

        IEnumerable<ReportContact> GetTrafficByTypeAndLifeCycleContacts(ReportFilters filters);

        IEnumerable<ReportContact> GetOnlineRegisteredContacts(ReportFilters filters);

        HotlistGridData GetHotlistReportData(ReportFilters filters);

        ReportResult GetNewLeadsVisuvalization(ReportFilters filters);

        void InsertUserDashboardSettings(int userId, IList<Byte> dashboardId);

        IEnumerable<DashboardItems> GetDashboardItems(int userId);

        IEnumerable<BDXCustomLeadContactInfo> GetBDXCustomLeadReportContactInfo(DateTime startDate, DateTime endDate, int accountId, bool isAdmin, int userID);

        IEnumerable<Report> FindAllCustomReports(string name, int accountId, int userId, int pageNumber, int pageSize, out int totalHits, string sortField, ListSortDirection direction);

        ReportResult GetCustomReportData(ReportFilters reportFilters);

        bool GetCustomReports(int accountId);

        IList<WebVisitReport> GetWebVisitsReport(ReportFilters filters, out int TotalHits);

        string GetReportNameByType(int accountId, byte reportType);
        ReportResult GetAllDatabaseLifeCycleData(ReportFilters filters);
        IEnumerable<TourByContactReportInfo> GetTourByContactsReportData(int accountId,DateTime startDate,DateTime endDate,int[] tourStatus,int[] tourType,int[] tourCommunity,int pageSize,int pageNumber,string sortField,string sortDirection);

        IEnumerable<dynamic> GetNightlyStatusReportData(ReportFilters filters);
        IEnumerable<LoginFrequencyReport> GetLoginFrequencyReportData(ReportFilters filters);

        int GetImportDropdownValue(int accountId);
    }
}
