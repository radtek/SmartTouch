
CREATE PROCEDURE [dbo].[ContactEmailEngagementSummary]
	@contactId INT,
	@accountId INT
AS
BEGIN
		   DECLARE @emailsSentCount INT
		   DECLARE @campaignSentCount INT
		   DECLARE @workflowCount INT

		   DECLARE @contactEmailEngagementTable TABLE (WorkflowsCount INT,CampaignsCount INT,EmailsCount INT)

	       SELECT @emailsSentCount = COUNT(1) FROM dbo.ContactEmailAudit(NOLOCK) CEA
				 JOIN dbo.ContactEmails(NOLOCK) CE ON CEA.ContactEmailID = CE.ContactEmailID
				 JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CE.ContactID
				 JOIN EnterpriseCommunication.dbo.SentMailDetails(NOLOCK) SMD ON SMD.RequestGuid = CEA.RequestGuid
				 JOIN EnterpriseCommunication.dbo.SentMails(NOLOCK) SM ON SM.RequestGuid = SMD.RequestGuid
			WHERE CEA.[Status] = 1 AND C.ContactID = @contactId AND C.AccountID = @accountId
				AND CE.ContactID = @contactId AND CE.AccountID = @accountId AND CEA.RequestGuid <> '00000000-0000-0000-0000-000000000000'

			SELECT @campaignSentCount= COUNT(1) FROM vCampaignRecipients(nolock) CR
			JOIN dbo.Campaigns(NOLOCK) C ON CR.CampaignID = C.CampaignID 
			WHERE CR.ContactID = @contactId AND CR.DeliveryStatus=111 AND C.AccountID = @accountId AND C.IsDeleted = 0

			 SELECT @workflowCount = COUNT(DISTINCT  CWA.WorkflowID) from ContactWorkflowAudit(nolock) CWA
			  JOIN Workflows(nolock) W ON W.WorkflowID = CWA.WorkflowID
			 WHERE  CWA.ContactID=@contactId AND W.AccountID=@accountId AND W.IsDeleted=0

			 INSERT INTO @contactEmailEngagementTable VALUES(isnull(@workflowCount,0),isnull(@campaignSentCount,0),isnull(@emailsSentCount,0))

			 SELECT WorkflowsCount,CampaignsCount,EmailsCount  FROM @contactEmailEngagementTable
END

/*
EXEC [dbo].[ContactEmailEngagementSummary]
      @contactId = 1741720,
	  @accountId =4218
*/