using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class SmartSearchProcessor : CronJobProcessor
    {
        readonly IContactRepository contactRepository;
        readonly IAdvancedSearchRepository advancedSearchRepository;
        readonly IAdvancedSearchService advancedSearchService;


        public SmartSearchProcessor(CronJobDb cronJob, JobService jobService, string savedsearchcontactprocessorcache)
            : base(cronJob, jobService, savedsearchcontactprocessorcache)
        {
            this.contactRepository = IoC.Container.GetInstance<IContactRepository>();
            this.advancedSearchRepository = IoC.Container.GetInstance<IAdvancedSearchRepository>();
            this.advancedSearchService = IoC.Container.GetInstance<IAdvancedSearchService>();
        }

        protected override void Execute()
        {
            Logger.Current.Informational("Entering into SmartSearchProcessor");
            try
            {
                List<int> contactIds = null;
                Dictionary<int, int> searchDefinitions = advancedSearchRepository.GetAllSearchDefinitionIsAndAccountIds();
                foreach (var data in searchDefinitions)
                {
                    try
                    {
                        GetSavedSearchContactIdsRequest advRequest = new GetSavedSearchContactIdsRequest()
                        {
                            SearchDefinitionId = data.Key,
                            AccountId = data.Value
                        };
                        var task = Task.Run(() => advancedSearchService.GetSavedSearchContactIds(advRequest));
                        contactIds = task.Result;
                        List<SmartSearchContact> savedSearchContacts = new List<SmartSearchContact>();
                        foreach (int contactId in contactIds)
                        {
                            SmartSearchContact savedSearchcontact = new SmartSearchContact();
                            savedSearchcontact.SearchDefinitionID = data.Key;
                            savedSearchcontact.ContactID = contactId;
                            savedSearchcontact.AccountID = data.Value;
                            savedSearchcontact.IsActive = true;
                            savedSearchContacts.Add(savedSearchcontact);

                        }
                        contactRepository.DeleteSavedSearchContactsBySearchDefinitionId(data.Key, data.Value);
                        contactRepository.InsertBulkSavedSearchesContacts(savedSearchContacts);
                        advancedSearchRepository.UpdateSmartSearchQueue(data.Key, data.Value, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.Current.Error(string.Format("Couldn't not fetch data for Smart Search {0} , Account {1}", data.Key, data.Value), ex);
                        advancedSearchRepository.UpdateSmartSearchQueue(data.Key, data.Value, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Error while Smart Search Contacts Processing" , ex);
            }
        }
    }
}
