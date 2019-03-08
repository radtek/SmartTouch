using System;
using System.Collections.Generic;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Search;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs
{
    public class SmartSearchJob : BaseJob
    {
        private readonly IContactRepository _contactRepository;
        private readonly IAdvancedSearchRepository _advancedSearchRepository;
        private readonly IAdvancedSearchService _advancedSearchService;

        public SmartSearchJob(
            IContactRepository contactRepository,
            IAdvancedSearchRepository advancedSearchRepository,
            IAdvancedSearchService advancedSearchService)
        {
            _contactRepository = contactRepository;
            _advancedSearchRepository = advancedSearchRepository;
            _advancedSearchService = advancedSearchService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Informational("Entering into SmartSearchProcessor");
            var searchDefinitions = _advancedSearchRepository.GetAllSearchDefinitionIsAndAccountIds();
            foreach (var seachDefinition in searchDefinitions)
            {
                try
                {
                    var advRequest = new GetSavedSearchContactIdsRequest
                    {
                        SearchDefinitionId = seachDefinition.Key,
                        AccountId = seachDefinition.Value
                    };
                    var contactIds = _advancedSearchService.GetSavedSearchContactIds(advRequest).Result;
                    var savedSearchContacts = new List<SmartSearchContact>();
                    foreach (var contactId in contactIds)
                    {
                        var savedSearchcontact = new SmartSearchContact
                        {
                            SearchDefinitionID = seachDefinition.Key,
                            ContactID = contactId,
                            AccountID = seachDefinition.Value,
                            IsActive = true
                        };
                        savedSearchContacts.Add(savedSearchcontact);
                    }
                    _contactRepository.DeleteSavedSearchContactsBySearchDefinitionId(seachDefinition.Key, seachDefinition.Value);
                    _contactRepository.InsertBulkSavedSearchesContacts(savedSearchContacts);
                    _advancedSearchRepository.UpdateSmartSearchQueue(seachDefinition.Key, seachDefinition.Value, true);
                }
                catch (Exception ex)
                {
                    Log.Error($"Couldn't not fetch data for Smart Search {seachDefinition.Key} , Account {seachDefinition.Value}", ex);
                    _advancedSearchRepository.UpdateSmartSearchQueue(seachDefinition.Key, seachDefinition.Value, true);
                }
            }
        }
    }
}
