using AutoMapper;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using RestSharp.Extensions.MonoHttp;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Action;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.ImplicitSync;
using SmartTouch.CRM.ApplicationServices.Messaging.Messages;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.Domain;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using DA = SmartTouch.CRM.Domain.Actions;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Notes;

namespace SmartTouch.CRM.ApplicationServices.ServiceImplementations
{
    public class ActionService : IActionService
    {
        readonly DA.IActionRepository actionRepository;
        readonly ITagRepository tagRepository;
        readonly IUserRepository userRepository;
        readonly IUnitOfWork unitOfWork;
        readonly IAccountService accountService;
        readonly IServiceProviderRepository serviceProvider;
        readonly IContactRepository contactRepository;
        readonly IUrlService urlService;
        readonly IAccountRepository accountRepository;
        readonly IMessageService messageService;
        readonly IContactService contactService;
        readonly ICachingService cachingService;
        readonly INoteService noteService;

        IIndexingService indexingService;

        public ActionService(DA.IActionRepository actionRepository, ITagRepository tagRepository,
            IUnitOfWork unitOfWork, IIndexingService indexingService, IServiceProviderRepository serviceProvider, IUserRepository userRepository,
            IMessageService messageService, IContactRepository contactRepository, IUrlService urlService, IAccountService accountService,
            IAccountRepository accountRepository, IContactService contactService, ICachingService cachingService, INoteService noteService)
        {
            if (actionRepository == null) throw new ArgumentNullException("actionRepository");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            this.actionRepository = actionRepository;
            this.tagRepository = tagRepository;
            this.userRepository = userRepository;
            this.unitOfWork = unitOfWork;
            this.messageService = messageService;
            this.serviceProvider = serviceProvider;
            this.indexingService = indexingService;
            this.contactRepository = contactRepository;
            this.accountService = accountService;
            this.accountRepository = accountRepository;
            this.urlService = urlService;
            this.contactService = contactService;
            this.cachingService = cachingService;
            this.noteService = noteService;
        }

        public GetActionResponse GetAction(GetActionRequest request)
        {
            Logger.Current.Verbose("Request to fetch Action based on ActionId and ContactId");
            GetActionResponse response = new GetActionResponse();
            Logger.Current.Informational("ActionId : " + request.Id);
            Logger.Current.Informational("ContactId : " + request.ContactId);
            DA.Action action = actionRepository.FindBy(request.Id, request.ContactId);
            if (action == null)
            {
                response.Exception = GetActionsNotFoundException();
            }
            response.ActionViewModel = Mapper.Map<DA.Action, ActionViewModel>(action);
            response.ActionViewModel.IsCompleted = actionRepository.GetActionCompletedStatus(request.Id);
            response.ActionViewModel.PreviousCompletedStatus = response.ActionViewModel.IsCompleted;
            response.ActionViewModel.ActionTypeValue = actionRepository.GetActionTypeValueById(response.ActionViewModel.ActionType.HasValue ? response.ActionViewModel.ActionType.Value : (short)0);
            return response;
        }

        public GetActionListResponse GetContactActions(GetActionListRequest request)
        {
            Logger.Current.Verbose("Request to fetch Action based on ContactId");
            GetActionListResponse response = new GetActionListResponse();
            Logger.Current.Informational("ContactId : " + request.Id);
            IEnumerable<DA.Action> actions = actionRepository.FindByContact(request.Id);

            if (actions == null)
                response.Exception = GetActionsNotFoundException();
            else
            {
                IEnumerable<ActionViewModel> actionlist = Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionViewModel>>(actions);
             
                response.ActionListViewModel = actionlist;
            }
            return response;
        }

        public GetActionListResponse GetOpportunityActions(GetActionListRequest request)
        {
            Logger.Current.Verbose("Request to fetch Action based on ContactId");
            GetActionListResponse response = new GetActionListResponse();
            Logger.Current.Informational("ContactId : " + request.Id);
            IEnumerable<DA.Action> actions = actionRepository.FindByOpportunity(request.Id);

            if (actions == null)
            {
                response.Exception = GetActionsNotFoundException();
            }
            else
            {
                IEnumerable<ActionViewModel> actionlist = Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionViewModel>>(actions);
                response.ActionListViewModel = actionlist;
            }
            return response;
        }

        private void UpdateLastTouched(List<LastTouchedDetails> lastTouchedDetails, short actionType, int accountId)
        {
            var dropdownValues = cachingService.GetDropdownValues(accountId);

            var dropdowntype = dropdownValues.Where(p => p.DropdownID == (byte)DropdownFieldTypes.ActionType).Select(p => p.DropdownValuesList).FirstOrDefault()
                           .Where(d => d.DropdownValueID == actionType).Select(p => p.DropdownValueTypeID).FirstOrDefault();

            contactRepository.UpdateLastTouchedInformation(lastTouchedDetails, AppModules.ContactActions, dropdowntype);
            contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = lastTouchedDetails.Select(p => p.ContactID).ToList(), Ids = lastTouchedDetails.ToLookup(o => o.ContactID, o => { return true; }) });
        }

        public InsertActionResponse InsertAction(InsertActionRequest request)
        {
            Logger.Current.Verbose("Request for inserting an action");
            Logger.Current.Informational("Action Contacts count :" + request.ActionViewModel.Contacts.Count());
            var createdBy = request.ActionViewModel.CreatedBy;
            Guid? emailGuid = Guid.NewGuid();
            Guid? textGuid = Guid.NewGuid();
            IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
            IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
            DA.Action action = Mapper.Map<ActionViewModel, DA.Action>(request.ActionViewModel);
            int? mailBulkID = null;
            if (action.ReminderTypes.Contains(ReminderType.PopUp))
                action.NotificationStatus = NotificationStatus.New;

            IsActionValid(action, request.RequestedFrom);
            DA.Action newAction = new DA.Action();
            IEnumerable<int> contactIDS = null;
            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            if (isPrivate)
            {
                #region Inserting Action in Private Mode
                IEnumerable<ContactOwner> contactOwners = contactRepository.GetAllContactOwners(action.Contacts.Select(c => c.ContactID).ToList(), action.OwnerIds, action.CreatedBy);
                foreach (int ownId in contactOwners.Select(s => s.OwnerID).Distinct())
                {
                    if (action.ReminderTypes.Contains(ReminderType.Email))
                        emailGuids.Add(ownId, Guid.NewGuid());
                    if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                        textGuids.Add(ownId, Guid.NewGuid());

                    contactIDS = contactOwners.Where(u => u.OwnerID == ownId).Select(s => s.ContactID).ToList();
                    if (createdBy != ownId)
                        action.OwnerIds = new List<int>() { createdBy, ownId };
                    else
                        action.OwnerIds = new List<int>() { ownId };

                    action.ContactIDS = contactIDS;
                    action.EmailRequestGuid = action.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                    action.TextRequestGuid = action.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;
                    action.EmailGuids = emailGuids;
                    action.TextGuids = textGuids;
                    actionRepository.Insert(action);
                    newAction = unitOfWork.Commit() as DA.Action;

                    foreach (Tag tag in action.Tags.Where(t => t.Id == 0))
                    {
                        Tag savedTag;
                        if (tag.Id == 0)
                        {
                            savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                            indexingService.IndexTag(savedTag);
                            accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                        }
                    }

                    if (action.Contacts != null && action.Contacts.Any())
                    {
                        this.UpdateActionBulkData(newAction.Id, request.AccountId, (int)request.RequestedBy, request.AccountPrimaryEmail, request.AccountDomain, action.IcsCanlender, action.IcsCanlenderToContact, request.AccountAddress, 
                            false, contactIDS, isPrivate, ownId, emailGuids, textGuids);
                    }
                    if (request.ActionViewModel.IsCompleted == true)
                    {
                        List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(newAction.Id, (bool)request.ActionViewModel.IsCompleted, null, request.ActionViewModel.OppurtunityId, true,
                            (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(1), true, request.ActionViewModel.MailBulkId);
                        this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                           request.AccountId, request.RequestedBy.Value);
                        UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                    }

                    #region For Bulk Operation with Actions
                    if (action.SelectAll == true)
                    {
                        BulkOperations operationData = new BulkOperations()
                        {
                            OperationID = newAction.Id,
                            OperationType = (int)BulkOperationTypes.Action,
                            SearchCriteria = HttpUtility.HtmlEncode(request.SelectAllSearchCriteria),
                            AdvancedSearchCriteria = request.AdvancedSearchCritieria,
                            SearchDefinitionID = null,
                            AccountID = request.AccountId,
                            UserID = (int)request.RequestedBy,
                            RoleID = request.RoleId,
                            AccountPrimaryEmail = request.AccountPrimaryEmail,
                            AccountDomain = request.AccountDomain,
                            ActionCompleted = request.ActionViewModel.IsCompleted
                        };
                        InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                        {
                            OperationData = operationData,
                            AccountId = request.AccountId,
                            RequestedBy = request.RequestedBy,
                            CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                            RoleId = request.RoleId,
                            DrillDownContactIds = request.DrillDownContactIds
                        };
                        accountService.InsertBulkOperation(bulkOperationRequest);
                    }
                    #endregion
                    actionRepository.UpdateCRMOutlookMap(newAction, request.RequestedFrom);
                }
                #endregion
            }
            else
            {
                #region Inserting Action In Public Mode
                int ownerId = 0;
                if (action.ReminderTypes.Contains(ReminderType.Email))
                    foreach (int owId in action.OwnerIds)
                    {
                        emailGuids.Add(owId, Guid.NewGuid());
                    }
                if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                    foreach (int owId in action.OwnerIds)
                    {
                        textGuids.Add(owId, Guid.NewGuid());
                    }

                action.EmailRequestGuid = action.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                action.TextRequestGuid = action.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;
                action.EmailGuids = emailGuids;
                action.TextGuids = textGuids;
                actionRepository.Insert(action);
                newAction = unitOfWork.Commit() as DA.Action;

                foreach (Tag tag in action.Tags.Where(t => t.Id == 0))
                {
                    Tag savedTag;
                    if (tag.Id == 0)
                    {
                        savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                        indexingService.IndexTag(savedTag);
                        accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                }

                if (action.Contacts != null && action.Contacts.Any())
                {
                    this.UpdateActionBulkData(newAction.Id, request.AccountId, (int)request.RequestedBy, request.AccountPrimaryEmail, request.AccountDomain, action.IcsCanlender, action.IcsCanlenderToContact, request.AccountAddress, 
                        false, contactIDS, isPrivate, ownerId, emailGuids, textGuids);
                }
                if (request.ActionViewModel.IsCompleted == true)
                {
                    List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(newAction.Id, (bool)request.ActionViewModel.IsCompleted, null, request.ActionViewModel.OppurtunityId, true,
                        (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(1),
                        (!string.IsNullOrEmpty(action.ActionTemplateHtml) && request.ActionViewModel.IsHtmlSave == true && request.ActionViewModel.ActionDate.Value.Date >= DateTime.Now.Date)?true:false, mailBulkID.HasValue?mailBulkID.Value:0);
                    this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = newAction.Id, ActionType = (short)request.ActionViewModel.ActionType } },
                           request.AccountId, request.RequestedBy.Value);
                    UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                }
                #region Bulk operation with actions
                if (action.SelectAll == true)
                {
                    BulkOperations operationData = new BulkOperations()
                    {
                        OperationID = newAction.Id,
                        OperationType = (int)BulkOperationTypes.Action,
                        SearchCriteria = HttpUtility.HtmlEncode(request.SelectAllSearchCriteria),
                        AdvancedSearchCriteria = request.AdvancedSearchCritieria,
                        SearchDefinitionID = null,
                        AccountID = request.AccountId,
                        UserID = (int)request.RequestedBy,
                        RoleID = request.RoleId,
                        AccountPrimaryEmail = request.AccountPrimaryEmail,
                        AccountDomain = request.AccountDomain,
                        ActionCompleted = request.ActionViewModel.IsCompleted
                    };
                    InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                    {
                        OperationData = operationData,
                        AccountId = request.AccountId,
                        RequestedBy = request.RequestedBy,
                        CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                        RoleId = request.RoleId,
                        DrillDownContactIds = request.DrillDownContactIds
                    };
                    accountService.InsertBulkOperation(bulkOperationRequest);
                }
                #endregion
                actionRepository.UpdateCRMOutlookMap(newAction, request.RequestedFrom);
                #endregion
            }

            #region Adding Action Details to NoteCategory.
            if (request.ActionViewModel.AddToNoteSummary == true && request.ActionViewModel.IsCompleted == true)
                InsertActionDetailsToNoteCategory(request);
            #endregion

            if (newAction != null && newAction.ActionContacts.IsAny())
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = newAction.ActionContacts.Select(s => s.ContactId).ToList(), IndexType = 1 });
            return new InsertActionResponse() { ActionViewModel = Mapper.Map<DA.Action, ActionViewModel>(newAction) };
        }

        public Domain.Actions.Action UpdateActionBulkData(int actionId, int accountId, int userId, string accountPrimaryEmail, string accountDomain, bool icsCalender, bool icsCalendarToContacts, string AccountAddress, bool isCompleted, IEnumerable<int> contactIds, bool isPrivate, int ownerId, IDictionary<int, Guid> emailGuids, IDictionary<int, Guid> textGuids)
        {
            var action = actionRepository.FindByActionId(actionId);
            #region Variable Declearation
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
            string reminder = string.Empty;
            string update = string.Empty;
            IList<int> assignedUserIds = new List<int>();
            #endregion

            #region Sending Ics and Assigned User Notification.
            if (action != null && (icsCalender == true || action.OwnerIds.IsAny() || icsCalendarToContacts))
            {
                address = accountRepository.GetAddress(accountId);
                if (address != null)
                {
                    location = address.City;
                    accountAddress = address.ToString();
                }
                accountPhoneNumber = accountRepository.GetPrimaryPhone(accountId);
                Logger.Current.Informational("Email is one of the reminder-type for this action");
                IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                if (serviceProviders != null && serviceProviders.Any())
                    senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                if (serviceProviders.FirstOrDefault() != null)
                    emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                else
                    throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                string fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? accountPrimaryEmail : senderEmail.EmailId;
                if (isPrivate)
                {
                    if (icsCalender || ownerId != action.CreatedBy)
                        RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, false, reminder, update, contactIds, isPrivate, ownerId);
                    if (icsCalendarToContacts)
                        RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, icsCalendarToContacts, reminder, update, contactIds, isPrivate, ownerId);
                }
                else
                {
                    if (icsCalender || icsCalendarToContacts)
                        assignedUserIds = action.OwnerIds;
                    else
                        assignedUserIds = action.OwnerIds.Where(s => s != action.CreatedBy).ToList();

                    foreach (int id in assignedUserIds)
                    {
                        emailGuid = Guid.NewGuid();
                        RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, false, reminder, update, contactIds, isPrivate, id);
                    }
                    if (icsCalendarToContacts)
                        RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, icsCalendarToContacts, reminder, update, contactIds, isPrivate, action.CreatedBy);
                }
            }
            #endregion

            #region Geting Primary Service Provider Details.

            if (action != null && (action.ReminderTypes.Contains(ReminderType.Email) || action.ReminderTypes.Contains(ReminderType.TextMessage)))
            {
                action.EmailRequestGuid = action.ReminderTypes.Contains(ReminderType.Email) ? emailGuid : null;
                action.TextRequestGuid = action.ReminderTypes.Contains(ReminderType.TextMessage) ? textGuid : null;
                if (action.ReminderTypes.Contains(ReminderType.Email))
                {
                    Logger.Current.Informational("Email is one of the reminder-type for this action");
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(accountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                }
                if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                {
                    Logger.Current.Informational("Text message is one of the reminder-type for this action");
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

            if (action != null)
            {
                #region Sending Remnder Emails
                if (action.ReminderTypes.Contains(ReminderType.Email))
                {
                    string fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? accountPrimaryEmail : senderEmail.EmailId;
                    reminder = "Reminder";
                    emailGuid = Guid.NewGuid();
                    if (string.IsNullOrEmpty(accountAddress))
                        accountAddress = AccountAddress;

                    if (isPrivate)
                    {
                        emailGuid = emailGuids.Where(k => k.Key == ownerId).Select(s => s.Value).FirstOrDefault();
                        RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, false, reminder, update, contactIds, isPrivate, ownerId);
                    }
                    else
                    {
                        foreach (int id in action.OwnerIds)
                        {
                            emailGuid = emailGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                            RemindByEmail(accountId, action.Id, action, fromEmail, emailGuid.Value, emailLoginToken.Value, accountAddress, accountPhoneNumber, accountDomain, location, icsCalender, false, reminder, update, contactIds, isPrivate, id);
                        }
                    }

                }
                #endregion

                #region Sending Text Reminder Notification
                if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                {
                    textGuid = Guid.NewGuid();
                    if (isPrivate)
                    {
                        textGuid = textGuids.Where(k => k.Key == ownerId).Select(s => s.Value).FirstOrDefault();
                        RemindByText(accountId, action, senderPhoneNumber, textGuid.Value, textLoginToken.Value, ownerId);
                    }
                    else
                    {
                        foreach (int id in action.OwnerIds)
                        {
                            textGuid = textGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                            RemindByText(accountId, action, senderPhoneNumber, textGuid.Value, textLoginToken.Value, id);
                        }
                    }
                }

                #endregion

            }

            this.addToTopic(action, accountId);
            #region Completing the Action Notification
            if (isCompleted == true)
            {
                List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(action.Id, isCompleted, null, 0, true,
                    userId, DateTime.Now.ToUniversalTime().AddMinutes(1), true, null);
                this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = action.Id, ActionType = (short)action.ActionType } },
                            accountId, userId);
                UpdateLastTouched(lastTouchedDetails, (short)action.ActionType, accountId);
            }
            #endregion
            return action;
        }

        void RemindByEmail(int accountId, int actionId, DA.Action action, string fromMail, Guid requestGuid, Guid loginToken, string address, string phone, string accountDomain, string location, bool icsCalender, bool icsCalendarToContacts, string reminder, string update, IEnumerable<int> contactIDs, bool isPrivate, int ownerId)
        {
            Logger.Current.Verbose("Request received to remind an action by email");
            DateTime date = new DateTime();
            Guid guid = new Guid();
            LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest mailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
            //var primaryEmail = userRepository.GetUserPrimaryEmail(ownerId);
            string userName = userRepository.GetUserName(ownerId);
            Account account = accountRepository.GetAccountMinDetails(accountId);
            string description = string.Empty;
            DateTime? StartDate = new DateTime();
            DateTime? EndDate = new DateTime();
            string actionType = actionRepository.GetActionTypeById(action.ActionType.Value);

            if (action.Details.Length > 60)
            {
                description = action.Details.Substring(0, 60);
            }
            else
                description = action.Details;

            if ((icsCalender == true || icsCalendarToContacts) && string.IsNullOrEmpty(reminder))
            {
                string subject = "Welcome SmartTouch® - Action Reminder -" + account.AccountName + " - " + actionType + "";
                StartDate = new DateTime(action.ActionDate.Value.Year, action.ActionDate.Value.Month, action.ActionDate.Value.Day, action.ActionStartTime.Value.Hour, action.ActionStartTime.Value.Minute, action.ActionStartTime.Value.Second);
                EndDate = new DateTime(action.ActionDate.Value.Year, action.ActionDate.Value.Month, action.ActionDate.Value.Day, action.ActionEndTime.Value.Hour, action.ActionEndTime.Value.Minute, action.ActionEndTime.Value.Second);
                guid = GenserateIcsFile(StartDate, EndDate, location, description, subject, userName);
            }

            #region Sending Email
            if (fromMail != null && requestGuid != new Guid() && loginToken != new Guid())
            {
                IEnumerable<int> contactIds = new List<int>();
                if (isPrivate)
                    contactIds = contactIDs;
                else
                    contactIds = action.Contacts.Select(s => s.ContactID);


                IEnumerable<string> tagNames = action.Tags.Select(s => s.TagName);
                date = action.CreatedOn.ToUtc().ToUserDateTime();
                mailRequest.Body = GenerateEmailBody(action, accountId, action.Details, contactIds, tagNames, address, phone, date.ToString(), icsCalendarToContacts);//action.Details;
                mailRequest.From = fromMail;
                mailRequest.IsBodyHtml = true;
                if (!string.IsNullOrEmpty(reminder))
                {
                    mailRequest.Subject = "SmartTouch® Action Reminder - " + account.AccountName + " - " + actionType;
                    mailRequest.ScheduledTime = action.RemindOn;
                }
                else if (!string.IsNullOrEmpty(update))
                {
                    mailRequest.Subject = "SmartTouch® Updated Action Notification- " + account.AccountName + " - " + actionType;
                }
                else
                {
                    mailRequest.Subject = "SmartTouch® New Action Notification- " + account.AccountName + " - " + actionType;
                }
                mailRequest.To = this.GetToEmails(icsCalender, icsCalendarToContacts, ownerId, contactIds);
                mailRequest.TokenGuid = loginToken;
                mailRequest.RequestGuid = requestGuid;
                if (string.IsNullOrEmpty(reminder) && (icsCalender == true || icsCalendarToContacts))
                    mailRequest.AttachmentGUID = guid;
                mailRequest.AccountDomain = accountDomain;
                mailRequest.CategoryID = (byte)EmailNotificationsCategory.ActionReminders;
                mailRequest.AccountID = accountId;
                MailService mailService = new MailService();
                mailService.Send(mailRequest);
            }
            else
                Logger.Current.Informational("Above details are not sufficient for sending an email");
            #endregion
        }

        private List<string> GetToEmails(bool icsCalendar, bool icsCalendarToContacts, int ownerId, IEnumerable<int> contactIDs) 
        {
            List<string> emails = new List<string>();
            if (icsCalendarToContacts)
            {
                IEnumerable<Person> contacts = contactRepository.GetEmailById(contactIDs);
                List<string> contactEmails = contacts.Where(w => !string.IsNullOrEmpty(w.Email)).Select(s => s.Email).ToList();
                if (contactEmails.IsAny())
                    emails.AddRange(contactEmails);
            }
            else
            {
                var primaryEmail = userRepository.GetUserPrimaryEmail(ownerId);
                emails.Add(primaryEmail);
            }
            return emails;
        }

        private static Guid GenserateIcsFile(DateTime? TourDate, DateTime? endDate, string location, string description, string subject, string userName)
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

        private static string[] IcsContent(DateTime? TourDate, DateTime? endDate, string location, string description, string subject, string userName)
        {

            string[] iscConent = { "BEGIN:VCALENDAR",
                                   "VERSION:2.0",
                                   "BEGIN:VEVENT",
                                   "STATUS:TENTATIVE",
                                   "DTSTART:" + TourDate.Value.ToString("yyyyMMdd\\THHmmss\\Z"),
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

        string GenerateEmailBody(DA.Action action, int accountId, string actionMessage, IEnumerable<int> contactIds, IEnumerable<string> tagNames, string address, string phone, string createdDate, bool icsCalendarToContacts)
        {
            string body = "";
            string filename = EmailTemplate.ActionsReminder.ToString() + ".txt";
            string savedFileName = Path.Combine(System.Configuration.ConfigurationManager.AppSettings["EMAILTEMPLATES_PHYSICAL_PATH"].ToString(), filename);
            string actionTags = string.IsNullOrEmpty(string.Join(", ", tagNames)) ? "No tags" : string.Join(", ", tagNames);
            string accountLogo = string.Empty;
            string actionType = actionRepository.GetActionTypeById(action.ActionType.Value);
            string startTime = action.ActionStartTime != null ? action.ActionStartTime.Value.ToUtc().ToUserDateTime().ToString("hh:mm tt") : string.Empty;
            string endTime = action.ActionEndTime != null ? action.ActionEndTime.Value.ToUtc().ToUserDateTime().ToString("hh:mm tt") : string.Empty;
            string reminderDate = action.RemindOn != null ? action.RemindOn.Value.ToUtc().ToUserDateTime().ToString() : string.Empty;
            #region Getting Account Primary Details
            ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });

            if (!String.IsNullOrEmpty(response.AccountLogoInfo.StorageName))
                accountLogo = urlService.GetUrl(accountId, ImageCategory.AccountLogo, response.AccountLogoInfo.StorageName);
            else
                accountLogo = "";
            string accountName = response.AccountLogoInfo.AccountName;
            string accountImage = string.Empty;
            if (!string.IsNullOrEmpty(accountLogo))
            {
                accountImage = accountImage + "<td align='right' valign='center' style='margin:0px;padding:0px 0px 25px 0px;'><img src='" + accountLogo + "' alt='" + accountName + "' style='width:100px;' width ='100' ></td>";
            }
            else
            {
                accountImage = "";
            }
            #endregion

            List<string> actionContacts = new List<string>();
            #region Creating Table With Contacts Ids
            if (contactIds.Any() && !icsCalendarToContacts)
            {
                List<DA.ActionContactsSummary> contactsSummary = actionRepository.GetActionContactsStatus(action.Id, contactIds, accountId);
                var domainurl = accountService.GetAccountDomainUrl(new GetAccountDomainUrlRequest() { AccountId = accountId }).DomainUrl;

                if (contactsSummary.Count > 0)
                    contactsSummary.ToList().ForEach(c =>
                    {
                        var link = string.Empty;
                        if (c.ContactType == ContactType.Person)
                            link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domainurl + "/person/" + c.ContactId + "'" + "style=" + "color:#0e749f;" +
                                ">" + c.ContactName + "</a> </span>";
                        else if (c.ContactType == ContactType.Company)
                            link = "<span style='color:#2B2B2B;'> <a href=" + "'" + "http://" + domainurl + "/company/" + c.ContactId + "'" + "style=" + "color:#0e749f;" +
                                ">" + c.ContactName + "</a> </span>";

                        var actionStatus = c.Status ? "Completed" : "Not completed";
                        actionContacts.Add("<tr><td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + link + "</td>" +
                            "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + c.PrimaryEmail + "</td>" +
                            "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" +
                            (!string.IsNullOrEmpty(c.PhoneCountryCode) ? "+" + c.PhoneCountryCode + " " : "") + c.PrimaryPhone + " " + (!string.IsNullOrEmpty(c.PhoneExtension) ? " Ext." + c.PhoneExtension : "") + "</td>" +
                            "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + actionStatus + "</td>" +
                            "<td style='margin:0px;padding:10px;font-size:12px; font-weight:normal;color:#555555;font-family:arial,sans-serif;border-bottom:solid 1px #d9d9d9;'>" + c.Lifecycle + "</td>" + "</tr>");
                    });
            }
            string contactsBody = string.Join("", actionContacts);
            #endregion
            #region Mapping the Details with Template.
            using (StreamReader reader = new StreamReader(savedFileName))
            {
                do
                {
                    body = reader.ReadToEnd().Replace("[ActionType]", actionType).Replace("[ActionDate]",action.ActionDate.Value.ToString("d")).Replace("[StartTime]", startTime).Replace("[EndTime]", endTime).Replace("[ReminderDATE]", reminderDate)
                            .Replace("[MESSAGE]", actionMessage).Replace("[CONTACTS]", contactsBody).Replace("[TAGS]", actionTags)
                            .Replace("[ADDRESS]", address).Replace("[PHONE]", phone).Replace("[CREATEDDATE]", createdDate.ToString())
                            .Replace("[AccountName]", accountName).Replace("[AccountImage]", accountImage);
                } while (!reader.EndOfStream);
            }
            #endregion
            return body;
        }

        void RemindByText(int accountId, DA.Action action, string fromNo, Guid requestGuid, Guid loginToken, int ownerId)
        {
            Logger.Current.Verbose("Request received to remind an action by text");

            LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest textRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest();
            var phoneNumber = userRepository.GetUserPrimaryPhoneNumber(action.CreatedBy);

            if (fromNo != null && !string.IsNullOrEmpty(phoneNumber) && requestGuid != new Guid() && loginToken != new Guid())
            {
                textRequest.From = fromNo;   //"5123374222";
                textRequest.Message = action.Details;
                textRequest.RequestGuid = requestGuid;
                textRequest.TokenGuid = loginToken;
                textRequest.ScheduledTime = action.RemindOn;
                textRequest.To = new List<string>() { phoneNumber };
                TextService textService = new TextService();
                textService.SendText(textRequest);
            }
        }

        private void addToTopic(DA.Action action, int accountId)
        {
            if (action.Tags.Any())
            {
                var messages = new List<TrackMessage>();
                foreach (var contact in action.Contacts)
                {
                    foreach (var tag in action.Tags.Where(t => t.Id > 0))
                    {
                        var message = new TrackMessage()
                        {
                            EntityId = action.Id,
                            AccountId = accountId,
                            ContactId = contact.ContactID,
                            UserId = action.CreatedBy,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactActionTagAdded,
                            LinkedEntityId = tag.Id
                        };
                        messages.Add(message);
                    }
                }
                messageService.SendMessages(new SendMessagesRequest() { Messages = messages });
            }
        }


        public DeleteActionResponse DeleteAction(DeleteActionRequest request)
        {
            Logger.Current.Verbose("Request for action delete for all contacts");

            Dictionary<int, Guid?> guids = new Dictionary<int, Guid?>();
            MailService mailService = new MailService();
            TextService textService = new TextService();
            IEnumerable<Guid> emailGuids = actionRepository.GetUserEmailGuids(request.ActionId);
            IEnumerable<Guid> textGuids = actionRepository.GetUserTextGuids(request.ActionId);
            IEnumerable<int> assContactIds = actionRepository.GetContactIds(request.ActionId);
            foreach (Guid guid in emailGuids)
            {
                mailService.RemoveAllScheduledReminder(guid);
            }

            foreach (Guid guid in textGuids)
            {
                textService.RemoveAllTextScheduledReminder(guid);
            }
            Logger.Current.Informational("ActionId to delete :" + request.ActionId);

            if (request.DeleteForAll)
                guids = actionRepository.DeleteActionForAll(request.ActionId, (int)request.RequestedBy);
            else
            {
                var contactId = request.ContactId.HasValue ? request.ContactId.Value : 0;
                guids = actionRepository.DeleteAction(request.ActionId, contactId, (int)request.RequestedBy);
            }
            if (guids != null)
            {
                List<int> keys = new List<int>(guids.Keys);
                foreach (var key in keys)
                {
                    if (key == 1 && guids[key] != null)
                        mailService.RemoveScheduledReminder(guids[key].Value);
                    else if (key == 2 && guids[key] != null)
                        textService.RemoveScheduledReminder(guids[key].Value);
                }
            }
            if (request.ContactId.HasValue)
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = new List<int>() { request.ContactId.Value }, IndexType = 1 });
            else
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = assContactIds.ToList(), IndexType = 1 });
            return new DeleteActionResponse();
        }

        public DeleteActionsResponse ActionsDelete(DeleteActionsRequest request)
        {
            Logger.Current.Verbose("Request for action delete");

            Dictionary<int, Guid?> guids = new Dictionary<int, Guid?>();
            MailService mailService = new MailService();
            TextService textService = new TextService();

            foreach (var actionId in request.ActionIds)
            {

                Logger.Current.Informational("ActionId to delete :" + actionId);
                IEnumerable<Guid> emailGuids = actionRepository.GetUserEmailGuids(actionId);
                IEnumerable<Guid> textGuids = actionRepository.GetUserTextGuids(actionId);
                foreach (Guid guid in emailGuids)
                {
                    mailService.RemoveAllScheduledReminder(guid);
                }

                foreach (Guid guid in textGuids)
                {
                    textService.RemoveAllTextScheduledReminder(guid);
                }
                guids = actionRepository.DeleteActionForAll(actionId, (int)request.RequestedBy);

                if (guids != null)
                {
                    List<int> keys = new List<int>(guids.Keys);
                    foreach (var key in keys)
                    {
                        if (key == 1 && guids[key] != null)
                            mailService.RemoveScheduledReminder(guids[key].Value);
                        else if (key == 2 && guids[key] != null)
                            textService.RemoveScheduledReminder(guids[key].Value);
                    }
                }

                IEnumerable<int> assContactIds = actionRepository.GetContactIds(actionId);
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = assContactIds.ToList(), IndexType = 1 });
            }
            return new DeleteActionsResponse();
        }

        public UpdateActionResponse UpdateAction(UpdateActionRequest request)
        {
            Logger.Current.Verbose("Request for updating an action");
            Logger.Current.Informational("ActionId to update :" + request.ActionViewModel.ActionId);
            var createdBy = request.ActionViewModel.CreatedBy;
            DA.Action action = Mapper.Map<ActionViewModel, DA.Action>(request.ActionViewModel);

            if (action.ReminderTypes.Contains(ReminderType.PopUp))
                action.NotificationStatus = NotificationStatus.New;
            #region Declearing Variables
            Guid emailGuid = Guid.NewGuid();
            Guid textGuid = Guid.NewGuid();
            IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
            IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
            IEnumerable<int> deletedusers = null;
            IEnumerable<int> deletedContactIds = null;
            MailService mailService = new MailService();
            TextService textService = new TextService();
            Guid? emailLoginToken = null;
            Guid? textLoginToken = null;
            string senderPhoneNumber = string.Empty;
            Email senderEmail = new Email();
            Address address = new Address();
            string location = string.Empty;
            string fromEmail = string.Empty;
            string reminder = string.Empty;
            string accountAddress = string.Empty;
            IList<int> assignedUserIds = new List<int>();
            IEnumerable<int> contactIDS = null;
            DA.Action updatedAction = new DA.Action();
            int? mailBulkID = null;
            #endregion

            if (string.IsNullOrEmpty(accountAddress))
                accountAddress = request.AccountAddress;
            IsActionValid(action, request.RequestedFrom);

            bool isPrivate = cachingService.IsModulePrivate(AppModules.Contacts, request.AccountId);
            IEnumerable<int> assignedOwnerIds = actionRepository.GetAllOwnerIds(action.Id);
            IEnumerable<int> assContactIds = actionRepository.GetContactIds(action.Id);
            if (assContactIds.IsAny())
                deletedContactIds = assContactIds.Where(w => !action.Contacts.Select(s => s.ContactID).Contains(w)).Select(se => se).ToList();
            #region Deleting Reminder email and text notification for deleted users
            if (assignedOwnerIds.IsAny())
                deletedusers = assignedOwnerIds.Where(i => !action.OwnerIds.Contains(i)).Select(s => s).ToList();

            if (action.ReminderTypes.Contains(ReminderType.Email))
                foreach (int delId in deletedusers)
                {
                    Guid userEmailGuid = actionRepository.GetUserEmailGuid(action.Id, delId);
                    if (userEmailGuid != new Guid())
                        mailService.RemoveAllScheduledReminder(userEmailGuid);
                }

            if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                foreach (int delId in deletedusers)
                {
                    Guid userTextGuid = actionRepository.GetUserTextGuid(action.Id, delId);
                    if (userTextGuid != new Guid())
                        textService.RemoveAllTextScheduledReminder(userTextGuid);
                }

            #endregion
            if (isPrivate)
            {
                #region Updating Action while contacts in Private mode.
                IEnumerable<ContactOwner> contactOwners = contactRepository.GetAllContactOwners(action.Contacts.Select(c => c.ContactID).ToList(), action.OwnerIds, action.CreatedBy);
                foreach (int owId in contactOwners.Select(o => o.OwnerID).Distinct())
                {
                    if (action.ReminderTypes.Contains(ReminderType.Email))
                    {
                        if (!assignedOwnerIds.Contains(owId))
                            emailGuids.Add(owId, Guid.NewGuid());
                    }

                    if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                    {
                        if (!assignedOwnerIds.Contains(owId))
                            textGuids.Add(owId, Guid.NewGuid());
                    }

                    contactIDS = contactOwners.Where(u => u.OwnerID == owId).Select(s => s.ContactID).ToList();

                    if (createdBy != owId)
                        action.OwnerIds = new List<int>() { createdBy, owId };
                    else
                        action.OwnerIds = new List<int>() { owId };

                    action.ContactIDS = contactIDS;
                    action.EmailGuids = emailGuids;
                    action.TextGuids = textGuids;

                    actionRepository.Update(action);
                    updatedAction = unitOfWork.Commit() as DA.Action;

                    #region Sending Ics and assigned user notifications.
                    if (action != null && (action.IcsCanlender == true || action.OwnerIds.IsAny() || action.IcsCanlenderToContact))
                    {
                        emailGuid = Guid.NewGuid();
                        address = accountRepository.GetAddress(request.AccountId);
                        if (address != null)
                        {
                            location = address.City;
                            accountAddress = address.ToString();
                        }

                        if (!request.ActionViewModel.EmailRequestGuid.HasValue)
                            action.EmailRequestGuid = emailGuid;
                        IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                        if (serviceProviders != null && serviceProviders.Any())
                            senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                        if (serviceProviders.FirstOrDefault() != null)
                            emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                        else
                            throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                        fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;

                        if (action.IcsCanlender || owId != action.CreatedBy)
                            RemindByEmail(request.AccountId, action.Id, action, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, 
                                request.ActionViewModel.IcsCanlender, false, reminder, "update", contactIDS, isPrivate, owId);
                        if (action.IcsCanlenderToContact)
                            RemindByEmail(request.AccountId, action.Id, action, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location,
                                false, action.IcsCanlenderToContact, reminder, "update", contactIDS, isPrivate, owId);
                    }
                    #endregion
                    #region Getting Defaul Service Provider Details
                    if (request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.Email))          //Insert reminder  && 
                    {
                        if (!request.ActionViewModel.EmailRequestGuid.HasValue)
                            action.EmailRequestGuid = emailGuid;
                        IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                        if (serviceProviders != null && serviceProviders.Any())
                            senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                        if (serviceProviders.FirstOrDefault() != null)
                            emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                        else
                            throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                    }
                    if (request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))          //Insert reminder    && 
                    {
                        if (!request.ActionViewModel.TextRequestGuid.HasValue)
                            action.TextRequestGuid = textGuid;
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
                    Logger.Current.Verbose("Action sync status is being changed to NotInSync. Action Id: " + request.ActionViewModel.ActionId);
                    actionRepository.UpdateCRMOutlookMap(updatedAction, request.RequestedFrom);

                    fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;
                    #region sending Reminder and  Text notifications
                    if (action != null && request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.Email))
                    {
                        Guid userGuid = emailGuids.Where(k => k.Key == owId).Select(v => v.Value).FirstOrDefault();
                        if (userGuid != new Guid())
                            EmailReminderOperation(request.ActionViewModel, action, fromEmail, userGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, contactIDS, isPrivate, owId, true, false);
                        else
                            EmailReminderOperation(request.ActionViewModel, action, fromEmail, emailGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, contactIDS, isPrivate, owId, false, false);

                    }

                    if (action != null && request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))
                    {
                        Guid userGuid = textGuids.Where(k => k.Key == owId).Select(v => v.Value).FirstOrDefault();
                        if (userGuid != new Guid())
                            TextReminderOperation(request.ActionViewModel, action, senderPhoneNumber, userGuid, textLoginToken, request.AccountId, owId, true);
                        else
                            TextReminderOperation(request.ActionViewModel, action, senderPhoneNumber, textGuid, textLoginToken, request.AccountId, owId, false);

                    }

                    #endregion
                    #region indexing tags
                    foreach (Tag tag in action.Tags.Where(t => t.Id == 0))
                    {
                        Tag savedTag;
                        if (tag.Id == 0)
                        {
                            savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                            indexingService.IndexTag(savedTag);
                            accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                        }
                        else
                            savedTag = tag;
                    }
                    #endregion
                    this.addToTopic(action, request.AccountId);
                    var contacts = request.ActionViewModel.Contacts.ToList();
                    #region Updating Lasttouched and Action Complete
                    if (request.ActionViewModel.IsFromDashboard == true)
                    {

                        if (request.ActionViewModel.PreviousCompletedStatus != request.ActionViewModel.IsCompleted)
                        {
                            List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(request.ActionViewModel.ActionId, request.ActionViewModel.IsCompleted, null,
                                request.ActionViewModel.OppurtunityId, true, (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(2), true, request.ActionViewModel.MailBulkId);
                            this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                            UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                        }
                    }

                    else if (request.ActionViewModel.PreviousCompletedStatus != request.ActionViewModel.IsCompleted && request.ActionViewModel.Contacts.Count() == 1)
                    {
                        List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(request.ActionViewModel.ActionId, request.ActionViewModel.IsCompleted, contacts[0].Id,
                            request.ActionViewModel.OppurtunityId, true, (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(2), true, request.ActionViewModel.MailBulkId);
                        this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                        UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                    }

                    else if (request.ActionViewModel.PreviousActionType != request.ActionViewModel.ActionType && request.ActionViewModel.IsCompleted == true && request.ActionViewModel.PreviousCompletedStatus == request.ActionViewModel.IsCompleted)
                    {
                        List<LastTouchedDetails> lastTouchedDetails = new List<LastTouchedDetails>();
                        foreach (var contact in contacts)
                        {
                            LastTouchedDetails details = new LastTouchedDetails();
                            details.ActionID = request.ActionViewModel.ActionId;
                            details.ContactID = contact.Id;
                            details.LastTouchedDate = DateTime.UtcNow;
                            lastTouchedDetails.Add(details);
                        }
                        this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                        UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                    }
                    #endregion
                }
                #endregion
            }
            else
            {
                #region Updating Action while contacts in Public mode
                #region Updating Action
                if (action.ReminderTypes.Contains(ReminderType.Email))
                    foreach (int assId in action.OwnerIds)
                    {
                        if (!assignedOwnerIds.Contains(assId))
                            emailGuids.Add(assId, Guid.NewGuid());

                    }

                if (action.ReminderTypes.Contains(ReminderType.TextMessage))
                    foreach (int assId in action.OwnerIds)
                    {
                        if (!assignedOwnerIds.Contains(assId))
                            textGuids.Add(assId, Guid.NewGuid());

                    }

                if (!string.IsNullOrEmpty(action.ActionTemplateHtml) && request.ActionViewModel.IsHtmlSave == true)
                {
                    string userPrimaryEmail = userRepository.GetUserPrimaryEmail(request.ActionViewModel.LastUpdatedBy);
                    mailBulkID = actionRepository.InsertBulkMailOperationDetails(action.Details, action.ActionTemplateHtml, updatedAction.Id, userPrimaryEmail);
                    action.MailBulkId = mailBulkID;
                }

                action.EmailGuids = emailGuids;
                action.TextGuids = textGuids;
                actionRepository.Update(action);
                updatedAction = unitOfWork.Commit() as DA.Action;

                #endregion

                if (action != null && (action.IcsCanlender == true || action.OwnerIds.IsAny() || action.IcsCanlenderToContact))
                {
                    #region Sending ICS and Assigned User Notifications
                    address = accountRepository.GetAddress(request.AccountId);
                    if (address != null)
                    {
                        location = address.City;
                        accountAddress = address.ToString();
                    }

                    if (!request.ActionViewModel.EmailRequestGuid.HasValue)
                        action.EmailRequestGuid = emailGuid;
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");

                    fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;

                    if (action.IcsCanlender)
                        assignedUserIds = action.OwnerIds;
                    else
                        assignedUserIds = action.OwnerIds.Where(s => s != action.CreatedBy).ToList();

                    foreach (int id in assignedUserIds)
                    {
                        emailGuid = Guid.NewGuid();
                        RemindByEmail(request.AccountId, action.Id, action, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, 
                            request.ActionViewModel.IcsCanlender, false ,reminder, "update", contactIDS, isPrivate, id);
                    }
                    if (action.IcsCanlenderToContact)
                        RemindByEmail(request.AccountId, action.Id, action, fromEmail, emailGuid, emailLoginToken.Value, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location,
                            request.ActionViewModel.IcsCanlender, action.IcsCanlenderToContact, reminder, "update", contactIDS, isPrivate, action.CreatedBy);
                    #endregion

                }

                #region Getting Default service Provider Details.
                if (request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.Email))          //Insert reminder  && 
                {
                    if (!request.ActionViewModel.EmailRequestGuid.HasValue)
                        action.EmailRequestGuid = emailGuid;
                    IEnumerable<ServiceProvider> serviceProviders = serviceProvider.GetAccountCommunicationProviders(request.AccountId, CommunicationType.Mail, MailType.TransactionalEmail);
                    if (serviceProviders != null && serviceProviders.Any())
                        senderEmail = serviceProvider.GetServiceProviderEmail(serviceProviders.FirstOrDefault().Id);
                    if (serviceProviders.FirstOrDefault() != null)
                        emailLoginToken = serviceProviders.FirstOrDefault().LoginToken;
                    else
                        throw new UnsupportedOperationException("[|Email providers are not configured for this account|]");
                }
                if (request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))          //Insert reminder    && 
                {
                    if (!request.ActionViewModel.TextRequestGuid.HasValue)
                        action.TextRequestGuid = textGuid;
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



                Logger.Current.Verbose("Action sync status is being changed to NotInSync. Action Id: " + request.ActionViewModel.ActionId);
                actionRepository.UpdateCRMOutlookMap(updatedAction, request.RequestedFrom);
                #region Sending Reminder and Text Notifications
                fromEmail = string.IsNullOrEmpty(senderEmail.EmailId) ? request.AccountPrimaryEmail : senderEmail.EmailId;
                if (action != null && request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.Email))
                {
                    foreach (int id in action.OwnerIds)
                    {
                        Guid userEmailGuid = emailGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                        if (userEmailGuid != new Guid())
                            EmailReminderOperation(request.ActionViewModel, action, fromEmail, userEmailGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, contactIDS, isPrivate, id, true, false);
                        else
                            EmailReminderOperation(request.ActionViewModel, action, fromEmail, emailGuid, emailLoginToken, request.AccountId, accountAddress, request.AccountPhoneNumber, request.AccountDomain, location, contactIDS, isPrivate, id, false, false);

                    }

                }

                if (action != null && request.ActionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage))
                {
                    foreach (int id in action.OwnerIds)
                    {
                        Guid userTextGuid = textGuids.Where(k => k.Key == id).Select(s => s.Value).FirstOrDefault();
                        if (userTextGuid != new Guid())
                            TextReminderOperation(request.ActionViewModel, action, senderPhoneNumber, userTextGuid, textLoginToken, request.AccountId, id, true);
                        else
                            TextReminderOperation(request.ActionViewModel, action, senderPhoneNumber, textGuid, textLoginToken, request.AccountId, id, false);

                    }

                }
                #endregion

                #region Indexing tags
                foreach (Tag tag in action.Tags.Where(t => t.Id == 0))
                {
                    Tag savedTag;
                    if (tag.Id == 0)
                    {
                        savedTag = tagRepository.FindBy(tag.TagName, request.AccountId);
                        indexingService.IndexTag(savedTag);
                        accountRepository.ScheduleAnalyticsRefresh(savedTag.Id, (byte)IndexType.Tags);
                    }
                    else
                        savedTag = tag;
                }
                #endregion
                this.addToTopic(action, request.AccountId);
                var contacts = request.ActionViewModel.Contacts.ToList();
                #region Updating Action Complete and Lasthouched
                if (request.ActionViewModel.IsFromDashboard == true)
                {
                    if (request.ActionViewModel.PreviousCompletedStatus != request.ActionViewModel.IsCompleted)
                    {
                        List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(request.ActionViewModel.ActionId, request.ActionViewModel.IsCompleted, null,
                            request.ActionViewModel.OppurtunityId, true, (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(2),
                            (!string.IsNullOrEmpty(action.ActionTemplateHtml) && request.ActionViewModel.IsHtmlSave == true && request.ActionViewModel.ActionDate.Value.Date >= DateTime.Now.Date) ? true : false, mailBulkID.HasValue ? mailBulkID.Value : 0);
                        this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                        UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                    }
                }

                else if (request.ActionViewModel.PreviousCompletedStatus != request.ActionViewModel.IsCompleted && request.ActionViewModel.Contacts.Count() == 1)
                {
                    List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(request.ActionViewModel.ActionId, request.ActionViewModel.IsCompleted, contacts[0].Id,
                        request.ActionViewModel.OppurtunityId, true, (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(2),
                        (!string.IsNullOrEmpty(action.ActionTemplateHtml) && request.ActionViewModel.IsHtmlSave == true && request.ActionViewModel.ActionDate.Value.Date >= DateTime.Now.Date) ? true : false, mailBulkID.HasValue ? mailBulkID.Value : 0);
                    this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                    UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                }

                else if (request.ActionViewModel.PreviousActionType != request.ActionViewModel.ActionType && request.ActionViewModel.IsCompleted == true && request.ActionViewModel.PreviousCompletedStatus == request.ActionViewModel.IsCompleted)
                {
                    List<LastTouchedDetails> lastTouchedDetails = new List<LastTouchedDetails>();
                    foreach (var contact in contacts)
                    {
                        LastTouchedDetails details = new LastTouchedDetails();
                        details.ActionID = request.ActionViewModel.ActionId;
                        details.ContactID = contact.Id;
                        details.LastTouchedDate = DateTime.UtcNow;
                        lastTouchedDetails.Add(details);
                    }
                    this.ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { new DA.Action() { Id = request.ActionViewModel.ActionId, ActionType = (short)request.ActionViewModel.ActionType } },
                            request.AccountId, request.RequestedBy.Value);
                    UpdateLastTouched(lastTouchedDetails, (short)request.ActionViewModel.ActionType, request.AccountId);
                }
                #endregion
                #endregion
            }

            #region Adding Action Details to NoteCategory.
            if (request.ActionViewModel.AddToNoteSummary== true && request.ActionViewModel.IsCompleted == true)
            {
                if(updatedAction.SelectAll)
                    actionRepository.ActionDetailsAddingToNoteSummary(new List<int>() { updatedAction.Id }, new List<int>() { }, request.AccountId, request.RequestedBy.Value);
                else
                    actionRepository.ActionDetailsAddingToNoteSummary(new List<int>() { updatedAction.Id }, updatedAction.ActionContacts.Where(c => c.IsCompleted == true).Select(s => s.ContactId).ToList(), request.AccountId, request.RequestedBy.Value);
            }
            #endregion


            if (deletedContactIds.IsAny() || (updatedAction != null && updatedAction.ActionContacts.IsAny()))
            {
                List<int> ids = updatedAction.ActionContacts.Select(s => s.ContactId).Concat(deletedContactIds).ToList();
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = ids, IndexType = 1 });
            }

            return new UpdateActionResponse() { ActionViewModel = Mapper.Map<DA.Action, ActionViewModel>(updatedAction) };
        }

        void EmailReminderOperation(ActionViewModel actionViewModel, DA.Action action, string fromEmail, Guid requestGuid, Guid? loginToken, int accountId, string address, string phone, string accountDomain, string location, IEnumerable<int> contactIDs, bool isPrivate, int ownerId, bool newUserGuid, bool icsCalendarToContacts)
        {
            MailService mailService = new MailService();
            Guid userGuid = actionRepository.GetUserEmailGuid(action.Id, ownerId);
            string description = string.Empty;
            string subject = string.Empty;
            string reminder = string.Empty;
            string update = string.Empty;
            string userName = userRepository.GetUserName(ownerId);
            Guid icsGuid = new Guid();
            string actionType = actionRepository.GetActionTypeById(action.ActionType.Value);
            ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameResponse response = accountService.GetStorageName(new ApplicationServices.Messaging.Accounts.GetAccountImageStorageNameRequest()
            {
                AccountId = accountId
            });
            string accountName = response.AccountLogoInfo.AccountName;

            if (!string.IsNullOrEmpty(action.Details))
                description = action.Details;

            if (action.IcsCanlender == true)
            {
                subject = "Welcome SmartTouch® - Action Reminder -" + accountName + " - " + actionType + "";
                icsGuid = GenserateIcsFile(action.ActionDate, action.ActionDate, location, description, subject, userName);
            }

            IEnumerable<int> contactIds = new List<int>();
            if (isPrivate)
                contactIds = contactIDs;
            else
                contactIds = action.Contacts.Select(s => s.ContactID);

            if (actionViewModel.EmailRequestGuid.HasValue && !newUserGuid)
            {
                if (!actionViewModel.SelectedReminderTypes.Contains(ReminderType.Email) && actionViewModel.EmailRequestGuid.Value != null)       //Delete reminder
                {
                    Logger.Current.Informational("Deleting Email reminder for Action : " + action.Id);
                    mailService.RemoveScheduledReminder(actionViewModel.EmailRequestGuid.Value);
                }
                else if (actionViewModel.SelectedReminderTypes.Contains(ReminderType.Email) && actionViewModel.EmailRequestGuid.Value != null && loginToken != null && userGuid != new Guid())         //Update reminder
                {
                    Logger.Current.Informational("Updating Email reminder for Action : " + action.Id);
                    LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest sendMailRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendMailRequest();
                    sendMailRequest.TokenGuid = loginToken.Value;
                    sendMailRequest.From = fromEmail;
                    sendMailRequest.ScheduledTime = action.RemindOn;
                    sendMailRequest.Subject = subject;
                    sendMailRequest.Body = GenerateEmailBody(action, accountId, action.Details, contactIds, action.Tags.Select(s => s.TagName), address, phone, action.CreatedOn.ToString(), icsCalendarToContacts);
                    sendMailRequest.To = new List<string>() { userRepository.GetUserPrimaryEmail(ownerId) };
                    sendMailRequest.AttachmentGUID = icsGuid;
                    sendMailRequest.AccountDomain = accountDomain;
                    sendMailRequest.AccountID = accountId;
                    mailService.UpdateScheduledReminder(userGuid, sendMailRequest);
                }
            }
            else if (actionViewModel.SelectedReminderTypes.Contains(ReminderType.Email) && actionViewModel.EmailRequestGuid.HasValue && loginToken != null)          //Insert reminder
            {
                Logger.Current.Informational("Inserting Email reminder for Action : " + action.Id);
                reminder = "Reminder";
                RemindByEmail(accountId, action.Id, action, fromEmail, requestGuid, loginToken.Value, address, phone, accountDomain, location, action.IcsCanlender, action.IcsCanlenderToContact,reminder, update, contactIds, isPrivate, ownerId);
            }
        }

        void TextReminderOperation(ActionViewModel actionViewModel, DA.Action action, string fromText, Guid requestGuid, Guid? loginToken, int accountId, int ownerId, bool newUserGuid)
        {
            TextService textService = new TextService();
            Guid userTextGuid = actionRepository.GetUserTextGuid(action.Id, ownerId);
            if (actionViewModel.TextRequestGuid.HasValue && !newUserGuid)
            {
                if (actionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage) && actionViewModel.TextRequestGuid.Value != null && loginToken != null && fromText != null && userTextGuid != new Guid())   //Update reminder
                {
                    var phoneNumber = userRepository.GetUserPrimaryPhoneNumber(action.CreatedBy);

                    if (!string.IsNullOrEmpty(phoneNumber))
                    {
                        Logger.Current.Informational("Updating Text reminder for Action : " + action.Id);
                        LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest sendTextRequest = new LandmarkIT.Enterprise.CommunicationManager.Requests.SendTextRequest();
                        sendTextRequest.TokenGuid = loginToken.Value;
                        sendTextRequest.From = fromText;
                        sendTextRequest.ScheduledTime = action.RemindOn;
                        sendTextRequest.Message = action.Details;
                        sendTextRequest.To = new List<string>() { phoneNumber };
                        textService.UpdateScheduledReminder(userTextGuid, sendTextRequest);
                    }
                }
            }
            else if (actionViewModel.SelectedReminderTypes.Contains(ReminderType.TextMessage) && loginToken != null)          //Insert reminder
            {
                Logger.Current.Informational("Inserting Text reminder for Action : " + action.Id);
                RemindByText(accountId, action, fromText, requestGuid, loginToken.Value, 89);
            }
        }

        public DeactivateActionContactResponse ContactDeleteForAction(DeactivateActionContactRequest request)
        {
            DA.Action action = actionRepository.FindBy(request.ActionId);
            RawContact contact = action.Contacts.SingleOrDefault(c => c.ContactID == request.ContactId);
            if (action.Contacts.Count == 1)
            {
                action.Contacts.Remove(contact);
                //actionRepository.Update(action);
                actionRepository.Delete(action);
            }
            else
            {
                action.Contacts.Remove(contact);
                actionRepository.Update(action);
            }
            unitOfWork.Commit();
            accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = new List<int>() { request.ContactId }, IndexType = 1 });
            return new DeactivateActionContactResponse();
        }

        public CompletedActionResponse ActionStatus(CompletedActionRequest request)
        {
            Logger.Current.Verbose("Request for Action status change");
            Logger.Current.Informational("Action status :" + request.isCompleted);

            if (request.AddToNoteSummary == true && request.CompletedForAll == true)
               actionRepository.ActionDetailsAddingToNoteSummary(new List<int>() { request.actionId }, new List<int>() { }, request.AccountId, request.RequestedBy.Value);
            else if(request.AddToNoteSummary == true && request.contactId.HasValue)
                actionRepository.ActionDetailsAddingToNoteSummary(new List<int>() { request.actionId }, new List<int>() {request.contactId.Value }, request.AccountId, request.RequestedBy.Value);


            List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionCompleted(request.actionId, request.isCompleted, request.contactId, request.opportunityId, request.CompletedForAll, (int)request.RequestedBy, DateTime.Now.ToUniversalTime().AddMinutes(2), request.IsSchedule, request.MailBulkId);
            var action = actionRepository.FindBy(request.actionId);

            if (request.isCompleted == true)
            {
                UpdateLastTouched(lastTouchedDetails, (short)action.ActionType, request.AccountId);
                ActionCompleteTrackMessage(lastTouchedDetails, new List<DA.Action>() { action }, request.AccountId, request.RequestedBy.Value);
            }
            accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = action.ActionContacts.Select(s => s.ContactId).ToList(), IndexType = 1 });
            return new CompletedActionResponse();

        }

        public CompletedActionsResponse ActionsMarkedComplete(CompletedActionsRequest request)
        {
            Logger.Current.Verbose("Request for Actions status change");

            if(request.AddToNoteSummary)
                actionRepository.ActionDetailsAddingToNoteSummary(request.ActionIds.ToList(), new List<int>() { }, request.AccountId, request.RequestedBy.Value);


            List<LastTouchedDetails> lastTouchedDetails = actionRepository.ActionsMarkedComplete(request.ActionIds, (int)request.RequestedBy, request.UpdatedOn, request.IsSheduled);
            var actions = actionRepository.FindBy(request.ActionIds);

            foreach (var action in actions)
            {
                List<LastTouchedDetails> list = lastTouchedDetails.Where(p => p.ActionID == action.Id).ToList();
                UpdateLastTouched(list, (short)action.ActionType, request.AccountId);
                ActionCompleteTrackMessage(lastTouchedDetails, actions, request.AccountId, request.RequestedBy.Value);
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = action.ActionContacts.Select(s => s.ContactId).ToList(), IndexType = 1 });
            }
            return new CompletedActionsResponse();
        }

        public void ActionCompleteTrackMessage(List<LastTouchedDetails> lastTouchedDetails, IEnumerable<DA.Action> actions, int accountId, int createdBy)
        {
            if (lastTouchedDetails.IsAny())
            {
                var messages = new List<TrackMessage>();
                foreach (var detail in lastTouchedDetails)
                {
                    short? actionType = actions.Where(w => w.Id == detail.ActionID).Select(s => s.ActionType).FirstOrDefault();
                    if (actionType.HasValue)
                    {
                        var lastTouchedMessage = new TrackMessage()
                        {
                            EntityId = actionType.Value,
                            AccountId = accountId,
                            ContactId = detail.ContactID,
                            UserId = createdBy,
                            LeadScoreConditionType = (int)LeadScoreConditionType.ContactActionCompleted,
                            LinkedEntityId = detail.ActionID
                        };
                        messages.Add(lastTouchedMessage);
                    }
                }
                messageService.SendMessages(new SendMessagesRequest() { Messages = messages });
            }
        }

        public CompletedActionsResponse ActionsMarkedInComplete(CompletedActionsRequest request)
        {
            Logger.Current.Verbose("Request for Actions status change");

            actionRepository.ActionsMarkedInComplete(request.ActionIds, (int)request.RequestedBy, request.UpdatedOn, request.IsSheduled);

            var actions = actionRepository.FindBy(request.ActionIds);
            foreach (var action in actions)
                accountRepository.InsertIndexingData(new IndexingData() { EntityIDs = action.ActionContacts.Select(s => s.ContactId).ToList(), IndexType = 1 });
            return new CompletedActionsResponse();
        }

        public GetContactsCountResponse ActionContactsCount(GetContactsCountRequest request)
        {
            Logger.Current.Verbose("Request for action contacts count");
            Logger.Current.Informational("ActionId :" + request.Id);
            GetContactsCountResponse response = new GetContactsCountResponse();
            response.Count = actionRepository.ContactsCount(request.Id);
            response.SelectAll = actionRepository.IsActionFromSelectAll(request.Id);
            return response;
        }

        public GetActionListResponse GetUserCreatedActions(GetActionListRequest request)
        {
            Logger.Current.Verbose("Request to fetch User created Actions");
            GetActionListResponse response = new GetActionListResponse();
            request.SortField = request.SortField == null ? "RemindOn" : request.SortField;
            if (request.SortField != null)
            {
                var maps = SmartTouch.CRM.ApplicationServices.ObjectMappers.MapperConfigurationProvider.Instance.FindTypeMapFor<ActionViewModel, DA.Action>();
                foreach (var propertyMap in maps.GetPropertyMaps())
                {
                    if (propertyMap.SourceMember != null && request.SortField.Equals(propertyMap.SourceMember.Name))
                    {
                        request.SortField = propertyMap.DestinationProperty.MemberInfo.Name;
                        break;
                    }
                }
            }
            Logger.Current.Informational("userID : " + request.RequestedBy);
            DA.UserActions userActions = actionRepository.FindByUser(request.UserIds, request.AccountId, request.PageNumber, request.Limit, request.Name, request.Filter,
                request.SortField, request.IsDashboard, request.StartDate, request.EndDate, request.FilterByActionType, request.SortDirection);
            IEnumerable<DA.Action> actions = userActions.Actions;

            if (actions == null)
            {
                response.Exception = GetActionsNotFoundException();
            }
            else
            {
                IEnumerable<ActionViewModel> actionlist = Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionViewModel>>(actions);
                response.ActionListViewModel = actionlist;
                if (!request.IsDashboard)
                    response.TotalHits = userActions.TotalCount;
            }
            return response;
        }

        public ReIndexDocumentResponse ReIndexActions(ReIndexDocumentRequest request)
        {
            Logger.Current.Verbose("Request for reindexing actions.");
            IEnumerable<DA.Action> documents = actionRepository.FindAll();
            int count = indexingService.ReIndexAll<DA.Action>(documents);
            return new ReIndexDocumentResponse() { Documents = count };
        }

        public UnsupportedOperationException GetActionsNotFoundException()
        {
            Logger.Current.Informational("No actions found for this contact");
            throw new UnsupportedOperationException("[|Action not found for the contact.|]");
        }

        void IsActionValid(DA.Action action, RequestOrigin? origin)
        {
            IEnumerable<BusinessRule> brokenRules = new List<BusinessRule>();

            Logger.Current.Informational("Action broken rules count :" + brokenRules.Count());
            if (origin == RequestOrigin.Outlook)
                brokenRules = action.OutlookValidation();
            else
                brokenRules = action.GetBrokenRules();
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

        /// <summary>
        /// Get new tasks/actions from SmartCRM to sync with Outlook
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public GetTasksToSyncResponse GetNewActionsToSync(GetTasksToSyncRequest request)
        {
            Logger.Current.Verbose("In GetNewActionsToSync(). User " + request.RequestedBy);
            GetTasksToSyncResponse tasksToSyncResponse = new GetTasksToSyncResponse();

            IEnumerable<DA.Action> actionsToSync =
                actionRepository.GetNewActionsToSync(request.AccountId
                , (int)request.RequestedBy
                , request.MaxNumRecords
                , request.TimeStamp
                , request.FirstSync
                , request.OperationType);

            IEnumerable<CRMOutlookSync> crmOutlookSyncViewModels = new List<CRMOutlookSync>();

            if (actionsToSync != null && actionsToSync.Any())
            {
                Logger.Current.Verbose("Syncing new tasks:  " + actionsToSync.Select(c => c.Id).ToArray().ToString());
                crmOutlookSyncViewModels = contactRepository.GetEntityOutlookSyncMap(request.AccountId
                    , (int)request.RequestedBy
                    , request.MaxNumRecords
                    , request.TimeStamp
                    , actionsToSync.Select(c => c.Id));
            }
            else
            {
                Logger.Current.Verbose("No new tasks found to sync for user: " + request.RequestedBy);
            }

            tasksToSyncResponse.ActionsToSync = Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionViewModel>>(actionsToSync);
            var actionIdsToSync = actionsToSync.Select(c => c.Id).ToList();
            if (actionIdsToSync.Any())
            {
                IEnumerable<int> completedActions = getMultipleActionsCompletedStatuses(actionIdsToSync);
                foreach (var item in tasksToSyncResponse.ActionsToSync)
                {
                    item.IsCompleted = completedActions.Contains(item.ActionId);
                }
            }
            tasksToSyncResponse.CRMOutlookSyncMappings = Mapper.Map<IEnumerable<CRMOutlookSync>, IEnumerable<CRMOutlookSyncViewModel>>(crmOutlookSyncViewModels);
            foreach (var item in tasksToSyncResponse.CRMOutlookSyncMappings)
            {
                item.Action = tasksToSyncResponse.ActionsToSync.Where(a => a.ActionId == item.EntityID).FirstOrDefault();
            }
            return tasksToSyncResponse;
        }

        public GetTasksToSyncResponse GetModifiedActionsToSync(GetTasksToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the updated tasks to sync for user " + request.RequestedBy);
            GetTasksToSyncResponse taskToSyncResponse = new GetTasksToSyncResponse();

            IEnumerable<DA.Action> actionsToSync =
                actionRepository.GetModifiedActionsToSync(request.AccountId
                , (int)request.RequestedBy
                , request.MaxNumRecords
                , request.TimeStamp
                , request.FirstSync);

            IEnumerable<CRMOutlookSync> crmOutlookSyncViewModels = new List<CRMOutlookSync>();

            if (actionsToSync != null && actionsToSync.Any())
            {
                Logger.Current.Verbose("Syncing modified tasks:  " + actionsToSync.Select(c => c.Id).ToArray().ToString());
                crmOutlookSyncViewModels = contactRepository.GetEntityOutlookSyncMap(request.AccountId
                    , (int)request.RequestedBy
                    , request.MaxNumRecords
                    , request.TimeStamp
                    , actionsToSync.Select(c => c.Id));
            }
            taskToSyncResponse.ActionsToSync = Mapper.Map<IEnumerable<DA.Action>, IEnumerable<ActionViewModel>>(actionsToSync);
            var actionIds = actionsToSync.Select(c => c.Id).ToList();
            if (actionIds.Any())
            {
                IEnumerable<int> completedActions = getMultipleActionsCompletedStatuses(actionIds);
                foreach (var item in taskToSyncResponse.ActionsToSync)
                {
                    item.IsCompleted = completedActions.Contains(item.ActionId);
                }

            }
            taskToSyncResponse.CRMOutlookSyncMappings = Mapper.Map<IEnumerable<CRMOutlookSync>, IEnumerable<CRMOutlookSyncViewModel>>(crmOutlookSyncViewModels);
            foreach (var item in taskToSyncResponse.CRMOutlookSyncMappings)
            {
                item.Action = taskToSyncResponse.ActionsToSync.Where(a => a.ActionId == item.EntityID).FirstOrDefault();
            }
            return taskToSyncResponse;
        }

        IEnumerable<int> getMultipleActionsCompletedStatuses(IEnumerable<int> actionIds)
        {
            return actionRepository.GetCompletedActionsByIds(actionIds);
        }

        public GetTasksToSyncResponse GetDeletedTasksToSync(GetTasksToSyncRequest request)
        {
            Logger.Current.Verbose("Request for fetching the deleted tasks to sync for user " + request.RequestedBy);
            GetTasksToSyncResponse response = new GetTasksToSyncResponse();

            response.DeletedActions = actionRepository.GetDeletedTasksToSync(request.AccountId, (int)request.RequestedBy
                , request.MaxNumRecords, request.TimeStamp);

            return response;
        }

        public IList<int> GetAllAssignedUserIds(int actionId)
        {
            IList<int> ownerIds = null;
            ownerIds = actionRepository.GetAllOwnerIds(actionId);
            return ownerIds;
        }

        public BulkSendEmailResponse BulkMailSendForActionContacts(BulkSendEmailRequest request)
        {
            BulkSendEmailResponse response = new BulkSendEmailResponse();
            List<DA.ActionsMailOperation> actionMailOperations = new List<DA.ActionsMailOperation>();
            string userPrimaryEmail = userRepository.GetUserPrimaryEmail(request.RequestedBy.Value);
            int mailBulkID = actionRepository.InsertBulkMailOperationDetails(request.Subject, request.Body, 0, userPrimaryEmail);
            foreach (int actionId in request.ActionIds)
            {
                DA.ActionsMailOperation actionMailOperation = new DA.ActionsMailOperation();
                actionMailOperation.ActionID = actionId;
                actionMailOperation.IsScheduled = false;
                actionMailOperation.IsProcessed = (byte)ActionSendMailStatus.ReadyToProcess;
                actionMailOperation.MailBulkOperationID = mailBulkID;
                actionMailOperations.Add(actionMailOperation);

            }
            actionRepository.InsertActionsMailOperationDetails(actionMailOperations);
            return response;
        }

        private void InsertActionDetailsToNoteCategory(InsertActionRequest request)
        {
            NoteViewModel noteViewModel = new NoteViewModel();
            noteViewModel.AccountId = request.ActionViewModel.AccountId;
            noteViewModel.NoteDetails = request.ActionViewModel.ActionMessage;
            noteViewModel.NoteCategory = noteService.GetActionDetailsNoteCategoryID(request.ActionViewModel.AccountId.Value, (short)DropdownValueTypes.ActionDetails, (byte)DropdownFieldTypes.NoteCategory);
            noteViewModel.AddToContactSummary = true;
            noteViewModel.Contacts = request.ActionViewModel.Contacts.ToList();
            noteViewModel.CreatedBy = request.ActionViewModel.CreatedBy;
            noteViewModel.CreatedOn = DateTime.Now.ToUniversalTime();
            noteViewModel.SelectAll = request.ActionViewModel.SelectAll;
            noteViewModel.TagsList = new List<TagViewModel>() { };

            SaveNoteResponse response =  noteService.InsertNote(new SaveNoteRequest()
            {
                NoteViewModel = noteViewModel
            });

            if (request.ActionViewModel.SelectAll)
            {
                BulkOperations operationData = new BulkOperations()
                {
                    OperationID = response.NoteViewModel.NoteId,
                    OperationType = (int)BulkOperationTypes.Note,
                    SearchCriteria = HttpUtility.HtmlEncode(request.SelectAllSearchCriteria),
                    AdvancedSearchCriteria = request.AdvancedSearchCritieria,
                    SearchDefinitionID = null,
                    AccountID = request.AccountId,
                    UserID = (int)request.RequestedBy,
                    RoleID = request.RoleId,
                    AccountPrimaryEmail = request.AccountPrimaryEmail,
                    AccountDomain = request.AccountDomain,
                    ActionCompleted = request.ActionViewModel.IsCompleted
                };
                InsertBulkOperationRequest bulkOperationRequest = new InsertBulkOperationRequest()
                {
                    OperationData = operationData,
                    AccountId = request.AccountId,
                    RequestedBy = request.RequestedBy,
                    CreatedOn = DateTime.Now.ToUniversalTime().AddMinutes(1),
                    RoleId = request.RoleId,
                    DrillDownContactIds = request.DrillDownContactIds
                };

                accountService.InsertBulkOperation(bulkOperationRequest);
            }
        }
    }
}
