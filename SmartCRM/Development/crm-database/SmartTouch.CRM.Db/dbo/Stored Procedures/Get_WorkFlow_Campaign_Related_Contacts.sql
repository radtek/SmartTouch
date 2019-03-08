
CREATE PROCEDURE [dbo].[Get_WorkFlow_Campaign_Related_Contacts] 
	@WorkflowId smallint,
	@CampaignId int,
	@CampaignDrillDownType tinyint,
	@FromDate datetime,
	@EndDate datetime
AS
BEGIN
    DECLARE @AccountId INT
	IF @FromDate = @EndDate
		BEGIN
			SELECT @FromDate = CreatedOn FROM Workflows (NOLOCK) WHERE WorkflowID = @WorkflowId
			SET @EndDate = GETUTCDATE()
		END
	SELECT @AccountId = AccountID FROM Workflows(NOLOCK) WHERE WorkflowID = @WorkflowId
	DECLARE @Contacts TABLE (ID INT IDENTITY(1,1), ContactID INT)
	IF (@CampaignDrillDownType = 6) -- Sent = 6
		BEGIN
			INSERT INTO	@Contacts SELECT DISTINCT  ContactID  FROM CampaignRecipients (NOLOCK) 
			WHERE WorkflowID = @WorkflowId AND AccountID = @AccountId AND CampaignID = @CampaignId AND DeliveryStatus = 114 AND DeliveredOn BETWEEN @FromDate AND @EndDate
		END
    IF(@CampaignDrillDownType = 3) -- Delivered
		BEGIN
			INSERT INTO	@Contacts SELECT DISTINCT  ContactID  FROM CampaignRecipients (NOLOCK) 
			WHERE WorkflowID = @WorkflowId AND AccountID = @AccountId AND CampaignID = @CampaignId AND DeliveryStatus = 111 AND DeliveredOn BETWEEN @FromDate AND @EndDate
		END
    IF(@CampaignDrillDownType = 1 OR @CampaignDrillDownType = 2 OR @CampaignDrillDownType = 4 OR @CampaignDrillDownType = 5)--Opened = 1, Clicked = 2 , Complained = 4 , unsubscribed = 5
		BEGIN
			INSERT INTO @Contacts SELECT DISTINCT  VR.ContactID  FROM CampaignStatistics (NOLOCK) VS
			INNER JOIN CampaignRecipients (NOLOCK) VR ON VR.CampaignRecipientID = VS.CampaignRecipientID AND VR.AccountID = @AccountId
		    WHERE VR.WorkflowID = @WorkflowId AND VS.CampaignID = @CampaignId AND VS.ActivityType = @CampaignDrillDownType AND VR.DeliveredOn BETWEEN @FromDate AND @EndDate
		END

	 SELECT ContactID  FROM @Contacts
END

/*
	exec Get_WorkFlow_Campaign_Related_Contacts
		@WorkflowId = 1158,
		@CampaignId  = 10425,
		@CampaignDrillDownType  = 3,
		@FromDate  = '5/20/2016 12:00:00 AM',
		@EndDate = '5/27/2016 11:59:00 PM'
*/
GO


