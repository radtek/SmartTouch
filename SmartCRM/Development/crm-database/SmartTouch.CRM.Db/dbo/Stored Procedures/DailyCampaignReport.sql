CREATE  PROCEDURE [dbo].[DailyCampaignReport ]
   @StartDate DATETIME = NULL,
   @EndDate DATETIME = NULL,
   @AccountIds VARCHAR(100) = '',
   @Take INT = 0, @Skip INT = 0
AS
BEGIN 

DECLARE @CampaignReport TABLE (AccountName NVARCHAR(100),CampaignId INT, CampaignSubject nvarchar(200),[ServiceProvider] varchar(50), Recipients INT ,[Sent] INT ,Delivered INT ,Bounced INT ,Opened INT ,Clicked INT,Complained INT ,TagsAll INT,TagsActive INT ,SavedSearchAll INT ,SavedSearchActive INT)
CREATE TABLE #dailyCampaignReport  (Day int,AccountName NVARCHAR(100),CampaignId INT, CampaignSubject nvarchar(200),[Vmta] varchar(50), Recipients INT ,[Sent] INT ,Delivered varchar(20) ,Bounced  varchar(20) ,Opened  varchar(20) ,Clicked  varchar(20),Complained  varchar(20) ,TagsAll INT,TagsActive INT ,SavedSearchAll INT ,SavedSearchActive INT)


IF @AccountIds <>''
BEGIN

DECLARE @AccountID TABLE(AccountID INT)

INSERT INTO @AccountID (AccountID)
SELECT Datavalue  From dbo.Split (@AccountIdS ,',')

	
		INSERT INTO @CampaignReport (AccountName,CampaignId,CampaignSubject,[ServiceProvider], Recipients,[Sent],Delivered,Bounced,Opened,Clicked,Complained, TagsAll,TagsActive,SavedSearchAll,SavedSearchActive)
		SELECT a.AccountName [AccountName]
					  , c.CampaignId CampaignId
					  ,c.Subject [CampaignSubject]
					  ,mr.VMTA [ServiceProvider]
					  , (ca.Recipients) Recipients
					  , (ca.Sent) [Sent], (ca.Delivered) Delivered
					  , (ca.Bounced) Bounced
					  , (ca.Opened) Opened
					  , (ca.Clicked) Clicked
					  ,ca.Complained Complained
					  , (CASE WHEN C.TagRecipients = 1 THEN 1 ELSE 0 END) TagsAll
					  ,( select  case when len(t.campaignid)>1 and C.TagRecipients = 2 THEN 1 ELSE 0 END)TagsActive
					  ,(CASE WHEN C.SSRecipients = 1 THEN 1 ELSE 0 END) SavedSearchAll
					  ,( select  case when len(m.campaignid)>1 and  C.SSRecipients = 2    THEN 1 ELSE 0 END)   SavedSearchActive
			   FROM Campaigns (NOLOCK) c
			   INNER JOIN CampaignAnalytics (NOLOCK) ca on ca.campaignid = c.campaignid
			   INNER JOIN Accounts a (NOLOCK) on a.accountid = c.accountid
			   INNER JOIN ServiceProviders SP (NOLOCK) ON SP.ServiceProviderID = c.ServiceProviderID
			   INNER JOIN EnterpriseCommunication.dbo.MailRegistration mr ON mr.guid = sp.LoginToken
			   left  join [dbo].[CampaignSearchDefinitionMap] m on m.CampaignID = c.CampaignID
			   left join CampaignContactTagMap t on t.CampaignID = c.CampaignID
			   INNER JOIN @AccountID aa on aa.AccountID = a.AccountID
			   WHERE c.ProcessedDate between  @StartDate and   @EndDate and c.IsLinkedToWorkflows = 0 AND c.CampaignStatusID = 105


		INSERT INTO  #dailyCampaignReport (CampaignId ,AccountName , CampaignSubject ,[Vmta] , Recipients  ,[Sent]  ,Delivered  ,Bounced  ,Opened   ,Clicked  ,Complained  ,TagsAll,TagsActive ,SavedSearchAll ,SavedSearchActive)
			select distinct CampaignId [CampaignId], AccountName [Account Name], 
					CampaignSubject [Campaign Subject], 
					ISNULL(ServiceProvider, '') [Service Provider],
					Recipients,
					[Sent],
					CASE WHEN Delivered <> 0 AND [Sent] <>0
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Delivered)/ (convert (decimal (10,2),[Sent]))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Delivered as varchar(50)) AS Delivered,
					CASE WHEN Bounced <> 0  AND [Sent] <>0
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Bounced)/ (convert (decimal (10,2),[Recipients]))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Bounced as varchar(50)) AS Bounced,
					CASE WHEN Opened <> 0 AND Delivered <>0 
						THEN (cast((CONVERT (INT,(convert (decimal (10,2),Opened)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+(cast(Opened as varchar(100))) as Opened,
					CASE WHEN Clicked <> 0  AND Delivered <>0 
						THEN (cast((CONVERT (INT,(convert (decimal (10,2),Clicked)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+ +'|'+(cast(Clicked as varchar(100))) as Clicked ,
					CASE WHEN Complained <> 0  AND Delivered <>0 
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Complained)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Complained as varchar(50)) AS Complained,
					TagsAll [Tags All],
					TagsActive [Tags Active],
					SavedSearchAll [SavedSearch All],
					SavedSearchActive [SavedSearch Active] 
					from  @CampaignReport
		

SELECT * FROM #dailyCampaignReport ORDER BY  CampaignId desc

END

ELSE IF isnull(@AccountIds,'') = ''

BEGIN 

DECLARE @Days TABLE (Id INT, Period INT, PeriodDate DATETIME)

INSERT INTO @Days 
SELECT 1,  1, DATEADD(dd,-1,GETUTCDATE())
UNION
SELECT 2, 7, DATEADD(dd,-7,GETUTCDATE())



DECLARE @counter int = 1
DECLARE @day int = 1
DECLARE @perioddDay datetime

WHILE @counter < 3
	BEGIN
		SELECT @day = @counter, @perioddDay =PeriodDate FROM @Days WHERE Id = @counter
		
		INSERT INTO @CampaignReport (AccountName,CampaignId,CampaignSubject,[ServiceProvider], Recipients,[Sent],Delivered,Bounced,Opened,Clicked,Complained, TagsAll,TagsActive,SavedSearchAll,SavedSearchActive)
		SELECT a.AccountName [AccountName]
					  , c.CampaignId CampaignId
					  ,c.Subject [CampaignSubject]
					  ,mr.VMTA [ServiceProvider]
					  , (ca.Recipients) Recipients
					  , (ca.Sent) [Sent], (ca.Delivered) Delivered
					  , (ca.Bounced) Bounced
					  , (ca.Opened) Opened
					  , (ca.Clicked) Clicked
					  ,ca.Complained Complained
					  , (CASE WHEN C.TagRecipients = 1 THEN 1 ELSE 0 END) TagsAll
					  ,( select  case when len(t.campaignid)>1 and C.TagRecipients = 2 THEN 1 ELSE 0 END)TagsActive
					  ,(CASE WHEN C.SSRecipients = 1 THEN 1 ELSE 0 END) SavedSearchAll
					  ,( select  case when len(m.campaignid)>1 and  C.SSRecipients = 2    THEN 1 ELSE 0 END)   SavedSearchActive
			   FROM Campaigns (NOLOCK) c
			   INNER JOIN CampaignAnalytics (NOLOCK) ca on ca.campaignid = c.campaignid
			   INNER JOIN Accounts a (NOLOCK) on a.accountid = c.accountid
			   INNER JOIN ServiceProviders SP (NOLOCK) ON SP.ServiceProviderID = c.ServiceProviderID
			   INNER JOIN EnterpriseCommunication.dbo.MailRegistration mr ON mr.guid = sp.LoginToken
			   left  join [dbo].[CampaignSearchDefinitionMap] m on m.CampaignID = c.CampaignID
			   left join CampaignContactTagMap t on t.CampaignID = c.CampaignID
				WHERE c.ProcessedDate > @perioddDay and c.IsLinkedToWorkflows = 0 AND c.CampaignStatusID =105


		INSERT INTO  #dailyCampaignReport (CampaignId,Day ,AccountName , CampaignSubject ,[VMTA] , Recipients  ,[Sent]  ,Delivered  ,Bounced  ,Opened   ,Clicked  ,Complained  ,TagsAll,TagsActive ,SavedSearchAll ,SavedSearchActive)
			select distinct CampaignId [CampaignId],@day [Day], AccountName [Account Name], 
					CampaignSubject [Campaign Subject], 
					ServiceProvider [Service Provider],
					Recipients,
					[Sent],
					CASE WHEN Delivered <> 0 AND [Sent] <>0
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Delivered)/ (convert (decimal (10,2),[Sent]))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Delivered as varchar(50)) AS Delivered,
					CASE WHEN Bounced <> 0  AND [Sent] <>0
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Bounced)/ (convert (decimal (10,2),[Recipients]))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Bounced as varchar(50)) AS Bounced,
					CASE WHEN Opened <> 0 AND Delivered <>0 
						THEN (cast((CONVERT (INT,(convert (decimal (10,2),Opened)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+(cast(Opened as varchar(100))) as Opened,
					CASE WHEN Clicked <> 0  AND Delivered <>0 
						THEN (cast((CONVERT (INT,(convert (decimal (10,2),Clicked)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+ +'|'+(cast(Clicked as varchar(100))) as Clicked ,
					CASE WHEN Complained <> 0  AND Delivered <>0 
						THEN(Cast ((CONVERT (INT,(convert (decimal (10,2),Complained)/ (convert (decimal (10,2),Delivered))  * 100))) as varchar(200)))
						ELSE CAST(0 AS VARCHAR(50))END+'%'+  +'|'+Cast(Complained as varchar(50)) AS Complained,
					TagsAll [Tags All],
					TagsActive [Tags Active],
					SavedSearchAll [SavedSearch All],
					SavedSearchActive [SavedSearch Active] 
					from  @CampaignReport
		

		SET @counter = @counter + 1
	END


SELECT * FROM #dailyCampaignReport ORDER BY  CampaignId desc

END 


END 
