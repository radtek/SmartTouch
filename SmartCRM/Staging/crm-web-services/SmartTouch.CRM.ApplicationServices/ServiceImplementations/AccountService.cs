using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Extensions;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using Microsoft.Web.Administration;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using CMO = LandmarkIT.Enterprise.CommunicationManager.Operations;
using CMR = LandmarkIT.Enterprise.CommunicationManager.Requests;
using SmartTouch.CRM.ApplicationServices.ObjectMappers;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using S = SmartTouch.CRM.ApplicationServices.Messaging.Search;
using ID = SmartTouch.CRM.ApplicationServices.Messaging.ImportData;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using LandmarkIT.Enterprise.Extensions;
using System.Threading.Tasks;
using SmartTouch.CRM.ApplicationServices.ServiceAgents;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class AccountService : IAccountService
    {
        readonly IContactRepository contactRepository;
        readonly IAccountRepository accountRepository;
        readonly ILeadAdaptersRepository leadAdaptersRepository;
        readonly IImportDataRepository importRepository;
        readonly ISubscriptionRepository subscriptionRepository;
        readonly IDropdownRepository dropdownRepository;
        readonly ICommunicationProviderService serviceProviderService;
        readonly ICustomFieldRepository customFieldsRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IIndexingService indexingService;
        readonly ICustomFieldService customFieldService;
        readonly ISearchService<Contact> searchService;
        readonly IUserRepository userRepository;
        readonly ICachingService cacheService;
        readonly ITagRepository tagRepository;
        readonly IServiceProviderRepository serviceproviderRepository;
        readonly IUrlService urlService;
        readonly IReportService reportService;
        readonly IUserService userService;

        readonly IAdvancedSearchRepository advancedSearchRepository;
        IImageService imageService;

        public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork,
            IContactRepository contactRepository, IImportDataRepository importRepository,
            ILeadAdaptersRepository leadAdaptersRepository, ISubscriptionRepository subscriptionRepository,
            IDropdownRepository dropdownRepository, IIndexingService indexingService, ISearchService<Contact> searchService,
            ICommunicationProviderService serviceProviderService, ICustomFieldRepository customFieldsRepository, IUserRepository userRepository,
            ICachingService cacheService, IServiceProviderRepository serviceproviderRepository, IUrlService urlService, IImageService imageService,
            IReportService reportService, ITagRepository tagRepository, IAdvancedSearchRepository advncedSearchRepository, ICustomFieldService customFieldService, IUserService userService)
        {
            if (contactRepository == null) throw new ArgumentNullException("contactRepository");
            if (accountRepository == null) throw new ArgumentNullException("accountRepository");
            if (importRepository == null) throw new ArgumentNullException("importRepository");
            if (leadAdaptersRepository == null) throw new ArgumentNullException("leadAdaptersRepository");
            if (subscriptionRepository == null) throw new ArgumentNullException("subscriptionRepository");
            if (serviceProviderService == null) throw new ArgumentNullException("serviceProviderRepository");
            if (userRepository == null) throw new ArgumentNullException("userRepository");
            if (advncedSearchRepository == null) throw new ArgumentNullException("advncedSearchRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.accountRepository = accountRepository;
            this.contactRepository = contactRepository;
            this.importRepository = importRepository;
            this.leadAdaptersRepository = leadAdaptersRepository;
            this.subscriptionRepository = subscriptionRepository;
            this.dropdownRepository = dropdownRepository;
            this.serviceProviderService = serviceProviderService;
            this.unitOfWork = unitOfWork;
            this.indexingService = indexingService;
            this.searchService = searchService;
            this.customFieldsRepository = customFieldsRepository;
            this.userRepository = userRepository;
            this.cacheService = cacheService;
            this.serviceproviderRepository = serviceproviderRepository;
            this.urlService = urlService;
            this.reportService = reportService;
            this.imageService = imageService;
            this.tagRepository = tagRepository;
            this.advancedSearchRepository = advncedSearchRepository;
            this.customFieldService = customFieldService;
            this.userService = userService;
        }

        #region Accounts


        /// <summary>
        /// Gets the reputation count.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public GetSenderReputationCountResponse GetReputationCount(GetSenderReputationCountRequest request)
        {
            GetSenderReputationCountResponse response = new GetSenderReputationCountResponse();
            response.ReputationCount = accountRepository.GetReputationCount(request.AccountId, request.StartDate, request.EndDate);
            return response;
        }

        /// <summary>
        /// Gets all accounts.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        public GetAccountsResponse GetAllAccounts(GetAccountsRequest request)
        {
            GetAccountsResponse response = new GetAccountsResponse();
            IEnumerable<AccountsGridData> accounts = null;
            IList<AccountViewModel> viewModel = new List<AccountViewModel>();
            AccountsList accountList = accountRepository.FindAllAccounts(request.Query, request.Limit, request.PageNumber, request.Status, request.SortField, request.SortDirection);
            accounts = accountList.AccountGridData;

            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                foreach (AccountsGridData account in accounts)
                {
                    viewModel.Add(Mapper.Map<AccountsGridData, AccountViewModel>(account));
                }
                response.Accounts = viewModel;
                response.TotalHits = accountList.TotalHits;
            }
            return response;
        }

        public string GetRowData(int newDataId, int oldDataId)
        {
            RawContact newContact = importRepository.GetImportContactByDataID(newDataId);

            if (newContact != null)
            {
                string source = importRepository.GetSourceByJobId(newContact.JobID);

                string FirstName = newContact.FirstName != null ? newContact.FirstName : null;
                string LastName = newContact.LastName != null ? newContact.LastName : null;
                string PrimaryEmail = newContact.PrimaryEmail != null ? newContact.PrimaryEmail : null;
                string CompanyName = newContact.CompanyName != null ? newContact.CompanyName : null;
                StringBuilder hash = new StringBuilder();
                IList<Email> emails = new List<Email>();
                if (!string.IsNullOrEmpty(PrimaryEmail))
                {

                    Email primaryemail = new Email()
                    {
                        EmailId = PrimaryEmail,
                        AccountID = newContact.AccountID,
                        IsPrimary = true
                    };
                    emails.Add(primaryemail);
                    hash.Append("-").Append(PrimaryEmail);
                }
                else
                {
                    hash.Append("-").Append("na@na.com");
                }

                Person person = new Person()
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    CompanyName = CompanyName,
                    Emails = emails,
                    AccountID = newContact.AccountID
                };
                if (string.IsNullOrEmpty(FirstName))
                    hash.Append("-").Append(string.Empty);
                else
                    hash.Append("-").Append(FirstName);

                if (string.IsNullOrEmpty(LastName))
                    hash.Append("-").Append(string.Empty);
                else
                    hash.Append("-").Append(LastName);

                if (string.IsNullOrEmpty(CompanyName))
                    hash.Append("-").Append(string.Empty);
                else
                    hash.Append("-").Append(CompanyName);


                Contact duplicateperson = default(Person);
                bool firstTimeProcessing = true;
                if (oldDataId == 0)
                {
                    SearchParameters parameters = new SearchParameters() { AccountId = newContact.AccountID };
                    SearchResult<Contact> duplicateResult = searchService.DuplicateSearch(person, parameters);
                    if (!duplicateResult.Results.Any())
                    {
                        string primaryEmail = person.Emails != null ? person.Emails.Where(w => w.IsPrimary).Select(s => s.EmailId).FirstOrDefault() : string.Empty;
                        IEnumerable<Contact> duplicateContacts = contactRepository.CheckDuplicate(person.FirstName, person.LastName, primaryEmail, person.CompanyName, person.Id, newContact.AccountID, (byte)ContactType.Person);
                        duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
                    }

                    if (duplicateResult != null && duplicateResult.Results != null && duplicateResult.Results.Count() > 0)
                    {
                        firstTimeProcessing = contactRepository.IsNewContact(duplicateResult.Results.FirstOrDefault().Id);
                        if (!firstTimeProcessing)
                        {
                            duplicateperson = duplicateResult.Results.FirstOrDefault();
                            Logger.Current.Informational("duplicate person " + ((duplicateperson != null) ? duplicateperson.Id : 0));
                        }
                    }
                }
                else if (oldDataId != 0)
                {
                    RawContact oldContact = importRepository.GetImportContactByDataID(oldDataId);
                    Person cdata = new Person();
                    cdata.FirstName = oldContact.FirstName;
                    cdata.LastName = oldContact.LastName;
                    Email email = new Email();
                    List<Email> emailsdata = new List<Email>();
                    email.EmailId = oldContact.PrimaryEmail;
                    email.IsPrimary = true;
                    emailsdata.Add(email);
                    cdata.Emails = emailsdata;
                    cdata.CompanyName = oldContact.CompanyName;

                    Address address = new Address();
                    List<Address> addresses = new List<Address>();
                    address.ZipCode = oldContact.ZipCode;
                    addresses.Add(address);

                    cdata.Addresses = addresses;

                    List<ContactCustomField> customfields = importRepository.GetImportCustomFieldsByRefID(oldContact.ReferenceId);
                    cdata.CustomFields = customfields;
                    cdata.ReferenceId = oldContact.ReferenceId;

                    duplicateperson = cdata;
                }

                var oldNewValues = new Dictionary<string, dynamic> { };
                string oldSource = string.Empty;

                if (duplicateperson != null)
                    oldSource = importRepository.GetLeadAdapterTypeByReferenceId(duplicateperson.ReferenceId);
                else
                    duplicateperson = new Person();

                var properties = typeof(RawContact)
                              .GetProperties(BindingFlags.Public | BindingFlags.Instance);

                Logger.Current.Informational("source" + source);
                oldNewValues.Add("Source", new { OldValue = string.IsNullOrEmpty(oldSource) ? string.Empty : oldSource, NewValue = source });
                byte leadAdapterType = leadAdaptersRepository.GetLeadAdapterTypeByJobId(newContact.JobID);

                foreach (var property in properties)
                {
                    var newValue = GetPropValue(newContact, property.Name);

                    var elementvalue = string.Empty;

                    Logger.Current.Informational("newValue" + newValue);

                    if (property.Name == "FirstName")
                    {
                        elementvalue = ((Person)duplicateperson).FirstName == null ? "" : ((Person)duplicateperson).FirstName;
                        oldNewValues.Add(property.Name, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "LastName")
                    {
                        elementvalue = ((Person)duplicateperson).LastName;
                        oldNewValues.Add(property.Name, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "PrimaryEmail")
                    {
                        Email primaryemail = duplicateperson.Emails == null ? null : duplicateperson.Emails.Where(i => i.IsPrimary == true).FirstOrDefault();
                        elementvalue = primaryemail != null ? primaryemail.EmailId : "";

                        Logger.Current.Informational("primaryemail" + elementvalue);

                        oldNewValues.Add(property.Name, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "CompanyName")
                    {
                        elementvalue = duplicateperson.CompanyName;
                        oldNewValues.Add(property.Name, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "CustomFieldsData")
                    {
                        GetAllCustomFieldsResponse response = customFieldService.GetAllCustomFields(new GetAllCustomFieldsRequest(newContact.AccountID));

                        Logger.Current.Informational("Custom fields" + response);

                        IEnumerable<FieldViewModel> customFields = response.CustomFields.Where(i => i.IsLeadAdapterField && i.LeadAdapterType == leadAdapterType && i.StatusId == FieldStatus.Active);

                        Logger.Current.Informational("Custom fields1" + customFields.Count());
                        List<string> str = new List<string>();
                        var a = newValue.Split('~');

                        foreach (var item in a)
                        {
                            if (item.Contains("##$##"))
                                str.Add(item);
                            else
                            {
                                if (str.Count > 0)
                                    str[str.Count - 1] = str.LastOrDefault() + "~" + item;
                            }


                        }

                        foreach (var data in str)
                        {
                            if (data != "")
                            {
                                var b = data.Split(new string[] { "##$##" }, StringSplitOptions.None);
                                var fieldId = Convert.ToInt32(b[0]);
                                var c = b[1].Split('|');
                                var fieldValue = c[1];

                                var customField = customFields.Where(p => p.FieldId == fieldId).FirstOrDefault();

                                Logger.Current.Informational("Custom field" + customField);

                                if (customField != null)
                                {
                                    var customfielddata = duplicateperson.CustomFields == null ? null : duplicateperson.CustomFields.Where(i => i.CustomFieldId == customField.FieldId).FirstOrDefault();
                                    if (customfielddata != null)
                                        elementvalue = customfielddata.Value;
                                    else
                                        elementvalue = string.Empty;

                                    oldNewValues.Add(string.IsNullOrEmpty(customField.Title) ? string.Empty : customField.Title, new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue.Replace("\\n", "{BREAK}"), NewValue = fieldValue.Replace("\\n", "{BREAK}") });
                                }
                            }
                        }
                    }
                    else if (property.Name == "ZipCode")
                    {
                        elementvalue = duplicateperson.Addresses != null ? duplicateperson.Addresses.Select(p => p.ZipCode).LastOrDefault() : "";

                        Logger.Current.Informational("Addresses" + elementvalue);

                        oldNewValues.Add("PostalCode", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "City" && leadAdapterType == 15)
                    {
                        elementvalue = duplicateperson.Addresses != null ? duplicateperson.Addresses.Select(p => p.City).LastOrDefault() : "";

                        Logger.Current.Informational("Addresses" + elementvalue);

                        oldNewValues.Add("City", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "State" && leadAdapterType == 15)
                    {
                        elementvalue = duplicateperson.Addresses != null ? duplicateperson.Addresses.Select(p => p.State.Name).LastOrDefault() : "";

                        Logger.Current.Informational("Addresses" + elementvalue);

                        oldNewValues.Add("State", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "Country" && leadAdapterType == 15)
                    {
                        elementvalue = duplicateperson.Addresses != null ? duplicateperson.Addresses.Select(p => p.Country.Name).LastOrDefault() : "";

                        Logger.Current.Informational("Addresses" + elementvalue);

                        oldNewValues.Add("Country", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "AddressLine1" && leadAdapterType == 15)
                    {
                        elementvalue = duplicateperson.Addresses != null ? duplicateperson.Addresses.Select(p => p.AddressLine1).LastOrDefault() : "";

                        Logger.Current.Informational("Addresses" + elementvalue);

                        oldNewValues.Add("AddressLine1", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newValue });
                    }
                    else if (property.Name == "PhoneData")
                    {
                        IEnumerable<DropdownValueViewModel> phoneDropdownFields = this.GetPhoneDropdownFields(newContact.AccountID);
                        DropdownValueViewModel drpvalue = phoneDropdownFields.Where(i => i.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                        if (duplicateperson.Phones != null)
                        {
                            var mobilePhone = duplicateperson.Phones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
                            if (mobilePhone != null)
                                elementvalue = mobilePhone.Number;
                        }

                        Logger.Current.Informational("work Phone " + elementvalue);

                        var newPhoneSplit = newValue.IsAny() ? newValue.Split('|') : new string[1];
                        var newPhone = string.Empty;
                        if (newPhoneSplit.Length > 1)
                        {
                            newPhone = newPhoneSplit[1];
                        }
                        oldNewValues.Add("PhoneNumber", new { OldValue = string.IsNullOrEmpty(elementvalue) ? string.Empty : elementvalue, NewValue = newPhone });
                    }
                }
                JavaScriptSerializer js = new JavaScriptSerializer();
                string rowData = js.Serialize(oldNewValues);
                return rowData;
            }
            else
            {
                Logger.Current.Informational("newContact is empty");
                return "";
            }
        }

        public IEnumerable<DropdownValueViewModel> GetPhoneDropdownFields(int accountId)
        {
            var dropdownfeildsresposne = cacheService.GetDropdownValues(accountId);
            IEnumerable<DropdownValueViewModel> phoneFields = dropdownfeildsresposne.Where(x => x.DropdownID == (short)DropdownFieldTypes.PhoneNumberType).Select(x => x.DropdownValuesList).FirstOrDefault();
            return phoneFields;
        }

        public string GetDomainUrlByAccountId(int accountId)
        {
            string domainUrl = accountRepository.GetDomainUrlByAccountId(accountId);
            return domainUrl;
        }

        public static string GetPropValue(object src, string propName)
        {
            var value = string.Empty;

            try
            {
                value = src.GetType().GetProperty(propName).GetValue(src, null).ToString();
            }
            catch (Exception)
            {
                value = string.Empty;
            }
            return value;
        }

        public GetBdxAccountsResponse GetBdxAccounts(GetBdxAccountsRequest request)
        {
            GetBdxAccountsResponse response = new GetBdxAccountsResponse();
            response.BdxAccounts = accountRepository.GetBdxAccounts(request.AccountName);
            return response;
        }


        public GetAccountResponse GetAccountByName(GetAccountNameRequest request)
        {
            GetAccountResponse response = new GetAccountResponse();
            Account accounts = accountRepository.FindByName(request.name);
            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                AccountViewModel accountViewModel = Mapper.Map<Account, AccountViewModel>(accounts);
                response.AccountViewModel = accountViewModel;
            }
            return response;
        }

        public GetAccountResponse GetAccountById(GetAccountIdRequest request)
        {
            GetAccountResponse response = new GetAccountResponse();
            Account accounts = accountRepository.FindByAccountID(request.accountId);
            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                AccountViewModel accountViewModel = Mapper.Map<Account, AccountViewModel>(accounts);
                response.AccountViewModel = accountViewModel;
            }
            return response;
        }

        public InsertBulkOperationResponse InsertBulkOperation(InsertBulkOperationRequest request)
        {
            InsertBulkOperationResponse response = new InsertBulkOperationResponse();

            AdvancedSearchViewModel advancedSearchViewModel = JsonConvert.DeserializeObject<AdvancedSearchViewModel>(request.OperationData.AdvancedSearchCriteria);

            SearchDefinition searchDefinition = Mapper.Map<AdvancedSearchViewModel, SearchDefinition>(advancedSearchViewModel);
            if (searchDefinition != null && request.DrillDownContactIds == null)
            {
                searchDefinition.AccountID = request.AccountId;
                searchDefinition.CreatedBy = (int)request.RequestedBy;
                searchDefinition.CreatedOn = request.CreatedOn;
                searchDefinition.SelectAllSearch = true;
                searchDefinition.Id = 0;

                foreach (var filter in searchDefinition.Filters)
                {
                    filter.SearchFilterId = 0;

                }

                advancedSearchRepository.Insert(searchDefinition);
                SearchDefinition newSavedSearch = unitOfWork.Commit() as SearchDefinition;

                request.OperationData.SearchDefinitionID = newSavedSearch.Id;
            }

            request.OperationData.CreatedOn = request.CreatedOn;
            contactRepository.InsertBulkOperation(request.OperationData, request.DrillDownContactIds);

            return response;
        }

        public Account GetAccountMinDetails(int accountId)
        {
            return accountRepository.GetAccountMinDetails(accountId);
        }

        public void InsertBulkData(int[] contactIds, int bulkOperationId)
        {
            accountRepository.InsertBulkData(contactIds, bulkOperationId);
        }


        public GetSubscriptionSettingsResponse GetSubscriptionSettings(GetSubscriptionSettingsRequest request)
        {
            GetSubscriptionSettingsResponse response = new GetSubscriptionSettingsResponse();
            IEnumerable<SubscriptionSettings> settings = accountRepository.GetSubscriptionSettings(request.SubscriptionId, request.DomainUrl);
            response.SubscriptionSettings = settings;
            return response;
        }


        public GetBulkOperationDataResponse GetBulkOperationData(GetBulkOperationDataRequest request)
        {
            GetBulkOperationDataResponse response = new GetBulkOperationDataResponse();
            response.BulkOperations = accountRepository.GetBulkOperationData();
            if (response.BulkOperations != null)
            {
                response.BulkContactIDs = accountRepository.GetBulkContacts(response.BulkOperations.BulkOperationID);
            }
            return response;
        }

        public void DeleteBulkOperationData(int bulkOperationId)
        {
            accountRepository.DeleteBulkOperationData(bulkOperationId);
        }

        public UpdateBulkOperationStatusResponse UpdateBulkOperationStatus(UpdateBulkOperationStatusRequest request)
        {
            accountRepository.UpdateBulkOperationStatus(request.BulkOperationId, request.Status);
            return new UpdateBulkOperationStatusResponse();
        }

        public GetAccountAuthorizationResponse GetAccountByDomainUrl(GetAccountAuthorizationRequest request)
        {
            GetAccountAuthorizationResponse response = new GetAccountAuthorizationResponse();
            try
            {
                Logger.Current.Informational("The requested url is : " + request.name);
                Account accounts = accountRepository.FindByDomainUrl(request.name);
                if (accounts == null)
                {
                    response.Exception = GetAccountNotFoundException();
                }
                else
                {
                    AccountViewModel accountViewModel = Mapper.Map<Account, AccountViewModel>(accounts);
                    response.AccountId = accountViewModel.AccountID;
                    response.AccountName = accountViewModel.AccountName;
                    response.PrimaryEmail = accountViewModel.PrimaryEmail;
                    response.Status = accountViewModel.Status;
                    response.HelpURL = accountViewModel.HelpURL;
                    response.SubscriptionId = accountViewModel.SubscriptionId;
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("URL", request.name);
                Logger.Current.Error("An error occured while searching an account by domainurl. ", ex);
                response.Exception = new UnsupportedOperationException("The requested account was not found.");
            }
            return response;
        }

        public GetAccountsResponse GetAccounts()
        {
            GetAccountsResponse response = new GetAccountsResponse();
            IEnumerable<Account> accounts = accountRepository.FindAll();
            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                IEnumerable<AccountViewModel> list = Mapper.Map<IEnumerable<Account>, IEnumerable<AccountViewModel>>(accounts);
                response.Accounts = list;
            }
            return response;
        }

        public GetAccountResponse GetAccount(GetAccountRequest request)
        {
            GetAccountResponse response = new GetAccountResponse();
            Account account = null;
            account = accountRepository.GetAccount(request.Id, request.RequestBySTAdmin);
            if (account == null)
                response.Exception = GetAccountNotFoundException();
            else
            {
                AccountViewModel accountViewModel = Mapper.Map<Account, AccountViewModel>(account);
                if (accountViewModel.Image != null)
                {
                    if (!string.IsNullOrEmpty(accountViewModel.Image.StorageName))
                    {
                        switch (accountViewModel.Image.ImageCategoryID)
                        {
                            case ImageCategory.AccountLogo:
                                accountViewModel.Image.ImageContent = urlService.GetUrl(accountViewModel.AccountID, ImageCategory.AccountLogo, accountViewModel.Image.StorageName);
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }
                }
                else
                {
                    accountViewModel.Image = new ImageViewModel();
                }
                if (accountViewModel.Phones == null || !accountViewModel.Phones.Any())
                    accountViewModel.Phones.Add(new { PhoneType = "Work", PhoneNumber = "" });
                if (accountViewModel.SocialMediaUrls == null || !accountViewModel.SocialMediaUrls.Any())
                    accountViewModel.SocialMediaUrls.Add(new { MediaType = "Facebook", Url = "" });
                if (request.RequestBySTAdmin)
                {
                    if (accountViewModel.WebAnalyticsProvider == null)
                    {
                        Logger.Current.Informational("WebAnalytics Provider not configured to this account");
                        accountViewModel.WebAnalyticsProvider =
                                new WebAnalyticsProviderViewModel()
                                {
                                    AccountID = request.AccountId,
                                    CreatedBy = (int)request.RequestedBy,
                                    APIKey = "",
                                    LastUpdatedBy = (int)request.RequestedBy,
                                    LastUpdatedOn = DateTime.Now.ToUniversalTime(),
                                    StatusID = WebAnalyticsStatus.Inactive,
                                    CreatedOn = DateTime.Now.ToUniversalTime(),
                                    NotificationStatus = false,
                                    RequestInterval = 0,
                                    TrackingDomain = ""
                                };
                    }
                }
                else
                    account.WebAnalyticsProvider = null;
                response.AccountViewModel = accountViewModel;
            }
            return response;
        }

        public GetAddressResponse GetPrimaryAddress(GetAddressRequest request)
        {
            Logger.Current.Verbose("Requst received for fetching primary address of an account");
            GetAddressResponse response = new GetAddressResponse();
            AddressViewModel addressVM = new AddressViewModel();
            Address address = accountRepository.GetAddress(request.AccountId);
            if (address != null)
                addressVM = Mapper.Map<Address, AddressViewModel>(address);
            response.Address = addressVM.ToString();
            if (!string.IsNullOrEmpty(addressVM.City))
                response.Location = addressVM.City.ToString();
            return response;
        }

        public GetPrimaryPhoneResponse GetPrimaryPhone(GetPrimaryPhoneRequest request)
        {
            Logger.Current.Verbose("Request received for fetching account primary phone number");
            GetPrimaryPhoneResponse response = new GetPrimaryPhoneResponse();
            response.PrimaryPhone = accountRepository.GetPrimaryPhone(request.AccountId);
            return response;
        }

        public InsertAccountResponse InsertAccount(InsertAccountRequest request)
        {

            if (request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
                )
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };
            Account account = Mapper.Map<AccountViewModel, Account>(request.AccountViewModel);
            if (account.Addresses == null || !account.Addresses.Any())
                throw new UnsupportedOperationException("[|Add at least one Address|]");

            isAccountValid(account);

            var viewModel = request.AccountViewModel;

            if (!viewModel.Phones.Where(p => p.PhoneType == "Home" && p.PhoneNumber != "").Any() &&
               !viewModel.Phones.Where(p => p.PhoneType == "Work" && p.PhoneNumber != "").Any() &&
               !viewModel.Phones.Where(p => p.PhoneType == "Mobile" && p.PhoneNumber != "").Any())
            {
                throw new UnsupportedOperationException("[|Add at least one phone number|]");
            };

            if (viewModel.Phones.Where(p => p.PhoneType == "Home").Count() > 1 ||
                viewModel.Phones.Where(p => p.PhoneType == "Work").Count() > 1 ||
                viewModel.Phones.Where(p => p.PhoneType == "Mobile").Count() > 1)
            {
                throw new UnsupportedOperationException("[|Multiple phone numbers of similar type are not accepted.|]");
            };

            bool isDuplicate = accountRepository.IsDuplicateAccount(account.AccountName, 0, account.DomainURL);
            if (isDuplicate)
                throw new UnsupportedOperationException("[|Account Name or Domain URL already exists.|]");

            //byte subscriptionId = InsertCustomSubscription(account.AccountName);
            //account.SubscriptionID = 2;
            IEnumerable<Address> Addresses = account.Addresses;
            foreach (Address address in Addresses)
            {
                address.ApplyNation(contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode));
            }
            int AccountID = accountRepository.InsertAccount(account);
            Account newAccount = accountRepository.FindBy(AccountID);
            if (newAccount != null)
            {
                CreateLeadAdapterConfiguration(newAccount.Id, (int)request.RequestedBy);
                SaveImageResponse imageresponse = new SaveImageResponse();
                if (request.AccountViewModel.Image.ImageContent != null)
                {
                    imageresponse = imageService.SaveImage(new SaveImageRequest { ViewModel = request.AccountViewModel.Image, AccountId = newAccount.Id, ImageCategory = ImageCategory.AccountLogo });
                    if (imageresponse != null)
                        SaveAccountLogo(imageresponse.ImageViewModel);
                }
            }
            cacheService.AddOrUpdateAccount(newAccount.Id);
            AccountViewModel accountViewModel = Mapper.Map<Account, AccountViewModel>(newAccount);
            InsertAccountResponse accountResponse = new InsertAccountResponse();
            accountResponse.AccountViewModel = accountViewModel;
            CreateDomain(newAccount.DomainURL);
            IEnumerable<string> imageDomains = accountRepository.GetImageDomains(newAccount.Id);
            if (imageDomains.IsAny())
                AddImageDomainBindingss(imageDomains.ToArray(), newAccount.DomainURL);
            indexingService.SetupDynamicIndices(newAccount.Id);
            unitOfWork.Commit();
            return accountResponse;
        }

        //public List<ReportDataViewModel> GetAccountReportData()
        //{
        //    DataSet accountList = accountRepository.GetAccountReport();
        //    List<ReportDataViewModel> list = Mapper.Map<List<AccountHealthReport>, List<ReportDataViewModel>>(accountList);
        //    return list;
        //}

        void InsertDefaultCustomTab(int accountId)
        {
            InsertCustomFieldTabRequest request = new InsertCustomFieldTabRequest();
            CustomFieldTabViewModel tab = new CustomFieldTabViewModel();
            CustomFieldSectionViewModel section = new CustomFieldSectionViewModel();
            CustomFieldViewModel field = new CustomFieldViewModel();
            field.AccountID = accountId;
            field.Title = "Comments";
            field.DisplayName = field.Title;
            field.FieldInputTypeId = FieldType.textarea;
            field.IsCustomField = true;
            field.StatusId = FieldStatus.Active;

            section.CustomFields = new List<CustomFieldViewModel>();
            section.CustomFields.Add(field);
            section.Name = "Default";
            section.StatusId = CustomFieldSectionStatus.Active;

            tab.AccountId = accountId;
            tab.Name = "Default";
            tab.Sections = new List<CustomFieldSectionViewModel>();
            tab.Sections.Add(section);
            tab.StatusId = CustomFieldTabStatus.Active;
            request.CustomFieldTabViewModel = tab;

            customFieldService.InsertCustomFieldTab(request);
        }

        public void SaveAccountLogo(ImageViewModel image)
        {
            Image Image = Mapper.Map<ImageViewModel, Image>(image);
            accountRepository.SaveAccountLogo(Image);
        }
        void InsertPreConfiguredSearches(int accountID, int userId)
        {
            IEnumerable<SearchDefinition> Defaultsearches = advancedSearchRepository.FindAllDefault();
            foreach (var Searchdefination in Defaultsearches)
            {
                SearchDefinition searchDefination = new SearchDefinition
                {
                    AccountID = accountID,
                    Filters = Searchdefination.Filters,
                    IsPreConfiguredSearch = true,
                    CreatedBy = userId,
                    IsFavoriteSearch = false,
                    Name = Searchdefination.Name,
                    PredicateType = Searchdefination.PredicateType,
                    CustomPredicateScript = Searchdefination.CustomPredicateScript,
                    TagsList = Searchdefination.TagsList == null ? new List<Tag>() : Searchdefination.TagsList,
                    ElasticQuery = Searchdefination.ElasticQuery,
                    CreatedOn = System.DateTime.UtcNow.Date,
                    SelectedColumns = Searchdefination.SelectedColumns,
                    Fields = Searchdefination.Fields
                };
                advancedSearchRepository.Insert(searchDefination);
                unitOfWork.Commit();
            }

        }
        void InsertDefaultCommunicationProviders(int AccountId, int userId)
        {
            IEnumerable<ServiceProvider> Serviceproviderlist = serviceproviderRepository.AccountServiceProviders(1);
            CMO.MailService mailService = new CMO.MailService();
            foreach (ServiceProvider provider in Serviceproviderlist)
            {
                var registration = mailService.GetMailRegistrationDetails(provider.LoginToken);
                var MailRegisterRequest = new CMR.RegisterMailRequest
                {
                    APIKey = registration.APIKey,
                    MailProviderID = registration.MailProviderID,
                    Host = registration.Host,
                    Name = registration.Name,
                    UserName = registration.UserName,
                    Password = registration.Password,
                    RequestGuid = Guid.NewGuid(),
                    IsSSLEnabled = true,
                    Port = registration.Port,
                    VMTA = registration.VMTA,
                    SenderDomain = registration.SenderDomain,
                };
                if (MailRegisterRequest.MailProviderID == CMR.MailProvider.SendGrid || MailRegisterRequest.MailProviderID == CMR.MailProvider.SmartTouch)
                {
                    var response = MailRequestConverterExtensions.EmailRegistrationRequest(MailRegisterRequest);
                    if (response != null)
                    {
                        provider.CreatedBy = userId;
                        provider.AccountId = AccountId;
                        //provider.IsDefault = provider.IsDefault;
                        provider.LoginToken = response.Token;
                        serviceproviderRepository.Insert(provider);
                        unitOfWork.Commit();
                    }
                }
            }//foreach end
            var tempMailchimpregisterRequest = new CMR.RegisterMailRequest
            {
                APIKey = "",
                MailProviderID = LandmarkIT.Enterprise.CommunicationManager.Requests.MailProvider.MailChimp,
                Host = "",
                Name = "",
                UserName = "",
                Password = "",
                RequestGuid = Guid.NewGuid(),
                IsSSLEnabled = true,
                Port = 0,
                VMTA = ""
            };
            var mailResponse = MailRequestConverterExtensions.EmailRegistrationRequest(tempMailchimpregisterRequest);
            if (mailResponse != null)
            {
                ServiceProvider mailchimpProvider = new ServiceProvider();
                mailchimpProvider.CommunicationTypeID = CommunicationType.Mail;
                mailchimpProvider.MailType = MailType.BulkEmail;
                mailchimpProvider.CreatedBy = userId;
                mailchimpProvider.CreatedDate = DateTime.UtcNow;
                mailchimpProvider.ProviderName = "MailChimp";
                mailchimpProvider.AccountId = AccountId;
                mailchimpProvider.IsDefault = false;
                mailchimpProvider.LoginToken = mailResponse.Token;
                serviceproviderRepository.Insert(mailchimpProvider);
                unitOfWork.Commit();
            }

        }

        void CreateDomain(string domainName)
        {
            if (domainName != null)
            {
                Logger.Current.Informational("Creating sub-domain with name as : " + domainName);

                string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];
                string sslKey = System.Configuration.ConfigurationManager.AppSettings["SSLSerialNumber"];

                var port = 80;

                var serverManager = new ServerManager();
                var smartTouchSite = serverManager.Sites[smartTouchSiteName];

                smartTouchSite.Bindings.Add(string.Format("*:{0}:{1}", port, domainName), "http");
                Logger.Current.Informational("Added http binding successfully.");

                bool isHttpsMode = false;
                string httpsMode = System.Configuration.ConfigurationManager.AppSettings["IsHttpsMode"];
                bool.TryParse(httpsMode, out isHttpsMode);

                if (isHttpsMode)
                {
                    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);

                    var certificate = store.Certificates.Find(X509FindType.FindBySerialNumber, sslKey, false).OfType<X509Certificate>().FirstOrDefault();
                    if (certificate != null)
                    {
                        Logger.Current.Informational("certificate found");
                    }
                    else
                    {
                        Logger.Current.Informational("certificate not found");
                        foreach (X509Certificate2 objCert in store.Certificates)
                        {
                            string serialNumber = objCert.SerialNumber.Trim().ToString().ToUpper();
                            Logger.Current.Verbose("Certificate name" + objCert.FriendlyName + " Store serial number:" + objCert.SerialNumber.Trim());
                            string orgSerialNumber = sslKey.Trim().ToString().ToUpper();
                            if (String.Equals(serialNumber, orgSerialNumber, StringComparison.InvariantCulture))
                            {
                                certificate = objCert;
                            }
                        }
                        if (certificate != null)
                        {
                            Logger.Current.Informational("Certificate found.");
                        }
                        else
                            Logger.Current.Informational("Certificate not found.");
                    }
                    if (certificate != null)
                    {
                        smartTouchSite.Bindings.Add("*:443:" + domainName, certificate.GetCertHash(), store.Name);
                        Logger.Current.Informational("Added https binding successfully.");
                    }
                    else
                        Logger.Current.Informational("Https binding unsuccessful.");

                    store.Close();
                }
                smartTouchSite.ServerAutoStart = false;
                serverManager.CommitChanges();
                Logger.Current.Informational("Sub-domain creation was successful for https protocol");
            }
        }
        /// <summary>
        /// This method is used to add campaign link, 
        /// all links are loaded with the {accountcode}.{imagedomain}.com
        /// </summary>
        /// <param name="domainNames"></param>
        /// <param name="accountCode"></param>
        void AddImageDomainBindingss(string[] domainNames, string accountCode)
        {
            string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];
            if (domainNames.IsAny())
            {
                foreach (string domainName in domainNames)
                {
                    Logger.Current.Informational("Adding Image sub-domain with domain name : " + domainName + " account : " + accountCode);
                    int count = 0;
                    var port = 80;
                    var domainNameWithOutProtocol = domainName.Replace("http://", "").Replace("https://", "").Replace("www.", "");
                    //check if given domain name is sub-domain. if it is already sub domain, we shouldn't add binding.
                    foreach (char c in domainNameWithOutProtocol)
                        if (c == '.') count++;
                    if (count == 1)
                    {
                        try
                        {
                            var serverManager = new ServerManager();
                            var smartTouchSite = serverManager.Sites[smartTouchSiteName];

                            accountCode = accountCode.Split('.').FirstOrDefault();
                            Logger.Current.Informational("Creating sub-domain with name as : " + domainNameWithOutProtocol);
                            smartTouchSite.Bindings.Add(string.Format("*:{0}:{1}.{2}", port, accountCode, domainNameWithOutProtocol), "http");
                            smartTouchSite.ServerAutoStart = false;
                            serverManager.CommitChanges();
                            Logger.Current.Informational("Added " + string.Format("*:{0}:{1}.{2}", port, accountCode, domainNameWithOutProtocol) + " binding successfully.");
                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Error("Error while adding Image binding " + domainNameWithOutProtocol, ex);
                        }
                    }
                }
            }

        }

        void RemoveImageBindings(string[] domainNames, string oldAccountURL)
        {
            foreach (string domainName in domainNames)
            {
                if (domainName != null)
                {
                    Logger.Current.Informational("Removing image sub-domain with name : " + domainName + " and url: " + oldAccountURL);

                    string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];
                    var port = 80;

                    try
                    {
                        var domainNameWithOutProtocol = domainName.Replace("http://", "").Replace("https://", "").Replace("www.", "");
                        var serverManager = new ServerManager();
                        var smartTouchSite = serverManager.Sites[smartTouchSiteName];
                        oldAccountURL = oldAccountURL.Split('.').FirstOrDefault();
                        Binding httpBinding = smartTouchSite.Bindings.SingleOrDefault(b => b.BindingInformation.Equals(string.Format("*:{0}:{1}.{2}", port, oldAccountURL, domainNameWithOutProtocol)));

                        if (httpBinding != null)
                            smartTouchSite.Bindings.Remove(httpBinding);

                        smartTouchSite.ServerAutoStart = false;
                        serverManager.CommitChanges();
                        Logger.Current.Informational("Image domain removed successful.");
                    }
                    catch(Exception ex)
                    {
                        Logger.Current.Error("Error logging while removing imagedomain:", ex);
                    }
                        
                }
            }
        }

        public static string GetNonNumericData(string input)
        {
            Regex regex = new Regex("[^a-zA-Z0-9]");
            string output = regex.Replace(input, "");
            if (output.Length > 20)
                return "";
            return regex.Replace(input, "");
        }

        void RemoveDomain(string domainName)
        {
            if (domainName != null)
            {
                Logger.Current.Informational("Removing sub-domain with name : " + domainName);

                string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];
                string sslKey = System.Configuration.ConfigurationManager.AppSettings["SSLSerialNumber"];

                var port = 80;

                var serverManager = new ServerManager();
                var smartTouchSite = serverManager.Sites[smartTouchSiteName];

                Binding httpBinding = smartTouchSite.Bindings.SingleOrDefault(b => b.BindingInformation.Equals(string.Format("*:{0}:{1}", port, domainName)));
                Binding httpsBinding = smartTouchSite.Bindings.SingleOrDefault(b => b.BindingInformation.Equals(string.Format("*:{0}:{1}", 443, domainName)));

                if (httpBinding != null)
                    smartTouchSite.Bindings.Remove(httpBinding);
                if (httpsBinding != null)
                    smartTouchSite.Bindings.Remove(httpsBinding);

                bool isHttpsMode = false;
                string httpsMode = System.Configuration.ConfigurationManager.AppSettings["IsHttpsMode"];
                bool.TryParse(httpsMode, out isHttpsMode);

                if (isHttpsMode)
                {
                    X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                    store.Open(OpenFlags.ReadOnly);

                    var certificate = store.Certificates.Find(X509FindType.FindBySerialNumber, sslKey, false).OfType<X509Certificate>().FirstOrDefault();
                    if (certificate != null)
                    {
                        Logger.Current.Informational("certificate found");
                    }
                    else
                    {
                        Logger.Current.Informational("certificate not found");
                        foreach (X509Certificate2 objCert in store.Certificates)
                        {
                            string serialNumber = objCert.SerialNumber.Trim().ToString().ToUpper();
                            Logger.Current.Informational("Certificate name" + objCert.FriendlyName + " Store serial number:" + objCert.SerialNumber.Trim());
                            string orgSerialNumber = sslKey.Trim().ToString().ToUpper();
                            if (String.Equals(serialNumber, orgSerialNumber, StringComparison.InvariantCulture))
                            {
                                certificate = objCert;
                            }
                        }
                        if (certificate != null)
                        {
                            Logger.Current.Informational("Certificate found.");
                        }
                        else
                            Logger.Current.Informational("Certificate not found.");
                    }

                    store.Close();

                    foreach (var binding in smartTouchSite.Bindings)
                    {
                        if (binding.Protocol.Equals("https") && !binding.BindingInformation.Equals(string.Format("*:{0}:{1}", 443, domainName)))
                        {
                            binding.CertificateHash = certificate.GetCertHash();
                            break;
                        }
                    }
                }

                smartTouchSite.ServerAutoStart = false;
                serverManager.CommitChanges();
                Logger.Current.Informational("Sub-domain removal was successful.");
            }
        }

        public void UpdateDomain(string oldDomain, string newDomain)
        {
            if (newDomain != null && oldDomain != null)
            {
                Logger.Current.Verbose("Request received for updating domain-url");
                Logger.Current.Informational("Request received for creating new domain with name as : " + newDomain);
                string smartTouchSiteName = System.Configuration.ConfigurationManager.AppSettings["SmarttouchSiteName"];

                var port = 80;
                var serverManager = new ServerManager();
                var smartTouchSite = serverManager.Sites[smartTouchSiteName];

                Binding httpBinding = smartTouchSite.Bindings.SingleOrDefault(b => b.BindingInformation.Equals(string.Format("*:{0}:{1}", port, newDomain)));
                Binding httpsBinding = smartTouchSite.Bindings.SingleOrDefault(b => b.BindingInformation.Equals(string.Format("*:{0}:{1}", 443, newDomain)));

                if (httpBinding != null || httpsBinding != null)
                    throw new UnsupportedOperationException("[|Domain already exist with this name. Please choose another url|]");
                else
                {
                    RemoveDomain(oldDomain);
                    CreateDomain(newDomain);
                }
            }
        }

        private void UpdateImageDomains(string[] domainNames, string oldURL, string newURL)
        {
            RemoveImageBindings(domainNames, oldURL);
            AddImageDomainBindingss(domainNames, newURL);
        }

        public UpdateAccountResponse UpdateAccount(UpdateAccountRequest request)
        {

            if (request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
                request.AccountViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
                )
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };

            Account account = Mapper.Map<AccountViewModel, Account>(request.AccountViewModel);
            if (account.Addresses == null || !account.Addresses.Any())
                throw new UnsupportedOperationException("[|Add at least one Address|]");

            isAccountValid(account);
            var viewModel = request.AccountViewModel;

            if (!viewModel.Phones.Where(p => p.PhoneType == "Home" && p.PhoneNumber != "").Any() &&
               !viewModel.Phones.Where(p => p.PhoneType == "Work" && p.PhoneNumber != "").Any() &&
               !viewModel.Phones.Where(p => p.PhoneType == "Mobile" && p.PhoneNumber != "").Any())
            {
                throw new UnsupportedOperationException("[|Add at least one phone number|]");
            };

            if (viewModel.Phones.Count(p => p.PhoneType == "Home") > 1 ||
                viewModel.Phones.Count(p => p.PhoneType == "Work") > 1 ||
                viewModel.Phones.Count(p => p.PhoneType == "Mobile") > 1)
            {
                throw new UnsupportedOperationException("[|Multiple phone numbers of similar type are not accepted.|]");
            }

            if (accountRepository.IsDuplicateAccount(account.AccountName, account.Id, account.DomainURL))
                throw new UnsupportedOperationException("[|Account already exists with same account name or domain-url.|]");

            if (request.AccountViewModel.DomainURL != request.AccountViewModel.PreviousDomainURL)
                UpdateDomain(request.AccountViewModel.PreviousDomainURL, request.AccountViewModel.DomainURL);

            if (!request.IsSettingsUpdate)
            {
                int existingUsersCount = accountRepository.GetUsersCount(account.Id, request.AccountViewModel.SelectedRoles);
                if (existingUsersCount > request.AccountViewModel.UserLimit)
                    throw new UnsupportedOperationException("[|There are already more Active Users than the specified count|]");
            }
            //if (request.AccountViewModel.UserLimit != request.AccountViewModel.PreviousUserLimit)
            accountRepository.UpdateUserLimit(account.Id, request.AccountViewModel.UserLimit, request.AccountViewModel.SelectedRoles);

            IEnumerable<Address> Addresses = account.Addresses;
            foreach (Address address in Addresses)
            {
                address.ApplyNation(contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode));
            }

            accountRepository.Update(account);
            Account updatedAccount = unitOfWork.Commit() as Account;
            cacheService.AddOrUpdateAccount(updatedAccount.Id);
            SaveImageResponse imageresponse = new SaveImageResponse();

            if (viewModel.Image.ImageContent != null)
            {
                imageresponse = imageService.SaveImage(new SaveImageRequest { ViewModel = viewModel.Image, AccountId = updatedAccount.Id, ImageCategory = ImageCategory.AccountLogo });
                if (imageresponse != null)
                    SaveAccountLogo(imageresponse.ImageViewModel);
            }
            IEnumerable<string> imageDomains = accountRepository.GetImageDomains(account.Id);
            if (request.AccountViewModel.DomainURL != request.AccountViewModel.PreviousDomainURL && imageDomains.IsAny())
            {
                UpdateImageDomains(imageDomains.ToArray(), request.AccountViewModel.PreviousDomainURL, request.AccountViewModel.DomainURL);
            }
            else
            {
                var defaultProviderImageDomain = request.AccountViewModel.ServiceProviderRegistrationDetails.Where(sv => sv.IsDefault == true && sv.CommunicationType == CommunicationType.Mail).Select(s => s.ImageDomainId).ToList();
                if (defaultProviderImageDomain.IsAny())
                {
                    List<byte> imgDmIds = new List<byte>() { };
                    defaultProviderImageDomain.Each(s =>
                    {
                        if (s.HasValue && s.Value > 0)
                            imgDmIds.Add(s.Value);
                    });
                    string[] imageDms = accountRepository.GetImageDoaminsById(imgDmIds);
                    AddImageDomainBindingss(imageDms, request.AccountViewModel.DomainURL);
                }
            }

            List<ModuleViewModel> moduleViewModel = ArrangeSubModules(viewModel, true);
            //if (isSubscriptionChanged)
            //{ 
            //accountRepository.isSubscriptionChange
            //}


            InsertSubscriptionPermissions(updatedAccount.SubscriptionID, moduleViewModel, viewModel.AccountID);

            InsertDataSharingPermissions(updatedAccount.Id, request.AccountViewModel, updatedAccount.SubscriptionID);

            string detail = request.AccountViewModel.AccountName + " " + "[|account was updated|]";
            string subject = request.AccountViewModel.AccountName + " " + "[|account updated successfully|]";
            InsertNotification(updatedAccount.Id, detail, subject);
            return new UpdateAccountResponse();

        }


        //AccountViewModel updateViewModel(AccountViewModel viewModel)
        //{
        //    var emptyAddresses = viewModel.Addresses.Where(a => a.AddressID == 0
        //        && string.IsNullOrEmpty(a.AddressLine1) && string.IsNullOrEmpty(a.AddressLine2)
        //        && string.IsNullOrEmpty(a.City) && string.IsNullOrEmpty(a.ZipCode)
        //        //&& (a.Country == null || a.Country.Code.Equals("US") || a.Country.Code.Equals(""))
        //        && (a.State == null || string.IsNullOrEmpty(a.State.Code) || a.State.Code.Equals(""))).ToList();
        //    foreach (AddressViewModel addressViewModel in emptyAddresses)
        //    {
        //        viewModel.Addresses.Remove(addressViewModel);
        //    }
        //    return viewModel;
        //}

        public DeleteAccountResponse DeleteAccount(DeleteAccountRequest request)
        {
            Account account = accountRepository.FindBy(request.Id);
            if (account != null)
            {
                accountRepository.Delete(account);
                unitOfWork.Commit();

                RemoveDomain(account.DomainURL);
                string detail = "[|An account was deleted|]";
                string subject = "[|Account deleted successfully|]";
                InsertNotification(request.AccountId, detail, subject);
                return new DeleteAccountResponse();
            }
            else
            {
                return new DeleteAccountResponse() { Exception = GetAccountNotFoundException() };
            }
        }

        private UnsupportedOperationException GetAccountNotFoundException()
        {
            throw new UnsupportedOperationException("The requested account was not found.");
        }

        void isAccountValid(Account account)
        {
            IEnumerable<BusinessRule> brokenRules = account.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules)
                {
                    brokenRulesBuilder.AppendLine(rule.RuleDescription);
                }

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public AccountStatusUpdateResponse UpdateAccountStatus(AccountStatusUpdateRequest request)
        {
            Logger.Current.Verbose("Updating the account statuses.");
            string detail = string.Empty;
            string subject = string.Empty;
            AccountStatusUpdateResponse accountStatusUpdateResponse = new AccountStatusUpdateResponse();
            accountStatusUpdateResponse.Toemails = accountRepository.UpdateAccountStatus(request.AccountID, request.StatusID);

            foreach (var accountId in request.AccountID)
            {
                Account account = accountRepository.FindBy(accountId);

                //var accountIdIndex = request.AccountID.Select((id, index) => new { ID = id, Index = index }).Where(s => s.ID.Equals(accountId)).Select(x => x.Index).FirstOrDefault();
                var accountName = account.AccountName;
                if (request.StatusID == (byte)AccountStatus.Terminate)   //This status is also considered as Paused
                {
                    Logger.Current.Verbose("Updated account " + accountId + "status to terminate.");
                    detail = accountName + " " + "[|account was paused|]";
                    subject = accountName + " " + "[|account paused successfully|]";
                    InsertNotification(accountId, detail, subject);
                }
                else if (request.StatusID == (byte)AccountStatus.Suspend)   //Suspend => Closed
                {
                    Logger.Current.Verbose("Updated account " + accountId + "status to suspend.");
                    detail = accountName + " " + "[|account was closed|]";
                    subject = accountName + " " + "[|account closed successfully|]";
                    InsertNotification(accountId, detail, subject);
                }
                else if (request.StatusID == (byte)AccountStatus.Maintanance)   //Suspend => Closed
                {
                    Logger.Current.Verbose("Updated account " + accountId + "status to maintanance.");
                    detail = accountName + " " + "[|account was under maintanance|]";
                    subject = accountName + " " + "[|account was under maintanace|]";
                    InsertNotification(accountId, detail, subject);
                }
                else if (request.StatusID == (byte)AccountStatus.Delete)
                {
                    Logger.Current.Verbose("Updated account " + accountId + "status to delete.");
                    RemoveDomain(account.DomainURL);
                    detail = accountName + " " + "[|account was deleted|]";
                    subject = accountName + " " + "[|account deleted successfully|]";
                    InsertNotification(accountId, detail, subject);
                }
            }
            return accountStatusUpdateResponse;
        }

        byte InsertCustomSubscription(string accountName)
        {
            Subscription subscription = new Subscription();
            subscription.SubscriptionName = accountName + " " + "Subscription";
            subscriptionRepository.Insert(subscription);
            Subscription newSubscription = unitOfWork.Commit() as Subscription;
            if (newSubscription != null)
            {
                return newSubscription.Id;
            }
            else return 0;
        }

        void InsertSubscriptionPermissions(byte subscriptionId, List<ModuleViewModel> moduleslist, int accountId)
        {
            Logger.Current.Informational("Inserting subscription-permissions for an account : " + accountId);
            List<ModuleViewModel> modulesViewModel = moduleslist;
            List<byte> modules = modulesViewModel.Where(s => s.IsSelected == true).GroupBy(s => s.ModuleId).Select(y => y.First().ModuleId).ToList();
            subscriptionRepository.InsertSubscriptionPermissions(subscriptionId, modules, accountId);
        }

        void InsertDataSharingPermissions(int accountId, AccountViewModel model, byte subscriptionId)
        {
            Logger.Current.Informational("Inserting data-sharing permissions for an account : " + accountId);
            List<ModuleViewModel> modulesViewModel = ArrangeSubModules(model, false);
            List<byte> modules = modulesViewModel.Where(s => s.IsPrivate == true).GroupBy(s => s.ModuleId).Select(y => y.First().ModuleId).ToList();
            accountRepository.InsertDataSharingPermissions(accountId, modules, subscriptionId);
        }

        void CreateLeadAdapterConfiguration(int accountId, int requestedBy)
        {
            string LeadAdapterPhysicalPath = ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"];
            leadAdaptersRepository.CreateLeadAdapterFolders(accountId.ToString(), LeadAdapterPhysicalPath);
            LeadAdapterAndAccountMap leadadapter = new LeadAdapterAndAccountMap();
            leadadapter.AccountID = accountId;
            leadadapter.ArchivePath = Path.Combine(LeadAdapterPhysicalPath, accountId.ToString(), "Import", "Archive");
            leadadapter.BuilderNumber = "";
            leadadapter.CreatedBy = requestedBy;
            leadadapter.CreatedDateTime = DateTime.Now.ToUniversalTime();
            leadadapter.LeadAdapterCommunicationTypeID = LeadAdapterCommunicationType.FTP;
            leadadapter.LeadAdapterTypeID = LeadAdapterTypes.Import;
            leadadapter.LocalFilePath = Path.Combine(LeadAdapterPhysicalPath, accountId.ToString(), "Import", "Local");
            leadadapter.ModifiedBy = requestedBy;
            leadadapter.ModifiedDateTime = DateTime.Now.ToUniversalTime();
            leadadapter.Password = "";
            leadadapter.Port = 0;
            leadadapter.RequestGuid = Guid.NewGuid();
            leadadapter.Url = "";
            leadadapter.UserName = "";
            leadAdaptersRepository.Insert(leadadapter);
        }

        void InsertNotification(int accountId, string detail, string subject)
        {
            Notification notificationdata = new Notification();
            notificationdata.Details = detail;
            notificationdata.EntityId = accountId;
            notificationdata.Subject = subject;
            notificationdata.Time = DateTime.Now.ToUniversalTime();
            notificationdata.Status = NotificationStatus.New;
            // notificationdata.UserID = createdBy;
            notificationdata.ModuleID = (byte)AppModules.Accounts;
            userRepository.AddBulkNotifications(notificationdata);
        }

        List<ModuleViewModel> ArrangeSubModules(AccountViewModel model, bool subscriptionScreen)
        {
            List<ModuleViewModel> modulesViewModel = new List<ModuleViewModel>();
            if (subscriptionScreen)
                modulesViewModel = model.Modules.ToList();
            else
                modulesViewModel = model.SubscribedModules.ToList();
            List<ModuleViewModel> subModules = new List<ModuleViewModel>();
            foreach (var viewModel in modulesViewModel)
            {
                if (viewModel.SubModules.Any())
                {
                    subModules.AddRange(viewModel.SubModules);
                }
            }
            modulesViewModel.AddRange(subModules);
            return modulesViewModel;
        }

        public GetDataAccessPermissionsResponse GetSharingPermissions(GetDataAccessPermissionsRequest request)
        {
            GetDataAccessPermissionsResponse response = new GetDataAccessPermissionsResponse();
            IEnumerable<byte> moduleIds;
            if (request != null)
            {
                moduleIds = accountRepository.GetPrivateModules(request.accountId);
                if (moduleIds.Any())
                {
                    foreach (var moduleId in moduleIds)
                    {
                        var matched = request.Modules.Where(s => s.ModuleId == moduleId || s.SubModules.Any(c => c.ModuleId == moduleId)).FirstOrDefault();
                        if (matched != null)
                            matched.IsPrivate = true;
                    }
                }
                response.Modules = request.Modules;
            }
            return response;
        }

        public GetModuleSharingPermissionResponse GetModuleSharingPermission(GetModuleSharingPermissionRequest request)
        {
            GetModuleSharingPermissionResponse response = new GetModuleSharingPermissionResponse();
            response.DataSharing = accountRepository.GetModuleSharingPermission(request.ModuleId, request.AccountId);
            return response;
        }

        public GetPrivateModulesResponse GetPrivateModules(GetPrivateModulesRequest request)
        {
            GetPrivateModulesResponse response = new GetPrivateModulesResponse();
            Logger.Current.Verbose("Request to fetch private modules of an account");
            Logger.Current.Informational("AccountId : " + request.AccountId);
            IEnumerable<byte> ModuleIDs = null;
            if (request != null)
            {
                ModuleIDs = accountRepository.GetPrivateModules(request.AccountId);
            }
            response.ModuleIds = ModuleIDs;

            return response;
        }

        public AccountViewModel CopyAccount(AccountViewModel model)
        {
            if (model != null)
            {
                model.AccountID = 0;
                if (model.Addresses.Count == 0)
                {
                    model.Addresses = new List<AddressViewModel>() { new AddressViewModel() { AddressID = 0, AddressTypeID = 4, Country = new Country { Code = "" }, State = new State { Code = "" } } };
                }
                model.AccountID = 0;
                model.FirstName = "";
                model.LastName = "";
                model.PrimaryEmail = null;
                model.Status = (byte)AccountStatus.Draft;
                model.DomainURL = "";
                model.Image = null;
                model.LogoImageID = null;
                return model;
            }
            else return null;
        }

        public GetAccountPermissionsResponse GetAccountPermissions(GetAccountPermissionsRequest request)
        {
            GetAccountPermissionsResponse response = new GetAccountPermissionsResponse();
            Logger.Current.Verbose("Request to fetch Account Permissions");
            Logger.Current.Informational("AccountId : " + request.AccountId);
            IEnumerable<byte> ModuleIDs = null;
            if (request != null)
            {
                ModuleIDs = accountRepository.GetAccountPermissions(request.AccountId);
            }
            response.ModuleIds = ModuleIDs;
            return response;
        }

        public void SendDailySummaryEmails(GetDailySummaryEmailsRequest emailsRequest)
        {
            try
            {
                var tokenGuid = new Guid();
                Logger.Current.Verbose("Request received to send Daily-Summary emails in Account service");
                DateTime date = DateTime.Now;
                date.AddDays(-1);
                date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
                var dailySummaryEmails = accountRepository.GetDailySummaryEmails();
                if (dailySummaryEmails.Count > 0)
                {
                    foreach (var summaryEmail in dailySummaryEmails)
                    {
                        try
                        {
                            Email senderEmail = new Email();
                            var status = accountRepository.GetAccountStatus(summaryEmail.AccountID);
                            if (status != AccountStatus.Suspend)
                            {
                                tokenGuid = GetAccountEmailProvider(summaryEmail.AccountID);
                                ServiceProviderViewModel serviceProviderViewModel = serviceProviderService.GetAccountServiceProviders(new GetServiceProviderRequest()
                                {
                                    CommunicationTypeId = CommunicationType.Mail,
                                    AccountId = summaryEmail.AccountID,
                                    MailType = MailType.TransactionalEmail
                                }).ServiceProviderViewModel.FirstOrDefault();
                                if (serviceProviderViewModel != null)
                                    senderEmail = serviceproviderRepository.GetServiceProviderEmail(serviceProviderViewModel.CommunicationLogID);

                                var address = summaryEmail.PrimaryAddress;
                                var addressVM = new AddressViewModel();
                                if (address != null)
                                    addressVM = Mapper.Map<Address, AddressViewModel>(address);
                                if (summaryEmail.Users.Count > 0)
                                {
                                    foreach (var user in summaryEmail.Users)
                                    {
                                        try
                                        {

                                            string fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? summaryEmail.AccountEmail : senderEmail.EmailId;
                                            MailService mailService = new MailService();
                                            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest request = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                                            request.TokenGuid = tokenGuid;
                                            request.RequestGuid = Guid.NewGuid();
                                            request.ScheduledTime = DateTime.Now.ToUniversalTime();
                                            request.Subject = "SmartTouch Daily Summary - " + summaryEmail.AccountName + ":" + user.UserName + " - " + date;
                                            request.Body = GenerateSummaryEmailBody(user.UserSettings, summaryEmail.DomainURL, summaryEmail.AccountName,
                                                user.UserName, user.UserEmail, addressVM.ToString(), summaryEmail.PrimaryPhone, summaryEmail.AccountID);
                                            request.To = new List<string>() { user.UserEmail };
                                            request.IsBodyHtml = true;
                                            request.From = fromEmail;
                                            request.AccountDomain = summaryEmail.DomainURL;
                                            request.AccountID = summaryEmail.AccountID;
                                            mailService.SendAsync(request);
                                            accountRepository.InsertDailySummaryEmailAudit(user.UserId, 1);
                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Current.Informational("Error occured in daily-smmary email for user with Id :" + user.UserId);
                                            Logger.Current.Error("An error occured while sending daily-summary email :" + ex);
                                            accountRepository.InsertDailySummaryEmailAudit(user.UserId, 2);
                                            continue;
                                        }
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Current.Informational("An error occured with email providers for account :" + summaryEmail.AccountID);
                            Logger.Current.Error("An error occured while getting email providers for account :", ex);
                            continue;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while sending Daily-Summary emails : " + ex);
            }
        }

        public Dictionary<int, string> GetAllAccountsIds()
        {
            return accountRepository.GetAllAccountsIds();
        }

        public List<ContactGroup> GetAllContactsByAccount(int accountid)
        {
            HttpWebResponse response;
            List<ContactGroup> elasticContacts = new List<ContactGroup>();

            List<ContactAccountGroup> result = accountRepository.GetContactCampaignByAccount(accountid);

            Dictionary<int, string> accountIDs = accountRepository.GetAllAccountsIds();
            foreach (KeyValuePair<int, string> accountID in accountIDs)
            {
                bool isError = false;
                try
                {
                    string str = ConfigurationManager.AppSettings["ELASTICSEARCH_INSTANCE"];
                    UriBuilder builder = new UriBuilder(str);
                    builder.Path += "contacts" + accountID.Key + "/people,companies,contacts/_count";
                    Uri resultUri = builder.Uri;
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(resultUri);
                    request.Headers.Add("Translate: f");
                    request.Credentials = CredentialCache.DefaultCredentials;
                    request.Method = "GET";
                    request.ContentType = "text/html";
                    //if (request.GetResponse() as )
                    response = request.GetResponse() as HttpWebResponse;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        string strOutputXml = reader.ReadLine();
                        dynamic c = Newtonsoft.Json.Linq.JObject.Parse(strOutputXml);
                        elasticContacts.Add(new ContactGroup() { AccountID = accountID.Key, ElasticCount = c.count, AccountName = accountID.Value });
                    }
                }

                catch (WebException)
                {
                    isError = true;
                }
                if (isError) continue;
            }

            IEnumerable<ContactGroup> list = Mapper.Map<IEnumerable<ContactAccountGroup>, IEnumerable<ContactGroup>>(result);

            List<ContactGroup> contactsList = elasticContacts.Union(list).ToList();
            contactsList.ForEach(c =>
            {
                c.ContactsCount = list.Where(l => l.AccountID == c.AccountID).Select(l => l.ContactsCount).FirstOrDefault();
                c.ElasticCount = elasticContacts.Where(l => l.AccountID == c.AccountID).Select(l => l.ElasticCount).FirstOrDefault();
            });
            var Alllist = contactsList.Where(c => c.ElasticCount != 0 || c.ElasticCount != null).ToList();

            return Alllist;
        }

        public AccountHealthReport GetAccountReportData()
        {

            AccountHealthReport accountHealthReport = new AccountHealthReport();
            List<HealthFormReport> Forms = new List<HealthFormReport>();
            List<HealthCampaignReport> CampaignsList = new List<HealthCampaignReport>();
            List<HealthEmailsReport> EmailsList = new List<HealthEmailsReport>();
            List<HealthWorkflowReport> HealthWorkflowReportList = new List<HealthWorkflowReport>();
            List<CampaignSent> CampaignSentList = new List<CampaignSent>();
            List<FailedImports> FailedImportsList = new List<FailedImports>();
            List<SucceededImports> SucceededImportsList = new List<SucceededImports>();
            List<InProgressImport> InProgressImportList = new List<InProgressImport>();
            List<FailedLeadAdapter> FailedLeadAdapterList = new List<Domain.Accounts.FailedLeadAdapter>();
            List<SucceededLeadAdapters> SucceededLeadAdaptersList = new List<Domain.Accounts.SucceededLeadAdapters>();
            List<ContactLeadScore> ContactLeadScoreList = new List<Domain.Accounts.ContactLeadScore>();


            DataSet accountList = accountRepository.GetAccountReport();
            DataTable dt0 = accountList.Tables[0];
            DataTable dt1 = accountList.Tables[1];
            DataTable dt2 = accountList.Tables[2];
            DataTable dt3 = accountList.Tables[3];
            DataTable dt4 = accountList.Tables[4];
            DataTable dt5 = accountList.Tables[5];
            DataTable dt6 = accountList.Tables[6];
            DataTable dt7 = accountList.Tables[7];
            DataTable dt8 = accountList.Tables[8];
            DataTable dt9 = accountList.Tables[9];
            DataTable dt10 = accountList.Tables[10];


            HealthFormReport healthReport;
            HealthCampaignReport campaign;
            HealthEmailsReport emails;

            HealthWorkflowReport workFlow;
            CampaignSent campaignSent;
            SucceededImports succeededImports;
            InProgressImport inProgressImport;
            FailedLeadAdapter failedLeadAdapter;
            SucceededLeadAdapters succeededLeadAdapters;
            ContactLeadScore contactLeadScore;
            FailedImports failedImport;


            foreach (DataRow datarow in dt0.Rows)
            {
                campaign = new HealthCampaignReport();
                campaign.AccountID = (int)datarow["AccountId"];
                campaign.AccountName = datarow["AccountName"].ToString();
                campaign.Failed = (int)datarow["Failed"];
                campaign.Sending = (int)datarow["Sending"];
                CampaignsList.Add(campaign);
            }

            foreach (DataRow datarow in dt1.Rows)
            {
                healthReport = new HealthFormReport();
                healthReport.AccountID = (int)datarow["AccountID"];
                healthReport.AccountName = datarow["AccountName"].ToString();

                healthReport.Data = datarow["Data"].ToString();
                healthReport.Module = datarow["Module"].ToString();
                Forms.Add(healthReport);
            }
            foreach (DataRow datarow in dt2.Rows)
            {
                emails = new HealthEmailsReport();
                emails.Data = datarow["Date"].ToString();
                emails.Module = datarow["Module"].ToString();
                emails.AccountID = (int)datarow["AccountID"];
                emails.AccountName = datarow["AccountName"].ToString();
                EmailsList.Add(emails);
            }
            foreach (DataRow datarow in dt3.Rows)
            {
                campaignSent = new CampaignSent();
                campaignSent.AccountID = (int)datarow["AccountID"];
                campaignSent.AccountName = datarow["AccountName"].ToString();
                campaignSent.Data = datarow["Data"].ToString();
                campaignSent.Module = datarow["Module"].ToString();
                CampaignSentList.Add(campaignSent);
            }
            foreach (DataRow datarow in dt4.Rows)
            {
                failedImport = new FailedImports();
                failedImport.AccountID = (int)datarow["AccountID"];
                failedImport.AccountName = datarow["AccountName"].ToString();
                failedImport.Data = datarow["Data"].ToString();
                failedImport.Module = datarow["Module"].ToString();
                FailedImportsList.Add(failedImport);
            }
            foreach (DataRow datarow in dt5.Rows)
            {
                succeededImports = new SucceededImports();
                succeededImports.AccountID = (int)datarow["AccountID"];
                succeededImports.AccountName = datarow["AccountName"].ToString();
                succeededImports.Data = datarow["Data"].ToString();
                succeededImports.Module = datarow["Module"].ToString();
                SucceededImportsList.Add(succeededImports);
            }
            foreach (DataRow datarow in dt6.Rows)
            {
                inProgressImport = new InProgressImport();
                inProgressImport.AccountID = (int)datarow["AccountID"];
                inProgressImport.AccountName = datarow["AccountName"].ToString();
                inProgressImport.Date = datarow["StartDate"].ToString();
                inProgressImport.Module = datarow["Module"].ToString();
                InProgressImportList.Add(inProgressImport);
            }
            foreach (DataRow datarow in dt7.Rows)
            {
                failedLeadAdapter = new FailedLeadAdapter();
                failedLeadAdapter.AccountID = (int)datarow["AccountID"];
                failedLeadAdapter.AccountName = datarow["AccountName"].ToString();
                failedLeadAdapter.Data = datarow["Data"].ToString();
                failedLeadAdapter.Module = datarow["Module"].ToString();
                FailedLeadAdapterList.Add(failedLeadAdapter);
            }
            foreach (DataRow datarow in dt8.Rows)
            {
                succeededLeadAdapters = new SucceededLeadAdapters();
                succeededLeadAdapters.AccountID = (int)datarow["AccountID"];
                succeededLeadAdapters.AccountName = datarow["AccountName"].ToString();
                succeededLeadAdapters.Data = datarow["Data"].ToString();
                succeededLeadAdapters.Module = datarow["Module"].ToString();
                SucceededLeadAdaptersList.Add(succeededLeadAdapters);
            }
            foreach (DataRow datarow in dt9.Rows)
            {
                contactLeadScore = new ContactLeadScore();
                contactLeadScore.AccountID = (int)datarow["AccountID"];
                contactLeadScore.AccountName = datarow["AccountName"].ToString();
                contactLeadScore.Data = datarow["Data"].ToString();
                contactLeadScore.Module = datarow["Module"].ToString();
                ContactLeadScoreList.Add(contactLeadScore);
            }
            foreach (DataRow datarow in dt10.Rows)
            {
                workFlow = new HealthWorkflowReport();
                workFlow.AccountID = (int)datarow["AccountID"];
                workFlow.ContactsCount = (int?)datarow["ContactsCount"];
                HealthWorkflowReportList.Add(workFlow);
            }
            accountHealthReport.Campaigns = CampaignsList;
            accountHealthReport.Forms = Forms;
            accountHealthReport.Emails = EmailsList;
            accountHealthReport.CampaignSent = CampaignSentList;
            accountHealthReport.FailedImports = FailedImportsList;
            accountHealthReport.SucceededImports = SucceededImportsList;
            accountHealthReport.InProgressImport = InProgressImportList;
            accountHealthReport.FailedLeadAdapter = FailedLeadAdapterList;
            accountHealthReport.SucceededLeadAdapters = SucceededLeadAdaptersList;
            accountHealthReport.ContactLeadScore = ContactLeadScoreList;
            accountHealthReport.HealthWorkflowReport = HealthWorkflowReportList;
            return accountHealthReport;
        }

        public Guid GetAccountEmailProvider(int accountId)
        {
            Guid loginToken = new Guid();
            ServiceProviderViewModel serviceProviderViewModel = serviceProviderService.GetAccountServiceProviders(new GetServiceProviderRequest()
            {
                CommunicationTypeId = CommunicationType.Mail,
                AccountId = accountId,
                MailType = MailType.TransactionalEmail
            }).ServiceProviderViewModel.FirstOrDefault();
            if (serviceProviderViewModel != null)
                loginToken = serviceProviderViewModel.LoginToken;
            else
                throw new UnsupportedOperationException("[|Service providers are not configured for this account to send an email|]");
            return loginToken;
        }

        string GenerateSummaryEmailBody(IList<UserSettingsSummary> userSettings, string domain, string accountName, string userName, string userEmail, string address, string phone, int accountId)
        {
            List<string> emailBody = new List<string>();
            List<string> linkedContacts = new List<string>();
            List<string> linkedOpportunities = new List<string>();
            string body = "";
            string filename = EmailTemplate.DailySummary.ToString() + ".txt";
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
            string imagesUrl = ConfigurationManager.AppSettings["IMAGE_HOSTING_SERVICE_URL"];
            string accountLogo = string.Empty;
            var accountLogoInformation = accountRepository.GetImageStorageName(accountId);
            if (!String.IsNullOrEmpty(accountLogoInformation.StorageName))
                accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, accountLogoInformation.StorageName);
            else
                accountLogo = "";
            string accountImage = string.Empty;
            if (!string.IsNullOrEmpty(accountLogo))
            {
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'> </td>";
            }
            else
            {
                accountImage = "";
            }
            if (userSettings.Count > 0)
            {
                List<UserContactActivitySummary> contactsSummary = userSettings.FirstOrDefault().ContactsSummary;
                List<UserOpportunityActivitySummary> oppSummary = userSettings.FirstOrDefault().OpportunitySummary;
                List<UserContactActivitySummary> ownerChangedSummary = userSettings.FirstOrDefault().OwnerChangedContacts;
                UserActionActivitySummary actionActivitySummary = userSettings.FirstOrDefault().UserContactActionSummary;
                if (contactsSummary.Count > 0)
                {
                    contactsSummary.Each(s =>
                    {
                        if (s.ContactType == ContactType.Person)
                            linkedContacts.Add("<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domain + "/person/" + s.contactId.Value + "'" + "style=" + "color:#0e749f;" + ">" + s.ContactName + "</a> </span>");
                        else
                            linkedContacts.Add("<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domain + "/company/" + s.contactId.Value + "'" + "style=" + "color:#0e749f;" + ">" + s.ContactName + "</a> </span>");
                    });
                    emailBody.Add(GetContactsBody(linkedContacts));
                }
                if (oppSummary.Count > 0)
                {
                    oppSummary.Each(o =>
                    {
                        linkedOpportunities.Add("<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domain + "/viewopportunity/" + o.OpportunityId.Value + "'" + "style=" + "color:#0e749f;" + ">"
                            + o.OpportunityName + "</a> </span>");
                    });
                    emailBody.Add(GetOpportunitiesBody(linkedOpportunities));
                }
                if (ownerChangedSummary.Count > 0)
                {
                    List<string> contAssignBody = new List<string>();
                    var assignedGroup = ownerChangedSummary.GroupBy(gr => gr.LastUpdatedBy).ToList();
                    contAssignBody.Add("<div style='border-bottom:1px solid #e8e8e8;padding:25px 0'>");
                    contAssignBody.Add("<div style='color: #757575; font-size: 18px; font-weight: 300;margin-bottom:15px;'>Contacts assigned to you (<span>"
                        + ownerChangedSummary.Count + "</span>) </div>");
                    assignedGroup.ForEach(f =>
                    {
                        contAssignBody.Add(" <div style='color: #757575; font-size: 13px; font-weight: 600; padding: 10px 0;'> Assigned by " + f.Key + "</div>");
                        f.ToList().Each(s =>
                        {
                            contAssignBody.Add("<div style='color: #757575; font-size: 13px; font-weight: 600;'>");
                            if (s.ContactType == ContactType.Person)
                                contAssignBody.Add("<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domain + "/person/" + s.contactId.Value + "'" + "style=" + "color:#0e749f;" + ">"
                                                 + s.ContactName + "</a> </span>");
                            else
                                contAssignBody.Add("<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domain + "/company/" + s.contactId.Value + "'" + "style=" + "color:#0e749f;" + ">"
                                                 + s.ContactName + "</a> </span>");
                            contAssignBody.Add("</div>");
                        });
                    });
                    contAssignBody.Add("</div>");
                    emailBody.Add(string.Join("", contAssignBody));
                }
                if (actionActivitySummary.ActionDetails.IsAny())
                {
                    string actionsBody = "<div style='border-bottom:1px solid #e8e8e8;padding:25px 0'>" +
                     "<div style='color: #757575; font-size: 18px; font-weight: 300;margin-bottom:15px;'>" +
                         "Actions assigned to you (<span>" + actionActivitySummary.ActionDetails.Count + "</span>)</div>" +
                     "<div style='color: #757575; font-size: 13px; font-weight: 600;'>" +
                         string.Join(", ", actionActivitySummary.ActionDetails) + "</div></div>";
                    emailBody.Add(actionsBody);
                }
                if (actionActivitySummary.ReminderDetails.IsAny())
                {
                    string remindersBody = "<div style='border-bottom:1px solid #e8e8e8;padding:25px 0'>" +
                   "<div style='color: #757575; font-size: 18px; font-weight: 300;margin-bottom:15px;'>" +
                       "Reminder(s) (<span>" + actionActivitySummary.ReminderDetails.Count + "</span>)</div>" +
                   "<div style='color: #757575; font-size: 13px; font-weight: 600;'>" +
                       string.Join(", ", actionActivitySummary.ReminderDetails) + "</div></div>";
                    emailBody.Add(remindersBody);

                }
                if (contactsSummary.Count == 0 && oppSummary.Count == 0 && ownerChangedSummary.Count == 0 && !actionActivitySummary.ActionDetails.IsAny() && !actionActivitySummary.ReminderDetails.IsAny())
                {
                    string noActivities = "<div style='height: 200px;'> <div style='color: #999;line-height:200px;text-align:center;'>No actions are performed for today</div></div>";
                    emailBody.Add(noActivities);
                }
            }

            Logger.Current.Informational("The path for Daily-Summary Email template is :" + savedFileName);
            var mailBody = string.Join("", emailBody);
            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    body = reader.ReadToEnd().Replace("[BODY]", mailBody).Replace("[IMAGES_URL]", imagesUrl).Replace("[ACCOUNTNAME]", accountName).Replace("[USERNAME]", userName).
                        Replace("[USEREMAIL]", userEmail).Replace("[ADDRESS]", address).Replace("[PHONE]", phone).Replace("[STURL]", domain).Replace("[AccountImage]", accountImage);
                } while (!reader.EndOfStream);
            }
            return body;
        }

        string GetContactsBody(List<string> contactsList)
        {
            return "<div style='border-bottom:1px solid #e8e8e8;padding:25px 0'>" +
                    "<div style='color: #757575; font-size: 18px; font-weight: 300;margin-bottom:15px;'>" +
                        "Contacts created by you (<span>" + contactsList.Count + "</span>)</div>" +
                    "<div style='color: #757575; font-size: 13px; font-weight: 600;'>" +
                        string.Join(", ", contactsList) + "</div></div>";
        }

        string GetOpportunitiesBody(List<string> oppList)
        {
            return "<div style='border-bottom:1px solid #e8e8e8;padding:25px 0'>" +
                    "<div style='color: #757575; font-size: 18px; font-weight: 300;margin-bottom:15px;'>" +
                        "Opportunities created by you (<span>" + oppList.Count + "</span>)</div>" +
                    "<div style='color: #757575; font-size: 13px; font-weight: 600;'>" +
                        string.Join(", ", oppList) + "</div></div>";
        }

        public CheckDomainURLAvailabilityResponse IsDomainURLExist(CheckDomainURLAvailabilityRequest request)
        {
            CheckDomainURLAvailabilityResponse response = new CheckDomainURLAvailabilityResponse();
            if (request.DomainURL != null || request.DomainURL != "")
            {
                if (IsURLValid(request.DomainURL))
                {
                    var domainurl = request.DomainURL + "" + System.Configuration.ConfigurationManager.AppSettings["SMARTCRMDOMAIN"];
                    response.Available = accountRepository.CheckDomainAvailability(domainurl);
                }
                else
                    response.Message = "[|special characters are not allowed in domain name except '-' but not as a starting or ending character|]";
            }
            else
                response.Available = false;
            return response;
        }

        bool IsURLValid(string domainName)
        {
            string pattern = @"^[a-zA-Z0-9]+\-*[a-zA-Z0-9]+$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            if (regex.IsMatch(domainName))
                return true;
            else
                return false;
        }

        public GetImportForAccountResponse GetImportDataByAccountID(GetImportForAccountRequest request)
        {
            GetImportForAccountResponse response = new GetImportForAccountResponse();
            LeadAdapterAndAccountMap lead = importRepository.GetImportForAccountID(request.AccountId);
            response.Import = Mapper.Map<LeadAdapterAndAccountMap, LeadAdapterViewModel>(lead);
            return response;
        }

        public GetWebAnalyticsProvidersResponse GetWebAnalyticsProviders(GetWebAnalyticsProvidersRequest request)
        {
            GetWebAnalyticsProvidersResponse response = new GetWebAnalyticsProvidersResponse();

            var webAnalyticsProviderKeys = accountRepository.GetWebAnalyticsProviders();
            response.WebAnalyticsProviders = webAnalyticsProviderKeys;
            return response;
        }

        public GetWebAnalyticsProvidersResponse GetAccountWebAnalyticsProviders(GetWebAnalyticsProvidersRequest request)
        {
            GetWebAnalyticsProvidersResponse response = new GetWebAnalyticsProvidersResponse();

            var webAnalyticsProviderKeys = accountRepository.GetAccountWebAnalyticsProviders(request.AccountId);
            response.WebAnalyticsProviders = webAnalyticsProviderKeys;
            return response;
        }

        public GetAccountDomainUrlResponse GetAccountDomainUrl(GetAccountDomainUrlRequest request)
        {
            GetAccountDomainUrlResponse response = new GetAccountDomainUrlResponse();
            response.DomainUrl = accountRepository.GetAccountDomainUrl(request.AccountId);
            return response;
        }

        public GetAccountIdByContactIdResponse GetAccountIdByContactId(GetAccountIdByContactIdRequest request)
        {
            return new GetAccountIdByContactIdResponse() { AccountId = accountRepository.GetAccountIdByContactId(request.ContactId) };
        }

        public GetAccountDropboxKeyResponse GetAccountDropboxKey(GetAccountIdRequest request)
        {
            GetAccountDropboxKeyResponse response = new GetAccountDropboxKeyResponse();
            var dropboxKey = accountRepository.GetDropboxKey(request.accountId);
            response.DropboxKey = dropboxKey;
            return response;
        }

        public GetServiceProviderSenderEmailResponse GetDefaultBulkEmailProvider(GetServiceProviderSenderEmailRequest request)
        {
            GetServiceProviderSenderEmailResponse response = new GetServiceProviderSenderEmailResponse();
            response.SenderEmail = accountRepository.GetServiceProviderEmail(request.ServiceProviderID);
            return response;
        }

        public GetAccountImageStorageNameResponse GetStorageName(GetAccountImageStorageNameRequest request)
        {
            GetAccountImageStorageNameResponse response = new GetAccountImageStorageNameResponse();
            response.AccountLogoInfo = accountRepository.GetImageStorageName(request.AccountId);
            return response;
        }

        public GetAccountListResponse GetAllAccounts()
        {
            GetAccountListResponse response = new GetAccountListResponse();
            IEnumerable<Account> accounts = accountRepository.GetAllAccounts();
            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                IEnumerable<AccountListViewModel> list = Mapper.Map<IEnumerable<Account>, IEnumerable<AccountListViewModel>>(accounts);
                response.Accounts = list;
            }
            return response;
        }

        public GetAccountSubscriptionDataResponse GetSubscriptionDataByAccountID(GetAccountSubscriptionDataRequest request)
        {
            GetAccountSubscriptionDataResponse response = new GetAccountSubscriptionDataResponse();
            response.AccountSubscriptionData = accountRepository.GetSubscriptionDataByAccountID(request.AccountId);
            return response;
        }

        public GetAllAccountsBySubscriptionResponse GetAllAccountsBySubscription(GetAllAccountsBySubscriptionRequest request)
        {
            GetAllAccountsBySubscriptionResponse response = new GetAllAccountsBySubscriptionResponse();
            IEnumerable<Account> accounts = accountRepository.GetAccountsBySubscription(request.ID);
            if (accounts == null)
            {
                response.Exception = GetAccountNotFoundException();
            }
            else
            {
                IEnumerable<AccountListViewModel> list = Mapper.Map<IEnumerable<Account>, IEnumerable<AccountListViewModel>>(accounts);
                response.Accounts = list;
            }
            return response;
        }

        public GetAccountPrimaryEmailResponse GetAccountPrimaryEmail(GetAccountPrimaryEmailRequest request)
        {
            GetAccountPrimaryEmailResponse response = new GetAccountPrimaryEmailResponse();
            response.PrimaryEmail = accountRepository.GetAccountPrimaryEmail(request.AccountId);
            return response;
        }

        public Dictionary<Guid, string> GetTransactionalProviderDetails(int accountId)
        {
            return serviceproviderRepository.GetTransactionalProviderDetails(accountId);
        }

        public GetIndexingDataResponce GetIndexingData(GetIndexingDataRequest request)
        {
            GetIndexingDataResponce responce = new GetIndexingDataResponce();
            responce.IndexingData = accountRepository.GetIndexingData(request.ChunkSize);
            return responce;
        }

        public void DeleteIndexedData(IList<int> entityIds)
        {
            accountRepository.DeleteIndexedData(entityIds);
        }

        public UpdateIndexingStatusResponse UpdateIndexingStatus(UpdateIndexingStatusRequest request)
        {
            accountRepository.UpdateIndexStatusToFail(request.ReferenceIds, (int)request.Status);
            return new UpdateIndexingStatusResponse();
        }
        public InsertIndexingDataResponce InsertIndexingData(InsertIndexingDataRequest request)
        {
            InsertIndexingDataResponce responce = new InsertIndexingDataResponce();
            accountRepository.InsertIndexingData(request.IndexingData);
            return responce;
        }

        public GetTermsAndConditionsResponse GetTermsAndConditions(GetTermsAndConditionsRequest request)
        {
            GetTermsAndConditionsResponse response = new GetTermsAndConditionsResponse();
            if (request != null && request.AccountId != 0)
                response.TC = accountRepository.GetTC(request.AccountId);
            return response;
        }

        public ShowTCResponse ShowTC(ShowTCRequest request)
        {
            ShowTCResponse response = new ShowTCResponse();
            if (request.AccountId != 0)
                response.ShowTC = accountRepository.ShowTC(request.AccountId);
            return response;
        }

        public UpdateTCAcceptanceResponse UpdateTCAcceptance(UpdateTCAcceptanceRequest request)
        {
            if (request != null && request.RequestedBy.HasValue)
                userService.UpdateTCAcceptance(new UpdateTCAcceptanceRequest() { RequestedBy = request.RequestedBy.Value, AccountId = request.AccountId });
            return new UpdateTCAcceptanceResponse();
        }

        public GetFirstLoginUserSettingsResponse GetFirstLoginUserSettings(GetFirstLoginUserSettingsRequest request)
        {
            GetFirstLoginUserSettingsResponse response = new GetFirstLoginUserSettingsResponse();
            if (request != null && request.RequestedBy.HasValue)
                response.UserSettings = userService.GetFirstLoginUserSettings(new GetFirstLoginUserSettingsRequest() { RequestedBy = request.RequestedBy.Value }).UserSettings;
            return response;
        }

        public string GettingLitmusTestAPIKey(int accountId)
        {
            return accountRepository.GetLitmusTestAPIKey(accountId);
        }

        public GetGoogleDriveAPIKeyResponse GetGoogleDriveAPIKey(GetGoogleDriveAPIKeyRequest request)
        {
            GetGoogleDriveAPIKeyResponse response = new GetGoogleDriveAPIKeyResponse();
            var accountData = accountRepository.GetGoogleDriveAPIKey(request.AccountId);
            response.APIKey = accountData.GoogleDriveAPIKey;
            response.ClientID = accountData.GoogleDriveClientID;
            return response;
        }

        #endregion

        #region Imports and leadadapters code

        public GetImportdataResponse GetImportData(GetImportdataRequest request)
        {
            try
            {
                Logger.Current.Verbose("Request to get imported data from the file (xls or csv) that is uploaded by the user");
                var data = new DataSet();
                GetImportdataResponse getImportdataResponse = new GetImportdataResponse();
                ImportDataListViewModel importDataListViewModel = new ImportDataListViewModel();
                var destinationPath = Path.Combine(ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(), request.AccountID.ToString(), request.filename);
                var fileInfo = new FileInfo(destinationPath);
                if (fileInfo.Extension == ".csv")
                {
                    ReadCSV readCSV = new ReadCSV();
                    data = readCSV.GetDataSetFromCSVfileUsingCSVHelper(destinationPath);
                }
                else
                {
                    ReadExcel readExcel = new ReadExcel();
                    data = readExcel.ToDataSet(destinationPath, true);
                }

                var columns = data.Tables[0].Columns;
                var rows = data.Tables[0].Rows;
                if (rows.Count > 100000)
                {
                    Deletefile(destinationPath);
                    throw new UnsupportedOperationException("[|Failed to schedule the import - File exceeds acceptable data range. Rows should be less than 100000|]");
                }
                else if (columns.Count > 100)
                {
                    Deletefile(destinationPath);
                    throw new UnsupportedOperationException("[|Failed to schedule the import - File exceeds acceptable data range. Columns should be less than 100|]");
                }
                else if (columns.Count == 0 || rows.Count == 0)
                {
                    Deletefile(destinationPath);
                    throw new UnsupportedOperationException("[|Failed to schedule the import - File does not contains valid data|]");
                }


                int EmptyRowCount = GetStartingEmptyRowsCount(data.Tables[0]);

                if (EmptyRowCount == 1)
                {
                    Deletefile(destinationPath);
                    throw new UnsupportedOperationException("[|Failed to schedule the import - The first row is empty|]");
                }
                else if (EmptyRowCount > 1)
                {
                    Deletefile(destinationPath);
                    throw new UnsupportedOperationException("[|Failed to schedule the import - The first|] " + EmptyRowCount + " [|rows are empty|]");
                }


                if (data != null)
                {
                    List<Field> customFields = customFieldsRepository.GetAllCustomFieldsForImports(request.AccountID).ToList();
                    IEnumerable<DropdownViewModel> dropdownFields = cacheService.GetDropdownValues(request.AccountID);
                    IEnumerable<DropdownValueViewModel> phonefields = dropdownFields.Where(i => i.DropdownID == 1).Select(i => i.DropdownValuesList).FirstOrDefault();

                    foreach (DropdownValueViewModel drpvm in phonefields)
                    {
                        Field phfield = new Field();
                        phfield.AccountID = request.AccountID;
                        phfield.DisplayName = drpvm.DropdownValue;
                        phfield.Title = drpvm.DropdownValue;
                        phfield.Id = drpvm.DropdownValueID;
                        phfield.IsCustomField = false;
                        phfield.IsDropdownField = true;
                        customFields.Add(phfield);
                    }
                    importDataListViewModel.Fields = customFields;


                    List<ImportDataViewModel> lstImportdata = new List<ImportDataViewModel>();
                    for (int i = 0; i < columns.Count; i++)
                    {
                        ImportDataViewModel importDataViewModel = new ImportDataViewModel();
                        importDataViewModel.SheetColumnName = columns[i].ToString();
                        importDataViewModel.ContactFieldName = string.Empty;
                        importDataViewModel.PreviewData = rows[0].ItemArray[i].ToString();
                        lstImportdata.Add(importDataViewModel);
                    }
                    importDataListViewModel.Imports = lstImportdata;
                    importDataListViewModel.FileName = request.filename;
                    getImportdataResponse.ImportDataListViewModel = importDataListViewModel;
                }
                return getImportdataResponse;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                return new GetImportdataResponse() { Exception = ex };
            }
        }

        public GetImportedContactsResponse GetImportedContacts(GetImportedContactsRequest request)
        {
            GetImportedContactsResponse response = new GetImportedContactsResponse();
            response.ContactIds = importRepository.GetImportedContacts(request.LastModifiedOn);
            response.TagIds = importRepository.GetImportTags(request.LastModifiedOn);
            return response;
        }

        private static int GetStartingEmptyRowsCount(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            int emptyrowcount = default(int);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                bool allNull = true;
                for (int j = 0; j < columnCount; j++)
                {
                    if (dt.Rows[i][j] != DBNull.Value && !string.IsNullOrWhiteSpace((dt.Rows[i][j]).ToString()))
                    {
                        allNull = false;
                        return emptyrowcount;
                    }

                }
                if (allNull)
                {
                    emptyrowcount++;
                }
            }
            return emptyrowcount;
        }

        private void Deletefile(string filepath)
        {
            if (System.IO.File.Exists(filepath))
                System.IO.File.Delete(filepath);
        }

        public ImportDataResponse ImportData(ImportDataRequest request)
        {
            Logger.Current.Verbose("Request to import data from colums that are binded");
            var pathToSaveFile = Path.Combine(ConfigurationManager.AppSettings["LEADADAPTER_PHYSICAL_PATH"].ToString(), request.ImportDataListViewModel.AccountID.ToString());
            Logger.Current.Informational("Path to save file" + pathToSaveFile);
            var destinationPath = Path.Combine(pathToSaveFile, request.ImportDataListViewModel.FileName);
            Logger.Current.Informational("Destinationation path" + destinationPath);
            ImportDataResponse importdataResponse = new ImportDataResponse();

            LeadAdapterAndAccountMap leadAdapterAndAccountMap = leadAdaptersRepository.GetLeadAdapter(request.ImportDataListViewModel.AccountID, LeadAdapterTypes.Import);
            Logger.Current.Informational("Leadadapter file path path" + leadAdapterAndAccountMap.LocalFilePath);
            var importcolumns = request.ImportDataListViewModel.Imports.Where(i => !string.IsNullOrEmpty(i.ContactFieldName)).ToList();

            if (leadAdapterAndAccountMap == null)
                throw new Exception("[|Import is missing some major information. Contact Administrator|]");
            else
            {
                var jobLog = new LeadAdapterJobLogs();
                int AccountID = request.ImportDataListViewModel.AccountID;
                bool UpdateOnDuplicate = request.ImportDataListViewModel.UpdateOnDuplicate;
                byte duplicateLogic = request.ImportDataListViewModel.DuplicateLogic;

                jobLog.LeadAdapterAndAccountMapID = leadAdapterAndAccountMap.Id;
                jobLog.LeadAdapterJobStatusID = LeadAdapterJobStatus.ReadyToProcess;
                jobLog.Remarks = string.Empty;
                jobLog.FileName = request.ImportDataListViewModel.FileName;
                jobLog.CreatedBy = request.RequestedBy.Value;
                jobLog.CreatedDateTime = DateTime.UtcNow;

                Guid uniqueidentifier = Guid.NewGuid();

                int jobid = default(int);
                try
                {
                    List<LeadAdapterJobLogDetails> details = new List<LeadAdapterJobLogDetails>();
                    jobLog.LeadAdapterJobLogDetails = details;

                    jobid = importRepository.InsertLeadAdapterjob(jobLog, uniqueidentifier, UpdateOnDuplicate, true,
                                                      AccountID, request.ImportDataListViewModel.UserId, duplicateLogic, (int)request.RequestedBy, request.ImportDataListViewModel.IncludeInReports);
                    var fileExtension = Path.GetExtension(destinationPath);
                    Logger.Current.Informational("final Destination " + Path.Combine(leadAdapterAndAccountMap.LocalFilePath, uniqueidentifier + fileExtension));
                    Logger.Current.Informational("Destination path 1" + destinationPath);
                    File.Move(destinationPath, Path.Combine(leadAdapterAndAccountMap.LocalFilePath, uniqueidentifier + fileExtension));
                    File.Delete(destinationPath);
                    if (importcolumns != null && importcolumns.Any())
                    {
                        IEnumerable<ImportColumnMappings> mappings = Mapper.Map<IEnumerable<ImportDataViewModel>, IEnumerable<ImportColumnMappings>>(importcolumns);
                        mappings = mappings.ToList().Select(s => { s.JobID = jobid; return s; }).ToList();
                        importRepository.InsertColumnMappings(mappings);
                    }
                    if (request.ImportDataListViewModel.NeverBounceValidation)
                    {
                        importRepository.InsertNeverBounceRequest(AccountID, request.ImportDataListViewModel.UserId, (byte)NeverBounceEntityTypes.Imports, new List<int>() { jobid }, 0);
                        SendEmailForNeverBounce(AccountID, request.ImportDataListViewModel.UserId, "File Name", request.ImportDataListViewModel.FileName);
                    }
                }
                catch
                {
                    File.Delete(destinationPath);
                    throw;
                }

                IEnumerable<Tag> Tags = Mapper.Map<IEnumerable<TagViewModel>, IEnumerable<Tag>>(request.ImportDataListViewModel.TagsList);
                foreach (Tag tag in Tags)
                    if (tag.Id == 0)
                        tag.CreatedBy = request.RequestedBy;
                importRepository.InsertImportTags(jobid, Tags);

                foreach (Tag tag in Tags.Where(t => t.Id == 0))
                {
                    Tag savedTag = tagRepository.FindBy(tag.TagName, AccountID);
                    indexingService.IndexTag(savedTag);
                    accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                }
                return importdataResponse;
            }
        }

        public InsertNeverBounceResponse InsertNeverBounceRequest(InsertNeverBounceRequest request)
        {
            InsertNeverBounceResponse response = new InsertNeverBounceResponse();
            if (request.SearchdefinitionIds != null)
            {
                importRepository.InsertNeverBounceRequest(request.AccountId, request.RequestedBy.Value, (byte)NeverBounceEntityTypes.SavedSearches, request.SearchdefinitionIds.ToList(), request.TotalCount);
                IEnumerable<string> searchdefinitions = advancedSearchRepository.GetSearchDefinitionNamesByIds(request.SearchdefinitionIds.ToList());
                SendEmailForNeverBounce(request.AccountId, request.RequestedBy.Value, "Search Definition Name(s)", string.Join(",", searchdefinitions.ToArray()));
            }
            else if (request.TagIds != null)
            {
                importRepository.InsertNeverBounceRequest(request.AccountId, request.RequestedBy.Value, (byte)NeverBounceEntityTypes.Tags, request.TagIds.ToList(), request.TotalCount);
                IEnumerable<string> tagNames = tagRepository.GetTagNamesByIds(request.TagIds.ToList());
                SendEmailForNeverBounce(request.AccountId, request.RequestedBy.Value, "Tag Name(s)", string.Join(",", tagNames.ToArray()));
            }
            return response;


        }

        public GetImportsResponse GetAllImports(GetImportsRequest request)
        {
            GetImportsResponse response = new GetImportsResponse();
            var leadaccountmapId = importRepository.GetLeadAdapterAccountMap(request.AccountId);
            IEnumerable<ImportData> imports = importRepository.FindAll(request.Query, request.Limit, request.PageNumber, leadaccountmapId);
            if (imports == null)
            {
                response.Exception = new UnsupportedOperationException("[|Imported records not found|]");
            }
            else
            {
                IEnumerable<ImportListViewModel> list = Mapper.Map<IEnumerable<ImportData>, IEnumerable<ImportListViewModel>>(imports);
                response.Imports = list;
                response.TotalHits = importRepository.FindAll(request.Query, leadaccountmapId).Count();
            }
            return response;
        }

        private bool IsValidPhoneNumberLength(string phoneNumber)
        {
            string pattern = @"^[01]?[- .]?(\([2-9]\d{2}\)|[2-9]\d{2})[- .]?\d{3}[- .]?\d{4}$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(phoneNumber);
        }

        public string GetAccountPrivacyPolicy(int accountId)
        {
            string privacypolicy = accountRepository.GetAccountPrivacyPolicy(accountId);
            if (privacypolicy == null || privacypolicy == "" || privacypolicy == " ")
                privacypolicy = accountRepository.GetAccountPrivacyPolicy(1);
            return privacypolicy;
        }

        private bool IsValidURL(string Url)
        {
            string pattern = @"^^((((ht|f)tp(s?))\:\/\/)|(www.))?[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$";
            bool result = Regex.IsMatch(Url.ToLower(), pattern);
            return result;
        }

        public void ScheduleAnalyticsRefresh(int entityId, byte entityType)
        {
            accountRepository.ScheduleAnalyticsRefresh(entityId, entityType);
        }

        /// <summary>
        /// Getting Account has Disclaimer
        /// </summary>
        /// <param name="accountId"></param>
        /// <returns></returns>
        public bool? AccountHasDisclaimer(int accountId)
        {
            return accountRepository.GetAccountDisclaimer(accountId);
        }

        public GetNeverBounceResponse GetNeverBounceRequests(GetNeverBounceRequest request)
        {
            GetNeverBounceResponse response = new GetNeverBounceResponse();
            response.Queue = importRepository.GetNeverBounceRequests(request.AccountId, request.PageNumber, request.Limit);
            return response;
        }

        public GetNeverBounceAcceptedResponse GetAcceptedRequests(GetNeverBounceAcceptedRequests request)
        {
            GetNeverBounceAcceptedResponse response = new GetNeverBounceAcceptedResponse();
            response.Requests = importRepository.GetNeverBounceAcceptedRequests(request.Status);
            return response;
        }

        //public ID.GetContactEmailsResponse GetContactEmails(ID.GetContactEmailsRequest request)
        //{
        //    ID.GetContactEmailsResponse response = new ID.GetContactEmailsResponse();
        //    if (request.EntityType == NeverBounceEntityTypes.Imports || request.EntityType == NeverBounceEntityTypes.Tags)
        //        response.Contacts = importRepository.GetContactEmails(request.EntityType, request.EntityIds);
        //    else if (request.EntityType == NeverBounceEntityTypes.SavedSearches)
        //    {
        //        IEnumerable<ReportContact> reportContacts = new List<ReportContact>();
        //        if (!string.IsNullOrEmpty(request.EntityIds))
        //        {
        //            List<string> Ids = request.EntityIds.Split(',').ToList();
        //            Ids.ForEach(f => {
        //              var task =  Task.Run(() => advancedSearchService.GetContactEmails(new S.GetContactEmailsRequest() { AccountId = request.AccountId, SearchDefinitionID = int.Parse(f) }));
        //              IEnumerable<Contact> contacts = task.Result;
        //            });
        //        }
        //        response.Contacts = reportContacts;
        //    }
        //    else
        //        response.Contacts = null;
        //    return response;
        //}

        public UpdateNeverBounceResponse UpdateNeverBounceRequest(UpdateNeverBounceRequest request)
        {
            UpdateNeverBounceResponse response = new UpdateNeverBounceResponse();
            importRepository.UpdateNeverBounceRequest(request.Request);
            return response;
        }

        public UpdateNeverBouncePollingResponse UpdateNeverBouncePollingRequest(UpdateNeverBouncePollingRequest request)
        {
            UpdateNeverBouncePollingResponse response = new UpdateNeverBouncePollingResponse();
            importRepository.UpdateNeverBouncePollingResponse(request.Request);
            return response;
        }

        public UpdateNeverBounceResponse UpdateScrubQueueRequests(UpdateNeverBounceRequest request)
        {
            UpdateNeverBounceResponse response = new UpdateNeverBounceResponse();
            importRepository.UpdateRequest(request.Request.NeverBounceRequestID, request.RequestedBy.Value, request.Request.ServiceStatus);
            return response;
        }

        public UpdateEmailStatusResponse InsertEmailStatuses(UpdateEmailStatusRequest request)
        {
            UpdateEmailStatusResponse response = new UpdateEmailStatusResponse();
            importRepository.InsertNeverBounceResults(request.Results);
            return response;
        }

        public NeverBounceEmailDataResponse GetEmailData(NeverBounceEmailDataRequest request)
        {
            NeverBounceEmailDataResponse response = new NeverBounceEmailDataResponse();
            response.Data = importRepository.GetEmailData(request.NeverBounceRequestID);
            return response;
        }

        private void SendEmailForNeverBounce(int accountId, int userId, string fileName, string fileData)
        {
            Guid loginToken = new Guid();
            string accountPrimaryEmail = string.Empty;
            var primaryEmail = userRepository.GetUserPrimaryEmail(userId);
            string userName = userRepository.GetUserName(userId);
            Email senderEmail = new Email();
            Account account = accountRepository.GetAccountMinDetails(accountId);
            string toEmail = ConfigurationManager.AppSettings["NeverBounce_Update_Email"];
            if (account != null)
            {
                if (account.Email != null)
                    accountPrimaryEmail = account.Email.EmailId;

                IEnumerable<ServiceProvider> serviceProviders = serviceproviderRepository.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                if (serviceProviders != null && serviceProviders.FirstOrDefault() != null)
                {
                    loginToken = serviceProviders.FirstOrDefault().LoginToken;
                    senderEmail = serviceproviderRepository.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                }

                var tomails = toEmail.Split(',');
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                foreach (string ToMail in tomails)
                {

                    if (loginToken != new Guid())
                    {
                        string fromEmail = (senderEmail != null && !string.IsNullOrEmpty(senderEmail.EmailId)) ? senderEmail.EmailId : accountPrimaryEmail;
                        EmailAgent agent = new EmailAgent();
                        mailRequest.Body = GetBody(accountId, primaryEmail, userName, fileName, fileData);
                        mailRequest.From = fromEmail;
                        mailRequest.IsBodyHtml = true;
                        mailRequest.Subject = string.Format("{0} - Request for List Validation", account.AccountName);
                        mailRequest.To = new List<string>() { ToMail };
                        mailRequest.TokenGuid = loginToken;
                        mailRequest.RequestGuid = Guid.NewGuid();
                        mailRequest.ScheduledTime = DateTime.Now.ToUniversalTime().AddSeconds(5);
                        mailRequest.AccountDomain = account.DomainURL;
                        mailRequest.CategoryID = (byte)EmailNotificationsCategory.MailTesterEmail;
                        mailRequest.AccountID = accountId;
                        agent.SendEmail(mailRequest);
                    }

                }


            }
        #endregion
        }

        private string GetBody(int accountId, string email, string name, string fileName, string fileData)
        {
            string body = string.Empty;
            string accountLogo = string.Empty;
            string accountName = string.Empty;
            string accountImage = string.Empty;
            string filename = EmailTemplate.NeverBounceEmailValidationTemplate.ToString() + ".txt";
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
            AddressViewModel addressVM = new AddressViewModel();
            Address address = accountRepository.GetAddress(accountId);
            if (address != null)
                addressVM = Mapper.Map<Address, AddressViewModel>(address);
            string accountAddress = addressVM.ToString();
            string accountPhoneNumber = accountRepository.GetPrimaryPhone(accountId);
            AccountLogoInfo imgStorage = accountRepository.GetImageStorageName(accountId);


            if (imgStorage != null)
            {
                if (!String.IsNullOrEmpty(imgStorage.StorageName))
                    accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, imgStorage.StorageName);
                else
                    accountLogo = "";
                accountName = imgStorage.AccountName;
            }

            if (!string.IsNullOrEmpty(accountLogo))
            {
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width ='100' ></td>";
            }
            else
            {
                accountImage = "";
            }

            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    body = reader.ReadToEnd().Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage).Replace("[UserName]", name)
                            .Replace("[UserEmail]", email).Replace("[FileName]", fileName).Replace("[FileData]", fileData)
                            .Replace("[ADDRESS]", accountAddress).Replace("[PHONE]", accountPhoneNumber);
                } while (!reader.EndOfStream);
            }

            return body;
        }

        public string GetNeverBouceValidationDoneFileName(int neverBouceRequestId)
        {
            return accountRepository.GetNeverBouceValidationDoneFileName(neverBouceRequestId);
        }

        public void BulkInsertForDeletedContactsInRefreshAnalytics(int[] contactIds)
        {
            accountRepository.BulkInsertForDeletedContactsInRefreshAnalytics(contactIds);
        }

    }
}
