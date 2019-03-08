using LandmarkIT.Enterprise.Utilities.ExceptionHandling;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.LeadAdapters;
using SmartTouch.CRM.LeadAdapters.Providers;
using SmartTouch.CRM.SearchEngine.Search;
using System.Linq;
using System;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using System.Collections.Generic;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.JobProcessor;
using LandmarkIT.Enterprise.CommunicationManager.Database;
using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SmartTouch.CRM.Domain.Accounts;
using LandmarkIT.Enterprise.Extensions;

namespace SmartTouch.CRM.JobProcessor
{
    public class LeadProcessor : CronJobProcessor
    {
        readonly ILeadAdaptersRepository‏ leadAdaptersRepository‏;
        readonly IAccountRepository accountRepository;
        readonly IContactRepository contactRepository;
        readonly IServiceProviderRepository serviceProviderRepository;
        readonly IImportDataRepository importDataRepository;
        readonly ISearchService<Contact> searchService;
        readonly ITagService tagService;
        readonly ICustomFieldService customFieldService;
        readonly ICachingService cahceService;
        readonly ICommunicationService communicationService;
        readonly IUnitOfWork unitOfWork;
        readonly IMailGunService mailGunService;
        readonly IContactService contactService;
        public LeadProcessor(CronJobDb cronJob, JobService jobService, string leadProcessorCacheName)
            : base(cronJob, jobService, leadProcessorCacheName)
        {
            leadAdaptersRepository‏ = IoC.Container.GetInstance<ILeadAdaptersRepository‏>();
            contactRepository = IoC.Container.GetInstance<IContactRepository>();
            serviceProviderRepository = IoC.Container.GetInstance<IServiceProviderRepository>();
            importDataRepository = IoC.Container.GetInstance<IImportDataRepository>();
            searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
            tagService = IoC.Container.GetInstance<ITagService>();
            customFieldService = IoC.Container.GetInstance<ICustomFieldService>();
            cahceService = IoC.Container.GetInstance<ICachingService>();
            communicationService = IoC.Container.GetInstance<ICommunicationService>();
            unitOfWork = IoC.Container.GetInstance<IUnitOfWork>();
            mailGunService = IoC.Container.GetInstance<IMailGunService>();
            contactService = IoC.Container.GetInstance<IContactService>();
            accountRepository = IoC.Container.GetInstance<IAccountRepository>();
        }

        protected override void Execute()
        {
            Logger.Current.Verbose("LeadAdapter process trigger");
            var leadAdapterProvider = default(ILeadAdapterProvider);

            var allLeadAdapters = leadAdaptersRepository‏.GetAllLeadAdapters();
            if (allLeadAdapters.IsAny())
            {
                Logger.Current.Informational("Total lead adapters count:   " + allLeadAdapters.Count());
                foreach (var item in allLeadAdapters)
                {
                    leadAdapterProvider = default(ILeadAdapterProvider);
                    switch (item.LeadAdapterTypeID)
                    {
                        case LeadAdapterTypes.BDX:
                            leadAdapterProvider = new BDXLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.NHG:
                            leadAdapterProvider = new NHGLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.HotonHomes:
                            leadAdapterProvider = new HotonHomesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.PROPLeads:
                            leadAdapterProvider = new PROPLeadsLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.Zillow:
                            leadAdapterProvider = new ZillowLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.NewHomeFeed:
                            leadAdapterProvider = new NewHomeFeedLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.Condo:
                            leadAdapterProvider = new CondoLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.PrivateCommunities:
                            leadAdapterProvider = new PrivateCommunitiesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.IDX:
                            leadAdapterProvider = new IDXLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.BuzzBuzzHomes:
                            leadAdapterProvider = new BuzzBuzzHomesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.HomeFinder:
                            leadAdapterProvider = new HomeFinderLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        case LeadAdapterTypes.Facebook:
                            leadAdapterProvider = new FacebookLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService, accountRepository);
                            break;
                        //case LeadAdapterTypes.Trulia:
                        //    leadAdapterProvider = new TruliaLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                        //    importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                        //    break;
                        case LeadAdapterTypes.BuildersUpdate:
                            leadAdapterProvider = new BuildersUpdateLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                            importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                            break;
                        default:
                            break;
                    }


                    if (leadAdapterProvider != default(ILeadAdapterProvider))
                    {
                        leadAdapterProvider.Initialize();
                        Logger.Current.Informational("Get leadadapter provider process and the lead adapter is: " + item.LeadAdapterTypeID.ToString() + " and account id is: " + item.AccountID);
                        try
                        {
                            leadAdapterProvider.Process();
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
    }
}
