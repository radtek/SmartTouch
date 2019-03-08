using Microsoft.AspNet.Identity;
using SimpleInjector;
using SimpleInjector.Advanced;
using SimpleInjector.Extensions.LifetimeScoping;
using SimpleInjector.Integration.Web;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.Domain.Enterprises;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImageDomains;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Notifications;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.SeedList;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.ThirdPartyAuthentication;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Identity;
using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Repository.Repositories;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using System;
using System.Web.Http;

namespace SmartTouch.CRM.WebService.DependencyResolution
{
    /// <summary>
    /// for registering in Ioc
    /// </summary>
    public static class IoC
    {
        /// <summary>
        /// creating a reference variable for Container
        /// </summary>
        public static Container Container { get; set; }

        /// <summary>
        /// For Configuration
        /// </summary>
        /// <param name="container">container</param>
        public static void Configure(Container container)
        {
            container.Options.DefaultScopedLifestyle = new WebRequestLifestyle();
            container.Options.LifestyleSelectionBehavior = new SingletonLifestyleSelectionBehavior();
            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);
            
            container.Register<IUserStore<IdentityUser>, UserStore>();
            container.Register<IActionService, ActionService>();
            container.Register<IActionRepository, ActionRepository>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IUserService, UserService>();
            container.Register<IContactService, ContactService>();
            container.Register<ITagService, TagService>();
            container.Register<IAccountService, AccountService>();
            container.Register<ITourService, TourService>();
            container.Register<INoteService, NoteService>();
            container.Register<IObjectContextFactory, ObjectContextFactory>();
            container.Register<IContactRepository, ContactRepository>();
            container.Register<ITagRepository, TagRepository>();
            container.Register<IAccountRepository, AccountRepository>();
            container.Register<ITourRepository, TourRepository>();
            container.Register<INoteRepository, NoteRepository>();
            container.Register<IRoleService, RoleService>();
            container.Register<IRoleRepository, RoleRepository>();
            container.Register<ISubscriptionRepository, SubscriptionRepository>();
            container.Register<IReportService, ReportService>();
            container.Register<IReportRepository, ReportRepository>();
            container.Register<IImageDomainService, ImageDomainService>();
            container.Register<IImageDomainRepository, ImageDomainRepository>();
            container.Register<ITagViewModel, TagViewModel>();
            container.Register<IAccountViewModel, AccountViewModel>();
            container.Register<ITourViewModel, TourViewModel>();
            container.Register<ICommunicationTrackerViewModel, CommunicationTrackerViewModel>();
            container.Register<ICommunicationRepository, CommunicationRepository>();
            container.Register<ICommunicationService, CommunicationService>();

            container.Register<IAttachmentViewModel, AttachmentViewModel>();
            container.Register<IAttachmentRepository, AttachmentRepository>();
            container.Register<IAttachmentService, AttachmentService>();
            container.Register<IUserSettingsRepository, UserSettingsRepository>();
            container.Register<IUserActivitiesRepository, UserActivitiesRepository>();

            container.Register<ICampaignViewModel, CampaignViewModel>();
            container.Register<ICampaignRepository, CampaignRepository>();
            container.Register<ICampaignService, CampaignService>();

            container.Register<IFormService, FormService>();
            container.Register<IFormRepository, FormRepository>();
            container.Register<IFormSubmissionRepository, FormSubmissionRepository>();

            container.Register<ICustomFieldRepository, CustomFieldRepository>();
            container.Register<ICustomFieldService, CustomFieldService>();

            container.Register<IServiceProviderViewModel, ServiceProviderViewModel>();
            container.Register<ICommunicationProviderService, CommunicationProviderService>();
            container.Register<IServiceProviderRepository, ServiceProviderRepository>();
            container.Register<IUrlService, UrlService>();
          
            container.Register<ILeadAdapterService, LeadAdapterService>();
            container.Register<ILeadAdaptersRepository, LeadAdaptersRepository>();
            container.Register<ILeadAdaptersJobLogsRepository, LeadAdapterJobLogsRepository>();
            container.Register<ILeadScoreViewModel, LeadScoreViewModel>();
            container.Register<ILeadScoreService, LeadScoreService>();
            container.Register<ILeadScoreRuleService, LeadScoreRuleService>();
            container.Register<ILeadScoreRepository, LeadScoreRepository>();
            container.Register<ILeadScoreRuleViewModel, LeadScoreRuleViewModel>();
            container.Register<ILeadScoreRuleRepository, LeadScoreRuleRepository>();

            container.Register<IDropdownRepository, DropdownRepository>();
            container.Register<IDropdownValuesService, DropdownValuesService>();
            container.Register<IImportDataRepository, ImportDataRepository>();
            container.Register<IContactRelationshipRepository, ContactRelationshipRepository>();

            container.Register<IContactEmailAuditRepository,ContactEmailAuditRepository>();
            container.Register<IContactTextMessageAuditRepository, ContactTextMessageAuditRepository>();

            container.Register<IContactRelationshipService, ContactRelationshipService>();
            container.Register<IOpportunitiesService, OpportunityService>();
            container.Register<IOpportunityRepository, OpportunityRepository>();
            container.Register<IAdvancedSearchService, AdvancedSearchService>();
            container.Register<IAdvancedSearchRepository, AdvancedSearchRepository>();

            container.Register<IWorkflowService, WorkflowService>();
            container.Register<IWorkflowRepository, WorkflowRepository>();

            container.Register<ICachingService, CachingService>();
            container.Register<IMessageQueuingService, MessageQueuingService>();
            container.Register<IIndexingService, IndexingService>();
            container.Register<IGeoService, GeoService>();
            container.RegisterConditional(typeof(ISearchService<>), typeof(SearchService<>), c=> !c.Handled);
            container.Register<IPublishSubscribeService, PublishSubscribeService>();
            container.Register<IWebAnalyticsProviderService, WebAnalyticsProviderService>();
            container.Register<IWebAnalyticsProviderRepository, WebAnalyticsProviderRepository>();
            container.Register<IImageService, ImageService>();
            container.Register<IThirdPartyAuthenticationRepository, ThirdPartyAuthenticationRepository>();
            container.Register<ISeedEmailService, SeedEmailService>();
            container.Register<ISeedListRepository, SeedListRepository>();
            container.Register<IMailGunService, MailGunService>();
            container.Register<IMessageService, MessageService>();
            container.Register<IMessageRepository, MessageRepository>();
            container.Register<IEnterpriseService, EnterpriseService>();
            container.Register<IEnterpriseServicesRepository, EnterpriseServicesRepository>();
            container.Register<IFindSpamService, FindSpamService>();
            container.Register<IThirdPartyClientService, ThirdPartyClientService>();
            container.Register<IPushNotificationService, PushNotificationService>();
            container.Register<IPushNotificationsRepository, PushNotificationRepository>();
            var services = GlobalConfiguration.Configuration.Services;
            var controllerTypes = services.GetHttpControllerTypeResolver()
                .GetControllerTypes(services.GetAssembliesResolver());

            foreach (var controllerType in controllerTypes)
            {
                container.Register(controllerType);
            }

            container.Verify();
            GlobalConfiguration.Configuration.DependencyResolver =
                    new SimpleInjectorWebApiDependencyResolver(container);
            IoC.Container = container;
        }
    }

    /// <summary>
    /// creating class for Singleton Lifestyle Selection Behavior
    /// </summary>
    public class SingletonLifestyleSelectionBehavior : ILifestyleSelectionBehavior
    {
        /// <summary>
        /// for selecting Lifestyle
        /// </summary>
        /// <param name="serviceType">serviceType</param>
        /// <param name="implementationType">implementationType</param>
        /// <returns></returns>
        public Lifestyle SelectLifestyle(Type serviceType, Type implementationType)
        {
            return Lifestyle.Scoped;
        }
    }
}