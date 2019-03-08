using System.Collections.Generic;
using LandmarkIT.Enterprise.Utilities.Logging;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Entities;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.Domain.Communication;

namespace SmartTouch.CRM.LeadAdapters.Providers
{
    public class BDXLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        IContactService contactService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        ILeadAdaptersRepository leadAdaptersRepository;
        public BDXLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.BDX, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
            customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            Logger.Current.Verbose("Enter into BDXLeadAdapterProvider");
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.contactService = contactService;
            this.importDataRepository = importDataRepository;
            this.leadAdaptersRepository = leadAdaptersRepository;
        }

        public override Dictionary<string, string> GetFieldMappings()
        {
            var mappings = new Dictionary<string, string>();
            mappings.Add("BuilderNumber", "BuilderNumber");
            mappings.Add("BuilderName", "BuilderName");
            mappings.Add("CommunityNumber", "CommunityNumber");
            mappings.Add("CommunityName", "CommunityName");
            mappings.Add("StateName", "StateName");
            mappings.Add("MarketName", "MarketName");
            mappings.Add("Comments", "Comments");
            mappings.Add("FirstName", "FirstName");
            mappings.Add("LastName", "LastName"); 
            mappings.Add("Phone", "Phone");
            mappings.Add("PhoneNumber", "PhoneNumber");
            mappings.Add("PostalCode", "PostalCode");
            mappings.Add("Email", "Email");
            mappings.Add("Country", "Country");
            mappings.Add("LeadType", "LeadType");
            mappings.Add("Source", "Source");
            return mappings;
        }

        public override string GetRootNode()
        {
            return "Lead";
        }
    }
}
