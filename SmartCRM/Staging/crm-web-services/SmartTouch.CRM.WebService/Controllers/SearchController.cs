using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.WebService.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.Controllers
{
    /// <summary>
    /// Creating search controller
    /// </summary>
    public class SearchController : SmartTouchApiController
    {
        readonly IAdvancedSearchService advancedSearchService;
        /// <summary>
        /// Creating constructor for search controller for accessing
        /// </summary>
        /// <param name="advancedSearchService"></param>
        public SearchController(IAdvancedSearchService advancedSearchService)
        {
            this.advancedSearchService = advancedSearchService;
        }

        /// <summary>
        /// Inserts a new Search.
        /// </summary>
        /// <param name="viewModel">Properties of a Advanced Search.</param>
        /// <returns>Saved Search Insertion Details Response</returns>
        [Route("AdvancedSearch")]
        [HttpPost]
        public HttpResponseMessage InsertSearch(AdvancedSearchViewModel viewModel)
        {
            viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            viewModel.CreatedBy = this.UserId;
            viewModel.AccountID = this.AccountId;
            SaveAdvancedSearchResponse response = advancedSearchService.InsertSavedSearch(
              new SaveAdvancedSearchRequest()
              {
                  AdvancedSearchViewModel = viewModel,
                  AccountId = this.AccountId,
                  RequestedBy = this.UserId,
                  RoleId = this.RoleId
              });

            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Updates an Advanced Search.
        /// </summary>
        /// <param name="viewModel">Properties of a Advanced Search.</param>
        /// <returns>Saved Search Updation Details Response</returns>
        [Route("AdvancedSearch")]
        [HttpPut]
        public HttpResponseMessage UpdateSearch(AdvancedSearchViewModel viewModel)
        {
            SaveAdvancedSearchResponse response = advancedSearchService.UpdateSavedSearch(
              new SaveAdvancedSearchRequest()
              {
                  AdvancedSearchViewModel = viewModel,
                  AccountId = this.AccountId,
                  RequestedBy = this.UserId,
                  RoleId = this.RoleId
              });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Runs an Advanced Search.
        /// </summary>
        /// <param name="viewModel">Properties of a Advanced Search.</param>
        /// <returns>Saved Search Details</returns>
        [Route("runadvancedsearch")]
        [HttpPost]
        [Authorize]
        public async Task<HttpResponseMessage> RunSearch(AdvancedSearchViewModel viewModel)
        {
            if (viewModel.AccountID == 0)
                viewModel.AccountID = this.AccountId;
            var pageSize = ReadCookie("savedsearchpagesize");
            var limit = 10;
            if (pageSize != null)
                int.TryParse(pageSize, out limit);

            AdvancedSearchResponse<ContactAdvancedSearchEntry> response = await advancedSearchService.RunSearchAsync(
              new AdvancedSearchRequest<ContactAdvancedSearchEntry>()
              {
                  SearchViewModel = viewModel,
                  AccountId = this.AccountId,
                  RoleId = this.RoleId,
                  RequestedBy = this.UserId,
                  IsAdvancedSearch = true,
                  Limit = limit,
              });
            return Request.BuildResponse(response);
        }

        /// <summary>
        /// Exports AdvancedSearch Result to CSV File.
        /// </summary>
        /// <param name="viewModel">Properties of Advanced Search.</param>
        /// <returns>Exported File Response Details</returns>
        [Route("exportcsvfile")]
        [HttpPost]
        public async Task<HttpResponseMessage> ExportToCSVFile(AdvancedSearchViewModel viewModel)
        {
            if (viewModel.AccountID == 0)
                viewModel.AccountID = this.AccountId;
            ExportSearchResponse response = await advancedSearchService.ExportSearchToCSVAsync(
              new ExportSearchRequest()
              {
                  SearchViewModel = viewModel
              });

            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            Stream stream = new MemoryStream(response.byteArray);
            result.Content = new StreamContent(stream);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = response.FileName
            };

            return result;
        }

        /// <summary>
        /// Gets Saved Searches.
        /// </summary>
        /// <param name="query">String query.</param>
        /// <returns>All Saved Searches</returns>
        [Route("getsavedsearches")]
        [HttpGet]
        public HttpResponseMessage GetSavedSearches(string query)
        {
            GetSavedSearchesRequest request = new GetSavedSearchesRequest();
            request.RequestedBy = this.UserId;
            request.AccountID = this.AccountId;
            request.Query = query;
            request.Limit = 10;
            request.PageNumber = 1;
            GetSavedSearchesResponse response = advancedSearchService.GetAllSavedSearches(request);
            return Request.BuildResponse(response);
        }


        /// <summary>
        /// Get search data.
        /// </summary>
        /// <param name="strValue">Search string.</param>
       
        public string ReadCookie(string strValue)
        {
            string strValues = string.Empty;

            var cookie = Request.Headers.GetValues(strValue).FirstOrDefault();

            if (cookie != null)
            {
                strValues = cookie;
            }
            return strValues;
        }
        /// <summary>
        /// Get contacts by saved search
        /// </summary>
        /// <param name="id"></param>
        /// <param name="accountId"></param>
        /// <param name="userId"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [Route("getcontactsbysearchdefinition")]
        [AllowAnonymous]
        public async Task<HttpResponseMessage> GetContactsFromSearchDefinition(int id, int accountId, int userId, int roleId)
        {
            GetCampaignRecipientIdsResponse response = new GetCampaignRecipientIdsResponse();
            try
            {
                var contacts = await advancedSearchService.GetSavedSearchContactIds(new GetSavedSearchContactIdsRequest()
                {
                    AccountId = accountId,
                    RequestedBy = userId,
                    RoleId = (short)roleId,
                    SearchDefinitionId = id
                });
                response.ContactIds = contacts;
            }
            catch(Exception ex)
            {
                response.Exception = ex;
            }
            
            return Request.BuildResponse(response);
        }
    }
}
