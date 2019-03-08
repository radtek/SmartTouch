using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

using SimpleInjector;
using SimpleInjector.Extensions.LifetimeScoping;
using SimpleInjector.Extensions;

using SmartTouch.CRM.Infrastructure.UnitOfWork;
using SmartTouch.CRM.Domain.Contacts;
using SmartTouch.CRM.Domain.Tags;
using SmartTouch.CRM.Domain.Accounts;
using SmartTouch.CRM.Domain.Notes;
using SmartTouch.CRM.Domain.Tours;
using SmartTouch.CRM.Domain.Dropdowns;
using SmartTouch.CRM.ApplicationServices.ServiceInterfaces;
using SmartTouch.CRM.ApplicationServices.ServiceImplementations;
using SmartTouch.CRM.ApplicationServices.ViewModels;
using SmartTouch.CRM.Repository.Database;
using SmartTouch.CRM.Repository;
using SmartTouch.CRM.Repository.Repositories;

using SmartTouch.CRM.Domain.Actions;
using SmartTouch.CRM.Domain.Communication;
using SmartTouch.CRM.ApplicationServices.Messaging;
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.SearchEngine.Indexing;

namespace SmartTouch.CRM.FullContactSyncEngine
{
    public static class IoC
    {
        public static Container Container { get; set; }

        public static void Configure(Container container)
        {
            //var lifetimeScope = new LifetimeScopeLifestyle();


            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);
            //container.Register<IUnitOfWork, UnitOfWork>(lifetimeScope);

            //container.RegisterManyForOpenGeneric(
            //    typeof(IRepository<>),
            //    lifetimeScope,
            //    typeof(IRepository<>).Assembly);

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
            container.Register<IMailGunService, MailGunService>();

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

            container.Register<IContactEmailAuditRepository, ContactEmailAuditRepository>();
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
            container.Register<IImageService, ImageService>();
            //container.RegisterOpenGeneric(typeof(ISearchService<>), typeof(SearchService<>));
            container.RegisterConditional(typeof(ISearchService<>), typeof(SearchService<>), c => !c.Handled);
            container.Register<IPublishSubscribeService, PublishSubscribeService>();
            container.Register<IWebAnalyticsProviderService, WebAnalyticsProviderService>();
            container.Register<IWebAnalyticsProviderRepository, WebAnalyticsProviderRepository>();
            container.Register<IMessageService, MessageService>();
            container.Register<IMessageRepository, MessageRepository>();
            container.Register<ISuppressionListService, SuppressionListService>();
            container.Register<ISuppressionListRepository, SuppressionListRepository>();
            //container.Register<IMessageQueuingService>(() =>
            //{
            //    string queueName = ConfigurationManager.AppSettings["LeadScoreQueue"];
            //    return new MessageQueuingService(queueName);
            //});



            container.Verify();
            IoC.Container = container;
        }
    }
}