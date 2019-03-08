using SmartTouch.CRM.ApplicationServices.ObjectMappers;
using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Excel;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Exceptions;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.CustomFields;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.Image;
using SmartTouch.CRM.ApplicationServices.Messaging.MailGun;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.Messaging.WebAnalytics;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.Images;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Communications;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Domain.Fields;
using SmartTouch.CRM.Domain.Search;
using System.IO;
using System.Configuration;
using SmartTouch.CRM.Domain.Communication;
using RestSharp;
using SmartTouch.CRM.Repository.Database;
using System.Net;
using System.Drawing.Imaging;
using System.Security.Claims;
using System.Threading;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ContactService : IContactService
    {
        readonly IContactRepository contactRepository;
        readonly IImageService imageService;
        readonly ITagRepository tagRepository;
        readonly IUnitOfWork unitOfWork;
        ISearchService<Contact> searchService;
        IIndexingService indexingService;
        readonly IUrlService urlService;
        readonly ICustomFieldService customFieldService;
        readonly ICachingService cachingService;
        readonly IDropdownRepository dropdownRepository;
        readonly IAccountService accountService;
        readonly IUserRepository userRepository;
        readonly IFormRepository formRepository;
        IMailGunService mailGunService;
        readonly IMessageService messageService;
        readonly IAdvancedSearchRepository advancedSearchRepository;
        readonly ICommunicationService communicationService;
        readonly IUserService userService;
        readonly IServiceProviderRepository serviceProvider;

        readonly IFormSubmissionRepository formSubmissionRepository; //NEXG-3014
        readonly IFindSpamService findSpamService; //NEXG-3014

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactService"/> class.
        /// </summary>
        /// <param name="contactRepository">The contact repository.</param>
        /// <param name="tagRepository">The tag repository.</param>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="urlService">The URL service.</param>
        /// <param name="customFieldService">The custom field service.</param>
        /// <param name="cachingService">The caching service.</param>
        /// <param name="dropdownRepository">The dropdown repository.</param>
        /// <param name="queuingService">The queuing service.</param>
        /// <param name="indexingService">The indexing service.</param>
        /// <param name="searchService">The search service.</param>
        /// <param name="accountService">The account service.</param>
        /// <param name="userRepository">The user repository.</param>
        /// <param name="imageService">The image service.</param>
        /// <param name="formRepository">The form repository.</param>
        /// <param name="mailGunService">The mail gun service.</param>
        /// <exception cref="System.ArgumentNullException">
        /// contactRepository
        /// or
        /// unitOfWork
        /// </exception>
        public ContactService(IContactRepository contactRepository, ITagRepository tagRepository, IUnitOfWork unitOfWork,
            IUrlService urlService, ICustomFieldService customFieldService, ICachingService cachingService, IDropdownRepository dropdownRepository,
            IIndexingService indexingService, ISearchService<Contact> searchService, IAccountService accountService,
            IUserRepository userRepository, IMessageService messageService, IImageService imageService, IFormRepository formRepository, IMailGunService mailGunService,
            IAdvancedSearchRepository advancedSearchRepository, ICommunicationService communicationService, IUserService userService, IServiceProviderRepository serviceProvider
            ,IFormSubmissionRepository formSubmissionRepository //NEXG-3014
            , IFindSpamService findSpamService  //NEXG-3014
            )
        {
            if (contactRepository == null) throw new ArgumentNullException("contactRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.contactRepository = contactRepository;
            this.tagRepository = tagRepository;
            this.unitOfWork = unitOfWork;
            this.searchService = searchService;
            this.indexingService = indexingService;
            this.urlService = urlService;
            this.customFieldService = customFieldService;
            this.cachingService = cachingService;
            this.dropdownRepository = dropdownRepository;
            this.accountService = accountService;
            this.userRepository = userRepository;
            this.imageService = imageService;
            this.formRepository = formRepository;
            this.mailGunService = mailGunService;
            this.messageService = messageService;
            this.advancedSearchRepository = advancedSearchRepository;
            this.communicationService = communicationService;
            this.userService = userService;
            this.serviceProvider = serviceProvider;
            this.formSubmissionRepository = formSubmissionRepository; //NEXG-3014
            this.findSpamService = findSpamService; //NEXG-3014
        }

        public GetUsersResponse GetUsers(GetUsersRequest request)
        {
            GetUsersResponse response = new GetUsersResponse();

            response.Owner = contactRepository.GetUsers(request.AccountID, request.UserId, request.IsSTadmin);
            if (response.Owner == null) throw new UnsupportedOperationException("[|The requested users list was not found.|]");

            return response;
        }

        public ChangeOwnerResponce ChangeOwner(ChangeOwnerRequest request)
        {
            Logger.Current.Verbose("Request for change the owner");

            IEnumerable<RawContact> contactList = Mapper.Map<IEnumerable<ContactEntry>, IEnumerable<RawContact>>(request.ChangeOwnerViewModel.Contacts);
            Logger.Current.Informational("Account Admin :" + request.ChangeOwnerViewModel.OwnerName);
            Logger.Current.Informational("Contacts :" + request.ChangeOwnerViewModel.Contacts.Count());

            List<int> contactIds = contactList.Select(p => p.ContactID).ToList();
            if (request.ChangeOwnerViewModel.SelectAll == false)
            {
                contactRepository.ChangeOwner(request.ChangeOwnerViewModel.OwnerId, contactIds, request.AccountId);
                UpdateOwnerBulkData((int)request.ChangeOwnerViewModel.OwnerId, (int)request.RequestedBy, request.ModuleId, contactIds);
            }

            return new ChangeOwnerResponce();
        }

        public User UpdateOwnerBulkData(int ownerId, int userId, int accountId, IEnumerable<int> contactIds)
        {
            List<int> ownerIds = new List<int>();
            ownerIds.Add(ownerId);
            //IEnumerable<Contact> Contacts = contactRepository.FindAll(contactIds.ToList());
            //IEnumerable<ContactCreatorInfo> creatorInfos = contactRepository.GetContactCreatorsInfo(contactIds);
            var owner = userRepository.FindBy(ownerId);
            if (userId != ownerId)
                addNewOwnerNotification(contactIds.ToList(), ownerId, (byte)AppModules.Contacts);

            //this.ContactIndexing(new ContactIndexingRequest() { ContactIds = contactIds.ToList() });
            accountService.InsertIndexingData(new InsertIndexingDataRequest()
            {
                IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = contactIds.ToList(), IndexType = (int)IndexType.Contacts }
            });
            //foreach (var contact in Contacts)
            //{
            //    var creatorInfo = creatorInfos.SingleOrDefault(c => c.ContactId == contact.Id);
            //    if (creatorInfo != null && creatorInfo.CreatedOn.HasValue)
            //        contact.CreatedOn = creatorInfo.CreatedOn.Value;
            //    if (creatorInfo != null)
            //        contact.CreatedBy = creatorInfo.CreatedBy;
            //    indexingService.IndexContact(contact);
            //    var matchedQueries = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { AccountId = accountId, Contact = contact });
            //    Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueries.MatchedQueries);
            //}
            return owner;
        }

        public Task UpdateBulkExcelExport(BulkOperations operations, IEnumerable<Contact> contacts, IEnumerable<FieldViewModel> searchFields)
        {
            ExportPersonViewModel viewModel = new ExportPersonViewModel();
            viewModel.selectedFields = operations.ExportSelectedFields.Split(',').Select(Int32.Parse).ToArray();
            viewModel.SearchFields = searchFields;
            viewModel.Contacts = contacts.ToList();
            viewModel.DownLoadAs = operations.ExportType == 1 ? "CSV" : (operations.ExportType == 2 ? "Excel" : (operations.ExportType == 3 ? "PDF" : ""));
            var task = Task.Run(() => this.GetAllContactsByIds(
               new ExportPersonsRequest()
               {
                   DownLoadAs = operations.ExportType == 1 ? "CSV" : (operations.ExportType == 2 ? "Excel" : (operations.ExportType == 3 ? "PDF" : "")),
                   DateFormat = operations.DateFormat,
                   AccountId = operations.AccountID,
                   TimeZone = operations.TimeZone,
                   ExportViewModel = viewModel
               }));
            ExportPersonsResponse exportResponse = task.Result;
            string fileKey = Guid.NewGuid().ToString() + exportResponse.FileName;
            bool result = cachingService.StoreTemporaryFile(fileKey, exportResponse.byteArray);
            Logger.Current.Informational("Is campaign filekey stored : " + result);
            contactRepository.UpdateExportBulkOperation(operations.BulkOperationID, fileKey, exportResponse.FileName);

            var pathToSaveFile = Path.Combine(ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString());
            var destinationPath = Path.Combine(pathToSaveFile, fileKey);
            System.IO.File.WriteAllBytes(destinationPath, exportResponse.byteArray);
            Logger.Current.Informational("Bulk Export File : " + destinationPath);

            Notification notification = new Notification();
            notification.Time = DateTime.Now.ToUniversalTime();
            notification.Status = NotificationStatus.New;
            notification.EntityId = operations.OperationID;
            notification.UserID = operations.UserID;
            notification.ModuleID = (byte)AppModules.Contacts;
            notification.Subject = "Your bulk data export request dated [ " + operations.CreatedOn + " ] is available now for download ";
            notification.Details = "Your bulk data export request dated [ " + operations.CreatedOn + " ] is available now for download ";
            notification.ModuleID = (byte)AppModules.Download;
            notification.DownloadFile = fileKey.Trim();
            userService.AddNotification(new AddNotificationRequest() { Notification = notification });

            SendDownloadNotification(operations);
            return default(Task);
        }

        private void SendDownloadNotification(BulkOperations operation)
        {
            Email senderEmail = new Email();
            Guid? emailLoginToken = null;
            Guid? emailGuid = Guid.NewGuid();
            var primaryEmail = userRepository.GetUserPrimaryEmail(operation.UserID);

            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();

            IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(operation.AccountID, CommunicationType.Mail, MailType.TransactionalEmail);
            if (serviceProviders != null && serviceProviders.Any())
                senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
            if (serviceProviders.FirstOrDefault() != null)
                emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
            else
                throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
            string fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? operation.AccountPrimaryEmail : senderEmail.EmailId;

            string text1 = "<table><tr><td>Hi,</td></tr><tr><td></td></tr><tr></tr><tr><td>This is to notify that your Export request has been processed successfully and is now avaialble for you in the Notification Center under Downloads.</td></tr><tr></tr><tr><td></td></tr><tr><td>Thank You,</td></tr><tr><td>SmartTouch Support.</td></tr></table>";

            string text = "Hi," + "@     This is to notify that your Export request has been processed successfully and is now avaialble for you in the Notification Center under Downloads.@Thank You,@SmartTouch Support.";

            //  mailRequest.Body = "Hi," + "\r\nThis is to notify that your bulk data export request has been processed successfully  - it is now available for you to download from Notification Center under downloads.\r\nThanks";//action.Details;

            text = text.Replace("@", "" + System.Environment.NewLine);
            mailRequest.Body = text1;
            mailRequest.From = fromEmail;
            mailRequest.IsBodyHtml = true;
            mailRequest.Subject = "Your Export Request [ " + operation.CreatedOn + " ] - Ready for Download ";
            mailRequest.To = new List<string>() { primaryEmail };

            mailRequest.TokenGuid = emailLoginToken.Value;
            mailRequest.RequestGuid = emailGuid.Value;

            mailRequest.AccountDomain = operation.AccountDomain;
            MailService mailService = new MailService();
            mailService.Send(mailRequest);
        }

        public UpdateContactViewResponse UpdateContactImage(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact image");
            Image image = Mapper.Map<ImageViewModel, Image>(request.Image);
            int contactId = contactRepository.UpdateContactImage(request.ContactId, image, request.AccountId, request.RequestedBy, request.LastUpdatedOn, request.LastUpdatedBy);
            ReindexContactCreatorInfo(contactId, request.AccountId);
            return new UpdateContactViewResponse { };
        }

        public UpdateContactViewResponse UpdateContactName(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact name");
            if (request.PersonViewModel != null)
            {
                Contact contact = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
                isContactValid(contact);
                Person person = contact as Person;
                SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");

                bool search = contactRepository.CheckIsDeletedContact(request.PersonViewModel.ContactID, request.PersonViewModel.AccountID);
                if (search)
                    throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");
            }

            int contactId = contactRepository.UpdateContactName(request.ContactId, request.FirstName, request.LastName, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);

            contactRepository.ExecuteStoredProc(contactId, 2);

            var currentContact = ReindexContactCreatorInfo(contactId, request.AccountId);

            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = currentContact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.person = Mapper.Map<Person, PersonViewModel>(currentContact as Person);
            contactRepository.PersistContactOutlookSync(currentContact);

            return response;
        }

        public UpdateContactViewResponse UpdateContactTitle(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact title");

            int contactId = contactRepository.UpdateContactTitle(request.ContactId, request.Title, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);
            contactRepository.ExecuteStoredProc(contactId, 2);
            var contact = ReindexContactCreatorInfo(contactId, request.AccountId);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = contact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.person = Mapper.Map<Person, PersonViewModel>(contact as Person);
            contactRepository.PersistContactOutlookSync(contact);
            return response;
        }

        public UpdateContactViewResponse UpdateCompanyName(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the company name");

            Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
            isContactValid(contact);

            Company company = contact as Company;
            SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
            IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Company = company }).Contacts;
            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

            if (duplicateResult.TotalHits > 0)
                throw new UnsupportedOperationException("[|Contact already exists.|]");
            bool search = contactRepository.CheckIsDeletedContact(request.CompanyViewModel.ContactID, request.CompanyViewModel.AccountID);
            if (search)
                throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");

            int contactId = contactRepository.UpdateCompanyName(request.ContactId, request.CompanyName, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);

            contactRepository.ExecuteStoredProc(contactId, 2);

            var currentContact = ReindexContactCreatorInfo(contactId, request.AccountId);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = currentContact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.company = Mapper.Map<Company, CompanyViewModel>(currentContact as Company);
            contactRepository.PersistContactOutlookSync(currentContact);

            return response;
        }

        public UpdateContactViewResponse UpdateContactPhone(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact primary phone");
            if (request.PersonViewModel != null)
            {
                Contact contact = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
                isContactValid(contact);
            }
            else
            {
                Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
                isContactValid(contact);
            }
            int contactId = contactRepository.UpdateContactPhone(request.ContactId, request.Phone, request.AccountId, request.LastUpdatedOn, request.LastUpdatedBy);

            contactRepository.ExecuteStoredProc(contactId, 2);

            var currentContact = ReindexContactCreatorInfo(contactId, request.AccountId);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = currentContact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.person = Mapper.Map<Person, PersonViewModel>(currentContact as Person);

            response.company = Mapper.Map<Company, CompanyViewModel>(currentContact as Company);
            contactRepository.PersistContactOutlookSync(currentContact);

            return response;

        }

        public UpdateContactViewResponse UpdateContactLifecycleStage(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact lifecycle stage");

            int contactId = contactRepository.UpdateContactLifecycleStage(request.ContactId, request.LifecycleStage, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);

            contactRepository.ExecuteStoredProc(contactId, 2);

            if (request.LifecycleStage != request.PreviousLifecycleStage)
                addToTopic(request.ContactId, request.LifecycleStage, request.AccountId);

            var contact = ReindexContactCreatorInfo(contactId, request.AccountId);

            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = contact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            return new UpdateContactViewResponse();

        }

        public UpdateContactViewResponse UpdateContactLeadSource(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact leadSource");
            //  DropdownValue leadsource = Mapper.Map<DropdownValueViewModel, DropdownValue>(request.ContactLeadSource);
            int contactId = contactRepository.UpdateContactLeadSource(request.ContactId, request.ContactLeadSourceId, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);

            contactRepository.ExecuteStoredProc(contactId, 2);

            var contact = ReindexContactCreatorInfo(contactId, request.AccountId);
            addToTopic(contact as Person, request.AccountId);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = contact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            return new UpdateContactViewResponse();
        }

        public UpdateContactViewResponse UpdateContactEmail(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact email");

            if (request.PersonViewModel != null)
            {

                Contact contact = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
                isContactValid(contact);
                Person person = contact as Person;
                SearchParameters parameters = new SearchParameters() { AccountId = person.AccountID };
                SearchResult<Contact> duplicateResult = searchService.DuplicateSearch(person, parameters);

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");

                bool search = contactRepository.CheckIsDeletedContact(request.PersonViewModel.ContactID, request.PersonViewModel.AccountID);
                if (search)
                    throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");
            }

            else
            {
                Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
                isContactValid(contact);

                Company company = contact as Company;
                SearchParameters parameters = new SearchParameters() { AccountId = company.AccountID };
                SearchResult<Contact> duplicateResult = searchService.DuplicateSearch(company, parameters);

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");
                bool search = contactRepository.CheckIsDeletedContact(request.CompanyViewModel.ContactID, request.CompanyViewModel.AccountID);
                if (search)
                    throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");
            }

            if ((request.Email.EmailStatusValue == 0 || request.Email.EmailStatusValue == EmailStatus.NotVerified) && !request.isStAdmin)
            {
                Logger.Current.Informational("Request received for updating email by non ST Admin");
                GetRestResponse getRest = mailGunService.EmailValidate(new GetRestRequest() { Email = request.Email.EmailId });
                JavaScriptSerializer js = new JavaScriptSerializer();
                dynamic restResponse = js.Deserialize<dynamic>(getRest.RestResponse.Content);
                if (restResponse != null)
                {
                    Logger.Current.Informational("Request received from mail-gun successfully");
                    if (restResponse["is_valid"] == true)
                    {
                        request.Email.EmailStatusValue = EmailStatus.Verified;
                    }
                    else
                    {
                        request.Email.EmailStatusValue = EmailStatus.HardBounce;
                    }
                }
                else
                {
                    request.Email.EmailStatusValue = EmailStatus.NotVerified;
                }
            }
            int contactId = contactRepository.UpdateContactEmail(request.ContactId, request.Email, request.AccountId, request.LastUpdatedOn, request.LastUpdatedBy);

            contactRepository.ExecuteStoredProc(contactId, 2);

            Contact Contact = contactRepository.FindBy(contactId, request.AccountId);
            List<int> contactIds = new List<int>();
            contactIds.Add(contactId);
            IEnumerable<ContactCreatorInfo> creatorInfos = contactRepository.GetContactCreatorsInfo(contactIds);

            var creatorInfo = creatorInfos.SingleOrDefault(c => c.ContactId == Contact.Id);
            if (creatorInfo != null && creatorInfo.CreatedOn.HasValue)
                Contact.CreatedOn = creatorInfo.CreatedOn.Value;
            if (creatorInfo != null)
                Contact.CreatedBy = creatorInfo.CreatedBy;
            indexingService.IndexContact(Contact);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = Contact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.person = Mapper.Map<Person, PersonViewModel>(Contact as Person);

            response.company = Mapper.Map<Company, CompanyViewModel>(Contact as Company);
            contactRepository.PersistContactOutlookSync(Contact);

            return response;
        }

        private Contact ReindexContactCreatorInfo(int contactId, int accountId)
        {
            var contact = contactRepository.FindAll(new List<int>() { contactId }).FirstOrDefault();
            indexingService.IndexContact(contact);
            return contact;
        }

        public UpdateContactViewResponse UpdateContactAddresses(UpdateContactViewRequest request)
        {
            Logger.Current.Verbose("Request for update the contact email");
            Address address = Mapper.Map<AddressViewModel, Address>(request.Address);
            address.ApplyNation(contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode));

            int contactId = contactRepository.UpdateContactAddresses(request.ContactId, address, request.LastUpdatedOn, request.LastUpdatedBy, request.AccountId);
            contactRepository.ExecuteStoredProc(contactId, 2);

            var contact = ReindexContactCreatorInfo(contactId, request.AccountId);
            var matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = contact, AccountId = request.AccountId });
            Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);

            UpdateContactViewResponse response = new UpdateContactViewResponse();
            response.person = Mapper.Map<Person, PersonViewModel>(contact as Person);

            response.company = Mapper.Map<Company, CompanyViewModel>(contact as Company);
            contactRepository.PersistContactOutlookSync(contact);
            return response;
        }

        public async Task<GetContactCampaignStatisticsResponse> GetContactCampaignSummary(GetContactCampaignStatisticsRequest request)
        {
            Logger.Current.Verbose("Request received to get the Emails count for the ContactId :" + request.ContactId);

            CampaignStatistics campaignstatistics = await contactRepository.GetContactCampaignSummary(request.ContactId, request.Period, request.AccountId);
            return new GetContactCampaignStatisticsResponse
            {
                Opened = campaignstatistics.Opened,
                Sent = campaignstatistics.Sent,
                Clicked = campaignstatistics.Clicked,
                Delivered = campaignstatistics.Delivered
            };
        }

        public GetOpportunitySummaryResponse GetOpportunitySummary(GetOpportunitySummaryRequest request)
        {
            Logger.Current.Verbose("Request received to get the Opportunity summary for the ContactId :" + request.ContactId);
            GetOpportunitySummaryResponse getOpportunitysummaryResponse = new GetOpportunitySummaryResponse();

            getOpportunitysummaryResponse.OpportunitySummary = contactRepository.GetOpportunitySummary(request.ContactId, request.Period);
            return getOpportunitysummaryResponse;
        }

        public GetPersonsCountResponse GetPersonsCount(GetPersonsCountRequest request)
        {
            GetPersonsCountResponse getPersonsCountResponse = new GetPersonsCountResponse();
            getPersonsCountResponse.PersonsCount = contactRepository.GetPersonsCount(request.ContactId, request.AccountId);
            getPersonsCountResponse.Persons = contactRepository.GetPersonsOfCompany(request.ContactId, request.AccountId);
            return getPersonsCountResponse;
        }

        public GetContactTypeResponse GetContactType(GetContactTypeRequest request)
        {
            Logger.Current.Verbose("Request received to get the contacty type of a contact with ContactID: " + request.Id);
            GetContactTypeResponse getContactTypeResponse = new GetContactTypeResponse();
            getContactTypeResponse.ContactType = contactRepository.GetContactType(request.Id);
            Logger.Current.Informational("Contact type: " + getContactTypeResponse.ContactType);
            return getContactTypeResponse;
        }

        public SearchContactsResponse<T> GetAllContacts<T>(SearchContactsRequest request) where T : IShallowContact
        {
            Logger.Current.Verbose("Request to fetch all contacts received.");
            IEnumerable<Type> types = new List<Type>() { typeof(Person), typeof(Company) };

            return search<T>(request, types, null, true, false);
        }

        public SearchContactsResponse<T> GetPersons<T>(SearchContactsRequest request) where T : IShallowContact
        {
            Logger.Current.Verbose("Request to fetch persons received.");
            IEnumerable<Type> types = new List<Type>() { typeof(Person) };
            return search<T>(request, types, null, false, false);
        }

        public SearchContactsResponse<T> GetCompanies<T>(SearchContactsRequest request) where T : IShallowContact
        {
            Logger.Current.Verbose("Request to fetch companies received.");
            IEnumerable<Type> types = new List<Type>() { typeof(Company) };
            return search<T>(request, types, null, false, false);
        }

        public async Task<ExportPersonsResponse> GetAllContactsByIds(ExportPersonsRequest request)
        {
             Logger.Current.Verbose("Getting all contacts by id to export");
            ExportPersonsResponse resp = new ExportPersonsResponse();

            IEnumerable<int> ownerRequiredFields = new List<int>() { (int)ContactFields.Owner, (int)ContactFields.CreatedBy };
            IEnumerable<int?> OwnerIds = new List<int?>();
            IEnumerable<Owner> Owners = new List<Owner>();
            if (request.ExportViewModel.selectedFields.Select(s => ownerRequiredFields.Contains(s)).Any())
            {
                OwnerIds = request.ExportViewModel.Contacts.Select(s => s.OwnerId);
                Owners = contactRepository.GetUserNames(OwnerIds);
            }
            var selectedFields = request.ExportViewModel.SearchFields.Where(s => request.ExportViewModel.selectedFields.Contains(s.FieldId) && s.FieldId != 42 && s.FieldId != 45 && s.FieldId != 46 && s.FieldId != 47 && s.FieldId != 48 && s.FieldId != 49
                && s.FieldId != 56 && s.FieldId != 57 && s.FieldId != 58 && s.FieldId != 60 && s.FieldId != 63 && s.FieldId != 64 && s.FieldId != 65 && s.FieldId != 66 && s.FieldId != 67)
            .OrderBy(d => d.FieldId).ToList();

            IEnumerable<Field> list = Mapper.Map<IEnumerable<FieldViewModel>, IEnumerable<Field>>(request.ExportViewModel.SearchFields);


            DataTable dt = await GetDataTable(selectedFields, list,
                request.ExportViewModel.Contacts, request.AccountId, Owners, (request.DownLoadAs == "CSV" ? DownloadType.CSV : DownloadType.Excel), request.DateFormat, request.TimeZone);

            string SearchDescription = string.Empty;
            if (request.ExportViewModel.SearchDefinitionId != 0)
            {
                SearchDescription = advancedSearchRepository.GetSearchDescription(request.ExportViewModel.SearchDefinitionId);
                if (!string.IsNullOrEmpty(SearchDescription))
                    SearchDescription = SearchDescription.Replace("<b>", "").Replace("</b>", "");
            }
            ReadExcel exl = new ReadExcel();
            byte[] array = null;
            if (request.ExportViewModel.DownLoadAs == "CSV")
            {
                array = exl.ConvertDataSetToCSV(dt, SearchDescription);
                resp.FileName = "Export.csv";
            }
            else if (request.ExportViewModel.DownLoadAs == "Excel")
            {
                array = exl.ConvertDataSetToExcel(dt, SearchDescription);
                resp.FileName = "Export.xls";
            }
            else
            {
                ReadPDF pdf = new ReadPDF();
                array = pdf.ExportToPdf(dt, SearchDescription);
                resp.FileName = "Export.pdf";
            }
            resp.byteArray = array;

            return resp;
        }

        public GetContactsByIDsResponse GetAllContactsByIds(GetContactsByIDsRequest request)
        {
            GetContactsByIDsResponse response = new GetContactsByIDsResponse();
            response.Contacts = contactRepository.GetContactsByContactIDs(request.ContactIDs);
            return response;
        }

        public GetImportedContactResponse GetImportedContacts(GetImportedContactRequest request)
        {
            Logger.Current.Verbose("Request for fetching recently-viewed contacts by user");
            GetImportedContactResponse response = new GetImportedContactResponse();

            IEnumerable<int> contactsIdList = contactRepository.GetContactsByImport(request.LeadAdapterJobID, request.recordStatus);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public async Task<DataTable> GetDataTable(List<FieldViewModel> selectedColumns, IEnumerable<Field> searchFields,
             IEnumerable<Contact> Contacts, int accountId, IEnumerable<Owner> owners, DownloadType fileType, string dateFormat, string timeZone)
        {
            var dropdowns = await cachingService.GetDropdownValuesAsync(accountId);
            var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                      Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var partnerTypes = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
            var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();

            var customeFieldValueList = new List<CustomFieldValueOptionViewModel>();
            if (selectedColumns.Any(a => a.FieldId > 200))       //Selected columns contains customfields
            {
                customeFieldValueList = customFieldService.GetCustomFieldValueOptions(new GetCustomFieldsValueOptionsRequest() { AccountId = accountId }).CustomFieldValueOptions.ToList();
            }

            var values = new object[selectedColumns.Count()];
            DataTable table = new DataTable();
            foreach (FieldViewModel selectedField in selectedColumns)
            {
                table.Columns.Add(selectedField.Title);
            }

            foreach (var item in Contacts)
            {
                Person person = null;
                Company company = null;
                bool isPerson = false;
                if (item.GetType().Equals(typeof(Person)))
                {
                    person = item as Person;
                    isPerson = true;
                }
                else
                    company = item as Company;

                var primaryAddress = item.Addresses != null ? item.Addresses.Any(a => a.IsDefault) ? item.Addresses.FirstOrDefault(a => a.IsDefault) : null : null;

                for (int i = 0; i < selectedColumns.Count(); i++)
                {
                    var value = "";
                    if (selectedColumns[i].FieldId == 1 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                            value = person.FirstName;
                        else value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 2 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                            value = person.LastName;
                        else value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 3 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (fileType == DownloadType.CSV)
                        {
                            value = String.Format("\"{0}\"", isPerson ? person.CompanyName : company.CompanyName);
                        }
                        else
                        {
                            value = isPerson ? person.CompanyName : company.CompanyName;
                        }
                    }
                    else if (selectedColumns[i].FieldId == 7 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        var primaryEmail = item.Emails != null ? item.Emails.Any(e => e.IsPrimary) ? item.Emails.FirstOrDefault(e => e.IsPrimary).EmailId : string.Empty : string.Empty;
                        value = primaryEmail;
                    }
                    else if (selectedColumns[i].FieldId == 9 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.FacebookUrl != null ? item.FacebookUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 8 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = person != null ? person.Title : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 10 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.TwitterUrl != null ? item.TwitterUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 11 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.LinkedInUrl != null ? item.LinkedInUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 12 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.GooglePlusUrl != null ? item.GooglePlusUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 13 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.WebsiteUrl != null ? item.WebsiteUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 14 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.BlogUrl != null ? item.BlogUrl.URL : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 15 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            if (fileType == DownloadType.CSV)
                            {
                                value = String.Format("\"{0}\"", primaryAddress.AddressLine1);
                            }
                            else
                            {
                                value = primaryAddress.AddressLine1;
                            }
                        }
                    }
                    else if (selectedColumns[i].FieldId == 16 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            if (fileType == DownloadType.CSV)
                            {
                                value = String.Format("\"{0}\"", primaryAddress.AddressLine2);
                            }
                            else
                            {
                                value = primaryAddress.AddressLine2;
                            }
                        }
                    }
                    else if (selectedColumns[i].FieldId == 17 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            if (fileType == DownloadType.CSV)
                            {
                                value = String.Format("\"{0}\"", primaryAddress.City);
                            }
                            else
                            {
                                value = primaryAddress.City;
                            }
                        }
                    }
                    else if (selectedColumns[i].FieldId == 18 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            value = primaryAddress.State.Name;
                        }
                    }
                    else if (selectedColumns[i].FieldId == 19 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            value = primaryAddress.ZipCode;
                        }
                    }
                    else if (selectedColumns[i].FieldId == 20 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (primaryAddress != null)
                        {
                            value = primaryAddress.Country.Name;
                        }
                    }
                    else if (selectedColumns[i].FieldId == 21 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                        {
                            var partnerTypeName = partnerTypes.Where(e => e.DropdownValueID == person.PartnerType).Select(s => s.DropdownValue).FirstOrDefault();
                            value = partnerTypeName;
                        }
                        else
                            value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 22 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        var lifeCycleName = lifecycleStages.Where(e => e.DropdownValueID == item.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
                        value = lifeCycleName;
                    }
                    else if (selectedColumns[i].FieldId == 23 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = item.DoNotEmail ? "Yes" : "No";
                    }
                    else if (selectedColumns[i].FieldId == 24 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        var leadSourceIds = item.LeadSources != null ? item.LeadSources.Where(w => !w.IsPrimary).Select(l => l.Id) : null;
                        var leadsources = string.Empty;
                        if (leadSourceIds != null)
                            leadsources = string.Join("| ", leadSources.Where(e => leadSourceIds.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
                        value = leadsources;
                    }
                    else if (selectedColumns[i].FieldId == 25 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        var ownerName = string.Empty;
                        if (item.OwnerId != 0)
                            ownerName = owners.Where(o => o.OwnerId == item.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
                        value = ownerName;
                    }
                    else if (selectedColumns[i].FieldId == 26 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        value = person != null ? person.LeadScore.ToString() : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 27 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        //value = person != null ? person.CreatedBy.ToString() : string.Empty;
                        if (person != null && person.CreatedBy.HasValue)
                        {
                            var createdBy = string.Empty;
                            if (item.CreatedBy.HasValue)
                                createdBy = owners.Where(o => o.OwnerId == item.CreatedBy.Value).Select(s => s.OwnerName).FirstOrDefault();
                            value = createdBy;
                        }
                        if (company != null && company.CreatedBy.HasValue)
                        {
                            var createdBy = string.Empty;
                            if (item.CreatedBy.HasValue)
                                createdBy = owners.Where(o => o.OwnerId == item.CreatedBy.Value).Select(s => s.OwnerName).FirstOrDefault();
                            value = createdBy;
                        }
                    }
                    else if (selectedColumns[i].FieldId == 28 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                            value = person.CreatedOn.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture);
                        else if (company != null)
                            value = company.CreatedOn.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture);
                    }
                    else if (selectedColumns[i].FieldId == 29 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                            value = person.LastContacted.HasValue == true ? person.LastContacted.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                        else if (company != null)
                            value = company.LastContacted.HasValue == true ? company.LastContacted.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;//(dateFormat + "hh:mm tt", CultureInfo.InvariantCulture) : string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 41 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                            value = await getLastTouchedThrough(person.LastContactedThrough);
                        else if (company != null)
                            value = await getLastTouchedThrough(company.LastContactedThrough);
                    }
                    else if (selectedColumns[i].FieldId == 44 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null && person.FirstContactSource != null)
                            value = await getSourceType((int)person.FirstContactSource.Value);
                        else if (company != null && company.FirstContactSource != null)
                            value = await getSourceType((int)company.FirstContactSource.Value);
                        else
                            value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == 50 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                        {
                            DateTime? leadSourceDate = person.LeadSources.IsAny() ? person.LeadSources.Where(w => !w.IsPrimary).OrderByDescending(d => d.LastUpdatedDate).Select(s => s.LastUpdatedDate).FirstOrDefault() : (DateTime?)null;
                            if (leadSourceDate != default(DateTime) && leadSourceDate != null)
                                value = leadSourceDate.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture);
                            else
                                value = string.Empty;
                        }
                        else
                            value = string.Empty;

                    }
                    else if (selectedColumns[i].FieldId == 51 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                        {
                            var firstLeadSource = person.LeadSources.IsAny() ? person.LeadSources.Where(w => w.IsPrimary).Select(s => s.Id).FirstOrDefault() : (int?)null;
                            value = leadSources.Where(w => w.DropdownValueID == firstLeadSource).Select(s => s.DropdownValue).FirstOrDefault();
                        }
                        else
                            value = string.Empty;

                    }
                    else if (selectedColumns[i].FieldId == 52 && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                        {
                            DateTime? firstLeadSourceData = person.LeadSources.IsAny() ? person.LeadSources.Where(w => w.IsPrimary).Select(s => s.LastUpdatedDate).FirstOrDefault() : (DateTime?)null;
                            if (firstLeadSourceData != default(DateTime) && firstLeadSourceData != null)
                                value = firstLeadSourceData.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture);
                            else
                                value = string.Empty;
                        }
                        else
                            value = string.Empty;

                    }
                    else if (selectedColumns[i].FieldId == (int)ContactFields.EmailStatus && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        int primaryEmailStatus = item.Emails != null ? item.Emails.Any(e => e.IsPrimary) ? (int)item.Emails.FirstOrDefault(e => e.IsPrimary).EmailStatusValue : 0 : 0;
                        value = await getEmailStatus(primaryEmailStatus);
                    }
                    else if (await IsPhoneField(selectedColumns[i].FieldId, searchFields))
                    {
                        value = await GetPhoneNumber(selectedColumns[i].FieldId, item.Phones);
                    }
                    else if (await IsCustomField(selectedColumns[i].FieldId, searchFields))
                    {
                        if (item.CustomFields != null)
                        {
                            var customField = item.CustomFields.Where(w => w.CustomFieldId == selectedColumns[i].FieldId).FirstOrDefault();
                            if (customField != null)
                                value = await GetCustomFieldValue(selectedColumns[i].FieldId, customField, (byte)selectedColumns[i].FieldInputTypeId, accountId, customeFieldValueList, dateFormat);
                            else
                                value = string.Empty;
                        }
                        else
                            value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == (int)ContactFields.NoteSummary && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null && !string.IsNullOrEmpty(person.NoteSummary))
                            value = person.NoteSummary.Replace("\n", ". ");
                        else if (company != null && !string.IsNullOrEmpty(company.NoteSummary))
                            value = company.NoteSummary.Replace("\n", ". ");
                        else value = string.Empty;
                    }
                    else if (selectedColumns[i].FieldId == (int)ContactFields.LastNoteDate && selectedColumns[i].IsDropdownField == false && selectedColumns[i].IsCustomField == false)
                    {
                        if (person != null)
                        {
                            value = person.LastNoteDate != null && person.LastNoteDate != default(DateTime) ? person.LastNoteDate.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture) : "";
                        }
                        else if (company != null)
                            value = company.LastNoteDate != null && company.LastNoteDate != default(DateTime) ? company.LastNoteDate.Value.ToUserDateTime().ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture) : "";
                    }
                    else if (selectedColumns[i].FieldId == 69)
                    {
                        if (person != null)
                            value = person.LastNote;
                        else if (company != null)
                            value = company.LastNote;
                        else value = string.Empty;
                    }
                    values[i] = value;
                }
                table.Rows.Add(values);
            }
            return table;
        }

        public Task<string> GetCustomFieldValue(int fieldId, ContactCustomField customFieldValue, byte fieldInputTypeId, int accountId,
         IEnumerable<CustomFieldValueOptionViewModel> customeFieldValueList, string dateFormat)
        {
            if ((byte)FieldType.checkbox == fieldInputTypeId || (byte)FieldType.radio == fieldInputTypeId ||
                (byte)FieldType.dropdown == fieldInputTypeId || (byte)FieldType.multiselectdropdown == fieldInputTypeId)
            {
                List<string> optionText = new List<string>();
                var fieldValueOption = customFieldValue.Value;

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
            else if ((byte)FieldType.date == fieldInputTypeId || (byte)FieldType.datetime == fieldInputTypeId)
            {
                DateTime value = customFieldValue.Value_Date;
                if (value != DateTime.MinValue) 
                {
                    var customeFieldValue = GetUserTime(value).ToString(dateFormat + " hh:mm tt", CultureInfo.InvariantCulture);
                return Task<string>.Run(() => customeFieldValue);
            }
            else
                    return Task<string>.Run(() => string.Empty);
            }
            else
            {
                Logger.Current.Informational("Customfield value : " + customFieldValue.Value);
                return Task<string>.Run(() => customFieldValue.Value);
            }
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
            return Task<string>.Run(() => customField);
        }

        public Task<bool> IsPhoneField(int fieldId, IEnumerable<Field> searchFields)
        {
            var phoneField = searchFields.Any(f => f.Id == fieldId && f.IsDropdownField == true);
            return Task<bool>.Run(() => phoneField);
        }

        private Task<string> getSourceType(int? sourceType)
        {
            string contactSourceType = string.Empty;
            if (sourceType.HasValue && sourceType.Value > 0)
            {
                contactSourceType = ((ContactSource)sourceType.Value).GetDisplayName();
            }
            return Task<string>.Run(() => contactSourceType);
        }

        private Task<string> getLastTouchedThrough(byte? lastTouched)
        {
            string LastTouchedThrough = string.Empty;
            if (lastTouched.HasValue && lastTouched.Value > 0)
            {
                LastTouchedThrough = ((LastTouchedValues)lastTouched.Value).GetDisplayName();
            }
            return Task<string>.Run(() => LastTouchedThrough);
        }

        private Task<string> getEmailStatus(int status)
        {
            string Status = string.Empty;
            if (status != 0)
            {
                if (status == 50)
                    Status = "Not Verified";
                else if (status == 51)
                    Status = "Verified";
                else if (status == 52)
                    Status = "Soft Bounce";
                else if (status == 53)
                    Status = "Hard Bounce";
                else if (status == 54)
                    Status = "Unsubscribed";
                else if (status == 55)
                    Status = "Subscribed";
                else if (status == 56)
                    Status = "Complained";
                else if (status == 57)
                    Status = "Suppressed";
            }
            return Task<string>.Run(() => Status);
        }

        SearchContactsResponse<T> search<T>(SearchContactsRequest request, IEnumerable<Type> types,
            IList<string> fields, bool matchAll, bool autoComplete) where T : IShallowContact
        {
            SearchContactsResponse<T> response = new SearchContactsResponse<T>();
            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            parameters.Types = types;
            parameters.MatchAll = matchAll;
            parameters.SortField = request.SortFieldType;
            parameters.SortDirection = request.SortDirection;
            parameters.AccountId = request.AccountId;
            parameters.Ids = request.ContactIDs != null ? request.ContactIDs.Select(i => i.ToString()).ToArray() : null;

            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            SearchResult<Contact> searchResult;
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
            if (request.ShowingFieldType == ContactShowingFieldType.MyContacts || request.ShowingFieldType == ContactShowingFieldType.RecentlyViewed)
            {
                List<int> idss = new List<int>();
                idss.Add(0);
                if (!request.ContactIDs.IsAny())
                    request.ContactIDs = idss.ToArray();

                parameters.Ids = request.ContactIDs.Select(i => i.ToString());
            }

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin && request.Module != AppModules.Opportunity)
            {
                int userId = (int)request.RequestedBy;
                searchResult = searchService.Search(request.Query, c => c.OwnerId == userId, parameters);
            }
            else
                searchResult = searchService.Search(request.Query, parameters);

            IEnumerable<Contact> contacts = searchResult.Results;
            Logger.Current.Informational("Search complete, total results:" + searchResult.Results.Count());

            if (contacts == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                if (typeof(T).Equals(typeof(ContactGridEntry)))
                {
                    IEnumerable<T> list = Mapper.Map<IEnumerable<Contact>, IEnumerable<T>>(contacts);

                    foreach (ContactGridEntry contactListEntry in list.Where(p => (p as ContactGridEntry).CreatedBy == request.RequestedBy).Select(c => c as ContactGridEntry))
                        contactListEntry.IsDelete = true;

                    foreach (ContactGridEntry contactListEntry in list.Where(c => !string.IsNullOrEmpty((c as ContactGridEntry).ContactImageUrl)).Select(c => c as ContactGridEntry))
                        contactListEntry.ContactImageUrl = urlService.GetUrl(contactListEntry.AccountID, ImageCategory.ContactProfile,
                            contactListEntry.ContactImageUrl);
                    response.Contacts = list;
                }
                else if (typeof(T).Equals(typeof(ContactListEntry)))
                {
                    IEnumerable<T> list = Mapper.Map<IEnumerable<Contact>, IEnumerable<T>>(contacts);

                    IEnumerable<int?> OwnerIds = list.Select(s => new List<int?>() { (s as ContactListEntry).OwnerId, (s as ContactListEntry).CreatedBy }).SelectMany(i => i);
                    IEnumerable<Owner> Owners = contactRepository.GetUserNames(OwnerIds);

                    var dropdowns = cachingService.GetDropdownValues(request.AccountId);
                    var lifecycleStages = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle).
                              Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                    var partnerTypes = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PartnerType).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                    var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                    foreach (var contact in list)
                    {
                        ContactListEntry entry = contact as ContactListEntry;
                        entry.LifecycleName = lifecycleStages.Where(e => e.DropdownValueID == entry.LifecycleStage).Select(s => s.DropdownValue).FirstOrDefault();
                        entry.PartnerTypeName = partnerTypes.Where(e => e.DropdownValueID == entry.PartnerType).Select(s => s.DropdownValue).FirstOrDefault();
                        entry.OwnerName = Owners.Where(o => o.OwnerId == entry.OwnerId).Select(s => s.OwnerName).FirstOrDefault();
                        entry.CreatedByUser = Owners.Where(o => o.OwnerId == entry.CreatedBy).Select(s => s.OwnerName).FirstOrDefault();
                        if (entry.FirstLeadSourceId != 0)
                            entry.FirstLeadSource = leadSources.Where(w => w.DropdownValueID == entry.FirstLeadSourceId).Select(s => s.DropdownValue).FirstOrDefault();
                        if (entry.LeadSourceIds != null)
                            entry.LeadSources = string.Join(", ", leadSources.Where(e => entry.LeadSourceIds.Contains(e.DropdownValueID)).Select(s => s.DropdownValue));
                        if (request.RequestedBy.HasValue)
                            entry.IsDelete = (entry.CreatedBy == request.RequestedBy.Value) ? true : false;
                    }
                    response.Contacts = list;
                }
                else
                {
                    IEnumerable<T> list = Mapper.Map<IEnumerable<Contact>, IEnumerable<T>>(contacts);
                    response.Contacts = list;
                }
                response.TotalHits = searchResult.TotalHits;
            }
            return response;
        }

        public GetContactsDataResponce GetContactsData(GetContactsDataRequest request)
        {
            GetContactsDataResponce response = new GetContactsDataResponce();
            IEnumerable<Type> types = new List<Type>() { typeof(Person), typeof(Company) };
            SearchParameters parameters = new SearchParameters();
            parameters.Limit = request.Limit;
            parameters.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            parameters.Types = types;
            parameters.MatchAll = true;
            parameters.SortField = request.SortFieldType;
            parameters.SortDirection = request.SortDirection;
            parameters.AccountId = request.AccountId;
            parameters.Ids = request.ContactIDs != null ? request.ContactIDs.Select(i => i.ToString()) : null;

            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            SearchResult<Contact> searchResult;
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
            if (request.ShowingFieldType == ContactShowingFieldType.MyContacts || request.ShowingFieldType == ContactShowingFieldType.RecentlyViewed)
                parameters.Ids = request.ContactIDs.Select(i => i.ToString());

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin && request.Module != AppModules.Opportunity)
            {
                int userId = (int)request.RequestedBy;
                searchResult = searchService.Search(request.Query, c => c.OwnerId == userId, parameters);
            }
            else
                searchResult = searchService.Search(request.Query, parameters);

            IEnumerable<Contact> contacts = searchResult.Results;
            Logger.Current.Informational("Search complete, total results:" + searchResult.Results.Count());

            response.Contacts = contacts;
            return response;
        }

        private IEnumerable<Contact> BulkContactIds(SearchParameters searchParameters, string query, int accountId, int userId, short roleId)
        {
            SearchResult<Contact> searchResult;
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
            if (isPrivate && !isAccountAdmin)
            {
                searchResult = searchService.Search(query, c => c.OwnerId == userId, searchParameters);
            }
            else
                searchResult = searchService.Search(query, searchParameters);

            return searchResult.Results;
        }

        public IEnumerable<Contact> GetAllContactsByCompanyIds(List<int?> Campanyids, int accountId)
        {
            IQueryable<Contact> comapnies = contactRepository.GetAllContactByIds(Campanyids, accountId).ToList().AsQueryable();
            return comapnies;
        }

        public AutoCompleteSearchResponse SearchCompanyByName(AutoCompleteSearchRequest request)
        {
            AutoCompleteSearchResponse response = new AutoCompleteSearchResponse();
            IList<Type> types = new List<Type>() { typeof(Company), typeof(Person) };            

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "companyNameAutoComplete";
            parameters.AccountId = request.AccountId;
            Logger.Current.Informational("Search string:" + request.Query + " Parameters:" + parameters.ToString());

            var result = searchService.AutoCompleteField(request.Query, parameters);
            IEnumerable<Suggestion> results = result.Results.Where(r => r.AccountId == request.AccountId);

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                response.Results = results;
            }
            return response;
        }

        public AutoCompleteSearchResponse SearchContactWithEmailId(AutoCompleteSearchRequest request)
        {
            AutoCompleteSearchResponse response = new AutoCompleteSearchResponse();
            IEnumerable<Suggestion> results = null;
            IList<Type> types = new List<Type>() { typeof(Company), typeof(Person) };

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "emailAutoComplete";
            parameters.AccountId = request.AccountId;
            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            var result = searchService.AutoCompleteField(request.Query, parameters);
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);

            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                results = result.Results.Where(r => r.DocumentOwnedBy == userId);
            }
            else
            {
                results = result.Results.Where(r => r.AccountId == request.AccountId);
            }

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                response.Results = results;
            }
            return response;
        }

        public AutoCompleteSearchResponse SearchContactWithPhone(AutoCompleteSearchRequest request)
        {
            AutoCompleteSearchResponse response = new AutoCompleteSearchResponse();
            IEnumerable<Suggestion> results = null;

            IList<Type> types = new List<Type>() { typeof(Company), typeof(Person) };

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "phoneNumberAutoComplete";
            parameters.AccountId = request.AccountId;
            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());

            var result = searchService.AutoCompleteField(request.Query, parameters);
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);

            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                results = result.Results.Where(r => r.DocumentOwnedBy == userId);
            }
            else
                results = result.Results.Where(r => r.AccountId == request.AccountId);

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
                response.Results = results;
            return response;
        }

        public AutoCompleteSearchResponse SearchContactTitles(AutoCompleteSearchRequest request)
        {
            AutoCompleteSearchResponse response = new AutoCompleteSearchResponse();
            IList<Type> types = new List<Type>() { typeof(Person) };

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "titleAutoComplete";
            parameters.AccountId = request.AccountId;
            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());

            var result = searchService.AutoCompleteField(request.Query, parameters);
            IEnumerable<Suggestion> results = result.Results.Where(r => r.AccountId == request.AccountId);

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                response.Results = results;
            }
            return response;
        }

        public AutoCompleteSearchResponse SearchContactFullName(AutoCompleteSearchRequest request)
        {
            AutoCompleteSearchResponse response = new AutoCompleteSearchResponse();
            IEnumerable<Suggestion> results = null;

            IList<Type> types = new List<Type>() { typeof(Person), typeof(Company) };

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "contactFullNameAutoComplete";
            parameters.AccountId = request.AccountId;

            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());

            var result = searchService.AutoCompleteField(request.Query, parameters);

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                results = result.Results.Where(r => r.DocumentOwnedBy == userId);
            }
            else
                results = result.Results;

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
                response.Results = results;
            return response;
        }

        public DeleteContactResponse DeleteContact(int id)
        {
            DeleteContactResponse response = new DeleteContactResponse();
            Contact contact = contactRepository.FindBy(id);
            contactRepository.Update(contact);
            unitOfWork.Commit();
            return response;
        }

        public GetPersonResponse GetPerson(GetPersonRequest request)
        {
            GetPersonResponse response = new GetPersonResponse();
            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            var contact = contactRepository.FindByContactId(request.Id, request.AccountId);

            if (contact == null)
            {
                response.Exception = GetContactNotFoundException();
            }
            else
            {
                if (contact.LeadSources != null)
                {
                    var dropdowns = cachingService.GetDropdownValues(request.AccountId);
                    var leadSources = dropdowns.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LeadSources).Select(s => s.DropdownValuesList).ToList().FirstOrDefault();
                    
                    foreach (var leadsource in contact.LeadSources)
                    {
                        var ls = leadSources.Where(l => l.DropdownValueID == leadsource.Id).IsAny();
                        leadsource.Value = (ls) ? leadSources.FirstOrDefault(l => l.DropdownValueID == leadsource.Id).DropdownValue : string.Empty;
                    }
                }
                Logger.Current.Informational("CompanyID information : " + contact.CompanyID);
                if (contact.CompanyID != null)
                {
                    contact.CompanyName = contactRepository.GetCompanyNameById((int)contact.CompanyID);
                }
                Logger.Current.Informational("Company name information : " + contact.CompanyName);

                PersonViewModel personViewModel = Mapper.Map<Person, PersonViewModel>(contact as Person);

                if (request.IncludeLastTouched.Equals(true))
                {
                    personViewModel.LastTouchedDate = contact.LastContacted;
                    if (contact.LastContactedThrough.HasValue && contact.LastContactedThrough.Value > 0)
                        personViewModel.LastTouchedType = ((LastTouchedValues)contact.LastContactedThrough).GetDisplayName();

                }

                if (personViewModel.Image != null && !string.IsNullOrEmpty(personViewModel.Image.StorageName))
                {
                    switch (personViewModel.Image.ImageCategoryID)
                    {
                        case ImageCategory.ContactProfile:
                            //Build  profile image path with unique id
                            personViewModel.Image.ImageContent = urlService.GetUrl(personViewModel.AccountID, ImageCategory.ContactProfile, personViewModel.Image.StorageName);
                            break;
                        case ImageCategory.Campaigns:
                            //Build the campaign images path with unique id
                            personViewModel.Image.ImageContent = urlService.GetUrl(personViewModel.AccountID, ImageCategory.Campaigns, personViewModel.Image.StorageName);
                            break;
                        default:
                            break;
                    }
                }
                else
                    personViewModel.Image = new ImageViewModel();

                IEnumerable<Tag> tags = tagRepository.FindByContact(request.Id, request.AccountId);
                personViewModel.LifeCycle = dropdownRepository.GetDropdownFieldValueBy(Convert.ToInt16(personViewModel.LifecycleStage));

                foreach (var address in personViewModel.Addresses)
                    address.AddressType = dropdownRepository.GetDropdownFieldValueBy(address.AddressTypeID);

                personViewModel.TagsList = tags;
                response.PersonViewModel = personViewModel;
                if (request.IncludeCustomFieldTabs == true)
                {
                    GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(response.PersonViewModel.AccountID);
                    response.PersonViewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs; 
                }
            }
            return response;
        }

        public GetAllCustomFieldTabsResponse GetCustomFieldTabs(GetAllCustomFieldTabsRequest request)
        {
            Logger.Current.Verbose(string.Format("Fetching custom fields for contact: {0}", request.AccountId));
            GetAllCustomFieldTabsResponse response = customFieldService.GetAllCustomFieldTabs(request);
            return response;
        }

        void hasAccess(int documentId, int? userId, int accountId, short roleId)
        {
            bool isAccountAdmin = cachingService.IsAccountAdmin(roleId, accountId);
            if (!isAccountAdmin)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, accountId);
                if (isPrivate && !searchService.IsOwnedBy(documentId, userId, accountId))
                    throw new PrivateDataAccessException("Requested user is not authorized to get this contact.");
            }
        }

        private IEnumerable<Email> updateEmailStatuses(IEnumerable<Email> emails, bool isStAdmin)
        {
            if (emails != null)
            {
                foreach (Email email in emails)
                {
                    if ((email.EmailStatusValue == 0 || email.EmailStatusValue == EmailStatus.NotVerified) && !isStAdmin)
                    {
                        try
                        {
                            GetRestResponse response = mailGunService.EmailValidate(new GetRestRequest() { Email = email.EmailId });
                            JavaScriptSerializer js = new JavaScriptSerializer();
                            dynamic restResponse = js.Deserialize<dynamic>(response.RestResponse.Content);
                            if (restResponse != null)
                            {
                                if (restResponse["is_valid"] == true)
                                {
                                    email.EmailStatusValue = EmailStatus.Verified;
                                }
                                else
                                {
                                    email.EmailStatusValue = EmailStatus.HardBounce;
                                }
                            }
                            else
                            {
                                email.EmailStatusValue = EmailStatus.NotVerified;
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Clear();
                            ex.Data.Add("Email", email.EmailId);
                            Logger.Current.Error("An error occured while deserializing data from MailGun ", ex);
                            email.EmailStatusValue = EmailStatus.NotVerified;
                            continue;
                        }
                    }
                }
            }
            return emails;
        }

        public InsertPersonResponse InsertPerson(InsertPersonRequest request)
        {

            if (request.PersonViewModel.SocialMediaUrls != null && (request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
               ))
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };

            if (string.IsNullOrEmpty(request.PersonViewModel.ContactImageUrl) && request.PersonViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.ContactProfile,
                    ViewModel = request.PersonViewModel.Image,
                    AccountId = request.AccountId
                });
                request.PersonViewModel.Image = imageResponse.ImageViewModel;
            }
            else if (!string.IsNullOrEmpty(request.PersonViewModel.ContactImageUrl))
            {
                DownloadImageResponse imageResponse = imageService.DownloadImage(new DownloadImageRequest()
                {
                    ImgCategory = ImageCategory.ContactProfile,
                    ImageInputUrl = request.PersonViewModel.ContactImageUrl,
                    AccountId = request.AccountId
                });

                if (imageResponse.ImageViewModel != null && !string.IsNullOrEmpty(imageResponse.ImageViewModel.StorageName))
                    request.PersonViewModel.Image = imageResponse.ImageViewModel;
                else
                    request.PersonViewModel.Image = null;
            }
            if (request.PersonViewModel.CustomFields != null)
            {
                request.PersonViewModel.CustomFields = request.PersonViewModel.CustomFields.Where(c => !string.IsNullOrEmpty(c.Value)).ToList();
                foreach (var contactcustomfield in request.PersonViewModel.CustomFields)
                {
                    if ((contactcustomfield.FieldInputTypeId == (int)FieldType.date || contactcustomfield.FieldInputTypeId == (int)FieldType.datetime) && !string.IsNullOrEmpty(contactcustomfield.Value))
                    {
                        if (request.PersonViewModel.DateFormat == "MM/dd/yyyy" && contactcustomfield.Value.Contains('/'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "MM-dd-yyyy" && contactcustomfield.Value.Contains('-'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "yyyy-MM-dd" && contactcustomfield.Value.Contains('-'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[0], dateString[1], dateString[2]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "dd/MM/yyyy" && contactcustomfield.Value.Contains('/'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[1], dateString[0]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                    }
                    else if (contactcustomfield.FieldInputTypeId == (int)FieldType.time && !string.IsNullOrEmpty(contactcustomfield.Value))
                    {
                        DateTime date = new DateTime();
                        DateTime.TryParse(contactcustomfield.Value, out date);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(date).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                }
            }

            Contact Person = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
            isContactValid(Person);
            var viewModel = request.PersonViewModel;
            Person person = Person as Person;

            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                SearchParameters parameters = new SearchParameters() { AccountId = person.AccountID };
                SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");
            }
           

            List<string> emailsList = new List<string>();

            if (request.PersonViewModel.Emails.IsAny())
            {
                foreach (Email email in request.PersonViewModel.Emails)
                {
                    if (!String.IsNullOrEmpty(email.EmailId))
                        emailsList.Add(email.EmailId);
                }
            }
            if (emailsList.Distinct().Count() != emailsList.Count)
                throw new UnsupportedOperationException("[|Contact cannot have duplicate Emails|]");

            ContactTableType contactTableType = Mapper.Map<PersonViewModel, ContactTableType>(updateViewModel(request.PersonViewModel));
            GetLocationsByZip(contactTableType.Addresses);

            if (contactTableType.CompanyID != null && contactTableType.CompanyID > 0)
               contactTableType.CompanyID = 0;

            contactTableType.Emails = updateEmailStatuses(contactTableType.Emails, request.isStAdmin);
            ContactTableType contact = contactRepository.InsertAndUpdateContact(contactTableType);
            if (contact.CompanyID.HasValue && contact.CompanyID > 0)
            {
                if (Person.CompanyID != null && Person.CompanyID > 0)
                    contactRepository.ExecuteStoredProc(contact.CompanyID.Value, 2);
                else
                    contactRepository.ExecuteStoredProc(contact.CompanyID.Value, 1);

                Contact newCompanyContact = contactRepository.FindAll(new List<int>() { contact.CompanyID.Value }, true).FirstOrDefault(); //contactRepository.FindByContactId(contact.CompanyID.Value, contact.AccountID);
                newCompanyContact.CreatedBy = newCompanyContact.CreatedBy.HasValue ? newCompanyContact.CreatedBy.Value : viewModel.CreatedBy;
                newCompanyContact.CreatedOn = new DateTime(viewModel.CreatedOn.Ticks - (viewModel.CreatedOn.Ticks % TimeSpan.TicksPerSecond));

                if (request.RequestedFrom != RequestOrigin.Forms)
                    indexingService.IndexContact(newCompanyContact);
                //Indexing Contacts or Lead from Froms by kiran on 23/05/2018- NEXG-3014
                else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                {
                    indexingService.IndexContact(newCompanyContact);
                }
                //End

                if (Person.CompanyID == null)
                {
                    var matchedQueriesResponse = new FindMatchedSavedSearchQueryResponse();
                    if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
                        matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = newCompanyContact, AccountId = request.AccountId });

                    //Find Saved searched query for Lead by kiran on 23/05/2018 - NEXG-3014
                    else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                        matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = newCompanyContact, AccountId = request.AccountId });
                    //end

                    Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);
                }

              
            }

            contactRepository.ExecuteStoredProc(contact.ContactID, 1);

            Contact newContact = contactRepository.FindByContactId(contact.ContactID, contact.AccountID);

            newContact.CreatedBy = newContact.CreatedBy.HasValue ? newContact.CreatedBy.Value : viewModel.CreatedBy;

            var logMessage = string.Format("Adding New Contact::\nviewModel.CreatedOn.Ticks:{0}\nTimeSpan.TicksPerSecond:{1}"
                , viewModel.CreatedOn.Ticks, TimeSpan.TicksPerSecond);
            Logger.Current.Verbose(logMessage);

            newContact.CreatedOn = new DateTime(viewModel.CreatedOn.Ticks - (viewModel.CreatedOn.Ticks % TimeSpan.TicksPerSecond));
            if(newContact.LeadSources != null)
            {
                foreach (var leadSource in newContact.LeadSources)
                    leadSource.LastUpdatedDate = new DateTime(leadSource.LastUpdatedDate.Ticks - (leadSource.LastUpdatedDate.Ticks % TimeSpan.TicksPerSecond));
            }
            
            newContact.CompanyName = request.PersonViewModel.CompanyName;
            newContact.CreatedOn = newContact.LastUpdatedOn ?? DateTime.UtcNow;
            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
                indexingService.IndexContact(newContact);
            //Indexing Contacts or Lead from Froms ro API by kiran on 23/05/2018 - NEXG-3014
            else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                indexingService.IndexContact(newContact);
            //End

            bool isadmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (isadmin == true && request.RequestedBy != newContact.OwnerId && newContact.OwnerId != null)
                addNewOwnerNotification(newContact.Id, newContact.OwnerId.Value, request.ModuleId);

            this.addToTopic(newContact.Id, newContact.LifecycleStage, newContact.AccountID);

            this.addToTopic(newContact as Person, newContact.AccountID);

            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { newContact }.ToList(),
                  new SearchParameters() { AccountId = newContact.AccountID });
                sendMatchedSavedSearchesMessage(matchedQueries, newContact.AccountID);
            }

            //Find Saved searched query for Lead by kiran on 23/05/2018 - NEXG-3014
            else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
            {
                var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { newContact }.ToList(),
                  new SearchParameters() { AccountId = newContact.AccountID });
                sendMatchedSavedSearchesMessage(matchedQueries, newContact.AccountID);
            }
            //End

            return new InsertPersonResponse() { PersonViewModel = Mapper.Map<Person, PersonViewModel>(newContact as Person) };
        }

        private DateTime AdjustTimeZoneOffset(DateTime localTime)
        {
            var offset = this.TimeZoneInstance.GetUtcOffset(localTime).TotalHours;
            var offset2 = TimeZoneInfo.Local.GetUtcOffset(localTime).TotalHours;
            return localTime.AddHours(offset2 - offset).ToUniversalTime();
        }

        private DateTime GetUserTime(DateTime utcTime)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInstance);
        }

        public string TimeZone
        {
            get
            {
                string tz = ((ClaimsPrincipal)Thread.CurrentPrincipal).Claims.Where(c => c.Type == "TimeZone").Select(c => c.Value).FirstOrDefault();
                tz = string.IsNullOrEmpty(tz) ? "Central Standard Time" : tz;
                return tz;
            }
            set
            {
                TimeZoneInstance = null;
                _timeZone = value;
            }
        }
        private string _timeZone;

        public TimeZoneInfo TimeZoneInstance
        {
            get
            {
                if (_timeZoneInstance == null)
                {
                    try
                    {
                        _timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    }
                    catch
                    {
                        TimeZone = "Central Standard Time";
                        _timeZoneInstance = TimeZoneInfo.FindSystemTimeZoneById(TimeZone);
                    }
                }
                return _timeZoneInstance;
            }
            private set { _timeZoneInstance = value; }
        }

        private TimeZoneInfo _timeZoneInstance;

        IEnumerable<Address> GetLocationsByZip(IEnumerable<Address> addresses)
        {
            foreach (Address address in addresses)
            {
                if ((address.State == null && address.Country == null) || (address.State != null && string.IsNullOrEmpty(address.State.Code) && address.Country != null && string.IsNullOrEmpty(address.Country.Code))
                    && string.IsNullOrEmpty(address.City) && !string.IsNullOrEmpty(address.ZipCode))
                {
                    TaxRate taxRate = contactRepository.GetTaxRateBasedOnZipCode(address.ZipCode);
                    if (taxRate != null)
                    {
                        string countryCode = taxRate.CountryID == 1 ? "US" : "CA";
                        Country country = new Country()
                        {
                            Code = countryCode.Trim()
                        };
                        string stateCode = taxRate.CountryID == 1 ? "US-" + taxRate.StateCode : "CA-" + taxRate.StateCode;
                        State state = new State()
                        {
                            Code = stateCode.Trim()
                        };
                        address.City = taxRate.CityName;
                        address.State = state;
                        address.Country = country;
                    }
                }
            }
            return addresses;
        }

        void addToTopic(Person person, int accountId)
        {
            if (!person.LeadSources.IsAny())
                return;
            var messages = new List<TrackMessage>();
            foreach (var leadSource in person.LeadSources)
            {
                var message = new TrackMessage
                {
                    EntityId = person.Id,
                    AccountId = accountId,
                    ContactId = person.Id,
                    LeadScoreConditionType = (int)LeadScoreConditionType.ContactLeadSource,
                    LinkedEntityId = leadSource.Id
                };
                messages.Add(message);
            }
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
        }

        //ChangeLifecycle
        void addToTopic(int contactId, short dropdownValueId, int accountId)
        {
            if (contactId == 0 || dropdownValueId == 0)
                return;
            var messages = new List<TrackMessage>();
            var message = new TrackMessage
            {
                EntityId = dropdownValueId,
                ContactId = contactId,
                AccountId = accountId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactLifecycleChange,
            };
            messages.Add(message);
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
        }

        //ChangeLifeCycle from imports and Laedadapters
        public void addToTopicFromImportsAndLeadadapter(int contactId, short dropdownValueId, int accountId)
        {
            if (contactId == 0 || dropdownValueId == 0)
                return;
            var messages = new List<TrackMessage>();
            var message = new TrackMessage
            {
                EntityId = dropdownValueId,
                ContactId = contactId,
                AccountId = accountId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactLifecycleChange,
            };
            messages.Add(message);
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
        }

        //ChangeOwner
        void addToTopic(int userId, int contactId, int accountId)
        {
            if (userId == 0 || contactId == 0 || accountId == 0)
                return;
            var messages = new List<TrackMessage>();
            var message = new TrackMessage
            {
                AccountId = accountId,
                EntityId = userId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactOwnerChange,
            };
            messages.Add(message);
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
        }

        void sendMatchedSavedSearchesMessage(IEnumerable<QueryMatch> matches, int accountId)
         {
            var messages = new List<TrackMessage>();
            foreach (var match in matches)
            {
                messages.Add(new TrackMessage()
                {
                    AccountId = accountId,
                    EntityId = match.SearchDefinitionId,
                    ContactId = match.DocumentId,
                    LeadScoreConditionType = (int)LeadScoreConditionType.ContactMatchesSavedSearch
                });
                Logger.Current.Informational("Contact matches a saved search, Search-Definitionid :" + match.SearchDefinitionId + ", ContactId : " + match.DocumentId);
            }
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
        }

        public UpdateLastTouchedResponse UpdateLastTouched(UpdateLastTouchedRequest request)
        {
            foreach (var lstTch in request.LastTouchedOn)
            {
                contactRepository.UpdateLastTouched(lstTch.Key, lstTch.Value, request.ModuleId, request.AccountId);
                this.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { lstTch.Key }, Ids = new Dictionary<int, bool>() { { lstTch.Key, true } }.ToLookup(o => o.Key, o => o.Value) });
            }
            return new UpdateLastTouchedResponse();
        }

        public UpdatePersonResponse UpdatePerson(UpdatePersonRequest request)
        {
            if(request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
                hasAccess(request.PersonViewModel.ContactID, request.RequestedBy, request.AccountId, request.RoleId);

            Logger.Current.Informational("Step 1");
            if (request.PersonViewModel.SocialMediaUrls != null && (request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
               request.PersonViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
               ))
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };

            if (string.IsNullOrEmpty(request.PersonViewModel.ContactImageUrl) && request.PersonViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.ContactProfile,
                    ViewModel = request.PersonViewModel.Image,
                    AccountId = request.AccountId
                });

                request.PersonViewModel.Image = imageResponse.ImageViewModel;
            }
            else if (!string.IsNullOrEmpty(request.PersonViewModel.ContactImageUrl))
            {
                DownloadImageResponse imageResponse = imageService.DownloadImage(new DownloadImageRequest()
                {
                    ImgCategory = ImageCategory.ContactProfile,
                    ImageInputUrl = request.PersonViewModel.ContactImageUrl,
                    AccountId = request.AccountId
                });
                if (imageResponse.ImageViewModel != null && !string.IsNullOrEmpty(imageResponse.ImageViewModel.StorageName))
                    request.PersonViewModel.Image = imageResponse.ImageViewModel;
                else
                    request.PersonViewModel.Image = null;
            }
            Logger.Current.Informational("Step 2");

            if (request.PersonViewModel.CustomFields != null)
            {
                Logger.Current.Verbose("Updating custom fields");
                request.PersonViewModel.CustomFields = request.PersonViewModel.CustomFields.Where(w => !string.IsNullOrEmpty(w.Value)).ToList();
                foreach (var contactcustomfield in request.PersonViewModel.CustomFields)
                {
                    Logger.Current.Informational("Step 3");

                    if ((contactcustomfield.FieldInputTypeId == (int)FieldType.date || contactcustomfield.FieldInputTypeId == (int)FieldType.datetime) && !string.IsNullOrEmpty(contactcustomfield.Value))
                    {
                        Logger.Current.Informational("Customfield value : " + contactcustomfield.Value);
                        if (request.PersonViewModel.DateFormat == "MM/dd/yyyy" && contactcustomfield.Value.Contains('/'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "MM-dd-yyyy" && contactcustomfield.Value.Contains('-'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "yyyy-MM-dd" && contactcustomfield.Value.Contains('-'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[0], dateString[1], dateString[2]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                        else if (request.PersonViewModel.DateFormat == "dd/MM/yyyy" && contactcustomfield.Value.Contains('/'))
                        {
                            int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                            DateTime customFieldDate = new DateTime(dateString[2], dateString[1], dateString[0]);
                            contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                        }
                    }
                    else if (contactcustomfield.FieldInputTypeId == (int)FieldType.time && !string.IsNullOrEmpty(contactcustomfield.Value))
                    {
                        DateTime date = new DateTime();
                        DateTime.TryParse(contactcustomfield.Value, out date);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(date).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                }
                Logger.Current.Verbose("Updated custom fields");
            }
            Contact contact = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
            isContactValid(contact);
            var viewModel = request.PersonViewModel;
            Logger.Current.Informational("Step 4");

            Person person = contact as Person;
            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = person }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

                if (duplicateResult != null && duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");
            }
            Logger.Current.Informational("Step 5");

            bool search = contactRepository.CheckIsDeletedContact(request.PersonViewModel.ContactID, request.PersonViewModel.AccountID);
            if (search)
                throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");

            List<string> emailsList = new List<string>();
            if (request.PersonViewModel.Emails.IsAny())
            {
                foreach (Email email in request.PersonViewModel.Emails)
                {
                    if (!String.IsNullOrEmpty(email.EmailId))
                        emailsList.Add(email.EmailId);
                }
            }
            if (emailsList.Distinct().Count() != emailsList.Count)
                throw new UnsupportedOperationException("[|Contact can not have duplicate Emails|]");

            ContactTableType contactTableType = Mapper.Map<PersonViewModel, ContactTableType>(updateViewModel(request.PersonViewModel));
            contactTableType.Emails = updateEmailStatuses(contactTableType.Emails, request.isStAdmin);
            GetLocationsByZip(contactTableType.Addresses);
            Logger.Current.Informational("Step 6");

            if (contactTableType.CompanyID != null && contactTableType.CompanyID > 0)
                contactTableType.CompanyID = null;

            ContactTableType UpdatedContact = contactRepository.InsertAndUpdateContact(contactTableType);
            Logger.Current.Informational("Step 7");

            if (UpdatedContact.CompanyID.HasValue && UpdatedContact.CompanyID > 0)
            {
                if (contact.CompanyID != null && contact.CompanyID > 0)
                    contactRepository.ExecuteStoredProc(UpdatedContact.CompanyID.Value, 2);
                else
                    contactRepository.ExecuteStoredProc(UpdatedContact.CompanyID.Value, 1);

                Contact newCompanyContact = contactRepository.FindAll(new List<int>() { UpdatedContact.CompanyID.Value }, true).FirstOrDefault(); //contactRepository.FindByContactId(UpdatedContact.CompanyID.Value, UpdatedContact.AccountID);
                newCompanyContact.CreatedBy = newCompanyContact.CreatedBy.HasValue ? newCompanyContact.CreatedBy.Value : viewModel.CreatedBy;
                newCompanyContact.CreatedOn = new DateTime(viewModel.CreatedOn.Ticks - (viewModel.CreatedOn.Ticks % TimeSpan.TicksPerSecond));
                if (request.RequestedFrom != RequestOrigin.Forms)
                    indexingService.IndexContact(newCompanyContact);
                //Insert contact into Elastic Search Data by kiran on 23/05/2018 -NEXG-3014
                else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                    indexingService.IndexContact(newCompanyContact);
                //End
                if (contact.CompanyID == null)
                {
                    var matchedQueriesResponse = new FindMatchedSavedSearchQueryResponse();
                    if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
                        matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = newCompanyContact, AccountId = request.AccountId });
                    //Get saved search for contacts by kiran on 23/05/2018 NEXG-3014
                   else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                        matchedQueriesResponse = this.FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = newCompanyContact, AccountId = request.AccountId });
                    //End

                    Logger.Current.Informational("No of saved-search queries matched for the updated contact : " + matchedQueriesResponse.MatchedQueries);
                }
            }
            Logger.Current.Informational("Step 8");

            contactRepository.ExecuteStoredProc(UpdatedContact.ContactID, 2);

            Contact newContact = contactRepository.FindByContactId(UpdatedContact.ContactID, contact.AccountID);
            Logger.Current.Informational("Step 9");
           
            if (request.PersonViewModel.IsLifecycleChanged)
                contactRepository.UpdateLifecycleStage(contact.Id, contact.LifecycleStage);

            if (request.RequestedFrom != RequestOrigin.Outlook)
            {
                contactRepository.PersistContactOutlookSync(newContact);
            }

            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
                this.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { newContact.Id }, Ids = new Dictionary<int, bool>() { { newContact.Id, true } }.ToLookup(o => o.Key, o => o.Value) });
            //Indexing Contacts into Elastic Search by kiran on 23/05/2018 -NEXG -2018
            else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)
                this.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { newContact.Id }, Ids = new Dictionary<int, bool>() { { newContact.Id, true } }.ToLookup(o => o.Key, o => o.Value) });
            //End
            Logger.Current.Informational("Step 10");

            bool isadmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            if (isadmin == true && request.RequestedBy != contact.OwnerId && contact.OwnerId != null && contact.OwnerId != contact.PreviousOwnerId)
                addNewOwnerNotification(contact.Id, contact.OwnerId.Value, request.ModuleId);

            if (request.PersonViewModel.LifecycleStage != request.PersonViewModel.PreviousLifecycleStage)
                addToTopic(request.PersonViewModel.ContactID, request.PersonViewModel.LifecycleStage, request.AccountId);

            Logger.Current.Informational("Step 11");

            this.addToTopic(contact as Person, request.AccountId);
            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { newContact }.ToList(),
                new SearchParameters() { AccountId = newContact.AccountID });
                sendMatchedSavedSearchesMessage(matchedQueries, request.AccountId);
            }
            //Find Macthed SavedSearch Query for Lead who enters by form or api into CRM by kiran on 23/05/2018 NEXG -3014
           else if (request.RequestedFrom == RequestOrigin.Forms || request.RequestedFrom == RequestOrigin.API)            {
               
                var matchedQueriesForms = searchService.FindMatchingQueries(new List<Contact>() { newContact }.ToList(),
                new SearchParameters() { AccountId = newContact.AccountID });
                sendMatchedSavedSearchesMessage(matchedQueriesForms, request.AccountId);
            }
            //End
            return new UpdatePersonResponse() { PersonViewModel = Mapper.Map<Person, PersonViewModel>(newContact as Person) };
        }

        PersonViewModel updateViewModel(PersonViewModel viewModel)
        {
            if (viewModel.Addresses != null && viewModel.Addresses.Count > 0)
            {
                var emptyAddresses = viewModel.Addresses.Where(a => a.ToString() == "-").ToList();
                foreach (AddressViewModel addressViewModel in emptyAddresses)
                    viewModel.Addresses.Remove(addressViewModel);
            }
            return viewModel;
        }

        public GetCompanyResponse GetCompany(GetCompanyRequest request)
        {
            GetCompanyResponse response = new GetCompanyResponse();
            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            Contact contact = contactRepository.FindByContactId(request.Id, request.AccountId);
            if (contact == null)
                response.Exception = GetContactNotFoundException();
            else
            {
                CompanyViewModel companyViewModel = Mapper.Map<Company, CompanyViewModel>(contact as Company);
                if (companyViewModel.Image != null && !string.IsNullOrEmpty(companyViewModel.Image.StorageName))
                {
                    switch (companyViewModel.Image.ImageCategoryID)
                    {
                        case ImageCategory.ContactProfile:
                            companyViewModel.Image.ImageContent = urlService.GetUrl(companyViewModel.AccountID, ImageCategory.ContactProfile, companyViewModel.Image.StorageName);
                            break;
                        case ImageCategory.Campaigns:
                            companyViewModel.Image.ImageContent = urlService.GetUrl(companyViewModel.AccountID, ImageCategory.Campaigns, companyViewModel.Image.StorageName);
                            break;
                        default:
                            companyViewModel.Image.ImageContent = string.Empty;
                            break;
                    }
                }
                else
                    companyViewModel.Image = new ImageViewModel();

                response.CompanyViewModel = companyViewModel;
                foreach (var address in companyViewModel.Addresses)
                    address.AddressType = dropdownRepository.GetDropdownFieldValueBy(address.AddressTypeID);

                GetAllCustomFieldTabsRequest customFieldTabs = new GetAllCustomFieldTabsRequest(response.CompanyViewModel.AccountID);
                response.CompanyViewModel.CustomFieldTabs = customFieldService.GetAllCustomFieldTabs(customFieldTabs).CustomFieldsViewModel.CustomFieldTabs;
                IEnumerable<Tag> tags = tagRepository.FindByContact(request.Id, request.AccountId);
                companyViewModel.TagsList = tags;
                response.CompanyViewModel = companyViewModel;
            }
            return response;
        }

        public InsertCompanyResponse InsertCompany(InsertCompanyRequest request)
        {
            if (request.CompanyViewModel.SocialMediaUrls != null && (request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
               ))
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };

            if (string.IsNullOrEmpty(request.CompanyViewModel.ContactImageUrl) && request.CompanyViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.ContactProfile,
                    ViewModel = request.CompanyViewModel.Image,
                    AccountId = request.AccountId
                });
                request.CompanyViewModel.Image = imageResponse.ImageViewModel;
            }
            else
            {
                DownloadImageResponse imageResponse = imageService.DownloadImage(new DownloadImageRequest()
                {
                    ImgCategory = ImageCategory.ContactProfile,
                    ImageInputUrl = request.CompanyViewModel.ContactImageUrl,
                    AccountId = request.AccountId
                });

                if (imageResponse.ImageViewModel != null && !string.IsNullOrEmpty(imageResponse.ImageViewModel.StorageName))
                    request.CompanyViewModel.Image = imageResponse.ImageViewModel;
                else
                    request.CompanyViewModel.Image = null;

            }

            request.CompanyViewModel.CustomFields = request.CompanyViewModel.CustomFields.Where(c => !string.IsNullOrEmpty(c.Value)).ToList();
            foreach (var contactcustomfield in request.CompanyViewModel.CustomFields)
            {

                if ((contactcustomfield.FieldInputTypeId == (int)FieldType.date || contactcustomfield.FieldInputTypeId == (int)FieldType.datetime) && !string.IsNullOrEmpty(contactcustomfield.Value))
                {
                    if (request.CompanyViewModel.DateFormat == "MM/dd/yyyy" && contactcustomfield.Value.Contains('/'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "MM-dd-yyyy" && contactcustomfield.Value.Contains('-'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "yyyy-MM-dd" && contactcustomfield.Value.Contains('-'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[0], dateString[1], dateString[2]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "dd/MM/yyyy" && contactcustomfield.Value.Contains('/'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[1], dateString[0]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                }
                else if (contactcustomfield.FieldInputTypeId == (int)FieldType.time && !string.IsNullOrEmpty(contactcustomfield.Value))
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(contactcustomfield.Value, out date);
                    contactcustomfield.Value = this.AdjustTimeZoneOffset(date).ToString("yyyy-MM-dd'T'HH:mm:ss");
                }
            }

            request.CompanyViewModel.ContactType = "2";

            Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
            isContactValid(contact);
            GetLocationsByZip(contact.Addresses);

            var viewModel = request.CompanyViewModel;

            Company company = contact as Company;

            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Company = company }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");
            }
           

            List<string> emailsList = new List<string>();
            foreach (Email email in request.CompanyViewModel.Emails)
            {
                if (!String.IsNullOrEmpty(email.EmailId))
                    emailsList.Add(email.EmailId);
            }

            if (emailsList.Distinct().Count() != emailsList.Count)
                throw new UnsupportedOperationException("[|Contact can not have duplicate Emails|]");

            ContactTableType contactTableType = Mapper.Map<CompanyViewModel, ContactTableType>(updateViewModel(request.CompanyViewModel));
            contactTableType.Emails = updateEmailStatuses(contactTableType.Emails, request.isStAdmin);
            GetLocationsByZip(contactTableType.Addresses);
            ContactTableType UpdatedCompany = contactRepository.InsertAndUpdateContact(contactTableType);
            contactRepository.ExecuteStoredProc(UpdatedCompany.CompanyID.Value, 1);

            Contact newCompanyContact = contactRepository.FindAll(new List<int>() { UpdatedCompany.CompanyID.Value }, true).FirstOrDefault(); //contactRepository.FindByContactId(UpdatedCompany.CompanyID.Value, UpdatedCompany.AccountID);

            bool isadmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            if (isadmin == true && request.RequestedBy != contact.OwnerId && contact.OwnerId != null)
                addNewOwnerNotification(contact.Id, contact.OwnerId.Value, request.ModuleId);

            newCompanyContact.CreatedBy = newCompanyContact.CreatedBy.HasValue ? newCompanyContact.CreatedBy.Value : viewModel.CreatedBy;
            newCompanyContact.CreatedOn = new DateTime(viewModel.CreatedOn.Ticks - (viewModel.CreatedOn.Ticks % TimeSpan.TicksPerSecond));
            int id = indexingService.IndexContact(newCompanyContact);
            Logger.Current.Informational("No of contacts successfully indexed : " + id);

            var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { newCompanyContact }.ToList(),
              new SearchParameters() { AccountId = newCompanyContact.AccountID });
            sendMatchedSavedSearchesMessage(matchedQueries, request.AccountId);

            return new InsertCompanyResponse() { CompanyViewModel = Mapper.Map<Company, CompanyViewModel>(newCompanyContact as Company) };
        }

        public FindMatchedSavedSearchQueryResponse FindMatchedSearchQuery(FindMatchedSavedSearchQueryRequest request)
        {
            FindMatchedSavedSearchQueryResponse response = new FindMatchedSavedSearchQueryResponse();
            if (request.Contact != null)
            {
                var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { request.Contact }.ToList(),
                  new SearchParameters() { AccountId = request.AccountId });
                response.MatchedQueries = matchedQueries.Count();
                Logger.Current.Informational("No of matched saved search queries : " + matchedQueries.Count());
                sendMatchedSavedSearchesMessage(matchedQueries, request.AccountId);
            }
            return response;
        }

        public FindMatchedSavedSearchesResponse FindMatchedSavedSearches(FindMatchedSavedSearchesRequest request)
        {
            FindMatchedSavedSearchesResponse response = new FindMatchedSavedSearchesResponse();
            if (request.Contact != null)
            {
                var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { request.Contact }.ToList(),
                  new SearchParameters() { AccountId = request.AccountId });
                response.MatchedSearches = matchedQueries;
            }
            return response;
        }

        public UpdateCompanyResponse UpdateCompany(UpdateCompanyRequest request)
        {
            hasAccess(request.CompanyViewModel.ContactID, request.RequestedBy, request.AccountId, request.RoleId);

            if (request.CompanyViewModel.SocialMediaUrls != null && (request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Facebook").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Twitter").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Website").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "LinkedIn").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Google+").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Blog").Count() > 1 ||
               request.CompanyViewModel.SocialMediaUrls.Where(p => p.MediaType == "Skype").Count() > 1
               ))
            {
                throw new UnsupportedOperationException("[|Multiple web & social media URLs of similar type are not accepted.|]");
            };
            if (string.IsNullOrEmpty(request.CompanyViewModel.ContactImageUrl) && request.CompanyViewModel.Image != null)
            {
                SaveImageResponse imageResponse = imageService.SaveImage(new SaveImageRequest()
                {
                    ImageCategory = ImageCategory.ContactProfile,
                    ViewModel = request.CompanyViewModel.Image,
                    AccountId = request.AccountId
                });
                request.CompanyViewModel.Image = imageResponse.ImageViewModel;
            }
            else
            {
                DownloadImageResponse imageResponse = imageService.DownloadImage(new DownloadImageRequest()
                {
                    ImgCategory = ImageCategory.ContactProfile,
                    ImageInputUrl = request.CompanyViewModel.ContactImageUrl,
                    AccountId = request.AccountId
                });

                if (imageResponse.ImageViewModel != null && !string.IsNullOrEmpty(imageResponse.ImageViewModel.StorageName))
                    request.CompanyViewModel.Image = imageResponse.ImageViewModel;
                else
                    request.CompanyViewModel.Image = null;
            }

            request.CompanyViewModel.CustomFields = request.CompanyViewModel.CustomFields.Where(c => !string.IsNullOrEmpty(c.Value)).ToList();
            foreach (var contactcustomfield in request.CompanyViewModel.CustomFields)
            {

                if ((contactcustomfield.FieldInputTypeId == (int)FieldType.date || contactcustomfield.FieldInputTypeId == (int)FieldType.datetime) && !string.IsNullOrEmpty(contactcustomfield.Value))
                {
                    if (request.CompanyViewModel.DateFormat == "MM/dd/yyyy" && contactcustomfield.Value.Contains('/'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "MM-dd-yyyy" && contactcustomfield.Value.Contains('-'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[0], dateString[1]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "yyyy-MM-dd" && contactcustomfield.Value.Contains('-'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('-').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[0], dateString[1], dateString[2]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                    else if (request.CompanyViewModel.DateFormat == "dd/MM/yyyy" && contactcustomfield.Value.Contains('/'))
                    {
                        int[] dateString = contactcustomfield.Value.Split('/').Select(s => Convert.ToInt32(s)).Select(i => i).ToArray();
                        DateTime customFieldDate = new DateTime(dateString[2], dateString[1], dateString[0]);
                        contactcustomfield.Value = this.AdjustTimeZoneOffset(customFieldDate).ToString("yyyy-MM-dd'T'HH:mm:ss");
                    }
                }
                else if (contactcustomfield.FieldInputTypeId == (int)FieldType.time && !string.IsNullOrEmpty(contactcustomfield.Value))
                {
                    DateTime date = new DateTime();
                    DateTime.TryParse(contactcustomfield.Value, out date);
                    contactcustomfield.Value = this.AdjustTimeZoneOffset(date).ToString("yyyy-MM-dd'T'HH:mm:ss");
                }
            }

            request.CompanyViewModel.ContactType = "2";
            Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
            isContactValid(contact);
            contact.DateFormat = request.CompanyViewModel.DateFormat;

            Company company = contact as Company;

            if (request.RequestedFrom != RequestOrigin.Forms && request.RequestedFrom != RequestOrigin.API)
            {
                SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
                IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Company = company }).Contacts;
                duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };

                if (duplicateResult.TotalHits > 0)
                    throw new UnsupportedOperationException("[|Contact already exists.|]");
            }

            bool search = contactRepository.CheckIsDeletedContact(request.CompanyViewModel.ContactID, request.CompanyViewModel.AccountID);
            if (search)
                throw new UnsupportedOperationException("[|The Contact you are looking for has been deleted.|]");

            List<string> emailsList = new List<string>();
            foreach (Email email in request.CompanyViewModel.Emails)
            {
                if (!String.IsNullOrEmpty(email.EmailId))
                    emailsList.Add(email.EmailId);
            }

            if (emailsList.Distinct().Count() != emailsList.Count)
                throw new UnsupportedOperationException("[|Contact can not have duplicate Emails|]");

            ContactTableType contactTableType = Mapper.Map<CompanyViewModel, ContactTableType>(updateViewModel(request.CompanyViewModel));
            contactTableType.Emails = updateEmailStatuses(contactTableType.Emails, request.isStAdmin);
            GetLocationsByZip(contactTableType.Addresses);
            ContactTableType UpdatedCompany = contactRepository.InsertAndUpdateContact(contactTableType);
            contactRepository.ExecuteStoredProc(UpdatedCompany.CompanyID.Value, 2);

            Contact newCompanyContact = contactRepository.FindByContactId(UpdatedCompany.CompanyID.Value, UpdatedCompany.AccountID);


            bool isadmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);


            if (isadmin == true && request.RequestedBy != contact.OwnerId && contact.OwnerId != null && contact.OwnerId != contact.PreviousOwnerId)
                addNewOwnerNotification(contact.Id, contact.OwnerId.Value, request.ModuleId);
            newCompanyContact.Emails = newCompanyContact.Emails.Where(ce => ce.IsDeleted == false).ToList();
            newCompanyContact.Phones = newCompanyContact.Phones.Where(cp => cp.IsDeleted == false).ToList();
            this.ContactIndexing(new ContactIndexingRequest() { ContactIds = new List<int>() { newCompanyContact.Id }, Ids = new Dictionary<int, bool>() { { newCompanyContact.Id, true } }.ToLookup(o => o.Key, o => o.Value) });

            //var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { newContact }.ToList(),
            //    new SearchParameters() { AccountId = newContact.AccountID });
            //sendMatchedSavedSearchesMessage(matchedQueries, request.AccountId);

            return new UpdateCompanyResponse();
        }

        void addNewOwnerNotification(int contactId, int ownerId, byte moduleId)
        {
            Notification notificationdata = new Notification();
            notificationdata.Details = "A contact has been assigned to you.";
            notificationdata.EntityId = contactId;
            notificationdata.Subject = "A contact has been assigned to you.";
            notificationdata.Time = DateTime.Now.ToUniversalTime();
            notificationdata.Status = NotificationStatus.New;
            notificationdata.UserID = ownerId;
            notificationdata.ModuleID = moduleId == 0 ? (byte)AppModules.Contacts : moduleId;
            userRepository.AddNotification(notificationdata);
        }

        void addNewOwnerNotification(List<int> contactIds, int ownerId, byte moduleId)
        {
            var notifications = new List<Notification>();
            foreach (var id in contactIds)
            {
                Notification notificationdata = new Notification();

                notificationdata.Details = "A contact has been assigned to you.";
                notificationdata.EntityId = id;
                notificationdata.Subject = "A contact has been assigned to you.";
                notificationdata.Time = DateTime.Now.ToUniversalTime();
                notificationdata.Status = NotificationStatus.New;
                notificationdata.UserID = ownerId;
                notificationdata.ModuleID = moduleId;
                notifications.Add(notificationdata);
            }
            userRepository.AddNotification(notifications);
        }

        CompanyViewModel updateViewModel(CompanyViewModel viewModel)
        {
            if (viewModel.Addresses != null)
            {
                var emptyAddresses = viewModel.Addresses.Where(a => a.AddressID == 0
                     && string.IsNullOrEmpty(a.AddressLine1) && string.IsNullOrEmpty(a.AddressLine2)
                     && string.IsNullOrEmpty(a.City) && string.IsNullOrEmpty(a.ZipCode)
                     && (a.Country == null || a.Country.Code.Equals(""))).ToList();

                foreach (AddressViewModel addressViewModel in emptyAddresses)
                    viewModel.Addresses.Remove(addressViewModel);
            }
            return viewModel;
        }

        public RemoveFromElasticResponse RemoveFromElastic(RemoveFromElasticRequest request)
        {
            RemoveFromElasticResponse response = new RemoveFromElasticResponse();
            if (request.ContactIds.IsAny())
                foreach (var contactId in request.ContactIds)
                {
                    var contact = contactRepository.GetDeletedContact(contactId);
                    indexingService.RemoveContact(contactId, contact.AccountID);
                    DeactivateContactForms(new List<int>() { contactId });
                }
            return response;
        }

        public DeactivateContactResponse Deactivate(DeactivateContactRequest request)
        {
            hasAccess(request.Id, request.RequestedBy, request.AccountId, request.RoleId);
            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);

            int? createdUser = contactRepository.FindCreatedUserByContactId(request.Id);

            if (!isAccountAdmin && (createdUser != request.RequestedBy))
            {
                throw new UnsupportedOperationException("[|Restricted Access: You are attempting to delete contact(s) which you have not created.\r\n Contact(s) created by you can only be deleted.|]");
            };
            var companyAssociatedContactIds = contactRepository.GetPersonsOfCompany(request.Id, request.AccountId);
            if (companyAssociatedContactIds.Count > 0)
            {
                throw new UnsupportedOperationException("[|# of Contact(s) are associated with this Company. Unable to delete Company.|]");
            }
            else
            {
                contactRepository.ExecuteStoredProc(request.Id, 3);
                contactRepository.DeactivateContact(request.Id, (int)request.RequestedBy, request.AccountId);
                indexingService.RemoveContact(request.Id, request.AccountId);
                DeactivateContactForms(new List<int>() { request.Id });
            }

            return new DeactivateContactResponse();
        }

        public GetContactFormSubmissionsResponse GetContactSubmittedForms(GetContactFormSubmissionsRequest request)
        {
            GetContactFormSubmissionsResponse response = new GetContactFormSubmissionsResponse();
            if (request.ContactId > 0)
            {
                response.FormSubmissions = contactRepository.GetContactFormSubmissions(request.ContactId);
            }
            return response;
        }

        public void UpdateDeleteBulkData(int operationId, int userId, int accountId, int[] contactIds)
        {
            foreach (int contactid in contactIds)
            {
                indexingService.RemoveContact(contactid, accountId);
            }

            if (contactIds.IsAny())
               accountService.BulkInsertForDeletedContactsInRefreshAnalytics(contactIds);

            GetContactFormSubmissionsResponse formSubmissionResponse = GetContactsFormSubmissionsList(new GetContactFormSubmissionsRequest() { ContactIds = contactIds });

            if (formSubmissionResponse != null && formSubmissionResponse.FormSubmissions != null)
            {
                var submittedForms = formSubmissionResponse.FormSubmissions.Select(c => c.FormId).ToList();
                foreach (int formId in submittedForms)
                {
                    Form form = formRepository.GetFormById(formId);
                    form.HTMLContent = null;
                    if (form.IsDeleted == false)
                        indexingService.Index<Form>(form);
                    Logger.Current.Informational("Indexing Form : FormId - " + form.Id);
                }
            }
        }

        public void DeactivateContactForms(IEnumerable<int> contactIds)
        {
            GetContactFormSubmissionsResponse formSubmissionResponse = GetContactsFormSubmissionsList(new GetContactFormSubmissionsRequest() { ContactIds = contactIds.ToArray() });

            if (formSubmissionResponse != null && formSubmissionResponse.FormSubmissions != null)
            {
                var submittedForms = formSubmissionResponse.FormSubmissions.Select(c => c.FormId).ToList();
                foreach (int formId in submittedForms)
                {
                    Form form = formRepository.GetFormById(formId);
                    form.HTMLContent = null;
                    if (form.IsDeleted == false)
                        indexingService.Index<Form>(form);
                    Logger.Current.Informational("Indexing Form : FormId - " + form.Id);
                }
            }
        }

        public DeactivateContactsResponse DeactivateContacts(DeactivateContactsRequest request)
        {
            int contactsDeleted = 0;
            if (request.ContactIds != null)
            {
                bool isexisted = contactRepository.IsExistedPersonsforCompanies(request.ContactIds, request.AccountId);
                if (isexisted)
                {
                    throw new UnsupportedOperationException("[|# of Contact(s) are associated with this Company(s). Unable to delete Company(s).|]");
                }
                else
                {
                    contactsDeleted = contactRepository.DeactivateContactsList(request.ContactIds, (int)request.RequestedBy, request.AccountId);
                    foreach (int contactid in request.ContactIds)
                    {
                        indexingService.RemoveContact(contactid, request.AccountId);
                        contactRepository.ExecuteStoredProc(contactid, 3);
                    }
                }
            }
            DeactivateContactForms(request.ContactIds);
            
            return new DeactivateContactsResponse() { ContactsDeleted = contactsDeleted };
        }

        public GetContactFormSubmissionsResponse GetContactsFormSubmissionsList(GetContactFormSubmissionsRequest request)
        {
            GetContactFormSubmissionsResponse response = new GetContactFormSubmissionsResponse();
            if (request.ContactIds != null)
            {
                response.FormSubmissions = contactRepository.GetContactsFormSubmissionsList(request.ContactIds);
            }
            return response;
        }

        public GetRecentViwedContactsResponse GetContactByUserId(GetRecentViwedContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching recently-viewed contacts by user");
            GetRecentViwedContactsResponse response = new GetRecentViwedContactsResponse();
            IList<int> contactsIdList = contactRepository.GetContactByUserId(request.UserId, request.ContactIDs, request.AccountId);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public ReIndexContactsResponse ReIndexContacts(ReIndexContactsRequest request)
        {
            int pageNumber = 0;
            int limit = 2000;
            try
            {
                limit = int.Parse(ConfigurationManager.AppSettings["CHUNK_SIZE"].ToString());
            }
            catch { }
            int indexedContacts = 0;
            var response = new GetAccountsResponse();
            if (request.AccountId != 0)
            {
                var arl = new List<AccountViewModel>();
                arl.Add(new AccountViewModel() { AccountID = request.AccountId });
                response.Accounts = arl;
            }
            else
            {
                response = accountService.GetAccounts();
            }
            foreach (int accountId in response.Accounts.Select(a => a.AccountID))
            {
                indexingService.SetupContactIndex(accountId);
                int lastIndexedContact = 0;
                Console.WriteLine("Contact starting from: " + lastIndexedContact);

                int totalContactsIndexed = 0;
                while (true)
                {
                    IEnumerable<Contact> documents = contactRepository.FindAll(pageNumber, limit, accountId, lastIndexedContact);
                    if (!documents.IsAny())
                        break;

                    indexedContacts = indexedContacts + indexingService.ReIndexAll(documents);
                    pageNumber++;
                    lastIndexedContact = documents.Min(c => c.Id);
                    totalContactsIndexed = totalContactsIndexed + documents.Count();
                    Console.WriteLine("Last indexed contact: " + lastIndexedContact);
                    Console.WriteLine("Total contacts indexed : " + totalContactsIndexed);
                    Console.WriteLine("----------------------------------------------------");
                    Console.WriteLine("");

                }
            }
            return new ReIndexContactsResponse() { IndexedContacts = indexedContacts };
        }

        public DeleteIndexResponse DeleteIndex(DeleteIndexRequest request)
        {
            bool result = indexingService.DeleteContactIndex();
            return new DeleteIndexResponse() { Result = result };
        }

        private ResourceNotFoundException GetContactNotFoundException()
        {
            return new ResourceNotFoundException("The requested contact was not found.");
        }

        void isContactValid(Contact contact)
        {
            IEnumerable<BusinessRule> brokenRules = contact.GetBrokenRules();

            if (brokenRules.Any())
            {
                StringBuilder brokenRulesBuilder = new StringBuilder();
                foreach (BusinessRule rule in brokenRules.Distinct())
                    brokenRulesBuilder.AppendLine(rule.RuleDescription + "<br>");

                throw new UnsupportedOperationException(brokenRulesBuilder.ToString());
            }
        }

        public async Task<GetTimeLineResponse> GetTimeLinesDataAsync(GetTimeLineRequest request)
        {
            GetTimeLineResponse response = new GetTimeLineResponse();
            IEnumerable<TimeLineContact> contact = await contactRepository.FindTimelinesAsync2(
                request.ContactID, request.OpportunityID, request.Limit, request.PageNumber,
                request.Module, request.Period, request.PageName, request.Activities, request.FromDate, request.ToDate, request.AccountId);

            contact.ToList().ForEach(w => w.AuditDate = w.AuditDate.ToJSDate());
            TimelineActivityAnalyzer analyser = new TimelineActivityAnalyzer(contact, request.DateFormat);
            IList<TimeLineEntryViewModel> timelines = Mapper.Map<IEnumerable<TimeLineContact>, IEnumerable<TimeLineEntryViewModel>>(analyser.GenerateAnalysis()).ToList();
            response.lsttimelineViewModel = timelines;
            IEnumerable<TimeLineGroup> timeLineGroup;
            if (request.PageNumber == 1)
            {
                response.TotalRecords = contactRepository.FindTimelinesTotalRecords2(request.ContactID, request.OpportunityID,
                    request.Module, request.Period, request.PageName, request.Activities, request.FromDate, request.ToDate, out timeLineGroup, request.AccountId);
                IList<TimeLineGroupViewModel> timelinegroupviewmodel = Mapper.Map<IEnumerable<TimeLineGroup>, IEnumerable<TimeLineGroupViewModel>>(timeLineGroup).ToList();
                response.timeLineGroup = timelinegroupviewmodel;
            }
            return response;
        }

        public AutoCompleteResponse SearchContactFullNameforRelation(AutoCompleteSearchRequest request)
        {
            AutoCompleteResponse response = new AutoCompleteResponse();
            IEnumerable<dynamic> results = null;

            IList<Type> types = new List<Type>() { typeof(Person), typeof(Company) };

            SearchParameters parameters = new SearchParameters();
            parameters.Types = types;
            parameters.AutoCompleteFieldName = "contactFullNameAutoComplete";
            parameters.AccountId = request.AccountId;

            Logger.Current.Informational("Search string:" + request.Query);
            Logger.Current.Informational("Parameters:" + parameters.ToString());
            var result = searchService.AutoCompleteField(request.Query, parameters);

            bool isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate && !isAccountAdmin)
            {
                int userId = (int)request.RequestedBy;
                results = result.Results.Where(r => r.DocumentOwnedBy == userId);
            }
            else
                results = result.Results;

            Logger.Current.Informational("Search complete, total results:" + result.Results.Count());

            if (results == null)
                response.Exception = GetContactNotFoundException();
            else
                response.Results = results;
            return response;
        }

        public DeleteRelationResponse DeleteRelationship(DeleteRelationRequest request)
        {
            contactRepository.DeleteRelation(request.ContactRelationshipMapID);
            unitOfWork.Commit();
            return new DeleteRelationResponse();
        }

        public PersonViewModel CopyPerson(PersonViewModel model)
        {
            if (model != null)
            {
                Logger.Current.Verbose("Request for copying a person");
                model.ContactID = 0;
                model.FirstName = string.Empty;
                model.LastName = string.Empty;
                model.ContactImageUrl = string.Empty;
                model.LastContacted = null;
                model.LastUpdatedBy = 0;
                model.LastUpdatedOn = null;
                model.LeadScore = 0;
                model.Actions = null;
                model.Notes = null;
                model.RelationshipViewModel = null;
                model.Image.ImageContent = null;
                model.Image.StorageName = null;
                model.Tours = null;

                if (model.Phones != null)
                {
                    foreach (Phone phone in model.Phones)
                    {
                        phone.ContactPhoneNumberID = 0;
                        phone.ContactID = 0;
                    }
                }
                if (model.CustomFields != null && model.CustomFields.Count != 0)
                {
                    foreach (ContactCustomFieldMapViewModel customField in model.CustomFields)
                    {
                        customField.ContactId = 0;
                        customField.ContactCustomFieldMapId = 0;
                    }
                }

                if (model.Addresses.IsAny())
                {
                    model.Addresses.Each(a => a.AddressID = 0);
                }
                Logger.Current.Verbose("Copy person successful");
                return model;
            }
            else return null;
        }

        public CompanyViewModel CopyCompany(CompanyViewModel model)
        {
            if (model != null)
            {
                Logger.Current.Verbose("Request for copying a company");
                model.ContactID = 0;
                model.CompanyName = string.Empty;
                model.ContactImageUrl = string.Empty;
                model.LastUpdatedBy = 0;
                model.LastUpdatedOn = null;
                model.Actions = null;
                model.Notes = null;
                model.RelationshipViewModel = null;
                model.Image.ImageContent = null;
                model.Image.StorageName = null;
                if (model.Phones != null)
                {
                    foreach (Phone phone in model.Phones)
                    {
                        phone.ContactPhoneNumberID = 0;
                        phone.ContactID = 0;
                    }
                }
                if (model.Addresses.IsAny())
                {
                    model.Addresses.Each(a => a.AddressID = 0);
                }
                if (model.CustomFields.IsAny())
                {
                    foreach (ContactCustomFieldMapViewModel customField in model.CustomFields)
                    {
                        customField.ContactId = 0;
                        customField.ContactCustomFieldMapId = 0;
                    }
                }
                Logger.Current.Verbose("Copy company successful");
                return model;
            }
            else return null;
        }

        public SendEmailResponse GetEmailSignatures(SendEmailRequest request)
        {
            SendEmailResponse response = new SendEmailResponse();
            IEnumerable<Email> emails = contactRepository.GetEmailSignaturesBy(request.UserID, request.AccountId);
            if (emails == null)
            {
                response.Exception = SendMailsisNotFoundException();
            }
            response.SendMailViewModels = Mapper.Map<IEnumerable<Email>, IEnumerable<SendMailViewModel>>(emails);
            return response;
        }

        private ResourceNotFoundException SendMailsisNotFoundException()
        {
            return new ResourceNotFoundException("The requested SendMails was not found.");
        }

        public GetContactLeadScoreResponse GetLeadScore(GetContactLeadScoreRequest request)
        {
            int leadScore = contactRepository.GetLeadScore(request.ContactId);
            return new GetContactLeadScoreResponse() { LeadScore = leadScore };
        }

        public GetTagContactsResponse GetTagRelatedContacts(GetTagContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching all the contacts that are mapped to the tag");
            GetTagContactsResponse response = new GetTagContactsResponse();
            IEnumerable<int> contactsIdList = contactRepository.GetContactsByTag(request.TagID, request.TagType, request.AccountId);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public GetActionRelatedContactsResponce GetActionRelatedContacts(GetActionRelatedContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching all the contacts that are mapped to the action");
            GetActionRelatedContactsResponce response = new GetActionRelatedContactsResponce();
            IList<int> contactsIdList = contactRepository.GetActionRelatedContacts(request.ActionID);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public GetSearchDefinitionContactsResponce GetSearchDefinitionContacts(GetSearchDefinitionContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching all the contacts that are mapped to the searchDefinition");
            GetSearchDefinitionContactsResponce responce = new GetSearchDefinitionContactsResponce();
            responce.ContactIdList = contactRepository.GetSearchDefinitionActiveContacts(request.ContactIds, request.AccountId);
            Logger.Current.Informational("Contacts Found: " + responce.ContactIdList.Count());
            return responce;
        }

        public GetClientIPAddressResponse GetClientIPAddress(GetClientIPAddressRequest request)
        {
            GetClientIPAddressResponse response = new GetClientIPAddressResponse();
            HttpRequest currentRequest = HttpContext.Current.Request;
            String clientIP = currentRequest.ServerVariables["REMOTE_ADDR"];
            //var stiTrackingId = currentRequest.Cookies["STITrackingID"];
            response.IPAddress = clientIP;
            contactRepository.TrackContactIPAddress(request.ContactId, response.IPAddress, request.STITrackingID);
            return response;
        }

        public ContactLeadScoreListResponse GetContactLeadScoreList(ContactLeadScoreListRequest request)
        {
            ContactLeadScoreListResponse response = new ContactLeadScoreListResponse();
            response.ContactLeadScorelist = Mapper.Map<IEnumerable<Contact>, IEnumerable<ContactLeadScoreListViewModel>>(contactRepository.GetContactLeadScoreList(request.AccountId)).ToList();
            return response;
        }

        public ChangeLifecycleResponse ChangeLifecycle(ChangeLifecycleRequest changeLifecycleRequest)
        {
            ChangeLifecycleResponse response = new ChangeLifecycleResponse();

            Logger.Current.Informational("Request received for changing lifecycle of a contact from a workflow");
            if (changeLifecycleRequest != null)
            {
                contactRepository.ChangeLifecycle(changeLifecycleRequest.dropdownValueId, changeLifecycleRequest.ContactId, changeLifecycleRequest.AccountId);
                addToTopic(changeLifecycleRequest.ContactId, changeLifecycleRequest.dropdownValueId, changeLifecycleRequest.AccountId);
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = new List<int>() { changeLifecycleRequest.ContactId }, IndexType = (int)IndexType.Contacts } });
            }
            return response;
        }

        public AssignUserResponse AssignUser(AssignUserRequest request)
        {
            AssignUserResponse response = new AssignUserResponse();

            Logger.Current.Informational("Request received for changing owner of a contact from workflow");
            if (request != null && request.ContactId != 0)
            {
                var contactIds = new List<int> { request.ContactId };


                //contactRepository.ChangeOwner(request.UserId, request.ContactId);
                //addNewOwnerNotification(contactIds, request.UserId, (byte)AppModules.Contacts);
                accountService.InsertIndexingData(new InsertIndexingDataRequest() { IndexingData = new Domain.Accounts.IndexingData() { EntityIDs = new List<int>() { request.ContactId }, IndexType = (int)IndexType.Contacts } });
            }
            return response;
        }

        public GetContactIDsByIPResponse GetContactIDsByIP(GetContactIDsByIPRequest request)
        {
            GetContactIDsByIPResponse response = new GetContactIDsByIPResponse();

            if (request != null && string.IsNullOrEmpty(request.IPAddress))
            {
                response.ContactIDs = contactRepository.FindContactIdsByIP(request.IPAddress);
            }
            return response;
        }

        public GetContactsByReferenceIdsResponse GetContactsByReferenceIds(GetContactsByReferenceIdsRequest request)
        {
            GetContactsByReferenceIdsResponse response = new GetContactsByReferenceIdsResponse();
            if (request != null && request.ReferenceIds != null && request.ReferenceIds.Any())
            {
                response.ContactIDs = contactRepository.FindContactIdsByReferenceIds(request.ReferenceIds);
            }
            return response;
        }

        public CompareKnownContactIdentitiesResponse CompareKnownIdentities(CompareKnownContactIdentitiesRequest request)
        {
            CompareKnownContactIdentitiesResponse response = new CompareKnownContactIdentitiesResponse();
            if (request != null && request.ReceivedIdentities.Any())
            {
                response.KnownIdentities = contactRepository.FilterKnownIdentities(request.ReceivedIdentities, request.AccountId);
            }
            return response;
        }

        private bool IsAccountAdmin(bool isSTadmin, short roleId, int accountId)
        {
            var isAccountAdmin = false;
            isAccountAdmin = isSTadmin ? true : cachingService.IsAccountAdmin(roleId, accountId);

            if (isAccountAdmin == false)
            {
                isAccountAdmin = (cachingService.IsModulePrivate(AppModules.Contacts, accountId) == false) ? true : false;
            }
            return isAccountAdmin;
        }

        public GetDashboardChartDetailsResponse GetNewLeadsChartDetails(GetDashboardChartDetailsRequest request)
        {
            Logger.Current.Verbose("Request for fetching the NewLeadsAreaChartDetails");

            DashboardChartDetailsViewModel viewModel = new DashboardChartDetailsViewModel();
            GetDashboardChartDetailsResponse response = new GetDashboardChartDetailsResponse();
            DashboardPieChartDetails pieChartDetails = new DashboardPieChartDetails();
            var isAccountAdmin = this.IsAccountAdmin(request.IsSTadmin, request.RoleId, request.AccountId);

            var outpreviousCount = default(int);
            var chartDetails = contactRepository.NewLeadsPieChartDetails(request.AccountId, request.UserId, isAccountAdmin, request.FromDate, request.ToDate);
            viewModel.Chart1Details = contactRepository.NewLeadsAreaChartDetails(request.AccountId, request.UserId, isAccountAdmin, out outpreviousCount, request.FromDate, request.ToDate);
            viewModel.PreviousCount = outpreviousCount;
            if (chartDetails != null && chartDetails.Any())
            {
                var otherDetails = chartDetails.OrderByDescending(d => d.TotalCount).Skip(5);
                var top_5_chartDetils = chartDetails.OrderByDescending(d => d.TotalCount).Take(5);
                if (otherDetails.Any())
                {
                    pieChartDetails.DropdownValue = "Others";
                    pieChartDetails.TotalCount = otherDetails.Sum(od => od.TotalCount);
                    top_5_chartDetils.Append(pieChartDetails);
                }
                viewModel.Chart2Details = top_5_chartDetils;
            }
            response.ChartDetailsViewModel = viewModel;
            return response;
        }

        public GetWorkflowContactsResponse GetWorkflowContacts(GetWorkflowContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contacts in the workflow");
            GetWorkflowContactsResponse response = new GetWorkflowContactsResponse();
            IEnumerable<int> contactsIdList = contactRepository.GetContactsForWorkflow(request.WorkflowID, request.WorkflowContactState);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public async Task<GetContactWebVisitsCountResponse> GetWebVisitsCount(GetContactWebVisitsCountRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contact web visits count for contact id: " + request.ContactId);
            GetContactWebVisitsCountResponse response = new GetContactWebVisitsCountResponse();
            response.WebVisitsCount = await contactRepository.GetContactWebVisitsCount(request.ContactId, request.Period);
            return response;
        }

        public GetContactWebVisitReportResponse GetContactWebVisits(GetContactWebVisitReportRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contact web visits count for contact id: " + request.ContactId);
            GetContactWebVisitReportResponse response = new GetContactWebVisitReportResponse();
            var webVisits = contactRepository.GetContactWebVisits(request.ContactId, request.Period);
            response.WebVisits = Mapper.Map<IEnumerable<WebVisit>, IEnumerable<WebVisitViewModel>>(webVisits);
            return response;
        }

        public async Task<GetContactAuditLeadScoreResponse> GetContactLeadScore(GetContactAuditLeadScoreRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contact lead score count for contact id: " + request.ContactId);
            GetContactAuditLeadScoreResponse response = new GetContactAuditLeadScoreResponse();
            int leadscore = await contactRepository.GetContactLeadScore(request.ContactId, request.Period, request.AccountId);
            response.LeadScore = leadscore;
            return response;
        }

        public GetCampaignContactsResponse GetCampaignContacts(GetCampaignContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contacts in the workflow");
            GetCampaignContactsResponse response = new GetCampaignContactsResponse();

            IEnumerable<int> contactsIdList = contactRepository.GetContactsForCampaign(request.CampaignID, request.CampaignDrillDownActivity, request.AccountId, request.CampaignLinkID);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public GetWorkflowContactsResponse GetWorkflowRelatedContacts(GetWorkflowContactsRequest request)
        {
            Logger.Current.Verbose("Request for fetching the contacts in the workflow");
            GetWorkflowContactsResponse response = new GetWorkflowContactsResponse();

            IEnumerable<int> contactsIdList = contactRepository.GetWorkflowContactsForCampaign(request.WorkflowID, request.CampaignID.Value, request.CampaignDrillDownsActivity, request.FromDate, request.ToDate);
            if (contactsIdList != null)
            {
                response.ContactIdList = contactsIdList;
                return response;
            }
            return null;
        }

        public GetContactsToSyncResponse GetContactsToSync(GetContactsToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the updated contacts to sync for user " + request.RequestedBy);
            GetContactsToSyncResponse response = new GetContactsToSyncResponse();

            IEnumerable<Person> contactsToSync = contactRepository.GetContactsToSync(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp, request.FirstSync, request.OperationType);

            IEnumerable<CRMOutlookSync> crmOutlookSyncViewModels = contactRepository.GetEntityOutlookSyncMap(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp, contactsToSync.Select(c => c.Id));
            if (contactsToSync != null && contactsToSync.Any())
                Logger.Current.Verbose("Syncing contacts:  " + contactsToSync.Select(c => c.Id).ToArray().ToString());

            response.Contacts = Mapper.Map<IEnumerable<Person>, IEnumerable<PersonViewModel>>(contactsToSync);
            response.CRMOutlookSyncMappings = Mapper.Map<IEnumerable<CRMOutlookSync>, IEnumerable<CRMOutlookSyncViewModel>>(crmOutlookSyncViewModels);
            response.CRMOutlookSyncMappings.ToList().ForEach(c => c.Contact = response.Contacts.Where(a => a.ContactID == c.EntityID).FirstOrDefault());

            return response;
        }

        public GetContactsToSyncResponse GetDeletedContactsToSync(GetContactsToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the deleted contacts to sync for user " + request.RequestedBy);
            GetContactsToSyncResponse response = new GetContactsToSyncResponse();

            response.DeletedContacts = contactRepository.GetDeletedContactsToSync(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp);

            return response;
        }

        public UpdateSyncedEntitiesResponse UpdateSyncedEntities(UpdateSyncedEntitiesRequest request)
        {
            Logger.Current.Verbose("Request for updating the synced contacts to sync for user " + request.RequestedBy);
            contactRepository.UpdateSyncedEntities(request.SyncedEntities);
            return new UpdateSyncedEntitiesResponse();
        }

        public InserImplicitSyncEmailInfoResponse InsertImplicitSyncEmailUpload(InserImplicitSyncEmailInfoRequest request)
        {
            MailService mailService = new MailService();
            Guid guid = mailService.InsertImplicitSyncEmailUpload(request.EmailInfo);
            InserImplicitSyncEmailInfoResponse response = new InserImplicitSyncEmailInfoResponse()
            {
                ResponseGuid = guid
            };
            return response;
        }

        public OutllokEmailSentResposne isOutlookEmailAlreadySynced(OutlookEmailSentRequest request)
        {
            MailService mailservice = new MailService();
            bool isEmailAlreadySent = mailservice.isOutlookEmailAlreadySent(request.SentDate);
            OutllokEmailSentResposne resposne = new OutllokEmailSentResposne()
            {
                isEmailAlreadySent = isEmailAlreadySent
            };
            return resposne;
        }

        public FindContactsByPrimaryEmailsResponse FindContactsByPrimaryEmails(FindContactsByPrimaryEmailsRequest request)
        {
            Logger.Current.Verbose("In FindContactsByPrimaryEmails method. AccountId: " + request.AccountId);
            FindContactsByPrimaryEmailsResponse response = new FindContactsByPrimaryEmailsResponse();
            response.ContactIDs = contactRepository.FindContactsByPrimaryEmails(request.Emails, request.AccountId);
            return response;
        }

        public FindContactsByEmailResponse FindContactsByEmail(FindContactsByEmailRequest request)
        {
            Logger.Current.Verbose("In FindContactsByPrimaryEmails method. AccountId: " + request.AccountId);
            FindContactsByEmailResponse response = new FindContactsByEmailResponse();
            response.ContactIDs = contactRepository.FindContactsByEmail(request.Email, request.AccountId);
            return response;
        }

        public async Task<GetEngagementDetailsResponse> GetEngagementInformation(GetEngagementDetailsRequest request)
        {
            Logger.Current.Verbose("Request for fetching the engagement details of contact id: " + request.ContactID);
            GetEngagementDetailsResponse response = new GetEngagementDetailsResponse();
            int leadscore = await contactRepository.GetContactLeadScore(request.ContactID, request.Period, request.AccountId);
            int webvisit = await contactRepository.GetContactWebVisitsCount(request.ContactID, request.Period);
            CampaignStatistics campaignstatistics = await contactRepository.GetContactCampaignSummary(request.ContactID, request.Period, request.AccountId);
            EmailStatistics emailStats = await contactRepository.GetContactEmailStatistics(request.AccountId, request.ContactID, request.Period);
            response.LeadScore = leadscore;
            response.WebVisits = webvisit;
            response.CampaignInfo = new CampaignStats()
            {
                Clicked = campaignstatistics.Clicked,
                Opened = campaignstatistics.Opened,
                Delivered = campaignstatistics.Delivered,
                Sent = campaignstatistics.Sent
            };
            response.EmailInfo = new EmailStats()
            {
                Delivered = emailStats.Delivered,
                Opened = emailStats.Opened,
                Clicked = emailStats.Clicked
            };

            return response;
        }

        public ContactEmailEngagementDetails GetContactEmailEngagementDetails(int contactId, int accountId)
        {
            ContactEmailEngagementDetails details = contactRepository.GetContactEmailEngagementDtails(contactId, accountId);
            return details;

        }

        public GetContactEmailIdResponse GetEmailID(GetContactEmailIdRequest request)
        {
            GetContactEmailIdResponse response = new GetContactEmailIdResponse();
            int ContactEmailId = contactRepository.GetEmailID(request.emailID, request.contactID);
            response.ContactEmailID = ContactEmailId;
            return response;
        }

        public GetContactWebVisitsSummaryResponse GetContactWebVisitsSummary(GetContactWebVisitsSummaryRequest request)
        {
            GetContactWebVisitsSummaryResponse response = new GetContactWebVisitsSummaryResponse();
            int TotalHits = default(int);
            var visits = contactRepository.GetContactWebVisitsSummary(request.ContactId, request.PageNumber, request.PageSize, out TotalHits);
            response.WebVisits = Mapper.Map<IEnumerable<ContactWebVisitSummary>, IEnumerable<ContactWebVisitSummaryViewModel>>(visits);
            response.TotalHits = TotalHits;
            return response;
        }

        public GetWebVisitByVisitIDResponse GetWebVisitByVisitID(GetWebVisitByVisitIDRequest request)
        {
            Logger.Current.Verbose("Request for fetching web visit details for visitId: " + request.ContactWebVisitID);
            GetWebVisitByVisitIDResponse response = new GetWebVisitByVisitIDResponse();
            var visits = contactRepository.GetWebVisitByVisitID(request.ContactWebVisitID);
            var visitsViewModel = new List<WebVisitViewModel>();
            foreach (var visit in visits)
            {
                visitsViewModel.Add(Mapper.Map<WebVisit, WebVisitViewModel>(visit));
            }
            response.WebVisits = visitsViewModel;
            return response;
        }

        public ContactIndexingResponce ContactIndexing(ContactIndexingRequest request)
        {
            ContactIndexingResponce responce = new ContactIndexingResponce();
            IEnumerable<Contact> Contacts = contactRepository.FindAll(request.ContactIds);
            var iterations = Math.Ceiling(Contacts.Count() / 2000m);

            for (var i = 0; i < iterations; i++)
            {
                var contacts = Contacts.Skip(i * 2000).Take(2000);
                indexingService.IndexContacts(contacts);
            }
            Logger.Current.Informational("Indexed contacts successfully, inserting trackmessages");
            var messages = new List<TrackMessage>();
            foreach (var contact in Contacts)
            {
                //NEXG-2664, if no ids then take contact ids from look up.
                var lookups = request.Ids.IsAny() ? request.Ids : request.ContactIds.ToLookup(p => p, p => true);
                var data = lookups.Where(s => s.Key == contact.Id);
                
                if (data.Any(a => a.Any(i => i.Equals(true))))
                {
                    var matchedQueries = searchService.FindMatchingQueries(new List<Contact>() { contact }.ToList(), new SearchParameters() { AccountId = contact.AccountID });
                    Logger.Current.Informational("No of matched saved search queries : " + matchedQueries.Count() + ", ContactId " + contact.Id);

                    foreach (var match in matchedQueries)
                    {
                        messages.Add(new TrackMessage()
                        {
                            AccountId = contact.AccountID,
                            EntityId = match.SearchDefinitionId,
                            ContactId = match.DocumentId,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactMatchesSavedSearch
                        });
                        Logger.Current.Informational("Contact matches a saved search, Search-Definitionid :" + match.SearchDefinitionId + ", ContactId : " + match.DocumentId);
                    }
                }
            }
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest() { Messages = messages });
            return responce;
        }
        
        public ImportDataUpdateResponce ImportDataUpdate(ImportDataUpdateRequest request)
        {
            ImportDataUpdateResponce responce = new ImportDataUpdateResponce();
            IEnumerable<Contact> Contacts = contactRepository.FindAll(request.ContactIds);
            foreach (var contact in Contacts)
            {
                FindMatchedSearchQuery(new FindMatchedSavedSearchQueryRequest() { Contact = contact, AccountId = contact.AccountID });
            }
            insertToTrackMessages(request.TagIds, request.ContactIds, Contacts.Select(k => (int)k.OwnerId).FirstOrDefault(), Contacts.Select(k => k.AccountID).FirstOrDefault());
            return responce;
        }

        private void insertToTrackMessages(IEnumerable<int> tagIds, IEnumerable<int> contactIds, int userId, int accountId)
        {
            if (tagIds.Any() && contactIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    var messages = new List<TrackMessage>();
                    foreach (var contactId in contactIds)
                    {
                        var message = new TrackMessage()
                        {
                            EntityId = tagId,
                            AccountId = accountId,
                            ContactId = contactId,
                            UserId = userId,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactTagAdded,
                            LinkedEntityId = tagId
                        };
                        messages.Add(message);
                    }
                    messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
                    {
                        Messages = messages
                    });
                }
            }
        }

        public GetOutlookEmailInformationResponse GetEmailInformation(GetOutlookEmailInformationRequest request)
        {
            GetOutlookEmailInformationResponse response = new GetOutlookEmailInformationResponse();
            response.OutlookInformation = contactRepository.GetEmailInformation(request.EmailsToUpload, request.AccountId);
            return response;
        }

        public void InsertOutlookEmailAuditInformation(InsertOutlookEmailAuditInformationRequest request)
        {
            contactRepository.InsertOutlookEmailAuditInformation(request.Emails, request.AccountId, (int)request.RequestedBy, request.Guid, request.SentUTCDate);
        }

        public GetContactCreatorsInfoResponse GetContactCreatorsInfo(GetContactCreatorsInfoRequest request)
        {
            GetContactCreatorsInfoResponse response = new GetContactCreatorsInfoResponse();
            response.GetContactCreatorsInfo = contactRepository.GetContactCreatorsInfo(request.ContactIDs);
            return response;
        }

        public GetBulkContactsResponse GetBulkContacts(GetBulkContactsRequest request)
        {
            GetBulkContactsResponse response = new GetBulkContactsResponse();

            SearchParameters parameters = new SearchParameters();

            var splitstring = request.BulkOperations.SearchCriteria.Split('$');

            string sort = string.Empty;
            string name = string.Empty;
            string type = string.Empty;

            if(splitstring.Length > 2)
            {
                sort = splitstring[2] == "null" ? null : splitstring[2];
                name = splitstring[0] == "null" ? null : splitstring[0];
                type = splitstring[1] == "null" ? null : splitstring[1];
            }
            

            if ((string.IsNullOrEmpty(sort) || sort.Equals("0")) && string.IsNullOrEmpty(name) && (!type.Equals("4")))
                sort = "1";
            else if ((string.IsNullOrEmpty(sort) && !string.IsNullOrEmpty(name)) || type.Equals("4"))
                sort = "0";

            IEnumerable<Type> types = type == "" ? new List<Type>() { typeof(Person) } : type == "1" ?
                                   new List<Type>() { typeof(Company) } :
                                   new List<Type>() { typeof(Person), typeof(Company) };


            parameters.Limit = 500000;
            parameters.PageNumber = 1;
            parameters.Types = types;
            parameters.MatchAll = true;
            parameters.SortField = (ContactSortFieldType)Convert.ToInt16(sort);
            parameters.AccountId = request.BulkOperations.AccountID;
            parameters.Ids = request.ContactIds != null ? request.ContactIds.Select(i => i.ToString()) : null;

            Logger.Current.Informational("Request for getting BulkContacts " + request.BulkOperations.AccountID);
            if (request.BulkOperations.OperationType != (int)BulkOperationTypes.Export)
                parameters.Fields = new List<ContactFields>() { ContactFields.ContactId, ContactFields.IsActive };

            IEnumerable<Contact> contacts = BulkContactIds(parameters, name, request.BulkOperations.AccountID, request.BulkOperations.UserID, request.BulkOperations.RoleID);
            response.ContactIds = contacts.Select(p => p.Id).ToArray();

            Logger.Current.Informational("End of Request for getting BulkContacts " + response.ContactIds.Length);

            response.Contacts = contacts;
            return response;
        }

        public FindContactsOfUserByFirstAndLastNameResponse FindContactsOfUserByFirstAndLastName(FindContactsOfUserByFirstAndLastNameRequest request)
        {
            var response = new FindContactsOfUserByFirstAndLastNameResponse();
            response.ContactIds = contactRepository.FindContactsOfUserByFirstAndLastName(request.ContactNames, (int)request.RequestedBy, request.AccountId);
            return response;
        }

        public CheckContactDuplicateResponse CheckIfDuplicate(CheckContactDuplicateRequest request)
        {
            CheckContactDuplicateResponse response = new CheckContactDuplicateResponse();
            IEnumerable<Contact> duplicates = new List<Contact>();
            if (request != null && (request.PersonVM != null || request.CompanyVM != null || request.Person != null || request.Company != null))
            {
                Contact contact = null;
                if (request.PersonVM != null)
                    contact = Mapper.Map<PersonViewModel, Person>(request.PersonVM);
                else if (request.CompanyVM != null)
                    contact = Mapper.Map<CompanyViewModel, Company>(request.CompanyVM);
                else if (request.Person != null)
                    contact = (Contact)request.Person;
                else if (request.Company != null)
                    contact = (Contact)request.Company;

                SearchParameters parameters = new SearchParameters() { AccountId = contact.AccountID };
                SearchResult<Contact> duplicateResult = searchService.DuplicateSearch(contact, parameters);
                if (duplicateResult.Results.IsAny())
                    duplicates = duplicateResult.Results;
                else
                {
                    Person person = null;
                    Company company = null;
                    if (request.PersonVM != null || request.Person != null)
                        person = contact as Person;
                    if (request.CompanyVM != null || request.Company != null)
                        company = contact as Company;

                    string primaryEmail = string.Empty;
                    if (person != null)
                    {
                        primaryEmail = person.Emails != null ? person.Emails.Where(w => w.IsPrimary).Select(s => s.EmailId).FirstOrDefault() : string.Empty;
                        duplicates = contactRepository.CheckDuplicate(person.FirstName, person.LastName, primaryEmail, person.CompanyName, person.Id, contact.AccountID, (byte)ContactType.Person);
                    }

                    if (company != null)
                    {
                        primaryEmail = company.Emails != null ? company.Emails.Where(w => w.IsPrimary).Select(s => s.EmailId).FirstOrDefault() : string.Empty;
                        duplicates = contactRepository.CheckDuplicate(string.Empty, string.Empty, primaryEmail, company.CompanyName, company.Id, contact.AccountID, (byte)ContactType.Company);
                    }

                }
            }
            response.Contacts = duplicates;
            return response;
        }

        public GetContactSummaryResponse GetContactSummary(GetContactSummaryRequest request)
        {
            Logger.Current.Verbose("Fetching contact summary for contactid: {0}" + request.ContactId);
            GetContactSummaryResponse response = new GetContactSummaryResponse();
            response.ContactSummary = contactRepository.GetContactSummary(request.ContactId);
            return response;
        }

        public CustomContactViewModel PersonDuplicateCheck(InsertPersonRequest request)
        {
            CustomContactViewModel customViewModel = new CustomContactViewModel();
            Contact contact = Mapper.Map<PersonViewModel, Person>(updateViewModel(request.PersonViewModel));
            isContactValid(contact);
            Person Person = contact as Person;
            SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
            IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Person = Person }).Contacts;
            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
            customViewModel.HasPermission = true;
            if (duplicateResult.TotalHits > 0)
            {
                var data = duplicateResult.Results.Select(s => s).FirstOrDefault();
                if (data != null)
                {
                    customViewModel.DuplicateContactId = data.Id;
                    customViewModel.Email = data.Emails.Select(e => e.EmailId).FirstOrDefault();
                    if (data.GetType().Equals(typeof(Person)))
                    {
                        Person person = data as Person;
                        customViewModel.Name = person.GetContactName() ?? "";
                    }
                    else
                        customViewModel.Name = data.CompanyName ?? "";

                    try
                    {
                        hasAccess(data.Id, request.RequestedBy, request.AccountId, request.RoleId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while validating user permission", ex);
                        customViewModel.HasPermission = false;
                    }
                }
            }
            return customViewModel;
        }

        public CustomContactViewModel CompanyDuplicateCheck(InsertCompanyRequest request)
        {
            CustomContactViewModel customViewModel = new CustomContactViewModel();
            Contact contact = Mapper.Map<CompanyViewModel, Company>(updateViewModel(request.CompanyViewModel));
            isContactValid(contact);
            Company Company = contact as Company;
            SearchResult<Contact> duplicateResult = new SearchResult<Contact>();
            IEnumerable<Contact> duplicateContacts = this.CheckIfDuplicate(new CheckContactDuplicateRequest() { Company = Company }).Contacts;
            duplicateResult = new SearchResult<Contact>() { Results = duplicateContacts, TotalHits = duplicateContacts != null ? duplicateContacts.Count() : 0 };
            customViewModel.HasPermission = true;

            if (duplicateResult.TotalHits > 0)
            {
                var data = duplicateResult.Results.Select(s => s).FirstOrDefault();
                if (data != null)
                {
                    customViewModel.DuplicateContactId = data.Id;
                    customViewModel.Email = data.Emails.Select(e => e.EmailId).FirstOrDefault();
                    customViewModel.Name = data.CompanyName ?? "";

                    try
                    {
                        hasAccess(data.Id, request.RequestedBy, request.AccountId, request.RoleId);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error("An error occured while validating user permission", ex);
                        customViewModel.HasPermission = false;
                    }
                }
            }
            return customViewModel;
        }

        public UpdateContactCustomFieldResponse UpdateContactCustomField(UpdateContactCustomFieldRequest request)
        {
            Logger.Current.Verbose("In ContactService/UpdateContactCustomField");
            if (request.inputType == (short)FieldType.datetime || request.inputType == (short)FieldType.date || request.inputType == (short)FieldType.time)
            {
                DateTime date = new DateTime();
                DateTime.TryParse(request.Value, out date);
                request.Value = this.AdjustTimeZoneOffset(date).ToString("yyyy-MM-dd'T'HH:mm:ss");
            }
            int result = contactRepository.UpdateContactCustomField(request.ContactId, request.FieldId, request.Value);
            UpdateContactCustomFieldResponse response = new UpdateContactCustomFieldResponse() { Result = result };
            ReindexContactCreatorInfo(request.ContactId, request.AccountId);
            return response;            
        }

        public InsertAPILeadSubmissionResponse InsertAPILeadSubmissionData(InsertAPILeadSubmissionRequest request)
        {
            InsertAPILeadSubmissionResponse response = new InsertAPILeadSubmissionResponse();
            //if (request.ApiLeadSubmissionViewModel.AccountID != 0 && request.ApiLeadSubmissionViewModel.FormID != 0)
            //{
            //    bool isValid = formRepository.IsValidAPISubmission(request.ApiLeadSubmissionViewModel.FormID, request.ApiLeadSubmissionViewModel.AccountID);
            //    if (!isValid)
            //        return new InsertAPILeadSubmissionResponse() { Exception = new UnsupportedOperationException("[|Invalid API Submission|]") };

            APILeadSubmission apiLeadSubmission = Mapper.Map<APILeadSubmissionViewModel, APILeadSubmission>(request.ApiLeadSubmissionViewModel);
            
           int apiLeadSubmissionID = contactRepository.InsertAPILeadSubmissionData(apiLeadSubmission);
            /* Process the API Lead Submission into CRM Leads
             * added by kiran on 30/05/2015 NEXG- 3014*/
            if (apiLeadSubmissionID != 0)
            {
                var task = Task.Run(() => ProcessAPILeadSubmissions(apiLeadSubmissionID));
                task.Wait();
            }
            //End
            //}
            return response;
        }

        public GetAPILeadSubmissionDataResponse GetAPILeadSubMissionData()
        {
            GetAPILeadSubmissionDataResponse response = new GetAPILeadSubmissionDataResponse();
            APILeadSubmission apiLeadSubmissionData = contactRepository.GetAPILeadSubmittedData();
            response.APILeadSubmissionViewModel = Mapper.Map<APILeadSubmission, APILeadSubmissionViewModel>(apiLeadSubmissionData);
            return response;

        }

        /// <summary>
        /// Get API Lead SubmissionData by APILeadSubmissionID NEXG-3014
        /// </summary>
        /// <param name="apiLeadSubmissionID"></param>
        /// <returns></returns>
        public GetAPILeadSubmissionDataResponse GetAPILeadSubMissionData(int apiLeadSubmissionID)
        {
            GetAPILeadSubmissionDataResponse response = new GetAPILeadSubmissionDataResponse();
            APILeadSubmission apiLeadSubmissionData = contactRepository.GetAPILeadSubmittedData(apiLeadSubmissionID);
            response.APILeadSubmissionViewModel = Mapper.Map<APILeadSubmission, APILeadSubmissionViewModel>(apiLeadSubmissionData);
            return response;

        }

        public GetContactEngagementSummaryResponse GetContactWorkflowSummaryDetails(GetContactEngagementSummaryRequest request)
        {
            GetContactEngagementSummaryResponse response = new GetContactEngagementSummaryResponse();
            var records = (request.PageNumber - 1) * request.Limit;
            IEnumerable<ContactWorkflowSummary> workflowDetails = contactRepository.GetContactWorkflowSummaryDetails(request.ContactId, request.AccountId);
            response.ContactWorkflowDetails = workflowDetails.IsAny() ? workflowDetails.Skip(records).Take(request.Limit) : new List<ContactWorkflowSummary>() { };
            response.TotalHits = workflowDetails.IsAny() ? workflowDetails.Count() : 0;
            return response;
        }

        public GetContactEngagementSummaryResponse GetContactEmailSummaryDetails(GetContactEngagementSummaryRequest request)
        {
            GetContactEngagementSummaryResponse response = new GetContactEngagementSummaryResponse();
            var records = (request.PageNumber - 1) * request.Limit;
            IEnumerable<ContactEmailSummary> emailDetails = contactRepository.GetContactEmailSummaryDetails(request.ContactId, request.AccountId);
            if (request.Type == 1)
                emailDetails = emailDetails.Where(o => o.Opened == true).ToList();
            else if (request.Type == 2)
                emailDetails = emailDetails.Where(o => o.Clicked > 0).ToList();

            response.ContactEmailDetails = emailDetails.IsAny() ? emailDetails.Skip(records).Take(request.Limit) : new List<ContactEmailSummary>() { };
            response.TotalHits = emailDetails.IsAny() ? emailDetails.Count() : 0;
            return response;
        }

        public GetContactEngagementSummaryResponse GetContactCampaignSummaryDetails(GetContactEngagementSummaryRequest request)
        {
            GetContactEngagementSummaryResponse response = new GetContactEngagementSummaryResponse();
            var records = (request.PageNumber - 1) * request.Limit;
            IEnumerable<ContactCampaigSummary> campaignDetails = contactRepository.GetContactCampaignSummaryDetails(request.ContactId, request.AccountId);
            if (request.Type == 1)
                campaignDetails = campaignDetails.Where(o => o.OpenStatus == true).ToList();
            else if (request.Type == 2)
                campaignDetails = campaignDetails.Where(o => o.LinksClicked > 0).ToList();

            response.ContactCampaignDetails = campaignDetails.IsAny() ? campaignDetails.Skip(records).Take(request.Limit) : new List<ContactCampaigSummary>() { };
            response.TotalHits = campaignDetails.IsAny() ? campaignDetails.Count() : 0;
            return response;
        }

        public InsertEmailOpenOrClickEntryResponse InsertEmailClickEntry(InsertEmailOpenOrClickEntryRequest request)
        {
            InsertEmailOpenOrClickEntryResponse response = new InsertEmailOpenOrClickEntryResponse();
            HttpRequest currentRequest = HttpContext.Current.Request;
            String clientIP = currentRequest.ServerVariables["REMOTE_ADDR"];
            MailService mailService = new MailService();
            mailService.InsertEmailClickEntry(request.SentMailDetailID, request.ContactID, request.EmailLinkID, request.ActivityType, request.ActivityDate, clientIP);
            return response;
        }

        public GetEmailLinkURLResponse GetSendMailDetailIdByLinkId(GetEmailLinkURLRequest request)
        {
            GetEmailLinkURLResponse response = new GetEmailLinkURLResponse();
            MailService mailService = new MailService();
            var link = mailService.GetSendMailDetailIdByLinkId(request.LinkId);
            if (link != null)
            {
                EmailLinkViewModel emailLinkModel = new EmailLinkViewModel();
                emailLinkModel.EmailLinkId = link.EmailLinkID;
                emailLinkModel.SendMailDetailId = link.SentMailDetailID;
                emailLinkModel.URL = new Url();
                emailLinkModel.URL.URL = link.LinkURL;
                emailLinkModel.LinkIndex = link.LinkIndex;
                response.EmailLinkViewModel = emailLinkModel;
            }
            return response;
        }

        public GetEngagementDetailsResponse GetEmailStastics(GetEngagementDetailsRequest request)
        {
            GetEngagementDetailsResponse response = new GetEngagementDetailsResponse();
            return response;
        }

        public ProcessFullContactResponse ProcessFullContactSync(ProcessFullContactRequest request)
        {
            ProcessFullContactResponse response = new ProcessFullContactResponse();
            int PageNumber = 0;
            int refreshedContacts = 0;
            if (request.AccountId != 0 && request.Limit != 0)
            {
                while (true)
                {
                    try
                    {
                        var contactsDb = contactRepository.GetContacts(request.AccountId, request.Limit, PageNumber);
                        if (!contactsDb.IsAny())
                            break;

                        var updatedContacts = this.FullContactRefresh(contactsDb);
                        if (updatedContacts != null && updatedContacts.Count() > 0)
                        {
                            IEnumerable<int> contactIds = updatedContacts.Select(s => s.Id);
                            Console.WriteLine("List of contacts that were updated : " + string.Join(", ", contactIds));

                            UpdateContact(updatedContacts, request.RequestedBy.Value);
                        }

                        refreshedContacts += updatedContacts.Count();
                        PageNumber++;
                    }
                    catch (Exception Ex)
                    {
                        Console.WriteLine("An error occured : " + Ex.StackTrace.ToString());
                        Logger.Current.Error("An error occured while processing contacts", Ex);
                        continue;
                    }
                }
            }
            return response;
        }

        private IEnumerable<Contact> FullContactRefresh(IEnumerable<Contact> contacts)
        {
            List<Contact> refreshedContacts = new List<Contact>();
            if (contacts != null && contacts.Count() > 0)
            {
                Logger.Current.Informational("No of contacts fetched for update : " + contacts.Count());
                foreach (var contact in contacts.Where(w => !string.IsNullOrEmpty(w.Email)))
                {
                    var updatedContact = ManageContactProfile(contact).FirstOrDefault();
                    if (updatedContact.Value != null && updatedContact.Key == true)
                    {
                        Console.WriteLine("Data was found :" + contact.Id);
                        refreshedContacts.Add(updatedContact.Value);
                    }
                    else
                        Console.WriteLine("No data found for this contact :" + contact.Id);
                }
            }
            return refreshedContacts;
        }

        Dictionary<bool, Contact> ManageContactProfile(Contact contact)
        {
            Logger.Current.Informational("Request for fetching social profile information for : " + contact.ToString());
            bool isChanged = true;
            Dictionary<bool, Contact> personVM = new Dictionary<bool, Contact>();
            Contact updatedModel;
            if (contact.GetType().Equals(typeof(Person)))
                updatedModel = contact as Person;
            else
                updatedModel = contact as Company;

            FullContact fullContact = GetContactData(contact.Email, ContactType.Person);

            try
            {
                if (fullContact != null && fullContact.socialProfiles != null && fullContact.socialProfiles.Count > 0)
                {
                    var facebookUrl = fullContact.socialProfiles.FirstOrDefault(p => p.typeId == "facebook");
                    var twitterUrl = fullContact.socialProfiles.FirstOrDefault(p => p.typeId == "twitter");
                    var linkedinUrl = fullContact.socialProfiles.FirstOrDefault(p => p.typeId == "linkedin");
                    var googleUrl = fullContact.socialProfiles.FirstOrDefault(p => p.typeId == "google");

                    if (facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.url) && updatedModel.FacebookUrl != null)
                    {
                        Logger.Current.Informational("Facebook url : " + facebookUrl.url);
                        updatedModel.FacebookUrl.URL = facebookUrl.url;
                    }
                    else if (updatedModel.FacebookUrl == null && facebookUrl != null && !string.IsNullOrEmpty(facebookUrl.url))
                    {
                        updatedModel.FacebookUrl = new Url() { URL = facebookUrl.url };
                    }

                    if (twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.url) && updatedModel.TwitterUrl != null)
                    {
                        Logger.Current.Informational("Twitter url : " + twitterUrl.url);
                        updatedModel.TwitterUrl.URL = twitterUrl.url;
                    }
                    else if (updatedModel.TwitterUrl == null && twitterUrl != null && !string.IsNullOrEmpty(twitterUrl.url))
                    {
                        updatedModel.TwitterUrl = new Url() { URL = twitterUrl.url };
                    }

                    if (linkedinUrl != null && !string.IsNullOrEmpty(linkedinUrl.url) && updatedModel.LinkedInUrl != null)
                    {
                        Logger.Current.Informational("LinkedIn url : " + linkedinUrl.url);
                        updatedModel.LinkedInUrl.URL = linkedinUrl.url;
                    }
                    else if (updatedModel.LinkedInUrl == null && linkedinUrl != null && !string.IsNullOrEmpty(linkedinUrl.url))
                    {
                        updatedModel.LinkedInUrl = new Url() { URL = linkedinUrl.url };
                    }

                    if (googleUrl != null && !string.IsNullOrEmpty(googleUrl.url) && updatedModel.GooglePlusUrl != null)
                    {
                        Logger.Current.Informational("Google url : " + googleUrl.url);
                        updatedModel.GooglePlusUrl.URL = googleUrl.url;
                    }
                    else if (updatedModel.GooglePlusUrl == null && googleUrl != null && !string.IsNullOrEmpty(googleUrl.url))
                    {
                        updatedModel.GooglePlusUrl = new Url() { URL = googleUrl.url };
                    }
                }
                if (fullContact != null && fullContact.photos != null && fullContact.photos.Count > 0)
                    updatedModel.ImageUrl = fullContact.photos.FirstOrDefault().url;

                if (fullContact != null && fullContact.socialProfiles == null && fullContact.photos == null)
                    isChanged = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured inside ManageContactProfile : " + ex.StackTrace.ToString());
                Logger.Current.Error("An error occured inside ManageContactProfile : ", ex);
            }
            personVM.Add(isChanged, updatedModel);
            return personVM;
        }

        FullContact GetContactData(string emailId, ContactType contactType)
        {
            FullContact fullContact = new FullContact();
            try
            {
                Logger.Current.Informational("Request for getting data from Fullcontact.");
                Logger.Current.Informational("EmailId : " + emailId);
                var client = new RestClient("https://api.fullcontact.com");
                var apiKey = System.Configuration.ConfigurationManager.AppSettings["FullContactKey"];

                var request = new RestRequest("v2/person.json?", Method.GET);
                request.RequestFormat = DataFormat.Json;
                request.AddParameter("email", emailId, ParameterType.GetOrPost);
                request.AddParameter("apiKey", apiKey, ParameterType.GetOrPost);

                request.AddHeader("Content-Type", "application/json;charset=UTF-8");

                fullContact = client.Execute<FullContact>(request).Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured while contacting FullContact : " + ex.StackTrace.ToString());
                Logger.Current.Error("Error occured while requesting Fullcontact : ", ex);
            }
            return fullContact;
        }

        public dynamic DownloadImage(string ImageInputUrl, int accountId, int userId)
        {
            string imageUrl = string.Empty;
            string storagePath = string.Empty;
            var extension = Path.GetExtension(ImageInputUrl) == "" ? ".JPEG" : Path.GetExtension(ImageInputUrl);
            string storageName = Guid.NewGuid().ToString() + extension;
            string fileOriginalName = Path.GetFileName(ImageInputUrl);
            var imagePhysicalPath = ConfigurationManager.AppSettings["IMAGE_PHYSICAL_PATH"].ToString();

            ImagesDb imageDb = new ImagesDb();

            Logger.Current.Informational("Request for downloading an image with url : " + ImageInputUrl + " Length of url : " + ImageInputUrl.Length);
            try
            {
                //Checking for EditMode
                if (!System.IO.File.Exists(imagePhysicalPath + "/" + ImageInputUrl))
                {
                    WebRequest req = WebRequest.Create(ImageInputUrl);
                    req.Timeout = 10000;
                    WebResponse response = req.GetResponse();

                    Stream stream = response.GetResponseStream();
                    System.Drawing.Image Image = System.Drawing.Image.FromStream(stream);


                    using (MemoryStream ms = new MemoryStream())
                    {
                        if (!System.IO.Directory.Exists(imagePhysicalPath + "/" + accountId))
                        {
                            Directory.CreateDirectory(imagePhysicalPath + "/" + accountId);
                        }
                        storagePath = accountId + "/" + "pi";
                        imageDb.FriendlyName = fileOriginalName;
                        imagePhysicalPath = imagePhysicalPath + "/" + storagePath;
                        if (!System.IO.Directory.Exists(imagePhysicalPath))
                        {
                            System.IO.Directory.CreateDirectory(imagePhysicalPath);
                        }
                        imagePhysicalPath = Path.Combine(imagePhysicalPath, storageName);
                        if (!System.IO.File.Exists(imagePhysicalPath))
                        {
                            Image.Save(imagePhysicalPath, ImageFormat.Jpeg);
                        }
                        imageUrl = storageName;
                        //imageDb.ImageContent = imageUrl;
                        imageDb.OriginalName = fileOriginalName;
                        //imageDb.ImageType = Path.GetExtension(ImageInputUrl) == "" ? ".JPEG" : Path.GetExtension(ImageInputUrl);
                        imageDb.StorageName = storageName;
                        imageDb.ImageCategoryID = ImageCategory.ContactProfile;
                        imageDb.AccountID = Convert.ToInt16(accountId);
                        imageDb.CreatedBy = userId;
                        imageDb.CreatedDate = DateTime.UtcNow;
                    }
                }
                else { imageUrl = ImageInputUrl; }
                return imageDb;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occured :" + ex.StackTrace.ToString());
                Logger.Current.Error("An error occured while downloading an image : ", ex);
                return null;
            }
        }

        void UpdateContact(IEnumerable<Contact> contacts, int userId)
        {
            Logger.Current.Informational("Request for updating a contact");
            int updatedCount = 0;
            if (contacts != null && contacts.Count() > 0)
            {
                foreach (var contact in contacts)
                {

                    if (!string.IsNullOrEmpty(contact.ImageUrl))
                    {
                        Console.WriteLine("Contact image found ");
                        var imagesDb = DownloadImage(contact.ImageUrl, contact.AccountID, userId);
                        if (imagesDb != null)
                        {
                            int imageId = contactRepository.UpdateImage(imagesDb, userId);
                            contact.ContactImage.Id = imageId;
                        }
                    }
                    Communication communication = this.getContactCommunication(contact);
                    int communicationId = contactRepository.InsertAndUpdateCommunication(communication, contact.Id, contact.AccountID);
                    contactRepository.UpdateContact(contact, communicationId);
                }
                Logger.Current.Informational("No of records updated : " + updatedCount);
            }
        }

        private Communication getContactCommunication(Contact contact)
        {
            Communication commu = new Communication();
            if (contact != null)
            {
                commu.FacebookUrl = !string.IsNullOrEmpty(contact.FacebookLink) ? contact.FacebookLink : null;
                commu.TwitterUrl = !string.IsNullOrEmpty(contact.TwitterLink) ? contact.TwitterLink : null;
                commu.GooglePlusUrl = !string.IsNullOrEmpty(contact.GooglePlusLink) ? contact.GooglePlusLink : null;
                commu.LinkedInUrl = !string.IsNullOrEmpty(contact.LinkedInLink) ? contact.LinkedInLink : null;
            }
            return commu;
        }

        public GetNeverBounceBadEmailContactResponse GetNeverBounceBadEmailContacts(GetNeverBounceBadEmailContactRequest request)
        {
            GetNeverBounceBadEmailContactResponse response = new GetNeverBounceBadEmailContactResponse();
            response.ContactIdList = contactRepository.GetNeverBounceBadEmailContactIds(request.NeverbounceRequestID, request.EmailStatus);
            return response;
        }

        public List<LinkClickedDetails> GetEmailClickedLinkURLs(int sentMailDetailedId,int contactId)
        {
            return contactRepository.GetEmailClickedLinkURLs(sentMailDetailedId, contactId);

        }

        public List<LinkClickedDetails> GetCampaignClickedLinkURLs(int campaignId, int recipientId)
        {
            return contactRepository.GetCampaignClickedLinkURLs(campaignId, recipientId);
        }

        #region Process API Lead Submissions added by kiran on 30/05/2015 NEXG-3014

        /// <summary>
        /// Process API Lead Submissions into CRM Leads NEXG-3014
        /// </summary>
        /// <param name="apiLeadSubmissionID"></param>
        public void ProcessAPILeadSubmissions(int apiLeadSubmissionID)
        {
            Logger.Current.Informational("Entering into APILeadSubmission processor");
            try
            {
               // if (!IsAPILeadProcessing)
               // {
                   // IsAPILeadProcessing = true;
                    GetAPILeadSubmissionDataResponse response = new GetAPILeadSubmissionDataResponse();
                    response = GetAPILeadSubMissionData(apiLeadSubmissionID);
                    string spamRemarks = string.Empty;
                    if (response != null && response.APILeadSubmissionViewModel != null)
                    {
                        try
                        {
                            int contactID = 0;
                            short roleId = userRepository.GettingRoleIDByUserID(response.APILeadSubmissionViewModel.OwnerID);
                            PersonViewModel viewModel = JsonConvert.DeserializeObject<PersonViewModel>(response.APILeadSubmissionViewModel.SubmittedData);
                            viewModel.AccountID = response.APILeadSubmissionViewModel.AccountID;
                            viewModel.OwnerId = response.APILeadSubmissionViewModel.OwnerID;
                            viewModel.LastUpdatedBy = response.APILeadSubmissionViewModel.OwnerID;
                            viewModel.FirstName = !string.IsNullOrEmpty(viewModel.FirstName) ? viewModel.FirstName.Trim() : viewModel.FirstName;
                            viewModel.LastName = !string.IsNullOrEmpty(viewModel.LastName) ? viewModel.LastName.Trim() : viewModel.LastName;

                            if (viewModel.Phones.IsAny())
                            {
                                viewModel.Phones.Each(p =>
                                {
                                    p.Number = !string.IsNullOrEmpty(p.Number) ? p.Number.Trim() : p.Number;
                                });
                            }

                            if (viewModel.CustomFields.IsAny())
                            {
                                viewModel.CustomFields.Each(cm =>
                                {
                                    cm.Value = !string.IsNullOrEmpty(cm.Value) ? cm.Value.Trim() : cm.Value;
                                });
                            }

                            Dictionary<string, string> fields = GetContactFields(viewModel);
                            bool isSpam = findSpamService.SpamCheck(fields, viewModel.AccountID, response.APILeadSubmissionViewModel.IPAddress, 0, false, out spamRemarks);
                            if (isSpam && spamRemarks == "Invalid MobilePhone")
                            {
                                isSpam = false;
                                spamRemarks = "";
                            }
                            if (!isSpam)
                            {
                                #region NotSpam
                                var dropdownValues = dropdownRepository.FindAll("", 10, 1, viewModel.AccountID);
                                IEnumerable<DropdownViewModel> dropdownViewModel = Mapper.Map<IEnumerable<Dropdown>, IEnumerable<DropdownViewModel>>(dropdownValues);

                                string primaryEmail = viewModel.Emails.IsAny() ? viewModel.Emails.Where(e => e.IsPrimary).Select(s => s.EmailId).FirstOrDefault() : "";

                                if (string.IsNullOrEmpty(viewModel.FirstName) && string.IsNullOrEmpty(viewModel.LastName) && !string.IsNullOrEmpty(primaryEmail) && !IsValidEmail(primaryEmail))
                                {
                                    contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Fail, "Bad Email", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    return;
                                }

                                var duplicatesResponse = CheckIfDuplicate(new CheckContactDuplicateRequest() { PersonVM = viewModel });

                                var leadSourceField = viewModel.SelectedLeadSource.IsAny() ? viewModel.SelectedLeadSource.FirstOrDefault() : null;
                                var communityField = viewModel.Communities.IsAny() ? viewModel.Communities.FirstOrDefault() : null;
                                var leadSourceDropDown = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LeadSources).FirstOrDefault().DropdownValuesList;
                                var leadSourceDropDownValue = new DropdownValueViewModel();
                                if (leadSourceField != null)
                                {
                                    Logger.Current.Verbose("Attempting to fetch Lead Source drop down value submitted");
                                    leadSourceDropDownValue = leadSourceDropDown.Where(e => e.DropdownValueID == leadSourceField.DropdownValueID).FirstOrDefault();
                                }

                                if (leadSourceDropDownValue == null || leadSourceField == null)
                                {
                                    Logger.Current.Verbose("Attempting to fetch Next Gen account default Lead Source drop down value");
                                    leadSourceDropDownValue = leadSourceDropDown.Where(e => e.IsDefault).FirstOrDefault();
                                    if (leadSourceDropDownValue == null)
                                    {
                                        Logger.Current.Verbose("Attempting to fetch First Lead Source drop down value");
                                        leadSourceDropDownValue = leadSourceDropDown.FirstOrDefault();
                                        if (leadSourceDropDownValue == null)
                                            throw new UnsupportedOperationException("[|The accound do not have the specified or any lead source configured. Please contact administrator|]");
                                    }
                                }

                                DropdownValueViewModel leadSourceDropdownViewModel = new DropdownValueViewModel()
                                {
                                    AccountID = viewModel.AccountID,
                                    DropdownID = (byte)ContactFields.LeadSource,
                                    DropdownValue = leadSourceDropDownValue.DropdownValue,
                                    DropdownValueID = leadSourceDropDownValue.DropdownValueID
                                };

                                var communityDropdownViewModel = new DropdownValueViewModel();
                                if (communityField != null)
                                {
                                    IEnumerable<DropdownValueViewModel> communities = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.Community).FirstOrDefault().DropdownValuesList;
                                    DropdownValueViewModel selectedcommunity = communities.Where(x => x.DropdownValueID == communityField.DropdownValueID).FirstOrDefault();
                                    if (selectedcommunity != null && selectedcommunity.DropdownValueID > 0)
                                    {
                                        communityDropdownViewModel = new DropdownValueViewModel()
                                        {
                                            AccountID = viewModel.AccountID,
                                            DropdownID = (byte)ContactFields.Community,
                                            DropdownValue = selectedcommunity.DropdownValue,
                                            DropdownValueID = selectedcommunity.DropdownValueID
                                        };
                                    }
                                    //throw new UnsupportedOperationException("[|Community field is deleted, Please contact adminstrator|]");


                                }

                                if (duplicatesResponse != null && duplicatesResponse.Contacts.IsAny())
                                {
                                    Logger.Current.Informational("Entering into APILeadSubmission Updation");
                                    viewModel.ContactID = duplicatesResponse.Contacts.FirstOrDefault().Id;
                                    viewModel.ContactSource = Entities.ContactSource.API;
                                    var existingContact = duplicatesResponse.Contacts.FirstOrDefault() as Person;

                                    var leadSource = existingContact.LeadSources != null ? existingContact.LeadSources.Select(e => e.Id).ToList() : new List<short>();
                                    PersonViewModel existingContactViewModel = Mapper.Map<Person, PersonViewModel>(existingContact);
                                    if (leadSource.IndexOf(leadSourceDropdownViewModel.DropdownValueID) == -1)
                                    {
                                        existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource ?? new List<DropdownValueViewModel>();
                                        existingContactViewModel.SelectedLeadSource = existingContactViewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                                    }

                                    var community = existingContact.Communities != null ? existingContact.Communities.Select(e => e.Id).ToList() : new List<short>();
                                    if (communityDropdownViewModel != null && community.IndexOf(communityDropdownViewModel.DropdownValueID) == -1)
                                    {
                                        existingContactViewModel.Communities = existingContactViewModel.Communities ?? new List<DropdownValueViewModel>();
                                        existingContactViewModel.Communities = existingContactViewModel.Communities.Append(communityDropdownViewModel);
                                    }

                                    PersonViewModel updateViewModel = GetUpdatedPersonData(existingContactViewModel, viewModel);
                                    updateViewModel.IncludeInReports = true;
                                    UpdatePersonResponse updatePersonResult = UpdatePerson(new UpdatePersonRequest()
                                    {
                                        PersonViewModel = updateViewModel,
                                        AccountId = response.APILeadSubmissionViewModel.AccountID,
                                        RequestedBy = response.APILeadSubmissionViewModel.OwnerID,
                                        RoleId = roleId,
                                        RequestedFrom = RequestOrigin.API
                                    });

                                    if (updatePersonResult.PersonViewModel.ContactID > 0)
                                    {
                                        contactID = updatePersonResult.PersonViewModel.ContactID;
                                        contactRepository.UpdateAPILeadSubmissionData(updatePersonResult.PersonViewModel.ContactID, (byte)SubmittedFormStatus.Completed, "Completed", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    }
                                }
                                else
                                {
                                    Logger.Current.Informational("Entering into APILeadSubmission Insertion");
                                    viewModel.FirstContactSource = Entities.ContactSource.API;
                                    viewModel.CreatedBy = response.APILeadSubmissionViewModel.OwnerID;
                                    viewModel.CreatedOn = DateTime.Now.ToUniversalTime();
                                    viewModel.SelectedLeadSource = new List<DropdownValueViewModel>();
                                    viewModel.SelectedLeadSource = viewModel.SelectedLeadSource.Append(leadSourceDropdownViewModel);
                                    viewModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
                                    if (communityField != null && communityDropdownViewModel.DropdownValueID > 0)
                                    {
                                        viewModel.Communities = new List<DropdownValueViewModel>();
                                        viewModel.Communities = viewModel.Communities.Append(communityDropdownViewModel);
                                    }

                                    //if (viewModel.LifecycleStage > 0)
                                    //{
                                    IEnumerable<DropdownValueViewModel> lifecycles = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.LifeCycle).FirstOrDefault().DropdownValuesList;
                                    if (!lifecycles.Where(l => l.DropdownValueID == viewModel.LifecycleStage).IsAny())
                                        viewModel.LifecycleStage = lifecycles.Where(l => l.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                                    //}
                                    if (viewModel.Phones.IsAny())
                                    {
                                        IEnumerable<DropdownValueViewModel> phones = dropdownViewModel.Where(c => c.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType).FirstOrDefault().DropdownValuesList;
                                        viewModel.Phones.Each(p =>
                                        {
                                            if (!phones.Where(t => t.DropdownValueID == p.PhoneType).IsAny())
                                                p.PhoneType = phones.Where(t => t.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
                                            if (!string.IsNullOrEmpty(p.Number) && (p.Number.Length < 10 || p.Number.Length > 15))
                                                p.IsDeleted = true;

                                        });

                                        viewModel.Phones = viewModel.Phones.Where(p => p.IsDeleted == false).ToList();

                                    }

                                    viewModel.IncludeInReports = true;
                                    InsertPersonResponse personResult = InsertPerson(new InsertPersonRequest()
                                    {
                                        PersonViewModel = viewModel,
                                        AccountId = response.APILeadSubmissionViewModel.AccountID,
                                        RequestedBy = response.APILeadSubmissionViewModel.OwnerID,
                                        RoleId = roleId,
                                        RequestedFrom = RequestOrigin.API
                                    });

                                    if (personResult.PersonViewModel.ContactID > 0)
                                    {
                                        contactID = personResult.PersonViewModel.ContactID;
                                        contactRepository.UpdateAPILeadSubmissionData(personResult.PersonViewModel.ContactID, (byte)SubmittedFormStatus.Completed, "Completed", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                                    }
                                }
                                if (contactID > 0 && viewModel.SelectedLeadSource.IsAny())
                                {
                                    //for Indexing Contact.
                                    formRepository.ScheduleIndexing(contactID, IndexType.Contacts, true);

                                if (response.APILeadSubmissionViewModel.FormID != 0)
                                {
                                    InsertFormSubmissionEntry(contactID,
                                        new SubmittedFormViewModel() { FormId = response.APILeadSubmissionViewModel.FormID, SubmittedOn = DateTime.UtcNow, SubmittedData = response.APILeadSubmissionViewModel.SubmittedData, AccountId = response.APILeadSubmissionViewModel.AccountID },
                                        viewModel.SelectedLeadSource.FirstOrDefault().DropdownValueID);
                                }

                            }
                                #endregion
                            }
                            else
                            {
                                contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Spam, spamRemarks, response.APILeadSubmissionViewModel.APILeadSubmissionID);
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.Data.Clear();
                            ex.Data.Add("APILeadSubmissinID", response.APILeadSubmissionViewModel.APILeadSubmissionID);
                            Logger.Current.Error("Error While Processing APILeadSubmission", ex);
                            contactRepository.UpdateAPILeadSubmissionData(null, (byte)SubmittedFormStatus.Fail, ex.Message, response.APILeadSubmissionViewModel.APILeadSubmissionID);

                        }
                        
                    }
                 
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while APILeadSubmission", ex);                
            }
           
        }

        private Dictionary<string, string> GetContactFields(PersonViewModel model)
        {
            if (model != null)
            {
                Dictionary<string, string> fields = new Dictionary<string, string>();

                if (!string.IsNullOrEmpty(model.FirstName))
                    fields.Add(((byte)ContactFields.FirstNameField).ToString(), model.FirstName);
                if (!string.IsNullOrEmpty(model.LastName))
                    fields.Add(((byte)ContactFields.LastNameField).ToString(), model.LastName);
                if (model.Emails != null && model.Emails.Where(w => w.IsPrimary && !string.IsNullOrEmpty(w.EmailId)).Any())
                    fields.Add(((byte)ContactFields.PrimaryEmail).ToString(), model.Emails.Where(w => w.IsPrimary && !string.IsNullOrEmpty(w.EmailId)).Select(s => s.EmailId).FirstOrDefault());
                if (model.CustomFields.IsAny())
                    foreach (var field in model.CustomFields)
                        fields.Add(field.CustomFieldId.ToString(), field.Value);

                return fields;
            }
            return new Dictionary<string, string>();
        }

        public bool IsValidEmail(string email)
        {
            string pattern = @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" + @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,24}))$";    //Source: http://msdn.microsoft.com/en-us/library/01escwtf(v=vs.110).aspx
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }

        private PersonViewModel GetUpdatedPersonData(PersonViewModel existingModel, PersonViewModel newModel)
        {
            Person person = new Person();
            var dropdownValues = cachingService.GetDropdownValues(newModel.AccountID);

            existingModel.FirstName = !string.IsNullOrEmpty(newModel.FirstName) ? newModel.FirstName : existingModel.FirstName;
            existingModel.LastName = !string.IsNullOrEmpty(newModel.LastName) ? newModel.LastName : existingModel.LastName;
            existingModel.CompanyName = !string.IsNullOrEmpty(newModel.CompanyName) ? newModel.CompanyName : existingModel.CompanyName;
            existingModel.Title = !string.IsNullOrEmpty(newModel.Title) ? newModel.Title : existingModel.Title;

            List<short> phoneTypeIds = newModel.Phones.IsAny() ? newModel.Phones.Select(p => p.PhoneType).ToList() : new List<short>();
            List<short> dropdownValueTyeIds = new List<short>();
            if (phoneTypeIds.IsAny())
            {
                dropdownValueTyeIds = formRepository.GetDropdownValueTypeIdsByPhoneTypes(phoneTypeIds, newModel.AccountID);
            }

            var mobilePhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.MobilePhone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null;
            var homePhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.Homephone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null;
            var workPhoneNumber = dropdownValueTyeIds.Contains((short)DropdownValueTypes.WorkPhone) ? newModel.Phones.Select(m => m.Number).FirstOrDefault() : null;

            var phoneTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.PhoneNumberType)
                 .Select(s => s.DropdownValuesList).FirstOrDefault().Where(d => d.IsActive == true);


            existingModel.Phones = new List<Phone>();

            Logger.Current.Informational("While Updating phones.");

            IEnumerable<Phone> existingPhones = formRepository.GetPhoneFields(existingModel.ContactID);

            Phone mobilePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).FirstOrDefault();
            if (!string.IsNullOrEmpty(mobilePhoneNumber) && !(mobilePhoneNumber.Length < 10 || mobilePhoneNumber.Length > 15))
            {
                string number = mobilePhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = mobilePhone != null ? mobilePhone.IsPrimary : (homePhoneNumber == null && workPhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.MobilePhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (mobilePhone != null)
                existingModel.Phones.Add(mobilePhone);

            Phone homePhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).FirstOrDefault();
            if (!string.IsNullOrEmpty(homePhoneNumber) && !(homePhoneNumber.Length < 10 || homePhoneNumber.Length > 15))
            {
                string number = homePhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = homePhone != null ? homePhone.IsPrimary : (mobilePhoneNumber == null && workPhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.Homephone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (homePhone != null)
                existingModel.Phones.Add(homePhone);

            Phone workPhone = existingPhones.Where(w => w.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).FirstOrDefault();
            if (!string.IsNullOrEmpty(workPhoneNumber) && !(workPhoneNumber.Length < 10 || workPhoneNumber.Length > 15))
            {
                string number = workPhoneNumber.TrimStart(new char[] { '0', '1' });
                Phone phone = new Phone();
                phone.Number = number;
                phone.AccountID = existingModel.AccountID;
                phone.IsPrimary = workPhone != null ? workPhone.IsPrimary : (mobilePhoneNumber == null && homePhoneNumber == null && !existingPhones.IsAny()) ||
                    phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.IsDefault).FirstOrDefault();
                phone.PhoneType = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValueID).FirstOrDefault();
                phone.PhoneTypeName = phoneTypes.Where(c => c.DropdownValueTypeID == (short)DropdownValueTypes.WorkPhone).Select(c => c.DropdownValue).FirstOrDefault();
                if (person.IsValidPhoneNumberLength(number))
                    existingModel.Phones.Add(phone);
            }
            else if (workPhone != null)
                existingModel.Phones.Add(workPhone);

            IEnumerable<Phone> existingNonDefaultPhones = existingPhones.Where(w => w.DropdownValueTypeID != 9 && w.DropdownValueTypeID != 10 && w.DropdownValueTypeID != 11 && !w.IsDeleted);
            if (existingNonDefaultPhones.IsAny())
                existingNonDefaultPhones.Each(e =>
                {
                    if (phoneTypeIds.IsAny())
                    {
                        if (phoneTypeIds.Contains(e.PhoneType))
                        {
                            var nonDefaultPhone = newModel.Phones.Where(p => p.PhoneType == e.PhoneType).FirstOrDefault();//workPhoneNumber.TrimStart(new char[] { '0', '1' });
                            string number = nonDefaultPhone.Number.TrimStart(new char[] { '0', '1' });
                            if (e.Number != number)
                            {
                                e.IsPrimary = false;
                                Phone phone = new Phone();
                                phone.Number = number;
                                phone.AccountID = existingModel.AccountID;
                                phone.IsPrimary = true;
                                phone.PhoneType = nonDefaultPhone.PhoneType;
                                phone.PhoneTypeName = nonDefaultPhone.PhoneTypeName;
                                if (person.IsValidPhoneNumberLength(number.TrimStart(new char[] { '0', '1' })))
                                    existingModel.Phones.Add(phone);

                            }

                        }
                    }
                    existingModel.Phones.Add(e);
                });

            Logger.Current.Informational("While Updating Addresses.");
            existingModel.Addresses = new List<AddressViewModel>();
            var addressLine1 = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.AddressLine1).FirstOrDefault() : null;
            var addressLine2 = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.AddressLine2).FirstOrDefault() : null;
            var city = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.City).FirstOrDefault() : null;
            var state = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.State).FirstOrDefault() : null;
            var zip = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.ZipCode).FirstOrDefault() : null;
            var country = newModel.Addresses.IsAny() ? newModel.Addresses.Select(a => a.Country).FirstOrDefault() : null;

            if (addressLine1 != null || addressLine2 != null || city != null || zip != null || country != null || state != null)
            {
                existingModel.AddressTypes = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.AddressType)
                                                .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);

                AddressViewModel newAddress = new AddressViewModel();
                newAddress.AddressTypeID = existingModel.AddressTypes.SingleOrDefault(a => a.IsDefault).DropdownValueID;
                newAddress.AddressLine1 = addressLine1 != null && !string.IsNullOrEmpty(addressLine1) ? addressLine1 : "";
                newAddress.AddressLine2 = addressLine2 != null && !string.IsNullOrEmpty(addressLine2) ? addressLine2 : "";
                newAddress.City = city != null && !string.IsNullOrEmpty(city) ? city : "";
                if (state != null)
                    newAddress.State = new State() { Code = state.Code };
                else
                    newAddress.State = new State();
                if (country != null)
                    newAddress.Country = new Country() { Code = country.Code };
                else
                    newAddress.Country = new Country();

                var zipCode = zip != null && !string.IsNullOrEmpty(zip) ? zip : "";
                newAddress.ZipCode = zipCode;
                newAddress.IsDefault = true;

                if ((newAddress.State != null && !string.IsNullOrEmpty(newAddress.State.Code)) &&
                    (newAddress.Country == null || string.IsNullOrEmpty(newAddress.Country.Code)))
                {
                    newAddress.Country = new Country();
                    newAddress.Country.Code = newAddress.State.Code.Substring(0, 2);
                }
                existingModel.Addresses.Add(newAddress);
            }

            existingModel.ContactType = Entities.ContactType.Person.ToString();
            existingModel.SecondaryEmails = new List<dynamic>();

            Logger.Current.Informational("While Updating Life Cycle Stage.");
            existingModel.LifecycleStages = dropdownValues.Where(s => s.DropdownID == (byte)DropdownFieldTypes.LifeCycle)
             .Select(s => s.DropdownValuesList).ToList().FirstOrDefault().Where(d => d.IsActive == true);
            var defaultLifeCycleType = existingModel.LifecycleStages.SingleOrDefault(a => a.IsDefault);
            if (newModel.LifecycleStage > 0)
            {
                if (!existingModel.LifecycleStages.Where(l => l.DropdownValueID == newModel.LifecycleStage).IsAny())
                    newModel.LifecycleStage = existingModel.LifecycleStages.Where(l => l.IsDefault).Select(s => s.DropdownValueID).FirstOrDefault();
            }
            existingModel.LifecycleStage = newModel.LifecycleStage > 0 ? newModel.LifecycleStage : defaultLifeCycleType.DropdownValueID;

            existingModel.AccountID = newModel.AccountID;
            existingModel.LastUpdatedOn = DateTime.Now.ToUniversalTime();
            GetAllCustomFieldsResponse accountCustomFields = new GetAllCustomFieldsResponse();
            GetAllCustomFieldsRequest request = new GetAllCustomFieldsRequest(newModel.AccountID);
            accountCustomFields.CustomFields = customFieldService.GetAllCustomFields(request).CustomFields;
            if (newModel.CustomFields.IsAny())
            {
                Logger.Current.Informational("While Updating Custom fields.");
                foreach (ContactCustomFieldMapViewModel submittedField in newModel.CustomFields)
                {
                    try
                    {
                        var isCustomField = accountCustomFields.CustomFields.Where(c => c.FieldId == submittedField.CustomFieldId).FirstOrDefault();
                        if (isCustomField != null)
                        {
                            ContactCustomFieldMapViewModel contactCustomField = new ContactCustomFieldMapViewModel();
                            contactCustomField.CustomFieldId = submittedField.CustomFieldId;
                            contactCustomField.Value = submittedField.Value;
                            contactCustomField.FieldInputTypeId = (int)isCustomField.FieldInputTypeId;
                            contactCustomField.ContactId = existingModel.ContactID;
                            var existingCustomField = existingModel.CustomFields.Where(c => c.CustomFieldId == isCustomField.FieldId).FirstOrDefault();
                            if (existingCustomField == null)
                                existingModel.CustomFields.Add(contactCustomField);
                            else
                                existingCustomField.Value = submittedField.Value;
                        }
                    }
                    catch
                    {
                        Logger.Current.Informational("While Update: Submitted customfieldId: " + submittedField.CustomFieldId + " cannot be Updated. Value: " + submittedField.Value);
                    }
                }
            }

            return existingModel;
        }

        /// <summary>
        /// Insert a form submission record into 'FormSubmissions' table that links to the new contact inserted
        /// </summary>
        /// <param name="contactId"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public FormSubmissionEntryViewModel InsertFormSubmissionEntry(int contactId, SubmittedFormViewModel viewModel, short leadSourceId)
        {
            FormSubmission formSubmission = new FormSubmission();
            formSubmission.ContactId = contactId;
            formSubmission.FormId = viewModel.FormId;
            formSubmission.IPAddress = viewModel.IPAddress;
            formSubmission.SubmittedOn = viewModel.SubmittedOn;
            formSubmission.StatusID = Entities.FormSubmissionStatus.New;
            formSubmission.SubmittedData = viewModel.SubmittedData;
            formSubmission.LeadSourceID = leadSourceId;
            FormSubmissionEntryResponse response = insertFormSubmission(contactId, viewModel.AccountId, formSubmission);
            return response.FormSubmissionEntry;
        }

        FormSubmissionEntryResponse insertFormSubmission(int contactId, int accountId, FormSubmission formSubmission)
        {
            Logger.Current.Informational("Inserting Form Submission: ContactId - " + contactId + ", AccountId - " + accountId);
            if (contactId == 0)
            {
                throw new Exception("[|Error occured while inserting form submission|]");
            };

            //isFormSubmissionValid(formSubmission);
            FormSubmission newFormSubmission = new FormSubmission();
            try
            {
                formSubmissionRepository.Insert(formSubmission);
                newFormSubmission = unitOfWork.Commit() as FormSubmission;

                this.addToTopic(formSubmission.FormId, contactId, accountId, newFormSubmission.Id);
                Form form = formRepository.GetFormById(formSubmission.FormId);
                form.HTMLContent = null;
                if (form.IsDeleted == false)
                    indexingService.Index<Form>(form);
                Logger.Current.Informational("Indexing Form : FormId - " + form.Id);
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while inserting Form Submission entry", ex);
            }

            return new FormSubmissionEntryResponse() { FormSubmissionEntry = Mapper.Map<FormSubmission, FormSubmissionEntryViewModel>(newFormSubmission) };
        }

        void addToTopic(int formId, int contactId, int accountId, int formSubmissionId)
        {
            var message = new TrackMessage()
            {
                EntityId = formId,
                AccountId = accountId,
                ContactId = contactId,
                LeadScoreConditionType = (int)LeadScoreConditionType.ContactSubmitsForm,
                LinkedEntityId = formSubmissionId
            };
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Message = message
            });
        }

        #endregion
    }
}
