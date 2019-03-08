
CREATE PROCEDURE [dbo].[GET_Account_Activity_Report_v1]
	(
		@AccountID int,
		@StartDate date,
		@EndDate date,
		@UserIDs varchar(MAX),
		@ModuleIDs varchar(4000) 
	)
AS
BEGIN

	DECLARE @LeadSourceList NVARCHAR(MAX),
			@LeadSourceList2 NVARCHAR(MAX),
			@LeadSourceListNull NVARCHAR(MAX),
			@ColumnList NVARCHAR(MAX)

	DECLARE @ResultID int

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_Activity_Report_v1', @AccountID)

	SET @ResultID = scope_identity()

	SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + ModuleName + ']',
			@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + ModuleName + '], 0) [' + ModuleName + ']',
			@LeadSourceList2 = COALESCE(@LeadSourceList2 + ' + ', '') + 'isnull([' + ModuleName + '], 0) '
			--@ColumnList = COALESCE(@ColumnList + '~', '') + ModuleName
	FROM	(
				SELECT	DISTINCT ModuleName
				FROM	dbo.Modules(NOLOCK)
				WHERE	ModuleID IN (
							SELECT DataValue FROM dbo.Split(@ModuleIDs, ',')
						)
			) tmp

		PRINT @LeadSourceList
		PRINT @LeadSourceList2
		PRINT @LeadSourceListNull

		--SELECT	@ColumnList ColumnList

		DECLARE @SQL nvarchar(MAX)

		SET @SQL = '
			SELECT	u.userid ID, isnull(firstname, '''') + '' '' + isnull(lastname, '''') Name, isnull(Total, 0) Total,
					' + @LeadSourceListNull + '
			FROM	dbo.users u left join (
						SELECT	UserID,
								(' + @LeadSourceList2 + ') Total,
								' + @LeadSourceList + '
						FROM	(
									SELECT	UAL.UserID, M.ModuleName, COUNT(UAL.UserActivityLogID) ContactsCount
									FROM	dbo.Modules M 
									        INNER JOIN dbo.UserActivityLogs UAL ON M.ModuleID = UAL.ModuleID  
											INNER JOIN dbo.Split(''' + @UserIDs + ''', '','') S ON S.DataValue = UAL.UserID
									WHERE	CONVERT(VARCHAR(10), UAL.LogDate, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
											AND UAL.UserActivityID = 1
											AND NOT EXISTS (select 1 from UserActivityLogs (NOLOCK) uali where  uali.AccountID = UAL.AccountID and uali.UserActivityID = 4 AND ual.EntityID = uali.EntityID AND uali.ModuleID = ual.ModuleID)  
											AND UAL.AccountID = ''' + CAST(@AccountID AS VARCHAR) +'''
											AND UAL.ModuleID IN (SELECT DataValue FROM dbo.Split(''' + @ModuleIDs + ''', '',''))
									GROUP BY UAL.UserID, M.ModuleID, M.ModuleName
								) tmp
						PIVOT(
							MAX(ContactsCount)
							FOR ModuleName IN ([Actions], [Campaigns], [Contacts], [Forms], [Notes], [Opportunity], [Tours])
						)p
					) tmp ON u.userID = tmp.UserID 

			WHERE	u.AccountID in (' + CAST(@AccountID AS VARCHAR) + ', 1) 
					AND U.UserID IN (SELECT DataValue FROM dbo.Split(''' + @UserIDs + ''', '','')) 
			ORDER BY Total desc, Name
		'

		PRINT @SQL

		EXEC sp_executesql @SQL

	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID

END

/*
		Get Acivity Report
		
	  EXEC [dbo].[GET_Account_Activity_Report_v1]

		 @AccountID			= 4218,
		 @StartDate         = '2015-05-24 12:00:00.000',       
		 @EndDate           = '2015-06-23 12:00:00.000',
		 @UserIDs			= 6887,
		 @ModuleIDs			= 4

		 

*/
GO


