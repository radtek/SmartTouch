CREATE  PROCEDURE [dbo].[GET_Account_Campagin_Reports_N](
	@AccountID int,
	@StartDate date,
	@EndDate date,
	@IsAdmin bit,
	@UserID int = 0
)
AS
BEGIN

SELECT	c.CampaignID, c.Name, Count(cr.contactID) as TotalSends,
SUM(CASE ActivityType WHEN 1 THEN 1 ELSE 0 END) as TotalOpens,
SUM(CASE ActivityType WHEN 2 THEN 1 ELSE 0 END) as TotalClicks,
SUM(CASE HasComplained WHEN 1 THEN 1 ELSE 0 END) as TotalCompliants,
SUM(CASE DeliveryStatus WHEN 111 THEN 1 ELSE 0 END) as TotalDelivered
into #tempData
FROM	dbo.Campaigns c (nolock)
INNER JOIN dbo.vCampaignRecipients cr (nolock) ON c.CampaignID = cr.CampaignID
LEFT JOIN dbo.vCampaignStatistics cs (nolock) ON cr.CampaignRecipientID = cs.CampaignRecipientID						    
WHERE	c.accountID = @AccountID AND c.IsDeleted = 0
AND CR.SentOn BETWEEN @StartDate AND @EndDate and (@IsAdmin = 1 OR C.CreatedBy = @UserID)
GROUP BY c.CampaignID, c.Name

select C.CampaignID, C.Name, TotalSends, TotalOpens, TotalClicks, TotalCompliants, TotalDelivered, mr.VMTA ProviderName, c.ScheduleTime from #tempData cd
join Campaigns c (nolock) on cd.CampaignID = c.CampaignID
INNER JOIN dbo.ServiceProviders sp (nolock) on  sp.ServiceProviderID = c.ServiceProviderID AND SP.AccountID = C.AccountID
INNER JOIN EnterpriseCommunication.dbo.MailRegistration mr (nolock) on mr.Guid = sp.LoginToken 
order by c.ScheduleTime desc

drop table #tempData

END

/*
EXEC GET_Account_Campagin_Reports_N
		@AccountID int = 4218,
		@StartDate date = DATEADD(dd, -30, GETDATE()),
		@EndDate date = DATEADD(dd, 1, GETDATE()),
		@IsAdmin = 1,
		@UserID = 0

EXEC GET_Account_Campagin_Reports
		@AccountID int = 1097,
		@StartDate date = DATEADD(dd, -30, GETDATE()),
		@EndDate date = DATEADD(dd, 1, GETDATE()),
		@IsAdmin = 0,
		@UserID = 5298

*/