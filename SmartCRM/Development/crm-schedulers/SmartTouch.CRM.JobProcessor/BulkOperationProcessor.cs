using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using LandmarkIT.Enterprise.Extensions;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.Messaging.User;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class BulkOperationProcessor : CronJobProcessor
    {
        readonly IAccountService accountService;
        readonly IContactService contactService;
        readonly IAdvancedSearchService advancedSearchService;
        readonly IActionService actionService;
        readonly INoteService noteService;
        readonly ITourService tourService;
        readonly ITagService tagService;
        readonly ICommunicationService communicationService;
        readonly IUserService userService;

        public BulkOperationProcessor(CronJobDb cronJob, JobService jobService, string bulkOperationCacheName)
            : base(cronJob, jobService, bulkOperationCacheName)
        {
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.advancedSearchService = IoC.Container.GetInstance<IAdvancedSearchService>();
            this.actionService = IoC.Container.GetInstance<IActionService>();
            this.noteService = IoC.Container.GetInstance<INoteService>();
            this.tourService = IoC.Container.GetInstance<ITourService>();
            this.tagService = IoC.Container.GetInstance<ITagService>();
            this.communicationService = IoC.Container.GetInstance<ICommunicationService>();
            this.userService = IoC.Container.GetInstance<IUserService>();
        }

        protected override void Execute()
        {
            GetBulkOperationDataResponse response = accountService.GetBulkOperationData(new GetBulkOperationDataRequest() { });
            try
            {
                Logger.Current.Informational("Entering into BulkOperation processor");

                while (response.BulkOperations != null)
                {
                    Notification notification = new Notification();
                    notification.Time = DateTime.Now.ToUniversalTime();
                    notification.Status = NotificationStatus.New;
                    notification.EntityId = response.BulkOperations.OperationID;
                    notification.UserID = response.BulkOperations.UserID;
                    notification.ModuleID = (byte)AppModules.Contacts;

                    try
                    {

                        if (response.BulkContactIDs.Length > 0)
                        {
                            GetBulkContactsResponse contactsResponse = contactService.GetBulkContacts(new GetBulkContactsRequest() { BulkOperations = response.BulkOperations, ContactIds = response.BulkContactIDs.ToList() });
                            accountService.InsertBulkData(contactsResponse.ContactIds, response.BulkOperations.BulkOperationID);

                            #region Actions
                            if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Action)
                            {
                                Logger.Current.Informational("Performing bulk add action with operationId : " + response.BulkOperations.OperationID);
                                IEnumerable<int> contactIds = null;
                                IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
                                IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
                                var action = actionService.UpdateActionBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID,
                                    response.BulkOperations.UserID, response.BulkOperations.AccountPrimaryEmail, response.BulkOperations.AccountDomain, false, false, "", response.BulkOperations.ActionCompleted, contactIds, false, 0, emailGuids, textGuids);

                                notification.Subject = action.Details;
                                notification.Details = "Bulk Operation for adding action completed successfully";

                                notification.ModuleID = (byte)AppModules.ContactActions;
                            }
                            #endregion

                            #region Note
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Note)
                            {
                                Logger.Current.Verbose("Updating Note Bulk Data for operationid: " + response.BulkOperations.OperationID);
                                if (contactsResponse.ContactIds.IsAny())
                                {
                                    var note = noteService.UpdateNoteBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
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
                                Logger.Current.Informational("Performing bulk add tour with operationId : " + response.BulkOperations.OperationID);
                                IDictionary<int, Guid> emailGuids = new Dictionary<int, Guid>();
                                IDictionary<int, Guid> textGuids = new Dictionary<int, Guid>();
                                var tour = tourService.UpdateTourBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID,
                                    response.BulkOperations.UserID, response.BulkOperations.AccountPrimaryEmail, response.BulkOperations.AccountDomain, false, false, "", null, false, 0, emailGuids, textGuids);
                                notification.Subject = tour.TourDetails;
                                notification.Details = "Bulk Operation for adding tour completed successfully";
                                notification.ModuleID = (byte)AppModules.ContactTours;
                            }
                            #endregion

                            #region Tag
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Tag)
                            {
                                Logger.Current.Informational("Performing bulk add tag with operationId : " + response.BulkOperations.OperationID);
                                var tag = tagService.UpdateTagBulkData(response.BulkOperations.OperationID, response.BulkOperations.AccountID, response.BulkOperations.UserID);
                                notification.Subject = tag.TagName;
                                notification.Details = "Bulk Operation for adding tag completed successfully";
                                notification.ModuleID = (byte)AppModules.Contacts;
                                accountService.ScheduleAnalyticsRefresh(response.BulkOperations.OperationID, (byte)IndexType.Tags);
                            }
                            #endregion

                            #region ChangeOwner
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.ChangeOwner)
                            {
                                Logger.Current.Informational("Performing bulk change owner with operationId : " + response.BulkOperations.OperationID);
                                if (contactsResponse.ContactIds.IsAny())
                                {
                                    var owner = contactService.UpdateOwnerBulkData(response.BulkOperations.OperationID, response.BulkOperations.UserID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
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
                                Logger.Current.Informational("Performing bulk export with operationId : " + response.BulkOperations.OperationID);
                                IEnumerable<FieldViewModel> searchFields = null;
                                GetAdvanceSearchFieldsResponse filedsresponse = advancedSearchService.GetSearchFields(new GetAdvanceSearchFieldsRequest() { accountId = response.BulkOperations.AccountID, RoleId = response.BulkOperations.RoleID });
                                if (filedsresponse.FieldsViewModel != null)
                                {
                                    searchFields = ExcludeNonViewableFields(filedsresponse.FieldsViewModel);
                                }

                                Task.Run(() => contactService.UpdateBulkExcelExport(response.BulkOperations, contactsResponse.Contacts, searchFields));
                            }
                            #endregion

                            #region Delete
                            else if (response.BulkOperations.OperationType == (int)BulkOperationTypes.Delete)
                            {
                                Logger.Current.Informational("Performing bulk delete with operationId : " + response.BulkOperations.OperationID);
                                if (contactsResponse.ContactIds.IsAny())
                                {
                                    contactService.UpdateDeleteBulkData(response.BulkOperations.OperationID, response.BulkOperations.UserID, response.BulkOperations.AccountID, contactsResponse.ContactIds);
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

                            accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                            {
                                BulkOperationId = response.BulkOperations.BulkOperationID,
                                Status = BulkOperationStatus.Completed
                            });

                            if (response.BulkOperations.OperationType != (int)BulkOperationTypes.Export)
                                userService.AddNotification(new AddNotificationRequest() { Notification = notification });


                        }
                        else
                            accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                            {
                                BulkOperationId = response.BulkOperations.BulkOperationID,
                                Status = BulkOperationStatus.Failed
                            });
                        response = accountService.GetBulkOperationData(new GetBulkOperationDataRequest() { });

                    }
                    catch (Exception exe)
                    {
                        Logger.Current.Error("Error while BulkOperation", exe);
                        accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                        {
                            BulkOperationId = response.BulkOperations.BulkOperationID,
                            Status = BulkOperationStatus.Failed
                        });
                        response = accountService.GetBulkOperationData(new GetBulkOperationDataRequest() { });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while BulkOperation", ex);
                accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
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
