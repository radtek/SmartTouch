using LandmarkIT.Enterprise.CommunicationManager.Operations;
using SimpleInjector;
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
using SmartTouch.CRM.Domain.Users;
using SmartTouch.CRM.Domain.Campaigns;
using SmartTouch.CRM.Domain.LeadAdapters;
using SmartTouch.CRM.Domain.Forms;
using SmartTouch.CRM.Domain.ImportData;
using SmartTouch.CRM.Domain.Opportunities;
using SmartTouch.CRM.Domain.CustomFields;
using SmartTouch.CRM.Domain.Roles;
using SmartTouch.CRM.Domain.Subscriptions;
using SmartTouch.CRM.Domain.Search;
using SmartTouch.CRM.Domain.LeadScoreRules;
using SmartTouch.CRM.Domain.ContactAudit;
using SmartTouch.CRM.SearchEngine.Indexing;
using SmartTouch.CRM.SearchEngine.Search;
using SmartTouch.CRM.MessageQueues;
using SmartTouch.CRM.Domain.WebAnalytics;
using SmartTouch.CRM.Domain.Reports;
using SmartTouch.CRM.Domain.Workflows;
using SmartTouch.CRM.Domain.SuppressionList;
using SmartTouch.CRM.JobProcessor.Jobs;
using SmartTouch.CRM.JobProcessor.Jobs.CampaignJobs;
using SmartTouch.CRM.JobProcessor.Jobs.NewBounce;
using SmartTouch.CRM.JobProcessor.Jobs.VMTALog;
using SmartTouch.CRM.JobProcessor.Jobs.WebAnalytics;
using SmartTouch.CRM.JobProcessor.Jobs.Schedulers;
using SmartTouch.CRM.JobProcessor.Utilities;
using SmartTouch.CRM.JobProcessor.Jobs.FormSubmission;
using SmartTouch.CRM.JobProcessor.Jobs.Leads;
using SmartTouch.CRM.JobProcessor.Services;
using SmartTouch.CRM.JobProcessor.Services.Implementation;

namespace SmartTouch.CRM.JobProcessor
{
    public static class IoC
    {
        public static Container Container { get; set; }

        public static void Configure(Container container)
        {
            var config = JobServiceConfiguration.FromAppConfig();
            container.Register(() => config, Lifestyle.Singleton);

            container.Register<IUnitOfWork, DatabaseUnitOfWork>(Lifestyle.Singleton);
            container.Register<IActionService, ActionService>();
            container.Register<IActionRepository, ActionRepository>();
            container.Register<IUserRepository, UserRepository>();
            container.Register<IUserService, UserService>();
            container.Register<IContactService, ContactService>();
            container.Register<ITagService, TagService>();
            container.Register<IAccountService, AccountService>();
            container.Register<IImportDataService, ImportDataService>();
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

            container.Register<IOpportunitiesService, OpportunityService>();
            container.Register<IOpportunityRepository, OpportunityRepository>();
            container.Register<IAdvancedSearchService, AdvancedSearchService>();
            container.Register<IAdvancedSearchRepository, AdvancedSearchRepository>();
            container.Register<ICachingService, CachingService>();
            container.Register<IGeoService, GeoService>();
            container.Register<IIndexingService, IndexingService>();
            container.Register<IMessageQueuingService, MessageQueuingService>();
            container.Register<IContactEmailAuditRepository, ContactEmailAuditRepository>();
            container.Register<IContactTextMessageAuditRepository, ContactTextMessageAuditRepository>();
            container.Register<IContactRelationshipRepository, ContactRelationshipRepository>();
            container.Register<IContactRelationshipService, ContactRelationshipService>();
            container.RegisterConditional(typeof(ISearchService<>), typeof(SearchService<>), c => !c.Handled);
            container.Register<IPublishSubscribeService, PublishSubscribeService>();
            container.Register<IWebAnalyticsProviderService, WebAnalyticsProviderService>();
            container.Register<IWebAnalyticsProviderRepository, WebAnalyticsProviderRepository>();
            container.Register<IImageService, ImageService>();
            container.Register<ISocialIntegrationService, SocialIntegrationService>();
            container.Register<IWorkflowService, WorkflowService>();
            container.Register<IWorkflowRepository, WorkflowRepository>();
            container.Register<IMessageService, MessageService>();
            container.Register<IMessageRepository, MessageRepository>();
            container.Register<IFindSpamService, FindSpamService>();
            container.Register<ISuppressionListService, SuppressionListService>();
            container.Register<ISuppressionListRepository, SuppressionListRepository>();
            container.Register<JobService, JobService>();
            container.Register<IKickfireService>(() => new KickfireService(config.SmarttouchPartnerKey));

            container.Register<WorkflowActionJob>();
            container.Register<ApiLeadSubmissionJob>();
            container.Register<BulkOperationJob>();
            container.Register<IndexJob>();
            container.Register<FormSubmissionJob>();
            container.Register<CampaignJob>();
            container.Register<CampaignMailTesterJob>();
            container.Register<LitmusTestJob>();
            container.Register<AutomationCampaignJob>();
            container.Register<SmartSearchJob>();
            container.Register<FileReadJob>();
            container.Register<FtpJob>();
            container.Register<NeverBounceFileJob>();
            container.Register<NeverBouncePollingJob>();
            container.Register<NeverBounceResultsJob>();
            container.Register<LeadScoreJob>();
            container.Register<WebAnalyticsKickfireJob>();
            container.Register<WebVisitDailySummaryJob>();
            container.Register<WebVisitEmailNotifierJob>();
            container.Register<BouncedMailDataJob>();
            container.Register<DailySummaryEmailJob>();
            container.Register<NightlyScheduledDeliverabilityReportJob>();
            container.Register<LeadJob>();
            container.Register<ApiManager>();
            container.Register<MailService>();
            container.Register<EmailValidator>();

            container.Register<CampaignProcessorJob>();
            container.Register<CampaignSendJob>();

            Container = container;
        }
    }
}
