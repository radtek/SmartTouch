
CREATE PROCEDURE [dbo].[Calc_CampaignAnalytics]
(
	@CampaignID INT
)
AS
BEGIN
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
	DECLARE @AccountID INT
	SELECT @AccountID = AccountID FROM Campaigns (NOLOCK) WHERE CampaignID = @CampaignID

	INSERT INTO @CampaignAnalytics
	SELECT @CampaignID,CR.ContactID, CR.CampaignRecipientID,CR.DeliveryStatus, CR.HasComplained, CR.HasUnsubscribed, CR.WorkflowID FROM  CampaignRecipients CR (NOLOCK) WHERE CampaignID = @CampaignID AND AccountID = @AccountID

	DELETE FROM CampaignAnalytics WHERE CampaignID = @CampaignID
	DECLARE @Recipients INT,@Delivered INT, @Sent INT, @Opened INT, @Clicked INT, @Complained INT, @OptedOut INT, @Bounced INT, @Blocked INT
	
	SELECT @Recipients = COUNT(1) FROM @CampaignAnalytics
	SELECT @Sent = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus NOT IN (113, 116, 119)  AND DeliveryStatus IS NOT NULL
	SELECT @Delivered = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus IN (111)
	SELECT @Bounced = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus IN (112,113)
	SELECT @Opened = COUNT(DISTINCT CampaignRecipientID) FROM CampaignStatistics (nolock) WHERE CampaignID = @CampaignID AND ActivityType = 1 AND AccountID = @AccountID
	SELECT @Clicked = COUNT(DISTINCT CampaignRecipientID) FROM CampaignStatistics (nolock) WHERE CampaignID = @CampaignID AND ActivityType = 2 AND AccountID = @AccountID
	SELECT @Complained = COUNT(1) FROM @CampaignAnalytics WHERE (HasComplained = 1 OR DeliveryStatus =115)
	SELECT @OptedOut = COUNT(1) FROM @CampaignAnalytics WHERE HasOptedOut = 1
	SELECT @Blocked = COUNT(1) FROM @CampaignAnalytics WHERE DeliveryStatus IN (119)

	INSERT INTO CampaignAnalytics(CampaignID, Recipients, [Sent], Delivered, Bounced , Blocked, Opened, Clicked, Complained, OptedOut, LastModifiedOn)
	SELECT @CampaignID, @Recipients, @Sent, @Delivered,@Bounced, @Blocked, @Opened, @Clicked, @Complained, @OptedOut,GETUTCDATE()
END
