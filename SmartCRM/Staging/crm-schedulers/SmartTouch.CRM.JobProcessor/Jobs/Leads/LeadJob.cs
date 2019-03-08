using Quartz;
using SmartTouch.CRM.JobProcessor.QuartzScheduler.Scheduler;
using System;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.LeadAdapters;
using SmartTouch.CRM.LeadAdapters.Providers;
using SmartTouch.CRM.SearchEngine.Search;
using System.Linq;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Accounts;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.JobProcessor.Jobs.Leads
{
    public class LeadJob : BaseJob
    {
        private readonly ILeadAdaptersRepository‏ _leadAdaptersRepository‏;
        private readonly IAccountRepository _accountRepository;
        private readonly IServiceProviderRepository _serviceProviderRepository;
        private readonly IImportDataRepository _importDataRepository;
        private readonly ISearchService<Contact> _searchService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ICachingService _cahceService;
        private readonly ICommunicationService _communicationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailGunService _mailGunService;
        private readonly IContactService _contactService;

        public LeadJob(
            ILeadAdaptersRepository‏ leadAdaptersRepository‏,
            IAccountRepository accountRepository,
            IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository,
            ISearchService<Contact> searchService,
            ICustomFieldService customFieldService,
            ICachingService cahceService,
            ICommunicationService communicationService,
            IUnitOfWork unitOfWork,
            IMailGunService mailGunService,
            IContactService contactService)
        {
            _leadAdaptersRepository = leadAdaptersRepository;
            _accountRepository = accountRepository;
            _serviceProviderRepository = serviceProviderRepository;
            _importDataRepository = importDataRepository;
            _searchService = searchService;
            _customFieldService = customFieldService;
            _cahceService = cahceService;
            _communicationService = communicationService;
            _unitOfWork = unitOfWork;
            _mailGunService = mailGunService;
            _contactService = contactService;
        }

        protected override void ExecuteInternal(IJobExecutionContext context)
        {
            Log.Verbose("LeadAdapter process trigger");

            var allLeadAdapters = _leadAdaptersRepository‏.GetAllLeadAdapters();
            if (!allLeadAdapters.IsAny())
                return;

            Log.Informational("Total lead adapters count: " + allLeadAdapters.Count());
            foreach (var leadAdapter in allLeadAdapters)
            {
                ILeadAdapterProvider leadAdapterProvider;
                switch (leadAdapter.LeadAdapterTypeID)
                {
                    case LeadAdapterTypes.BDX:
                        leadAdapterProvider = new BDXLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.NHG:
                        leadAdapterProvider = new NHGLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.HotonHomes:
                        leadAdapterProvider = new HotonHomesLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.PROPLeads:
                        leadAdapterProvider = new PROPLeadsLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.Zillow:
                        leadAdapterProvider = new ZillowLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.NewHomeFeed:
                        leadAdapterProvider = new NewHomeFeedLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.Condo:
                        leadAdapterProvider = new CondoLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.PrivateCommunities:
                        leadAdapterProvider = new PrivateCommunitiesLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.IDX:
                        leadAdapterProvider = new IDXLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.BuzzBuzzHomes:
                        leadAdapterProvider = new BuzzBuzzHomesLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.HomeFinder:
                        leadAdapterProvider = new HomeFinderLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    case LeadAdapterTypes.Facebook:
                        leadAdapterProvider = new FacebookLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService, _accountRepository);
                        break;
                    case LeadAdapterTypes.BuildersUpdate:
                        leadAdapterProvider = new BuildersUpdateLeadAdapterProvider(leadAdapter.AccountID, leadAdapter.Id, _leadAdaptersRepository‏, _serviceProviderRepository,
                            _importDataRepository, _searchService, _unitOfWork, _customFieldService, _cahceService, _communicationService, _mailGunService, _contactService);
                        break;
                    default:
                        throw new InvalidOperationException($"LeadAdapterType {leadAdapter} not supported");
                }

                Log.Informational("Get leadadapter provider process and the lead adapter is: " + leadAdapter.LeadAdapterTypeID.ToString() + " and account id is: " + leadAdapter.AccountID);
                try
                {
                    leadAdapterProvider.Initialize();
                    leadAdapterProvider.Process();
                }
                catch (Exception ex)
                {
                    Log.Error($"Exception while processing lead adapter, Lead Adapter {leadAdapter.LeadAdapterTypeID}, Account {leadAdapter.AccountName}", ex);
                }
            }
        }
    }
}
