
CREATE PROCEDURE [dbo].[GET_AccountsGrid]
AS
BEGIN
	SET NOCOUNT ON
	BEGIN TRY
		
		;WITH RepCount
		AS (
			SELECT CASE WHEN COUNT(1) > 1000 THEN 20 ELSE 0 END RCount, AccountID
				FROM dbo.Campaigns WITH (NOLOCK) 
				WHERE CampaignStatusID = 105 AND LastUpdatedOn >= DATEADD(dd,-30, GETUTCDATE()) AND LastUpdatedOn <= GETUTCDATE()
				GROUP BY AccountID
			), SentCampaigns
		AS
			(
			SELECT C.AccountID, CR.DeliveryStatus, CR.HasComplained, COUNT(CR.CampaignRecipientID) AS RecipientCount
				FROM dbo.CampaignRecipients CR WITH (NOLOCK)
					INNER JOIN dbo.Campaigns C WITH (NOLOCK) ON CR.CampaignID = C.CampaignID AND C.AccountID = CR.AccountID
				GROUP BY C.AccountID, CR.DeliveryStatus, CR.HasComplained
			), OpenRate
		AS
			(
			SELECT C.AccountID, COUNT(1) AS OpenRate 
				FROM dbo.CampaignStatistics CS WITH (NOLOCK)
					INNER JOIN dbo.CampaignRecipients CR (NOLOCK) ON CS.CampaignRecipientID = CR.CampaignRecipientID AND CS.CampaignID = CR.CampaignID AND CS.AccountID = CR.AccountID
					INNER JOIN dbo.Campaigns C WITH (NOLOCK) ON C.CampaignID = CR.CampaignID AND CR.AccountID = C.AccountID
				WHERE CS.ActivityType = 1 
				GROUP BY C.AccountID
			), SenRepCount
		AS
			(
			SELECT RC.AccountID, 
				CASE WHEN (Sum(case when SC.HasComplained = 1 then SC.RecipientCount else 0 end) + SUM(RC.RCount)) / SUM(SC.RecipientCount) < 1 THEN 20 ELSE 0 END +
				CASE WHEN (Sum(case when SC.DeliveryStatus = 113 then SC.RecipientCount else 0 end) + SUM(RC.RCount)) / SUM(SC.RecipientCount) < 1 THEN 20 ELSE 0 END +
				CASE WHEN (Sum(case when SC.DeliveryStatus = 112 then SC.RecipientCount else 0 end) + SUM(RC.RCount)) / SUM(SC.RecipientCount) < 1 THEN 20 ELSE 0 END SenderReputationCount
				FROM RepCount RC
					LEFT JOIN OpenRate ORA ON RC.AccountID = ORA.AccountID
					LEFT JOIN SentCampaigns SC ON RC.AccountID = SC.AccountID
				GROUP BY RC.AccountID
		)

		SELECT A.AccountID, A.IsDeleted, A.AccountName, A.ContactsCount, A.EmailsCount, A.[Status], 
			A.DomainURL, ISNULL(SR.SenderReputationCount, 0) SenderReputationCount
			FROM dbo.Accounts(NOLOCK) A 
				LEFT JOIN SenRepCount SR ON A.AccountID = SR.AccountID
			WHERE A.IsDeleted = 0

	END TRY
	BEGIN CATCH

	SELECT CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE()

	END CATCH
	SET NOCOUNT OFF

END

/*

  EXEC [dbo].[GET_AccountsGrid]
   
*/


