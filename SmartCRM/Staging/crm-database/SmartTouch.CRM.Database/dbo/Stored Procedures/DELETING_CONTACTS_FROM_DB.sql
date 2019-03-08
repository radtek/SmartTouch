 
CREATE  PROCEDURE  [dbo].[DELETING_CONTACTS_FROM_DB]
(
@AccountID INT ,
@StartDate DATETIME,
@EndDate   DATETIME 
)
AS
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY

    SELECT  * INTO [DataImports] FROM 
	(SELECT  DISTINCT C.ContactID,0 Statu,ROW_NUMBER() OVER (ORDER BY  C.ContactID ASC) RN  FROM Contacts  C 
	INNER JOIN  Contacts_Audit ca ON C.ContactID   = ca.ContactID
	WHERE  CA.AccountID =  @AccountID AND   CA.AuditAction  =  'I' AND  AuditDate BETWEEN @StartDate  AND @EndDate)  T

	DROP TABLE IF EXISTS #TemContatcs

	SELECT  * INTO  #TemContatcs FROM (
	SELECT  DISTINCT C.ContactID,0 Statu,ROW_NUMBER() OVER (ORDER BY  C.ContactID ASC) RN  FROM Contacts  C 
	INNER JOIN  Contacts_Audit ca ON C.ContactID   = ca.ContactID
	WHERE  CA.AccountID =  @AccountID AND   CA.AuditAction  =  'I' AND  AuditDate BETWEEN @StartDate  AND @EndDate  
	)T


	DECLARE  @TotCount INT ,@ContactID INT 

	SET  @TotCount = (SELECT  COUNT(1) FROM #TemContatcs WHERE  Statu = 0 )

	WHILE  @TotCount > 0
	 BEGIN

			SET  @ContactID = (SELECT  TOP 1 ContactID  FROM #TemContatcs WHERE  Statu = 0  ORDER BY  RN  )

			DELETE FROM ImportContactData                   WHERE ContactID  = @ContactID
			DELETE FROM ImportContactLogs                   WHERE ContactID  = @ContactID
			DELETE FROM [Workflow].[TrackMessages]          WHERE ContactID  = @ContactID 
			DELETE FROM OpportunitiesRelationshipMap        WHERE ContactID  = @ContactID 
			DELETE FROM OpportunitiesRelationshipMap_Audit  WHERE ContactID  = @ContactID 
			DELETE FROM [LeadScoreMessages]				    WHERE ContactID  = @ContactID
			DELETE FROM ContactActionMap				    WHERE ContactID  = @ContactID 
			DELETE FROM ContactActionMap_Audit			    WHERE ContactID  = @ContactID 
			DELETE FROM CommunicationTracker				WHERE ContactID  = @ContactID 
			DELETE FROM ContactNoteMap					    WHERE ContactID  = @ContactID 
			DELETE FROM ContactNoteMap_Audit				WHERE ContactID  = @ContactID 
			DELETE FROM OpportunityContactMap				WHERE ContactID  = @ContactID 
			DELETE FROM OpportunityContactMap_Audit		    WHERE ContactID  = @ContactID
			DELETE FROM ContactTourMap					    WHERE ContactID  = @ContactID 
			DELETE FROM ContactTourMap_Audit				WHERE ContactID  = @ContactID 
			DELETE FROM [BulkContactData]					WHERE ContactID  = @ContactID 
			DELETE FROM ContactRelationshipMap			    WHERE ContactID  = @ContactID 
			DELETE FROM ContactRelationshipMap_Audit		WHERE ContactID  = @ContactID 
			DELETE FROM [dbo].[SubmittedFormFieldData]  WHERE SubmittedFormDataID IN  
			(SELECT  SubmittedFormDataID FROM [SubmittedFormData] WITH (NOLOCK) WHERE  [FormSubmissionID] 
			IN  (SELECT  [FormSubmissionID] FROM FormSubmissions WITH (NOLOCK) WHERE ContactID  = @ContactID  ))
			DELETE FROM  [dbo].[SubmittedFormData] WHERE [FormSubmissionID] IN  (SELECT  [FormSubmissionID] FROM FormSubmissions WITH (NOLOCK) WHERE ContactID  = @ContactID  )
			DELETE FROM FormSubmissions					    WHERE ContactID  = @ContactID 
			DELETE FROM ContactEmailAudit WHERE  ContactEmailID IN  (SELECT ContactEmailID FROM ContactEmails WITH (NOLOCK)	WHERE ContactID  = @ContactID )
			DELETE FROM ContactEmails						WHERE ContactID  = @ContactID 
			DELETE FROM ContactEmails_Audit				    WHERE ContactID  = @ContactID 
			DELETE FROM Documents							WHERE ContactID  = @ContactID 
			DELETE FROM LeadScores						    WHERE ContactID  = @ContactID 
			DELETE FROM ContactCommunityMap				    WHERE ContactID  = @ContactID 
			DELETE FROM MomentaryCampaignRecipients		    WHERE ContactID  = @ContactID 
			DELETE FROM ContactIPAddresses				    WHERE ContactID  = @ContactID 
			DELETE FROM ContactLeadAdapterMap				WHERE ContactID  = @ContactID 
			DELETE FROM ContactAddressMap					WHERE ContactID  = @ContactID 
			DELETE FROM ContactTextMessageAudit WHERE  ContactPhoneNumberID IN  (SELECT  ContactPhoneNumberID FROM ContactPhoneNumbers WHERE ContactID  = @ContactID )
			DELETE FROM ContactPhoneNumbers				    WHERE ContactID  = @ContactID 
			DELETE FROM ContactCustomFieldMap				WHERE ContactID  = @ContactID 
			DELETE FROM ContactWebVisits					WHERE ContactID  = @ContactID 
			DELETE FROM ContactTagMap						WHERE ContactID  = @ContactID 
			DELETE FROM ContactLeadSourceMap				WHERE ContactID  = @ContactID 
			DELETE FROM DocRepositorys					    WHERE ContactID  = @ContactID 
			DELETE FROM ContactWorkflowAudit				WHERE ContactID  = @ContactID 
			DELETE FROM [ReceivedMailAudit]				    WHERE [SentByContactID]  = @ContactID 
			DELETE FROM [WorkflowUserAssignmentAudit]		WHERE ContactID  = @ContactID
			DELETE FROM  [dbo].[CampaignStatistics] WHERE  [CampaignRecipientID] IN (SELECT [CampaignRecipientID] FROM  [dbo].[CampaignRecipients] WHERE ContactID  = @ContactID )
			DELETE  FROM   [dbo].[CampaignRecipients] WHERE ContactID  = @ContactID  
			DELETE FROM Contacts							WHERE ContactID  = @ContactID 
			DELETE FROM Contacts_Audit                      WHERE ContactID  = @ContactID 

			UPDATE   #TemContatcs SET  Statu = 1 WHERE  ContactID = @ContactID

			SET  @TotCount = 0
			SET  @ContactID = 0
			SET  @TotCount = (SELECT  COUNT(1) FROM #TemContatcs WHERE  Statu = 0 )

			SELECT  COUNT(1) FROM   #TemContatcs WHERE  Statu = 1

	 END
END TRY

BEGIN CATCH
	
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END   