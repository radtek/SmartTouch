

CREATE   PROCEDURE [dbo].[Update_AccountActivities]
AS
BEGIN


	;WITH cte 
	AS
	(   
	    SELECT distinct A.AccountID,COUNT(DISTINCT U.userid)as [Userscount],max(C.ProcessedDate)as ProcessedDate ,MAX(L.[AuditedOn]) AS LastLogin ,ROW_NUMBER() OVER(PARTITION BY a.AccountID ORDER BY ProcessedDate DESC) AS rn
		FROM Accounts A WITH (NOLOCK)
		left JOIN Users U WITH (NOLOCK) ON U.AccountID = A.AccountID  AND  U.Status =  1  AND U.IsDeleted = 0
		left JOIN Campaigns C WITH (NOLOCK) ON C.AccountID =  A.AccountID and c.IsDeleted = 0 AND C.IsLinkedToWorkflows = 0
		LEFT JOIN [dbo].[LoginAudit] L WITH (NOLOCK) ON L.UserID = U.UserID and u.AccountID = l.AccountID
		WHERE A.IsDeleted = 0 
	    GROUP BY  A.AccountID,C.ProcessedDate
	)
	SELECT * INTO #TEM FROM 
	(
	SELECT * FROM cte WHERE rn = 1
	)T

--SELECT * FROM #TEM

	UPDATE AA
	SET AA.[ActiveUsers] = t.[Userscount] ,
		AA.[LastCampaignSent] =  t.ProcessedDate,
        AA.[LastLogin] = T.LastLogin
	 FROM [dbo].[AccountActivities] AA
	INNER JOIN #TEM T ON T.AccountID = AA.AccountID



	INSERT INTO [dbo].[AccountActivities] ([AccountID],[ActiveUsers],[LastCampaignSent],[LastLogin])
	SELECT AccountID,[Userscount],ProcessedDate,LastLogin FROM #TEM 
	WHERE AccountID NOT IN (SELECT AccountID FROM [dbo].[AccountActivities])



END 


