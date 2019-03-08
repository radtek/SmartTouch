using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SmartTouch.CRM.Domain.Login;

namespace SmartTouch.CRM.Repository.Database
{
    public class CRMDb : DbContext
    {
        public CRMDb()
        {
            this.Database.CommandTimeout = 180; //time in seconds
        }

        public DbSet<ContactsDb> Contacts { get; set; }
        public DbSet<ContactsAuditDb> ContactsAudit { get; set; }
        public DbSet<CountriesDb> Countries { get; set; }
        public DbSet<StatesDb> States { get; set; }
        public DbSet<AddressesDb> Addresses { get; set; }
        public DbSet<ContactSourceDb> ContactFirstSource { get; set; }
        public DbSet<TagsDb> Tags { get; set; }
        public DbSet<VTagsLeadScoreDb> vTagsLeadScore { get; set; }
        public DbSet<VTagsDb> VTags { get; set; }
        public DbSet<VCampaignRecipientsDb> vCampaignRecipients { get; set; }
        public DbSet<vCampaignStatisticsDb> vCampaignStatistics { get; set; }
        
        public DbSet<UsersDb> Users { get; set; }
        public DbSet<UserSettingsDb> UserSettings { get; set; }
        public DbSet<RolesDb> Roles { get; set; }
        public DbSet<AccountsDb> Accounts { get; set; }
        public DbSet<SubscriptionsDb> Subscriptions { get; set; }
        public DbSet<SubscriptionSettingsDb> SubscriptionSettings { get; set; }
        public DbSet<SubscriptionSettingTypesDb> SubscriptionSettingTypes { get; set; }

        //public DbSet<CulturesDb> Cultures { get; set; }
        public DbSet<ContactTagMapDb> ContactTags { get; set; }
        public DbSet<ReportsDb> Reports { get; set; }
        public DbSet<ReportModuleMapDb> ReportModuleMap { get; set; }
        public DbSet<AccountCustomReportsDb> AccountCustomReports { get; set; }

        public DbSet<DropdownDb> Dropdowns { get; set; }
        public DbSet<DropdownValueDb> DropdownValues { get; set; }

        public DbSet<ModulesDb> Modules { get; set; }
        public DbSet<OperationsDb> Operations { get; set; }
        public DbSet<RoleModulesMapDb> RoleModules { get; set; }
        public DbSet<SubscriptionModuleMapDb> SubscriptionModules { get; set; }
        public DbSet<DataAccessPermissionDb> DataAccessPermissions { get; set; }

        public DbSet<UserActivitiesDb> UserActivities { get; set; }
        public DbSet<UserActivityLogsDb> UserActivitiesLog { get; set; }

        public DbSet<ActionsDb> Actions { get; set; }
        public DbSet<ActionAuditDb> Actions_Audit { get; set; }
        public DbSet<ActionTagsMapDb> ActionTags { get; set; }
        public DbSet<ContactActionMapDb> ActionContacts { get; set; }
        public DbSet<UserActionMapDb> ActionUsers { get; set; }
        public DbSet<ActionsMailOperationDb> ActionsMailOperations { get; set; }

        public DbSet<BulkOperationsDb> BulkOperations { get; set; }
        public DbSet<SubmittedFormDataDb> SubmittedFormData { get; set; }
        public DbSet<SubmittedFormFieldDataDb> SubmittedFormFieldData { get; set; }

        public DbSet<BulkContactData> BulkContactData { get; set; }

        public DbSet<NeverBounceEmailStatusDb> NeverBounceEmailStatus { get; set; }
        public DbSet<ImportContactData> ImportContactData { get; set; }
        public DbSet<ImportPhoneData> ImportPhoneData { get; set; }
        public DbSet<ImportCustomData> ImportCustomData { get; set; }
        public DbSet<ImportContactsEmailStatusesDb> ImportContactsEmailStatuses { get; set; }
        public DbSet<NeverBounceRequestDb> NeverBounceRequests { get; set; }
        public DbSet<NeverBounceMappingsDb> NeverBounceMappings { get; set; }
        public DbSet<IndexData> IndexData { get; set; }

        public DbSet<MenuCategoryDB> MenuCategory { get; set; }
        public DbSet<MenuDB> Menu { get; set; }
        public DbSet<CommunicationsDb> Communications { get; set; }
        public DbSet<DropdownValueTypeDb> DropdownValuTypes { get; set; }

        public DbSet<NotesDb> Notes { get; set; }
        public DbSet<ContactNoteMapDb> ContactNotes { get; set; }
        public DbSet<NoteTagsMapDb> NoteTags { get; set; }
        public DbSet<ContactNoteSummaryDb> ContactNoteSummary { get; set; }
        public DbSet<TourDb> Tours { get; set; }
        public DbSet<TourAuditDb> Tours_Audit { get; set; }
        public DbSet<ContactTourMapDb> ContactTours { get; set; }
        public DbSet<UserTourMapDb> UserTours { get; set; }
        public DbSet<CommunitiesDb> Communities { get; set; }
        public DbSet<ContactRelationshipDb> ContactRelationshipMap { get; set; }
        public DbSet<ContactCommunityMapDb> ContactCommunityMap { get; set; }

        public DbSet<ContactTimeLineDb> GetContactTimeLines { get; set; }
        public DbSet<OpportunitiesTimeLineDb> GetOpportunityTimeLines { get; set; }
        public DbSet<CommunicationTrackerDb> CommunicationTrackers { get; set; }
        public DbSet<AttachmentsDb> Attachment { get; set; }
        public DbSet<ServiceProvidersDb>ServiceProviders { get; set; }
        public DbSet<ImageDomainsDb> ImageDomains { get; set; }
        public DbSet<ContactImagesDb> ContactImagesDb { get; set; }

        public DbSet<ContactEmailAuditDb> ContactEmailAudit { get; set; }
        public DbSet<ContactTextMessageAuditDb> ContactTextMessage { get; set; }
        public DbSet<LeadSourceDb> LeadSource { get; set; }
     

        public DbSet<CampaignTemplatesDb> CampaignTemplates { get; set; }
        public DbSet<CampaignsDb> Campaigns { get; set; }
        public DbSet<CampaignTagMapDb> CampaignTags { get; set; }
        public DbSet<CampaignRecipientsDb> CampaignRecipients { get; set; }
        public DbSet<CampaignStatusesDb> CampaignStatuses { get; set; }
        public DbSet<CampaignContactTagMapDb> CampaignContactTags { get; set; }
        public DbSet<CampaignSearchDefinitionsDb> CampaignSearchDefinitions { get; set; }
        public DbSet<CampaignTrackerDb> CampaignStatistics { get; set; }
        public DbSet<CampaignLinksDb> CampaignLinks { get; set; }
        public DbSet<CampaignThemesDb> CampaignThemes { get; set; }
        public DbSet<CampaignPlainTextContentDb> CampaignPlainTextContent { get; set; }
        public DbSet<ResentCampaignDb> ResentCampaigns { get; set; }
        public DbSet<CampaignLogDetailsDb> CampaignLogDetails { get; set; }
        public DbSet<MomentaryCampaignRecipientsDb> MomentaryCampaignRecipients { get; set; }
        public DbSet<CampaignTypeDb> CampaignTypes { get; set; }
        public DbSet<ImagesDb> Images { get; set; }
        public DbSet<DateFormatDb> DateFormats { get; set; }
        public DbSet<CurrenciesDb> Currencies { get; set; }
        public DbSet<ScoreCategoriesDb> Categories { get; set; }
        public DbSet<ConditionDb> Conditions { get; set; }
        public DbSet<LeadScoreRulesDb> LeadScoreRules{ get; set; }
        public DbSet<LeadScoreConditionValuesDb> LeadScoreConditionValues { get; set; }
        public DbSet<CampaignMailTestDb> CampaignMailTest { get; set; }

        public DbSet<LeadScoreDb> LeadScores { get; set; }
        public DbSet<AccountEmailsDb> AccountEmails { get; set; }
       
       
        public DbSet<FormsDb> Forms { get; set; }
        public DbSet<FormTagsDb> FormTags { get; set; }
        public DbSet<FieldInputTypesDb> FieldInputTypes { get; set; }
        public DbSet<FieldsDb> Fields { get; set; }
        public DbSet<FormFieldsDb> FormFields { get; set; }
        public DbSet<FormSubmissionDb> FormSubmissions { get; set; }
        public DbSet<StatusesDb> Statuses { get; set; }
        public DbSet<FormNotificationSettingsDb> FormSettings { get; set; }

        public DbSet<CustomFieldTabDb> CustomFieldTabs { get; set; }
        public DbSet<CustomFieldSectionDb> CustomFieldSections { get; set; }
        public DbSet<CustomFieldTabSectionMapDb> CustomFieldTabSectionMap { get; set; }
        public DbSet<CustomFieldValueOptionsDb> CustomFieldValueOptions { get; set; }
        public DbSet<ContactCustomFieldsDb> ContactCustomFields{ get; set; }

    
        public DbSet<GetImportDataDb> GetImportData { get; set; }
        public DbSet<NotificationDb> Notifications { get; set; }
        public DbSet<OpportunitiesDb> Opportunities { get; set; }
        public DbSet<OpportunityContactMap> OpportunityContactMap { get; set; }
        public DbSet<OpportunityNoteMap> OpportunityNoteMap { get; set; }
        public DbSet<OpportunityActionMap> OpportunityActionMap { get; set; }
        public DbSet<RelationshipTypesDb> Relationships { get; set; }
        public DbSet<OpportunityTagMap> OpportunityTagMap { get; set; }
        public DbSet<OpportunitiesRelationshipMapDb> OpportunityRelationMap { get; set; }
        public DbSet<ContactLeadSourceMapDb> ContactLeadSourcesMap { get; set; }
        public DbSet<ContactPhoneNumbersDb> ContactPhoneNumbers { get; set; }
        public DbSet<ContactEmailsDb> ContactEmails { get; set; }

        public DbSet<LoginAudit> LoginAudit { get; set; }
        public DbSet<UserProfileAudit> UserProfileAudit { get; set; }
        public DbSet<DailySummaryEmailAuditDb> DailySummaryEmailAudit { get; set; }

        public DbSet<OpportunityStageGroupsDb> OpportunityStageGroupsDb { get; set; }
        public DbSet<OpportunityGroupsDb> OpportunityGroupsDb { get; set; }

        public DbSet<WebAnalyticsProvidersDb> WebAnalyticsProviders { get; set; }
        public DbSet<WebVisitsDb> WebVisits { get; set; }
        public DbSet<WebVisitEmailAuditDb> WebVisitEmailAudit { get; set; }
        public DbSet<WebVisitDailySummaryEmailAuditDb> WebVisitDailySummaryEmailAudit { get; set; }
        public DbSet<WebVisitUserNotificationMapDb> WebVisitUserNotificationMap { get; set; }
    
        public DbSet<ContactIPAddressesDb> ContactIPAddresses { get; set; }
        public DbSet<UserSocialMediaPostsDb> UserSocialMediaPosts { get; set; }
        public DbSet<CRMOutlookSyncDb> CRMOutlookSync { get; set; }
        public DbSet<DashBoardUserSettingsMap> DashBoardUserSettingsMap { get; set; }
        public DbSet<DashboardSettingsDb> DashboardSettings { get; set; }
        public DbSet<SeedEmailDb> SeedEmail { get; set; }
        public DbSet<TaxRateDb> TaxRates { get; set; }
        public DbSet<AccountSettingsDb> accountSettings { get; set; }
        public DbSet<LeadScoreMessageDb> Messages { get; set; }

        #region ImportData
        public DbSet<ImportDataSettingsDb> ImpoortDataSettings { get; set; }
        public DbSet<ImportTagMapDb> ImportTagMap { get; set; }
        public DbSet<ImportColumnMappingsDb> ImportColumnMappigns { get; set; }
        #endregion

        public DbSet<InvalidCouponsEngagedContactsDb> InvalidaCouponsEngagedContacts { get; set; }

        #region LeadAdapters

        public DbSet<LeadAdapterAndAccountMapDb> LeadAdapters { get; set; }       
        public DbSet<ContactLeadAdapterMapDb> ContactLeadAdapters { get; set; }
        public DbSet<LeadAdapterErrorStatusDb> LeadAdapterErrorStatus { get; set; }
        public DbSet<LeadAdapterRecordStatusDb> LeadAdapterRecordStatus { get; set; }
        public DbSet<LeadAdapterTypesDb> LeadAdapterTypes { get; set; }
        public DbSet<LeadAdapterJobLogsDb> LeadAdapterJobLogs { get; set; }
        public DbSet<LeadAdapterJobLogDetailsDb> LeadAdapterJobLogDetails { get; set; }
        public DbSet<LeadAdapterTagMapDb> LeadAdapterTags { get; set; }
        public DbSet<LeadAdapterCustomFieldsDb> LeadAdapterCustomFields { get; set; }
        public DbSet<FacebookLeadAdapterDb> FacebookLeadAdapters { get; set; }
        public DbSet<FacebookLeadGenDb> FacebookLeadGen { get; set; }

        #endregion 

        #region Advanced Search

        public DbSet<SearchDefinitionsDb> SearchDefinitions { get; set; }
        public DbSet<SearchFiltersDb> SearchFilters { get; set; }
        public DbSet<SearchPredicateTypesDb> SearchPredicates { get; set; }
        public DbSet<SearchQualifierTypesDb> SearchQualifiers { get; set; }
        public DbSet<SearchDefinitionTagMapDb> SearchDefinitionTagMap { get; set; }
        public DbSet<AVColumnPreferencesDb> AVColumnPreferences { get; set; }
       
        #endregion

        #region Workflows

        public DbSet<WorkflowsDb> Workflows { get; set; }
        public DbSet<WorkflowTriggerTypesDb> WorkflowTriggerTypes { get; set; }
        //public DbSet<WorkflowActionTypesDb> WorkflowActionTypes { get; set; }
        public DbSet<WorkflowActionsDb> WorkflowActions { get; set; }
        public DbSet<WorkflowCampaignActionsDb> WorkflowCampaignActions { get; set; }
        public DbSet<WorkflowContactFieldActionsDb> WorkflowContactFieldActions { get; set; }
        public DbSet<WorkflowLeadScoreActionsDb> WorkflowLeadScoreActions { get; set; }
        public DbSet<WorkflowEmailNotificationActionDb> WorkflowEmailNotificationAction { get; set; }
        public DbSet<WorkflowLifeCycleActionsDb> WorkflowLifeCycleActions { get; set; }
        public DbSet<WorkflowNotifyUserActionsDb> WorkflowNotifyUserActions { get; set; }
        public DbSet<WorkflowTagActionsDb> WorkflowTagActions { get; set; }
        public DbSet<WorkFlowTextNotificationActionsDb> WorkflowTextNotificationActions { get; set; }
        public DbSet<WorkFlowUserAssignmentActionsDb> WorkflowUserAssignmentActions { get; set; }
        public DbSet<WorkflowUserAssignmentAuditDb> WorkflowUserAssignmentAudit { get; set; }
        public DbSet<WorkflowDelayTimerActionsDb> WorkFlowDelayTimerActions { get; set; }
        public DbSet<WorkflowCampaignActionLinksDb> WorkflowCampaignActionLinks { get; set; }
        public DbSet<ContactWorkflowAuditDb> ContactWorkflowAudit { get; set; }
        public DbSet<WorkflowTriggersDb> WorkflowTriggers { get; set; }
        public DbSet<WorkflowTimerActionsDb> WorkflowTimerActions { get; set; }
        public DbSet<RoundRobinContactAssignmentDb> RoundRobinAssignment { get; set; }
        public DbSet<TriggerWorkflowActionsDb> TriggerWorkflowActions { get; set; }
        public DbSet<ClientRefreshTokensDb> ClientRefreshTokens { get; set; }
        public DbSet<TrackActionsDb> TrackActions { get; set; }
        public DbSet<TrackActionLogDb> TrackActionLogs { get; set; }
        public DbSet<TrackMessagesDb> TrackMessages { get; set; }
        #endregion
        public DbSet<ThirdPartyClientsDb> ThirdPartyClients { get; set; }        
        public DbSet<ApplicationTourDetailsDb> ApplicationTourDetails { get; set; }
        public DbSet<DivisionsDb> Divisions { get; set; }
        public DbSet<SectionsDB> Sections { get; set; }
        #region Marketing Messages
        public DbSet<MarketingMessagesDb> MarketingMessages { get; set; }
        public DbSet<MarketingMessageAccountMapDb> MarketingMessageAccountMap { get; set; }
        public DbSet<MarketingMessageContentMapDb> MarketingMessageContentMap { get; set; }
        #endregion
        #region Refresh Analytics
        public DbSet<RefreshAnalyticsDb> RefreshAnalytics { get; set; }
        #endregion
        #region SpamService
        public DbSet<SpamKeyWordsDb> SpamKeyWords { get; set; }
        public DbSet<SpamIPAddressesDb> SpamIPAddresses { get; set; }
        public DbSet<SpamValidatorsDb> SpamValidators { get; set; }
        #endregion
        #region  Suppressed Emails
        public DbSet<SuppressedEmailsDb> SuppressedEmails { get; set; }
        public DbSet<SuppressedDomainsDb> SuppressedDomains { get; set; }
        #endregion
        #region
        public DbSet<APILeadSubmissionsDb> APILeadSubmissions { get; set; }
        #endregion
        #region
        public DbSet<SmartSearchContactsDb> SmartSearchContacts { get; set; }
        public DbSet<SmartSearchQueueDb> SmartSearchQueues { get; set; }
        #endregion
        #region Campaign Litmus Results
        public DbSet<CampaignLitmusMapDb> CampaignLitmusResults { get; set; }
        #endregion

        #region
        public DbSet<PushNotificationsBb> PushNotifications { get; set; }
        #endregion

        public DbSet<EnvironmentSettingsDb> EnvironmentSettings { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EnvironmentSettingsDb>().ToTable("EnvironmentSettings");
            modelBuilder.Entity<ContactsDb>().ToTable("Contacts");
            modelBuilder.Entity<ContactRelationshipDb>().ToTable("ContactRelationshipMap");
            modelBuilder.Entity<AddressesDb>().ToTable("Addresses");
            modelBuilder.Entity<StatesDb>().ToTable("States");
            modelBuilder.Entity<CountriesDb>().ToTable("Countries");
            modelBuilder.Entity<CommunicationsDb>().ToTable("Communications");
            modelBuilder.Entity<TagsDb>().ToTable("Tags");
            modelBuilder.Entity<VTagsDb>().ToTable("vTags");
            modelBuilder.Entity<VTagsLeadScoreDb>().ToTable("vTagsLeadScore");
            modelBuilder.Entity<VCampaignRecipientsDb>().ToTable("vCampaignRecipients");
            modelBuilder.Entity<vCampaignStatisticsDb>().ToTable("vCampaignStatistics");  

            modelBuilder.Entity<UsersDb>().ToTable("Users");
            modelBuilder.Entity<DropdownDb>().ToTable("Dropdowns");
            modelBuilder.Entity<DropdownValueDb>().ToTable("DropdownValues");
            modelBuilder.Entity<TaxRateDb>().ToTable("TaxRates");
            modelBuilder.Entity<ContactSourceDb>().ToTable("ContactSource");
            modelBuilder.Entity<BulkOperationsDb>().ToTable("BulkOperations");
            modelBuilder.Entity<BulkContactData>().ToTable("BulkContactData");      

            modelBuilder.Entity<SubmittedFormDataDb>().ToTable("SubmittedFormData");
            modelBuilder.Entity<SubmittedFormFieldDataDb>().ToTable("SubmittedFormFieldData");


            modelBuilder.Entity<AccountsDb>().ToTable("Accounts");
            //modelBuilder.Entity<CulturesDb>().ToTable("Cultures");
            modelBuilder.Entity<ContactTagMapDb>().ToTable("ContactTagMap");
            modelBuilder.Entity<UserActionMapDb>().ToTable("UserActionMap");
            modelBuilder.Entity<ActionsMailOperationDb>().ToTable("ActionsMailOperations");

            modelBuilder.Entity<RolesDb>().ToTable("Roles");
            modelBuilder.Entity<SubscriptionsDb>().ToTable("Subscriptions");
            modelBuilder.Entity<SubscriptionSettingsDb>().ToTable("SubscriptionSettings");
            modelBuilder.Entity<SubscriptionSettingTypesDb>().ToTable("SubscriptionSettingTypes");
            
            modelBuilder.Entity<ModulesDb>().ToTable("Modules");
            modelBuilder.Entity<OperationsDb>().ToTable("Operations");
            modelBuilder.Entity<DataAccessPermissionDb>().ToTable("AccountDataAccessPermissions");

            modelBuilder.Entity<UserActivitiesDb>().ToTable("UserActivities");
            modelBuilder.Entity<UserActivityLogsDb>().ToTable("UserActivityLogs");
            modelBuilder.Entity<ReportsDb>().ToTable("Reports");
            modelBuilder.Entity<ReportModuleMapDb>().ToTable("ReportModuleMap");
            modelBuilder.Entity<AccountCustomReportsDb>().ToTable("AccountCustomReports");
            modelBuilder.Entity<ActionsDb>().ToTable("Actions");
            modelBuilder.Entity<ActionAuditDb>().ToTable("Actions_Audit");
            modelBuilder.Entity<ImportContactData>().ToTable("ImportContactData");
            modelBuilder.Entity<NeverBounceEmailStatusDb>().ToTable("NeverBounceEmailStatus");
            modelBuilder.Entity<ImportPhoneData>().ToTable("ImportPhoneData");
            modelBuilder.Entity<ImportCustomData>().ToTable("ImportCustomData");           
            modelBuilder.Entity<ImportContactsEmailStatusesDb>().ToTable("ImportContactsEmailStatuses");
            modelBuilder.Entity<NeverBounceRequestDb>().ToTable("NeverBounceRequests");
            modelBuilder.Entity<NeverBounceMappingsDb>().ToTable("NeverBounceMappings");

            modelBuilder.Entity<IndexData>().ToTable("IndexData");
            modelBuilder.Entity<ActionTagsMapDb>().ToTable("ActionTagMap");
            modelBuilder.Entity<ContactActionMapDb>().ToTable("ContactActionMap");

            modelBuilder.Entity<NotesDb>().ToTable("Notes");
            modelBuilder.Entity<ContactNoteMapDb>().ToTable("ContactNoteMap");
            modelBuilder.Entity<ContactNoteSummaryDb>().ToTable("ContactSummary");
            modelBuilder.Entity<OpportunityNoteMap>().ToTable("OpportunityNoteMap");
            modelBuilder.Entity<OpportunityActionMap>().ToTable("OpportunityActionMap");
            modelBuilder.Entity<NoteTagsMapDb>().ToTable("NoteTagMap");
            modelBuilder.Entity<ContactEmailAuditDb>().ToTable("ContactEmailAudit");
            modelBuilder.Entity<ContactTextMessageAuditDb>().ToTable("ContactTextMessageAudit");
            //modelBuilder.Entity<ContactAddressMapDb>().ToTable("ContactAddressMap");
            // modelBuilder.Entity<ContactTagsDb>().ToTable("ContactTags");
            modelBuilder.Entity<MenuCategoryDB>().ToTable("MenuCategories");
            modelBuilder.Entity<MenuDB>().ToTable("MenuItems");
            modelBuilder.Entity<TourDb>().ToTable("Tours");
            modelBuilder.Entity<TourAuditDb>().ToTable("Tours_Audit");
            modelBuilder.Entity<CommunitiesDb>().ToTable("Communities");
            modelBuilder.Entity<ContactCommunityMapDb>().ToTable("ContactCommunityMap");
            modelBuilder.Entity<ContactTourMapDb>().ToTable("ContactTourMap");
            modelBuilder.Entity<UserTourMapDb>().ToTable("UserTourMap");
            modelBuilder.Entity<UserSettingsDb>().ToTable("UserSettings");
            modelBuilder.Entity<ContactTimeLineDb>().ToTable("GET_TIME_LINES");
            modelBuilder.Entity<OpportunitiesTimeLineDb>().ToTable("GET_Opportunities_Timelines");
            modelBuilder.Entity<CommunicationTrackerDb>().ToTable("CommunicationTracker");
            modelBuilder.Entity<AttachmentsDb>().ToTable("Documents");
            modelBuilder.Entity<ServiceProvidersDb>().ToTable("ServiceProviders");
            modelBuilder.Entity<ContactImagesDb>().ToTable("ContactImages");
            modelBuilder.Entity<ImageDomainsDb>().ToTable("ImageDomains");
            

            modelBuilder.Entity<CampaignTemplatesDb>().ToTable("CampaignTemplates");
            modelBuilder.Entity<CampaignsDb>().ToTable("Campaigns");
            modelBuilder.Entity<CampaignTagMapDb>().ToTable("CampaignTagMap");
            modelBuilder.Entity<CampaignContactTagMapDb>().ToTable("CampaignContactTagMap");
            modelBuilder.Entity<CampaignRecipientsDb>().ToTable("CampaignRecipients");
            modelBuilder.Entity<ImagesDb>().ToTable("Images");
            modelBuilder.Entity<CampaignStatusesDb>().ToTable("CampaignStatuses");
            modelBuilder.Entity<CampaignSearchDefinitionsDb>().ToTable("CampaignSearchDefinitionMap");
            modelBuilder.Entity<CampaignTrackerDb>().ToTable("CampaignStatistics");
            modelBuilder.Entity<CampaignLinksDb>().ToTable("CampaignLinks");
            modelBuilder.Entity<CampaignThemesDb>().ToTable("CampaignThemes");
            modelBuilder.Entity<CampaignPlainTextContentDb>().ToTable("CampaignPlainTextContentMap");
            modelBuilder.Entity<ResentCampaignDb>().ToTable("ResentCampaigns");
            modelBuilder.Entity<CampaignLogDetailsDb>().ToTable("CampaignLogDetails");
            modelBuilder.Entity<CampaignTypeDb>().ToTable("CampaignTypes");
            modelBuilder.Entity<MomentaryCampaignRecipientsDb>().ToTable("MomentaryCampaignRecipients");
            modelBuilder.Entity<DateFormatDb>().ToTable("DateFormats");
            modelBuilder.Entity<CurrenciesDb>().ToTable("Currencies");
            modelBuilder.Entity<CampaignMailTestDb>().ToTable("CampaignMailTest");

            modelBuilder.Entity<DropdownValueTypeDb>().ToTable("DropdownValueTypes");
            modelBuilder.Entity<GetImportDataDb>().ToTable("Get_Import_Data");
            modelBuilder.Entity<LeadScoreDb>().ToTable("LeadScores");
            modelBuilder.Entity<LeadScoreRulesDb>().ToTable("LeadScoreRules");
            modelBuilder.Entity<LeadScoreConditionValuesDb>().ToTable("LeadScoreConditionValues");
            modelBuilder.Entity<ScoreCategoriesDb>().ToTable("ScoreCategories");
            modelBuilder.Entity<ConditionDb>().ToTable("Conditions");
            
            modelBuilder.Entity<AccountEmailsDb>().ToTable("AccountEmails");
            modelBuilder.Entity<FormsDb>().ToTable("Forms");
            modelBuilder.Entity<FormTagsDb>().ToTable("FormTags");
            modelBuilder.Entity<StatusesDb>().ToTable("Statuses");
            modelBuilder.Entity<FieldInputTypesDb>().ToTable("FieldInputTypes");
            modelBuilder.Entity<FieldsDb>().ToTable("Fields");
            modelBuilder.Entity<FormFieldsDb>().ToTable("FormFields");
            modelBuilder.Entity<FormSubmissionDb>().ToTable("FormSubmissions");
            modelBuilder.Entity<FormNotificationSettingsDb>().ToTable("FormNotificationSettings");

            modelBuilder.Entity<CustomFieldTabDb>().ToTable("CustomFieldTabs");
            modelBuilder.Entity<CustomFieldSectionDb>().ToTable("CustomFieldSections");
            modelBuilder.Entity<CustomFieldTabSectionMapDb>().ToTable("CustomFieldSectionCustomFieldMap");
            modelBuilder.Entity<CustomFieldValueOptionsDb>().ToTable("CustomFieldValueOptions");
            modelBuilder.Entity<ContactCustomFieldsDb>().ToTable("ContactCustomFieldMap");

            modelBuilder.Entity<NotificationDb>().ToTable("Notifications");
            modelBuilder.Entity<OpportunitiesDb>().ToTable("Opportunities");
            modelBuilder.Entity<OpportunityContactMap>().ToTable("OpportunityContactMap");
            modelBuilder.Entity<OpportunityTagMap>().ToTable("OpportunityTagMap");
        //    modelBuilder.Entity<OpportunityNoteMap>().ToTable("OpportunityNoteMap");
            modelBuilder.Entity<RelationshipTypesDb>().ToTable("RelationshipTypes");
            modelBuilder.Entity<OpportunitiesRelationshipMapDb>().ToTable("OpportunitiesRelationshipMap");

            modelBuilder.Entity<SubscriptionModuleMapDb>().ToTable("SubscriptionModuleMap");
            modelBuilder.Entity<RoleModulesMapDb>().ToTable("RoleModuleMap");

            modelBuilder.Entity<ContactLeadSourceMapDb>().ToTable("ContactLeadSourceMap");

            modelBuilder.Entity<ContactPhoneNumbersDb>().ToTable("ContactPhoneNumbers");
            modelBuilder.Entity<ContactEmailsDb>().ToTable("ContactEmails");

            modelBuilder.Entity<OpportunityStageGroupsDb>().ToTable("OpportunityStageGroups");
            modelBuilder.Entity<OpportunityGroupsDb>().ToTable("OpportunityGroups");
            modelBuilder.Entity<ContactsAuditDb>().ToTable("Contacts_Audit");
            modelBuilder.Entity<UserSocialMediaPostsDb>().ToTable("UserSocialMediaPosts");
            modelBuilder.Entity<CRMOutlookSyncDb>().ToTable("CRMOutlookSync");
            modelBuilder.Entity<DashboardSettingsDb>().ToTable("DashboardSettings");
            modelBuilder.Entity<DashBoardUserSettingsMap>().ToTable("DashboardUserSettingsMap");
            modelBuilder.Entity<ThirdPartyClientsDb>().ToTable("ThirdPartyClients");
            modelBuilder.Entity<ClientRefreshTokensDb>().ToTable("ClientRefreshTokens");
            modelBuilder.Entity<AccountSettingsDb>().ToTable("AccountSettings");

            modelBuilder.Entity<SeedEmailDb>().ToTable("SeedList");
            modelBuilder.Entity<ApplicationTourDetailsDb>().ToTable("ApplicationTourDetails");
            modelBuilder.Entity<DivisionsDb>().ToTable("Divisions");
            modelBuilder.Entity<SectionsDB>().ToTable("Sections");

            #region WebAnalytics
            modelBuilder.Entity<WebAnalyticsProvidersDb>().ToTable("WebAnalyticsProviders");
            modelBuilder.Entity<WebVisitEmailAuditDb>().ToTable("WebVisitEmailAudit");
            modelBuilder.Entity<WebVisitDailySummaryEmailAuditDb>().ToTable("WebVisitDailySummaryEmailAudit");
            modelBuilder.Entity<WebVisitUserNotificationMapDb>().ToTable("WebVisitUserNotificationMap");
            modelBuilder.Entity<ContactIPAddressesDb>().ToTable("ContactIPAddresses");
            modelBuilder.Entity<WebVisitsDb>().ToTable("ContactWebVisits");

            #endregion

            #region Workflows
            
            
            modelBuilder.Entity<WorkflowsDb>().ToTable("Workflows");
            modelBuilder.Entity<WorkflowTriggerTypesDb>().ToTable("WorkflowTriggerTypes");
            //modelBuilder.Entity<WorkflowActionTypesDb>().ToTable("WorkflowActionTypes");
            modelBuilder.Entity<WorkflowActionsDb>().ToTable("WorkflowActions");
            modelBuilder.Entity<WorkflowCampaignActionsDb>().ToTable("WorkflowCampaignActions"); 
            modelBuilder.Entity<WorkflowContactFieldActionsDb>().ToTable("WorkflowContactFieldAction");
            modelBuilder.Entity<WorkflowLeadScoreActionsDb>().ToTable("WorkFlowLeadScoreAction");
            modelBuilder.Entity<WorkflowLifeCycleActionsDb>().ToTable("WorkFlowLifeCycleAction");
            modelBuilder.Entity<WorkflowNotifyUserActionsDb>().ToTable("WorkflowNotifyUserAction");
            modelBuilder.Entity<WorkflowTagActionsDb>().ToTable("WorkflowTagAction");
            modelBuilder.Entity<WorkFlowTextNotificationActionsDb>().ToTable("WorkFlowTextNotificationAction");
            modelBuilder.Entity<WorkFlowUserAssignmentActionsDb>().ToTable("WorkFlowUserAssignmentAction");
            modelBuilder.Entity<WorkflowUserAssignmentAuditDb>().ToTable("WorkflowUserAssignmentAudit");
            modelBuilder.Entity<WorkflowEmailNotificationActionDb>().ToTable("WorkflowEmailNotificationAction");
            
            modelBuilder.Entity<WorkflowDelayTimerActionsDb>().ToTable("WorkFlowDelayTimerAction");
            modelBuilder.Entity<WorkflowCampaignActionLinksDb>().ToTable("WorkflowCampaignActionLinks");
            modelBuilder.Entity<ContactWorkflowAuditDb>().ToTable("ContactWorkflowAudit");
            modelBuilder.Entity<WorkflowTriggersDb>().ToTable("WorkflowTriggers");
            modelBuilder.Entity<WorkflowTimerActionsDb>().ToTable("WorkflowTimerActions");
            modelBuilder.Entity<TriggerWorkflowActionsDb>().ToTable("TriggerWorkflowAction");
            modelBuilder.Ignore<BaseWorkflowActionsDb>();

            modelBuilder.Entity<TrackActionsDb>().ToTable("TrackActions", "Workflow");
            modelBuilder.Entity<TrackMessagesDb>().ToTable("TrackMessages", "Workflow");
            modelBuilder.Entity<RoundRobinContactAssignmentDb>().ToTable("RoundRobinContactAssignment");

            #endregion

            #region leadadapters
            modelBuilder.Entity<LeadAdapterAndAccountMapDb>().ToTable("LeadAdapterAndAccountMap");
            modelBuilder.Entity<ContactLeadAdapterMapDb>().ToTable("ContactLeadAdapterMap");
            modelBuilder.Entity<LeadAdapterErrorStatusDb>().ToTable("LeadAdapterErrorStatus");
            modelBuilder.Entity<LeadAdapterTypesDb>().ToTable("LeadAdapterTypes");
            modelBuilder.Entity<LeadAdapterJobLogsDb>().ToTable("LeadAdapterJobLogs");
            modelBuilder.Entity<LeadAdapterJobLogDetailsDb>().ToTable("LeadAdapterJobLogDetails");
            modelBuilder.Entity<LeadAdapterRecordStatusDb>().ToTable("LeadAdapterRecordStatus");
            modelBuilder.Entity<LeadAdapterTagMapDb>().ToTable("LeadAdapterTagMap");
            modelBuilder.Entity<LeadAdapterCustomFieldsDb>().ToTable("LeadAdapterCustomFields");
            modelBuilder.Entity<FacebookLeadAdapterDb>().ToTable("FacebookLeadAdapter");
            modelBuilder.Entity<FacebookLeadGenDb>().ToTable("FacebookLeadgen");

            #endregion 

            #region ImportData
            modelBuilder.Entity<ImportDataSettingsDb>().ToTable("ImportDataSettings");
            modelBuilder.Entity<ImportTagMapDb>().ToTable("ImportTagMap");
            modelBuilder.Entity<ImportColumnMappingsDb>().ToTable("ImportColumnMappings");
            #endregion

            modelBuilder.Entity<LoginAudit>().ToTable("LoginAudit");
            modelBuilder.Entity<UserProfileAudit>().ToTable("UserProfileAudit");
            modelBuilder.Entity<DailySummaryEmailAuditDb>().ToTable("DailySummaryEmailAudit");
            
            #region Advanced Search

            modelBuilder.Entity<SearchDefinitionsDb>().ToTable("SearchDefinitions");
            modelBuilder.Entity<SearchFiltersDb>().ToTable("SearchFilters");
            modelBuilder.Entity<SearchPredicateTypesDb>().ToTable("SearchPredicateTypes");
            modelBuilder.Entity<SearchQualifierTypesDb>().ToTable("SearchQualifierTypes");
            modelBuilder.Entity<SearchDefinitionTagMapDb>().ToTable("SearchDefinitionTagMap");
            modelBuilder.Entity<AVColumnPreferencesDb>().ToTable("AVColumnPreferences");
            

            #endregion
            modelBuilder.Entity<LeadScoreMessageDb>().ToTable("LeadScoreMessages");
            #region Marketing Messages
            modelBuilder.Entity<MarketingMessagesDb>().ToTable("MarketingMessages");
            modelBuilder.Entity<MarketingMessageAccountMapDb>().ToTable("MarketingMessageAccountMap");
            modelBuilder.Entity<MarketingMessageContentMapDb>().ToTable("MarketingMessageContentMap");
            #endregion
            #region
            modelBuilder.Entity<RefreshAnalyticsDb>().ToTable("RefreshAnalytics");
            #endregion
            #region Spam Service
            modelBuilder.Entity<SpamKeyWordsDb>().ToTable("SpamKeyWords");
            modelBuilder.Entity<SpamIPAddressesDb>().ToTable("SpamIPAddresses");
            modelBuilder.Entity<SpamValidatorsDb>().ToTable("SpamValidators");
            #endregion

            #region Suppressed Emails
            modelBuilder.Entity<SuppressedEmailsDb>().ToTable("SuppressedEmails");
            modelBuilder.Entity<SuppressedDomainsDb>().ToTable("SuppressedDomains");
            #endregion

            #region For GrabOn Invalid CouponReport
            modelBuilder.Entity<InvalidCouponsEngagedContactsDb>().ToTable("InvalidCouponsEngagedContacts");
            #endregion

            #region API Lead Submissions
            modelBuilder.Entity<APILeadSubmissionsDb>().ToTable("APILeadSubmissions");
            #endregion

            #region SavedSearchContacts Insertion
            modelBuilder.Entity<SmartSearchContactsDb>().ToTable("SmartSearchContacts");
            modelBuilder.Entity<SmartSearchQueueDb>().ToTable("SmartSearchQueue");
            #endregion

            #region Campaign Litmus Results
            modelBuilder.Entity<CampaignLitmusMapDb>().ToTable("CampaignLitmusMap");
            #endregion

            #region
            modelBuilder.Entity<PushNotificationsBb>().ToTable("PushNotifications");
            #endregion

            modelBuilder.Entity<ContactsDb>().HasMany(c => c.Addresses)
                .WithMany()
                .Map(x =>
                    {
                        x.MapLeftKey("ContactID");
                        x.MapRightKey("AddressID");
                        x.ToTable("ContactAddressMap");
                    });            



            modelBuilder.Entity<ContactRelationshipDb>()
                  .HasRequired(m => m.Contact)
                  .WithMany(t => t.ContactRelations)
                  .HasForeignKey(m => m.ContactID)
                  .WillCascadeOnDelete(false);

            modelBuilder.Entity<ContactRelationshipDb>()
                        .HasRequired(m => m.RelatedContact)
                        .WithMany(t => t.RelatedContactRelations)
                        .HasForeignKey(m => m.RelatedContactID)
                        .WillCascadeOnDelete(false);


            modelBuilder.Entity<AccountsDb>().HasMany(c => c.Addresses)
                .WithMany()
                .Map(x =>
                {
                    x.MapLeftKey("AccountID");
                    x.MapRightKey("AddressID");
                    x.ToTable("AccountAddressMap");
                });

            modelBuilder.Entity<UsersDb>().HasMany(c => c.Addresses)
                .WithMany()
                .Map(x =>
                {
                    x.MapLeftKey("UserID");
                    x.MapRightKey("AddressID");
                    x.ToTable("UserAddressMap");
                });

            //modelBuilder.Entity<UsersDb>().HasMany(e => e.Emails)
            //    .WithMany()
            //    .Map(x =>
            //{
            //    x.MapLeftKey("UserID");
            //    x.MapRightKey("EmailID");
            //    x.ToTable("UserEmailsMap");
            //});

            //modelBuilder.Entity<SubscriptionsDb>().
            //    HasMany(r => r.SubscriptionModules).
            //    WithMany(p => p.Subscriptions).
            //    Map(m =>
            //    {
            //        m.MapLeftKey("SubscriptionID");
            //        m.MapRightKey("ModuleID");
            //        m.ToTable("SubscriptionModuleMap");
            //    });

            //modelBuilder.Entity<FormsDb>().HasMany(e => e.FormFields)
            //    .WithMany()
            //    .Map(x =>
            //    {
            //        x.MapLeftKey("FormID");
            //        x.MapRightKey("FormID");
            //        x.ToTable("UserEmailsMap");
            //    });

            //modelBuilder.Entity<SubscriptionsDb>().HasMany(c => c.ModuleOperations)
            //    .WithMany()
            //    .Map(x =>
            //    {
            //        x.MapLeftKey("SubscriptionID");
            //        x.MapRightKey("ModuleOperationsMapID");
            //        x.ToTable("SubscriptionPermissionsMap");
            //    });

            //modelBuilder.Entity<RolesDb>().HasMany(c => c.ModuleOperations)
            //   .WithMany()
            //   .Map(x =>
            //   {
            //       x.MapLeftKey("RoleID");
            //       x.MapRightKey("ModuleOperationsMapID");
            //       x.ToTable("RolePermissionsMap");
            //   });
        }
    }
}