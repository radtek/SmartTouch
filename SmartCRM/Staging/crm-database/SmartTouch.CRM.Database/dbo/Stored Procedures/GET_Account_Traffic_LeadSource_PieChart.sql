
CREATE PROC [dbo].[GET_Account_Traffic_LeadSource_PieChart]
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

	DECLARE @PrevFromDate	datetime = DATEADD(day, -30, @FromDate),
			@PrevToDate		datetime = DATEADD(day, -30, @ToDate)

	IF (@IsAdmin = 1)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,Dv.DropdownValueID,  COUNT(CLM.ContactLeadSourceMapID) TotalCount 
				FROM dbo.Contacts(NOLOCK) C
					INNER JOIN dbo.ContactLeadSourceMap (NOLOCK) CLM ON C.ContactID = CLM.ContactID and CLM.IsPrimaryLeadSource=1
					INNER JOIN dbo.DropdownValues  (NOLOCK) DV ON DV.DropdownValueID = CLM.LeadSouceID AND DV.AccountID = C.AccountID
					INNER JOIN dbo.DropdownValues  (NOLOCK) DVC ON DVC.AccountID = @AccountID AND DVC.DropdownID = 7
					INNER JOIN dbo.Tours  (NOLOCK) T ON T.CommunityID = DVC.DropdownValueID AND T.AccountID = C.AccountID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) CTM ON CTM.ContactID = C.ContactID and T.TourID = CTM.TourID
					INNER JOIN dbo.Users  (NOLOCK) u on (u.accountid = C.accountid or u.AccountID=1) and u.UserID = t.CreatedBy and u.Status=1
			WHERE C.AccountID = @AccountID
			    AND C.IsDeleted = 0 AND C.IncludeInReports=1
			    AND DV.DropdownID = 5
				And dv.IsActive=1
			--	AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) >= CONVERT(VARCHAR(10), @FromDate, 120)
			--	AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) <= CONVERT(VARCHAR(10), @ToDate, 120)
				AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @FromDate, 120)
				AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @ToDate, 120)
			GROUP BY LeadSouceID, DV.DropdownValue,DV.DropdownValueID
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,DV.DropdownValueID, COUNT(CLM.ContactLeadSourceMapID) TotalCount 
				FROM dbo.Contacts(NOLOCK) C
					INNER JOIN dbo.ContactLeadSourceMap  (NOLOCK) CLM ON C.ContactID = CLM.ContactID and CLM.IsPrimaryLeadSource=1
					INNER JOIN dbo.DropdownValues  (NOLOCK) DV ON DV.DropdownValueID = CLM.LeadSouceID AND DV.AccountID = C.AccountID
					INNER JOIN dbo.DropdownValues  (NOLOCK) DVC ON DVC.AccountID = @AccountID AND DVC.DropdownID = 7
					INNER JOIN dbo.Tours (NOLOCK)  T ON T.CommunityID = DVC.DropdownValueID AND T.AccountID = C.AccountID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) CTM ON CTM.ContactID = C.ContactID  and T.TourID = CTM.TourID
			        WHERE C.AccountID = @AccountID
					 AND C.IsDeleted = 0 AND C.IncludeInReports=1
					  AND DV.DropdownID = 5 
					  AND C.OwnerID = @OwnerID
					  And dv.IsActive=1
			--	AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) >= CONVERT(VARCHAR(10), @FromDate, 120)
			--	AND CONVERT(VARCHAR(10), C.LastUpdatedOn, 120) <= CONVERT(VARCHAR(10), @ToDate, 120)
				AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @FromDate, 120)
				AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @ToDate, 120)
			GROUP BY LeadSouceID, DV.DropdownValue,DV.DropdownValueID
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
	  EXEC [dbo].[GET_Account_Traffic_LeadSource_PieChart]
		 @AccountID			= 339,
		 @FromDate          = '11/05/2016 6:30:00 PM',       
		 @ToDate            = '12/05/2016 6:29:00 PM',
		 @IsAdmin			= 1,
		 @OwnerID			= 233

*/
