
CREATE PROCEDURE  [dbo].[INSERT_CAMPAIGN_OPENENTRY]
	(
		@LinkID		INT,
		@CampaignId INT,
		@CampaignRecipientId INT
	)
AS
BEGIN

	DECLARE @CampaignLinkId int = -1;
	DECLARE @DeliveredOn Datetime;
	DECLARE @AccountId INT
	DECLARE @CampaignRecipients TABLE 
	(
		CampaignRecipientID INT,
		AccountID INT,
		CampaignID INT,
		ContactID INT
	)

	IF (@CampaignRecipientId != 0)
	BEGIN
		INSERT INTO @CampaignRecipients
		SELECT vcr.CampaignRecipientID, vcr.AccountId, vcr.CampaignID, vcr.ContactID FROM CampaignRecipients (NOLOCK) vcr
									WHERE vcr.CampaignRecipientID = @CampaignRecipientId

		SELECT @CampaignId = CampaignID, @AccountId = AccountId FROM @CampaignRecipients
	END
    ELSE 

	SELECT @AccountId = AccountId FROM Campaigns (NOLOCK) WHERE CampaignID = @CampaignId

	SELECT @CampaignLinkId = CampaignLinkID FROM CampaignLinks (NOLOCK) WHERE LinkIndex = COALESCE(@LinkID,-1) and CampaignID = @CampaignId

	INSERT INTO CampaignStatistics(ContactId, CampaignID, ActivityType, CampaignLinkID, ActivityDate, LinkIndex, CampaignRecipientID, AccountId)
						  SELECT NULL, @CampaignId, 1, NULL, GETUTCDATE(), NULL, @CampaignRecipientId, @AccountId

	IF @CampaignLinkId != -1
	BEGIN
	
		INSERT INTO CampaignStatistics(ContactId, CampaignID, ActivityType, CampaignLinkID, ActivityDate, LinkIndex, CampaignRecipientID, AccountId)
						SELECT NULL, @CampaignId, 2, @CampaignLinkId , GETUTCDATE(), @LinkID, @CampaignRecipientId	, @AccountId
	END

	SELECT @DeliveredOn = DeliveredOn FROM CampaignRecipients (NOLOCK) WHERE CampaignRecipientID = @CampaignRecipientId AND AccountID = @AccountID

	IF(@DeliveredOn is null)
	BEGIN
		UPDATE CampaignRecipients SET DeliveredOn = GETUTCDATE(), DeliveryStatus = 111, LastModifiedOn = GETUTCDATE() WHERE CampaignRecipientID = @CampaignRecipientId AND AccountID = @AccountID
	END

	SELECT * FROM @CampaignRecipients

END
