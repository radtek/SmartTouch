
CREATE PROCEDURE [dbo].[GET_Account_Campagin_Reports](
	@AccountID int,
	@StartDate date,
	@EndDate date,
	@IsAdmin bit,
	@UserID int = 0,
	@campaignId  int = 0
)
AS
BEGIN

     IF (@campaignId = 0)
			BEGIN
				 ;WITH Camps AS(
			SELECT C.Name,COALESCE(C.Subject,'') as Subject, CA.Clicked TotalClicks,CA.Opened TotalOpens,CA.Delivered TotalDelivered,CA.Sent TotalSends,CA.Complained TotalCompliants,c.CampaignID,
			COALESCE(sp.ProviderName +  ' - ' + mr.VMTA, '') ProviderName,
			COALESCE(C.ServiceProviderCampaignID,'') AS JobId, C.ProcessedDate AS ProcessedDate, C.ScheduleTime
			FROM Campaigns(NOLOCK) C
			INNER JOIN CampaignAnalytics(NOLOCK) CA ON C.CampaignID = CA.CampaignID 
			INNER JOIN dbo.ServiceProviders sp (nolock) on  sp.ServiceProviderID = c.ServiceProviderID AND SP.AccountID = C.AccountID
			INNER JOIN EnterpriseCommunication.dbo. MailRegistration mr (nolock) on mr.Guid = sp.LoginToken 
			WHERE	c.accountID = @AccountID AND c.IsDeleted = 0 AND c.CampaignStatusID != 109
			AND CAST(C.ProcessedDate AS DATE) BETWEEN @StartDate AND @EndDate
			AND (@IsAdmin = 1 OR C.CreatedBy = @UserID) )

			, WorkflowCamp AS (
			SELECT C.CampaignID, COUNT(DISTINCT w.WorkflowID) AS WorkflowsCount FROM Campaigns (nolock) C
			INNER JOIN WorkflowCampaignActions (nolock) WCA ON WCA.CampaignID = C.CampaignID
			INNER JOIN WorkflowActions (nolock) WA ON WA.WorkflowActionID = WCA.WorkflowActionID 
			INNER JOIN Workflows (nolock) W ON W.WorkflowID = WA.WorkflowID
			WHERE c.accountID = @AccountID AND c.IsDeleted = 0 AND c.CampaignStatusID != 109
			AND CAST(C.ProcessedDate AS DATE) BETWEEN @StartDate AND @EndDate
			AND (@IsAdmin = 1 OR C.CreatedBy = @UserID)
			GROUP BY C.CampaignID)

			SELECT C.*, ISNULL(WC.WorkflowsCount, 0) AS WorkflowsCount FROM Camps  C
			LEFT JOIN WorkflowCamp WC ON WC.CampaignID = C.CampaignID
			ORDER BY C.ProcessedDate DESC
			END
			ELSE
			BEGIN
				SELECT W.WorkflowName FROM Campaigns (nolock) C
				INNER JOIN WorkflowCampaignActions (nolock) WCA ON WCA.CampaignID = C.CampaignID
				INNER JOIN WorkflowActions (nolock) WA ON WA.WorkflowActionID = WCA.WorkflowActionID 
				INNER JOIN Workflows (nolock) W ON W.WorkflowID = WA.WorkflowID
				WHERE C.CampaignID = @campaignId
				GROUP BY W.WorkflowName
			END



	--IF (@IsAdmin = 1)
	--BEGIN

	

	--	SELECT	CampaignID, Name, COUNT(ContactID) TotalSends, SUM(Opens) TotalOpens, SUM(Clicks) TotalClicks,SUM(Abused) TotalCompliants,
	--			(SELECT COUNT(ContactID) FROM dbo.CampaignRecipients WHERE CampaignID = tmp.CampaignID AND DeliveryStatus = 111) TotalDelivered,VMTA ProviderName
	--	FROM	(
	--				SELECT	c.CampaignID, c.Name, cr.contactID,
	--						Opens = CASE ActivityType WHEN 1 THEN 1 ELSE 0 END,
	--						Clicks = CASE ActivityType WHEN 2 THEN 1 ELSE 0 END,
	--						Delivered = CASE DeliveryStatus WHEN 111 THEN 1 ELSE 0 END,
	--						Abused = CASE HasComplained WHEN 1 THEN 1 ELSE 0 END,
	--						cr.SentOn,mr.VMTA
	--				FROM	dbo.Campaigns c 
	--						INNER JOIN dbo.CampaignRecipients cr ON c.CampaignID = cr.CampaignID
	--				        INNER JOIN dbo.ServiceProviders sp on  sp.ServiceProviderID = c.ServiceProviderID AND SP.AccountID = C.AccountID
	--					    INNER JOIN EnterpriseCommunication.dbo.MailRegistration mr on mr.Guid = sp.LoginToken
	--						LEFT JOIN dbo.CampaignStatistics cs ON cr.CampaignRecipientID = cs.CampaignRecipientID						    
	--				WHERE	c.accountID = @AccountID AND c.IsDeleted = 0
	--						AND CAST(SentOn AS DATE) BETWEEN @StartDate AND @EndDate
	--				GROUP BY c.CampaignID, c.Name, cr.contactID, ActivityType, DeliveryStatus, cr.SentOn,HasComplained, mr.VMTA
	--			) tmp
	--	GROUP BY CampaignID, Name,VMTA
	--	ORDER BY MAX(SentOn) DESC 
	--END
	--ELSE
	--BEGIN
	--	SELECT	CampaignID, Name, COUNT(ContactID) TotalSends, SUM(Opens) TotalOpens, SUM(Clicks) TotalClicks,SUM(Abused) TotalCompliants,
	--		(SELECT COUNT(ContactID) FROM dbo.CampaignRecipients WHERE CampaignID = tmp.CampaignID AND DeliveryStatus = 111) TotalDelivered,VMTA ProviderName
	--	FROM	(
	--				SELECT	c.CampaignID, c.Name, cr.contactID,
	--						Opens = CASE ActivityType WHEN 1 THEN 1 ELSE 0 END,
	--						Clicks = CASE ActivityType WHEN 2 THEN 1 ELSE 0 END,
	--						Delivered = CASE DeliveryStatus WHEN 111 THEN 1 ELSE 0 END,
	--						Abused = CASE HasComplained WHEN 1 THEN 1 ELSE 0 END,
	--						SentOn,mr.VMTA
	--				FROM	dbo.Campaigns c 
	--						INNER JOIN dbo.CampaignRecipients cr ON c.CampaignID = cr.CampaignID
	--				        INNER JOIN dbo.ServiceProviders sp on  sp.ServiceProviderID = c.ServiceProviderID AND SP.AccountID = C.AccountID
	--						INNER JOIN EnterpriseCommunication.dbo.MailRegistration mr on mr.Guid = sp.LoginToken
	--						LEFT JOIN dbo.CampaignStatistics cs ON cr.CampaignRecipientID = cs.CampaignRecipientID
	--				WHERE	c.accountID = @AccountID AND c.IsDeleted = 0
	--						AND c.CreatedBy = @UserID
	--						AND CAST(SentOn AS DATE) BETWEEN @StartDate AND @EndDate
	--				GROUP BY c.CampaignID, c.Name, cr.contactID, ActivityType, DeliveryStatus, cr.SentOn,HasComplained,mr.VMTA
	--			) tmp
	--	GROUP BY CampaignID, Name,VMTA
	--	ORDER BY MAX(SentOn) DESC 
	--END
END

/*
EXEC GET_Account_Campagin_Reports
		@AccountID int = 4218,
		@StartDate date = DATEADD(dd, -30, GETDATE()),
		@EndDate date = DATEADD(dd, 1, GETDATE()),
		@IsAdmin = 1,
		@UserID = 0


		declare @AccountID int = 66,
		@StartDate datetime,
		@EndDate datetime,
		@IsAdmin bit = 1,
		@UserID int = 0
		set @StartDate = DATEADD(dd, -30, GETUTCDATE())
		set @EndDate  = DATEADD(dd, 1, GETUTCDATE())
		EXEC GET_Account_Campagin_Reports @AccountID, @StartDate, @EndDate, @IsAdmin, @UserId

*/