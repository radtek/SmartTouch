
CREATE FUNCTION [dbo].[GetReputationInfo_1]
	(
		@AccountID		int,
		@FromDate		datetime,
		@ToDate			datetime
	)
RETURNS @Results TABLE
  (
	 AccountID int,
	 SenderReputationCount int
  )
AS
BEGIN	
	DECLARE @OverAllRecipients	TABLE (DeliveryStatus tinyint, HasComplianed bit, Records int)
	DECLARE @SenderRepCount	int = 0,	
			@AllRecpCount	int = 0,
			@OpenRate		int = 0
		
	SET @SenderRepCount = (SELECT CASE WHEN COUNT(1) > 1000 THEN 20 ELSE 0 END 
							FROM SmartCRM.dbo.Campaigns WITH (NOLOCK) 
							WHERE CampaignStatusID = 105 AND AccountID = @ACCOUNTID
								AND LastUpdatedOn >= DATEADD(dd,-30, GETUTCDATE()) AND LastUpdatedOn <= GETUTCDATE());

		INSERT INTO @OverAllRecipients (DeliveryStatus, HasComplianed, Records)
		SELECT CR.DeliveryStatus, CR.HasComplained, COUNT(CR.CampaignRecipientID)
			FROM SmartCRM.dbo.vCampaignRecipients CR WITH (NOLOCK)
				INNER JOIN SmartCRM.dbo.Campaigns C WITH (NOLOCK) ON CR.CampaignID = C.CampaignID
			WHERE C.AccountID = @AccountID AND CR.SentOn >= @FromDate AND CR.SentOn <= @ToDate
			GROUP BY CR.DeliveryStatus, CR.HasComplained
		

	SELECT @AllRecpCount = SUM(Records) FROM @OverAllRecipients	

	IF (@AllRecpCount > 0)
		BEGIN
			--COMPLIANT RATE
			SET @SenderRepCount = @SenderRepCount+(CASE WHEN (((SELECT SUM(Records) FROM @OverAllRecipients WHERE HasComplianed = 1)/@AllRecpCount)* 100) < 1 THEN 20 ELSE 0 END) 

			--HARDBOUNCE CODE	
			SET @SenderRepCount = @SenderRepCount+(CASE WHEN (((SELECT COUNT(Records) FROM @OverAllRecipients WHERE DeliveryStatus = 113)/@AllRecpCount)* 100) < 1 THEN 20 ELSE 0 END) 

			--SOFTBOUNCE
			SET @SenderRepCount = @SenderRepCount +  ( CASE WHEN (((SELECT COUNT(Records) FROM @OverAllRecipients WHERE DeliveryStatus = 112)/@AllRecpCount)* 100) < 1 THEN 20 ELSE 0 END) 
	
			-- OPEN RATE
			SET @OpenRate = (SELECT COUNT(1) FROM SmartCRM.dbo.vCampaignStatistics CS WITH (NOLOCK)
									INNER JOIN SmartCRM.dbo.vCampaignRecipients CR (NOLOCK) ON CS.CampaignRecipientID = CR.CampaignRecipientID
									INNER JOIN SmartCRM.dbo.Campaigns C WITH (NOLOCK) ON C.CampaignID = CR.CampaignID 
									WHERE CS.ActivityType = 1)

			SET @SenderRepCount = @SenderRepCount+(CASE WHEN (@OpenRate/@AllRecpCount) * 100 > 1 THEN 20 ELSE 0 END)
		END
     
		INSERT INTO @Results (AccountID, SenderReputationCount)
		SELECT @AccountID, @SenderRepCount
   RETURN
END




