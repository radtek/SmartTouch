using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Dashboard;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.ApplicationServices.Messaging.Tour;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class TourService : ITourService
    {
        readonly ITourRepository tourRepository;
        readonly IContactRepository contactRepository;
        readonly IUserRepository userRepository;
        readonly IServiceProviderRepository serviceProvider;
        readonly IUnitOfWork unitOfWork;
        readonly IMessageService messageService;
        IIndexingService indexingService;
        readonly ICachingService cachingService;
        readonly IAccountRepository accountRepository;
        readonly IOpportunityRepository opportunityRepository;
        readonly IUrlService urlService;

        public TourService(ITourRepository tourRepository, IContactRepository contactRepository,
            IUnitOfWork unitOfWork,
            IIndexingService indexingService, IServiceProviderRepository serviceProviderRepository, IUserRepository userRepository,
            IMessageService messageService, ICachingService cachingService, IOpportunityRepository opportunityRepository, IUrlService urlService, IAccountRepository accountRepository
            )
        {
            //intentionally skipped to write the logic when contactRepository and unitOfWork are null to know when this is actually required;

            this.tourRepository = tourRepository;
            this.contactRepository = contactRepository;
            this.serviceProvider = serviceProviderRepository;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.messageService = messageService;
            this.indexingService = indexingService;
            this.cachingService = cachingService;
            this.opportunityRepository = opportunityRepository;
            this.urlService = urlService;
            this.accountRepository = accountRepository;
        }

        public GetContactTourMapResponse GetContactTourMapId(int tourId, int contactId)
        {
            Logger.Current.Verbose("Request received to fetch the contact-tour mapping ID for Tour with TourID " + tourId + " and for Contact with ContactID " + contactId);
            GetContactTourMapResponse response = new GetContactTourMapResponse();
            response.ContactTourMapResponseId = tourRepository.GetContactTourMapId(tourId, contactId);
            Logger.Current.Informational("Search completed, found contact-tour mapping with ID = " + response.ContactTourMapResponseId);
            return response;
        }

        public GetTourListResponse GetTourList(GetTourListRequest request)
        {
            GetTourListResponse response = new GetTourListResponse();
            response.ToursListViewModel = Mapper.Map<IEnumerable<Tour>, IEnumerable<TourViewModel>>(tourRepository.FindByContact(request.Id));
            return response;
        }

        public GetContactsCountResponse TourContactsCount(GetContactsCountRequest request)
        {
            Logger.Current.Verbose("Request for tour contacts count");
            Logger.Current.Informational("TourId :" + request.Id);
            GetContactsCountResponse response = new GetContactsCountResponse();
            response.Count = tourRepository.ContactsCount(request.Id);
            response.SelectAll = tourRepository.IsTourFromSelectAll(request.Id);
            return response;
        }

        public CompletedTourResponse TourStatus(CompletedTourRequest request)
        {
            Logger.Current.Verbose("Request for Tour status change");
            Logger.Current.Informational("Tour status :" + request.isCompleted);
            tourRepository.TourCompleted(request.tourId, request.isCompleted, request.contactId, request.CompletedForAll, (int)request.RequestedBy, request.UpdatedOn);
            Tour tour = tourRepository.FindBy(request.tourId);
            List<int> Ids = new List<int>();
            if (request.isCompleted && !request.CompletedForAll && request.contactId.HasValue)
                Ids.Add(request.contactId.Value);
            else if (request.isCompleted && request.CompletedForAll && tour.Contacts.IsAny())
                Ids.AddRange(tour.Contacts.Select(s => s.Id));
            List<LastTouchedDetails> details = new List<LastTouchedDetails>();
            foreach (var id in Ids)
            {
                LastTouchedDetails detail = new LastTouchedDetails();
                detail.ContactID = id;
                detail.LastTouchedDate = DateTime.UtcNow;
                details.Add(detail);
            }
            updateLastTouchedInformation(details);

            if(request.isCompleted && request.AddToContactSummary)
               tourRepository.AddingTourDetailsToContactSummary(new List<int>() { tour.Id }, Ids, request.AccountId, request.RequestedBy.Value);

            return new CompletedTourResponse();
        }

        public GetTourResponse GetTour(int tourId)
        {
            Logger.Current.Verbose("Request received to fetch the Tour with TourID: " + tourId);
            GetTourResponse response = new GetTourResponse();
            Tour tour = tourRepository.GetTourByID(tourId);

            if (tour.Contacts != null)
                Logger.Current.Informational("Tour found with associated contacts count: " + tour.Contacts.Count);

            TourViewModel tourViewModel = Mapper.Map<Tour, TourViewModel>(tour);

            tourViewModel.IsCompleted = tourRepository.GetTourCompletedStatus(tourId);

            tourViewModel.PreviousCompletedStatus = tourViewModel.IsCompleted;

            response.TourViewModel = tourViewModel;
            if (tour.SelectAll == false)
            {
                response.TourViewModel.Contacts.Clear();    //Revisit this area. For time being we are removing all the contacts and adding new contacts.
                foreach (Contact contact in tour.Contacts)
                {
                    var contactEntry = Mapper.Map<Contact, ContactEntry>(contact);
                    tourViewModel.Contacts.Add(contactEntry);
                }
            }
            return response;
        }

        public InsertTourResponse InsertTour(InsertTourRequest request)
        {
            Logger.Current.Verbose("Request received to insert a new tour.");
            var createdBy = request.TourViewModel.CreatedBy;
            Guid? emailGuid = Guid.NewGuid();
            Guid? textGuid = Guid.NewGuid();
            IDictionary<int,Guid> userEmailGuids = new Dictionary<int, Guid>();
            IDictionary<int, Guid> userTextGuids = new Dictionary<int, Guid>();
            Tour tour = Mapper.Map<TourViewModel, Tour>(request.TourViewModel);
            tour.IsCompleted = false;
            if (tour.ReminderTypes.Contains(ReminderType.PopUp))
                tour.NotificationStatus = NotificationStatus.New;

            isTourValid(tour, request.RequestedFrom);
            Tour newTour = new Tour();
            IEnumerable<int> contactIDS = null;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate)
            {
                #region Contacts In Private Mode
                IEnumerable<ContactOwner> contactOwners = contactRepository.GetAllContactOwners(tour.Contacts.Select(c => c.Id).ToList(), tour.OwnerIds, tour.CreatedBy);

                foreach (int owId in contactOwners.Select(o => o.OwnerID).Distinct())
                {
                    if(tour.ReminderTypes.Contains(ReminderType.Email))
                        userEmailGuids.Add(owId, Guid.NewGuid());
                    if (tour.ReminderTypes.Contains(ReminderType.TextMessage))
                        userTextGuids.Add(owId, Guid.NewGuid());

                    contactIDS = contactOwners.Where(u => u.OwnerID == owId).Select(s => s.ContactID).ToList();
                    if (createdBy != owId)
                        tour.OwnerIds = new List<int>() { createdBy, owId };
                    else
                        tour.OwnerIds = new List<int>() { owId };
                    tour.ContactIDS = contactIDS;
                    tour.EmailGuid = userEmailGuids;
                    tour.TextGuid = userTextGuids;
                    tour.EmailRequestGuid = tour.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                    tour.TextRequestGuid = tour.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;

                    tourRepository.Insert(tour);
                    newTour = unitOfWork.Commit() as Tour;
                    if (newTour.TourContacts != null && newTour.TourContacts.Any())
                    {
                        UpdateTourBulkData(newTour.Id, request.AccountId, (int)request.RequestedBy, request.AccountPrimaryEmail, request.AccountPhoneNumber, tour.IcsCanlender, request.AccountAddress, contactIDS, isPrivate, owId, userEmailGuids,userTextGuids);
                    }

                    if (request.TourViewModel.IsCompleted == true)
                    {
                        tourRepository.TourCompleted(newTour.Id, true, null, true, request.TourViewModel.LastUpdatedBy, DateTime.Now.ToUniversalTime().AddMinutes(1));
                        IEnumerable<int> Ids = tour.Contacts.Select(c => c.Id);
                        List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                        foreach (var id in Ids)
                        {
                            LastTouchedDetails detail = new LastTouchedDetails();
                            detail.ContactID = id;
                            detail.LastTouchedDate = DateTime.UtcNow;
                            details.Add(detail);
                        }
                        updateLastTouchedInformation(details);
                    }
                    Logger.Current.Informational("Tour inserted successfully.");
                    Logger.Current.Verbose("Tour sync status is being changed to in sync. Action Id: " + request.TourViewModel.TourID);
                    tourRepository.UpdateCRMOutlookMap(newTour, request.RequestedFrom);
                }
                #endregion
            }
            else
            {
                #region Contacts In Public Mode.
                int ownerId = 0;
                if(tour.ReminderTypes.Contains(ReminderType.Email))
                    foreach(int owId in tour.OwnerIds)
                    {
                        userEmailGuids.Add(owId, Guid.NewGuid());
                    }

                if(tour.ReminderTypes.Contains(ReminderType.TextMessage))
                    foreach (int owId in tour.OwnerIds)
                    {
                        userTextGuids.Add(owId, Guid.NewGuid());
                    }

                tour.EmailRequestGuid = tour.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                tour.TextRequestGuid = tour.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;
                tour.EmailGuid = userEmailGuids;
                tour.TextGuid = userTextGuids;
                tourRepository.Insert(tour);
                newTour = unitOfWork.Commit() as Tour;
                if (newTour.TourContacts != null && newTour.TourContacts.Any())
                {
                    UpdateTourBulkData(newTour.Id, request.AccountId, (int)request.RequestedBy, request.AccountPrimaryEmail, request.AccountPhoneNumber, tour.IcsCanlender, request.AccountAddress, contactIDS, isPrivate, ownerId, userEmailGuids,userTextGuids);
                }

                if (request.TourViewModel.IsCompleted == true)
                {
                    tourRepository.TourCompleted(newTour.Id, true, null, true, request.TourViewModel.LastUpdatedBy, DateTime.Now.ToUniversalTime().AddMinutes(1));
                    IEnumerable<int> Ids = tour.Contacts.Select(c => c.Id);
                    List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                    foreach (var id in Ids)
                    {
                        LastTouchedDetails detail = new LastTouchedDetails();
                        detail.ContactID = id;
                        detail.LastTouchedDate = DateTime.UtcNow;
                        details.Add(detail);
                    }
                    updateLastTouchedInformation(details);
                }
                Logger.Current.Informational("Tour inserted successfully.");

                Logger.Current.Verbose("Tour sync status is being changed to in sync. Action Id: " + request.TourViewModel.TourID);
                tourRepository.UpdateCRMOutlookMap(newTour, request.RequestedFrom);
                #endregion 
            }

            if (newTour != null && newTour.TourContacts.IsAny())
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = newTour.TourContacts.Select(s => s.ContactId).ToList(), IndexType = 1 });
            return new InsertTourResponse() { TourViewModel = Mapper.Map<Tour, TourViewModel>(newTour) };
        }

        void updateLastTouchedInformation(List<LastTouchedDetails> details)
        {
            if (details.IsAny())
                contactRepository.UpdateLastTouchedInformation(details, AppModules.ContactTours, null);
        }

        public Tour UpdateTourBulkData(int tourId, int accountId, int userId, string accountPrimaryEmail, string accountDomain, bool icsCalender, string AccountAddress, IEnumerable<int> contactIDs, bool isPrivate, int ownerId,IDictionary<int,Guid> emailGuids, IDictionary<int, Guid> textGuids)
        {
            var tour = tourRepository.FindByTourId(tourId);
            #region For Declearing vairables.
            Guid? emailGuid = Guid.NewGuid();
            Guid? textGuid = Guid.NewGuid();
            Guid? emailLoginToken = null;
            Guid? textLoginToken = null;
            string senderPhoneNumber = string.Empty;
            Email senderEmail = new Email();
            Address address = new Address();
            string accountPhoneNumber = string.Empty;
            string accountAddress = string.Empty;
            string location = string.Empty;
            string fromEmail = string.Empty;
            string reminder = string.Empty;
            string update = string.Empty;
            IList<int> assignedUserIds = new List<int>();
            #endregion

            if (tour != null && (icsCalender == true || tour.OwnerIds.Any()))
            {
                #region For sending ics and assigned users Email Notifications.
                address = accountRepository.GetAddress(accountId);
                if (address != null)
                {
                    location = address.City;
                    accountAddress = address.ToString();
                }
                accountPhoneNumber = accountRepository.GetPrimaryPhone(accountId);
                IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                if (serviceProviders != null && serviceProviders.Any())
                    senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                if (serviceProviders.FirstOrDefault() != null)
                    emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                else
                    throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? accountPrimaryEmail : senderEmail.EmailId;

                if (isPrivate)
                {
                    if (icsCalender || ownerId != tour.CreatedBy)
                    {
                        emailGuid = Guid.NewGuid();
                        RemindByEmail(accountId, tour.Id, tour, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, reminder, update, ownerId, contactIDs, isPrivate);
                    }
                }
                else
                {
                    if (icsCalender)
                        assignedUserIds = tour.OwnerIds;
                    else
                        assignedUserIds = tour.OwnerIds.Where(s => s != tour.CreatedBy).ToList();

                    foreach (int id in assignedUserIds)
                    {
                        emailGuid = Guid.NewGuid();
                        RemindByEmail(accountId, tour.Id, tour, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, reminder, update, id, contactIDs, isPrivate);
                    }
                }
                #endregion

            }
            #region for getting Default service Provider Details
            if (tour != null && (tour.ReminderTypes.Contains(ReminderType.Email) || tour.ReminderTypes.Contains(ReminderType.TextMessage)))
            {
                tour.EmailRequestGuid = tour.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                tour.TextRequestGuid = tour.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;
                if (tour.ReminderTypes.Contains(ReminderType.Email))
                {
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                }
                if (tour.ReminderTypes.Contains(ReminderType.TextMessage))
                {
                    ServiceProvider serviceProviders = serviceProvider.GetSendTextServiceProviders(accountId, CommunicationType.Text);
                    if (serviceProviders != null)
                    {
                        textLoginToken = serviceProviders.LoginToken;
                        senderPhoneNumber = serviceProviders.SenderPhoneNumber;
                    }
                    else
                        throw new UnsupportedOperationException("[|Text providers are not configured for this account|]");
                }
            }

            #endregion

            #region for sending Remainder emails and Reminder Text
            if (tour != null)
            {
                if (tour.ReminderTypes != null && tour.ReminderTypes.Contains(ReminderType.Email))
                {
                    reminder = "Reminder";
                    fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? accountPrimaryEmail : senderEmail.EmailId;
                    emailGuid = Guid.NewGuid();
                    if (string.IsNullOrEmpty(accountAddress))
                        accountAddress = AccountAddress;
                    if (isPrivate)
                    {
                        emailGuid = emailGuids.Where(k => k.Key == ownerId).Select(s => s.Value).FirstOrDefault();
                        RemindByEmail(accountId, tour.Id, tour, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, reminder, update, ownerId, contactIDs, isPrivate);
                    }
                    else
                    {
                        foreach (int id in tour.OwnerIds)
                        {
                            emailGuid = emailGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                            RemindByEmail(accountId, tour.Id, tour, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, reminder, update, id, contactIDs, isPrivate);
                        }
                    }


                }
                if (tour.ReminderTypes != null && tour.ReminderTypes.Contains(ReminderType.TextMessage))
                {
                    if (isPrivate)
                    {
                        textGuid = textGuids.Where(k => k.Key == ownerId).Select(s => s.Value).FirstOrDefault();
                        RemindByText(accountId, tour, senderPhoneNumber, textGuid.Value, textLoginToken.Value, ownerId);
                    }
                    else
                    {
                        foreach (int id in tour.OwnerIds)
                        {
                            textGuid = textGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                            RemindByText(accountId, tour, senderPhoneNumber, textGuid.Value, textLoginToken.Value, id);
                        }
                    }

                }

            }
            #endregion
            this.addToTopic(tour, accountId);
            return tour;
        }

        void RemindByEmail(int accountId, int tourId, Tour tour, string fromMail, Guid requestGuid, Guid loginToken, string address, string phone, string accountDomain, string location, bool icsCalender, string reminder, string update, int userId, IEnumerable<int> contactIDs, bool isPrivate)
        {
            Logger.Current.Verbose("Request received to remind tour by email");
            try
            {
                #region Sending Email.
                Guid guid = new Guid();
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                var primaryEmail = userRepository.GetUserPrimaryEmail(userId);
                var userName = userRepository.GetUserName(userId);
                Account account = accountRepository.GetAccountMinDetails(accountId);
                string description = string.Empty;
                if (tour.TourDetails != null)
                    description = tour.TourDetails.Length > 60 ? tour.TourDetails.Substring(0, 60) + "..." : tour.TourDetails;

                if (icsCalender == true && string.IsNullOrEmpty(reminder))
                {
                    string subject = "SmartTouch - Tour Reminder -" + account.AccountName + ":" + description + "";
                    guid = GenerateIcsFile(tour.TourDate, tour.TourDate, location, description, subject, userName);
                }

                if (primaryEmail != null && fromMail != null && requestGuid != new Guid() && loginToken != new Guid())
                {
                    mailRequest.Body = GenerateEmailBody(tourId, accountId, tour, address, phone, reminder, contactIDs, isPrivate);
                    if (string.IsNullOrEmpty(reminder) && icsCalender == true)
                        mailRequest.AttachmentGUID = guid;
                    mailRequest.From = fromMail;
                    mailRequest.IsBodyHtml = true;
                    if (!string.IsNullOrEmpty(reminder))
                    {
                        mailRequest.ScheduledTime = tour.ReminderDate;
                        mailRequest.Subject = "SmartTouch Tour Reminder - " + account.AccountName + ":" + description + "";
                    }
                    else if (!string.IsNullOrEmpty(update))
                    {
                        mailRequest.Subject = "SmartTouch Updated Tour Notification - " + account.AccountName + ":" + description + "";
                    }
                    else
                    {
                        mailRequest.Subject = "SmartTouch New Tour Notification - " + account.AccountName + ":" + description + "";
                    }
                    mailRequest.To = new List<string>() { primaryEmail };
                    mailRequest.CategoryID = (byte)EmailNotificationsCategory.TourReminders;
                    mailRequest.TokenGuid = loginToken;
                    mailRequest.RequestGuid = requestGuid;
                    mailRequest.AccountDomain = accountDomain;
                    mailRequest.AccountID = accountId;
                    MailService mailService = new MailService();
                    mailService.Send(mailRequest);
                }
                #endregion
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                throw new UnsupportedOperationException("Tour created successfully.An error occured while generating reminder email.");
            }
        }

        private static Guid GenerateIcsFile(DateTime TourDate, DateTime? endDate, string location, string description, string subject, string userName)
        {
            Guid guid = Guid.NewGuid();
            string myGuid = Convert.ToString(guid);
            string[] data = IcsContent(TourDate, endDate, location, description, subject, userName);
            string attachmentPath = System.Configuration.ConfigurationManager.AppSettings["ATTACHMENT_PHYSICAL_PATH"].ToString();
            StringBuilder strinbuilder = new StringBuilder();
            foreach (string value in data)
            {
                strinbuilder.Append(value);
                strinbuilder.Append('\n');
            }
            string createText = strinbuilder + Environment.NewLine;
            File.WriteAllText(attachmentPath + myGuid + ".ics", createText);
            return guid;

        }

        private static string[] IcsContent(DateTime TourDate, DateTime? endDate, string location, string description, string subject, string userName)
        {

            if (endDate != null)
            {
                endDate = endDate.Value.AddHours(1);
            }

            string[] iscConent = { "BEGIN:VCALENDAR",
                                "VERSION:2.0",
                                "BEGIN:VEVENT",
                                "STATUS:TENTATIVE",
                                "DTSTART:" + TourDate.ToString("yyyyMMdd\\THHmmss\\Z"),
                                "DTEND:" + endDate.Value.ToString("yyyyMMdd\\THHmmss\\Z"),
                                "SUMMARY;ENCODING=QUOTED-PRINTABLE:" + subject,
                                "LOCATION;ENCODING=QUOTED-PRINTABLE:" + location,
                                "ORGANIZER;ENCODING=QUOTED-PRINTABLE:" + userName,
                                "DESCRIPTION;ENCODING=QUOTED-PRINTABLE:" + description,
                                "END:VEVENT",
                                "END:VCALENDAR"
                                };

            return iscConent;


        }

        string GenerateEmailBody(int tourId, int accountId, Tour tour, string address, string phone, string reminder, IEnumerable<int> contactIDs, bool isPrivate)
        {
            DateTime date = new DateTime();
            Logger.Current.Informational("Request received for generating an email for tour reminder");
            string body = "";
            try
            {
                #region Getting Account Primary Address Details
                date = tour.TourDate.ToUtc().ToUserDateTime();
                string filename = EmailTemplate.ToursReminder.ToString() + ".txt";
                string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
                string community = cachingService.GetDropdownValues(accountId).Where(s => s.DropdownID == (byte)DropdownFieldTypes.Community).
                    Select(s => s.DropdownValuesList.Where(w => w.DropdownValueID == tour.CommunityID).Select(d => d.DropdownValue)).FirstOrDefault().FirstOrDefault();
                var tourType = cachingService.GetDropdownValues(accountId).Where(s => s.DropdownID == (byte)DropdownFieldTypes.TourType).
                    Select(s => s.DropdownValuesList.Where(w => w.DropdownValueID == tour.TourType).Select(d => d.DropdownValue)).FirstOrDefault().FirstOrDefault();
                List<string> contactsList = new List<string>();
                string accountLogo = string.Empty;
                var accountLogoInformation = accountRepository.GetImageStorageName(accountId);
                if (!String.IsNullOrEmpty(accountLogoInformation.StorageName))
                    accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, accountLogoInformation.StorageName);
                else
                    accountLogo = "";
                string accountName = accountLogoInformation.AccountName;
                string accountImage = string.Empty;
                if (!string.IsNullOrEmpty(accountLogo))
                {
                    accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width='100'></td>";
                }
                else
                {
                    accountImage = "";
                }
                #endregion
                #region Creating table using ContactIds
                IEnumerable<int> contactIds = tour.Contacts.Select(s => s.Id);
                var contacts = contactRepository.GetContacts(contactIds, accountId);
                if (contacts != null && contacts.Any())
                {
                    IEnumerable<TourContactsSummary> contactsSummary = new List<TourContactsSummary>();
                    var domainurl = accountRepository.GetAccountDomainUrl(accountId);
                    if (isPrivate || contactIDs.IsAny())
                        contactsSummary = tourRepository.GetTourContactsSummary(tourId, contactIDs, accountId);
                    else
                        contactsSummary = tourRepository.GetTourContactsSummary(tourId, contactIds, accountId);


                    if (contactsSummary.Any())
                        contactsSummary.ToList().ForEach(c =>
                        {
                            var link = string.Empty;
                            if (c.ContactType == ContactType.Person)
                                link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domainurl + "/person/" + c.ContactId + "'" + "style=" + "color:#0e749f;" +
                                    ">" + c.ContactName + "</a> </span>";
                            else if (c.ContactType == ContactType.Company)
                                link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domainurl + "/company/" + c.ContactId + "'" + "style=" + "color:#0e749f;" +
                                    ">" + c.ContactName + "</a> </span>";


                            var tourStatus = c.Status ? "Completed" : "Not completed";
                            contactsList.Add("<tr><td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + link + "</td>" +
                                "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + c.PrimaryEmail + "</td>" +
                                "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" +
                            (!string.IsNullOrEmpty(c.PhoneCountryCode) ? "+" + c.PhoneCountryCode + " " : "") + c.PrimaryPhone + " " + (!string.IsNullOrEmpty(c.PhoneExtension) ? " Ext." + c.PhoneExtension : "") + "</td>" +
                                "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + tourStatus + "</td>" +
                                "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + c.Lifecycle + "</td>" + "</tr>");
                        });
                    else
                        contactsList.Add("<tr><td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" +
                            "<div class='notecordsfound'><div><i class='icon st - icon - browser - windows - 2'></i></div><span class='bolder smaller - 90'>[|No records found|]</span></div></td></tr>");

                }
                
                string contactsBody = string.Join("", contactsList);

                #endregion

                #region Mapping Details to Template.
                using (StreamReader reader = new StreamReader(savedFileName))
                {
                    do
                    {
                        body = reader.ReadToEnd().Replace("[COMMUNITY]", community).Replace("[DATETIME]", date.ToString()).
                            Replace("[TOURDETAILS]", tour.TourDetails).Replace("[TOURTYPE]", tourType).Replace("[RTF]", "").Replace("[CONTACTS]", contactsBody)
                            .Replace("[ADDRESS]", address).Replace("[PHONE]", phone).Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage);
                    } while (!reader.EndOfStream);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.Current.Error("An error occured while generating email body for reminder notification : ", ex);
                throw ex;
            }
            return body;
        }

        void RemindByText(int accountId, Tour tour, string fromText, Guid requestGuid, Guid loginToken, int userId)
        {
            Logger.Current.Verbose("Request received to remind tour by text");
            try
            {
                #region Sending Text Notification
                LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest textRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest();
                var phoneNumber = userRepository.GetUserPrimaryPhoneNumber(userId);

                if (fromText != null && !string.IsNullOrEmpty(phoneNumber) && requestGuid != new Guid() && loginToken != new Guid())
                {
                    textRequest.From = fromText;   //"5123374222";
                    textRequest.Message = tour.TourDetails;
                    textRequest.RequestGuid = requestGuid;
                    textRequest.TokenGuid = loginToken;
                    textRequest.ScheduledTime = tour.ReminderDate;
                    textRequest.To = new List<string>() { phoneNumber };
                    TextService textService = new TextService();
                    textService.SendText(textRequest);
                }
                #endregion;
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                throw new UnsupportedOperationException("[|Failed while generating reminder text. Please enter no more than 160 characters if reminder by text is chosen.|]");
            }
        }

        public UpdateTourResponse UpdateTour(UpdateTourRequest request)
        {
            #region Updating Tour.
            Logger.Current.Verbose("Request received to update tour with TourID " + request.TourViewModel.TourID);
            Tour tour = Mapper.Map<TourViewModel, Tour>(request.TourViewModel);
            tour.IsCompleted = false;
            if (tour.ReminderTypes.Contains(ReminderType.PopUp))
                tour.NotificationStatus = NotificationStatus.New;

            Guid emailGuid = Guid.NewGuid();
            Guid textGuid = Guid.NewGuid();
            IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
            IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
            MailService mailService = new MailService();
            TextService textService = new TextService();
            Guid? emailLoginToken = null;
            Guid textLoginToken = Guid.NewGuid();
            Email senderEmail = new Email();
            Address address = new Address();
            string location = string.Empty;
            string fromEmail = string.Empty;
            string reminder = string.Empty;
            string accountAddress = string.Empty;
            string senderPhoneNumber = string.Empty;
            IList<int> assignedUserIds = new List<int>();
            IEnumerable<int> contactIDS = null;
            IEnumerable<int> deletedusers = null;
            IEnumerable<int> deletedContactIds = null;
            var createdBy = request.TourViewModel.CreatedBy;
            if (string.IsNullOrEmpty(accountAddress))
                accountAddress = request.AccountAddress;
            isTourValid(tour, request.RequestedFrom);
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            IEnumerable<int> assignedOwnerIds = tourRepository.GetAssignedUserIds(tour.Id);
            IEnumerable<int> assContactIds = tourRepository.GetContactIds(tour.Id);
            if (assContactIds.IsAny())
                deletedContactIds = assContactIds.Where(w => !tour.Contacts.Select(s => s.Id).Contains(w)).Select(se => se).ToList();

            #region Deleting Reminder email and Text Notifications for deleted users.
            if(assignedOwnerIds.IsAny())
                deletedusers = assignedOwnerIds.Where(i => !tour.OwnerIds.Contains(i)).Select(s => s).ToList();

            if(tour.ReminderTypes.Contains(ReminderType.Email))
                foreach (int delId in deletedusers)
                {
                    Guid userEmailGuid = tourRepository.GetUserEmailGuidByUserId(delId, tour.Id);
                    if (userEmailGuid != new Guid())
                        mailService.RemoveAllScheduledReminder(userEmailGuid);
                }

            if (tour.ReminderTypes.Contains(ReminderType.TextMessage))
                foreach (int delId in deletedusers)
                {
                    Guid userTextGuid = tourRepository.GetUserTextEmailGuid(delId, tour.Id);
                    if (userTextGuid != new Guid())
                        textService.RemoveAllTextScheduledReminder(userTextGuid);
                }

            #endregion
            Tour updatedTour = new Tour();
            if (isPrivate)
            {
                #region Contacts In Private Mode.
                IEnumerable<ContactOwner> contactOwners = contactRepository.GetAllContactOwners(tour.Contacts.Select(c => c.Id).ToList(), tour.OwnerIds, tour.CreatedBy);

                foreach (int owId in contactOwners.Select(o => o.OwnerID).Distinct())
                {
                    if(tour.ReminderTypes.Contains(ReminderType.Email))
                    {
                        if (!assignedOwnerIds.Contains(owId))
                            emailGuids.Add(owId, Guid.NewGuid());
                    }

                    if (tour.ReminderTypes.Contains(ReminderType.TextMessage))
                    {
                        if (!assignedOwnerIds.Contains(owId))
                            textGuids.Add(owId, Guid.NewGuid());
                    }


                    contactIDS = contactOwners.Where(u => u.OwnerID == owId).Select(s => s.ContactID).ToList();

                    if (createdBy != owId)
                        tour.OwnerIds = new List<int>() { createdBy, owId };
                    else
                        tour.OwnerIds = new List<int>() { owId };

                    tour.ContactIDS = contactIDS;
                    tour.EmailGuid = emailGuids;
                    tour.TextGuid = textGuids;

                    tourRepository.Update(tour);
                    updatedTour = unitOfWork.Commit() as Tour;

                    emailGuid = Guid.NewGuid();
                    if (tour != null && (tour.IcsCanlender == true || tour.OwnerIds.Any()))
                    {
                        address = accountRepository.GetAddress(request.AccountId);
                        if (address != null)
                        {
                            location = address.City;
                            accountAddress = address.ToString();
                        }

                        if (!request.TourViewModel.EmailRequestGuid.HasValue)
                            tour.EmailRequestGuid = emailGuid;
                        IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                        if (serviceProviders != null && serviceProviders.Any())
                            senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                        if (serviceProviders.FirstOrDefault() != null)
                            emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                        else
                            throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                        fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;

                        if (tour.IcsCanlender || owId != tour.CreatedBy)
                            RemindByEmail(request.AccountId, tour.Id, tour, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, tour.IcsCanlender, reminder, "Update", owId, contactIDS, isPrivate);

                    }
                    #region Getting Service Provider Details
                    if (request.TourViewModel.SelectedReminderTypes != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.Email))          //Insert reminder
                    {
                        if (!request.TourViewModel.EmailRequestGuid.HasValue)
                            tour.EmailRequestGuid = emailGuid;
                        IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                        if (serviceProviders != null && serviceProviders.Any())
                            senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                        if (serviceProviders.FirstOrDefault() != null)
                            emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                        else
                            throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                    }
                    if (request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))          //Insert reminder
                    {
                        if (!request.TourViewModel.TextRequestGuid.HasValue)
                            tour.TextRequestGuid = textGuid;
                        ServiceProvider serviceProviders = serviceProvider.GetSendTextServiceProviders(request.AccountId, CommunicationType.Text);
                        if (serviceProviders != null)
                        {
                            textLoginToken = serviceProviders.LoginToken;
                            senderPhoneNumber = serviceProviders.SenderPhoneNumber;
                        }

                        else
                            throw new UnsupportedOperationException("[|Text providers are not configured for this account|]");

                    }

                    #endregion

                    #region Sending Reminder Emails
                    fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;
                    if (tour != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.Email))
                    {
                        Guid userGuid = emailGuids.Where(k => k.Key == owId).Select(v => v.Value).FirstOrDefault();
                        if(userGuid != new Guid())
                            EmailReminderOperation(request.TourViewModel, tour, fromEmail, userGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, owId, contactIDS, isPrivate,true);
                        else
                            EmailReminderOperation(request.TourViewModel, tour, fromEmail, emailGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, owId, contactIDS, isPrivate,false);

                    }

                    if (tour != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))
                    {
                        Guid userGuid = textGuids.Where(k => k.Key == owId).Select(v => v.Value).FirstOrDefault();
                        if (userGuid != new Guid())
                            TextReminderOperation(request.TourViewModel, tour, senderPhoneNumber, userGuid, textLoginToken, request.AccountId, owId,true);
                        else
                            TextReminderOperation(request.TourViewModel, tour, senderPhoneNumber, textGuid, textLoginToken, request.AccountId, owId,false);

                    }
                    #endregion
                    this.addToTopic(tour, request.AccountId);
                    #region Tour Completion.
                    var contacts = request.TourViewModel.Contacts.ToList();
                    if (request.TourViewModel.PreviousCompletedStatus != request.TourViewModel.IsCompleted && request.TourViewModel.Contacts.Count() == 1)
                        tourRepository.TourCompleted(tour.Id, request.TourViewModel.IsCompleted, contacts[0].Id, true, request.TourViewModel.LastUpdatedBy, DateTime.Now.ToUniversalTime().AddMinutes(1));
                    if (request.TourViewModel.PreviousCompletedStatus != request.TourViewModel.IsCompleted && request.TourViewModel.IsCompleted)
                    {
                        IEnumerable<int> Ids = request.TourViewModel.Contacts.Select(c => c.Id);
                        if(Ids.Count() == 1)
                        {
                            List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                            foreach (var id in Ids)
                            {
                                LastTouchedDetails detail = new LastTouchedDetails();
                                detail.ContactID = id;
                                detail.LastTouchedDate = DateTime.UtcNow;
                                details.Add(detail);
                            }
                            updateLastTouchedInformation(details);
                        }

                    }
                    #endregion
                    tourRepository.UpdateCRMOutlookMap(updatedTour, request.RequestedFrom);

                    Logger.Current.Informational("Tour updated successfully.");
                }
                #endregion
            }
            else
            {
                #region Contacts In Public Mode.

                #region updating tour
                if(tour.ReminderTypes.Contains(ReminderType.Email))
                    foreach (int assId in tour.OwnerIds)
                    {
                        if (!assignedOwnerIds.Contains(assId))
                            emailGuids.Add(assId, Guid.NewGuid());

                    }
                if(tour.ReminderTypes.Contains(ReminderType.TextMessage))
                    foreach (int assId in tour.OwnerIds)
                    {
                        if (!assignedOwnerIds.Contains(assId))
                            textGuids.Add(assId, Guid.NewGuid());

                    }

                tour.EmailGuid = emailGuids;
                tour.TextGuid = textGuids;
                tourRepository.Update(tour);
                updatedTour = unitOfWork.Commit() as Tour;

                #endregion

                if (tour != null && (tour.IcsCanlender == true || tour.OwnerIds.Any()))
                {
                    #region sending Ics And Assigned User Notification,
                    address = accountRepository.GetAddress(request.AccountId);
                    if (address != null)
                    {
                        location = address.City;
                        accountAddress = address.ToString();
                    }

                    if (!request.TourViewModel.EmailRequestGuid.HasValue)
                        tour.EmailRequestGuid = emailGuid;
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                    fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;

                    if (tour.IcsCanlender)
                        assignedUserIds = tour.OwnerIds;
                    else
                        assignedUserIds = tour.OwnerIds.Where(s => s != tour.CreatedBy).ToList();

                    foreach (int id in assignedUserIds)
                    {
                        emailGuid = Guid.NewGuid();
                        RemindByEmail(request.AccountId, tour.Id, tour, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, tour.IcsCanlender, reminder, "Update", id, contactIDS, false);
                    }
                    #endregion

                }

                #region Getting Service Provider details.
                if (request.TourViewModel.SelectedReminderTypes != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.Email))          //Insert reminder
                {
                    if (!request.TourViewModel.EmailRequestGuid.HasValue)
                        tour.EmailRequestGuid = emailGuid;
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                }
                if (request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))          //Insert reminder
                {
                    if (!request.TourViewModel.TextRequestGuid.HasValue)
                        tour.TextRequestGuid = textGuid;
                    ServiceProvider serviceProviders = serviceProvider.GetSendTextServiceProviders(request.AccountId, CommunicationType.Text);
                    if (serviceProviders != null)
                    {
                        textLoginToken = serviceProviders.LoginToken;
                        senderPhoneNumber = serviceProviders.SenderPhoneNumber;
                    }

                    else
                        throw new UnsupportedOperationException("[|Text providers are not configured for this account|]");

                }

                #endregion
                #region sending Remiander Email and Text Notifications
                fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;
                if (tour != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.Email))
                {
                    foreach (int id in tour.OwnerIds)
                    {
                        Guid userGuid = emailGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                        if(userGuid != new Guid())
                            EmailReminderOperation(request.TourViewModel, tour, fromEmail, userGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, id, contactIDS, isPrivate,true);
                        else
                            EmailReminderOperation(request.TourViewModel, tour, fromEmail, emailGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, id, contactIDS, isPrivate,false);

                    }

                }

                if (tour != null && request.TourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))
                {
                    foreach (int id in tour.OwnerIds)
                    {
                        Guid userGuid = textGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                        if (userGuid != new Guid())
                            TextReminderOperation(request.TourViewModel, tour, senderPhoneNumber, userGuid, textLoginToken, request.AccountId, id,true);
                        else
                            TextReminderOperation(request.TourViewModel, tour, senderPhoneNumber, textGuid, textLoginToken, request.AccountId, id,false);

                    }

                }
                #endregion
                this.addToTopic(tour, request.AccountId);
                #region Tour Completion
                var contacts = request.TourViewModel.Contacts.ToList();
                if (request.TourViewModel.PreviousCompletedStatus != request.TourViewModel.IsCompleted && request.TourViewModel.Contacts.Count() == 1)
                    tourRepository.TourCompleted(tour.Id, request.TourViewModel.IsCompleted, contacts[0].Id, true, request.TourViewModel.LastUpdatedBy, DateTime.Now.ToUniversalTime().AddMinutes(1));

                if (request.TourViewModel.PreviousCompletedStatus != request.TourViewModel.IsCompleted && request.TourViewModel.IsCompleted)
                {
                    IEnumerable<int> Ids = request.TourViewModel.Contacts.Select(c => c.Id);
                    if(Ids.Count() == 1)
                    {
                        List<LastTouchedDetails> details = new List<LastTouchedDetails>();
                        foreach (var id in Ids)
                        {
                            LastTouchedDetails detail = new LastTouchedDetails();
                            detail.ContactID = id;
                            detail.LastTouchedDate = DateTime.UtcNow;
                            details.Add(detail);
                        }
                        updateLastTouchedInformation(details);
                    }

                }
                #endregion
                tourRepository.UpdateCRMOutlookMap(updatedTour, request.RequestedFrom);

                Logger.Current.Informational("Tour updated successfully.");
                #endregion
            }

            if (deletedContactIds.IsAny() || (updatedTour != null && updatedTour.TourContacts.IsAny()))
            {
                List<int> ids = updatedTour.TourContacts.Select(s => s.ContactId).Concat(deletedContactIds).ToList();
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = ids, IndexType = 1 });
            }

            #region Adding Action Details to NoteCategory.
            if (request.TourViewModel.AddToContactSummary == true && request.TourViewModel.IsCompleted == true)
            {
                if (updatedTour.SelectAll)
                    tourRepository.AddingTourDetailsToContactSummary(new List<int>() { updatedTour.Id }, new List<int>() { }, request.AccountId, request.RequestedBy.Value);
                else
                    tourRepository.AddingTourDetailsToContactSummary(new List<int>() { updatedTour.Id }, updatedTour.TourContacts.Where(c => c.IsCompleted == true).Select(s => s.ContactId).ToList(), request.AccountId, request.RequestedBy.Value);
            }
            #endregion


            return new UpdateTourResponse() { TourViewModel = Mapper.Map<Tour, TourViewModel>(updatedTour) };

            #endregion
        }

        private void EmailReminderOperation(TourViewModel tourViewModel, Tour tour, string fromEmail, Guid requestGuid, Guid? loginToken, int accountId, string address, string phone, string accountDomain, string location, int userId,IEnumerable<int> contactIDs,bool isPrivate,bool newUserGuid)
        {
            MailService mailService = new MailService();
            Guid UserGuid = tourRepository.GetUserEmailGuidByUserId(userId,tour.Id);
            string description = string.Empty;
            string reminder = string.Empty;
            string update = string.Empty;
            var accountInformation = accountRepository.GetImageStorageName(accountId);
            var userName = userRepository.GetUserName(userId);
            string accountName = accountInformation.AccountName;
            IEnumerable<int> contactIdS = null;
            Guid icsGuid = new Guid();
            if (isPrivate)
                contactIdS = contactIDs;

            if (!string.IsNullOrEmpty(tour.TourDetails))
                description = tour.TourDetails;

            if (tour.IcsCanlender == true)
            {
                string subject = "SmartTouch - Tour Reminder -" + accountName + ":" + description + "";
                icsGuid = GenerateIcsFile(tour.TourDate, tour.TourDate, location, description, subject, userName);
            }

            if (tourViewModel.EmailRequestGuid.HasValue && !newUserGuid)
            {
                #region Sending Reminder Email
                if (tourViewModel.SelectedReminderTypes.Contains(ReminderType.Email) && tourViewModel.EmailRequestGuid.Value != null && loginToken != null && UserGuid != new Guid())         //Update reminder
                {
                    LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                    sendMailRequest.TokenGuid = loginToken.Value;
                    sendMailRequest.From = fromEmail;
                    sendMailRequest.ScheduledTime = tour.ReminderDate;
                    sendMailRequest.Body = GenerateEmailBody(tour.Id, accountId, tour, address, phone, reminder, contactIdS, isPrivate);
                    sendMailRequest.To = new List<string>() { userRepository.GetUserPrimaryEmail(userId) };
                    sendMailRequest.AttachmentGUID = icsGuid;
                    sendMailRequest.AccountDomain = accountDomain;
                    sendMailRequest.AccountID = accountId;
                    mailService.UpdateScheduledReminder(UserGuid, sendMailRequest);
                }
                #endregion
            }
            else if (tourViewModel.SelectedReminderTypes.Contains(ReminderType.Email) && tourViewModel.EmailRequestGuid.HasValue && loginToken != null)          //Insert reminder
            {
                reminder = "Reminder";
                RemindByEmail(accountId, tour.Id, tour, fromEmail, requestGuid, loginToken.Value, address, phone, accountDomain, location, tour.IcsCanlender, reminder, update, userId, contactIdS, false);
            }
        }

        private void TextReminderOperation(TourViewModel tourViewModel, Tour tour, string fromText, Guid requestGuid, Guid loginToken, int accountId, int userId,bool userNewGuid)
        {
            TextService textService = new TextService();
            Guid UserGuid = tourRepository.GetUserTextEmailGuid(userId,tour.Id);
            if (tourViewModel.TextRequestGuid.HasValue && !userNewGuid)
            {
                #region Sending Text Reminder Email.
                if (tourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage) && tourViewModel.TextRequestGuid.Value != null && loginToken != null && UserGuid != new Guid())         //Update reminder
                {
                    var phoneNumber = userRepository.GetUserPrimaryPhoneNumber(userId);

                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest sendTextRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest();
                        sendTextRequest.TokenGuid = loginToken;
                        sendTextRequest.From = fromText;
                        sendTextRequest.ScheduledTime = tour.ReminderDate;
                        sendTextRequest.Message = tour.TourDetails;
                        sendTextRequest.To = new List<string>() { phoneNumber };
                        textService.UpdateScheduledReminder(UserGuid, sendTextRequest);
                    }
                }
                #endregion
            }
            else if (tourViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage) && loginToken != null)          //Insert reminder
            {
                RemindByText(accountId, tour, fromText, requestGuid, loginToken, userId);
            }
        }

        private void addToTopic(Tour tour, int accountId)
        {
            var messages = new List<TrackMessage>();

            foreach (var contact in tour.Contacts)
            {
                messages.Add(new TrackMessage()
                {
                    EntityId = tour.Id,
                    AccountId = accountId,
                    ContactId = contact.Id,
                    LeadScoreConditionType = (int)LeadScoreConditionType.ContactTourType,
                    LinkedEntityId = tour.TourType
                });
            }
            messageService.SendMessages(new Messaging.Messages.SendMessagesRequest()
            {
                Messages = messages
            });
        }

        public DeleteTourResponse DeleteTour(int tourId, int userId, int contactId)
        {
            Logger.Current.Verbose("Request received to delete tour with TourID " + tourId);
            DeleteTourResponse response = new DeleteTourResponse();
            MailService mailService = new MailService();
            TextService textServce = new TextService();
            IEnumerable<Guid> emailGuids = tourRepository.GetUserEmailGuids(tourId);
            IEnumerable<Guid> textGuids = tourRepository.GetUserTextGuids(tourId);
            IEnumerable<int> assContactIds = tourRepository.GetContactIds(tourId);
            foreach(Guid guid in emailGuids)
            {
                mailService.RemoveAllScheduledReminder(guid);
            }

            foreach (Guid guid in textGuids)
            {
                textServce.RemoveAllTextScheduledReminder(guid);
            }

            var guids = tourRepository.DeleteTour(tourId, userId, contactId);
            unitOfWork.Commit();
            if (guids != null)
            {
                List<int> keys = new List<int>(guids.Keys);
                foreach (var key in keys)
                {
                    if (key == 1 && guids[key] != null)
                        mailService.RemoveScheduledReminder(guids[key].Value);
                    //else if (key == 2 && guids[key] != null)
                    //  textService.RemoveScheduledReminder(guids[key].Value);
                }
            }

            if (contactId != 0)
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = new List<int>(){ contactId }, IndexType = 1 });
            else
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = assContactIds.ToList(), IndexType = 1 });

            Logger.Current.Informational("Tour deleted successfully.");
            return response;
        }

        public GetTourContactsCountResponse TourContactsCount(int tourId)
        {
            Logger.Current.Verbose("Request received to get the contacts count for tour with TourID " + tourId);
            GetTourContactsCountResponse response = new GetTourContactsCountResponse();
            response.Count = tourRepository.TourContactsCount(tourId);
            response.SelectAll = tourRepository.IsTourFromSelectAll(tourId);
            Logger.Current.Informational("Contacts Count = " + response.Count);
            return response;
        }

        public GetContactTourIsCreatedResponse IsTourCreate(int contactId)
        {
            //Logger.Current.Verbose("Request received to get the contacts count for tour with contactId " + contactId);
            GetContactTourIsCreatedResponse response = new GetContactTourIsCreatedResponse();
            response.ContactCommunities = tourRepository.GetContactComunity(contactId);
            //Logger.Current.Informational("Contacts Count = " + response.Count);
            return response;
        }

        public ReIndexDocumentResponse ReIndexTours(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for reindexing tours.");

            IEnumerable<Tour> documents = tourRepository.FindAll();
            int count = indexingService.ReIndexAll<Tour>(documents);

            return new ReIndexDocumentResponse() { Documents = count };
        }

        public void isTourValid(Tour tour, RequestOrigin? origin)
        {
            Logger.Current.Verbose("Request received to validate tour with TourID " + tour.Id);
            IEnumerable<BusinessRule> brokenRules = new List<BusinessRule>();
            if (origin == RequestOrigin.Outlook)
                brokenRules = tour.OutlookValidation();
            else
                brokenRules = tour.GetBrokenRules();

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

        public GetDashboardChartDetailsResponse GetToursBySourceAreaChartDetails(GetDashboardChartDetailsRequest request)
        {
            Logger.Current.Verbose("Request received to validate ToursBySource " + request.AccountId);
            DashboardChartDetailsViewModel viewModel = new DashboardChartDetailsViewModel();
            GetDashboardChartDetailsResponse response = new GetDashboardChartDetailsResponse();
            var isAccountAdmin = true;

            var tourData = tourRepository.ToursByLeadsourceAreaChartDetails(request.AccountId, request.UserId, isAccountAdmin, request.FromDate, request.ToDate);
            if (tourData != null && tourData.Any())
            {
                IEnumerable<dynamic> tours = tourData.GroupBy(tl => tl.DateNo).Select(g => new
                {
                    DateNumber = g.Key,
                    Present = g.Where(p => p.Present != 0).Select(pt => pt.Present).FirstOrDefault(),
                    Previous = g.Where(p => p.Previous != 0).Select(pt => pt.Previous).FirstOrDefault()
                });
                viewModel.Chart1Details = tours.OrderBy(t => t.DateNumber);
                viewModel.PresentCount = tours.Sum(t => t.Present);
                viewModel.PreviousCount = tours.Sum(t => t.Previous);
            }
            response.ChartDetailsViewModel = viewModel;
            return response;

        }

        public GetDashboardChartDetailsResponse GetToursBySourcePieChartDetails(GetDashboardChartDetailsRequest request)
        {
            Logger.Current.Verbose("Request received to validate ToursBySource " + request.AccountId);
            DashboardChartDetailsViewModel viewModel = new DashboardChartDetailsViewModel();
            GetDashboardChartDetailsResponse response = new GetDashboardChartDetailsResponse();
            DashboardPieChartDetails chartDetails = new DashboardPieChartDetails();
            var isAccountAdmin = true;

            var details = tourRepository.ToursBySourcePieChartDetails(request.AccountId, request.UserId, isAccountAdmin, request.FromDate, request.ToDate);
            if (details != null)
            {
                var otherDetails = details.OrderByDescending(d => d.TotalCount).Skip(5);
                var pieChartDetails = details.OrderByDescending(d => d.TotalCount).Take(5);
                if (otherDetails.Any())
                {
                    chartDetails.DropdownValue = "Others";
                    chartDetails.TotalCount = otherDetails.Sum(od => od.TotalCount);
                    pieChartDetails.Append(chartDetails);
                }
                viewModel.Chart2Details = pieChartDetails;
            }
            response.ChartDetailsViewModel = viewModel;
            return response;

        }

        public GetDashboardChartDetailsResponse GetToursByTypeBarChartDetails(GetDashboardChartDetailsRequest request)
        {
            Logger.Current.Verbose("Request received to validate ToursByType " + request.AccountId);
            DashboardChartDetailsViewModel viewModel = new DashboardChartDetailsViewModel();
            GetDashboardChartDetailsResponse response = new GetDashboardChartDetailsResponse();
            var isAccountAdmin = false;
            if (request.IsSTadmin == true)
                isAccountAdmin = true;
            else
                isAccountAdmin = cachingService.IsAccountAdmin(request.RoleId, request.AccountId);
            var chartDetails = tourRepository.ToursByTypeBarChartDetails(request.AccountId, request.UserId, true, request.FromDate, request.ToDate);
            if (chartDetails != null && chartDetails.Any())
            {
                var maxTotalvisits = chartDetails.MaxBy(mv => mv.TotalVisits).TotalVisits;
                var maxUniqueVisits = chartDetails.MaxBy(mv => mv.UniqueVisitors).UniqueVisitors;
                viewModel.MaxValue = maxTotalvisits > maxUniqueVisits ? maxTotalvisits : maxUniqueVisits;
            }
            viewModel.Chart1Details = chartDetails;
            response.ChartDetailsViewModel = viewModel;
            return response;

        }

        public GetDashboardChartDetailsResponse GetToursByTypeFunnelChartDetails(GetDashboardChartDetailsRequest request)
        {
            Logger.Current.Verbose("Request received to validate ToursByType " + request.AccountId);
            DashboardChartDetailsViewModel viewModel = new DashboardChartDetailsViewModel();
            GetDashboardChartDetailsResponse response = new GetDashboardChartDetailsResponse();
            var isAccountAdmin = false;

            if (isAccountAdmin == false)
            {
                bool isPrivate = cachingService.IsModulePrivate(AppModules.Opportunity, request.AccountId);
                if (isPrivate == false)
                    isAccountAdmin = true;
            }
            var details = opportunityRepository.OppertunityPipelinefunnelChartDetails(request.AccountId, new List<int>() { request.UserId }.ToArray() , isAccountAdmin, request.FromDate, request.ToDate);
            if (details != null && details.Any())
                viewModel.Chart2Details = details;
            response.ChartDetailsViewModel = viewModel;
            return response;

        }

        public GetToursToSyncResponse GetToursToSync(GetToursToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the updated tours to sync for user " + request.RequestedBy);
            GetToursToSyncResponse response = new GetToursToSyncResponse();

            IEnumerable<Tour> toursToSync = tourRepository.GetToursToSync(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp, request.FirstSync, request.OperationType);

            IEnumerable<CRMOutlookSync> crmOutlookSyncViewModels = contactRepository.GetEntityOutlookSyncMap(request.AccountId, (int)request.RequestedBy
               , request.MaxNumRecords, request.TimeStamp, toursToSync.Select(c => c.Id));

            //IEnumerable<KeyValuePair<int, int>> completedContacts = tourRepository.GetContactCompletedTours(crmOutlookSyncViewModels.Select(c => c.EntityID));

            if (toursToSync != null && toursToSync.Any())
                Logger.Current.Verbose("Syncing tours:  " + toursToSync.Select(c => c.Id).ToArray().ToString());

            response.ToursToSync = Mapper.Map<IEnumerable<Tour>, IEnumerable<TourViewModel>>(toursToSync);
            response.CRMOutlookSyncMappings = Mapper.Map<IEnumerable<CRMOutlookSync>, IEnumerable<CRMOutlookSyncViewModel>>(crmOutlookSyncViewModels);
            foreach (var mapping in response.CRMOutlookSyncMappings)
            {
                mapping.Tour = response.ToursToSync.Where(a => a.TourID == mapping.EntityID).FirstOrDefault();
            }

            //response.CompletedContacts = completedContacts;
            return response;
        }

        public GetToursToSyncResponse GetDeletedToursToSync(GetToursToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the deleted tour to sync for user " + request.RequestedBy);
            GetToursToSyncResponse response = new GetToursToSyncResponse();

            response.DeletedTours = tourRepository.GetDeletedToursToSync(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp);

            return response;
        }

    }
}
