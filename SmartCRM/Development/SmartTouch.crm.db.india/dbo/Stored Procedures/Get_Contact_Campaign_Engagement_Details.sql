
CREATE PROCEDURE [dbo].[Get_Contact_Campaign_Engagement_Details]
	@contactId int,
	@accountId int
	
AS
BEGIN
	   ;with TempCte
			  as
			  (
			  SELECT distinct C.Name AS CampaigName,CR.SentON,
	
			 ( select case when  Cs.CampaignRecipientID > 0 then   1 else 0 end )as OpenStatus,
			CASE WHEN cs.Activitytype is null  then 0
				 WHEN cs.Activitytype=1 then 0
				 WHEN cs.Activitytype=2 then Count ( DISTINCT cs.LinkIndex)END AS LinksClicked,cs.ActivityType
			FROM CampaignRecipients(nolock) CR 
			JOIN  Campaigns(nolock) C ON C.CampaignID = CR.CampaignID
			left   JOIN  CampaignStatistics(nolock) CS ON CR.CampaignRecipientID=CS.CampaignRecipientID  
			WHERE CR.ContactID =@contactId  AND Cr.AccountID = @accountId   AND CR.DeliveryStatus = 111
			GROUP BY C.Name, CR.CampaignID,CR.SentON,Cs.CampaignRecipientID,CS.ActivityType
			)
			Select CampaigName,SentON,OpenStatus,LinksClicked From (Select c.*, Row_number() over(partition by C.CampaigName  Order by C.CampaigName,LinksClicked DESC  )ronum
			from TempCte c)T where T.ronum!>1 
			ORDER BY T.SentOn DESC
END


/*
	EXEC [dbo].[Get_Contact_Campaign_Engagement_Details] 1741720,4218
 */



