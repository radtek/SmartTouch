using System;
using Quartz;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using SmartTouch.CRM.LeadAdapters;
using SmartTouch.CRM.LeadAdapters.Providers;
using SmartTouch.CRM.SearchEngine.Search;
using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using SmartTouch.CRM.Domain.LeadAdapters;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.JobProcessor.Jobs
{
    public class ImportLeadJob : BaseJob
    {
        private readonly ILeadAdaptersRepository‏ _leadAdaptersRepository‏;
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly IImportDataRepository _importDataRepository;
        private readonly ISearchService<Contact> _searchService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ISuppressionListService _suppressionListService;
        private readonly ICachingService _cacheService;
        private readonly ICommunicationService _communicationService;
        private readonly IMailGunService _mailGunService;
        private readonly IContactService _contactService;
        private readonly IUnitOfWork _unitofWork;
        private readonly IDropdownValuesService _dropdownValuesService;

        public ImportLeadJob(
            ILeadAdaptersRepository‏ leadAdaptersRepository,
            IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository,
            ISearchService<Contact> searchService,
            ICustomFieldService customFieldService,
            ISuppressionListService suppressionListService,
            ICommunicationService communicationService,
            IMailGunService mailGunService,
            IContactService contactService,
            IUnitOfWork unitofWork,
            ICachingService cacheService,
            IDropdownValuesService dropdownValuesService)
        {
            _leadAdaptersRepository‏ = leadAdaptersRepository;
            _serviceProviderRepository = serviceProviderRepository;
            _importDataRepository = importDataRepository;
            _searchService = searchService;
            _customFieldService = customFieldService;
            _suppressionListService = suppressionListService;
            _cacheService = cacheService;
            _communicationService = communicationService;
            _mailGunService = mailGunService;
            _contactService = contactService;
            _unitofWork = unitofWork;
            _dropdownValuesService = dropdownValuesService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Informational("Entering into ExportLead processor");
            var allLeadAdapters = _leadAdaptersRepository‏.GetAllLeadAdapters();
            if (!allLeadAdapters.IsAny())
                return;

            foreach (var leadAdapter in allLeadAdapters)
            {
                var leadAdapterProvider = new ExcelLeadAdapterProvider(
                    leadAdapter.AccountID,
                    leadAdapter.Id,
                    _leadAdaptersRepository,
                    _serviceProviderRepository,
                    _importDataRepository,
                    _searchService,
                    _unitofWork,
                    _customFieldService,
                    _suppressionListService,
                    _cacheService,
                    _communicationService,
                    _mailGunService,
                    _contactService,
                    _dropdownValuesService);

                Log.Informational("Get leadadapter provider process and the lead adapter is: " + leadAdapter.LeadAdapterTypeID.ToString() + " and account id is: " + leadAdapter.AccountID);
                try
                {
                    leadAdapterProvider.Initialize();
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception while processing lead adapter, Lead Adapter {leadAdapter.LeadAdapterTypeID}, Account {leadAdapter.AccountName}", ex);
                    ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
                }
            }
        }
    }
}
