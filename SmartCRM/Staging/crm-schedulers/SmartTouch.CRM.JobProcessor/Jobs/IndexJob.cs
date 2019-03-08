using System;
using System.Linq;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using Quartz;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;

namespace SmartTouch.CRM.JobProcessor.Jobs
{
    public class IndexJob : BaseJob
    {
        private readonly IAccountService _accountService;
        private readonly IContactService _contactService;
        private readonly ICampaignService _campaignService;
        private readonly IOpportunitiesService _opportunityService;
        private readonly IFormService _formService;
        private readonly ITagService _tagService;
        private readonly JobServiceConfiguration _jobConfig;

        public IndexJob(
            IAccountService accountService,
            IContactService contactService,
            ICampaignService campaignService,
            IOpportunitiesService opportunityService,
            IFormService formService,
            ITagService tagService,
            JobServiceConfiguration jobConfig)
        {
            _accountService = accountService;
            _contactService = contactService;
            _campaignService = campaignService;
            _opportunityService = opportunityService;
            _formService = formService;
            _tagService = tagService;
            _jobConfig = jobConfig;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            var indexingRequests = _accountService
                .GetIndexingData(new GetIndexingDataRequest { ChunkSize = _jobConfig.IndexChunkSize })
                .IndexingData
                .ToArray();

            if (!indexingRequests.IsAny())
                return;

            Log.Informational("Entering into Indexing processor");
            foreach (var indexingRequest in indexingRequests)
            {
                try
                {
                    Log.Informational("Application", indexingRequest?.EntityIDs.Any() ?? false ? "Count00==" + indexingRequest.EntityIDs.Count() + indexingRequest.IndexType : "Empty Index Count00=" + indexingRequest.IndexType);
                    var ids = indexingRequest.Ids.Select(s => s.Key);
                    if (ids.IsAny())
                    {
                        switch ((IndexType)indexingRequest.IndexType)
                        {
                            case IndexType.Contacts:
                                _contactService.ContactIndexing(new ContactIndexingRequest { ContactIds = indexingRequest.EntityIDs, Ids = indexingRequest.Ids });
                                break;
                            case IndexType.Campaigns:
                                _campaignService.CampaignIndexing(new CampaignIndexingRequest { CampaignIds = indexingRequest.EntityIDs });
                                break;
                            case IndexType.Opportunity:
                                _opportunityService.OpportunityIndexing(new OpportunityIndexingRequest { OpportunityIds = indexingRequest.EntityIDs });
                                break;
                            case IndexType.Forms:
                                _formService.FormIndexing(new FormIndexingRequest { FormIds = indexingRequest.EntityIDs });
                                break;
                            case IndexType.Tags:
                                _tagService.TagIndexing(new TagIndexingRequest { TagIds = indexingRequest.EntityIDs });
                                break;
                            case IndexType.Contacts_Delete:
                                _contactService.RemoveFromElastic(new RemoveFromElasticRequest { ContactIds = indexingRequest.EntityIDs });
                                break;
                        }

                        _accountService.UpdateIndexingStatus(new UpdateIndexingStatusRequest
                        {
                            ReferenceIds = indexingRequest.ReferenceIDs,
                            Status = IndexingStatus.Success
                        });
                    }
                }
                catch (Exception ex)
                {
                    if (indexingRequest?.EntityIDs?.IsAny() ?? false)
                    {
                        _accountService.UpdateIndexingStatus(new UpdateIndexingStatusRequest
                        {
                            ReferenceIds = indexingRequest.ReferenceIDs,
                            Status = IndexingStatus.Failed
                        });
                    }

                    ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                }
            }
        }
    }
}
