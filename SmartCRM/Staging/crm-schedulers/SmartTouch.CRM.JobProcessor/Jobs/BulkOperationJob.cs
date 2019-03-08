using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandmarkIT.Enterprise.Extensions;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs
{
    public class BulkOperationJob : BaseJob
    {
        private readonly IAccountService _accountService;
        private readonly IContactService _contactService;
        private readonly IAdvancedSearchService _advancedSearchService;
        private readonly IActionService _actionService;
        private readonly INoteService _noteService;
        private readonly ITourService _tourService;
        private readonly ITagService _tagService;
        private readonly IUserService _userService;

        public BulkOperationJob(
            IAccountService accountService,
            IContactService contactService,
            IAdvancedSearchService advancedSearchService,
            IActionService actionService,
            INoteService noteService,
            ITourService tourService,
            ITagService tagService,
            IUserService userService)
        {
            _accountService = accountService;
            _contactService = contactService;
            _advancedSearchService = advancedSearchService;
            _actionService = actionService;
            _noteService = noteService;
            _tourService = tourService;
            _tagService = tagService;
            _userService = userService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var response = _accountService.GetBulkOperationData(new GetBulkOperationDataRequest());
            try
            {
                Log.Informational("Entering into BulkOperation processor");

                List<int> ids = null;
                while (response.BulkOperations != null)
                {
                    var notification = new Notification
                    {
                        Time = DateTime.Now.ToUniversalTime(),
                        Status = NotificationStatus.New,
                        EntityId = response.BulkOperations.OperationID,
                        UserID = response.BulkOperations.UserID,
                        ModuleID = (byte) AppModules.Contacts
                    };

                    try
                    {
                        if (response.BulkOperations.SearchDefinitionID != null)
                        {
                            Log.Informational("Bulk SearchDefinitionID" + response.BulkOperations.SearchDefinitionID);
                            GetSavedSearchContactIdsRequest advRequest = new GetSavedSearchContactIdsRequest() { SearchDefinitionId = (int)response.BulkOperations.SearchDefinitionID, AccountId = response.BulkOperations.AccountID };
                            Log.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In BulkOperation");
                            ids = _advancedSearchService.GetSavedSearchContactIds(advRequest).Result;
                        }
                        else if (response.BulkContactIDs.Length > 0)
                        {
                            Log.Informational("Bulk contacts Length" + response.BulkContactIDs.ToList());
                            ids = response.BulkContactIDs.ToList();
                        }

                        if (ids.IsAny() || !string.IsNullOrEmpty(response.BulkOperations.SearchCriteria))
                        {
                            GetBulkContactsResponse contactsResponse = _contactService.GetBulkContacts(new GetBulkContactsRequest() { BulkOperations = response.BulkOperations, ContactIds = ids });
                            _accountService.InsertBulkData(contactsResponse.ContactIds, response.BulkOperations.BulkOperationID);

                            #region Actions
                            if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Action)
                            {
                                Log.Informational("Performing bulk add action with operationId : " + response.BulkOperations.OperationID);
                                IEnumerable<int> contactIds = null;
                                IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
                                IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
                                var action = _actionService.UpdateActionBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID,
                                    response.BulkOperations.UserID, response.BulkOperations.AccountPrimaryEmail, response.BulkOperations.AccountDomain, false, false, "", response.BulkOperations.ActionCompleted, contactIds, false, 0, emailGuids, textGuids);

                                notification.Subject = action.Details;
                                notification.Details = "Bulk Operation for adding action completed successfully";

                                notification.ModuleID = (byte)AppModules.ContactActions;
                            }
                            #endregion

                            #region Note
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Note)
                            {
                                Log.Verbose("Updating Note Bulk Data for operationid: " + response.BulkOperations.OperationID);
                                if (ids.IsAny())
                                {
                                    var note = _noteService.UpdateNoteBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
                                    notification.Subject = note.Details;
                                    notification.Details = "Bulk Operation for adding note completed successfully";
                                }
                                else
                                {
                                    notification.Subject = "Could not perform operation";
                                    notification.Details = "Opeartion could not be performed as there were no contacts found";
                                }
                                notification.ModuleID = (byte)AppModules.Contacts;
                            }
                            #endregion

                            #region Tour
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Tour)
                            {
                                Log.Informational("Performing bulk add tour with operationId : " + response.BulkOperations.OperationID);
                                IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
                                IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
                                var tour = _tourService.UpdateTourBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID,
                                    response.BulkOperations.UserID, response.BulkOperations.AccountPrimaryEmail, response.BulkOperations.AccountDomain, false, "", null, false, 0, emailGuids, textGuids);
                                notification.Subject = tour.TourDetails;
                                notification.Details = "Bulk Operation for adding tour completed successfully";
                                notification.ModuleID = (byte)AppModules.ContactTours;
                            }
                            #endregion

                            #region Tag
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Tag)
                            {
                                Log.Informational("Performing bulk add tag with operationId : " + response.BulkOperations.OperationID);
                                var tag = _tagService.UpdateTagBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID, response.BulkOperations.UserID);
                                notification.Subject = tag.TagName;
                                notification.Details = "Bulk Operation for adding tag completed successfully";
                                notification.ModuleID = (byte)AppModules.Contacts;
                                _accountService.ScheduleAnalyticsRefresh(response.BulkOperations.OperationID, (byte)IndexType.Tags);
                            }
                            #endregion

                            #region ChangeOwner
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.ChangeOwner)
                            {
                                Log.Informational("Performing bulk change owner with operationId : " + response.BulkOperations.OperationID);
                                if (ids.IsAny())
                                {
                                    var owner = _contactService.UpdateOwnerBulkData(response.BulkOperations.OperationID, response.BulkOperations.UserID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
                                    notification.Subject = "New owner: " + owner.FirstName + " " + owner.LastName;
                                    notification.Details = "Bulk Operation for changing owner completed successfully";
                                }
                                else
                                {
                                    notification.Subject = "Could not perform operation";
                                    notification.Details = "Opeartion could not be performed as there were no contacts found";
                                }
                                notification.ModuleID = (byte)AppModules.Contacts;
                            }
                            #endregion

                            #region Export
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Export)
                            {
                                Log.Informational("Performing bulk export with operationId : " + response.BulkOperations.OperationID);
                                IEnumerable<FieldViewModel> searchFields = null;
                                GetAdvanceSearchFieldsResponse filedsresponse = _advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest() { accountId = response.BulkOperations.AccountID, RoleId = response.BulkOperations.RoleID });
                                if (filedsresponse.FieldsViewModel != null)
                                {
                                    searchFields = ExcludeNonViewableFields(filedsresponse.FieldsViewModel);
                                }

                                Task.Run(() => _contactService.UpdateBulkExcelExport(response.BulkOperations, contactsResponse.Contacts, searchFields));
                            }
                            #endregion

                            #region Delete
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Delete)
                            {
                                Log.Informational("Performing bulk delete with operationId : " + response.BulkOperations.OperationID);
                                if (ids.IsAny())
                                {
                                    _contactService.UpdateDeleteBulkData(response.BulkOperations.OperationID, response.BulkOperations.UserID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
                                    notification.Subject = "Bulk Operation for deleting contacts completed successfully";
                                    notification.Details = "Bulk Operation for deleting contacts completed successfully";
                                }
                                else
                                {
                                    notification.Subject = "Could not perform operation";
                                    notification.Details = "Opeartion could not be performed as there were no contacts found";
                                }
                                notification.ModuleID = (byte)AppModules.Contacts;
                            }
                            #endregion

                            _accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                            {
                                BulkOperationId = response.BulkOperations.BulkOperationID,
                                Status = BulkOperationStatus.Completed
                            });

                            if (response.BulkOperations.OperationType != (int)BulkOperationTypes.Export)
                                _userService.AddNotification(new AddNotificationRequest() { Notification = notification });

                        }
                        else
                            _accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                            {
                                BulkOperationId = response.BulkOperations.BulkOperationID,
                                Status = BulkOperationStatus.Failed
                            });

                        //TODO: Remove this, will cause job run too much time 
                        response = _accountService.GetBulkOperationData(new GetBulkOperationDataRequest() { });

                    }
                    catch (Exception exe)
                    {
                        Log.Error("Error while BulkOperation", exe);
                        _accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                        {
                            BulkOperationId = response.BulkOperations.BulkOperationID,
                            Status = BulkOperationStatus.Failed
                        });
                        //TODO: Remove this, will cause job run too much time 
                        response = _accountService.GetBulkOperationData(new GetBulkOperationDataRequest() { });
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while BulkOperation", ex);
                _accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                {
                    BulkOperationId = response.BulkOperations.BulkOperationID,
                    Status = BulkOperationStatus.Failed
                });
            }
        }

        private IEnumerable<FieldViewModel> ExcludeNonViewableFields(IEnumerable<FieldViewModel> fields)
        {
            IEnumerable<FieldViewModel> fieldModel = new List<FieldViewModel>();
            if (fields != null && fields.Any())
                fieldModel = fields.Where(w => w.FieldId != 42 && w.FieldId != 45 && w.FieldId != 46 && w.FieldId != 47 && w.FieldId != 48 && w.FieldId != 49
                    && w.FieldId != 56 && w.FieldId != 57 && w.FieldId != 58 && w.FieldId != 60);
            return fieldModel;
        }
    }
}
