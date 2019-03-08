
CREATE PROCEDURE [dbo].[Deleting_Accounts_Count_Test] 
(
@Accountid int = 0,
@CountStatus int /* Count=0 means before excution or 1 means after deleting */
)
AS
BEGIN
  SET NOCOUNT ON

  CREATE TABLE #ObjCountNum (
    Id int IDENTITY (1, 1),
    ObjectName varchar(50),
    RecordsCount int,
    StatusOfCount varchar(70)
  )
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'VCampaignStatistics' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM VCampaignStatistics WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignLinks ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignLinks WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'WorkflowCampaignActionLinks ' ObjectName,
      COUNT(LinkID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowCampaignActionLinks WITH (NOLOCK)
    WHERE LinkID IN (SELECT
      CampaignLinkID
    FROM CampaignLinks cl
    INNER JOIN Campaigns c
      ON cl.CampaignID = c.CampaignID
    WHERE c.AccountID = @AccountID)
  PRINT 'Deleting_CampaignLinkS_sp-count'
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'vCampaignRecipients' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM vCampaignRecipients WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'VCampaignStatistics ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM VCampaignStatistics WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'ResentCampaigns ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ResentCampaigns WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignTagMap ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignTagMap WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignContactTagMap ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignContactTagMap WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'UserSocialMediaPosts ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSocialMediaPosts WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignLinks ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignLinks WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'WorkflowCampaignActions ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowCampaignActions WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignSearchDefinitionMap ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignSearchDefinitionMap WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignAnalytics ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignAnalytics WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CampaignLogDetails ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignLogDetails WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'MomentaryCampaignRecipients ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM MomentaryCampaignRecipients WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'ClassicCampaigns ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ClassicCampaigns WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Campaigns ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Campaigns WITH (NOLOCK)
    WHERE CampaignID IN (SELECT
      CampaignID
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  PRINT 'Deleting_Campaigns_sp-count'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignTemplateS ' ObjectName,
      COUNT(CampaignTemplateID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignTemplateS WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_CampaignTemplates_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactCommunityMap ' ObjectName,
      COUNT(ContactCommunityMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactCommunityMap WITH (NOLOCK)
    WHERE CommunityID IN (SELECT
      CommunityID
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours ' ObjectName,
      COUNT(TourID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours WITH (NOLOCK)
    WHERE CommunityID IN (SELECT
      CommunityID
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours_Audit ' ObjectName,
      COUNT(AuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours_Audit WITH (NOLOCK)
    WHERE CommunityID IN (SELECT
      CommunityID
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Communities ' ObjectName,
      COUNT(CommunityID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_Community_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmailAudit  ' ObjectName,
      COUNT(ContactEmailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmailAudit WITH (NOLOCK)
    WHERE ContactEmailID IN (SELECT
      ContactEmailID
    FROM ContactEmails
    WHERE Accountid = @Accountid)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails ' ObjectName,
      COUNT(ContactEmailID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails WITH (NOLOCK)
    WHERE Accountid = @Accountid
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails_Audit WITH (NOLOCK)
    WHERE Accountid = @Accountid

  PRINT 'Deleting_ContactEmails_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTextMessageAudit ' ObjectName,
      COUNT(ContactTextMessageAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTextMessageAudit WITH (NOLOCK)
    WHERE ContactPhoneNumberID IN (SELECT
      ContactPhoneNumberID
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactPhoneNumbers ' ObjectName,
      COUNT(ContactPhoneNumberID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactPhoneNumbers WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_ContactPhoneNumbers_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportContactData ' ObjectName,
      COUNT(ImportContactDataID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportContactData WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportContactLogs ' ObjectName,
      COUNT(ImportContactLogID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportContactLogs WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTrackMessages ' ObjectName,
      COUNT(TrackMessageID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM [Workflow].[TrackMessages] WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunitiesRelationshipMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunitiesRelationshipMap_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScoreMessages ' ObjectName,
      COUNT(LeadScoreMessageID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScoreMessages WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactActionMap ' ObjectName,
      COUNT(ContactActionMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactActionMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactActionMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactActionMap_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CommunicationTracker ' ObjectName,
      COUNT(CommunicationTrackerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CommunicationTracker WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactNoteMap ' ObjectName,
      COUNT(ContactNoteMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactNoteMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityContactMap ' ObjectName,
      COUNT(OpportunityContactMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityContactMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityContactMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityContactMap_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTourMap ' ObjectName,
      COUNT(ContactTourMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTourMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTourMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTourMap_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'BulkContactData ' ObjectName,
      COUNT(BulkContactDataId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM BulkContactData WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactRelationshipMap ' ObjectName,
      COUNT(ContactRelationshipMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactRelationshipMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactRelationshipMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactRelationshipMap_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormSubmissions ' ObjectName,
      COUNT(FormSubmissionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormSubmissions WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails ' ObjectName,
      COUNT(ContactEmailId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails_Audit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Documents ' ObjectName,
      COUNT(DocumentId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Documents WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScores ' ObjectName,
      COUNT(LeadScoreId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScores WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactCommunityMap ' ObjectName,
      COUNT(ContactCommunityMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactCommunityMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'MomentaryCampaignRecipients ' ObjectName,
      COUNT(MomentaryRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM MomentaryCampaignRecipients WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactIPAddresses ' ObjectName,
      COUNT(ContactIPAddressid) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactIPAddresses WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactLeadAdapterMap ' ObjectName,
      COUNT(ContactLeadAdapterMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactLeadAdapterMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactAddressMap ' ObjectName,
      COUNT(ContactAddressMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactAddressMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactPhoneNumbers ' ObjectName,
      COUNT(ContactPhoneNumberId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactPhoneNumbers WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactCustomFieldMap ' ObjectName,
      COUNT(ContactCustomFieldMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactCustomFieldMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactWebVisits ' ObjectName,
      COUNT(ContactWebVisitId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactWebVisits WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTagMap ' ObjectName,
      COUNT(ContactTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTagMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactLeadSourceMap ' ObjectName,
      COUNT(ContactLeadSourceMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactLeadSourceMap WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'DocRepositorys ' ObjectName,
      COUNT(DocumentID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM DocRepositorys WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactWorkflowAudit ' ObjectName,
      COUNT(ContactWorkflowAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactWorkflowAudit WITH (NOLOCK)
    WHERE ContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ReceivedMailAudit ' ObjectName,
      COUNT(ReceivedMailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ReceivedMailAudit WITH (NOLOCK)
    WHERE SentByContactID IN (SELECT
      ContactID
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Contacts ' ObjectName,
      COUNT(ContactId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Contacts_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Contacts_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Contacts_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'Fields ' ObjectName,
      COUNT(FieldID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Fields WITH (NOLOCK)
    WHERE CustomFieldSectionID IN (SELECT
      CustomFieldSectionID
    FROM CustomFieldSections WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)

    SELECT
      'CustomFieldSections ' ObjectName,
      COUNT(CustomFieldSectionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CustomFieldSections WITH (NOLOCK)
    WHERE TabID IN (SELECT
      CustomFieldTabID
    FROM CustomFieldTabs WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  PRINT 'Deleting_CustomFieldSections_sp'


  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchFilters ' ObjectName,
      COUNT(SearchFilterID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchFilters WITH (NOLOCK)
    WHERE DropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SubscriptionDefaultDropdownValueMap ' ObjectName,
      COUNT(SubscriptionDefaultDropdownValueMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SubscriptionDefaultDropdownValueMap WITH (NOLOCK)
    WHERE DropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowContactFieldAction ' ObjectName,
      COUNT(WorkflowContactFieldActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowContactFieldAction WITH (NOLOCK)
    WHERE DropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityStageGroups' ObjectName,
      COUNT(OpportunityStageGroupID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityStageGroups WITH (NOLOCK)
    WHERE DropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Addresses ' ObjectName,
      COUNT(AddressID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Addresses WITH (NOLOCK)
    WHERE AddressTypeID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours ' ObjectName,
      COUNT(TourID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours WITH (NOLOCK)
    WHERE TourType IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours ' ObjectName,
      COUNT(TourID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours WITH (NOLOCK)
    WHERE CommunityID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Forms' ObjectName,
      COUNT(FormID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Forms WITH (NOLOCK)
    WHERE LeadSourceID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Contacts ' ObjectName,
      COUNT(ContactID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Contacts WITH (NOLOCK)
    WHERE LifecycleStage IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE OpportunityStageID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE LifecycleDropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkFlowLifeCycleAction ' ObjectName,
      COUNT(WorkFlowLifeCycleActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkFlowLifeCycleAction WITH (NOLOCK)
    WHERE LifecycleDropdownValueID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Opportunities  ' ObjectName,
      COUNT(OpportunityID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Opportunities WITH (NOLOCK)
    WHERE StageID IN (SELECT
      DropdownValueID
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'DropdownValues ' ObjectName,
      COUNT(DropdownValueID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_DropdownValues_sp'
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CustomFieldValueOptions ' ObjectName,
      COUNT(CustomFieldValueOptionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CustomFieldValueOptions WITH (NOLOCK)
    WHERE CustomFieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowContactFieldAction ' ObjectName,
      COUNT(WorkflowContactFieldActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowContactFieldAction WITH (NOLOCK)
    WHERE FieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormFields ' ObjectName,
      COUNT(FormFieldID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormFields WITH (NOLOCK)
    WHERE FieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchFilters ' ObjectName,
      COUNT(SearchFilterID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchFilters WITH (NOLOCK)
    WHERE FieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AVColumnPreferences ' ObjectName,
      COUNT(AVColumnPreferenceID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AVColumnPreferences WITH (NOLOCK)
    WHERE FieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportCustomData ' ObjectName,
      COUNT(ImportCustomDataID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportCustomData WITH (NOLOCK)
    WHERE FieldID IN (SELECT
      FieldID
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Fields ' ObjectName,
      COUNT(FieldID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_Fields_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterTagMap ' ObjectName,
      COUNT(LeadAdapterTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterTagMap WITH (NOLOCK)
    WHERE LeadAdapterID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterJobLogs ' ObjectName,
      COUNT(LeadAdapterJobLogID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterJobLogs WITH (NOLOCK)
    WHERE LeadAdapterAndAccountMapID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE LeadAdapterID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactLeadAdapterMap ' ObjectName,
      COUNT(ContactLeadAdapterMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactLeadAdapterMap WITH (NOLOCK)
    WHERE LeadAdapterID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterAndAccountMap ' ObjectName,
      COUNT(LeadAdapterAndAccountMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_LeadAdapterAndAccountMap_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScores ' ObjectName,
      COUNT(LeadScoreID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScores WITH (NOLOCK)
    WHERE LeadScoreRuleID IN (SELECT
      LeadScoreRuleID
    FROM LeadScoreRules WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScoreConditionValues ' ObjectName,
      COUNT(LeadScoreConditionValueID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScoreConditionValues WITH (NOLOCK)
    WHERE LeadScoreRuleID IN (SELECT
      LeadScoreRuleID
    FROM LeadScoreRules WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScoreRules ' ObjectName,
      COUNT(LeadScoreRuleID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScoreRules WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_LeadScoreRules_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'NoteTagMap ' ObjectName,
      COUNT(NoteTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM NoteTagMap WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityNoteMap ' ObjectName,
      COUNT(OpportunityNoteMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityNoteMap WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityNoteMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityNoteMap_Audit WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'NoteTagMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM NoteTagMap_Audit WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactNoteMap ' ObjectName,
      COUNT(ContactNoteMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactNoteMap WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactNoteMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactNoteMap_Audit WITH (NOLOCK)
    WHERE NoteID IN (SELECT
      NoteID
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notes ' ObjectName,
      COUNT(NoteID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notes_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notes_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Notes_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'RoleModuleMap ' ObjectName,
      COUNT(RoleModuleMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM RoleModuleMap WITH (NOLOCK)
    WHERE RoleID IN (SELECT
      RoleID
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Users' ObjectName,
      COUNT(UserID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Users WITH (NOLOCK)
    WHERE RoleID IN (SELECT
      RoleID
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'BulkOperations ' ObjectName,
      COUNT(BulkOperationID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM BulkOperations WITH (NOLOCK)
    WHERE RoleID IN (SELECT
      RoleID
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Roles ' ObjectName,
      COUNT(RoleID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Roles_sp'


  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchFilters ' ObjectName,
      COUNT(SearchFilterID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchFilters WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchDefinitionTagMap ' ObjectName,
      COUNT(SearchDefinitionTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchDefinitionTagMap WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchDefinitionSubscriptionMap ' ObjectName,
      COUNT(SearchDefinitionSubscriptionMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchDefinitionSubscriptionMap WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignSearchDefinitionMap ' ObjectName,
      COUNT(CampaignSearchDefinitionMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignSearchDefinitionMap WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'BulkOperations ' ObjectName,
      COUNT(BulkOperationID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM BulkOperations WITH (NOLOCK)
    WHERE SearchDefinitionID IN (SELECT
      SearchDefinitionID
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchDefinitions ' ObjectName,
      COUNT(SearchDefinitionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_SearchDefinitions_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0001 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0001 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0002 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0002 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0003 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0003 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0004 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0004 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0005 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0005 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignRecipients_0006 ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignRecipients_0006 WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Campaigns ' ObjectName,
      COUNT(CampaignID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Campaigns WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountEmails ' ObjectName,
      COUNT(EmailID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountEmails WITH (NOLOCK)
    WHERE ServiceProviderID IN (SELECT
      ServiceProviderID
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ServiceProviders ' ObjectName,
      COUNT(ServiceProviderID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_ServiceProviders_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTourMap ' ObjectName,
      COUNT(ContactTourMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTourMap
    WHERE TourID IN (SELECT
      TourID
    FROM Tours
    WHERE Accountid = @Accountid)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTourMap_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTourMap_Audit
    WHERE TourID IN (SELECT
      TourID
    FROM Tours
    WHERE Accountid = @Accountid)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours' ObjectName,
      COUNT(TourID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours
    WHERE Accountid = @Accountid
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours_Audit ' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours_Audit
    WHERE Accountid = @Accountid

  PRINT 'Deleting_Tours_sp'


  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'DashboardUserSettingsMap ' ObjectName,
      COUNT(UserSettingsMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM DashboardUserSettingsMap WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LoginAudit ' ObjectName,
      COUNT(LoginAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LoginAudit WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notifications ' ObjectName,
      COUNT(NotificationID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notifications WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserAddressMap ' ObjectName,
      COUNT(UserAddressMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserAddressMap WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserProfileAudit ' ObjectName,
      COUNT(UserProfileAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserProfileAudit WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserSettings ' ObjectName,
      COUNT(UserSettingID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSettings WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserSocialMediaPosts ' ObjectName,
      COUNT(UserSocialMediaPostID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSocialMediaPosts WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebVisitDailySummaryEmailAudit ' ObjectName,
      COUNT(WebVisitDailySummaryEmailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebVisitDailySummaryEmailAudit WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebVisitUserNotificationMap ' ObjectName,
      COUNT(WebVisitUserNotificationMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebVisitUserNotificationMap WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  --INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
  --  SELECT
  --    'WorkFlowUserAssignmentAction ' ObjectName,
  --    COUNT(WorkFlowUserAssignmentActionID) RecordsCount,
  --    (CASE
  --      WHEN @CountStatus = 0 THEN 'Before deleting records count'
  --      ELSE 'After deleting records count'
  --    END) StatusOfCount
  --  FROM WorkFlowUserAssignmentAction WITH (NOLOCK)
  --  WHERE UserID IN (SELECT
  --    UserID
  --  FROM Users WITH (NOLOCK)
  --  WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTrackMessages' ObjectName,
      COUNT(TrackMessageID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM [Workflow].[TrackMessages] WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountEmails ' ObjectName,
      COUNT(EmailID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountEmails WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserActivityLogs ' ObjectName,
      COUNT(UserActivityLogID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserActivityLogs WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'StoreProcParamsList ' ObjectName,
      COUNT(ListID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM StoreProcParamsList WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserSettings ' ObjectName,
      COUNT(UserSettingID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSettings WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Emails ' ObjectName,
      COUNT(EmailID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Emails WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  --INSERT INTO #ObjCountNum (ObjectName,RecordsCount,StatusOfCount)   
  --SELECT 'WorkflowNotifyUserAction ' ObjectName,COUNT(WorkflowNotifyUserActionID) RecordsCount,(Case when @CountStatus=0 then 'Before deleting records count' else 'After deleting records count' end ) StatusOfCount
  -- FROM WorkflowNotifyUserAction WITH (NOLOCK) WHERE  UserID IN (SELECT UserID FROM Users WITH (NOLOCK) WHERE  AccountID = @AccountID )
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserSocialMediaPosts ' ObjectName,
      COUNT(UserSocialMediaPostID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSocialMediaPosts WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'DailySummaryEmailAudit ' ObjectName,
      COUNT(DailySummaryEmailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM DailySummaryEmailAudit WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ReceivedMailAudit ' ObjectName,
      COUNT(ReceivedMailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ReceivedMailAudit WITH (NOLOCK)
    WHERE UserID IN (SELECT
      UserID
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Users ' ObjectName,
      COUNT(UserID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_Users_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebVisitDailySummaryEmailAudit ' ObjectName,
      COUNT(WebVisitDailySummaryEmailAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebVisitDailySummaryEmailAudit WITH (NOLOCK)
    WHERE WebAnalyticsProviderID IN (SELECT
      WebAnalyticsProviderID
    FROM WebAnalyticsProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebVisitUserNotificationMap ' ObjectName,
      COUNT(WebVisitUserNotificationMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebVisitUserNotificationMap WITH (NOLOCK)
    WHERE WebAnalyticsProviderID IN (SELECT
      WebAnalyticsProviderID
    FROM WebAnalyticsProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebAnalyticsProviders ' ObjectName,
      COUNT(WebAnalyticsProviderID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebAnalyticsProviders WITH (NOLOCK)
    WHERE WebAnalyticsProviderID IN (SELECT
      WebAnalyticsProviderID
    FROM WebAnalyticsProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  PRINT 'Deleting_WebAnalyticsProviders_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'vCampaignRecipients ' ObjectName,
      COUNT(CampaignRecipientID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM vCampaignRecipients WITH (NOLOCK)
    WHERE WorkflowID IN (SELECT
      WorkflowID
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowActions ' ObjectName,
      COUNT(WorkflowActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowActions WITH (NOLOCK)
    WHERE WorkflowID IN (SELECT
      WorkflowID
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactWorkflowAudit ' ObjectName,
      COUNT(ContactWorkflowAuditID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactWorkflowAudit WITH (NOLOCK)
    WHERE WorkflowID IN (SELECT
      WorkflowID
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE WorkflowID IN (SELECT
      WorkflowID
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTrackActions ' ObjectName,
      COUNT(TrackActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Workflow.TrackActions WITH (NOLOCK)
    WHERE WorkflowID IN (SELECT
      WorkflowID
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Workflows ' ObjectName,
      COUNT(WorkflowID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Workflows_sp'



  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers ' ObjectName,
      COUNT(WorkflowTriggerID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE FormID IN (SELECT
      FormID
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SubmittedFormData ' ObjectName,
      COUNT(SubmittedFormDataID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SubmittedFormData WITH (NOLOCK)
    WHERE FormID IN (SELECT
      FormID
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormFields ' ObjectName,
      COUNT(FormFieldID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormFields WITH (NOLOCK)
    WHERE FormID IN (SELECT
      FormID
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormTags ' ObjectName,
      COUNT(FormTagID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormTags WITH (NOLOCK)
    WHERE FormID IN (SELECT
      FormID
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormSubmissions ' ObjectName,
      COUNT(FormSubmissionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormSubmissions WITH (NOLOCK)
    WHERE FormID IN (SELECT
      FormID
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Forms ' ObjectName,
      COUNT(FormID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Forms_sp'


  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactActionMap' ObjectName,
      COUNT(ContactActionMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactActionMap WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactActionMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactActionMap_Audit WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ActionTagMap' ObjectName,
      COUNT(ActionTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ActionTagMap WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityActionMap' ObjectName,
      COUNT(OpportunityActionMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityActionMap WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityActionMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityActionMap_Audit WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTrackActions' ObjectName,
      COUNT(TrackActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Workflow.TrackActions WITH (NOLOCK)
    WHERE ActionID IN (SELECT
      ActionID
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Actions' ObjectName,
      COUNT(ActionID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Actions_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Documents' ObjectName,
      COUNT(DocumentID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Documents WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityRelationshipMap' ObjectName,
      COUNT(OpportunityRelationshipMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunitiesRelationshipMap WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunitiesRelationshipMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunitiesRelationshipMap_Audit WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityActionMap' ObjectName,
      COUNT(OpportunityActionMapid) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityActionMap WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityActionMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityActionMap_Audit WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityContactMap' ObjectName,
      COUNT(OpportunityContactMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityContactMap WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityContactMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityContactMap_Audit WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityNoteMap' ObjectName,
      COUNT(OpportunityNoteMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityNoteMap WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityNoteMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityNoteMap_Audit WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityTagMap' ObjectName,
      COUNT(OpportunityTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityTagMap WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityTagMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityTagMap_Audit WITH (NOLOCK)
    WHERE OpportunityID IN (SELECT
      OpportunityID
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Opportunities' ObjectName,
      COUNT(OpportunityId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Opportunities_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Opportunities_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID

  PRINT 'Deleting_Opportunities_sp'

  --INSERT INTO #ObjCountNum (ObjectName,RecordsCount,StatusOfCount)   SELECT 'UserAddressMap' ObjectName,COUNT(UserAddressMapID) RecordsCount,(Case when @CountStatus=0 then 'Before deleting records count' else 'After deleting records count' end ) StatusOfCount FROM UserAddressMap WITH (NOLOCK) WHERE  AddressID IN (SELECT AddressID FROM Addresses WITH (NOLOCK) WHERE  AccountID = @AccountID )
  -- INSERT INTO #ObjCountNum (ObjectName,RecordsCount,StatusOfCount)   SELECT 'AccountAddressMap' ObjectName,COUNT(AddressMapID) RecordsCount,(Case when @CountStatus=0 then 'Before deleting records count' else 'After deleting records count' end ) StatusOfCount FROM AccountAddressMap WITH (NOLOCK) WHERE  AddressID IN (SELECT AddressID FROM Addresses WITH (NOLOCK) WHERE  AccountID = @AccountID )
  --INSERT INTO #ObjCountNum (ObjectName,RecordsCount,StatusOfCount)   SELECT 'ContactAddressMap' ObjectName,COUNT(ContactAddressMapID) RecordsCount,(Case when @CountStatus=0 then 'Before deleting records count' else 'After deleting records count' end ) StatusOfCount FROM ContactAddressMap WITH (NOLOCK) WHERE  AddressID IN (SELECT AddressID FROM Addresses WITH (NOLOCK) WHERE  AccountID = @AccountID )
  --INSERT INTO #ObjCountNum (ObjectName,RecordsCount,StatusOfCount)   SELECT 'Addresses' ObjectName,COUNT(AddressID) RecordsCount,(Case when @CountStatus=0 then 'Before deleting records count' else 'After deleting records count' end ) StatusOfCount FROM Addresses WITH (NOLOCK) WHERE  AddressID IN (SELECT AddressID FROM Addresses WITH (NOLOCK) WHERE  AccountID = @AccountID )
  --Print 'Deleting_Addresses_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTriggers' ObjectName,
      COUNT(WorkflowTriggerId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTriggers WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'FormTags' ObjectName,
      COUNT(FormTagId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM FormTags WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTagAction' ObjectName,
      COUNT(WorkflowTagActionId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WorkflowTagAction WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ActionTagMap' ObjectName,
      COUNT(ActionTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ActionTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignContactTagMap' ObjectName,
      COUNT(CampaignContactTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignContactTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignTagMap' ObjectName,
      COUNT(CampaignTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'NoteTagMap' ObjectName,
      COUNT(NoteTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM NoteTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'NoteTagMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM NoteTagMap_Audit WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportTagMap' ObjectName,
      COUNT(ImportTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterTagMap' ObjectName,
      COUNT(LeadAdapterTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityTagMap' ObjectName,
      COUNT(OpportunityTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityTagMap_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityTagMap_Audit WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactTagMap' ObjectName,
      COUNT(ContactTagMapId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactTagMap WITH (NOLOCK)
    WHERE TagID IN (SELECT
      TagID
    FROM TagS WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tags' ObjectName,
      COUNT(TagId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tags WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tags_Audit' ObjectName,
      COUNT(AuditId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tags_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Tags_sp'

  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterJobLogDetails' ObjectName,
      COUNT(LeadAdapterJobLogDetailId) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterJobLogDetails WITH (NOLOCK)
    WHERE LeadAdapterJobLogID IN (SELECT
      LeadAdapterJobLogID
    FROM LeadAdapterJobLogs WITH (NOLOCK)
    WHERE LeadAdapterAndAccountMapID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID))
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportTagMap' ObjectName,
      COUNT(ImportTagMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportTagMap WITH (NOLOCK)
    WHERE LeadAdapterJobLogID IN (SELECT
      LeadAdapterJobLogID
    FROM LeadAdapterJobLogs WITH (NOLOCK)
    WHERE LeadAdapterAndAccountMapID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID))
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterJobLogs' ObjectName,
      COUNT(LeadAdapterJobLogID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterJobLogs WITH (NOLOCK)
    WHERE LeadAdapterAndAccountMapID IN (SELECT
      LeadAdapterAndAccountMapID
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID)
  PRINT 'Deleting_LeadAdapterJobLogs_sp'
-------------------------------------------------------------------------------------------------
/* Addresses sp */

 INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserAddressMap' ObjectName,
      COUNT(UserAddressMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserAddressMap u WITH (NOLOCK)
    inner join Addresses ua on ua.AddressID = u. AddressID WHERE u.UserID IN (SELECT UserID FROM Users WHERE AccountID = @AccountID)


 INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountAddressMap' ObjectName,
      COUNT(AccountAddressMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountAddressMap u WITH (NOLOCK)
    inner join  Addresses ua on ua.AddressID = u. AddressID WHERE  AccountID = @Accountid


 INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactAddressMap' ObjectName,
      COUNT(ContactAddressMapID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactAddressMap u WITH (NOLOCK)
   inner join  Addresses AA ON AA.AddressID = u.AddressID  INNER JOIN Contacts C ON u.contactID = C.CONTACTid  WHERE C.Accountid = @AccountID
 
 INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Addresses' ObjectName,
      COUNT(u.AddressID) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Addresses u WITH (NOLOCK)
   inner join  ContactAddressMap AA ON AA.AddressID = u.AddressID  INNER JOIN Contacts C ON aA.contactID = C.CONTACTid  WHERE C.Accountid = @AccountID
----------------------------------------------------------------------------------------------------------------------
/*  Accounts */
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountSettings' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountSettings WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountAddressMap' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountAddressMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountDataAccessPermissions' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountDataAccessPermissions WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CampaignTemplates' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CampaignTemplates WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'AccountEmails' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM AccountEmails WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Actions' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Actions WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Actions_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Actions_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ActiveContacts' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ActiveContacts WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'BulkOperations' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM BulkOperations WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ClassicCampaigns' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ClassicCampaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Communities' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Communities WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactEmails_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactEmails_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ContactPhoneNumbers' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ContactPhoneNumbers WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Contacts' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Contacts WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Contacts_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Contacts_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'CustomFieldTabs' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM CustomFieldTabs WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'DropdownValues' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM DropdownValues WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Emails' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Emails WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Fields' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Fields WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Forms' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Forms WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportDataSettings' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportDataSettings WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadAdapterAndAccountMap' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadAdapterAndAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScoreMessages' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScoreMessages WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'LeadScoreRules' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM LeadScoreRules WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'OpportunityStageGroups' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM OpportunityStageGroups WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Reports' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Reports WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Roles' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SearchDefinitions' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SearchDefinitions WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ServiceProviders' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ServiceProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'StoreProcExecutionResults' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM StoreProcExecutionResults WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'StoreProcParamsList' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM StoreProcParamsList WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SubscriptionModuleMap' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SubscriptionModuleMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ThirdPartyClients' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ThirdPartyClients WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tours' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Users' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Users WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebAnalyticsProviders' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebAnalyticsProviders WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WebVisitUserNotificationMap' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM WebVisitUserNotificationMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SpamIPAddresses' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SpamIPAddresses WITH (NOLOCK)
    WHERE AccountID = @AccountID

	  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SpamValidators' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SpamValidators WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Workflows' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Workflows WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Images' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Images WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'ImportContactData' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM ImportContactData WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Opportunities' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Opportunities WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Opportunities_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Opportunities_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notes' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notes_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notes_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tags' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tags WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Tags_Audit' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tags_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'MarketingMessageAccountMap' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM MarketingMessageAccountMap WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserSettings' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserSettings WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SubmittedFormData' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM SubmittedFormData WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'SubmittedFormData' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Tours_Audit WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'UserActivityLogs' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM UserActivityLogs WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Campaigns' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'WorkflowTrackMessages' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Workflow.TrackMessages WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Roles' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Roles WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Notes' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Notes WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Campaigns' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Campaigns WITH (NOLOCK)
    WHERE AccountID = @AccountID
  INSERT INTO #ObjCountNum (ObjectName, RecordsCount, StatusOfCount)
    SELECT
      'Accounts' ObjectName,
      COUNT(*) RecordsCount,
      (CASE
        WHEN @CountStatus = 0 THEN 'Before deleting records count'
        ELSE 'After deleting records count'
      END) StatusOfCount
    FROM Accounts WITH (NOLOCK)
    WHERE AccountID = @AccountID
  PRINT 'Deleting_Accounts_sp'


  SELECT
    Id,
    ObjectName,
    RecordsCount,
    StatusOfCount
  FROM #ObjCountNum


  SET NOCOUNT OFF
END


/*
EXEC [Deleting_Accounts_Count_test] 60,0
*/