using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using LandmarkIT.Enterprise.Extensions;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.LeadAdapters;
using SmartTouch.CRM.LeadAdapters.Providers;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartTouch.CRM.JobProcessor
{
    public class ImportLeadProcessor : CronJobProcessor
    {
        readonly ILeadAdaptersRepository‏ leadAdaptersRepository‏;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IImportDataRepository importDataRepository;
        readonly ISearchService<Contact> searchService;
        readonly ICustomFieldService customFieldService;
        readonly ISuppressionListService suppressionListService;
        readonly ISuppressionListRepository suppressionListRepository;
        readonly ICachingService cahceService;
        readonly ICommunicationService communicationService;
        readonly IMailGunService mailGunService;
        readonly IContactService contactService;
        readonly IUnitOfWork unitofWork;
        readonly IDropdownValuesService dropdownValuesService;

        public ImportLeadProcessor(CronJobDb cronJob, JobService jobService, string importProcessorCacheName)
            : base(cronJob, jobService, importProcessorCacheName)
        {
            leadAdaptersRepository‏ = IoC.Container.GetInstance<ILeadAdaptersRepository‏>();
            serviceProviderRepository = IoC.Container.GetInstance<IServiceProviderRepository>();
            importDataRepository = IoC.Container.GetInstance<IImportDataRepository>();
            searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
            customFieldService = IoC.Container.GetInstance<ICustomFieldService>();
            suppressionListService = IoC.Container.GetInstance<ISuppressionListService>();
            suppressionListRepository = IoC.Container.GetInstance<ISuppressionListRepository>();
            cahceService = IoC.Container.GetInstance<ICachingService>();
            communicationService = IoC.Container.GetInstance<ICommunicationService>();
            mailGunService = IoC.Container.GetInstance<IMailGunService>();
            contactService = IoC.Container.GetInstance<IContactService>();
            unitofWork = IoC.Container.GetInstance<IUnitOfWork>();
            dropdownValuesService = IoC.Container.GetInstance<IDropdownValuesService>();
        }

        protected override void Execute()
        {
            try
            {
                Logger.Current.Informational("Entering into ExportLead processor");
                var LeadAdapters = leadAdaptersRepository‏.GetImportLeads();
                if(LeadAdapters.IsAny())
                {
                    var leadAdapterProvider = default(ILeadAdapterProvider);
                    foreach (var item in LeadAdapters)
                    {
                        leadAdapterProvider = new ExcelLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository, serviceProviderRepository,
                           importDataRepository, searchService, unitofWork, customFieldService, suppressionListService, cahceService, communicationService, mailGunService, contactService, dropdownValuesService);

                        if (leadAdapterProvider != default(ILeadAdapterProvider))
                        {
                            Logger.Current.Informational("Get leadadapter provider process and the lead adapter is: " + item.LeadAdapterTypeID.ToString() + " and account id is: " + item.AccountID);
                            try
                            {
                                leadAdapterProvider.Initialize();
                            }
                            catch (Exception ex)
                            {
                                var message = string.Format("Exception while processing lead adapter, Lead Adapter {0}, Account {1}", item.LeadAdapterTypeID, item.AccountName);
                                Logger.Current.Error(message, ex);
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                Logger.Current.Error("Error while Export Lead Processor" + ex);
            }
        }
    }
}
