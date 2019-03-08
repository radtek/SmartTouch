using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;

namespace SmartTouch.CRM.ApplicationServices.ServiceInterfaces
{
    public interface IAdvancedSearchService
    {
        QuickSearchResponse QuickSearch(QuickSearchRequest request);
        GetSavedSearchesResponse GetAllSavedSearches(GetSavedSearchesRequest request);
        SaveAdvancedSearchResponse InsertSavedSearch(SaveAdvancedSearchRequest request);
        SaveAdvancedSearchResponse UpdateSavedSearch(SaveAdvancedSearchRequest request);
        DeleteSearchResponse DeleteSearches(DeleteSearchRequest request);
        Task<GetSearchResponse> GetSavedSearchAsync(GetSearchRequest request);
        Task<GetSearchResponse> GetSavedSearchWithEmailFilterAppendedAsync(GetSearchRequest request);
        Task<List<int>> GetSavedSearchContactIds(GetSavedSearchContactIdsRequest request);
        Task<List<Person>> GetActiveContactIds(GetSavedSearchContactIdsRequest request);
        Task<AdvancedSearchResponse<T>> RunSearchAsync<T>(AdvancedSearchRequest<T> request) where T : IShallowContact;
        Task<AdvancedSearchResponse<T>> ViewContactsAsync<T>(AdvancedSearchRequest<T> request) where T : IShallowContact;
        Task<ExportSearchResponse> ExportSearchAsync(ExportSearchRequest request);
        Task<ExportSearchResponse> ExportSearchToCSVAsync(ExportSearchRequest request);
        Task<ExportSearchResponse> ExportSearchToExcelAsync(ExportSearchRequest request);
        Task<CampaignRecipientsSummaryResponse> ContactSummaryBySearchDefinitionAsync(CampaignRecipientsSummaryRequest request);
        GetAdvanceSearchFieldsResponse GetSearchFields(GetAdvanceSearchFieldsRequest request);
        GetUpdatableFieldsResponse GetUpdatableFields(GetUpdatableFieldsRequest request);
        GetSearchValueOptionsResponse GetSearchValueOptions(GetSearchValueOptionsRequest request);
        Task<ExportSearchResponse> ExportSearchToPDFAsync(ExportSearchRequest request);
        bool SaveAsFavoriteSearch(short searchDefinitionId);
        InsertLastRunActivityResponse InsertLastRun(InsertLastRunActivityRequest request);
        InsertViewActivityResponse InsertViewActivity(InsertViewActivityRequest request);
        int IndexSavedSearches(int accountId);
        SaveAdvancedSearchResponse InsertSavedSearchForSelectAll(SaveAdvancedSearchRequest request);
        GetSearchDefinitionDescriptionRespone GetSearchDefinitionDescription(GetSearchDefinitionDescriptionRequest request);
        GetAdvancedViewColumnsResponse GetColumns(GetAdvancedViewColumnsRequest request);
        SaveAdvancedViewColumnsResponse SaveColumns(SaveAdvancedViewColumnsRequest request);
        Task<List<Contact>> GetContactEmails(GetContactEmailsRequest request);
        SearchDefinition GetSavedSearch(GetSearchRequest request);
    }
}
