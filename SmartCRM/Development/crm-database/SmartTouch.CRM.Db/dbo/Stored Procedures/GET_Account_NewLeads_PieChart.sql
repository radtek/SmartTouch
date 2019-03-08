
CREATE PROC [dbo].[GET_Account_NewLeads_PieChart]
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
			SELECT DV.DropdownValue, COUNT(CLS.ContactID) TotalCount 
				FROM dbo.Contacts_Audit(NOLOCK) CA 
					INNER JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID
					INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID AND DV.AccountID = CA.AccountID
			WHERE CA.AccountID = @AccountID AND CA.IsDeleted = 0 
				--AND CA.ContactType = 1 
				AND CA.AuditAction = 'I'
				AND CONVERT(VARCHAR(10), CA.AuditDate, 120) BETWEEN @FromDate AND @ToDate
			GROUP BY DV.DropdownValue
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue, COUNT(CLS.ContactID) TotalCount  
				FROM dbo.Contacts_Audit(NOLOCK) CA
					INNER JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID
					INNER JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID AND DV.AccountID = CA.AccountID
			WHERE CA.AccountID = @AccountID AND CA.IsDeleted = 0 
			--	AND CA.ContactType = 1
				 AND CA.OwnerID = @OwnerID AND CA.AuditAction = 'I'
				AND CONVERT(VARCHAR(10), CA.AuditDate, 120) BETWEEN @FromDate AND @ToDate
			GROUP BY DV.DropdownValue
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
	  EXEC [dbo].[GET_Account_NewLeads_PieChart]
		 @AccountID			= 3395,
		 @FromDate          = '2014-11-01 00:00:00.000',       
		 @ToDate            = '2014-12-20 00:00:00.000',
		 @IsAdmin			= 0,
		 @OwnerID			= 5346

*/















GO


