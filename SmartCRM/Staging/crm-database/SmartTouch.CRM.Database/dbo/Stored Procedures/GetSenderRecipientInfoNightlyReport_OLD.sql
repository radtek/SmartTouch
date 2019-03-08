CREATE   PROCEDURE [dbo].[GetSenderRecipientInfoNightlyReport_OLD]
AS
BEGIN 

DECLARE @GetSenderRecipientInfoNightlyReport TABLE (AccountName NVARCHAR(100),SenderReputationCount INT ,CampaignsSent INT ,Recipients INT ,[Sent] INT ,Delivered INT ,Bounced int ,Opened int ,Clicked int,TagsAll INT,TagsActive INT ,SSAll INT ,SSActive INT)

INSERT INTO @GetSenderRecipientInfoNightlyReport (AccountName,SenderReputationCount,CampaignsSent,Recipients,[Sent],Delivered,Bounced,Opened,Clicked,TagsAll,TagsActive,SSAll,SSActive)
	SELECT a.AccountName
		,sr.SenderReputationCount
		,COUNT(c.CampaignId) CampaignsSent           
		, SUM(ca.Recipients) Recipients
		, SUM(ca.Sent) [Sent], SUM(ca.Delivered) Delivered
		, SUM(ca.Bounced) Bounced
		, SUM(ca.Opened) Opened
		, SUM(ca.Clicked) Clicked 
		, COUNT(CASE WHEN C.TagRecipients = 1 THEN 1 ELSE 0 END) TagsAll
		,( select  case when len(t.campaignid)>1 and C.TagRecipients = 2 THEN 1 ELSE 0 END)TagsActive
		,COUNT(CASE WHEN C.SSRecipients = 1 THEN 1 ELSE 0 END) SSAll
		,( select  case when len(m.campaignid)>1 and  C.SSRecipients = 2    THEN 1 ELSE 0 END)   SavedSearchActive
	FROM Campaigns (NOLOCK) c
	INNER JOIN CampaignAnalytics (NOLOCK) ca on ca.campaignid = c.campaignid
	INNER JOIN Accounts a (NOLOCK) on a.accountid = c.accountid
	CROSS APPLY (SELECT * FROM [dbo].[GetReputationInfo](a.AccountID, DATEADD(dd,-1,GETUTCDATE()), GETUTCDATE()) sri where sri.AccountID = a.AccountID) as sr
	left  join [dbo].[CampaignSearchDefinitionMap] m on m.CampaignID = c.CampaignID
	left join CampaignContactTagMap t on t.CampaignID = c.CampaignID
	WHERE c.ProcessedDate > DATEADD(dd,-1,GETUTCDATE()) and c.IsLinkedToWorkflows = 0
	GROUP BY a.AccountName, sr.SenderReputationCount,t.campaignid,C.TagRecipients,m.campaignid,C.SSRecipients

SELECT * INTO #TEMP FROM 
(
select AccountName,SenderReputationCount,CampaignsSent,Recipients,[Sent],Delivered,
CASE WHEN Bounced <> 0 THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Bounced)/ (convert (decimal (10,2),REPLACE(Recipients,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Bounced as varchar(50)) AS Bounced,
CASE WHEN Opened <> 0 THEN (cast((CONVERT (INT,(convert (decimal (10,2),Opened)/ (convert (decimal (10,2),REPLACE(Delivered,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+(cast(Opened as varchar(100))) as Opened,
CASE WHEN Clicked <> 0 THEN (cast((CONVERT (INT,(convert (decimal (10,2),Clicked)/ (convert (decimal (10,2),REPLACE(Delivered,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+ +'|'+(cast(Clicked as varchar(100))) as Clicked ,TagsAll,TagsActive,SSAll,SSActive from  @GetSenderRecipientInfoNightlyReport
)T

SELECT * FROM #TEMP

DECLARE @GetSenderRecipientInfoNightlyReport1 TABLE (AccountName NVARCHAR(100),SenderReputationCount INT ,CampaignsSent INT ,Recipients INT ,[Sent] INT ,Delivered INT ,Bounced INT ,Opened INT ,Clicked INT,TagsAll INT,TagsActive INT ,SSAll INT ,SSActive INT)

INSERT INTO @GetSenderRecipientInfoNightlyReport1 (AccountName,SenderReputationCount,CampaignsSent,Recipients,[Sent],Delivered,Bounced,Opened,Clicked,TagsAll,TagsActive,SSAll,SSActive)
	SELECT a.AccountName
		,sr.SenderReputationCount
		,COUNT(c.CampaignId) CampaignsSent           
		, SUM(ca.Recipients) Recipients
		, SUM(ca.Sent) [Sent], SUM(ca.Delivered) Delivered
		, SUM(ca.Bounced) Bounced
		, SUM(ca.Opened) Opened
		, SUM(ca.Clicked) Clicked 
		, sum(CASE WHEN C.TagRecipients = 1 THEN 1 ELSE 0 END) TagsAll
		,sum(   case when len(t.campaignid)>1 and C.TagRecipients = 2 THEN 1 ELSE 0 END)TagsActive
		,sum(CASE WHEN C.SSRecipients = 1 THEN 1 ELSE 0 END) SSAll
		,sum(   case when len(m.campaignid)>1 and  C.SSRecipients = 2    THEN 1 ELSE 0 END)   SavedSearchActive
	FROM Campaigns (NOLOCK) c
	INNER JOIN CampaignAnalytics (NOLOCK) ca on ca.campaignid = c.campaignid
	INNER JOIN Accounts a (NOLOCK) on a.accountid = c.accountid
	CROSS APPLY (SELECT * FROM [dbo].[GetReputationInfo](a.AccountID, DATEADD(dd,-7,GETUTCDATE()), GETUTCDATE()) sri where sri.AccountID = a.AccountID) as sr
	left  join [dbo].[CampaignSearchDefinitionMap] m on m.CampaignID = c.CampaignID
	left join CampaignContactTagMap t on t.CampaignID = c.CampaignID
	WHERE c.ProcessedDate > DATEADD(dd,-7,GETUTCDATE()) and c.IsLinkedToWorkflows = 0 AND A.IsDeleted = 0 and A.Status = 1
	GROUP BY a.AccountName, sr.SenderReputationCount
	ORDER BY sr.SenderReputationCount ASC

SELECT * INTO #TEMP_1 FROM 
(
select AccountName,SenderReputationCount,CampaignsSent,Recipients,[Sent],Delivered,
CASE WHEN Bounced <> 0 THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Bounced)/ (convert (decimal (10,2),REPLACE(Recipients,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Bounced as varchar(50)) AS Bounced,
CASE WHEN Opened <> 0 THEN (cast((CONVERT (INT,(convert (decimal (10,2),Opened)/ (convert (decimal (10,2),REPLACE(Delivered,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+(cast(Opened as varchar(100))) as Opened,
CASE WHEN Clicked <> 0 THEN (cast((CONVERT (INT,(convert (decimal (10,2),Clicked)/ (convert (decimal (10,2),REPLACE(Delivered,0,1)))  * 100))) as varchar(200)))ELSE CAST(0 AS VARCHAR(50))END+'%'+ +'|'+(cast(Clicked as varchar(100))) as Clicked ,TagsAll,TagsActive,SSAll,SSActive from  @GetSenderRecipientInfoNightlyReport1
)TT

SELECT * FROM #TEMP_1

END