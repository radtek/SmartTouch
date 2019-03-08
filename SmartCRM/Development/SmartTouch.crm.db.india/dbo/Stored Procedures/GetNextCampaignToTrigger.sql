
CREATE PROCEDURE [dbo].[GetNextCampaignToTrigger]
AS
BEGIN
	DECLARE @Scheduled INT = 102, @Queued INT = 106, @Delayed INT = 117
	DECLARE @ScheduledCampaignID INT = 0
	DECLARE @DebugMode BIT = 0	

	SET @ScheduledCampaignID = (SELECT TOP 1 CampaignID FROM Campaigns (NOLOCK) C 
		WHERE 
			C.IsDeleted = 0 AND 
			C.ScheduleTime < GETUTCDATE() AND 
			C.CampaignStatusID IN (@Scheduled, @Queued) AND 
			c.IsRecipientsProcessed = 1 ORDER BY C.ScheduleTime, C.CreatedDate)

	PRINT @ScheduledCampaignID
	
	IF(@ScheduledCampaignID > 0) 
		BEGIN 
			SELECT * FROM CAMPAIGNS (NOLOCK) 
			WHERE 
				CampaignID = @ScheduledCampaignID
		END
	ELSE 
		BEGIN	
			-- Fetching Delayed Campaigns if no scheduled campaigns found
			DECLARE @DelayedCampaignFound BIT = 0
			DECLARE @QualifiedStartTime DATETIME = DATEADD(HOUR,-6,GETUTCDATE())			
			DECLARE @QualifiedEndTime DATETIME = DATEADD(MINUTE,-15,GETUTCDATE())

			DECLARE @DelayedCampaignID INT
		
			;WITH Retries (CampaignID, RetryCount)
			AS
			(
				SELECT CampaignID,COUNT(1) FROM CampaignRetryAudit (NOLOCK)
					GROUP BY CampaignID
			) ,
			LastRetry (CampaignID,RetriedOn,CampaignRetryAuditID,CampaignStatus)
			AS
			(
				SELECT CampaignID,MAX(RetriedOn) RetriedOn,Max(CampaignRetryAuditID) CampaignRetryAuditID,CampaignStatus FROM CampaignRetryAudit (NOLOCK)
					GROUP BY CampaignID,CampaignStatus
			), 
			Camps (CampaignID, RetryCount, RetriedOn)
			AS
			( 
				SELECT C.CampaignID, ISNULL(CRA.RetryCount,0) RetryCount, ISNULL(LR.RetriedOn,cast(-53690 as datetime)) RetriedOn FROM Campaigns (NOLOCK) C    -- cast(-53690 as datetime) gets the min date. This is used to sort the retried on date accordingly
					LEFT JOIN Retries CRA  ON CRA.CampaignID = C.CampaignID
					LEFT JOIN LastRetry LR ON C.CampaignID = LR.CampaignID
				WHERE C.IsDeleted = 0 AND C.CampaignStatusID = @Delayed AND c.IsRecipientsProcessed = 1 AND C.ProcessedDate > @QualifiedStartTime
			)	
			SELECT * INTO #TTable FROM Camps
				WHERE (RetriedOn <= @QualifiedEndTime OR RetriedOn IS NULL) AND RetryCount < 3
		
	
			
			SET @DelayedCampaignID = (SELECT TOP 1 CAMPAIGNID FROM #TTable ORDER BY RetriedOn)
		
			IF(@DelayedCampaignID IS NOT NULL)
			BEGIN
				INSERT INTO CampaignRetryAudit VALUES (@DelayedCampaignID,GETUTCDATE(),117,'Retrying')
			END
			SELECT * FROM Campaigns (NOLOCK) WHERE CampaignID = @DelayedCampaignID
		
		END
END


GO


