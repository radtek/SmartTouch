
CREATE PROCEDURE [dbo].[Get_Account_BDX_FreemiumCustomLead_Report]
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
			SELECT	DISTINCT C.ContactID, (ISNULL(C.FirstName, '') + ' ' + ISNULL(C.LastName, '')) AS [FullName], CA.ContactType,
					CA.LastUpdatedOn AS CreatedOn, CE.Email, CE.ContactEmailID, CPN.PhoneNumber, CPN.ContactPhoneNumberID,
					(ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, '')) AS [Owner]
			FROM	dbo.Contacts(NOLOCK) C
					LEFT JOIN dbo.Users(NOLOCK) U ON U.AccountID IN (C.AccountID, 1) AND U.UserID = C.OwnerID
					INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAM ON LAM.IsDelete = 0 AND C.AccountID = LAM.AccountID
					INNER JOIN dbo.Contacts_Audit(NOLOCK) CA ON CA.AccountID = C.AccountID AND CA.ContactID = C.ContactID AND CA.AuditAction= 'I'
					LEFT JOIN dbo.ContactEmails(NOLOCK) CE ON C.ContactID = CE.ContactID AND CE.IsPrimary = 1 AND CE.AccountID = C.AccountID
					LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) CPN ON CPN.ContactID = C.ContactID AND cpn.IsPrimary = 1 AND CPN.AccountID=C.AccountID
			WHERE	C.AccountID = @AccountID
					AND C.IsDeleted = 0
					AND CA.ContactSource = 1
					AND CA.SourceType = 1
					AND LAM.LeadAdapterTypeID = 1
					AND CONVERT(VARCHAR(10),CA.LastUpdatedOn, 120) BETWEEN @FromDate AND @ToDate
		END
		ELSE IF (@IsAdmin = 0)
		BEGIN
			SELECT	DISTINCT C.ContactID, (ISNULL(C.FirstName, '') + ' ' + ISNULL(C.LastName, '')) AS [FullName], CA.ContactType, CA.LastUpdatedOn AS CreatedOn,
					CE.Email, CE.ContactEmailID, CPN.PhoneNumber, CPN.ContactPhoneNumberID, (ISNULL(U.FirstName, '') + ' ' + ISNULL(U.LastName, '')) AS [Owner]
			FROM	dbo.Contacts(NOLOCK) C 
					INNER JOIN dbo.Users(NOLOCK) U ON U.AccountID IN (C.AccountID, 1) AND U.UserID = @OwnerID
					INNER JOIN dbo.LeadAdapterAndAccountMap(NOLOCK) LAM ON  LAM.IsDelete = 0 AND C.AccountID = LAM.AccountID
					INNER JOIN dbo.Contacts_Audit(NOLOCK) CA ON CA.AccountID = C.AccountID AND CA.ContactID=C.ContactID AND CA.AuditAction='I'
					LEFT JOIN dbo.ContactEmails(NOLOCK) CE ON C.ContactID = CE.ContactID AND CE.IsPrimary = 1 AND CE.AccountID = C.AccountID
					LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) CPN ON CPN.ContactID = C.ContactID AND CPN.IsPrimary = 1 AND CPN.AccountID = C.AccountID
			WHERE	C.AccountID = @AccountID
					AND C.IsDeleted = 0
					AND CA.ContactSource = 1
					AND CA.SourceType = 1
					AND LAM.LeadAdapterTypeID = 1
					AND C.OwnerID = @OwnerID
					AND CONVERT(VARCHAR(10),CA.LastUpdatedOn, 120) BETWEEN @FromDate AND @ToDate
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
	  EXEC [dbo].[Get_Account_BDX_FreemiumCustomLead_Report]
		 @AccountID			= 45,
		 @FromDate          = '2014-11-01 00:00:00.000',       
		 @ToDate            = '2014-12-10 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100

*/






