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
    public class BuildersUpdateLeadAdapterProvider : BaseLeadAdapterProvider
    {
        IMailGunService mailGunService;
        ISearchService<Contact> searchService;
        IImportDataRepository importDataRepository;
        
        public BuildersUpdateLeadAdapterProvider(int accountId, int leadAdapterAndAccountMapID, ILeadAdaptersRepository leadAdaptersRepository, IServiceProviderRepository serviceProviderRepository,
            IImportDataRepository importDataRepository, ISearchService<Contact> searchService, IUnitOfWork unitOfWork,
            ICustomFieldService customFieldService, ICachingService cacheService, ICommunicationService communicationService, IMailGunService mailGunService, IContactService contactService)
            : base(accountId, leadAdapterAndAccountMapID, LeadAdapterTypes.BuildersUpdate, leadAdaptersRepository, importDataRepository, searchService, unitOfWork,
            customFieldService, cacheService, serviceProviderRepository, mailGunService, contactService)
        {
            Logger.Current.Verbose("Enter into BuildersUpdateLeadAdapterProvider");
            this.mailGunService = mailGunService;
            this.searchService = searchService;
            this.importDataRepository = importDataRepository;
        }

        public override Dictionary<string, string> GetFieldMappings()
        {
            Dictionary<string, string> fieldMappings = new Dictionary<string, string>();
            fieldMappings.Add("BuilderNumber", "BuilderId");
            fieldMappings.Add("CommunityNumber", "CommunityNumber");
            fieldMappings.Add("FirstName", "AgentFirstName");
            fieldMappings.Add("LastName", "AgentLastName");
            fieldMappings.Add("Email", "AgentEmail");
            fieldMappings.Add("Company", "AgentCompanyName");
            fieldMappings.Add("PhoneNumber", "AgentPhoneNumber");
            fieldMappings.Add("StreetAddress", "AgentAddress");
            fieldMappings.Add("City", "AgentCity");
            fieldMappings.Add("State", "AgentState");
            fieldMappings.Add("PostalCode", "AgentZip");
            fieldMappings.Add("Country", "AgentCountry");

            return fieldMappings;
        }

        public override string GetRootNode()
        {
            return "LeadType";
        }
    }
}
