

--CREATE  PROCEDURE [Update_AccountActivities_old]
--AS
--BEGIN


--	;WITH cte 
--	AS
--	(
--		SELECT distinct A.AccountID,A.CreatedOn,A.ModifiedOn,COUNT(userid)as [Userscount],A.SubscriptionID,C.[CreatedDate],ROW_NUMBER() OVER(PARTITION BY a.AccountID ORDER BY CreatedDate DESC) AS rn
--		FROM Accounts A 
--		left JOIN Users U ON U.AccountID = A.AccountID
--		left JOIN Campaigns C ON C.AccountID =  A.AccountID
--		WHERE A.IsDeleted = 0 AND A.[Status] = 1 and U.IsDeleted = 0  and u.[Status] = 1
--		and c.IsDeleted = 0 
--		GROUP BY  A.AccountID,A.CreatedOn,A.ModifiedOn,A.SubscriptionID,C.[CreatedDate]
--	)
--	SELECT * INTO #TEM FROM 
--	(
--	SELECT * FROM cte WHERE rn = 1
--	)T

----SELECT * FROM #TEM

--	UPDATE AA
--	SET AA.[ActiveUsers] = t.[Userscount] ,
--		AA.[AccountCreatedDate] =  t.CreatedOn,
--		AA.[LastUpdatedOn] =  t.ModifiedOn,
--		AA.[SubscriptionID] = t.SubscriptionID ,
--		AA.[LastCampaignSent] =  t.CreatedDate
--	 FROM [dbo].[AccountActivities] AA
--	INNER JOIN #TEM T ON T.AccountID = AA.AccountID



--	INSERT INTO [dbo].[AccountActivities] ([AccountID],[AccountCreatedDate],[ActiveUsers],[LastUpdatedOn],[SubscriptionID],[LastCampaignSent])
--	SELECT AccountID,CreatedOn,[Userscount],ModifiedOn,SubscriptionID,CreatedDate FROM #TEM 
--	WHERE AccountID NOT IN (SELECT AccountID FROM [dbo].[AccountActivities])



--END 


