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

namespace SmartTouch.CRM.LeadAdapterEngine
{
    public class LeadProcessor
    {
        private  bool isRunning = default(bool);
        static IDictionary<int, ILeadAdapterProvider> leadAdapters = new Dictionary<int, ILeadAdapterProvider>();

        public void Trigger(Object stateInfo)
        {
            try
            {
                Logger.Current.Verbose("LeadAdapter process trigger");
                if (isRunning) return;
                isRunning = true;
                var leadAdapterProvider = default(ILeadAdapterProvider);
                var leadAdaptersRepository‏ = IoC.Container.GetInstance<ILeadAdaptersRepository‏>();
                var serviceProviderRepository = IoC.Container.GetInstance<IServiceProviderRepository>();
                var importDataRepository = IoC.Container.GetInstance<IImportDataRepository>();
                var searchService = IoC.Container.GetInstance<ISearchService<Contact>>();
                var customFieldService = IoC.Container.GetInstance<ICustomFieldService>();
                var suppressionListService = IoC.Container.GetInstance<ISuppressionListService>();
                var cahceService = IoC.Container.GetInstance<ICachingService>();
                var communicationService = IoC.Container.GetInstance<ICommunicationService>();
                var unitOfWork = IoC.Container.GetInstance<IUnitOfWork>();
                var mailGunService = IoC.Container.GetInstance<IMailGunService>();
                var contactService = IoC.Container.GetInstance<IContactService>();
                var dropdownValueService = IoC.Container.GetInstance<IDropdownValuesService>();
                var allLeadAdapters = leadAdaptersRepository‏.GetAllLeadAdapters();
                Logger.Current.Informational("Total lead adapters count:   " + allLeadAdapters.Count());
                foreach (var item in allLeadAdapters)
                {
                    leadAdapterProvider = default(ILeadAdapterProvider);

                    switch (item.LeadAdapterTypeID)
                    {
                        case LeadAdapterTypes.BDX:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new BDXLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.NHG:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new NHGLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.HotonHomes:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new HotonHomesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.PROPLeads:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new PROPLeadsLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.Zillow:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new ZillowLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.NewHomeFeed:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new NewHomeFeedLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.Condo:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new CondoLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.Import:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new ExcelLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, suppressionListService, cahceService, communicationService, mailGunService, contactService, dropdownValueService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.PrivateCommunities:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new PrivateCommunitiesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.IDX:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new IDXLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                                       importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);                             
                                leadAdapterProvider.Initialize();
                                leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        case LeadAdapterTypes.BuzzBuzzHomes:
                            if (!leadAdapters.ContainsKey(item.Id))
                            {
                                leadAdapterProvider = new BuzzBuzzHomesLeadAdapterProvider(item.AccountID, item.Id, leadAdaptersRepository‏, serviceProviderRepository,
                                                        importDataRepository, searchService, unitOfWork, customFieldService, cahceService, communicationService, mailGunService, contactService);  

                               leadAdapterProvider.Initialize();
                               leadAdapters.Add(item.Id, leadAdapterProvider);
                            }
                            break;
                        default:
                            break;
                    }

                    leadAdapterProvider = leadAdapters[item.Id];
                    if (leadAdapterProvider != default(ILeadAdapterProvider))
                    {
                        Logger.Current.Informational("Get leadadapter provider process and the lead adapter is: " + item.LeadAdapterTypeID.ToString() + " and account id is: " + item.AccountID);
                        leadAdapterProvider.Process();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Current.Error("Exception occured while providers process", ex);
                ExceptionHandler.Current.HandleException(ex, DefaultExceptionPolicies.LOG_ONLY_POLICY);
            }
            finally
            {
                isRunning = false;
            }
        }
    }
}
