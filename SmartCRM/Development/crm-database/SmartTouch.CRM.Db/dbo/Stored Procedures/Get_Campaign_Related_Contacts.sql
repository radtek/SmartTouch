
CREATE PROC [dbo].[Get_Campaign_Related_Contacts]
(
	@campaignID INT,
	@accountId INT,
	@drillDownActivity INT,
	@linkId INT
)
AS
BEGIN

	DECLARE @Contacts TABLE (Id INT)

	SELECT CR.* 
	INTO #campaignRecipients
	FROM CampaignRecipients (NOLOCK) CR 
	WHERE CampaignID = @campaignID AND AccountID = @accountId

	SELECT  CR.CampaignRecipientID, CR.ContactID, CS.ActivityType, CS.ActivityDate, CS.CampaignLinkID
	INTO #campaignStatistics
	FROM CampaignStatistics (NOLOCK) CS
	INNER JOIN CampaignRecipients (NOLOCK) CR ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CR.AccountId = CS.AccountId
	WHERE CR.CampaignID = @campaignID AND CR.AccountID = @accountId

	IF  @drillDownActivity = 1 --Opened
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactId FROM #campaignStatistics (NOLOCK) WHERE ActivityType = 1
		END
	ELSE IF  @drillDownActivity = 2 --Clicked
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactId FROM #campaignStatistics (NOLOCK) WHERE ActivityType = 2
		END
	ELSE IF  @drillDownActivity = 3 --Delivered
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE DeliveryStatus = 111
		END
	ELSE IF  @drillDownActivity = 4 -- Unsubscrined
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE  HasUnsubscribed = 1
		END
	ELSE IF  @drillDownActivity = 5 --Complained
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE  HasComplained = 1
		END
	ELSE IF  @drillDownActivity = 6 --Sent
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE DeliveryStatus > 0
		END
	ELSE IF  @drillDownActivity = 7 --Bounced
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE DeliveryStatus IN (112,113)
		END
	ELSE IF  @drillDownActivity = 8 --NotViewed
		BEGIN
			INSERT INTO @Contacts
			SELECT CR.ContactId FROM #campaignRecipients (NOLOCK) CR
			LEFT JOIN #campaignStatistics CS (NOLOCK) ON CR.CampaignRecipientID = CS.CampaignRecipientID
			WHERE CS.CampaignRecipientID IS NULL
		END
	ELSE IF  @drillDownActivity = 9 --LinkClicked
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactId FROM #campaignStatistics WHERE CampaignLinkID = @linkId AND ActivityType = 2
		END
	ELSE IF  @drillDownActivity = 10 --All
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK)
		END
	ELSE IF  @drillDownActivity = 11 --Blocked
		BEGIN
			INSERT INTO @Contacts
			SELECT ContactID FROM #campaignRecipients (NOLOCK) WHERE DeliveryStatus = 119
		END

		SELECT DISTINCT Id FROM @Contacts
END