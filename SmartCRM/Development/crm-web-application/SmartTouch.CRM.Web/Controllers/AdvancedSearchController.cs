using Kendo.Mvc.UI;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Utilities.PDFGeneration;
using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.SessionState;

namespace SmartTouch.CRM.Web.Controllers
{
    public class AdvancedSearchController : SmartTouchController
    {
        readonly IAdvancedSearchService advancedSearchService;
        readonly IFormService formService;
        readonly ICachingService cachingService;

        public AdvancedSearchController(IAdvancedSearchService advancedSearchService, ICachingService cachingService, IFormService formService)
        {
            this.advancedSearchService = advancedSearchService;
            this.cachingService = cachingService;
            this.formService = formService;
        }

        /// <summary>
        /// Advanceds the search.
        /// </summary>
        /// <returns></returns>
        [Route("addsearch")]
        [MenuType(MenuCategory.AdvancedSearch, MenuCategory.LeftMenuCRM)]
        public ActionResult AdvancedSearch()
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            IList<FilterViewModel> filters = new List<FilterViewModel>();
            FilterViewModel filterviewmodel = new FilterViewModel();
            filterviewmodel.FieldId = (int)ContactFields.FirstNameField;
            filterviewmodel.SearchDefinitionID = 0;
            filterviewmodel.SearchFilterID = 0;
            filterviewmodel.SearchQualifierTypeID = (short)SearchQualifier.Is;
            filterviewmodel.SearchText = string.Empty;
            FilterViewModel lastNameFilter = new FilterViewModel();
            lastNameFilter.FieldId = (int)ContactFields.LastNameField;
            lastNameFilter.SearchDefinitionID = 0;
            lastNameFilter.SearchFilterID = 0;
            lastNameFilter.SearchQualifierTypeID = (short)SearchQualifier.Is;
            lastNameFilter.SearchText = string.Empty;
            filters.Add(filterviewmodel);
            filters.Add(lastNameFilter);
            IEnumerable<FieldViewModel> searchFields = null;
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            if (response.FieldsViewModel != null)
            {
                searchFields = response.FieldsViewModel;
            }
            AdvancedSearchViewModel searchViewModel = new AdvancedSearchViewModel()
            {
                SearchDefinitionName = "",
                SearchPredicateTypeID = 1,
                SearchFilters = filters,
                SearchFields = searchFields
            };
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            AddCookie("savedsearchpagesize", ItemsPerPage.ToString(), 1);
            int defaultPageNumber = 1;
            AddCookie("advancedsearchpagenumber", defaultPageNumber.ToString(), 1);
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            bool gridvisible = false;
            ViewBag.grid = gridvisible;
            return View("AddEditSearch", searchViewModel);
        }

        /// <summary>
        /// Saveds the search.
        /// </summary>
        /// <returns></returns>
        [Route("advancedsearch/search")]
        [MenuType(MenuCategory.AdvancedSearchList, MenuCategory.LeftMenuCRM)]
        public ActionResult SavedSearch()
        {
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.SearchDefinitionId = 1;
            return View("AdvancedSearchList");
        }

        /// <summary>
        /// Advanceds the search list.
        /// </summary>
        /// <returns></returns>
        [Route("advancedsearchlist")]
        [MenuType(MenuCategory.AdvancedSearchList, MenuCategory.LeftMenuCRM)]
        [OutputCache(Duration = 30)]
        public ActionResult AdvancedSearchList()
        {
            AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.DateFormat = this.Identity.ToDateFormat();
            ViewBag.SearchDefinitionId = 0;
            return View("AdvancedSearchList", viewModel);
        }

        /// <summary>
        /// Advanceds the search view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ActionResult AdvancedSearchViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("advancedsearchpagesize", request.PageSize.ToString(), 1);
            AddCookie("advancedsearchpagenumber", request.Page.ToString(), 1);
            GetSavedSearchesResponse response = advancedSearchService.GetAllSavedSearches(new GetSavedSearchesRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity),
                IsFavoriteSearch = false,
                IsPredefinedSearch = false,
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new DataSourceResult
            {
                Data = response.SearchResults,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Predifineds the advanced search view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ActionResult PredifinedAdvancedSearchViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("advancedsearchpagesize", request.PageSize.ToString(), 1);
            AddCookie("advancedsearchpagenumber", request.Page.ToString(), 1);
            GetSavedSearchesResponse response = advancedSearchService.GetAllSavedSearches(new GetSavedSearchesRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity),
                IsPredefinedSearch = true,
                IsFavoriteSearch = false,
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new DataSourceResult
            {
                Data = response.SearchResults,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Favourates the advanced search view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ActionResult FavourateAdvancedSearchViewRead([DataSourceRequest] DataSourceRequest request, string name)
        {
            AddCookie("advancedsearchpagesize", request.PageSize.ToString(), 1);
            AddCookie("advancedsearchpagenumber", request.Page.ToString(), 1);
            GetSavedSearchesResponse response = advancedSearchService.GetAllSavedSearches(new GetSavedSearchesRequest()
            {
                Query = name,
                Limit = request.PageSize,
                PageNumber = request.Page,
                AccountID = UserExtensions.ToAccountID(this.Identity),
                IsFavoriteSearch = true,
                IsPredefinedSearch = false,
                RequestedBy = this.Identity.ToUserID()
            });
            return Json(new DataSourceResult
            {
                Data = response.SearchResults,
                Total = response.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Contactses the result view read.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="aviewModel">The aview model.</param>
        /// <returns></returns>
        [SmarttouchSessionStateBehaviour(SessionStateBehavior.Required)]
        public async Task<ActionResult> ContactsResultViewRead([DataSourceRequest] DataSourceRequest request, string aviewModel)
        {
            AdvancedSearchViewModel viewModel = JsonConvert.DeserializeObject<AdvancedSearchViewModel>(aviewModel);
            if (viewModel.AccountID == 0)
                viewModel.AccountID = this.Identity.ToAccountID();
            var pageSize = request.PageSize;
            Session["AdvancedSearchVM"] = viewModel;
            AdvancedSearchResponse<ContactAdvancedSearchEntry> response;
            try
            {
                response = await advancedSearchService.RunSearchAsync(new AdvancedSearchRequest<ContactAdvancedSearchEntry>()
                {
                    SearchViewModel = viewModel,
                    AccountId = this.Identity.ToAccountID(),
                    RoleId = this.Identity.ToRoleID(),
                    RequestedBy = this.Identity.ToUserID(),
                    IsAdvancedSearch = true,
                    Limit = pageSize,
                    PageNumber = request.Page
                });
            }
            catch (Exception ex)
            {
                return Json(new DataSourceResult
                {
                    Errors = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new DataSourceResult
            {
                Data = (dynamic)response.SearchResult.Results,
                Total = (int)response.SearchResult.TotalHits
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Edits the search.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <returns></returns>
        [Route("editsearch")]
        [MenuType(MenuCategory.AdvancedSearch, MenuCategory.LeftMenuCRM)]
        public async Task<ActionResult> EditSearch(short SearchDefinitionID)
        {
            var identity = Thread.CurrentPrincipal.Identity;
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            var userId = identity.ToUserID();
            var isSTAdmin = identity.IsSTAdmin();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            AddCookie("savedsearchpagesize", ItemsPerPage.ToString(), 1);
            GetSearchResponse response = await advancedSearchService.GetSavedSearchAsync(new GetSearchRequest()
            {
                SearchDefinitionID = SearchDefinitionID,
                IncludeSearchResults = false,
                Limit = ItemsPerPage,
                AccountId = accountId,
                RoleId = roleId,
                RequestedBy = userId,
                IsSTAdmin = isSTAdmin
            });
            response.SearchViewModel.CreatedOn = response.SearchViewModel.CreatedOn.ToJSDate();
            foreach (var item in response.SearchViewModel.SearchFilters)
            {
                if (item.InputTypeId == 2)
                {
                    DateTime date = DateTime.Parse(item.SearchText.ToString());
                    item.SearchText = date.ToString();
                }
            }
            bool gridvisible = false;
            ViewBag.grid = gridvisible;
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            return View("AddEditSearch", response.SearchViewModel);
        }

        /// <summary>
        /// Views the search.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <returns></returns>
        [Route("viewsearch")]
        [MenuType(MenuCategory.AdvancedSearch, MenuCategory.LeftMenuCRM)]
        public async Task<ActionResult> ViewSearch(short SearchDefinitionID)
        {
            bool gridvisible = false;
            ViewBag.grid = gridvisible;
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            var userId = this.Identity.ToUserID();
            var isSTAdmin = this.Identity.IsSTAdmin();
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            AddCookie("savedsearchpagesize", ItemsPerPage.ToString(), 1);
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            GetSearchResponse response = await advancedSearchService.GetSavedSearchAsync(new GetSearchRequest()
            {
                SearchDefinitionID = SearchDefinitionID,
                AccountId = accountId,
                RoleId = roleId,
                IsSTAdmin = isSTAdmin
            });
            response.SearchViewModel.CreatedOn = response.SearchViewModel.CreatedOn.ToJSDate();
            if (SearchDefinitionID != 0 && response.SearchViewModel != null)
                advancedSearchService.InsertViewActivity(new InsertViewActivityRequest()
                {
                    AccountId = accountId,
                    RequestedBy = userId,
                    SearchDefinitionId = SearchDefinitionID,
                    SearchName = response.SearchViewModel.SearchDefinitionName
                });
            return View("AddEditSearch", response.SearchViewModel);
        }

        /// <summary>
        /// Views the contacts.
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ViewContacts(AdvancedSearchViewModel viewModel)
        {
            if (viewModel.AccountID == 0)
                viewModel.AccountID = this.Identity.ToAccountID();
            AdvancedSearchResponse<ContactAdvancedSearchEntry> response = await advancedSearchService.ViewContactsAsync(new AdvancedSearchRequest<ContactAdvancedSearchEntry>()
            {
                SearchViewModel = viewModel,
                AccountId = viewModel.AccountID,
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID(),
                IsAdvancedSearch = true,
                ViewContacts = true
            });
            Guid guid = Guid.NewGuid();
            if (response.ContactIds != null)
            {
                AddCookie("ContactsGuid", guid.ToString(), 1);
                var contactIds = response.ContactIds;
                cachingService.StoreSavedSearchContactIds(guid.ToString(), contactIds);
            }
            return Json(new
            {
                success = true,
                response = guid.ToString()
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Runs the search.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <param name="IsPreConfigSearch">if set to <c>true</c> [is pre configuration search].</param>
        /// <param name="IsFavoriteSearch">if set to <c>true</c> [is favorite search].</param>
        /// <returns></returns>
        [Route("runsearch")]
        [MenuType(MenuCategory.AdvancedSearch, MenuCategory.LeftMenuCRM)]
        public async Task<ActionResult> RunSearch(short SearchDefinitionID, bool IsPreConfigSearch, bool IsFavoriteSearch)
        {
            var identity = Thread.CurrentPrincipal.Identity;
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            AddCookie("savedsearchpagesize", ItemsPerPage.ToString(), 1);
            var accountId = identity.ToAccountID();
            var roleId = identity.ToRoleID();
            var userId = identity.ToUserID();
            var isSTAdmin = identity.IsSTAdmin();
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            bool isAccountAdmin = cachingService.IsAccountAdmin(this.Identity.ToRoleID(), this.Identity.ToAccountID());
            ViewBag.IsAccountAdmin = isAccountAdmin;
            GetSearchResponse response = await advancedSearchService.GetSavedSearchAsync(new GetSearchRequest()
            {
                SearchDefinitionID = SearchDefinitionID,
                IncludeSearchResults = false,
                Limit = ItemsPerPage,
                AccountId = accountId,
                RoleId = roleId,
                RequestedBy = userId,
                IsRunSearchRequest = true,
                IsSTAdmin = isSTAdmin
            });
            response.SearchViewModel.CreatedOn = response.SearchViewModel.CreatedOn.ToJSDate();
            response.SearchViewModel.IsFavoriteSearch = IsFavoriteSearch;
            response.SearchViewModel.IsPreConfiguredSearch = IsPreConfigSearch;
            if (SearchDefinitionID != 0 && response.SearchViewModel != null)
                advancedSearchService.InsertLastRun(new InsertLastRunActivityRequest()
                {
                    AccountId = accountId,
                    RequestedBy = userId,
                    SearchDefinitionId = SearchDefinitionID,
                    SearchName = response.SearchViewModel.SearchDefinitionName
                });
            bool gridvisible = true;
            ViewBag.grid = gridvisible;
            ViewBag.ItemsPerPage = ItemsPerPage;
            ViewBag.IsRunSearch = true;
            return View("AddEditSearch", response.SearchViewModel);
        }

        /// <summary>
        /// Copies the search.
        /// </summary>
        /// <param name="SearchDefinitionID">The search definition identifier.</param>
        /// <returns></returns>
        [Route("copysearch")]
        [MenuType(MenuCategory.AdvancedSearch, MenuCategory.LeftMenuCRM)]
        public async Task<ActionResult> CopySearch(short SearchDefinitionID)
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            var isSTAdmin = this.Identity.IsSTAdmin();
            GetSearchResponse response = await advancedSearchService.GetSavedSearchAsync(new GetSearchRequest()
            {
                SearchDefinitionID = SearchDefinitionID,
                IncludeSearchResults = false,
                AccountId = this.Identity.ToAccountID(),
                RoleId = roleId,
                IsSTAdmin = isSTAdmin
            });
            response.SearchViewModel.CreatedOn = response.SearchViewModel.CreatedOn.ToJSDate();
            response.SearchViewModel.SearchDefinitionName = "";
            response.SearchViewModel.SearchDefinitionID = 0;
            IList<FilterViewModel> searchFilters = response.SearchViewModel.SearchFilters;
            foreach (FilterViewModel searchfilter in searchFilters)
            {
                searchfilter.SearchFilterID = 0;
                searchfilter.SearchDefinitionID = 0;
            }
            short ItemsPerPage = default(short);
            short.TryParse(this.Identity.ToItemsPerPage(), out ItemsPerPage);
            AddCookie("savedsearchpagesize", ItemsPerPage.ToString(), 1);
            ViewBag.EmailPermission = cachingService.CheckSendMailPermissions(accountId, roleId);
            bool gridvisible = false;
            ViewBag.grid = gridvisible;
            response.SearchViewModel.SearchFilters = searchFilters;
            return View("AddEditSearch", response.SearchViewModel);
        }

        /// <summary>
        /// Adds the cookie.
        /// </summary>
        /// <param name="cookieName">Name of the cookie.</param>
        /// <param name="Value">The value.</param>
        /// <param name="days">The days.</param>
        public void AddCookie(string cookieName, string Value, int days)
        {
            HttpCookie CartCookie = new HttpCookie(cookieName, Value);
            CartCookie.Expires = DateTime.Now.AddDays(days);
            Response.Cookies.Add(CartCookie);
        }

        /// <summary>
        /// Deletes the searches.
        /// </summary>
        /// <param name="SearchID">The search identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteSearches(List<int> SearchID)
        {
            DeleteSearchRequest request = new DeleteSearchRequest()
            {
                SearchIDs = SearchID,
                AccountId = this.Identity.ToAccountID(),
                RequestedBy = this.Identity.ToUserID()
            };
            DeleteSearchResponse response = advancedSearchService.DeleteSearches(request);
            return Json(new
            {
                success = true,
                response = response.ResponseMessage
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Advanced search export.
        /// </summary>
        /// <returns></returns>
        public ActionResult _AdvancedSearchExport()
        {
            var accountId = this.Identity.ToAccountID();
            var roleId = this.Identity.ToRoleID();
            AdvancedSearchExportViewModel exportModel = new AdvancedSearchExportViewModel();
            IEnumerable<FieldViewModel> searchFields = null;
            GetAdvanceSearchFieldsResponse response = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest()
            {
                accountId = accountId,
                RoleId = roleId
            });
            if (response.FieldsViewModel != null)
            {
                searchFields = ExcludeNonViewableFields(response.FieldsViewModel);
            }
            exportModel.SearchFields = searchFields;
            return PartialView("_AdvancedSearchExport", exportModel);
        }

        private IEnumerable<FieldViewModel> ExcludeNonViewableFields(IEnumerable<FieldViewModel> fields)
        {
            IEnumerable<FieldViewModel> fieldModel = new List<FieldViewModel>();
            if (fields.IsAny())
                fieldModel = fields.Where(w => w.FieldId != 42 && w.FieldId != 45 && w.FieldId != 46 && w.FieldId != 47 && w.FieldId != 48 && w.FieldId != 49 && w.FieldId != 56 && w.FieldId != 57 && w.FieldId != 58 && w.FieldId != 60
                    && w.FieldId != 63 && w.FieldId != 64 && w.FieldId != 65 && w.FieldId != 66 && w.FieldId != 67 && w.FieldId != 71 && w.FieldId != 72);
            return fieldModel;
        }

        /// <summary>
        /// Exports the results.
        /// </summary>
        /// <param name="exportViewModel">The export view model.</param>
        /// <returns></returns>
        public async Task<ActionResult> ExportResults(AdvancedSearchExportViewModel exportViewModel)
        {
            var viewModel = (AdvancedSearchViewModel)Session["AdvancedSearchVM"];
            if (viewModel != null)
                viewModel.PageNumber = 1;
            int accountID = this.Identity.ToAccountID();
            string accountsids = System.Configuration.ConfigurationManager.AppSettings["Excluded_Accounts"].ToString();
            bool accountFound = accountsids.Contains(accountID.ToString());
            if (accountFound)
                throw new UnsupportedOperationException("[| You do not have permission to perform this operation.|]");
            ExportSearchResponse exportResponse = await advancedSearchService.ExportSearchAsync(new ExportSearchRequest()
            {
                SearchViewModel = viewModel,
                DownloadType = exportViewModel.DownloadType,
                AccountId = accountID,
                DateFormat = this.Identity.ToDateFormat(),
                TimeZone = this.Identity.ToTimeZone(),
                SelectedFields = exportViewModel.SelectedFields,
                SelectedContactIds = exportViewModel.SelectedContactIds
            });
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);

            //FormData data = formService.GetFormData(9849);
            //JavaScriptSerializer js = new JavaScriptSerializer();
            //var deserializedForm = js.Deserialize<Dictionary<string, Dictionary<string, string>>>(data.SubmittedData);
            //var model = (Dictionary<string, Dictionary<string, string>>)deserializedForm;
            //PDFGenerator pdfGen = new PDFGenerator();
            //byte[] bytes = pdfGen.GenerateFormSubmissionPDF(model, "LOCALHOST", "TEST FORM", "https://images.smarttouchinteractive.com/2039/ai/011b2149-27c1-4916-9605-72b25ac8235f.jpg");
            //bool result1 = cachingService.StoreTemporaryFile("64576457", bytes);


            Logger.Current.Informational("Did temporary file stored in cache : " + result.ToString());
            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = exportResponse.FileName
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Saves as favorite search.
        /// </summary>
        /// <param name="searchDefinitionId">The search definition identifier.</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveAsFavoriteSearch(short searchDefinitionId)
        {
            bool IsSavedAsFavoriteSearch = advancedSearchService.SaveAsFavoriteSearch(searchDefinitionId);
            return Json(new
            {
                success = true,
                IsSavedAsFavoriteSearch = IsSavedAsFavoriteSearch
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Exports to CSV file.
        /// </summary>
        /// <param name="advancedSearchViewModel">The advanced search view model.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> ExportToCSVFile(AdvancedSearchViewModel advancedSearchViewModel)
        {
            if (advancedSearchViewModel.AccountID == 0)
                advancedSearchViewModel.AccountID = this.Identity.ToAccountID();
            advancedSearchViewModel.PageNumber = 1;
            ExportSearchResponse exportResponse = await advancedSearchService.ExportSearchToCSVAsync(new ExportSearchRequest()
            {
                SearchViewModel = advancedSearchViewModel,
                FileType = "CSV",
                DateFormat = this.Identity.ToDateFormat(),
                TimeZone = this.Identity.ToTimeZone()
            });
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);
            Logger.Current.Informational("Did temporary file stored in cache : " + result.ToString());
            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = exportResponse.FileName
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Exports to excel file.
        /// </summary>
        /// <param name="advancedSearchViewModel">The advanced search view model.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> ExportToExcelFile(AdvancedSearchViewModel advancedSearchViewModel)
        {
            if (advancedSearchViewModel.AccountID == 0)
                advancedSearchViewModel.AccountID = this.Identity.ToAccountID();
            advancedSearchViewModel.PageNumber = 1;
            ExportSearchResponse exportResponse = await advancedSearchService.ExportSearchToExcelAsync(new ExportSearchRequest()
            {
                SearchViewModel = advancedSearchViewModel,
                FileType = "Excel",
                DateFormat = this.Identity.ToDateFormat(),
                TimeZone = this.Identity.ToTimeZone()
            });
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);
            Logger.Current.Informational("Did temporary file stored in cache : " + result.ToString());
            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = exportResponse.FileName
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Exports to PDF file.
        /// </summary>
        /// <param name="advancedSearchViewModel">The advanced search view model.</param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public async Task<ActionResult> ExportToPDFFile(AdvancedSearchViewModel advancedSearchViewModel)
        {
            if (advancedSearchViewModel.AccountID == 0)
                advancedSearchViewModel.AccountID = this.Identity.ToAccountID();
            advancedSearchViewModel.PageNumber = 1;
            ExportSearchResponse exportResponse = await advancedSearchService.ExportSearchToPDFAsync(new ExportSearchRequest()
            {
                SearchViewModel = advancedSearchViewModel,
                FileType = "PDF",
                DateFormat = this.Identity.ToDateFormat(),
                TimeZone = this.Identity.ToTimeZone()
            });
            string fileKey = Guid.NewGuid().ToString();
            bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);
            Logger.Current.Informational("Did temporary file stored in cache : " + result.ToString());
            return Json(new
            {
                success = true,
                fileKey = fileKey,
                fileName = exportResponse.FileName
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="fileKey">The file key.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        public ActionResult DownloadFile(string fileKey, string fileName)
        {
            byte[] file = cachingService.GetTemporaryFile(fileKey);
            return File(file, @"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", fileName);
        }

        /// <summary>
        /// Gets the search qualifiers.
        /// </summary>
        /// <param name="fieldId">The field identifier.</param>
        /// <param name="contactDropdown">The contact dropdown.</param>
        /// <returns></returns>
        [HttpGet]
        public JsonResult GetSearchQualifiers(int fieldId, int? contactDropdown)
        {
            var accountId = this.Identity.ToAccountID();
            GetSearchValueOptionsResponse response = advancedSearchService.GetSearchValueOptions(new GetSearchValueOptionsRequest()
            {
                FieldId = fieldId,
                AccountId = accountId,
                ContactDropdownId = contactDropdown,
                RoleId = this.Identity.ToRoleID(),
                RequestedBy = this.Identity.ToUserID(),
                IsSTAdmin = this.Identity.IsSTAdmin()
            });
            return Json(new
            {
                success = true,
                response = response.FieldValueOptions
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Quicks the search.
        /// </summary>
        /// <param name="quickSearchViewModel">The quick search view model.</param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult QuickSearch(QuickSearchViewModel quickSearchViewModel)
        {
            var accountId = this.Identity.ToAccountID();
            var userId = this.Identity.ToUserID();
            var roleId = this.Identity.ToRoleID();
            IEnumerable<SearchableEntity> entities = null;
            if (quickSearchViewModel.SearchableEntities != null && quickSearchViewModel.SearchableEntities.Any())
                entities = quickSearchViewModel.SearchableEntities.Select(s => (SearchableEntity)s);
            QuickSearchResponse response = advancedSearchService.QuickSearch(new QuickSearchRequest()
            {
                Query = quickSearchViewModel.Query,
                SearchableEntities = entities,
                AccountId = accountId,
                PageNumber = quickSearchViewModel.PageNumber,
                Limit = quickSearchViewModel.Limit,
                RequestedBy = userId,
                RoleId = roleId
            });
            return new JsonResult
            {
                Data = response,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}
