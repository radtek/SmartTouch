using AutoMapper;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
//using Newtonsoft.Json;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class AdvancedSearchService : IAdvancedSearchService
    {

        readonly IAdvancedSearchRepository advancedSearchRepository;
        readonly IUnitOfWork unitOfWork;
        ISearchService<Contact> searchService;
        readonly ICachingService cachingService;
        readonly IContactService contactService;
        readonly ICustomFieldService customFieldService;
        readonly IAccountService accountService;
        readonly ITagRepository tagRepository;
        readonly IWorkflowRepository workflowRepository;

        IGeoService geoService;
        IContactRepository contactRepository;
        IIndexingService indexingService;

        public AdvancedSearchService(IAdvancedSearchRepository advancedSearchRepository, ICachingService cachingService,
                                   IUnitOfWork unitOfWork, ISearchService<Contact> searchService, IGeoService geoService,
            IContactRepository contactRepository, IContactService contactService, ICustomFieldService customFieldService,
            IAccountService accountService, ITagRepository tagRepository, IIndexingService indexingService, IWorkflowRepository workflowRepository)
        {
            this.advancedSearchRepository = advancedSearchRepository;
            this.unitOfWork = unitOfWork;
            this.cachingService = cachingService;
            this.searchService = searchService;
            this.tagRepository = tagRepository;
            this.geoService = geoService;
            this.contactService = contactService;
            this.contactRepository = contactRepository;
            this.customFieldService = customFieldService;
            this.accountService = accountService;
            this.indexingService = indexingService;
            this.workflowRepository = workflowRepository;
        }

        public QuickSearchResponse QuickSearch(QuickSearchRequest request)
        {
            Logger.Current.Verbose("Request received from QuickSearch.");

            ISearchService<EntityBase<int>> searchServiceEntity = new SearchService<EntityBase<int>>();
            SearchParameters parameters = new SearchParameters();
            parameters.AccountId = request.AccountId;
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber;

            if (request.SearchableEntities != null && request.SearchableEntities.Any())
                parameters.Types = getSearchableTypes(request.SearchableEntities);
            else
            {
                var usersPermissions = cachingService.GetUserPermissions(request.AccountId);
                var accountPermissions = cachingService.GetAccountPermissions(request.AccountId);

                var userModules = usersPermissions.Where(s => s.RoleId == request.RoleId &&
                                  accountPermissions.Contains(s.ModuleId)).Select(r => r.ModuleId).ToList();

                List<Type> types = new List<Type>();
                if (userModules.Contains(3))
                {
                    types.Add(typeof(Contact));
                    types.Add(typeof(Person));
                }
                if (userModules.Contains(4))
                {
                    //  types.Add(typeof(Campaign)); commented by Ram on 2nd May 2018 CR NEXG-3001
                }
                if (userModules.Contains(16))
                {
                    // types.Add(typeof(Opportunity)); commented by Ram on 2nd May 2018 CR NEXG-3001
                }
                if (userModules.Contains(9))
                {
                    types.Add(typeof(Tag));
                }
                parameters.Types = types;
            }

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isContactsPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            //bool isCampaignsPrivate = cachingService.IsModulePrivate(AppModules.Campaigns, request.AccountId); commented by Ram on 2nd May 2018 CR NEXG-3001
            bool isFormsPrivate = cachingService.IsModulePrivate(AppModules.Forms, request.AccountId);
            //bool isOpportunitiesPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, request.AccountId); commented by Ram on 2nd May 2018 CR NEXG-3001

            IList<AppModules> privateModules = new List<AppModules>();

            if (isContactsPrivate)
                privateModules.Add(AppModules.Contacts);
            //if (isCampaignsPrivate)commented by Ram on 2nd May 2018 CR NEXG-3001
            //privateModules.Add(AppModules.Campaigns); commented by Ram on 2nd May 2018 CR NEXG-3001
            //if (isOpportunitiesPrivate)commented by Ram on 2nd May 2018 CR NEXG-3001
            //privateModules.Add(AppModules.Opportunity);commented by Ram on 2nd May 2018 CR NEXG-3001
            if (isFormsPrivate)
                privateModules.Add(AppModules.Forms);

            if (privateModules.Count > 0 && !isAccountAdmin)
            {
                int? userId = request.RequestedBy;
                parameters.IsPrivateSearch = true;
                parameters.PrivateModules = privateModules;
                parameters.DocumentOwnerId = userId;
            }
            else
            {
                parameters.IsPrivateSearch = false;
            }

            var results = searchServiceEntity.QuickSearch(request.Query, parameters);

            return new QuickSearchResponse() { SearchResult = results };
        }

        IEnumerable<Type> getSearchableTypes(IEnumerable<SearchableEntity> searchableEntities)
        {
            IList<Type> types = new List<Type>();
            foreach (SearchableEntity entity in searchableEntities)
            {
                if (entity == SearchableEntity.People)
                {
                    types.Add(typeof(Contact));
                    types.Add(typeof(Person));

                }
                else if (entity == SearchableEntity.Companies)
                {
                    types.Add(typeof(Contact));
                    types.Add(typeof(Company));
                }
                else if (entity == SearchableEntity.Tags)
                    types.Add(typeof(Tag));
                //else if (entity == SearchableEntity.Campaigns) commented by Ram on 2nd May 2018 CR NEXG-3001
                //    types.Add(typeof(Campaign)); commented by Ram on 2nd May 2018 CR NEXG-3001
                //else if (entity == SearchableEntity.Opportunities) commented by Ram on 2nd May 2018 CR NEXG-3001
                //    types.Add(typeof(Opportunity)); commented by Ram on 2nd May 2018 CR NEXG-3001
            }
            return types;
        }

        public SaveAdvancedSearchResponse InsertSavedSearch(SaveAdvancedSearchRequest request)
        {
            Logger.Current.Verbose("Request received for inserting saved search.");
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.AdvancedSearchViewModel);
            searchDefinition.SelectAllSearch = false;

            bool isSearchNameUnique = advancedSearchRepository.IsSearchNameUnique(searchDefinition);
            if (!isSearchNameUnique)
            {
                var message = "[|Search with name|] \"" + searchDefinition.Name + "\" [|already exists.|]" + "[|Please choose a different name.|]";
                throw new UnsupportedOperationException(message);
            }
            if (searchDefinition.IsPreConfiguredSearch == true)
            {
                searchDefinition.Id = 0;
                searchDefinition.IsPreConfiguredSearch = false;
                searchDefinition.IsFavoriteSearch = false;
                IEnumerable<SearchFilter> searchFilters = searchDefinition.Filters;
                foreach (SearchFilter searchfilter in searchFilters)
                {
                    searchfilter.SearchFilterId = 0;
                    searchfilter.SearchDefinitionID = 0;
                }
            }
            isValidSearchDefinition(searchDefinition);

            advancedSearchRepository.Insert(searchDefinition);
            SearchDefinition newSavedSearch = unitOfWork.Commit() as SearchDefinition;

            advancedSearchRepository.InsertSmartSearchQueue(newSavedSearch.Id, request.AdvancedSearchViewModel.AccountID);
            Logger.Current.Informational("Saved search inserted successfully");
            if (searchDefinition.TagsList != null)
            {
                foreach (Tag tag in searchDefinition.TagsList)
                {
                    if (tag != null && tag.Id == 0)
                    {
                        Tag savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                        indexingService.IndexTag(savedTag);
                        accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                }
            }

            saveAdvancedSearchQuery(newSavedSearch, request.RoleId, request.AccountId, request.RequestedBy.Value);

            return new SaveAdvancedSearchResponse() { AdvancedSearchViewModel = new AdvancedSearchViewModel() { SearchDefinitionID = (short)newSavedSearch.Id, SearchDefinitionName = newSavedSearch.Name } };
        }

        public SaveAdvancedSearchResponse InsertSavedSearchForSelectAll(SaveAdvancedSearchRequest request)
        {
            Logger.Current.Verbose("Request received for inserting select all saved search.");
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.AdvancedSearchViewModel);
            searchDefinition.SelectAllSearch = true;

            advancedSearchRepository.Insert(searchDefinition);
            SearchDefinition newSavedSearch = unitOfWork.Commit() as SearchDefinition;

            return new SaveAdvancedSearchResponse() { SearchDefiniationId = newSavedSearch.Id };
        }

        private void saveAdvancedSearchQuery(SearchDefinition searchDefinition, short roleId, int accountId, int requestedBy)
        {
            Logger.Current.Verbose("Request received for saving Advanced search query.");

            SearchParameters parameters = new SearchParameters();
            parameters.AccountId = accountId;

            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = requestedBy;
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = userId;
            }
            else
                parameters.IsPrivateSearch = false;

            searchService.SaveQuery(searchDefinition, parameters);
        }

        private int saveAdvancedSearchQueries(IEnumerable<SearchDefinition> searchDefinitions, int accountId)
        {
            Logger.Current.Verbose("Request received for indexing Saved Search queries.");
            return searchService.SaveQueries(searchDefinitions, new SearchParameters(), accountId);
        }

        public int IndexSavedSearches(int accountId)
        {
            int indexedCount = 0;
            IEnumerable<SearchDefinition> savedSearches = advancedSearchRepository.FindSearchDefinitions(accountId);
            if (savedSearches != null && savedSearches.Any())
                indexedCount = saveAdvancedSearchQueries(savedSearches, accountId);
            return indexedCount;
        }

        private void isValidSearchDefinition(SearchDefinition definition)
        {
            IEnumerable<BusinessRule> brokenRules = definition.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules.Distinct())
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public SaveAdvancedSearchResponse UpdateSavedSearch(SaveAdvancedSearchRequest request)
        {
            Logger.Current.Verbose("Request received for updating a saved search.");

            SearchDefinition savedSearch = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.AdvancedSearchViewModel);
            if (savedSearch != null)
                savedSearch.LastRunDate = advancedSearchRepository.GetLastRunDate(request.AdvancedSearchViewModel.SearchDefinitionID);

            bool isSearchNameUnique = advancedSearchRepository.IsSearchNameUnique(savedSearch);
            if (!isSearchNameUnique)
            {
                var message = "[|Search with name|] \"" + savedSearch.Name + "\" [|already exists. Please choose a different name.|]";
                throw new UnsupportedOperationException(message);
            }
            isValidSearchDefinition(savedSearch);

            savedSearch.SelectAllSearch = false;
            advancedSearchRepository.Update(savedSearch);

            Logger.Current.Informational("Saved search updated successfully");

            SearchDefinition updatedSearchDefination = unitOfWork.Commit() as SearchDefinition;

            advancedSearchRepository.InsertSmartSearchQueue(updatedSearchDefination.Id, request.AdvancedSearchViewModel.AccountID);

            if (updatedSearchDefination != null)
            {
                updatedSearchDefination.Filters = Mapper.Map<IEnumerable<SearchFilter>, IEnumerable<SearchFilter>>(savedSearch.Filters);
            }

            if (savedSearch.TagsList != null)
            {
                foreach (Tag tag in savedSearch.TagsList)
                {
                    if (tag != null && tag.Id == 0)
                    {
                        Tag savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                        indexingService.IndexTag(savedTag);
                        accountService.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                }
            }

            saveAdvancedSearchQuery(updatedSearchDefination, request.RoleId, request.AccountId, request.RequestedBy.Value);
            return new SaveAdvancedSearchResponse();
        }

        public GetSavedSearchesResponse GetAllSavedSearches(GetSavedSearchesRequest request)
        {
            GetSavedSearchesResponse response = new GetSavedSearchesResponse();
            IEnumerable<SearchDefinition> savedSearches = null;
            if (request.IsPredefinedSearch == false && request.IsFavoriteSearch == false)
                savedSearches = advancedSearchRepository.FindAll(request.Query, request.Limit, request.PageNumber, request.AccountID, request.RequestedBy.Value);
            else if (request.IsFavoriteSearch)
                savedSearches = advancedSearchRepository.FindAllFavoriteSearches(request.Limit, request.PageNumber, request.AccountID, request.RequestedBy.Value);
            else if (request.IsPredefinedSearch)
                savedSearches = advancedSearchRepository.FindAll(request.AccountID, request.RequestedBy.Value, request.IsPredefinedSearch, request.IsFavoriteSearch);
            if (savedSearches == null)
            {
                response.Exception = new UnsupportedOperationException("The requested user was not found.");
            }
            else
            {
                IEnumerable<AdvancedSearchViewModel> list = Mapper.Map<IEnumerable<SearchDefinition>, IEnumerable<AdvancedSearchViewModel>>(savedSearches);
                response.SearchResults = list;
                if (request.IsPredefinedSearch == false && request.IsFavoriteSearch == false)
                    response.TotalHits = list.IsAny() ? list.Select(s => s.TotalSearchsCount).FirstOrDefault() : 0;
                else if (request.IsFavoriteSearch)
                    response.TotalHits = list.IsAny() ? list.Select(s => s.TotalSearchsCount).FirstOrDefault() : 0;
                else if (request.IsPredefinedSearch)
                    response.TotalHits = list.IsAny() ? list.Select(s => s.TotalSearchsCount).FirstOrDefault() : 0; ;
            }
            return response;
        }

        public SearchDefinition GetSavedSearch(GetSearchRequest request)
        {
            return advancedSearchRepository.FindBy(request.SearchDefinitionID);
        }

        public async Task<GetSearchResponse> GetSavedSearchAsync(GetSearchRequest request)
        {
            GetSearchResponse response = new GetSearchResponse();
            Logger.Current.Verbose("Getting saved search");
            SearchDefinition searchDefinition = advancedSearchRepository.FindBy(request.SearchDefinitionID);
            if (searchDefinition.AccountID == null)
                searchDefinition.AccountID = request.AccountId;
            if (searchDefinition == null)
            {
                Logger.Current.Error("The search does not exist");
                response.Exception = new UnsupportedOperationException("The search does not exist");
            }
            else
            {
                GetAdvanceSearchFieldsResponse searchFieldsResponse = GetSearchFields(new GetAdvanceSearchFieldsRequest() { accountId = request.AccountId, RoleId = request.RoleId });
                if (searchDefinition.Filters != null && searchFieldsResponse != null && searchFieldsResponse.FieldsViewModel != null)
                {
                    foreach (var filter in searchDefinition.Filters)
                    {
                        var field = searchFieldsResponse.FieldsViewModel.Where(s => s.FieldId == (int)filter.Field && s.IsDropdownField == filter.IsDropdownField);
                        if (filter.IsDropdownField)
                            field = searchFieldsResponse.FieldsViewModel.Where(s => s.FieldId == filter.DropdownValueId && s.IsDropdownField == filter.IsDropdownField && s.IsCustomField == filter.IsCustomField);
                        int? accountid = field.Select(s => s.AccountID).FirstOrDefault();
                        filter.IsCustomField = (accountid.HasValue && !filter.IsDropdownField) ? true : false;
                        //short? inputId = field.Select(s => (short)s.FieldInputTypeId).FirstOrDefault();
                        filter.FieldOptionTypeId = field.Select(s => (byte)s.FieldInputTypeId).FirstOrDefault();
                        if (filter.IsDropdownField && filter.DropdownValueId.HasValue)
                            filter.DropdownId = searchFieldsResponse.FieldsViewModel.Where(w => w.FieldId == filter.DropdownValueId && w.IsDropdownField == filter.IsDropdownField)
                                .Select(s => s.DropdownId).FirstOrDefault();
                    }
                }
                AdvancedSearchViewModel searchViewModel = Mapper.Map<SearchDefinition, AdvancedSearchViewModel>(searchDefinition);
                if (searchFieldsResponse != null && searchFieldsResponse.FieldsViewModel != null)
                    searchViewModel.SearchFields = searchFieldsResponse.FieldsViewModel;
                if (searchDefinition.PageNumber == 0)
                    searchDefinition.PageNumber = 1;
                response.SearchViewModel = searchViewModel;
                if (searchViewModel != null)
                {
                    Logger.Current.Verbose("Conversion from SearchDefinition to AdvancedSearchViewModel is successful");
                    Logger.Current.Verbose("Request for fetching search-fields for accountId : " + request.AccountId);
                    Logger.Current.Verbose("Fetching value-options for required fields in edit mode");
                    var searchFilterFieldIds = searchViewModel.SearchFilters.Select(sf => sf.FieldId);
                    var valueOptionRequiredFields = searchViewModel.SearchFields.Where(sf => searchFilterFieldIds.Contains(sf.FieldId) &&
                                                                (sf.FieldInputTypeId == FieldType.checkbox || sf.FieldInputTypeId == FieldType.radio ||
                                                                 sf.FieldInputTypeId == FieldType.dropdown || sf.FieldInputTypeId == FieldType.multiselectdropdown));//.Select(s => s.FieldId)

                    if (valueOptionRequiredFields != null)
                    {
                        Logger.Current.Verbose("value-options are required for the above search definition and the count is :" + valueOptionRequiredFields.Count());
                        foreach (var field in valueOptionRequiredFields)
                        {
                            int? contactDropdown = null;
                            GetSearchValueOptionsResponse searchValueOptionsResponse = new GetSearchValueOptionsResponse();
                            if (field.FieldId == (int)ContactFields.PartnerTypeField)
                                contactDropdown = (byte)DropdownFieldTypes.PartnerType;
                            else if (field.FieldId == (int)ContactFields.LifecycleStageField)
                                contactDropdown = (byte)DropdownFieldTypes.LifeCycle;
                            else if (field.FieldId == (int)ContactFields.LeadSource || field.FieldId == (int)ContactFields.FirstLeadSource)
                                contactDropdown = (byte)DropdownFieldTypes.LeadSources;
                            else if (field.FieldId == (int)ContactFields.TourType)
                                contactDropdown = (byte)DropdownFieldTypes.TourType;
                            else if (field.FieldId == (int)ContactFields.Community)
                                contactDropdown = (byte)DropdownFieldTypes.Community;
                            else if (field.FieldId == (int)ContactFields.NoteCategory || field.FieldId == (int)ContactFields.LastNoteCategory)
                                contactDropdown = (byte)DropdownFieldTypes.NoteCategory;
                            else if (field.FieldId == (int)ContactFields.ActionType)
                                contactDropdown = (byte)DropdownFieldTypes.ActionType;

                            if (field.FieldId == (int)ContactFields.DonotEmail)
                                searchValueOptionsResponse.FieldValueOptions = getDoNotEmailValueOptions();
                            else if (field.FieldId == (int)ContactFields.LastTouchedThrough)
                                searchValueOptionsResponse.FieldValueOptions = getLastTouchedThroughValueOptions();
                            else if (field.FieldId == (int)ContactFields.EmailStatus)
                                searchValueOptionsResponse.FieldValueOptions = getEmailStatusValueOptions();
                            else
                                searchValueOptionsResponse = GetSearchValueOptions(new GetSearchValueOptionsRequest() { AccountId = request.AccountId, ContactDropdownId = contactDropdown, FieldId = field.FieldId, IsSTAdmin = request.IsSTAdmin });

                            var searchFilter = searchViewModel.SearchFilters.Where(s => s.FieldId == field.FieldId && s.IsDropdownField == field.IsDropdownField);
                            foreach (var filter in searchFilter)
                            {
                                filter.ValueOptions = searchValueOptionsResponse.FieldValueOptions;
                                filter.InputTypeId = (byte)searchFieldsResponse.FieldsViewModel.Where(w => w.FieldId == field.FieldId).Select(s => s.FieldInputTypeId).FirstOrDefault();
                            }
                        }
                    }
                }

                if (request.IncludeSearchResults)
                {
                    Logger.Current.Verbose("Serach results need to be included");
                    SearchParameters parameters = new SearchParameters();
                    parameters.Limit = request.Limit;
                    bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
                    bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
                    if (isPrivate && !isAccountAdmin && !request.IsAutomationRequest)
                    {
                        int userId = request.RequestedBy.HasValue ? (int)request.RequestedBy : 0;
                        parameters.IsPrivateSearch = true;
                        parameters.DocumentOwnerId = userId;
                    }
                    else
                        parameters.IsPrivateSearch = false;
                    searchViewModel.SearchResult = await runAdvancedSearchAsync(request.Query, searchDefinition, parameters);
                    Logger.Current.Verbose("Run search was successful");
                }
            }
            return response;
        }

        private List<FieldValueOption> getLastTouchedThroughValueOptions()
        {
            List<FieldValueOption> lastTouchedThroughOptions = new List<FieldValueOption>();
            FieldValueOption lastTouchedOption1 = new FieldValueOption();
            lastTouchedOption1.Id = 4; lastTouchedOption1.Value = "Campaign";
            FieldValueOption lastTouchedOption2 = new FieldValueOption();
            lastTouchedOption2.Id = 26; lastTouchedOption2.Value = "Send Text";
            FieldValueOption lastTouchedOption3 = new FieldValueOption();
            lastTouchedOption3.Id = 25; lastTouchedOption3.Value = "Send Mail";
            FieldValueOption lastTouchedOption4 = new FieldValueOption();
            lastTouchedOption4.Id = 46; lastTouchedOption4.Value = "Phone Call";
            FieldValueOption lastTouchedOption5 = new FieldValueOption();
            lastTouchedOption5.Id = 47; lastTouchedOption5.Value = "Email";
            FieldValueOption lastTouchedOption6 = new FieldValueOption();
            lastTouchedOption6.Id = 48; lastTouchedOption6.Value = "Appointment";
            FieldValueOption lastTouchedOption7 = new FieldValueOption();
            lastTouchedOption7.Id = 3; lastTouchedOption7.Value = "Action-Other";

            lastTouchedThroughOptions.Add(lastTouchedOption1);
            lastTouchedThroughOptions.Add(lastTouchedOption2);
            lastTouchedThroughOptions.Add(lastTouchedOption3);
            lastTouchedThroughOptions.Add(lastTouchedOption4);
            lastTouchedThroughOptions.Add(lastTouchedOption5);
            lastTouchedThroughOptions.Add(lastTouchedOption6);
            lastTouchedThroughOptions.Add(lastTouchedOption7);

            return lastTouchedThroughOptions;
        }

        private List<FieldValueOption> getDoNotEmailValueOptions()
        {
            List<FieldValueOption> doNotEmailOptions = new List<FieldValueOption>();
            FieldValueOption emailOptionNo = new FieldValueOption();
            emailOptionNo.Id = 0; emailOptionNo.Value = "No";
            FieldValueOption emailOptionYes = new FieldValueOption();
            emailOptionYes.Id = 1; emailOptionYes.Value = "Yes";
            doNotEmailOptions.Add(emailOptionNo);
            doNotEmailOptions.Add(emailOptionYes);

            return doNotEmailOptions;
        }

        private List<FieldValueOption> getEmailStatusValueOptions()
        {
            List<FieldValueOption> emailStatusOptions = new List<FieldValueOption>();
            emailStatusOptions.Add(new FieldValueOption() { Id = 50, Value = "Not Verified" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 51, Value = "Verified" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 52, Value = "Soft Bounce" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 53, Value = "Hard Bounce" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 54, Value = "Unsubscribed" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 56, Value = "Complained" });
            emailStatusOptions.Add(new FieldValueOption() { Id = 57, Value = "Suppressed" });

            return emailStatusOptions;
        }

        public async Task<List<int>> GetSavedSearchContactIds(GetSavedSearchContactIdsRequest request)
        {
            SearchDefinition searchDefinition = null;
            List<int> contactIds = new List<int>();
            if (request.SearchDefinitionId != 0)
            {
                var result = await this.GetSavedSearchAsync(new GetSearchRequest() { SearchDefinitionID = Convert.ToInt16(request.SearchDefinitionId), Limit = 200000, AccountId = request.AccountId });
                AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
                if (result != null)
                    viewModel = result.SearchViewModel;
                searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(viewModel);
                SearchParameters parameters = new SearchParameters();
                parameters.PageNumber = 1;
                parameters.Limit = 500000;
                parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.IsActive };
                bool isAccountAdmin = request.RoleId == 1 ? true : cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
                Logger.Current.Informational("Is Account Admin : " + isAccountAdmin.ToString());
                Logger.Current.Informational("Is Contacts in private mode: " + isPrivate.ToString());
                Logger.Current.Informational("RoleId: " + request.RoleId);
                Logger.Current.Informational("AccountID: " + request.AccountId);

                if (isPrivate && !isAccountAdmin)
                {
                    parameters.IsPrivateSearch = true;
                    parameters.DocumentOwnerId = request.RequestedBy;
                }
                else
                    parameters.IsPrivateSearch = false;
                var SearchResult = await searchService.AdvancedSearchAsync(null, searchDefinition, parameters);
                if (SearchResult != null)
                    return SearchResult.Results.Select(s => s.Id).ToList();
                else
                    return new List<int>();
            }
            else if (request.SearchDefinitionIds != null && request.SearchDefinitionIds.Any())
            {
                foreach (var id in request.SearchDefinitionIds)
                {
                    var result = await this.GetSavedSearchAsync(new GetSearchRequest() { SearchDefinitionID = id, Limit = 200000, AccountId = request.AccountId });
                    AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
                    if (result != null)
                        viewModel = result.SearchViewModel;
                    searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(viewModel);
                    SearchParameters parameters = new SearchParameters();
                    parameters.PageNumber = 1;
                    parameters.Limit = 500000;
                    parameters.Fields = new List<ContactFields>() { ContactFields.ContactId };
                    var SearchResult = await searchService.AdvancedSearchAsync(null, searchDefinition, parameters);
                    if (SearchResult != null)
                        contactIds.AddRange(SearchResult.Results.Select(s => s.Id));
                }
                return contactIds;
            }
            else
                return contactIds;
        }

        public async Task<List<Person>> GetActiveContactIds(GetSavedSearchContactIdsRequest request)
        {
            SearchDefinition searchDefinition = null;
            List<int> contactIds = new List<int>();
            if (request.SearchDefinitionId != 0)
            {
                var result = await this.GetSavedSearchAsync(new GetSearchRequest() { SearchDefinitionID = Convert.ToInt16(request.SearchDefinitionId), Limit = 200000, AccountId = request.AccountId });
                AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
                if (result != null)
                    viewModel = result.SearchViewModel;
                searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(viewModel);
                SearchParameters parameters = new SearchParameters();
                parameters.PageNumber = 1;
                parameters.Limit = 500000;
                parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.IsActive };
                bool isAccountAdmin = request.RoleId == 1 ? true : cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
                Logger.Current.Informational("Is Account Admin : " + isAccountAdmin.ToString());
                Logger.Current.Informational("Is Contacts in private mode: " + isPrivate.ToString());
                Logger.Current.Informational("RoleId: " + request.RoleId);
                Logger.Current.Informational("AccountID: " + request.AccountId);

                if (isPrivate && !isAccountAdmin)
                {
                    parameters.IsPrivateSearch = true;
                    parameters.DocumentOwnerId = request.RequestedBy;
                }
                else
                    parameters.IsPrivateSearch = false;
                parameters.IsActiveContactsSearch = true;
                var SearchResult = await searchService.AdvancedSearchAsync(null, searchDefinition, parameters);
                if (SearchResult != null)
                    return SearchResult.Results.Select(s => (Person)s).ToList();
                else
                    return new List<Person>();
            }
            else
                return new List<Person>();
        }

        public async Task<List<Contact>> GetContactEmails(GetContactEmailsRequest request)
        {
            SearchDefinition searchDefinition = null;
            if (request.SearchDefinitionID != 0)
            {
                var result = await this.GetSavedSearchAsync(new GetSearchRequest() { SearchDefinitionID = Convert.ToInt16(request.SearchDefinitionID), Limit = 200000, AccountId = request.AccountId });
                AdvancedSearchViewModel viewModel = new AdvancedSearchViewModel();
                if (result != null)
                    viewModel = result.SearchViewModel;
                searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(viewModel);
                SearchParameters parameters = new SearchParameters();
                parameters.PageNumber = 1;
                parameters.Limit = 500000;
                parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.PrimaryEmail, ContactFields.ContactEmailID, ContactFields.IsPrimaryEmail };
                parameters.IsPrivateSearch = false;
                var SearchResult = await searchService.AdvancedSearchAsync(null, searchDefinition, parameters);
                if (SearchResult != null)
                    return SearchResult.Results.ToList();
                else
                    return new List<Contact>();
            }
            else
                return new List<Contact>();
        }

        public async Task<GetSearchResponse> GetSavedSearchWithEmailFilterAppendedAsync(GetSearchRequest request)
        {
            GetSearchResponse response = new GetSearchResponse();
            SearchDefinition searchDefinition = advancedSearchRepository.FindBy(request.SearchDefinitionID);
            if (searchDefinition == null)
                throw new UnsupportedOperationException("The search does not exist");
            else
            {
                AdvancedSearchViewModel searchViewModel = Mapper.Map<SearchDefinition, AdvancedSearchViewModel>(searchDefinition);
                if (searchDefinition.PageNumber == 0)
                    searchDefinition.PageNumber = 1;
                response.SearchViewModel = searchViewModel;

                if (request.IncludeSearchResults)
                {
                    SearchParameters parameters = new SearchParameters();
                    parameters.Limit = request.Limit;
                    bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
                    bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
                    if (isPrivate && !isAccountAdmin)
                    {
                        int userId = (int)request.RequestedBy;
                        parameters.IsPrivateSearch = true;
                        parameters.DocumentOwnerId = userId;
                    }
                    else
                        parameters.IsPrivateSearch = false;
                    searchViewModel.SearchResult = await runSearchAsync<ContactListEntry>(searchDefinition, parameters);
                }
            }
            return response;
        }

        public DeleteSearchResponse DeleteSearches(DeleteSearchRequest request)
        {
            Logger.Current.Verbose("Request received for deleting saved searches");
            DeleteSearchResponse response = new DeleteSearchResponse();
            string message = advancedSearchRepository.DeleteSearches(request.SearchIDs);
            response.ResponseMessage = message;

            SearchParameters searchParameters = new SearchParameters();
            searchParameters.AccountId = request.AccountId;
            if (response.ResponseMessage == "")
            {
                foreach (var searchDefinitionId in request.SearchIDs)
                    searchService.RemoveQuery((short)searchDefinitionId, searchParameters);
            }
            else
                throw new UnsupportedOperationException(message);
            return response;
        }

        public async Task<AdvancedSearchResponse<T>> RunSearchAsync<T>(AdvancedSearchRequest<T> request) where T : IShallowContact
        {
            Logger.Current.Verbose("Request received for running advanced search.");
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);
            searchDefinition.PageNumber = (short)request.PageNumber;

            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";

            if (searchDefinition.Id > 0 && searchDefinition.Name != null)
                advancedSearchRepository.UpdateLastRunActivity(searchDefinition.Id, request.RequestedBy.Value, request.AccountId, searchDefinition.Name);
            isValidSearchDefinition(searchDefinition);

            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber;
            parameters.SortField = request.SortFieldType == ContactSortFieldType.NoSort ? ContactSortFieldType.RecentlyUpdatedContact : request.SortFieldType;
            parameters.SortDirection = request.SortDirection;
            parameters.Types = request.ContactTypes;
            if (request.IsResultsGrid && request.SortField != null)
            {
                List<string> sortFields = new List<string>();
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<ContactListEntry, Person>();

                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (propertyMap.SourceMember != null && request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        sortFields.Add(propertyMap.DestinationProperty.MemberInfo.Name);
                        break;
                    }
                }
                parameters.IsResultsGrid = request.IsResultsGrid;
                parameters.ResultsGridSortField = sortFields.FirstOrDefault();
                parameters.SortField = null;
            }

            if (request.Fields != null)
                parameters.Fields = request.Fields;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = userId;
            }
            else
                parameters.IsPrivateSearch = false;
            if (request.IsAdvancedSearch)
            {
                Logger.Current.Verbose("Request received for running advanced search");
                if (request.ViewContacts)
                {
                    //parameters.Fields = new List<ContactFields>() { ContactFields.ContactId };
                    //parameters.Limit = 100000;
                    if (request.ShowByCreated)
                    {
                        parameters.IsPrivateSearch = true;
                        parameters.DocumentOwnerId = (int)request.RequestedBy;
                    }
                    return new AdvancedSearchResponse<T>()
                    {
                        SearchResult = (dynamic)await runAdvancedSearchAsync(request.Query, searchDefinition, parameters)
                    };
                }
                return new AdvancedSearchResponse<T>()
                {
                    SearchResult = (dynamic)await runBasicAdvancedSearchAsync(searchDefinition, parameters)
                };
            }
            else
            {
                Logger.Current.Verbose("Request received for running a search");
                return new AdvancedSearchResponse<T>()
                {
                    SearchResult = (dynamic)await runSearchAsync<T>(searchDefinition, parameters)
                };
            }
        }

        public async Task<AdvancedSearchResponse<T>> ViewContactsAsync<T>(AdvancedSearchRequest<T> request) where T : IShallowContact
        {
            Logger.Current.Verbose("Request received for viewing contacts ");
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);

            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";

            isValidSearchDefinition(searchDefinition);

            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;

            if (request.Fields != null)
                parameters.Fields = request.Fields;

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                parameters.IsPrivateSearch = true;
                parameters.DocumentOwnerId = userId;
            }
            else
                parameters.IsPrivateSearch = false;
            if (request.ViewContacts)
            {
                parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.IsActive };
                parameters.Limit = 500000;
                var contactIds = await viewContacts(searchDefinition, parameters);
                return new AdvancedSearchResponse<T>()
                {
                    ContactIds = (contactIds.Results != null && contactIds.Results.Any()) ? contactIds.Results.Select(s => s.ContactID) : new List<int>()
                };
            }
            else
            {
                var contactIds = await viewContacts(searchDefinition, parameters);
                return new AdvancedSearchResponse<T>()
                {
                    ContactIds = (contactIds.Results != null && contactIds.Results.Any()) ? contactIds.Results.Select(s => s.ContactID) : new List<int>()
                };
            }
        }

        private async Task<SearchResult<T>> runSearchAsync<T>(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            parameters.PageNumber = searchDefinition.PageNumber;
            //parameters.Limit = parameters.Limit;


            searchDefinition = setDynamicDropdownTypes(searchDefinition);
            var result = await searchService.AdvancedSearchAsync(string.Empty, searchDefinition, parameters);
            //if (searchDefinition.Id > 0)
            //{
            //    advancedSearchRepository.UpdateLastRunDate(searchDefinition.Id);
            //}
            SearchResult<T> searchResult = new SearchResult<T>();
            searchResult.TotalHits = result.TotalHits;

            searchResult.Results = Mapper.Map<IEnumerable<Contact>, IEnumerable<T>>(result.Results);
            return searchResult;
        }

        private async Task<SearchResult<ContactListEntry>> runAdvancedSearchAsync(string query, SearchDefinition searchDefinition, SearchParameters parameters)
        {
            Logger.Current.Verbose("Request for Run advanced search");
            parameters.PageNumber = searchDefinition.PageNumber;
            //parameters.Limit = parameters.Limit;

            searchDefinition = setDynamicDropdownTypes(searchDefinition);
            var result = await searchService.AdvancedSearchAsync(query, searchDefinition, parameters);

            IEnumerable<int?> OwnerIds = result.Results.Select(s => new List<int?>() { s.OwnerId, s.CreatedBy }).SelectMany(i => i);
            IEnumerable<Owner> Owners = contactRepository.GetUserNames(OwnerIds);

            SearchResult<ContactListEntry> searchResult = new SearchResult<ContactListEntry>();
            searchResult.TotalHits = result.TotalHits;
            Logger.Current.Verbose("Advanced search results total :" + result.TotalHits);

            var listEntries = await ConvertToListEntry(result.Results);
            //searchResult.Results = Mapper.Map<IEnumerable<Contact>, IEnumerable<ContactListEntry>>(result.Results);

            var dropdowns = cachingService.GetDropdownValues(searchDefinition.AccountID);
            var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                      Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var partnerTypes = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            searchResult.Results = await ManageDropdowns(listEntries, lifecycleStages, partnerTypes, leadSources, Owners);

            return searchResult;
        }

        private async Task<SearchResult<ContactAdvancedSearchEntry>> runBasicAdvancedSearchAsync(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            Logger.Current.Verbose("Request for Run advanced search");
            try
            {
                parameters.PageNumber = searchDefinition.PageNumber;
                //parameters.Limit = parameters.Limit;
                parameters.Fields = new List<ContactFields> { ContactFields.FirstNameField, ContactFields.LastNameField, ContactFields.ContactId, ContactFields.CompanyNameField, 
                ContactFields.PrimaryEmail, ContactFields.DonotEmail, ContactFields.PrimaryEmailStatus, ContactFields.IsPrimaryEmail, ContactFields.CompanyId};

                var result = await searchService.AdvancedSearchAsync(string.Empty, searchDefinition, parameters);
                SearchResult<ContactAdvancedSearchEntry> searchResult = new SearchResult<ContactAdvancedSearchEntry>();
                searchResult.TotalHits = result.TotalHits;
                Logger.Current.Verbose("Advanced search results total :" + result.TotalHits);

                searchResult.Results = Mapper.Map<IEnumerable<Contact>, IEnumerable<ContactAdvancedSearchEntry>>(result.Results);
                return searchResult;
            }
            catch (KeyNotFoundException keyEx)
            {
                Logger.Current.Error("Few fields were deleted or made in-active: " + searchDefinition.Id, keyEx);
                throw new Exception("[|Some fields are made inactive/deleted in your search criteria. Please re-create your search and try again.|]");
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while executing search with searchdefinitionid : " + searchDefinition.Id, ex);
                throw ex;
            }
        }

        private async Task<SearchResult<ViewContactEntry>> viewContacts(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            Logger.Current.Verbose("Request for Run advanced search");
            parameters.PageNumber = searchDefinition.PageNumber;
            //parameters.Limit = parameters.Limit;

            searchDefinition = setDynamicDropdownTypes(searchDefinition);
            var result = await searchService.AdvancedSearchAsync(string.Empty, searchDefinition, parameters);
            SearchResult<ViewContactEntry> searchResult = new SearchResult<ViewContactEntry>();
            searchResult.Results = Mapper.Map<IEnumerable<Contact>, IEnumerable<ViewContactEntry>>(result.Results);
            searchResult.TotalHits = result.TotalHits;
            return searchResult;
        }

        private void getContactIds(SearchDefinition searchDefinition, SearchParameters parameters)
        {
            Logger.Current.Verbose("Request for fetching contactIds");
            parameters.PageNumber = searchDefinition.PageNumber;
            //parameters.Limit = parameters.Limit;

            searchDefinition = setDynamicDropdownTypes(searchDefinition);
        }

        private SearchDefinition setDynamicDropdownTypes(SearchDefinition searchDefinition)
        {
            foreach (var filter in searchDefinition.Filters)
            {
                if (filter.Field == ContactFields.WorkPhoneField)
                {
                    var field = getDropdownFieldValue(searchDefinition.AccountID.Value, DropdownFieldTypes.PhoneNumberType, DropdownValueTypes.WorkPhone);
                    filter.FieldOptionTypeId = field != null ? field.DropdownValueID : new Nullable<short>();
                }
                else if (filter.Field == ContactFields.HomePhoneField)
                {
                    var field = getDropdownFieldValue(searchDefinition.AccountID.Value, DropdownFieldTypes.PhoneNumberType, DropdownValueTypes.Homephone);
                    filter.FieldOptionTypeId = field != null ? field.DropdownValueID : new Nullable<short>();
                }
                else if (filter.Field == ContactFields.MobilePhoneField)
                {
                    var field = getDropdownFieldValue(searchDefinition.AccountID.Value, DropdownFieldTypes.PhoneNumberType, DropdownValueTypes.MobilePhone);
                    filter.FieldOptionTypeId = field != null ? field.DropdownValueID : new Nullable<short>();
                }
            }

            return searchDefinition;
        }

        private DropdownValueViewModel getDropdownFieldValue(int accountId, DropdownFieldTypes dropdownFieldType, DropdownValueTypes dropdownValueType)
        {
            var dropdownValues = cachingService.GetDropdownValues(accountId);
            var phoneDropdowns = dropdownValues.Where(d => d.DropdownID == (byte)dropdownFieldType).SelectMany(d => d.DropdownValuesList);
            var phoneField = phoneDropdowns.Where(d => d.DropdownValueTypeID == (byte)dropdownValueType).SingleOrDefault();
            return phoneField;
        }

        //public InsertBulkOperationResponse InsertBulkOperation(InsertBulkOperationRequest request)
        //{
        //    InsertBulkOperationResponse response = new InsertBulkOperationResponse();

        //    AdvancedSearchViewModel advancedSearchViewModel = JsonConvert.DeserializeObject<AdvancedSearchViewModel>(request.OperationData.AdvancedSearchCriteria);

        //    SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(advancedSearchViewModel);
        //    if (searchDefinition != null)
        //    {
        //        searchDefinition.AccountID = request.AccountId;
        //        searchDefinition.CreatedBy = (int)request.RequestedBy;
        //        searchDefinition.CreatedOn = request.CreatedOn;
        //        searchDefinition.SelectAllSearch = true;

        //        advancedSearchRepository.Insert(searchDefinition);
        //        SearchDefinition newSavedSearch = unitOfWork.Commit() as SearchDefinition;

        //        request.OperationData.SearchDefinitionID = newSavedSearch.Id;
        //    }

        //    request.OperationData.CreatedOn = request.CreatedOn;

        //    contactRepository.InsertBulkOperation(request.OperationData);

        //    return response;
        //}

        public async Task<ExportSearchResponse> ExportSearchAsync(ExportSearchRequest request)
        {
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);

            if (searchDefinition == null)
            {
                searchDefinition = new SearchDefinition();
                List<SearchFilter> filters = new List<SearchFilter>();

                SearchFilter enddate = new SearchFilter();
                enddate.Field = ContactFields.CreatedOn;
                enddate.Qualifier = SearchQualifier.IsNotEmpty;
                enddate.SearchText = "";
                enddate.IsDateTime = true;
                filters.Add(enddate);
                searchDefinition.CustomPredicateScript = " (" + filters.Count() + ") ";
                searchDefinition.AccountID = request.AccountId;
                searchDefinition.PredicateType = SearchPredicateType.And;
                searchDefinition.Filters = filters;
            }

            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            isValidSearchDefinition(searchDefinition);
            SearchResult<Contact> SearchResult = await searchService.AdvancedSearchExportAsync(string.Empty, searchDefinition, new SearchParameters() { PageNumber = 1 });

            if (request.SelectedContactIds != null && request.SelectedContactIds.Any())
                SearchResult.Results = SearchResult.Results.Where(m => request.SelectedContactIds.Contains(m.Id));

            IEnumerable<int> ownerRequiredFields = new List<int>() { (int)ContactFields.Owner, (int)ContactFields.CreatedBy };
            IEnumerable<int?> OwnerIds = new List<int?>();
            IEnumerable<Owner> Owners = new List<Owner>();
            if (request.SelectedFields.Select(s => ownerRequiredFields.Contains(s)).Any())
            {
                OwnerIds = SearchResult.Results.Select(s => s.OwnerId);
                Owners = contactRepository.GetUserNames(OwnerIds);
            }

            var selectedFields = request.SearchViewModel.SearchFields.Where(s => request.SelectedFields.Contains(s.FieldId) && s.FieldId != 42 && s.FieldId != 45 && s.FieldId != 46 && s.FieldId != 47 && s.FieldId != 48 && s.FieldId != 49
                && s.FieldId != 56 && s.FieldId != 57 && s.FieldId != 58 && s.FieldId != 60)
                .OrderBy(d => d.FieldId);

            DataTable dt = await contactService.GetDataTable(selectedFields.ToList(), searchDefinition.Fields,
                SearchResult.Results, request.SearchViewModel.AccountID, Owners, request.DownloadType, request.DateFormat, request.TimeZone);
            ExportSearchResponse response = new ExportSearchResponse();
            string searchDescription = this.GetSearchDefinitionDescription(new GetSearchDefinitionDescriptionRequest() { SearchDefinitionId = searchDefinition.Id }).Title;
            if (!string.IsNullOrEmpty(searchDescription))
                searchDescription = searchDescription.Replace("<b>", "").Replace("</b>", "");
            if (request.DownloadType == DownloadType.CSV)
            {
                ReadExcel exl = new ReadExcel();
                byte[] array = exl.ConvertDataSetToCSV(dt, searchDescription);
                response = new ExportSearchResponse() { byteArray = array, FileName = "Export.csv" };
            }
            else if (request.DownloadType == DownloadType.Excel)
            {
                ReadExcel exl = new ReadExcel();
                byte[] array = exl.ConvertDataSetToExcel(dt, searchDescription);
                response = new ExportSearchResponse() { byteArray = array, FileName = "Export.xlsx" };
            }
            else
            {
                ReadPDF pdf = new ReadPDF();
                byte[] array = pdf.ExportToPdf(dt, searchDescription);
                response = new ExportSearchResponse() { byteArray = array, FileName = "Export.pdf" };
            }
            return response;
        }

        public async Task<ExportSearchResponse> ExportSearchToCSVAsync(ExportSearchRequest request)
        {
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);
            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            isValidSearchDefinition(searchDefinition);
            SearchResult<Contact> SearchResult = await runSearchAsync<Contact>(searchDefinition, new SearchParameters() { Limit = 50000, PageNumber = 1 });

            IEnumerable<int> ownerRequiredFields = new List<int>() { (int)ContactFields.Owner, (int)ContactFields.CreatedBy };
            IEnumerable<int?> OwnerIds = new List<int?>();
            IEnumerable<Owner> Owners = new List<Owner>();
            if (request.SelectedFields.Select(s => ownerRequiredFields.Contains(s)).Any())
            {
                OwnerIds = SearchResult.Results.Select(s => s.OwnerId);
                Owners = contactRepository.GetUserNames(OwnerIds);
            }
            var selectedFields = request.SearchViewModel.SearchFields.Where(s => request.SelectedFields.Contains(s.FieldId));

            DataTable dt = await contactService.GetDataTable(selectedFields.ToList(), searchDefinition.Fields,
                SearchResult.Results, request.SearchViewModel.AccountID, Owners, request.DownloadType, request.DateFormat, request.TimeZone);
            ReadExcel exl = new ReadExcel();
            byte[] array = exl.ConvertDataSetToCSV(dt, string.Empty);
            ExportSearchResponse response = new ExportSearchResponse() { byteArray = array, FileName = "Export.csv" };
            return response;
        }

        public async Task<ExportSearchResponse> ExportSearchToExcelAsync(ExportSearchRequest request)
        {
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);
            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            isValidSearchDefinition(searchDefinition);
            SearchResult<Contact> SearchResult = await runSearchAsync<Contact>(searchDefinition, new SearchParameters() { Limit = 50000, PageNumber = 1 });
            IEnumerable<int> ownerRequiredFields = new List<int>() { (int)ContactFields.Owner, (int)ContactFields.CreatedBy };
            IEnumerable<int?> OwnerIds = new List<int?>();
            IEnumerable<Owner> Owners = new List<Owner>();
            if (request.SelectedFields.Select(s => ownerRequiredFields.Contains(s)).Any())
            {
                OwnerIds = SearchResult.Results.Select(s => s.OwnerId);
                Owners = contactRepository.GetUserNames(OwnerIds);
            }
            var selectedFields = request.SearchViewModel.SearchFields.Where(s => request.SelectedFields.Contains(s.FieldId));

            DataTable dt = await contactService.GetDataTable(selectedFields.ToList(), searchDefinition.Fields, SearchResult.Results,
                request.SearchViewModel.AccountID, Owners, request.DownloadType, request.DateFormat, request.TimeZone);
            ReadExcel exl = new ReadExcel();
            byte[] array = exl.ConvertDataSetToExcel(dt, string.Empty);
            ExportSearchResponse response = new ExportSearchResponse() { byteArray = array, FileName = "Export.xlsx" };
            return response;
        }

        public async Task<ExportSearchResponse> ExportSearchToPDFAsync(ExportSearchRequest request)
        {
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchViewModel);
            if (searchDefinition.Id == 0 && string.IsNullOrEmpty(searchDefinition.Name))
                searchDefinition.Name = "Adhoc Search";
            isValidSearchDefinition(searchDefinition);
            SearchResult<Contact> SearchResult = await runSearchAsync<Contact>(searchDefinition, new SearchParameters() { Limit = 50000, PageNumber = 1 });
            IEnumerable<int> ownerRequiredFields = new List<int>() { (int)ContactFields.Owner, (int)ContactFields.CreatedBy };
            IEnumerable<int?> OwnerIds = new List<int?>();
            IEnumerable<Owner> Owners = new List<Owner>();
            if (request.SelectedFields.Select(s => ownerRequiredFields.Contains(s)).Any())
            {
                OwnerIds = SearchResult.Results.Select(s => s.OwnerId);
                Owners = contactRepository.GetUserNames(OwnerIds);
            }
            var selectedFields = request.SearchViewModel.SearchFields.Where(s => request.SelectedFields.Contains(s.FieldId));

            DataTable dt = await contactService.GetDataTable(selectedFields.ToList(), searchDefinition.Fields,
                SearchResult.Results, request.SearchViewModel.AccountID, Owners, request.DownloadType, request.DateFormat, request.TimeZone);
            ReadPDF pdf = new ReadPDF();
            byte[] array = pdf.ExportToPdf(dt, string.Empty);
            ExportSearchResponse response = new ExportSearchResponse() { byteArray = array, FileName = "Export.pdf" };
            return response;
        }

        //public async Task<DataTable> GetDataTable(List<FieldViewModel> selectedColumns, IEnumerable<Field> searchFields,
        //    IEnumerable<Contact> Contacts, int accountId, IEnumerable<Owner> owners, DownloadType fileType, string dateFormat, string timeZone)
        //{
        //    var dropdowns = await cachingService.GetDropdownValuesAsync(accountId);
        //    var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
        //              Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
        //    var partnerTypes = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
        //    var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();

        //    var customeFieldValueList = new List<CustomFieldValueOptionViewModel>();
        //    if (selectedColumns.Any(a => a.FieldId > 200))       //Selected columns contains customfields
        //    {
        //        customeFieldValueList = customFieldService.GetCustomFieldValueOptions(new GetCustomFieldsValueOptionsRequest() { AccountId = accountId }).CustomFieldValueOptions.ToList();
        //    }

        //    var values = new object[selectedColumns.Count()];
        //    DataTable table = new DataTable();
        //    foreach (FieldViewModel selectedField in selectedColumns)
        //    {
        //        int value = table.Columns.IndexOf(selectedField.Title);
        //        if (value > 0) selectedField.Title = selectedField.Title + "(1)";
        //        table.Columns.Add(selectedField.Title);
        //    }

        //    foreach (var item in Contacts)
        //    {
        //        Person person = null;
        //        Company company = null;
        //        bool isPerson = false;
        //        if (item.GetType().Equals(typeof(Person)))
        //        {
        //            person = item as Person;
        //            isPerson = true;
        //        }
        //        else
        //            company = item as Company;

        //        var primaryAddress = item.Addresses != null ? item.Addresses.Any(a => a.IsDefault) ? item.Addresses.FirstOrDefault(a => a.IsDefault) : null : null;

        //        for (int i = 0; i < selectedColumns.Count(); i++)
        //        {
        //            var value = "";
        //            if (selectedColumns[i].FieldId == 1 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                    value = person.FirstName;
        //                else value = string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 2 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                    value = person.LastName;
        //                else value = string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 3 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (fileType == DownloadType.CSV)
        //                {
        //                    value = String.Format("\"{0}\"", isPerson ? person.CompanyName : company.CompanyName);
        //                }
        //                else
        //                {
        //                    value = isPerson ? person.CompanyName : company.CompanyName;
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 7 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                var primaryEmail = item.Emails != null ? item.Emails.Any(e => e.IsPrimary) ? item.Emails.FirstOrDefault(e => e.IsPrimary).EmailId : string.Empty : string.Empty;
        //                value = primaryEmail;
        //            }
        //            else if (selectedColumns[i].FieldId == 9 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.FacebookUrl != null ? item.FacebookUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 8 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = person != null ? person.Title : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 10 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.TwitterUrl != null ? item.TwitterUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 11 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.LinkedInUrl != null ? item.LinkedInUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 12 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.GooglePlusUrl != null ? item.GooglePlusUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 13 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.WebsiteUrl != null ? item.WebsiteUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 14 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.BlogUrl != null ? item.BlogUrl.URL : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 15 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    if (fileType == DownloadType.CSV)
        //                    {
        //                        value = String.Format("\"{0}\"", primaryAddress.AddressLine1);
        //                    }
        //                    else
        //                    {
        //                        value = primaryAddress.AddressLine1;
        //                    }
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 16 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    if (fileType == DownloadType.CSV)
        //                    {
        //                        value = String.Format("\"{0}\"", primaryAddress.AddressLine2);
        //                    }
        //                    else
        //                    {
        //                        value = primaryAddress.AddressLine2;
        //                    }
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 17 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    if (fileType == DownloadType.CSV)
        //                    {
        //                        value = String.Format("\"{0}\"", primaryAddress.City);
        //                    }
        //                    else
        //                    {
        //                        value = primaryAddress.City;
        //                    }
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 18 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    value = primaryAddress.State.Name;
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 19 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    value = primaryAddress.ZipCode;
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 20 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (primaryAddress != null)
        //                {
        //                    value = primaryAddress.Country.Name;
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 21 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                {
        //                    var partnerTypeName = partnerTypes.Where(e => e.DropdownValueID == person.PartnerType).Select(s => s.DropdownValue).FirstOrDefault();
        //                    value = partnerTypeName;
        //                }
        //                else
        //                    value = string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 22 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                var lifeCycleName = lifecycleStages.Where(e => e.DropdownValueID == item.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
        //                value = lifeCycleName;
        //            }
        //            else if (selectedColumns[i].FieldId == 23 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = item.DoNotEmail ? "Yes" : "No";
        //            }
        //            else if (selectedColumns[i].FieldId == 24 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                var leadSourceIds = item.LeadSources != null ? item.LeadSources.Where(w => !w.IsPrimary).Select(l => l.Id) : null;
        //                var leadsources = string.Empty;
        //                if (leadSourceIds != null)
        //                    leadsources = string.Join("| ", leadSources.Where(e => leadSourceIds.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
        //                value = leadsources;
        //            }
        //            else if (selectedColumns[i].FieldId == 25 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                var ownerName = string.Empty;
        //                if (item.OwnerId != 0)
        //                    ownerName = owners.Where(o => o.OwnerId == item.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
        //                value = ownerName;
        //            }
        //            else if (selectedColumns[i].FieldId == 26 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                value = person != null ? person.LeadScore.ToString() : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 27 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                //value = person != null ? person.CreatedBy.ToString() : string.Empty;
        //                if (person != null && person.CreatedBy.HasValue)
        //                {
        //                    var createdBy = string.Empty;
        //                    if (item.CreatedBy.HasValue)
        //                        createdBy = owners.Where(o => o.OwnerId == item.CreatedBy.Value).Select(s => s.OwnerName).FirstOrDefault();
        //                    value = createdBy;
        //                }
        //                if (company != null && company.CreatedBy.HasValue)
        //                {
        //                    var createdBy = string.Empty;
        //                    if (item.CreatedBy.HasValue)
        //                        createdBy = owners.Where(o => o.OwnerId == item.CreatedBy.Value).Select(s => s.OwnerName).FirstOrDefault();
        //                    value = createdBy;
        //                }
        //            }
        //            else if (selectedColumns[i].FieldId == 28 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                    value = person.CreatedOn.ToString(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture);
        //                else if (company != null)
        //                    value = company.CreatedOn.ToString(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture);
        //            }
        //            else if (selectedColumns[i].FieldId == 29 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                    value = person.LastContacted.HasValue == true ? person.LastContacted.Value.ToString(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
        //                else if (company != null)
        //                    value = company.LastContacted.HasValue == true ? company.LastContacted.Value.ToString(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;//(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 41 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null)
        //                    value = await getLastTouchedThrough(person.LastContactedThrough);
        //                else if (company != null)
        //                    value = await getLastTouchedThrough(company.LastContactedThrough);
        //            }
        //            else if (selectedColumns[i].FieldId == 44 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                if (person != null && person.FirstContactSource != null)
        //                    value = await getSourceType((int)person.FirstContactSource.Value);
        //                else if (company != null && company.FirstContactSource != null)
        //                    value = await getSourceType((int)company.FirstContactSource.Value);
        //                else
        //                    value = string.Empty;
        //            }
        //            else if (selectedColumns[i].FieldId == 50 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                DateTime? leadSourceDate = person.LeadSources.Where(w => !w.IsPrimary).OrderByDescending(d => d.LastUpdatedDate).Select(s => s.LastUpdatedDate).FirstOrDefault();
        //                value = leadSourceDate.HasValue && leadSourceDate.Value != new DateTime() ? leadSourceDate.ToString() : null;
        //            }
        //            else if (selectedColumns[i].FieldId == 51 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                var firstLeadSource = person.LeadSources.Where(w => w.IsPrimary).Select(s => s.Id).FirstOrDefault();
        //                value = leadSources.Where(w => w.DropdownValueID == firstLeadSource).Select(s => s.DropdownValue).FirstOrDefault();
        //            }
        //            else if (selectedColumns[i].FieldId == 52 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
        //            {
        //                DateTime? firstLeadSourceData = person.LeadSources.Where(w => w.IsPrimary).Select(s => s.LastUpdatedDate).FirstOrDefault();
        //                value = firstLeadSourceData.HasValue ? firstLeadSourceData.ToString() : null;
        //            }
        //            else if (selectedColumns[i].FieldId == 54)
        //            {
        //                if (person != null && !string.IsNullOrEmpty(person.NoteSummary))
        //                    value = person.NoteSummary.Replace("\n", "\\n");
        //                else if (company != null && !string.IsNullOrEmpty(company.NoteSummary))
        //                    value = company.NoteSummary.Replace("\n", "\\n");
        //            }
        //            else if (selectedColumns[i].FieldId == 55)
        //            {
        //                if (person != null)
        //                    value = person.LastNoteDate.HasValue && person.LastNoteDate.Value != new DateTime() ? person.LastNoteDate.ToString() : null;
        //                else value = string.Empty;
        //            }
        //            else if (await IsPhoneField(selectedColumns[i].FieldId, searchFields))
        //            {
        //                value = await GetPhoneNumber(selectedColumns[i].FieldId, item.Phones);
        //            }
        //            else if (await IsCustomField(selectedColumns[i].FieldId, searchFields))
        //            {
        //                if (item.CustomFields != null)
        //                {
        //                    var customField = item.CustomFields.Where(w => w.CustomFieldId == selectedColumns[i].FieldId).Select(s => s.Value).FirstOrDefault();
        //                    if (customField != null)
        //                        value = await GetCustomFieldValue(selectedColumns[i].FieldId, customField, (byte)selectedColumns[i].FieldInputTypeId, accountId, customeFieldValueList, dateFormat);
        //                    else
        //                        value = string.Empty;
        //                }
        //                else
        //                    value = string.Empty;
        //            }

        //            values[i] = value;
        //        }
        //        table.Rows.Add(values);
        //    }
        //    return table;
        //}

        public Task<bool> IsPhoneField(int fieldId, IEnumerable<Field> searchFields)
        {
            var phoneField = searchFields.Any(f => f.Id == fieldId && f.IsDropdownField == true);
            return Task<bool>.Run(() => phoneField);
        }

        public Task<string> GetPhoneNumber(int fieldId, IEnumerable<Phone> phones)
        {
            var phoneNumber = string.Empty;
            if (phones != null && phones.Any())
                phoneNumber = phones.Where(p => p.PhoneType == fieldId).Select(p => p.Number).FirstOrDefault();
            return Task<string>.Run(() => phoneNumber);
        }

        public Task<bool> IsCustomField(int fieldId, IEnumerable<Field> searchFields)
        {
            var customField = searchFields.Where(f => f.Id == fieldId && f.IsCustomField == true).Any();
            Logger.Current.Informational("Checking for customfield : " + fieldId + " response : " + customField);
            return Task<string>.Run(() => customField);
        }

        public Task<string> GetCustomFieldValue(int fieldId, string customFieldValue, byte fieldInputTypeId, int accountId,
            IEnumerable<CustomFieldValueOptionViewModel> customeFieldValueList, string dateFormat)
        {
            if ((byte)FieldType.checkbox == fieldInputTypeId || (byte)FieldType.radio == fieldInputTypeId ||
                (byte)FieldType.dropdown == fieldInputTypeId || (byte)FieldType.multiselectdropdown == fieldInputTypeId)
            {
                List<string> optionText = new List<string>();
                var fieldValueOption = customFieldValue;

                if (fieldValueOption != null && customeFieldValueList != null && customeFieldValueList.Any())
                {
                    foreach (var optionId in fieldValueOption.ToString().Split('|'))
                    {
                        var value = customeFieldValueList.Where(w => w.CustomFieldValueOptionId.ToString() == optionId).Select(s => s.Value).FirstOrDefault();
                        if (!string.IsNullOrEmpty(value))
                            optionText.Add(value);
                    }
                }
                string type = string.Join("|", optionText.ToArray());
                return Task<string>.Run(() => type);
            }
            else if ((byte)FieldType.date == fieldInputTypeId)
            {
                DateTime value = DateTime.Parse(customFieldValue);
                var customeFieldValue = value.ToString(dateFormat, CultureInfo.InvariantCulture);
                return Task<string>.Run(() => customeFieldValue);
            }
            else
            {
                Logger.Current.Informational("Customfield value : " + customFieldValue);
                var customeFieldValue = customFieldValue;
                return Task<string>.Run(() => customeFieldValue);
            }
        }

        public string GetEnumDescription(Enum en)
        {
            Type type = en.GetType();
            MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return en.ToString();
        }

        public async Task<CampaignRecipientsSummaryResponse> ContactSummaryBySearchDefinitionAsync(CampaignRecipientsSummaryRequest request)
        {
            int contactsBySearchDefinition = 0;
            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(request.SearchDefinition);
            if (request.SearchDefinition != null)
            {
                var definition = await runSearchAsync<ContactListEntry>(searchDefinition, new SearchParameters() { Limit = 0 });
                contactsBySearchDefinition = definition.Results.Count();
            }

            return new CampaignRecipientsSummaryResponse()
            {
                CountBySearchDefinition = contactsBySearchDefinition,
            };
        }

        public bool SaveAsFavoriteSearch(short searchDefinitionId)
        {
            bool IsUpdated = advancedSearchRepository.UpdateSearchDefinition(searchDefinitionId);
            return IsUpdated;
        }

        public GetAdvanceSearchFieldsResponse GetSearchFields(GetAdvanceSearchFieldsRequest request)
        {
            GetAdvanceSearchFieldsResponse response = new GetAdvanceSearchFieldsResponse();
            if (request != null)
            {
                Logger.Current.Verbose("Request for fetching search-fields");
                Logger.Current.Informational("Requested for search-field from accountId : " + request.accountId);
                IEnumerable<Field> searchFields = advancedSearchRepository.GetSearchFields(request.accountId);
                List<FieldViewModel> searchFieldsViewModel = null;
                if (searchFields != null)
                {
                    List<AppModules> modules = new List<AppModules>() { AppModules.LeadScore, AppModules.Forms, AppModules.WebAnalytics, AppModules.Tags };
                    Dictionary<AppModules, bool> permissions = cachingService.CheckModulePermissions(request.accountId, request.RoleId, modules);
                    if (permissions != null && permissions.Count > 0)
                    {
                        var modifiedSearchFields = searchFields;
                        if (!permissions.Where(w => w.Key == AppModules.LeadScore).FirstOrDefault().Value)
                            modifiedSearchFields = searchFields.Where(w => w.Id != 26).Select(s => s);
                        if (!permissions.Where(w => w.Key == AppModules.WebAnalytics).FirstOrDefault().Value)
                            modifiedSearchFields = modifiedSearchFields.Where(w => w.Id != 45 && w.Id != 46).Select(s => s);
                        if (!permissions.Where(w => w.Key == AppModules.Forms).FirstOrDefault().Value)
                            modifiedSearchFields = searchFields.Where(w => w.Id != 48 && w.Id != 49).Select(s => s);
                        if (!permissions.Where(w => w.Key == AppModules.Tags).FirstOrDefault().Value)
                            modifiedSearchFields = searchFields.Where(w => w.Id != 47).Select(s => s);
                        searchFields = modifiedSearchFields;
                    }
                    searchFieldsViewModel = Mapper.Map<IEnumerable<Field>, IEnumerable<FieldViewModel>>(searchFields).ToList();
                }
                var dropdownValuesList = cachingService.GetDropdownValues(request.accountId);
                var dropdownValues = dropdownValuesList.Where(d => d.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList()
                                     .FirstOrDefault().Where(d => d.IsActive == true);

                foreach (var dropDownValue in dropdownValues)
                {
                    FieldViewModel viewModel = new FieldViewModel();
                    viewModel.FieldId = dropDownValue.DropdownValueID;
                    viewModel.IsDropdownField = true;
                    viewModel.Title = dropDownValue.DropdownValue + " Phone";
                    viewModel.FieldInputTypeId = FieldType.text;
                    viewModel.AccountID = request.accountId;
                    viewModel.DropdownId = dropDownValue.DropdownID;
                    searchFieldsViewModel.Add(viewModel);
                }
                response.FieldsViewModel = searchFieldsViewModel.OrderBy(s => s.AccountID).ThenBy(s => s.IsLeadAdapterField).ThenBy(s => s.LeadAdapterType).ThenBy(s => s.Title);
            }
            return response;
        }

        public GetUpdatableFieldsResponse GetUpdatableFields(GetUpdatableFieldsRequest request)
        {
            GetUpdatableFieldsResponse response = new GetUpdatableFieldsResponse();
            if (request != null && request.AccountId != 0)
            {
                Logger.Current.Verbose("Request for fetching updatable-fields");
                Logger.Current.Informational("Requested for updatable-field from accountId : " + request.AccountId);
                IEnumerable<Field> searchFields = advancedSearchRepository.GetUpdatableFields(request.AccountId);
                List<FieldViewModel> searchFieldsViewModel = null;
                if (searchFields != null)
                {
                    searchFieldsViewModel = Mapper.Map<IEnumerable<Field>, IEnumerable<FieldViewModel>>(searchFields).ToList();
                    var dropdownValuesList = cachingService.GetDropdownValues(request.AccountId);
                    var phoneDropdownValues = dropdownValuesList.Where(d => d.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).Select(s => s.DropdownValuesList).ToList()
                                         .FirstOrDefault().Where(d => d.IsActive == true);
                    var lifeCycleValues = dropdownValuesList.Where(d => d.DropdownID == (byte)DropdownFieldTypes.LifeCycle).Select(s => s.DropdownValuesList).ToList()
                                         .FirstOrDefault().Where(d => d.IsActive == true);

                    foreach (var dropDownValue in phoneDropdownValues)
                    {
                        FieldViewModel viewModel = new FieldViewModel();
                        viewModel.FieldId = dropDownValue.DropdownValueID;
                        viewModel.IsDropdownField = true;
                        viewModel.Title = dropDownValue.DropdownValue + " Phone";
                        viewModel.FieldInputTypeId = FieldType.text;
                        viewModel.AccountID = request.AccountId;
                        viewModel.DropdownId = dropDownValue.DropdownID;
                        searchFieldsViewModel.Add(viewModel);
                    }
                    foreach (var lifeCycleValue in lifeCycleValues)
                    {
                        FieldViewModel viewModel = new FieldViewModel();
                        viewModel.FieldId = lifeCycleValue.DropdownValueID;
                        viewModel.IsDropdownField = true;
                        viewModel.Title = lifeCycleValue.DropdownValue;
                        viewModel.FieldInputTypeId = FieldType.text;
                        viewModel.AccountID = request.AccountId;
                        viewModel.DropdownId = lifeCycleValue.DropdownID;
                        searchFieldsViewModel.Add(viewModel);
                    }
                    response.FieldsViewModel = searchFieldsViewModel.OrderBy(s => s.AccountID).ThenBy(s => s.IsLeadAdapterField).ThenBy(s => s.LeadAdapterType).ThenBy(s => s.Title);
                }
            }
            return response;
        }

        public GetSearchValueOptionsResponse GetSearchValueOptions(GetSearchValueOptionsRequest request)
        {
            GetSearchValueOptionsResponse response = new GetSearchValueOptionsResponse();
            if (request != null)
            {
                Logger.Current.Verbose("Request for fetching search-qualifiers");
                Logger.Current.Informational("Requested for search-field from accountId : " + request.FieldId);

                IEnumerable<FieldValueOption> searchValueOptions = null;
                if (request.FieldId == (int)ContactFields.StateField)
                {
                    Logger.Current.Verbose("Request for getting states as value-options");
                    var statesResponse = geoService.GetAllStates(new Messaging.Geo.GetAllStatesRequest());
                    searchValueOptions = Mapper.Map<IEnumerable<State>, IEnumerable<FieldValueOption>>(statesResponse.States);

                }
                else if (request.FieldId == (int)ContactFields.CountryField)
                {
                    Logger.Current.Verbose("Request for getting countries as value-options");
                    var countriesResponse = geoService.GetCountries(new Messaging.Geo.GetCountriesRequest());
                    searchValueOptions = Mapper.Map<IEnumerable<Country>, IEnumerable<FieldValueOption>>(countriesResponse.Countries.Select(c => c as Country));
                }
                else if (request.FieldId == (int)ContactFields.Owner || request.FieldId == (int)ContactFields.CreatedBy || request.FieldId == (int)ContactFields.TourCreator
                    || request.FieldId == (int)ContactFields.TourAssignedUsers || request.FieldId == (int)ContactFields.ActionAssignedTo)
                {
                    Logger.Current.Verbose("Request for fetching users as value-options");
                    var usersResponse = new GetUsersResponse();
                    //bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
                    if (request.IsSTAdmin)
                        usersResponse = contactService.GetUsers(new GetUsersRequest() { AccountID = request.AccountId, RequestedBy = 0, IsSTadmin = request.IsSTAdmin });
                    else
                        usersResponse = contactService.GetUsers(new GetUsersRequest() { AccountID = request.AccountId, IsSTadmin = request.IsSTAdmin });
                    if (usersResponse.Owner != null)
                        searchValueOptions = Mapper.Map<IEnumerable<Owner>, IEnumerable<FieldValueOption>>(usersResponse.Owner);
                }
                else if (request.ContactDropdownId.HasValue)
                {
                    Logger.Current.Verbose("Request for fetching value-options from dropdowns");
                    var dropdownValuesList = cachingService.GetDropdownValues(request.AccountId);
                    var dropdownValues = dropdownValuesList.Where(d => d.DropdownID == request.ContactDropdownId.Value).Select(s => s.DropdownValuesList).ToList()
                           .FirstOrDefault().Where(d => d.IsActive == true);
                    searchValueOptions = Mapper.Map<IEnumerable<DropdownValueViewModel>, IEnumerable<FieldValueOption>>(dropdownValues);
                }
                else if (request.FieldId == (int)ContactFields.LeadAdapter)
                {
                    Logger.Current.Verbose("Request received for fetching lead adapters in advanced search");
                    searchValueOptions = advancedSearchRepository.GetLeadAdapters(request.AccountId);
                }
                else if (request.FieldId == (int)ContactFields.ContactTag)
                {
                    IEnumerable<Tag> tags = advancedSearchRepository.GetTags(request.AccountId);
                    searchValueOptions = mapTags(tags);
                }
                else if (request.FieldId == (int)ContactFields.FormName)
                {
                    IEnumerable<Form> forms = workflowRepository.GetAllForms(request.AccountId);
                    searchValueOptions = mapForms(forms);
                }
                else
                    searchValueOptions = advancedSearchRepository.GetSearchValueOptions(request.FieldId);

                if (searchValueOptions != null)
                    response.FieldValueOptions = searchValueOptions;
            }
            return response;
        }

        private IEnumerable<FieldValueOption> mapTags(IEnumerable<Tag> tags)
        {
            List<FieldValueOption> fieldOptions = new List<FieldValueOption>();
            if (tags != null && tags.Any())
            {
                foreach (var tag in tags.OrderBy(o => o.TagName))
                {
                    FieldValueOption option = new FieldValueOption();
                    option.Id = tag.Id;
                    option.Value = tag.TagName;
                    fieldOptions.Add(option);
                }
            }
            return fieldOptions;
        }

        private IEnumerable<FieldValueOption> mapForms(IEnumerable<Form> forms)
        {
            List<FieldValueOption> fieldOptions = new List<FieldValueOption>();
            if (forms != null && forms.Any())
                foreach (var form in forms)
                {
                    FieldValueOption option = new FieldValueOption();
                    option.Id = form.Id;
                    option.Value = form.Name;
                    fieldOptions.Add(option);
                }
            return fieldOptions;
        }

        public InsertLastRunActivityResponse InsertLastRun(InsertLastRunActivityRequest request)
        {
            InsertLastRunActivityResponse response = new InsertLastRunActivityResponse();
            if (request.AccountId != 0 && request.RequestedBy.HasValue)
            {
                advancedSearchRepository.UpdateLastRunActivity(request.SearchDefinitionId, request.RequestedBy.Value, request.AccountId, request.SearchName);
            }
            return response;
        }

        public GetSearchDefinitionDescriptionRespone GetSearchDefinitionDescription(GetSearchDefinitionDescriptionRequest request)
        {
            GetSearchDefinitionDescriptionRespone response = new GetSearchDefinitionDescriptionRespone();
            try
            {
                response.Title = advancedSearchRepository.GetSearchDescription(request.SearchDefinitionId);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while getting search description : ", ex);
                throw;
            }
            return response;
        }

        public InsertViewActivityResponse InsertViewActivity(InsertViewActivityRequest request)
        {
            InsertViewActivityResponse response = new InsertViewActivityResponse();
            if (request.AccountId != 0 && request.RequestedBy.HasValue)
                advancedSearchRepository.UpdateViewActivity(request.SearchDefinitionId, request.RequestedBy.Value, request.AccountId, request.SearchName);
            return response;
        }

        private async Task<List<ContactListEntry>> ConvertToListEntry(IEnumerable<Contact> contacts)
        {
            List<ContactListEntry> listEntries = new List<ContactListEntry>();
            if (contacts != null && contacts.IsAny())
            {
                try
                {
                    foreach (var c in contacts)
                    {
                        if (c.GetType().Equals(typeof(Person)))
                        {
                            Person person = c as Person;
                            AddressViewModel address = person.Addresses != null ? Mapper.Map<Address, AddressViewModel>(person.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault()) : null;
                            var email = person.Emails != null && person.Emails.Any() ? person.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                            var phone = person.Phones != null && person.Phones.Any() ? person.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                            var customFields = mapCustomFields(person.CustomFields);
                            var phones = mapPhones(person.Phones);
                            var lastTouchedThrough = getLastTouchedThrough(person.LastContactedThrough);
                            var sourceType = getSourceType((int?)person.FirstContactSource);

                            ContactListEntry listEntry = new ContactListEntry();
                            listEntry.ContactID = person.Id;
                            listEntry.FirstName = person.FirstName;
                            listEntry.LastName = person.LastName;
                            listEntry.Name = person.FirstName + " " + person.LastName;
                            listEntry.CompanyName = !string.IsNullOrEmpty(c.CompanyName) ? c.CompanyName : string.Empty;
                            listEntry.CompanyID = person.CompanyID;
                            listEntry.ContactType = (int)ContactType.Person;
                            listEntry.ContactImageUrl = person.ContactImage != null ? person.ContactImage.StorageName : null;
                            listEntry.AccountID = person.AccountID;
                            listEntry.ProfileImageKey = person.ProfileImageKey;
                            listEntry.LifecycleStage = person.LifecycleStage;
                            listEntry.LeadScore = person.LeadScore;
                            listEntry.DoNotEmail = person.DoNotEmail;
                            listEntry.PartnerType = person.PartnerType.HasValue ? person.PartnerType.Value : default(short);
                            listEntry.FacebookUrl = person.FacebookUrl != null ? person.FacebookUrl.URL : null;
                            listEntry.GooglePlusUrl = person.GooglePlusUrl != null ? person.GooglePlusUrl.URL : null;
                            listEntry.TwitterUrl = person.TwitterUrl != null ? person.TwitterUrl.URL : null;
                            listEntry.LinkedInUrl = person.LinkedInUrl != null ? person.LinkedInUrl.URL : null;
                            listEntry.BlogUrl = person.BlogUrl != null ? person.BlogUrl.URL : null;
                            listEntry.WebsiteUrl = person.WebsiteUrl != null ? person.WebsiteUrl.URL : null;
                            listEntry.OwnerId = person.OwnerId;
                            listEntry.Title = person.Title;
                            listEntry.CreatedOn = person.CreatedOn;
                            listEntry.CreatedBy = person.CreatedBy;
                            listEntry.LastTouched = person.LastContacted;
                            listEntry.LastTouchedThrough = person.LastTouchedThrough;
                            listEntry.LastUpdatedOn = person.LastUpdatedOn;
                            listEntry.LeadSourceIds = person.LeadSources != null && person.LeadSources.IsAny() ? person.LeadSources.Where(w => !w.IsPrimary).Select(s => s.Id).ToList() : null;

                            var leadSourceDate = person.LeadSources != null && person.LeadSources.IsAny() ? person.LeadSources.Where(w => !w.IsPrimary).Select(se => se.LastUpdatedDate) : null;
                            listEntry.LeadSourceDate = leadSourceDate != null && leadSourceDate.IsAny() ? string.Join(", ", leadSourceDate) : string.Empty;
                            listEntry.FirstLeadSourceId = person.LeadSources != null && person.LeadSources.IsAny() ? person.LeadSources.Where(w => w.IsPrimary).Select(se => se.Id).FirstOrDefault() : default(short);

                            DateTime? firstLeadSourceDate = person.LeadSources != null && person.LeadSources.IsAny() ? person.LeadSources.Where(w => w.IsPrimary).Select(se => se.LastUpdatedDate).FirstOrDefault() : (DateTime?)null;
                            listEntry.FirstLeadSourceDate = firstLeadSourceDate.HasValue ? firstLeadSourceDate.Value : (DateTime?)null;
                            listEntry.SourceType = await sourceType;

                            listEntry.PrimaryAddress = address;
                            listEntry.Address = address != null ? address.ToString() : "[|No address details|]";
                            listEntry.PrimaryEmail = email != null ? email.EmailId : "[|Email Not Available|]";
                            listEntry.PrimaryContactEmailID = email != null ? email.EmailID : default(int);
                            listEntry.PrimaryEmailStatus = email != null ? email.EmailStatusValue : (EmailStatus)0;
                            listEntry.Phone = phone != null ? phone.Number + " ," + phone.PhoneTypeName : "(xxx) xxx - xxxx";
                            //listEntry.Phone = phone != null ? (!string.IsNullOrEmpty(phone.CountryCode) ? "+" + phone.CountryCode + " " : "") + phone.Number + (!string.IsNullOrEmpty(phone.Extension) ? " Ext. " + phone.Extension : "") +
                            //                 " (" + phone.PhoneTypeName + ")" : "(xxx) xxx - xxxx";
                            listEntry.PrimaryContactPhoneNumberID = phone != null ? phone.ContactPhoneNumberID : default(int);
                            listEntry.CustomFields = await customFields;
                            listEntry.Phones = await phones;
                            listEntry.LastTouchedThrough = await lastTouchedThrough;
                            listEntry.LastNoteDate = person.LastNoteDate;
                            listEntry.NoteSummary = person.NoteSummary ?? "";
                            listEntry.FirstSourceType = person.FirstSourceType;
                            listEntry.LastNote = person.LastNote ?? "";

                            listEntries.Add(listEntry);
                        }
                        else if (c.GetType().Equals(typeof(Company)))
                        {
                            Company company = c as Company;
                            AddressViewModel address = company.Addresses != null ? Mapper.Map<Address, AddressViewModel>(company.Addresses.Where(a => a.IsDefault.Equals(true)).FirstOrDefault()) : null;
                            var email = company.Emails != null && company.Emails.Any() ? company.Emails.FirstOrDefault(e => e.IsPrimary) : null;
                            var phone = company.Phones != null && company.Phones.Any() ? company.Phones.FirstOrDefault(p => p.IsPrimary) : null;
                            var customFields = mapCustomFields(company.CustomFields);
                            var phones = mapPhones(company.Phones);
                            var lastTouchedThrough = getLastTouchedThrough(company.LastContactedThrough);
                            var sourceType = getSourceType((int?)company.FirstContactSource);

                            ContactListEntry listEntry = new ContactListEntry();
                            listEntry.ContactID = company.Id;

                            listEntry.CompanyName = !string.IsNullOrEmpty(c.CompanyName) ? c.CompanyName : string.Empty;
                            listEntry.Name = listEntry.CompanyName;
                            listEntry.CompanyID = company.CompanyID;
                            listEntry.ContactType = (int)ContactType.Company;
                            listEntry.ContactImageUrl = company.ContactImage != null ? company.ContactImage.StorageName : null;
                            listEntry.AccountID = company.AccountID;
                            listEntry.ProfileImageKey = company.ProfileImageKey;
                            listEntry.DoNotEmail = company.DoNotEmail;
                            listEntry.FacebookUrl = company.FacebookUrl != null ? company.FacebookUrl.URL : null;
                            listEntry.GooglePlusUrl = company.GooglePlusUrl != null ? company.GooglePlusUrl.URL : null;
                            listEntry.TwitterUrl = company.TwitterUrl != null ? company.TwitterUrl.URL : null;
                            listEntry.LinkedInUrl = company.LinkedInUrl != null ? company.LinkedInUrl.URL : null;
                            listEntry.BlogUrl = company.BlogUrl != null ? company.BlogUrl.URL : null;
                            listEntry.WebsiteUrl = company.WebsiteUrl != null ? company.WebsiteUrl.URL : null;
                            listEntry.OwnerId = company.OwnerId;
                            listEntry.CreatedOn = company.CreatedOn;
                            listEntry.CreatedBy = company.CreatedBy;
                            listEntry.LastTouched = company.LastContacted;
                            listEntry.LastTouchedThrough = company.LastTouchedThrough;
                            listEntry.LastUpdatedOn = company.LastUpdatedOn;
                            listEntry.PrimaryAddress = address;
                            listEntry.SourceType = await sourceType;
                            listEntry.Address = address != null ? address.ToString() : "[|No address details|]";
                            listEntry.PrimaryEmail = email != null ? email.EmailId : "[|Email Not Available|]";
                            listEntry.PrimaryContactEmailID = email != null ? email.EmailID : default(int);
                            listEntry.PrimaryEmailStatus = email != null ? email.EmailStatusValue : (EmailStatus)0;
                            listEntry.Phone = phone != null ? phone.Number + " ," + phone.PhoneTypeName : "(xxx) xxx - xxxx";
                            //listEntry.Phone = phone != null ? (!string.IsNullOrEmpty(phone.CountryCode) ? "+" + phone.CountryCode + " " : "") + phone.Number + (!string.IsNullOrEmpty(phone.Extension) ? " Ext. " + phone.Extension : "") +
                            //            " (" + phone.PhoneTypeName + ")" : "(xxx) xxx - xxxx";
                            listEntry.PrimaryContactPhoneNumberID = phone != null ? phone.ContactPhoneNumberID : default(int);
                            listEntry.Phones = await phones;
                            listEntry.LastTouchedThrough = await lastTouchedThrough;
                            listEntry.CustomFields = await customFields;
                            listEntry.NoteSummary = company.NoteSummary ?? "";
                            listEntry.LastNoteDate = company.LastNoteDate;
                            listEntry.FirstSourceType = company.FirstSourceType;
                            listEntry.LastNote = company.LastNote;
                            listEntries.Add(listEntry);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Current.Error("An error occured while converting contact to contactlistentry", ex);
                    throw ex;
                }
            }
            return listEntries;
        }

        private Task<List<Phone>> mapPhones(IEnumerable<Phone> contactPhones)
        {
            var phones = new List<Phone>();
            foreach (Phone phone in contactPhones)
            {
                if (phone != null)
                    phone.Number = phone.Number != null ? phone.Number : "(xxx) xxx - xxxx";
                //phone.Number = phone.Number != null ? (!string.IsNullOrEmpty(phone.CountryCode) ? "+" + phone.CountryCode + " " : "") + phone.Number + (!string.IsNullOrEmpty(phone.Extension) ? " Ext. " + phone.Extension : "") + 
                //    " (" + phone.PhoneTypeName + ")" : "(xxx) xxx - xxxx";
                phones.Add(phone);
            }
            return Task<List<Phone>>.Run(() => phones);
        }

        private Task<string> getLastTouchedThrough(byte? lastTouched)
        {
            string LastTouchedThrough = string.Empty;
            if (lastTouched.HasValue && lastTouched.Value > 0) LastTouchedThrough = ((LastTouchedValues)lastTouched.Value).GetDisplayName();
            return Task<string>.Run(() => LastTouchedThrough);
        }

        private Task<string> getSourceType(int? sourceType)
        {
            string contactSourceType = string.Empty;
            if (sourceType.HasValue && sourceType.Value > 0) contactSourceType = ((ContactSource)sourceType.Value).GetDisplayName();
            return Task<string>.Run(() => contactSourceType);
        }

        private Task<List<ContactCustomFieldMapViewModel>> mapCustomFields(IEnumerable<ContactCustomField> customFields)
        {
            return Task<IEnumerable<ContactCustomFieldMapViewModel>>.Run(() => Mapper.Map<IEnumerable<ContactCustomField>, List<ContactCustomFieldMapViewModel>>(customFields));
        }

        private Task<List<ContactListEntry>> ManageDropdowns(List<ContactListEntry> contacts, IEnumerable<DropdownValueViewModel> lifecycleStages, IEnumerable<DropdownValueViewModel> partnerTypes,
            IEnumerable<DropdownValueViewModel> leadSources, IEnumerable<Owner> Owners)
        {
            if (contacts != null)
            {
                foreach (var contact in contacts)
                {
                    contact.LifecycleName = lifecycleStages.Where(e => e.DropdownValueID == contact.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
                    contact.PartnerTypeName = partnerTypes.Where(e => e.DropdownValueID == contact.PartnerType).Select(s => s.DropdownValue).FirstOrDefault();
                    contact.OwnerName = Owners.Where(o => o.OwnerId == contact.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
                    contact.CreatedByUser = Owners.Where(o => o.OwnerId == contact.CreatedBy).Select(s => s.OwnerName).FirstOrDefault();
                    if (contact.LeadSourceIds != null)
                    {
                        contact.LeadSources = string.Join(", ", leadSources.Where(e => contact.LeadSourceIds.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
                        contact.FirstLeadSource = leadSources.Where(w => w.DropdownValueID == contact.FirstLeadSourceId).Select(s => s.DropdownValue).FirstOrDefault();
                    }
                }
            }

            return Task<List<ContactListEntry>>.Run(() => contacts);
        }

        private List<ContactFields> MapFieldsToContactField(IEnumerable<int> fields)
        {
            List<ContactFields> contactFields = new List<ContactFields>();
            if (fields != null && fields.Any())
            {
                foreach (var field in fields)
                {
                    try
                    {
                        if (field < 200)
                        {
                            var contactfield = (ContactFields)field;
                            contactFields.Add(contactfield);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("No contactfield found for the above field " + ex);
                        continue;
                    }
                }
                if (fields.Any(a => a > 200))
                {
                    contactFields.Add(ContactFields.CustomFields);
                }
            }
            return contactFields;
        }

        public GetAdvancedViewColumnsResponse GetColumns(GetAdvancedViewColumnsRequest request)
        {
            GetAdvancedViewColumnsResponse response = new GetAdvancedViewColumnsResponse();
            if (request != null)
            {
                IEnumerable<AVColumnPreferences> columnPreference = advancedSearchRepository.GetColumnPreferences(request.EntityId, request.EntityType);
                if (columnPreference != null)
                    response.ColumnPreferenceViewModel = Mapper.Map<IEnumerable<AVColumnPreferences>, IEnumerable<AVColumnPreferenceViewModel>>(columnPreference);
            }
            return response;
        }

        public SaveAdvancedViewColumnsResponse SaveColumns(SaveAdvancedViewColumnsRequest request)
        {
            SaveAdvancedViewColumnsResponse response = new SaveAdvancedViewColumnsResponse();
            if (request.model != null)
            {
                var ViewModel = request.model;
                advancedSearchRepository.SaveColumnPreferences(ViewModel.EntityID, ViewModel.EntityType, ViewModel.Fields, ViewModel.ShowingType);
            }
            return response;
        }
    }
}
