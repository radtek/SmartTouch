using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using System.Collections.Generic;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class TruliaLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        LeadAdapterTypes leadAdapterType;
        public TruliaLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService) :
            base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.Trulia, leadAdaptersRepository, importDataRepository, searchService, unitOfWork, customFieldService, cacheService, serviceProviderRepository,
            mailGunService, contactService)
        {
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.importDataRepository = importDataRepository;
            this.leadAdapterType = LeadAdapterTypes.Trulia;
        }

        public override Dictionary<string, string> GetFieldMappings()
        {
            Dictionary<string, string> mappings = new Dictionary<string, string>();
            mappings.Add("FirstName", "FirstName");
            mappings.Add("LastName", "LastName");
            mappings.Add("Email", "Email");
            mappings.Add("PhoneNumber", "Phone");
            mappings.Add("BuilderNumber", "BuilderNumber");
            mappings.Add("CommunityNumber", "CommunityNumber");
            return mappings;
        }

        public override string GetRootNode()
        {
            return "Lead";
        }
    }
}
