
-- =============================================
-- Author:		<Author,,Vadalisetty SurendraBabu>
-- Create date: <Create Date,,14-12-2016>
-- Description:	<Description,,For contact campaign engagement details>
-- =============================================
CREATE PROCEDURE [dbo].[Get_Contact_Campaign_Engagement_Details]
	@contactId int,
	@accountId int
	
AS
BEGIN
	   SELECT CR.CampaignRecipientID, C.CampaignID, C.Name, CR.DeliveredOn 
	   INTO #temp_ConctactCampaigns
	   FROM CampaignRecipients CR (NOLOCK)
	   INNER JOIN Campaigns C (NOLOCK) ON C.CampaignID = CR.CampaignID AND CR.AccountId = C.AccountID
	   WHERE CR.ContactID = @contactId AND CR.AccountId = @accountId AND CR.DeliveryStatus = 111

	   SELECT  CR.CampaignRecipientID, CS.ActivityType, CASE WHEN  CS.LinkIndex IS NOT NULL THEN CS.LinkIndex ELSE NULL END AS LinkIndex
	   INTO #temp_ContactCampaignStats
	   FROM CampaignStatistics CS (NOLOCK)
	   INNER JOIN #temp_ConctactCampaigns (NOLOCK) CR ON CR.CampaignRecipientID = CS.CampaignRecipientID

	   SELECT CampaignRecipientID, 1 as Opened, [2] as Clicked 
	   INTO #temp_CampaignStatistics
	   FROM 
	   (
		SELECT DISTINCT CampaignRecipientID, ActivityType, LinkIndex FROM #temp_ContactCampaignStats
	   ) src
	   PIVOT
	   (
			COUNT(LinkIndex)
			FOR ActivityType IN ([1], [2])
	   ) pv;
	   ;WITH CTE AS (
	   SELECT CR.CampaignID, CS.ActivityDate, ROW_NUMBER() OVER(PARTITION BY CR.CampaignID ORDER BY CS.ActivityDate DESC) RN
	   FROM CampaignStatistics CS (NOLOCK)
	   INNER JOIN #temp_ConctactCampaigns (NOLOCK) CR ON CR.CampaignRecipientID = CS.CampaignRecipientID)

	   SELECT CC.CampaignID, CC.Name AS CampaigName ,CC.DeliveredOn AS SentOn, COALESCE(CSS.Opened,0) AS OpenStatus,COALESCE( CSS.Clicked,0) AS LinksClicked, T.ActivityDate LastActivity,CC.CampaignRecipientID 
	   FROM #temp_ConctactCampaigns CC
	   LEFT JOIN #temp_CampaignStatistics CSS ON CSS.CampaignRecipientID = CC.CampaignRecipientID
	   LEFT JOIN CTE T ON T.CampaignID = CC.CampaignID AND RN = 1
	   ORDER BY CC.DeliveredOn DESC
END


/*
	EXEC [dbo].[Get_Contact_Campaign_Engagement_Details] 3927265,66
 */
