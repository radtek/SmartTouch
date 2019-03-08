CREATE PROCEDURE [dbo].[Calc_WorkflowCampaignAnalytics]
(
	@CampaignID INT,
	@WorkflowID INT,
	@FromDate DATETIME,
	@ToDate DATETIME
)
AS
BEGIN
	DECLARE @AccountID INT
	DECLARE @CampaignAnalytics TABLE
	(
		CampaignID INT,
		ContactID INT,
		RecipientID INT,
		DeliveryStatus INT,
		HasComplained BIT,
		HasOptedOut INT,
		WorkflowID INT
	)
	DECLARE @WorkflowCampaignAnalytics TABLE 
	(
		[CampaignID] [int] NULL,
		[Delivered] [int] NULL,
		[Bounced] [int] NULL,
		[Opened] [int] NULL,
		[Clicked] [int] NULL,
		[Complained] [int] NULL,
		[Unsubscribed] [int] NULL
	)

	IF @CampaignID = 0
		BEGIN
			SET @CampaignID = NULL
		END
	
	IF ISNULL(@FromDate,'') = ISNULL(@ToDate,'')
		BEGIN
			SELECT @FromDate = CreatedOn, @AccountID = AccountID FROM Workflows (NOLOCK) WHERE WorkflowID = @WorkflowID
			SET @ToDate = GETUTCDATE()
		END
	
	SELECT @AccountID = AccountID FROM Workflows(NOLOCK) WHERE WorkflowID = @WorkflowID
	
	DECLARE @Campaigns TABLE (RowID INT IDENTITY(1,1), CampaignID INT)
	INSERT INTO @CampaignAnalytics
	SELECT CR.CampaignID,CR.ContactID, CR.CampaignRecipientID,CR.DeliveryStatus, CR.HasComplained, CR.HasUnsubscribed, CR.WorkflowID FROM  
	CampaignRecipients CR (NOLOCK) WHERE  WorkflowID = @WorkflowID AND CR.AccountID = @AccountID AND CampaignID = COALESCE(@CampaignID,CampaignID) AND CR.DeliveredOn BETWEEN @FromDate AND @ToDate
	
	INSERT INTO @Campaigns
	SELECT WCA.CampaignID FROM WorkflowActions WA (NOLOCK)
	INNER JOIN WorkflowCampaignActions (NOLOCK) WCA ON WCA.WorkflowActionID = WA.WorkflowActionID
	WHERE WA.WorkflowID = @WorkflowID AND WA.IsDeleted = 0 AND WCA.CampaignID = COALESCE(@CampaignID, WCA.CampaignID)
	SELECT TOP 1 @CampaignID = CampaignID FROM @Campaigns ORDER BY RowID ASC
	
	WHILE @CampaignID > 0
		BEGIN
			DECLARE @Recipients INT,@Delivered INT, @Sent INT, @Opened INT, @Clicked INT, @Complained INT, @OptedOut INT, @Bounced INT
	
			SELECT @Recipients = COUNT(1) FROM @CampaignAnalytics
			SELECT @Sent = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus NOT IN (113) AND CampaignID = @CampaignID 
			SELECT @Delivered = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus IN (111) AND CampaignID = @CampaignID 
			SELECT @Bounced = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus  IN (112,113) AND CampaignID = @CampaignID 

			SELECT @Opened = COUNT(DISTINCT CS.CampaignRecipientID) FROM CampaignStatistics (nolock) CS
            INNER JOIN @CampaignAnalytics CA ON CA.RecipientID = CS.CampaignRecipientID
            WHERE CS.CampaignID = @CampaignID AND CS.ActivityType = 1 AND CS.ActivityDate BETWEEN @FromDate AND @ToDate AND CS.AccountID = @AccountID

			SELECT @Clicked = COUNT(DISTINCT CampaignRecipientID) FROM CampaignStatistics (nolock) CS 
            INNER JOIN @CampaignAnalytics CA ON CA.RecipientID = CS.CampaignRecipientID
            WHERE CS.CampaignID = @CampaignID AND CS.ActivityType = 2 AND CS.ActivityDate BETWEEN @FromDate AND @ToDate AND CS.AccountID = @AccountID

			SELECT @Complained = COUNT(1) FROM @CampaignAnalytics WHERE HasComplained = 1 AND CampaignID = @CampaignID 
			SELECT @OptedOut = COUNT(1) FROM @CampaignAnalytics WHERE HasOptedOut = 1 AND CampaignID = @CampaignID 
			INSERT INTO @WorkflowCampaignAnalytics(CampaignID,  Delivered, Bounced ,Opened, Clicked, Complained, Unsubscribed)
			SELECT  @CampaignID, @Delivered ,@Bounced, @Opened, @Clicked, @Complained, @OptedOut
			
			DELETE FROM @Campaigns WHERE CampaignID = @CampaignID
			SET @CampaignID = 0
			SELECT TOP 1 @CampaignID = CampaignID FROM @Campaigns 

		END

	SELECT C.Name AS CampaignName, WA.*, WA.Unsubscribed [OptedOut] FROM @WorkflowCampaignAnalytics WA
	INNER JOIN Campaigns C (NOLOCK) ON WA.CampaignID = C.CampaignID
END