﻿
CREATE PROC [dbo].[GET_Account_TrafficByLifcycle_Report]
	(
		@AccountID			int,
		@StartDate          datetime,
		@EndDate            datetime,
		@UserIDs			VARCHAR(MAX),
		@CommunityIDs		VARCHAR(MAX)
	)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY

		DECLARE @LeadSourceList		NVARCHAR(MAX),
				@LeadSourceList2	NVARCHAR(MAX),
				@LeadSourceListNull NVARCHAR(MAX),
				@SQLScript			NVARCHAR(MAX)


		SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + DropdownValue + ']',
			@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + DropdownValue + '], 0) [' + DropdownValue + ']',
			@LeadSourceList2 = COALESCE(@LeadSourceList2 + ' + ', '') + 'isnull([' + DropdownValue + '], 0) '
			FROM( SELECT DISTINCT DropdownValue FROM dbo.DropdownValues(NOLOCK)
					WHERE DropdownID = 3 AND (AccountID = @AccountID OR AccountID IS NULL)
				) Temp

		SET @SQLScript = '
			SELECT	ISNULL(FirstName, '''') + '' '' + ISNULL(LastName, '''') FullName,
					' + @LeadSourceListNull + ',
					ISNULL(Total, 0) Total
			FROM	DBO.Users(NOLOCK) U LEFT JOIN (
						SELECT	UserID,
								' + @LeadSourceList2 + '  Total,
								' + @LeadSourceList + '
						FROM	(
									SELECT	UserID, DropdownValue LeadSourceName, count(1) TotalContacts
									FROM	(
												SELECT	U.UserID, DV.DropdownValue, c.ContactID
												FROM	dbo.Users(NOLOCK) U INNER JOIN DBO.DropdownValues DV ON U.AccountID = DV.AccountID
														INNER JOIN dbo.Contacts(NOLOCK) C ON C.AccountID = U.AccountID AND C.OwnerID = U.UserID AND C.LifecycleStage= DV.DropdownValueID
														--INNER JOIN dbo.ContactLeadSourceMap CSM ON CSM.ContactID = C.ContactID AND CSM.LeadSouceID = DV.DropdownValueID
												WHERE	U.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
														AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
														AND DV.DropdownID = 3
														AND U.UserID IN (SELECT DataValue FROM dbo.Split(''' + @UserIDs + ''', '',''))
											) Temp
									GROUP BY UserID, DropdownValue
								) Temp
						PIVOT(
							MAX(TotalContacts)
							FOR LeadSourceName IN (' + @LeadSourceList + ')
						)p
				) tmp ON u.UserID = tmp.userid
			WHERE	U.UserID IN (SELECT DataValue FROM dbo.Split(''' + @UserIDs + ''' , '',''))
			ORDER BY Total desc, FullName'

		PRINT @SQLScript
		EXEC sp_executesql @SQLScript

	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF


END

/*
	  EXEC [dbo].[GET_Account_TrafficByLifcycle_Report]
		 @AccountID			= 1097,
		 @StartDate         = '2014-11-01 00:00:00.000', 
		 @EndDate           = '2014-11-30 00:00:00.000',      
		 @UserIDs			= '70', 
		 @CommunityIDs		= ''

*/






GO


