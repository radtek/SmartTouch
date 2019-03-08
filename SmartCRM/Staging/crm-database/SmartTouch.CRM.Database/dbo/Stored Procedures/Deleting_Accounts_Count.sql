

CREATE PROCEDURE [dbo].[Deleting_Accounts_Count]
(
      @Accountid int = 60
  )
  as
 BEGIN
 SET NOCOUNT ON


SELECT COUNT(CampaignID) VCampaignStatistics FROM VCampaignStatistics WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignLinks FROM CampaignLinks WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(LinkID) WorkflowCampaignActionLinks FROM WorkflowCampaignActionLinks WITH (NOLOCK) WHERE  LinkID  IN (SELECT CampaignLinkID FROM CampaignLinks cl 
inner join Campaigns c on cl.CampaignID = c.CampaignID WHERE  c.AccountID = @AccountID )  
Print  'Deleting_CampaignLinkS_sp-count'

SELECT COUNT(CampaignID) vCampaignRecipients FROM vCampaignRecipients WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) VCampaignStatistics FROM VCampaignStatistics WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) ResentCampaigns FROM ResentCampaigns WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignTagMap FROM CampaignTagMap WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignContactTagMap FROM CampaignContactTagMap WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) UserSocialMediaPosts FROM UserSocialMediaPosts WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignLinks FROM CampaignLinks WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) WorkflowCampaignActions FROM WorkflowCampaignActions WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignSearchDefinitionMap FROM CampaignSearchDefinitionMap WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignAnalytics FROM CampaignAnalytics WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) CampaignLogDetails FROM CampaignLogDetails WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) MomentaryCampaignRecipients  FROM MomentaryCampaignRecipients WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CampaignID) ClassicCampaigns FROM ClassicCampaigns WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )

SELECT COUNT(CampaignID) Campaigns FROM Campaigns WITH (NOLOCK) WHERE  CampaignID IN (SELECT CampaignID FROM Campaigns WITH (NOLOCK) WHERE  AccountID = @AccountID )

Print  'Deleting_Campaigns_sp-count'

SELECT COUNT(CampaignTemplateID) FROM CampaignTemplateS WITH (NOLOCK) WHERE AccountID = @AccountID 

Print  'Deleting_CampaignTemplates_sp'

SELECT COUNT(ContactCommunityMapID) ContactCommunityMap FROM ContactCommunityMap WITH (NOLOCK) WHERE   CommunityID IN (SELECT CommunityID FROM Communities WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(TourID) Tours FROM Tours WITH (NOLOCK) WHERE  CommunityID IN (SELECT CommunityID FROM Communities WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) Tours_Audit FROM Tours_Audit WITH (NOLOCK) WHERE  CommunityID IN (SELECT CommunityID FROM Communities WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CommunityID) Communities FROM Communities WITH (NOLOCK) WHERE AccountID = @AccountID 

Print  'Deleting_Community_sp'

SELECT COUNT(ContactEmailAuditID)ContactEmailAudit FROM ContactEmailAudit  WITH (NOLOCK) WHERE  ContactEmailID IN (SELECT ContactEmailID FROM ContactEmails WHERE Accountid = @Accountid )
SELECT COUNT(ContactEmailID)ContactEmails FROM ContactEmails   WITH (NOLOCK) WHERE Accountid = @Accountid
SELECT COUNT(AuditId)ContactEmails_Audit FROM ContactEmails_Audit  WITH (NOLOCK) WHERE Accountid = @Accountid

Print  'Deleting_ContactEmails_sp'

SELECT COUNT(ContactTextMessageAuditID) ContactTextMessageAudit FROM ContactTextMessageAudit WITH (NOLOCK) WHERE  ContactPhoneNumberID IN (SELECT ContactPhoneNumberID FROM Communities WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactPhoneNumberID) ContactPhoneNumbers FROM ContactPhoneNumbers WITH (NOLOCK) WHERE   AccountID = @AccountID 
Print  'Deleting_ContactPhoneNumbers_sp'

SELECT COUNT(ImportContactDataID) ImportContactData FROM ImportContactData WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ImportContactLogID) ImportContactLogs FROM ImportContactLogs WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(TrackMessageID) WorkflowTrackMessages FROM [Workflow].[TrackMessages] WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) OpportunitiesRelationshipMap_Audit FROM OpportunitiesRelationshipMap_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(LeadScoreMessageID) LeadScoreMessages FROM LeadScoreMessages WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactActionMapID) ContactActionMap FROM ContactActionMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) ContactActionMap_Audit FROM ContactActionMap_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(CommunicationTrackerID) CommunicationTracker FROM CommunicationTracker WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactNoteMapID) ContactNoteMap FROM ContactNoteMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(OpportunityContactMapID) OpportunityContactMap FROM OpportunityContactMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) OpportunityContactMap_Audit FROM OpportunityContactMap_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactTourMapID) ContactTourMap FROM ContactTourMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) ContactTourMap_Audit FROM ContactTourMap_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(BulkContactDataId) BulkContactData FROM BulkContactData WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactRelationshipMapID) ContactRelationshipMap FROM ContactRelationshipMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) ContactRelationshipMap_Audit FROM ContactRelationshipMap_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(FormSubmissionID) FormSubmissions FROM FormSubmissions WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactEmailId) ContactEmails FROM ContactEmails WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(AuditId) ContactEmails_Audit FROM ContactEmails_Audit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(DocumentId) Documents FROM Documents WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(LeadScoreId) LeadScores FROM LeadScores WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactCommunityMapId) ContactCommunityMap FROM ContactCommunityMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(MomentaryRecipientID) MomentaryCampaignRecipients FROM MomentaryCampaignRecipients WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactIPAddressid) ContactIPAddresses FROM ContactIPAddresses WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactLeadAdapterMapId) ContactLeadAdapterMap FROM ContactLeadAdapterMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactAddressMapId) ContactAddressMap FROM ContactAddressMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactPhoneNumberId) ContactPhoneNumbers FROM ContactPhoneNumbers WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactCustomFieldMapId) ContactCustomFieldMap FROM ContactCustomFieldMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactWebVisitId) ContactWebVisits FROM ContactWebVisits WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactTagMapId) ContactTagMap FROM ContactTagMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactLeadSourceMapId) ContactLeadSourceMap FROM ContactLeadSourceMap WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (DocumentID) DocRepositorys FROM DocRepositorys WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactWorkflowAuditID) ContactWorkflowAudit FROM ContactWorkflowAudit WITH (NOLOCK) WHERE  ContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ReceivedMailAuditID) ReceivedMailAudit FROM ReceivedMailAudit WITH (NOLOCK) WHERE  SentByContactID IN (SELECT ContactID FROM Contacts WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT(ContactId) Contacts FROM Contacts WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT(AuditId) Contacts_Audit FROM Contacts_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
Print  'Deleting_Contacts_sp'


SELECT COUNT (FieldID) Fields FROM Fields WITH (NOLOCK) WHERE  CustomFieldSectionID IN (SELECT CustomFieldSectionID FROM CustomFieldSections WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CustomFieldSectionID) CustomFieldSections FROM CustomFieldSections WITH (NOLOCK) WHERE  TabID IN (SELECT CustomFieldTabID FROM CustomFieldTabs WITH (NOLOCK) WHERE  AccountID = @AccountID )
Print 'Deleting_CustomFieldSections_sp'

SELECT COUNT (SearchFilterID) SearchFilters FROM SearchFilters WITH (NOLOCK) WHERE  DropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SubscriptionDefaultDropdownValueMapID) SubscriptionDefaultDropdownValueMap FROM SubscriptionDefaultDropdownValueMap WITH (NOLOCK) WHERE  DropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowContactFieldActionID) WorkflowContactFieldAction FROM WorkflowContactFieldAction WITH (NOLOCK) WHERE  DropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (OpportunityStageGroupID) OpportunityStageGroups FROM OpportunityStageGroups WITH (NOLOCK) WHERE  DropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (AddressID) Addresses FROM Addresses WITH (NOLOCK) WHERE  AddressTypeID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (TourID) Tours FROM Tours WITH (NOLOCK) WHERE  TourType IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (TourID) Tours FROM Tours WITH (NOLOCK) WHERE  CommunityID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormID) Forms FROM Forms WITH (NOLOCK) WHERE  LeadSourceID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ContactID) Contacts FROM Contacts WITH (NOLOCK) WHERE  LifecycleStage IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  OpportunityStageID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  LifecycleDropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkFlowLifeCycleActionID) WorkFlowLifeCycleAction FROM WorkFlowLifeCycleAction WITH (NOLOCK) WHERE  LifecycleDropdownValueID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (OpportunityID) Opportunities FROM Opportunities WITH (NOLOCK) WHERE  StageID IN (SELECT DropdownValueID FROM DropdownValues WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (DropdownValueID) DropdownValues FROM DropdownValues WITH (NOLOCK) WHERE    AccountID = @AccountID 

Print 'Deleting_DropdownValues_sp'
SELECT COUNT (CustomFieldValueOptionID) CustomFieldValueOptions FROM CustomFieldValueOptions WITH (NOLOCK) WHERE  CustomFieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowContactFieldActionID) WorkflowContactFieldAction FROM WorkflowContactFieldAction WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormFieldID) FormFields FROM FormFields WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SearchFilterID) SearchFilters FROM SearchFilters WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (AVColumnPreferenceID) AVColumnPreferences FROM AVColumnPreferences WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ImportCustomDataID) ImportCustomData FROM ImportCustomData WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM Fields WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FieldID) Fields FROM Fields WITH (NOLOCK) WHERE AccountID = @AccountID 

Print 'Deleting_Fields_sp'

SELECT COUNT (LeadAdapterTagMapID) LeadAdapterTagMap FROM LeadAdapterTagMap WITH (NOLOCK) WHERE  LeadAdapterID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (LeadAdapterJobLogID) LeadAdapterJobLogs FROM LeadAdapterJobLogs WITH (NOLOCK) WHERE  LeadAdapterAndAccountMapID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  LeadAdapterID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ContactLeadAdapterMapID) ContactLeadAdapterMap FROM ContactLeadAdapterMap WITH (NOLOCK) WHERE  LeadAdapterID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (LeadAdapterAndAccountMapID) LeadAdapterAndAccountMap FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE AccountID = @AccountID 
Print 'Deleting_LeadAdapterAndAccountMap_sp'

SELECT COUNT (LeadScoreID) LeadScores FROM LeadScores WITH (NOLOCK) WHERE  LeadScoreRuleID IN (SELECT LeadScoreRuleID FROM LeadScoreRules WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (LeadScoreConditionValueID) LeadScoreConditionValues FROM LeadScoreConditionValues WITH (NOLOCK) WHERE  LeadScoreRuleID IN (SELECT LeadScoreRuleID FROM LeadScoreRules WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (LeadScoreRuleID) LeadScoreRules FROM LeadScoreRules WITH (NOLOCK) WHERE   AccountID = @AccountID 

Print 'Deleting_LeadScoreRules_sp'

SELECT COUNT (NoteTagMapID) NoteTagMap FROM NoteTagMap WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (OpportunityNoteMapID) OpportunityNoteMap FROM OpportunityNoteMap WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (AuditId) OpportunityNoteMap_Audit FROM OpportunityNoteMap_Audit WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (AuditId) NoteTagMap_Audit FROM NoteTagMap_Audit WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ContactNoteMapID) ContactNoteMap FROM ContactNoteMap WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (AuditId) ContactNoteMap_Audit FROM ContactNoteMap_Audit WITH (NOLOCK) WHERE  NoteID IN (SELECT NoteID FROM Notes WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (NoteID) Notes FROM Notes WITH (NOLOCK) WHERE AccountID = @AccountID 
SELECT COUNT (AuditId) Notes_Audit FROM Notes_Audit WITH (NOLOCK) WHERE AccountID = @AccountID 
Print 'Deleting_Notes_sp'

SELECT COUNT (RoleModuleMapID) RoleModuleMap FROM RoleModuleMap WITH (NOLOCK) WHERE  RoleID IN (SELECT RoleID FROM Roles WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserID) Users  FROM Users WITH (NOLOCK) WHERE  RoleID IN (SELECT RoleID FROM Roles WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (BulkOperationID) BulkOperations FROM BulkOperations WITH (NOLOCK) WHERE  RoleID IN (SELECT RoleID FROM Roles WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (RoleID) Roles FROM Roles WITH (NOLOCK) WHERE  AccountID = @AccountID 
Print 'Deleting_Roles_sp'


SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SearchFilterID) SearchFilters FROM SearchFilters WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SearchDefinitionTagMapID) SearchDefinitionTagMap FROM SearchDefinitionTagMap WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SearchDefinitionSubscriptionMapID) SearchDefinitionSubscriptionMap FROM SearchDefinitionSubscriptionMap WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignSearchDefinitionMapID) CampaignSearchDefinitionMap FROM CampaignSearchDefinitionMap WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (BulkOperationID) BulkOperations FROM BulkOperations WITH (NOLOCK) WHERE  SearchDefinitionID IN (SELECT SearchDefinitionID FROM SearchDefinitions WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SearchDefinitionID) SearchDefinitions FROM SearchDefinitions WITH (NOLOCK) WHERE   AccountID = @AccountID 

Print 'Deleting_SearchDefinitions_sp'

SELECT COUNT (CampaignRecipientID) CampaignRecipients_0001 FROM CampaignRecipients_0001 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignRecipientID) CampaignRecipients_0002 FROM CampaignRecipients_0002 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignRecipientID) CampaignRecipients_0003 FROM CampaignRecipients_0003 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignRecipientID) CampaignRecipients_0004 FROM CampaignRecipients_0004 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignRecipientID) CampaignRecipients_0005 FROM CampaignRecipients_0005 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignRecipientID) CampaignRecipients_0006 FROM CampaignRecipients_0006 WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (CampaignID) Campaigns FROM Campaigns WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (EmailID) AccountEmails FROM AccountEmails WITH (NOLOCK) WHERE  ServiceProviderID IN (SELECT ServiceProviderID FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ServiceProviderID) ServiceProviders FROM ServiceProviders WITH (NOLOCK) WHERE  AccountID = @AccountID 

Print 'Deleting_ServiceProviders_sp'

SELECT COUNT(ContactTourMapID)ContactTourMap   FROM ContactTourMap WHERE TourID IN (SELECT TourID FROM Tours WHERE Accountid = @Accountid )
SELECT COUNT(AuditId)ContactTourMap_Audit FROM ContactTourMap_Audit WHERE TourID IN (SELECT TourID FROM Tours WHERE Accountid = @Accountid )
SELECT COUNT(TourID)Tours FROM Tours WHERE Accountid = @Accountid
SELECT COUNT(AuditId)Tours_Audit FROM Tours_Audit WHERE Accountid = @Accountid

Print 'Deleting_Tours_sp'

SELECT COUNT (UserSettingsMapID) DashboardUserSettingsMap FROM DashboardUserSettingsMap WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (LoginAuditID) LoginAudit FROM LoginAudit WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (NotificationID) Notifications FROM Notifications WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserAddressMapID) UserAddressMap FROM UserAddressMap WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserProfileAuditID) UserProfileAudit FROM UserProfileAudit WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserSettingID) UserSettings FROM UserSettings WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserSocialMediaPostID) UserSocialMediaPosts FROM UserSocialMediaPosts WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WebVisitDailySummaryEmailAuditID) WebVisitDailySummaryEmailAudit FROM WebVisitDailySummaryEmailAudit WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WebVisitUserNotificationMapID) WebVisitUserNotificationMap FROM WebVisitUserNotificationMap WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
--SELECT COUNT (WorkFlowUserAssignmentActionID) WorkFlowUserAssignmentAction FROM WorkFlowUserAssignmentAction WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (TrackMessageID) WorkflowTrackMessages FROM [Workflow].[TrackMessages] WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (EmailID) AccountEmails FROM AccountEmails WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserActivityLogID) UserActivityLogs FROM UserActivityLogs WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ListID) StoreProcParamsList FROM StoreProcParamsList WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserSettingID) UserSettings FROM UserSettings WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (EmailID) Emails FROM Emails WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
--SELECT COUNT (WorkflowNotifyUserActionID) WorkflowNotifyUserAction FROM WorkflowNotifyUserAction WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserSocialMediaPostID) UserSocialMediaPosts FROM UserSocialMediaPosts WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (DailySummaryEmailAuditID) DailySummaryEmailAudit FROM DailySummaryEmailAudit WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ReceivedMailAuditID) ReceivedMailAudit FROM ReceivedMailAudit WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (UserID) Users FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID 

Print 'Deleting_Users_sp'

SELECT COUNT (WebVisitDailySummaryEmailAuditID) WebVisitDailySummaryEmailAudit FROM WebVisitDailySummaryEmailAudit WITH (NOLOCK) WHERE  WebAnalyticsProviderID IN (SELECT WebAnalyticsProviderID FROM WebAnalyticsProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WebVisitUserNotificationMapID) WebVisitUserNotificationMap FROM WebVisitUserNotificationMap WITH (NOLOCK) WHERE  WebAnalyticsProviderID IN (SELECT WebAnalyticsProviderID FROM WebAnalyticsProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WebAnalyticsProviderID) WebAnalyticsProviders FROM WebAnalyticsProviders WITH (NOLOCK) WHERE  WebAnalyticsProviderID IN (SELECT WebAnalyticsProviderID FROM WebAnalyticsProviders WITH (NOLOCK) WHERE  AccountID = @AccountID )
Print 'Deleting_WebAnalyticsProviders_sp'

SELECT COUNT (CampaignRecipientID) vCampaignRecipients FROM vCampaignRecipients WITH (NOLOCK) WHERE  WorkflowID IN (SELECT WorkflowID FROM Workflows WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowActionID) WorkflowActions FROM WorkflowActions WITH (NOLOCK) WHERE  WorkflowID IN (SELECT WorkflowID FROM Workflows WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (ContactWorkflowAuditID) ContactWorkflowAudit FROM ContactWorkflowAudit WITH (NOLOCK) WHERE  WorkflowID IN (SELECT WorkflowID FROM Workflows WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  WorkflowID IN (SELECT WorkflowID FROM Workflows WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (TrackActionID) WorkflowTrackActions FROM Workflow.TrackActions WITH (NOLOCK) WHERE  WorkflowID IN (SELECT WorkflowID FROM Workflows WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (WorkflowID) Workflows FROM Workflows WITH (NOLOCK) WHERE   AccountID = @AccountID 
Print 'Deleting_Workflows_sp'



SELECT COUNT (WorkflowTriggerID) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  FormID IN (SELECT FormID FROM Forms WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (SubmittedFormDataID) SubmittedFormData FROM SubmittedFormData WITH (NOLOCK) WHERE  FormID IN (SELECT FormID FROM Forms WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormFieldID) FormFields FROM FormFields WITH (NOLOCK) WHERE  FormID IN (SELECT FormID FROM Forms WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormTagID) FormTags FROM FormTags WITH (NOLOCK) WHERE  FormID IN (SELECT FormID FROM Forms WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormSubmissionID) FormSubmissions FROM FormSubmissions WITH (NOLOCK) WHERE  FormID IN (SELECT FormID FROM Forms WITH (NOLOCK) WHERE  AccountID = @AccountID )
SELECT COUNT (FormID) Forms FROM Forms WITH (NOLOCK) WHERE   AccountID = @AccountID 
Print 'Deleting_Forms_sp' 
 
 SELECT COUNT (ContactActionMapID) ContactActionMap FROM ContactActionMap WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) ContactActionMap_Audit FROM ContactActionMap_Audit WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (ActionTagMapID) ActionTagMap FROM ActionTagMap WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityActionMapID) OpportunityActionMap FROM OpportunityActionMap WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunityActionMap_Audit FROM OpportunityActionMap_Audit WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (TrackActionID) WorkflowTrackActions FROM Workflow.TrackActions WITH (NOLOCK) WHERE  ActionID IN (SELECT ActionID FROM Actions WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (ActionID) Actions FROM Actions WITH (NOLOCK) WHERE AccountID = @AccountID 
 Print 'Deleting_Actions_sp'

 SELECT COUNT (DocumentID) Documents FROM Documents WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
  SELECT COUNT (OpportunityRelationshipMapID) OpportunitiesRelationshipMap FROM OpportunitiesRelationshipMap WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunitiesRelationshipMap_Audit FROM OpportunitiesRelationshipMap_Audit WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityActionMapID) OpportunityActionMap FROM OpportunityActionMap WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunityActionMap_Audit FROM OpportunityActionMap_Audit WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityContactMapID) OpportunityContactMap FROM OpportunityContactMap WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunityContactMap_Audit FROM OpportunityContactMap_Audit WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityNoteMapID) OpportunityNoteMap FROM OpportunityNoteMap WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunityNoteMap_Audit FROM OpportunityNoteMap_Audit WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityTagMapID) OpportunityTagMap FROM OpportunityTagMap WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
  SELECT COUNT (AuditId) OpportunityTagMap_Audit FROM OpportunityTagMap_Audit WITH (NOLOCK) WHERE  OpportunityID IN (SELECT OpportunityID FROM Opportunities WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityId) Opportunities FROM Opportunities WITH (NOLOCK) WHERE   AccountID = @AccountID 
 SELECT COUNT (AuditId) Opportunities_Audit FROM Opportunities_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 

 Print 'Deleting_Opportunities_sp'


select COUNT(a. AddressID) UserAddressMap from Addresses a inner join UserAddressMap ua on ua.AddressID = a. AddressID WHERE UA.UserID IN (SELECT UserID FROM Users WHERE AccountID = @AccountID)
 select COUNT(a. AddressID) AccountAddressMap from Addresses a inner join  AccountAddressMap ua on ua.AddressID = a. AddressID WHERE  AccountID = @Accountid
 select COUNT(a. AddressID) Addresses from Addresses a inner join  ContactAddressMap ua on ua.AddressID = a. AddressID WHERE UA.ContactID IN (SELECT ContactID FROM ContactS WHERE AccountID = @AccountID)
 select COUNT(a. AddressID) ContactAddressMap from ContactAddressMap a inner join  Addresses AA ON AA.AddressID = A.AddressID  INNER JOIN Contacts C ON A.contactID = C.CONTACTid  WHERE C.Accountid = @AccountID
  select COUNT(a. AddressID) Addresses from Addresses a inner join  ContactAddressMap AA ON AA.AddressID = A.AddressID  INNER JOIN Contacts C ON aA.contactID = C.CONTACTid  WHERE C.Accountid = @AccountID
 --Print 'Deleting_Addresses_sp'

 SELECT COUNT (WorkflowTriggerId) WorkflowTriggers FROM WorkflowTriggers WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
  SELECT COUNT (FormTagId) FormTags FROM FormTags WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (WorkflowTagActionId) WorkflowTagAction FROM WorkflowTagAction WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (ActionTagMapId) ActionTagMap FROM ActionTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (CampaignContactTagMapId) CampaignContactTagMap FROM CampaignContactTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (CampaignTagMapId) CampaignTagMap FROM CampaignTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (NoteTagMapId) NoteTagMap FROM NoteTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
  SELECT COUNT (AuditId) NoteTagMap_Audit FROM NoteTagMap_Audit WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (ImportTagMapId) ImportTagMap FROM ImportTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (LeadAdapterTagMapId) LeadAdapterTagMap FROM LeadAdapterTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (OpportunityTagMapId) OpportunityTagMap FROM OpportunityTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (AuditId) OpportunityTagMap_Audit FROM OpportunityTagMap_Audit WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (ContactTagMapId) ContactTagMap FROM ContactTagMap WITH (NOLOCK) WHERE  TagID IN (SELECT TagID FROM TagS WITH (NOLOCK) WHERE  AccountID = @AccountID )
 SELECT COUNT (TagId) Tags FROM Tags WITH (NOLOCK) WHERE   AccountID = @AccountID 
 SELECT COUNT (AuditId) Tags_Audit FROM Tags_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
  Print 'Deleting_Tags_sp'

  SELECT COUNT (LeadAdapterJobLogDetailId) LeadAdapterJobLogDetails FROM LeadAdapterJobLogDetails WITH (NOLOCK) WHERE  LeadAdapterJobLogID IN (SELECT LeadAdapterJobLogID FROM LeadAdapterJobLogs WITH (NOLOCK) WHERE LeadAdapterAndAccountMapID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID ) )
    SELECT COUNT (ImportTagMapID) ImportTagMap FROM ImportTagMap WITH (NOLOCK) WHERE  LeadAdapterJobLogID IN (SELECT LeadAdapterJobLogID FROM LeadAdapterJobLogs WITH (NOLOCK) WHERE  LeadAdapterAndAccountMapID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID ) )
  SELECT COUNT (LeadAdapterJobLogID) LeadAdapterJobLogs FROM LeadAdapterJobLogs WITH (NOLOCK) WHERE  LeadAdapterAndAccountMapID IN (SELECT LeadAdapterAndAccountMapID FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE  AccountID = @AccountID )
  Print 'Deleting_LeadAdapterJobLogs_sp'
 

SELECT COUNT (*) AccountSettings FROM AccountSettings WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) AccountAddressMap FROM AccountAddressMap WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) AccountDataAccessPermissions FROM AccountDataAccessPermissions WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) CampaignTemplates FROM CampaignTemplates WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) AccountEmails FROM AccountEmails WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Actions FROM Actions WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Actions_Audit FROM Actions_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ActiveContacts FROM ActiveContacts WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) BulkOperations FROM BulkOperations WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ClassicCampaigns FROM ClassicCampaigns WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Communities FROM Communities WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ContactEmails FROM ContactEmails WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ContactEmails_Audit FROM ContactEmails_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ContactPhoneNumbers FROM ContactPhoneNumbers WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Contacts FROM Contacts WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Contacts_Audit FROM Contacts_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) CustomFieldTabs FROM CustomFieldTabs WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) DropdownValues FROM DropdownValues WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Emails FROM Emails WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Fields FROM Fields WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Forms FROM Forms WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ImportDataSettings FROM ImportDataSettings WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) LeadAdapterAndAccountMap FROM LeadAdapterAndAccountMap WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) LeadScoreMessages FROM LeadScoreMessages WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) LeadScoreRules FROM LeadScoreRules WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) OpportunityStageGroups FROM OpportunityStageGroups WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Reports FROM Reports WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Roles FROM Roles WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) SearchDefinitions FROM SearchDefinitions WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ServiceProviders FROM ServiceProviders WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) StoreProcExecutionResults FROM StoreProcExecutionResults WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) StoreProcParamsList FROM StoreProcParamsList WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) SubscriptionModuleMap FROM SubscriptionModuleMap WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ThirdPartyClients FROM ThirdPartyClients WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Tours FROM Tours WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Users FROM Users WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) WebAnalyticsProviders FROM WebAnalyticsProviders WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) WebVisitUserNotificationMap FROM WebVisitUserNotificationMap WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) SpamIPAddresses FROM SpamIPAddresses WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) SpamValidators FROM SpamValidators WITH (NOLOCK) WHERE   AccountID = @AccountID
SELECT COUNT (*) Workflows FROM Workflows WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Images FROM Images WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) ImportContactData FROM ImportContactData WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Opportunities FROM Opportunities WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Opportunities_Audit FROM Opportunities_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Notes FROM Notes WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Notes_Audit FROM Notes_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Tags FROM Tags WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Tags_Audit FROM Tags_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) MarketingMessageAccountMap FROM MarketingMessageAccountMap WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) UserSettings FROM UserSettings WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) SubmittedFormData FROM SubmittedFormData WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Tours_Audit FROM Tours_Audit WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) UserActivityLogs FROM UserActivityLogs WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Campaigns FROM Campaigns WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) WorkflowTrackMessages FROM Workflow.TrackMessages WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Roles FROM Roles WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Notes FROM Notes WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Campaigns FROM Campaigns WITH (NOLOCK) WHERE   AccountID = @AccountID 
SELECT COUNT (*) Accounts FROM Accounts WITH (NOLOCK) WHERE   AccountID = @AccountID 
 Print 'Deleting_Accounts_sp'


 SET NOCOUNT off
end


/*
EXEC [Deleting_Accounts_Count] 60
*/
