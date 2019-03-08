

CREATE  PROCEDURE [dbo].[HandleNextCampaign]
(
	@CampaignId INT,
	@OwnerId INT,
	@SSContacts dbo.Contact_List READONLY
)
AS
BEGIN
BEGIN TRY
begin transaction
    
	DECLARE @ResentStatus INT
	DECLARE @PCampaignId INT
	DECLARE @LastProcessed DATETIME
	DECLARE @SSActive INT
	DECLARE @TagsActive INT
	DECLARE @ScheduleTime DATETIME
	DECLARE @AccountID INT
	DECLARE @Count INT
	DECLARE @ActivityDuration DATETIME = DATEADD(YEAR,-1,GETUTCDATE())
	SELECT @SSActive = SSRecipients,
		   @TagsActive = TagRecipients,
		   @ScheduleTime = ScheduleTime, @AccountID = AccountID
	 FROM Campaigns WHERE CampaignID = @CampaignId

	SELECT @Count = COUNT(1) FROM CampaignRecipients (NOLOCK) WHERE CampaignID =@CampaignId AND AccountID = @AccountID

	SELECT @ResentStatus = CampaignResentTo, @PCampaignId = ParentCampaignID
	FROM ResentCampaigns (NOLOCK) Where CampaignId = @CampaignId

	DECLARE @Recipients TABLE
	(
		ContactId INT,
		Email VARCHAR(250)
	)
	DECLARE @OldRecipients TABLE
	(
		ContactId INT,
		Email VARCHAR(250)
	)
	DECLARE @FinalRecipients TABLE
	(
		RowNum INT primary key,
		ContactId INT,
		Email VARCHAR(250)
	)

	DECLARE @FilteredRecipients TABLE
	(
		RowNum INT primary key,
		ContactId INT,
		Email VARCHAR(250)
	)

IF(@Count > 0)
	BEGIN
		PRINT('Has Recipients already')
		SELECT COUNT(1) FROM CampaignRecipients (nolock) WHERE CampaignId = @CampaignId AND AccountID = @AccountID AND (DeliveryStatus = 116 OR DeliveryStatus = 113 OR DeliveryStatus is NULL)
	END
	ELSE
		BEGIN
			/*NewContacts*/
			IF @ResentStatus = 122
				BEGIN
					PRINT('Resend to new contacts')
					SELECT TOP 1 @LastProcessed = ProcessedDate FROM (SELECT ProcessedDate FROM Campaigns C (NOLOCK) WHERE C.CampaignID = @CampaignId
					UNION
					SELECT CampaignResentTo AS ProcessedDate FROM ResentCampaigns RC (NOLOCK) WHERE RC.ParentCampaignID = @PCampaignId) as x
					ORDER BY ProcessedDate DESC
		
					INSERT INTO @OldRecipients
					SELECT CR.ContactID, CR.[To] FROM CampaignRecipients CR (NOLOCK)
					INNER JOIN ResentCampaigns RC (NOLOCK) ON ((CR.CampaignID = RC.ParentCampaignID) OR (CR.CampaignID = RC.CampaignID))
					INNER JOIN  Contacts C WITH (NOLOCK) ON C.ContactID = CR.ContactID AND C.AccountID = CR.AccountId
					WHERE RC.ParentCampaignID = @PCampaignId AND RC.CampaignResentTo =  @ResentStatus  AND CR.AccountID = @AccountID
					AND C.IsDeleted = 0 AND C.DoNotEmail = 0
				END
			/*NotViewedContacts*/
			ELSE IF @ResentStatus = 123
				BEGIN
					PRINT('Resend to not viewed contacts')
					INSERT INTO @OldRecipients (ContactId, Email)
					SELECT CR.ContactID, CR.[To]  FROM CampaignStatistics CS (NOLOCK)
					INNER JOIN ResentCampaigns RC (NOLOCK) ON ((CS.CampaignID = RC.ParentCampaignID) OR (CS.CampaignID = RC.CampaignID))
					INNER JOIN CampaignRecipients CR (NOLOCK) ON CS.CampaignRecipientID = CR.CampaignRecipientID
					INNER JOIN  Contacts C WITH (NOLOCK) ON C.ContactID = CR.ContactID AND C.AccountID = CR.AccountId
					WHERE RC.ParentCampaignID = @PCampaignId AND RC.CampaignResentTo =  @ResentStatus AND CS.AccountID = @AccountID AND CR.AccountID = @AccountID
					AND C.IsDeleted = 0 AND C.DoNotEmail = 0
				END
			ELSE
				BEGIN
					PRINT('New Campaign')
				END

			DECLARE @ImportedContacts dbo.Contact_List

			--get all imported contacts from this account
			INSERT INTO @ImportedContacts
			SELECT ContactId FROM Contacts (NOLOCK) WHERE AccountID = @AccountID AND FirstContactSource = 2 AND IsDeleted = 0

			;WITH ActiveContacts AS
				(
					--get contacts activity(open/click) in last 12 months
					SELECT CR.ContactID FROM CampaignStatistics CS (NOLOCK)
					INNER JOIN CampaignRecipients CR (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID AND Cr.AccountID = CS.AccountID
					INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CR.ContactID
					WHERE  (CS.ActivityDate > @ActivityDuration) AND	C.AccountID = @AccountID AND CS.AccountID = @AccountID
					UNION
					--get contacts created in last 12 months
					SELECT C.ContactID FROM Contacts (NOLOCK) C
					INNER JOIN Contacts_Audit (NOLOCK) CA ON CA.Contactid = C.ContactID
					--LEFT JOIN @ImportedContacts IC ON IC.ContactID = CA.ContactID
					WHERE C.IsDeleted = 0 AND CA.AuditAction ='I' --AND IC.ContactID IS NULL
					AND C.AccountID = @AccountID AND CA.LastUpdatedOn > @ActivityDuration  AND C.FirstContactSource !=2
				)
				, TagContacts AS
				(	
					--get active contacts
					SELECT CA.ContactID FROM ActiveContacts CA 
					INNER JOIN ContactTagMap CTM (NOLOCK) ON CA.ContactID = CTM.ContactID 
					INNER JOIN CampaignContactTagMap CMPTM (NOLOCK) ON  CTM.TagID = CMPTM.TagId
					--LEFT JOIN @ImportedContacts IC ON IC.ContactID = CA.ContactID
					WHERE CMPTM.CampaignID = @CampaignId AND @TagsActive = 2 --AND IC.ContactID IS NULL
					UNION
					--get all contacts
					SELECT CTM.ContactID FROM CampaignContactTagMap CMPTM (NOLOCK)
					INNER JOIN ContactTagMap CTM (NOLOCK) ON CTM.TagID = CMPTM.TagId
					WHERE CMPTM.CampaignID = @CampaignId AND @TagsActive = 1
				)
				, SDContacts AS
				(
					--get active contacts
					SELECT CA.ContactID FROM ActiveContacts CA 
					INNER JOIN @SSContacts SS ON CA.ContactID = SS.ContactID
					--LEFT JOIN @ImportedContacts IC ON IC.ContactID = CA.ContactID
					WHERE @SSActive = 2 --AND IC.ContactID IS NULL
					UNION
					--get all contacts
					SELECT SS.ContactID FROM @SSContacts SS
					WHERE @SSActive = 1
				)

		
				
				INSERT INTO @Recipients (ContactId, Email)  
				SELECT CCTM.ContactID, CE.Email FROM
				(SELECT * FROM TagContacts  UNION SELECT * FROM SDContacts) CCTM
				INNER JOIN ContactEmails CE (NOLOCK) ON CE.ContactID = CCTM.ContactID
				INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CCTM.ContactID
				WHERE CE.IsPrimary = 1 
				AND CE.IsDeleted = 0 
				AND CE.EmailStatus IN (50,51,52,0)
				AND C.IsDeleted = 0
				AND COALESCE(C.OwnerID,0) = COALESCE(NULLIF(@OwnerId,0), C.OwnerID,0)
				AND C.DoNotEmail  = 0
		
				;WITH FinalRecipients AS
				 (
				  SELECT * FROM @Recipients
				  EXCEPT
				  SELECT * FROM @OldRecipients
				 )
				 INSERT INTO @FinalRecipients (RowNum, ContactId, Email)
				 SELECT ROW_NUMBER() OVER ( ORDER BY ContactId), CONTACTID, EMAIL FROM FinalRecipients WHERE dbo.isValidEmailFormat_new(Email) = 1


				DECLARE @batchSize INT = 1000
				DECLARE @iternationNumber INT = 0
	
				WHILE 1 = 1
				BEGIN
					DELETE FROM @FilteredRecipients

					INSERT INTO @FilteredRecipients
					SELECT TOP (@batchSize)  RowNum, CONTACTID, EMAIL FROM @FinalRecipients where RowNum > (@iternationNumber * @BatchSize)

					INSERT INTO CampaignRecipients --WITH (TABLOCK) 
					([CampaignID]
				   ,[ContactID]
				   ,[CreatedDate]
				   ,[To]
				   ,[ScheduleTime]
				   ,[SentOn]
				   ,[GUID]
				   ,[DeliveredOn]
				   ,[DeliveryStatus]
				   ,[LastModifiedOn]
				   ,[OptOutStatus]
				   ,[Remarks]
				   ,[ServiceProviderID]
				   ,[HasUnsubscribed]
				   ,[UnsubscribedOn]
				   ,[HasComplained]
				   ,[ComplainedOn]
				   ,[WorkflowID]
				   ,[AccountId])
					
					SELECT   @CAMPAIGNID
				   ,CONTACTID
				   ,GETUTCDATE()
				   ,EMAIL
				   ,@ScheduleTime
				   ,NULL
				   ,NULL
				   ,NULL
				   ,NULL
				   ,GETUTCDATE()
				   ,0
				   ,NULL
				   ,NULL
				   ,0
				   ,NULL
				   ,0
				   ,NULL
				   ,NULL
				   ,@AccountID  FROM @FilteredRecipients

					IF @@ROWCOUNT < @batchSize BREAK
					set @iternationNumber = @iternationNumber + 1;
					print cast(@iternationNumber as varchar(10))
    
				END

				UPDATE Campaigns SET CampaignStatusID = 106 WHERE CampaignID = @CampaignId
				SELECT COUNT(1) FROM CampaignRecipients (NOLOCK) WHERE CampaignID = @CampaignId AND AccountID = @AccountID
		END
 COMMIT TRANSACTION
END TRY
BEGIN CATCH    
    -- in case of an error, ROLLBACK the transaction    
    ROLLBACK TRANSACTION
	
	 DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;

    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();
		print(@ErrorMessage)
    -- Use RAISERROR inside the CATCH block to return error
    -- information about the original error that caused
    -- execution to jump to the CATCH block.
    RAISERROR (@ErrorMessage, -- Message text.
               @ErrorSeverity, -- Severity.
               @ErrorState -- State.
               );
			    
END CATCH
END
/*
Execution
declare @contacts as dbo.Contact_List

exec [HandleNextCampaign] @CampaignId = 7396, @OwnerId = 0, @SSContacts = @contacts
*/