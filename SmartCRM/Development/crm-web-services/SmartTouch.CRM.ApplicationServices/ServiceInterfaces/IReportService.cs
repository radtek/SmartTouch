using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Reports;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Opportunities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IReportService
    {
        GetReportsResponse GetReportList(GetReportsRequest request);
        Task<HotListResponse> HotListDataAsync(HotListRequest request);
        StandardReportResponse RunCampaignReport(StandardReportRequest request);
     
        Task<StandardReportResponse> GetNewLeadsListAsync(StandardReportRequest request);
    //    Task<StandardReportResponse> GetNewLeadsVisualizationAsync(StandardReportRequest request);
        StandardReportResponse GetNewLeadsVisualizationAsync(StandardReportRequest request);
        ReportDataResponse GetTrafficByTypeData(ReportDataRequest request);
        ReportDataResponse GetTrafficBySourceData(ReportDataRequest request);
        ReportDataResponse GetTrafficByLifeCycleData(ReportDataRequest request);
        ReportDataResponse GetOpportunityPipeline(ReportDataRequest request);
        ReportDataResponse GetActivityData(ReportDataRequest request);
        ReportDataResponse GetTrafficByTypeAndLifeCycleData(ReportDataRequest request);
       
        ReportDataResponse GetTrafficByTypeContacts(ReportDataRequest request);
        ReportDataResponse GetOpportunityPipelineContacts(ReportDataRequest request);
        ReportDataResponse GetTrafficBySourceContacts(ReportDataRequest request);
        ReportDataResponse GetTrafficByLifeCycleContacts(ReportDataRequest request);
        ReportDataResponse GetTrafficByTypeAndLifeCycleContacts(ReportDataRequest request);

        ReportDataResponse RunFirstLeadSourceReport(ReportDataRequest request);
        ReportDataResponse FirstLeadSourceReportContacts(ReportDataRequest request);
        ReportDataResponse RunAllLeadSourceReport(ReportDataRequest request);
        ReportDataResponse AllLeadSourceReportContacts(ReportDataRequest request);
        ReportDataResponse AllDatabaseReportContacts(ReportDataRequest request);

        InsertDefaultReportsResponse InsertDefaultReports(InsertDefaultReportsRequest request);
        InsertLastRunActivityResponse UpdateLastRunActivity(InsertLastRunActivityRequest request);
        StandardReportResponse GetFormsCountSummaryReport(StandardReportRequest request);
        StandardReportResponse GetBDXFreemiumCustomLeadReportDetails(StandardReportRequest request);
        GetReportsResponse GetReportByType(GetReportsRequest request);
        ReportDataResponse GetOnlineRegisteredContacts(ReportDataRequest request);
        StandardReportResponse GetDashboardcampaignList(StandardReportRequest request);
        ReportDataResponse GetNewContactsVisualizationData(ReportDataRequest request);
        ReportDataResponse GetHotListData(ReportDataRequest request);
        ReportDataResponse GetActivityReportContacts(ReportDataRequest request);
        ReportDataResponse GetActivityReportTourContacts(ReportDataRequest request);
        ReportDataResponse GetActivityReportNoteContacts(ReportDataRequest request);
        ReportDataResponse GetDatabaseLifeCycleData(ReportDataRequest request);
        InsertUserDashboardSettingsResponse InsertUserDashboardSettings(InsertUserDashboardSettingsRequest request);
        GetDashboardItemsResponse GetDashboardItems(int userId);

        BDXCustomLeadReportResponse GetBDXCustomLeadReportDetails(BDXCustomLeadReportRequest request);

        GetReportsResponse GetCustomReportList(GetReportsRequest getReportsRequest);

        ReportDataResponse GetCustomReportData(ReportDataRequest request);

        CustomReportDataResponse GetCustomReports(CustomReportDataRequest request);
        ReportDataResponse GetWebVisitsReport(ReportDataRequest request);

        BDXCustomLeadReportResponse GetBDXCustomLeadReportDetailsExport(BDXCustomLeadReportRequest bDXCustomLeadReportRequest);

        ReportExportResponce RunCampaignReportExport(ReportExportRequest request);

        StandardReportResponse GetUntouchedLeadsVisualizationAsync(StandardReportRequest request);
        GetReportNameByTypeResponse GetReportNameByType(GetReportNameByTypeRequest request);
        GetReEngagementInfoResponse GetReEngagedContacts(GetReEngagementInfoRequest request);
        GetWorkflowsForCampaignReportResponse GetWorkflowsForCampaignReport(GetWorkflowsForCampaignReportRequest request);
        TourByContactsReponse GetTourByContactsReportData(TourByContactsRequest request);

        StandardReportResponse RunNightlyStatusReport(ReportDataRequest request);
        StandardReportResponse RunLoginFrequencyReport(ReportDataRequest request);
    }
}
