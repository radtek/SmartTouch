Create  PROCEDURE [dbo].[Classic_Campaign_Sends_SP]
AS
BEGIN 

	DROP TABLE IF EXISTS  #t

	;WITH CTE
	AS
	(
	select DISTINCT A.AccountName,c.Name,ProcessedDate AS LastCampaignSentDate ,ROW_NUMBER() OVER(PARTITION BY VMTA   ORDER BY  ProcessedDate DESC) AS N,VMTA ,S.[ProviderName], 
	ISNULL(STUFF((SELECT ', ' + [SearchDefinitionName]
	FROM [SearchDefinitions] SSM
	INNER JOIN dbo.[CampaignSearchDefinitionMap] SUB ON SUB.[SearchDefinitionID] = SSM.[SearchDefinitionID]
	WHERE SUB.CampaignID = C.CampaignID
	FOR XML PATH('')
	), 1, 1, ''), '') AS [SearchDefinitionName],
	ISNULL(STUFF((SELECT ', ' + TagName
	FROM Tags TT
	INNER JOIN CampaignTagMap SUB ON SUB.TagID = TT.TagID
	WHERE SUB.CampaignID = C.CampaignID
	FOR XML PATH('')
	), 1, 1, ''), '') AS [TagName]  
	from Accounts  A
	INNER JOIN  Campaigns  C ON C.AccountID = A.AccountID
	INNER JOIN  ServiceProviders S ON S.ServiceProviderID = C.ServiceProviderID
	INNER JOIN [EnterpriseCommunication].[dbo].[MailRegistration]  MR ON MR.[Guid]  = S.LoginToken
	INNER JOIN  [dbo].[CampaignSearchDefinitionMap] CS ON CS.CampaignID =  C.CampaignID
	INNER JOIN  [dbo].[CampaignTagMap] CT ON  CT.CampaignID =  C.CampaignID
	INNER JOIN Tags  T ON T.TagID  = CT.TagID
	INNER JOIN [dbo].[SearchDefinitions] SD ON SD.[SearchDefinitionID] =  CS.[SearchDefinitionID]
	where  ProcessedDate is not null AND  VMTA IS NOT NULL AND VMTA ! = '' AND ProcessedDate ! = ''

	)
	SELECT * INTO #T  FROM  CTE  WHERE N = 1

	

	SELECT * INTO #Classic_Campaign_Sends from (
	SELECT AccountName,Name CampaignName,LastCampaignSentDate,VMTA VMTAName,[SearchDefinitionName],[TagName]  FROM  #T
	UNION 
	SELECT  AccountName,CampaignName,LastCampaignSentDate,VMTAName,''[SearchDefinitionName],''[TagName] FROM  Classic_Campaign_Sends 
	)t

	select  * from  #Classic_Campaign_Sends  ORDER BY LastCampaignSentDate DESC



END 

