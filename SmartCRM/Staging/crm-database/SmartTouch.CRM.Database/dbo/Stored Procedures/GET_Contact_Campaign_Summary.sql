
CREATE PROCEDURE  [dbo].[GET_Contact_Campaign_Summary]
(
   @AccountID INT ,
   @ContactID INT  ,
   @Date  DATETIME  
)
AS
BEGIN


    SELECT * INTO #TEM
	FROM
	(
	SELECT CampaignRecipientID,ContactID,[SentOn],DeliveredOn,DeliveryStatus FROM  CampaignRecipients CR WITH (NOLOCK) WHERE  AccountID = @AccountID 
	AND ContactID = @ContactID 
	)T





    DECLARE @Recipients INT,@Delivered INT, @Sent INT, @Opened INT, @Clicked INT, @Complained INT, @OptedOut INT, @Bounced INT, @Blocked INT



    SELECT @Sent = COUNT(1) FROM #TEM WHERE  [SentOn] > @Date

	SELECT @Delivered = COUNT(1) FROM #TEM WHERE DeliveryStatus IN (111) AND [DeliveredOn] >@Date

	SELECT @Opened = COUNT(DISTINCT CampaignRecipientID) FROM CampaignStatistics (NOLOCK) WHERE ActivityType = 1 AND AccountID = @AccountID AND [ActivityDate] >@Date
	AND CampaignRecipientID IN (SELECT CampaignRecipientID FROM #TEM )

	SELECT @Clicked = COUNT(DISTINCT CampaignRecipientID) FROM CampaignStatistics (NOLOCK) WHERE   ActivityType = 2 AND AccountID = @AccountID AND [ActivityDate] >@Date
	AND CampaignRecipientID IN (SELECT CampaignRecipientID FROM #TEM   )


	
	SELECT @Sent[Sent] ,@Delivered Delivered ,@Opened Opened ,@Clicked Clicked

	

END

/*
DECLARE @DATE DATETIME  = GETUTCDATE()-1000
EXEC GET_Contact_Campaign_Summary 66,3927265,@DATE
*/



