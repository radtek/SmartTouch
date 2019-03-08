create PROCEDURE [dbo].[ProcessCampaignRecipients]
AS
BEGIN
	DECLARE @Campaigns TABLE (RowNumber INT IDENTITY(1,1),CampaignID INT, OwnerID INT, IsProcessed BIT, AccountID INT) 
	DECLARE @campaignsCount INT

	INSERT INTO @Campaigns
	SELECT CampaignID, CreatedBy,0, AccountID FROM Campaigns C (NOLOCK) WHERE C.IsDeleted = 0  AND C.ScheduleTime < GETUTCDATE() AND C.CampaignStatusID IN (102,106) AND (C.IsRecipientsProcessed = 0 or C.IsRecipientsProcessed is null) ORDER BY C.ScheduleTime 
ASC
	SELECT @campaignsCount = (SELECT COUNT(*) from @Campaigns)
	
	DECLARE @RowNumber INT = 1
	

	WHILE  @RowNumber <= @campaignsCount
	BEGIN
		DECLARE @DeleteCounter INT = 5000
		DECLARE @CampaignID INT 
		DECLARE @OwnerID INT
		DECLARE @AccountID INT
		DECLARE @SSContacts dbo.Contact_List
		DECLARE @serviceUri VARCHAR (1024)
		DECLARE @processResponse VARCHAR (1024)
		DECLARE @isPrivate BIT
		DECLARE @ssContactsCount INT
		DECLARE @deleteIterations INT
		SELECT @isPrivate= ISNULL((select top 1 IsPrivate from AccountDataAccessPermissions where accountid = @AccountID and ModuleID = 3),0)

		SELECT @CampaignID = CampaignID, @OwnerID = OwnerID,@AccountID=AccountID FROM @Campaigns WHERE RowNumber = @RowNumber --order by CampaignID OFFSET 1 rows fetch next 1 rows only
		SELECT @processResponse = ''
		SELECT @OwnerID = 
			CASE @isPrivate 
				WHEN  0 THEN  0 
				WHEN 1 THEN @OwnerID	
			END 

		SELECT @serviceUri = Value FROM EnvironmentSettings (nolock) WHERE Name = 'SmarttouchServiceUri'
		
		BEGIN TRY			
			IF EXISTS (SELECT 1 FROM CampaignSearchDefinitionMap (NOLOCK) WHERE CampaignID = @CampaignID)
			BEGIN
				SELECT @processResponse = (SELECT [dbo].[InsertCampaignRecipients](CONCAT(@serviceUri , '/insertCampaignRecipients/',@CampaignID)))
				INSERT INTO @SSContacts SELECT ContactID FROM [dbo].[MomentaryCampaignRecipients] (NOLOCK) WHERE campaignid = @CampaignID
			END
			
			print cast(@CampaignID as varchar(200))
			EXEC dbo.HandleNextCampaign @CampaignID, @OwnerID , @SSContacts
			UPDATE Campaigns SET IsRecipientsProcessed = 1 WHERE CampaignID = @CampaignID
			DELETE @SSContacts
		END TRY
		BEGIN CATCH		
			UPDATE Campaigns SET CampaignStatusID = 110, IsRecipientsProcessed=0, Remarks = ERROR_MESSAGE(), ProcessedDate = GETUTCDATE() WHERE CampaignID = @CampaignID
		END CATCH
		

		WHILE @DeleteCounter = 5000
		BEGIN
			DELETE TOP(5000) FROM MomentaryCampaignRecipients WHERE CampaignID = @CampaignID
			SET @DeleteCounter = @@ROWCOUNT
		END

		UPDATE @Campaigns SET IsProcessed = 1 WHERE RowNumber = @RowNumber		
		SET @RowNumber = @RowNumber + 1		
	END
END