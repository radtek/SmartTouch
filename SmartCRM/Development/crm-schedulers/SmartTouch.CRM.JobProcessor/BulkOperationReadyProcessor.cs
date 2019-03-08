using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class BulkOperationReadyProcessor : CronJobProcessor
    {
        readonly IAccountService accountService;
        readonly IContactService contactService;
        readonly IAdvancedSearchService advancedSearchService;

        public BulkOperationReadyProcessor(CronJobDb cronJob, JobService jobService, string bulkOperationCacheName)
            : base(cronJob, jobService, bulkOperationCacheName)
        {
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.advancedSearchService = IoC.Container.GetInstance<IAdvancedSearchService>();
        }

        protected override void Execute()
        {
            GetBulkOperationDataResponse response = accountService.GetQueuedBulkOperationData(new GetBulkOperationDataRequest() { });
            try
            {
                Logger.Current.Informational("Entering into BulkOperation Ready processor");

                List<int> ids = null;

                while (response.BulkOperations != null)
                {
                    try
                    {
                        if (response.BulkOperations.SearchDefinitionID != null)
                        {
                            Logger.Current.Informational("BulkOperation Ready processor SearchDefinitionID" + response.BulkOperations.SearchDefinitionID);
                            GetSavedSearchContactIdsRequest advRequest = new GetSavedSearchContactIdsRequest()
                            {
                                SearchDefinitionId = (int)response.BulkOperations.SearchDefinitionID,
                                AccountId = response.BulkOperations.AccountID,
                                SearchCriteria = response.BulkOperations.SearchCriteria,
                                RoleId = response.BulkOperations.RoleID,
                                RequestedBy = response.BulkOperations.UserID
                            };
                            Logger.Current.Informational("For Getting SavedSearch Contacts By SearchdefinitionID In BulkOperation Ready processor");
                            ids = advancedSearchService.GetSavedSearchContactIds(advRequest).Result;
                        }
                        else if(!response.BulkContactIDs.IsAny() && (response.BulkOperations.SearchDefinitionID == null || response.BulkOperations.SearchDefinitionID == 0))
                        {
                            GetBulkContactsResponse contactsResponse = contactService.GetBulkContacts(new GetBulkContactsRequest() {
                                BulkOperations = response.BulkOperations,
                                ContactIds = new List<int>() { },
                                RoleId = response.BulkOperations.RoleID,
                                RequestedBy = response.BulkOperations.UserID

                            });
                            ids = contactsResponse.ContactIds.ToList();
                        }

                        if(ids.IsAny() && ids.Count > 0)
                        {
                            var bulkConactsData = ids.Select(i => new BulkContact() { BulkOperationID = response.BulkOperations.BulkOperationID, ContactID = i }).ToList();
                            contactService.InsertBulkOperationConacts(new BulkoperationConactsInsertionRequest() { ConactsData = bulkConactsData });
                            Logger.Current.Informational("BulkOperation Ready processor Conacts Inserted Successfully,BulkoperationId: "+ response.BulkOperations.BulkOperationID);
                        }
                        else
                        {
                            if(!response.BulkContactIDs.IsAny())
                            {
                                accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                                {
                                    BulkOperationId = response.BulkOperations.BulkOperationID,
                                    Status = BulkOperationStatus.Failed
                                });
                            }
                            
                        }

                        accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                        {
                            BulkOperationId = response.BulkOperations.BulkOperationID,
                            Status = BulkOperationStatus.Created
                        });
                        response = accountService.GetQueuedBulkOperationData(new GetBulkOperationDataRequest() { });

                    }
                    catch (Exception exe)
                    {
                        Logger.Current.Error("Error while BulkOperation Ready Processor Processing", exe);
                        accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                        {
                            BulkOperationId = response.BulkOperations.BulkOperationID,
                            Status = BulkOperationStatus.Failed
                        });
                        response = accountService.GetQueuedBulkOperationData(new GetBulkOperationDataRequest() { });
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while BulkOperation Ready Processor", ex);
                accountService.UpdateBulkOperationStatus(new UpdateBulkOperationStatusRequest()
                {
                    BulkOperationId = response.BulkOperations.BulkOperationID,
                    Status = BulkOperationStatus.Failed
                });
            }
        }
    }
}
