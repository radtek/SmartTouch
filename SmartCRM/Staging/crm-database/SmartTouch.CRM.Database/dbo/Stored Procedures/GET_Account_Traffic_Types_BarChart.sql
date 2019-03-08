

CREATE PROC [dbo].[GET_Account_Traffic_Types_BarChart]
	(
		@AccountID		int,
		@FromDate       datetime,
		@ToDate         datetime,
		@IsAdmin		tinyint,
		@OwnerID		int
	)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY	

	IF (@IsAdmin = 1)
		BEGIN
			/* Default 30 Days */
			SELECT	dv2.DropdownValue,dv2.DropdownValueID, COUNT(CTM.ContactID) TotalVisits, COUNT(DISTINCT CTM.ContactID) UniqueVisitors
			FROM	dbo.DropdownValues(NOLOCK) DVC
			        INNER JOIN dbo.Tours(NOLOCK) T ON T.CommunityID = DVC.DropdownValueID AND DVC.DropdownID = 7
					INNER JOIN dbo.ContactTourMap(NOLOCK) CTM ON CTM.TourID = T.TourID
					INNER JOIN dbo.DropdownValues(NOLOCK) dv2 on dvc.AccountID = dv2.AccountID and t.TourType = dv2.DropdownValueID AND dv2.DropdownID = 8
					INNER JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CTM.ContactID AND C.AccountID = T.AccountID
					INNER JOIN dbo.Users(NOLOCK) u on (u.accountid = DVC.accountid or u.AccountID = 1) and u.UserID = t.CreatedBy and u.Status=1
			WHERE	DVC.AccountID = @AccountID
					AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @FromDate AND @ToDate
					AND C.IsDeleted = 0
					AND DVC.IsActive=1
			GROUP BY dv2.DropdownValue,dv2.DropdownValueID
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			SELECT	dv2.DropdownValue,dv2.DropdownValueID, COUNT(CTM.ContactID) TotalVisits, COUNT(DISTINCT CTM.ContactID) UniqueVisitors
			FROM	dbo.DropdownValues(NOLOCK) DVC INNER JOIN dbo.Tours(NOLOCK) T ON T.CommunityID = DVC.DropdownValueID AND DVC.DropdownID = 7
					INNER JOIN dbo.ContactTourMap(NOLOCK) CTM ON CTM.TourID = T.TourID 
					INNER JOIN dbo.DropdownValues(NOLOCK) dv2 on dvc.AccountID = dv2.AccountID and t.TourType = dv2.DropdownValueID AND dv2.DropdownID = 8
					INNER JOIN dbo.Contacts(NOLOCK) C ON C.ContactID = CTM.ContactID  AND C.AccountID = T.AccountID
					INNER JOIN dbo.Users(NOLOCK) u on (u.accountid = DVC.accountid or u.AccountID = 1)and u.UserID = t.CreatedBy and u.Status=1
			WHERE	DVC.AccountID = @AccountID
					AND T.CreatedBy = @OwnerID
					AND C.IsDeleted = 0
					AND DVC.IsActive=1
					AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @FromDate AND @ToDate
			GROUP BY dv2.DropdownValue,dv2.DropdownValueID
		END
	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF


END

/*
	  EXEC [dbo].[GET_Account_Traffic_Types_BarChart]
		 @AccountID			= 45,
		 @FromDate          = '2014-11-01 00:00:00.000',       
		 @ToDate            = '2014-11-30 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100

*/












