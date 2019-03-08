
CREATE PROC [dbo].[VMTA_Log_Processor]
AS

BEGIN
	DECLARE @LoopCounter INT,
	@NoOfRecords INT,
	@Email_Status SMALLINT,
	@Deliver_Status INT,
    @Optout_Status INT,
	@Recipient NVARCHAR(max),
	@ContactID INT,
	@TimeLogged DATETIME,
	@Remarks VARCHAR(2000),
	@CampaignRecipientId INT,
	@DetailID INT,
	@FileType INT,
	@AccountID INT,
	@SentOn DATETIME
	
	--DECLARE @CampaignDetails TABLE 
	--(
	--	RowId INT,
	--	ContactID INT, 
	--	CampaignLogDetailsID INT,
	--	CampaignId INT , 
	--	CampaignRecipientId INT,
	--	Recipient NVARCHAR(MAX), 
	--	DeliveryStatus SMALLINT,
	--	OptOutStatus SMALLINT, 
	--	BounceCategory INT,
	--	TimeLogged DATETIME,
	--	Remarks VARCHAR(2000),
	--	[Status] TINYINT,
	--	FileType INT,
	--	CreatedOn DATETIME
	--)

	DECLARE @FinalRecipients TABLE
	(
		ID INT IDENTITY(1,1), 
		CampaignRecipientId INT, 
		DeliveredOn DATETIME, 
		DeliveryStatus INT, 
		Remarks VARCHAR(200), 
		HasComplained BIT DEFAULT NULL, 
		ComplainedOn DATETIME DEFAULT NULL, 
		HasUnSubscribed BIT  DEFAULT NULL, 
		UnsubscribedOn DATETIME DEFAULT NULL
	)
	IF OBJECT_ID('tempdb..##CampaignDetails') IS NOT NULL BEGIN DROP TABLE ##CampaignDetails END

	
	CREATE TABLE  ##CampaignDetails (ContactID int, AccountID INT,CampaignLogDetailsID int, CampaignId int,
									CampaignRecipientID int,Recipient nvarchar(200),DeliveryStatus int,
          OptOutStatus nvarchar(100),BounceCategory int,TimeLogged datetime,
          Remarks nvarchar(100),
          [Status] bit,FileType bit,CreatedOn datetime,SentOn datetime,
          RowId int)

	PRINT 'Before getting CampaignLog Details ' + RTRIM(CAST(GETUTCDATE() AS nvarchar(30)))
	--INSERT INTO @CampaignDetails(ContactID, CampaignLogDetailsID,CampaignId, CampaignRecipientId, Recipient, DeliveryStatus,OptOutStatus,BounceCategory,TimeLogged,Remarks,Status,Filetype,CreatedOn, RowId)
	INSERT INTO ##CampaignDetails 
	SELECT TOP 1000 CR.ContactID,CR.AccountId,CLD.CampaignLogDetailsID,CR.CampaignId,CR.CampaignRecipientID,CLD.Recipient,CLD.DeliveryStatus,CLD.OptOutStatus,CLD.BounceCategory,CLD.TimeLogged,CLD.Remarks,CLD.[Status],CLD.FileType,CLD.CreatedOn,CR.SentON,ROW_NUMBER() OVER(ORDER BY CampaignLogDetailsID) AS RowId 
	FROM CampaignLogDetails CLD WITH (NOLOCK)
	LEFT JOIN CampaignRecipients CR WITH (NOLOCK) ON (CLD.CampaignId = CR.CampaignID OR CLD.CampaignRecipientId = CR.CampaignRecipientId) AND CR.[To] = CLD.Recipient AND CR.WorkflowID IS NULL	
	WHERE CLD.Status = 1
	
	
	PRINT 'After getting CampaignLog Details ' + RTRIM(CAST(GETUTCDATE() AS nvarchar(30)))
	
	SELECT @NoOfRecords = COUNT(1) FROM ##CampaignDetails WITH (NOLOCK)

	CREATE NONCLUSTERED INDEX Temp_CampaignDetails_RowId ON ##CampaignDetails(RowId);
	CREATE NONCLUSTERED INDEX Temp_CampaignDetails_CampaignId ON ##CampaignDetails(CampaignId);
	CREATE NONCLUSTERED INDEX Temp_CampaignDetails_CampaignLogDetailsID ON ##CampaignDetails(CampaignLogDetailsID);

	WHILE @NoOfRecords > 0
	BEGIN
		SET @LoopCounter = 1	
	SELECT GETUTCDATE()
		WHILE (@LoopCounter <= @NoOfRecords)
		BEGIN
 
			 SELECT @Deliver_Status = DeliveryStatus,@Optout_Status =OptOutStatus,@Recipient = Recipient,@ContactID =ContactID ,@TimeLogged = TimeLogged ,@Remarks =Remarks,@Filetype = Filetype,
			 @CampaignRecipientId = CampaignRecipientId,@DetailID = CampaignLogDetailsID,@AccountID = AccountID, @SentOn = SentOn FROM ##CampaignDetails WHERE RowId = @LoopCounter	
		 
		 --PRINT  @CampaignRecipientId +@AccountID

			  IF(@FILETYPE = 1)
				BEGIN
				 SET @Email_Status = CASE WHEN @Deliver_Status = 111 THEN 51 
										  WHEN @Deliver_Status = 113 THEN 53 
										  ELSE 52 END
				END
				
			  ELSE IF(@FILETYPE = 2)
				BEGIN
				 SET @Email_Status = CASE WHEN @Deliver_Status = 115 THEN 56 ELSE NULL END
				END
				
			UPDATE ContactEmails SET EmailStatus = (CASE WHEN @Optout_Status IS NULL THEN @Email_Status 
														ELSE @Optout_Status 
													END )
					WHERE ContactID = @ContactID AND Email = @Recipient AND IsDeleted = 0 AND EmailStatus !=54	
					
					IF(@Deliver_Status = 112 OR @Deliver_Status = 113)
					 BEGIN 
						IF @Remarks LIKE  '%diag%' AND @AccountID IS NOT NULL
						BEGIN 
							INSERT INTO BouncedEmailData(Email, AccountID, StatusID, SentOn, ContactID,[BouncedReason])
							VALUES (@Recipient, @AccountID, @Deliver_Status, @SentOn, @ContactID,@Remarks) 
						END 
						ELSE IF  @Remarks NOT LIKE  '%diag%' AND @AccountID IS NOT NULL
						BEGIN
					 		 INSERT INTO BouncedEmailData(Email, AccountID, StatusID, SentOn, ContactID,[BouncedReason])
							VALUES (@Recipient, @AccountID, @Deliver_Status, @SentOn, @ContactID,'') 
						END
					END 
					
			 IF(@Deliver_Status = 115 AND @FILETYPE = 2)
				BEGIN
					INSERT INTO @FinalRecipients(CampaignRecipientId, DeliveredOn, DeliveryStatus, Remarks,HasComplained, ComplainedOn)
					SELECT @CampaignRecipientId, @TimeLogged,@Deliver_Status, @Remarks,1,@TimeLogged
				END
			ELSE IF(@Optout_Status IS NOT NULL AND @Optout_Status = 54)
				BEGIN
					INSERT INTO @FinalRecipients(CampaignRecipientId, DeliveredOn, DeliveryStatus, Remarks,HasUnSubscribed, UnsubscribedOn)
					SELECT @CampaignRecipientId, @TimeLogged,@Deliver_Status, @Remarks,1,@TimeLogged
				END
			ELSE
				BEGIN
					INSERT INTO @FinalRecipients(CampaignRecipientId, DeliveredOn, DeliveryStatus, Remarks)
					SELECT @CampaignRecipientId, @TimeLogged,@Deliver_Status, @Remarks
				END

			SET @LoopCounter = @LoopCounter + 1
			 --end of inner while
		END
		
		declare @finalCount int
		select @finalCount = count(1) from @finalRecipients

		PRINT 'Before updating campaign recipients, count: '+ cast(@finalCount as varchar(100)) +' ' + RTRIM(CAST(GETUTCDATE() AS nvarchar(30)))
		--update campaign recipients
		UPDATE CR
		SET DeliveredOn = FR.DeliveredOn,
		DeliveryStatus = FR.DeliveryStatus,
		Remarks = FR.Remarks,
		HasComplained = COALESCE(FR.HasComplained, CR.HasComplained),
		ComplainedOn = COALESCE(FR.ComplainedOn, CR.ComplainedOn),
		HasUnsubscribed = COALESCE(FR.HasUnSubscribed,CR.HasUnSubscribed),
		UnsubscribedOn = COALESCE(FR.UnsubscribedOn, CR.UnsubscribedOn),
		LastModifiedOn = GETUTCDATE()
		FROM CampaignRecipients CR  WITH (NOLOCK)
		INNER JOIN @FinalRecipients FR ON FR.CampaignRecipientId = CR.CampaignRecipientID
		PRINT 'AFTER updating campaign recipients, count: '+ cast(@finalCount as varchar(100)) +' ' + RTRIM(CAST(GETUTCDATE() AS nvarchar(30)))

		;WITH CampaignIds AS(SELECT DISTINCT CampaignId from ##CampaignDetails) 

		-- re index campaign
	    INSERT INTO dbo.IndexData(ReferenceID, EntityID, IndexType, CreatedOn, IndexedOn, [Status])
		SELECT NEWID(), CampaignId, 2, GETUTCDATE(), NULL, 1
		FROM CampaignIds WHERE CampaignId IS NOT NULL
		

		INSERT INTO dbo.IndexData (ReferenceID,EntityID, IndexType, CreatedOn, IndexedOn, [Status])
		SELECT NEWID(), ContactId, 1, GETUTCDATE(), NULL, 1 
		FROM ##CampaignDetails WHERE DeliveryStatus = 113 AND ContactId IS NOT NULL

		-- Refresh campaign analytics
		-- entity type = 2
		INSERT INTO dbo.RefreshAnalytics
		SELECT DISTINCT CampaignId,2,1,GETUTCDATE()
		FROM ##CampaignDetails WITH (NOLOCK) WHERE CampaignId IS NOT NULL
		
		--DELETE FROM CampaignLogDetails WHERE CampaignLogDetailsID IN (SELECT CampaignLogDetailsID FROM ##CampaignDetails  WITH (NOLOCK) WHERE CampaignId IS NOT NULL)
		UPDATE CampaignLogDetails SET Status = 2
		WHERE CampaignLogDetailsID IN (SELECT CampaignLogDetailsID FROM ##CampaignDetails  WITH (NOLOCK) WHERE CampaignId IS NOT NULL)
		
		UPDATE CampaignLogDetails SET Status = 2  
		WHERE CampaignLogDetailsID IN (SELECT CampaignLogDetailsID FROM ##CampaignDetails  WITH (NOLOCK) WHERE CampaignId IS NULL)
		
		DELETE ##CampaignDetails
		
		INSERT INTO ##CampaignDetails (ContactID,AccountID, CampaignLogDetailsID,CampaignId, CampaignRecipientId, Recipient, DeliveryStatus,OptOutStatus,BounceCategory,TimeLogged,Remarks,Status,Filetype,CreatedOn,SentON,RowId)
		SELECT TOP 1000 CR.ContactID,CR.AccountId,CLD.CampaignLogDetailsID,CR.CampaignId,CR.CampaignRecipientID,CLD.Recipient,CLD.DeliveryStatus,CLD.OptOutStatus,CLD.BounceCategory,CLD.TimeLogged,CLD.Remarks,CLD.[Status],CLD.FileType,CLD.CreatedOn,CR.SentOn,ROW_NUMBER() OVER(ORDER BY CampaignLogDetailsID) AS RowId 
		FROM CampaignLogDetails CLD WITH (NOLOCK)
		LEFT JOIN CampaignRecipients CR WITH (NOLOCK) ON (CLD.CampaignId = CR.CampaignID OR CLD.CampaignRecipientId = CR.CampaignRecipientId) AND CR.[To] = CLD.Recipient AND CR.WorkflowID IS NULL	
		WHERE CLD.Status = 1
		
		SET @NoOfRecords = 0


   SELECT @NoOfRecords = COUNT(1) FROM ##CampaignDetails

		SELECT GETUTCDATE()
		-- end of outer while
	END
	
END
