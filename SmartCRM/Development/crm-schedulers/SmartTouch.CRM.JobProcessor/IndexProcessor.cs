using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.Messaging.Accounts;
using SmartTouch.CRM.ApplicationServices.Messaging.Campaigns;
using SmartTouch.CRM.ApplicationServices.Messaging.Contacts;
using SmartTouch.CRM.ApplicationServices.Messaging.Forms;
using SmartTouch.CRM.ApplicationServices.Messaging.Opportunity;
using SmartTouch.CRM.ApplicationServices.Messaging.Tags;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ValueObjects;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.SearchEngine.Indexing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTouch.CRM.JobProcessor
{
    public class IndexProcessor : CronJobProcessor
    {
        readonly IAccountService accountService;
        readonly IContactRepository contactRepository;
        readonly IIndexingService indexingService;
        readonly IContactService contactService;
        readonly ICampaignService campaignService;
        readonly IOpportunitiesService opportunityService;
        readonly IFormService formService;
        readonly ITagService tagService;

        public IndexProcessor(CronJobDb cronJob, JobService jobService, string indexProcessorCacheName)
            : base(cronJob, jobService, indexProcessorCacheName)
        {
            this.accountService = IoC.Container.GetInstance<IAccountService>();
            this.contactRepository = IoC.Container.GetInstance<IContactRepository>();
            this.indexingService = IoC.Container.GetInstance<IIndexingService>();
            this.contactService = IoC.Container.GetInstance<IContactService>();
            this.campaignService = IoC.Container.GetInstance<ICampaignService>();
            this.opportunityService = IoC.Container.GetInstance<IOpportunitiesService>();
            this.formService = IoC.Container.GetInstance<IFormService>();
            this.tagService = IoC.Container.GetInstance<ITagService>();
        }
        protected override void Execute()
        {
            var chunkSize = int.Parse(ConfigurationManager.AppSettings["INDEX_CHUNK_SIZE"]);
            GetIndexingDataResponce responce = accountService.GetIndexingData(new GetIndexingDataRequest() { ChunkSize = chunkSize });

            while (true && responce.IndexingData.IsAny())
            {
                try
                {
                    Logger.Current.Informational("Entering into Indexing processor");
                    foreach (var data in responce.IndexingData)
                    {
                        try
                        {
                            Logger.Current.Informational("Application", data.EntityIDs != null && data.EntityIDs.Any() ? "Count00==" + data.EntityIDs.Count().ToString() + data.IndexType : "Empty Index Count00=" + data.IndexType.ToString());
                            IEnumerable<int> Ids = data.Ids.Select(s => s.Key);
                            if (Ids != null && Ids.IsAny())
                            {
                                switch ((IndexType)data.IndexType)
                                {
                                    case IndexType.Contacts:
                                        contactService.ContactIndexing(new ContactIndexingRequest() { ContactIds = data.EntityIDs, Ids = data.Ids });
                                        break;
                                    case IndexType.Campaigns:
                                        campaignService.CampaignIndexing(new CampaignIndexingRequest() { CampaignIds = data.EntityIDs });
                                        break;
                                    case IndexType.Opportunity:
                                        opportunityService.OpportunityIndexing(new OpportunityIndexingRequest() { OpportunityIds = data.EntityIDs });
                                        break;
                                    case IndexType.Forms:
                                        formService.FormIndexing(new FormIndexingRequest() { FormIds = data.EntityIDs });
                                        break;
                                    case IndexType.Tags:
                                        tagService.TagIndexing(new TagIndexingRequest() { TagIds = data.EntityIDs });
                                        break;
                                    case IndexType.Contacts_Delete:
                                        contactService.RemoveFromElastic(new RemoveFromElasticRequest() { ContactIds = data.EntityIDs });
                                        break;
                                    default:
                                        break;
                                }
                                
                                accountService.UpdateIndexingStatus(new UpdateIndexingStatusRequest()
                                {
                                    ReferenceIds = data.ReferenceIDs,
                                    Status = 2
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            ExceptionHandler.Current.HandleException(e, DefaultExceptionPolicies.LOG_ONLY_POLICY);

                            if (data.EntityIDs != null && data.EntityIDs.IsAny())
                            {
                                accountService.UpdateIndexingStatus(new UpdateIndexingStatusRequest()
                                {
                                    ReferenceIds = data.ReferenceIDs,
                                    Status = 3
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                }

                responce = accountService.GetIndexingData(new GetIndexingDataRequest() { ChunkSize = chunkSize });
                if (responce.IndexingData.IsAny() == false) break;
            }

        }
    }
}
