
CREATE PROC [dbo].[GET_Account_TrafficByTourType_Lifcycle_Report]
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

		DECLARE @TourList		NVARCHAR(MAX),
				@TourList2	    NVARCHAR(MAX),
				@TourListNull   NVARCHAR(MAX),
				@SQLScript		NVARCHAR(MAX)


		SELECT	@TourList = COALESCE(@TourList + ', ', '') + '[' + DropdownValue + ']',
			    @TourListNull = COALESCE(@TourListNull + ', ', '') + 'isnull([' + DropdownValue + '], 0) [' + DropdownValue + ']',
			    @TourList2 = COALESCE(@TourList2 + ' + ', '') + 'isnull([' + DropdownValue + '], 0) '
			FROM( SELECT DISTINCT DropdownValue FROM dbo.DropdownValues(NOLOCK) 
					WHERE DropdownID = 8 AND (AccountID = @AccountID OR AccountID IS NULL)
				) Temp

		SET @SQLScript = '
			SELECT	ISNULL(FirstName, '''') + '' '' + ISNULL(LastName, '''') FullName,
					' + @TourListNull + ',
					ISNULL(Total, 0) Total
			FROM	DBO.Users(NOLOCK) U LEFT JOIN (
						SELECT	UserID,
								' + @TourList2 + '  Total,
								' + @TourList + '
						FROM	(
									SELECT	UserID, DropdownValue TourTypeName, count(1) TotalContacts
									FROM	(
												SELECT	U.UserID, DV.DropdownValue, c.ContactID
												FROM	dbo.Users(NOLOCK) U INNER JOIN DBO.DropdownValues DV ON U.AccountID = DV.AccountID
												        INNER JOIN dbo.Tours(NOLOCK) T ON T.TourType = DV.DropdownValueID														
														INNER JOIN dbo.ContactTourMap(NOLOCK) CTM ON CTM.TourID = T.TourID			                                           
														INNER JOIN dbo.Contacts(NOLOCK) C ON C.AccountID = U.AccountID AND C.OwnerID = U.UserID	AND C.ContactID = CTM.ContactID
												WHERE	U.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
														AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
														AND DV.DropdownID = 8
														AND U.UserID IN (SELECT DataValue FROM dbo.Split(''' + @UserIDs + ''', '',''))
											) Temp
									GROUP BY UserID, DropdownValue
								) Temp
						PIVOT(
							MAX(TotalContacts)
							FOR TourTypeName IN (' + @TourList + ')
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
	  EXEC [dbo].[GET_Account_TrafficByTourType_Lifcycle_Report]
		 @AccountID			=  45,
		 @StartDate         = '2014-03-16 23:27:06.000', 
		 @EndDate           = '2014-12-01 18:13:56.423',      
		 @UserIDs			= '11,49,50', 
		 @CommunityIDs		= ''

*/