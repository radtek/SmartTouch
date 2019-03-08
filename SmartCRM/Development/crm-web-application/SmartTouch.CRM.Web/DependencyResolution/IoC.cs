using Microsoft.AspNet.Identity;
using SimpleInjector;
using SimpleInjector.Extensions.LifetimeScoping;
using SimpleInjector.Integration.Web.Mvc;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.AccountSettings;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.ApplicationTour;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.MarketingMessageCenter;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Repository.Repositories;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System.Reflection;
using System.Web.Http;

namespace SmartTouch.CRM.Web
{
    public static class IoC
    {
        public static Container Container{get;set;}

        public static void Configure(Container container)
        {
            container.Register<IContactService, ContactService>();
            container.Register<IActionService, ActionService>();
            container.Register<IGeoService, GeoService>();
            container.Register<IObjectContextFactory, ObjectContextFactory>();
            container.Register<IContactRepository, ContactRepository>();
            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);
            container.Register<ITagRepository, TagRepository>();
            container.Register<IActionRepository, ActionRepository>();
            container.Register<ITagService, TagService>();
            container.Register<INoteService, NoteService>();
            container.Register<INoteRepository, NoteRepository>();
            container.Register<IAccountSettingsService, AccountSettingsService>();
            container.Register<IAccountSettingsRepository, AccountSettingsRepository>();
            container.Register<ICommunicationService, CommunicationService>();
            container.Register<IAccountService, AccountService>();
            container.Register<ICampaignService, CampaignService>();
            container.Register<IUserActivitiesRepository, UserActivitiesRepository>();
            container.Register<IRoleService, RoleService>();
            container.Register<IRoleRepository, RoleRepository>();
            container.Register<ISubscriptionRepository, SubscriptionRepository>();
            container.Register<IMailGunService, MailGunService>();
            container.Register<IMarketingMessageService, MarketingMessageService>();
            container.Register<IMarketingMessagesRopository, MarketingMessageRepository>();
            container.Register<IUserStore<IdentityUser>, UserStore>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IUserSettingsRepository, UserSettingsRepository>();
            container.Register<IAccountRepository, AccountRepository>();
            container.Register<ICommunicationRepository, CommunicationRepository>();
            container.Register<ICampaignRepository, CampaignRepository>();
            container.Register<ICampaignViewModel, CampaignViewModel>();
            container.Register<IAttachmentService, AttachmentService>();
            container.Register<IAttachmentRepository, AttachmentRepository>();
            container.Register<ICommunicationProviderService, CommunicationProviderService>();
            container.Register<IServiceProviderRepository, ServiceProviderRepository>();
            container.Register<ITourService, TourService>();
            container.Register<ITourRepository, TourRepository>();
            container.Register<ITourViewModel, TourViewModel>();
            container.Register<IUserService, UserService>();
            container.Register<IUrlService, UrlService>();
            container.Register<ILeadScoreRuleViewModel, LeadScoreRuleViewModel>();
            container.Register<ILeadScoreRuleRepository, LeadScoreRuleRepository>();
            container.Register<ILeadScoreRuleService, LeadScoreRuleService>();
            container.Register<ILeadScoreViewModel, LeadScoreViewModel>();
            container.Register<ILeadScoreRepository, LeadScoreRepository>();
            container.Register<ILeadScoreService, LeadScoreService>();
            container.Register<IDropdownRepository, DropdownRepository>();
            container.Register<IDropdownValuesService, DropdownValuesService>();
            container.Register<ILeadAdapterService, LeadAdapterService>();
            container.Register<ILeadAdaptersRepository, LeadAdaptersRepository>();
            container.Register<ILeadAdaptersJobLogsRepository, LeadAdapterJobLogsRepository>();
            container.Register<IFormService, FormService>();
            container.Register<IFormRepository, FormRepository>();
            container.Register<IFormSubmissionRepository, FormSubmissionRepository>();
            //NEXG-3014
            container.Register<IFindSpamService, FindSpamService>();
            container.Register<ICustomFieldRepository, CustomFieldRepository>();
            container.Register<ICustomFieldService, CustomFieldService>();
            container.Register<IImportDataRepository, ImportDataRepository>();
            container.Register<IOpportunitiesService, OpportunityService>();
            container.Register<IOpportunityRepository, OpportunityRepository>();
            container.Register<IAdvancedSearchService, AdvancedSearchService>();
            container.Register<IAdvancedSearchRepository, AdvancedSearchRepository>();
            container.Register<IWorkflowService, WorkflowService>();
            container.Register<IWorkflowRepository, WorkflowRepository>();
            container.Register<ICachingService, CachingService>();
            container.Register<IIndexingService, IndexingService>();
            container.Register<IContactEmailAuditRepository, ContactEmailAuditRepository>();
            container.Register<IContactTextMessageAuditRepository, ContactTextMessageAuditRepository>();
            container.Register<IContactRelationshipRepository, ContactRelationshipRepository>();
            container.Register<IContactRelationshipService, ContactRelationshipService>();
            container.RegisterConditional(typeof(ISearchService<>), typeof(SearchService<>), c => !c.Handled);
            container.Register<IReportService, ReportService>();
            container.Register<IReportRepository, ReportRepository>();
            container.Register<IImageService, ImageService>();
            container.Register<ISocialIntegrationService, SocialIntegrationService>();
            container.Register<IImageDomainService, ImageDomainService>();
            container.Register<IImageDomainRepository, ImageDomainRepository>();
            container.Register<IThirdPartyClientService, ThirdPartyClientService>();
            container.Register<IThirdPartyAuthenticationRepository, ThirdPartyAuthenticationRepository>();
            container.Register<ISeedEmailService, SeedEmailService>();
            container.Register<IWebAnalyticsProviderService, WebAnalyticsProviderService>();
            container.Register<IWebAnalyticsProviderRepository, WebAnalyticsProviderRepository>();
            container.Register<ISeedListRepository, SeedListRepository>();
            container.Register<IMessageService, MessageService>();
            container.Register<IMessageRepository, MessageRepository>();
            container.Register<ISubscriptionService, SubscriptionService>();
            container.Register<IApplicationTourService, ApplicationTourService>();
            container.Register<IApplicationTourDetailsRepository, ApplicationTourRepository>();
            container.Register<ISuppressionListService, SuppressionListService>();
            container.Register<ISuppressionListRepository, SuppressionListRepository>();
            container.RegisterInitializer<IdentityEmailService>(m => {
                m.ServiceProviderRepository = container.GetInstance<IServiceProviderRepository>();
            });
            container.RegisterMvcControllers(Assembly.GetExecutingAssembly());
            container.RegisterMvcIntegratedFilterProvider();
            /*Create a new SimpleInjectorDependencyResolver that wraps the, 
            container, and register that resolver in MVC.*/
            System.Web.Mvc.DependencyResolver.SetResolver(new SimpleInjectorDependencyResolver(container));
            IoC.Container = container;
        }
    }
}
