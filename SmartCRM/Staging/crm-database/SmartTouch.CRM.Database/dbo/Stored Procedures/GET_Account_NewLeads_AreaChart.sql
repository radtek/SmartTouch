
CREATE PROC [dbo].[GET_Account_NewLeads_AreaChart](
	@AccountID		int,
	@FromDate       datetime,
	@ToDate         datetime,
	@IsAdmin		tinyint,
	@OwnerID		int,
	@PreviousValue	int OUTPUT
)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY

	DECLARE @AreaChart		TABLE (DateNo tinyint, Present int)
	DECLARE @DateRecords	TABLE (DateNo tinyint)
	DECLARE @PrevFromDate	datetime = DATEADD(day, -30, @FromDate),
			@PrevToDate		datetime = DATEADD(day, -30, @ToDate),
			@ParamList		VARCHAR(4000),
			@AllUsers		VARCHAR(MAX),
			@I				tinyint = 1
			
		;WITH TempCTE (DateNo) 
		AS (
		   SELECT 1
		   UNION ALL
		   SELECT DateNo + 1 FROM TempCTE WHERE DateNo < 30
			)
		INSERT INTO @DateRecords (DateNo)
		SELECT DateNo FROM TempCTE OPTION (MAXRECURSION 0);

	SET @ParamList = '@FromDate = ''' + CONVERT(VARCHAR(10), @FromDate, 110) + ''', @ToDate = ''' + CONVERT(VARCHAR(10), @ToDate, 110) + ''', @IsAdmin = ' + CAST(@IsAdmin AS VARCHAR)
	
	INSERT INTO dbo.StoreProcParamsList(AccountID, UserID, ReportName, ParamList)
	VALUES(@AccountID, @OwnerID, 'GET_Account_NewLeads_AreaChart', @ParamList)

	/* Account Users List */
	SELECT @AllUsers = COALESCE(@AllUsers+',','') + CAST(UserID AS VARCHAR)
		FROM dbo.Users(NOLOCK)
		WHERE (AccountID = @AccountID) OR AccountID = 1 AND [Status] = 1

		IF (1 = 1)--@IsAdmin = 1
		BEGIN
			/* Default 30 Days */
			INSERT INTO @AreaChart (DateNo, Present)
			SELECT	(DATEDIFF(DAY,@FromDate, CA.LastUpdatedOn) + 1) DateNo, COUNT(CA.ContactID) Present 
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			INNER JOIN dbo.Contacts(NOLOCK) c on ca.AccountID = c.AccountID AND c.ContactID = ca.ContactID
			LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
			LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID
			WHERE CA.AccountID = @AccountID
				AND C.AccountID = @AccountID
				AND C.IsDeleted = 0
				AND CA.IsDeleted = 0
				--AND CA.ContactType = 1
				AND (C.OwnerID IS NULL OR
						C.OwnerID IN (SELECT DataValue FROM dbo.Split(@AllUsers, ',')))
				AND CA.AuditAction = 'I'
				AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 110) BETWEEN @FromDate AND @ToDate
			GROUP BY DATEDIFF(DAY,@FromDate, CA.LastUpdatedOn)

			/* Default Previous 30 Days */
			SELECT	@PreviousValue = COUNT(CA.ContactID)
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			INNER JOIN dbo.Contacts(NOLOCK) c on ca.AccountID = c.AccountID AND c.ContactID = ca.ContactID
			LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
			LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID
			WHERE CA.AccountID = @AccountID
				AND C.AccountID = @AccountID
				AND c.IsDeleted = 0
				AND CA.IsDeleted = 0
					--AND CA.ContactType = 1
				AND (C.OwnerID IS NULL OR
						C.OwnerID IN (SELECT DataValue FROM dbo.Split(@AllUsers, ',')))
					AND CA.AuditAction = 'I'
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 110) BETWEEN @PrevFromDate AND @PrevToDate
		END
		ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			INSERT INTO @AreaChart (DateNo, Present)
			SELECT	(DATEDIFF(DAY,@FromDate, CA.LastUpdatedOn) + 1) DateNo, COUNT(CA.ContactID) Present 
			FROM	dbo.Contacts_Audit CA 
			INNER JOIN dbo.Contacts(NOLOCK) c on ca.AccountID = c.AccountID AND c.ContactID = ca.ContactID
			LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
			LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID
			WHERE CA.AccountID = @AccountID
				AND C.AccountID = @AccountID
				AND c.IsDeleted = 0
				AND CA.IsDeleted = 0
				--AND CA.ContactType = 1
				AND CA.AuditAction = 'I'
				--AND CA.ContactType = 1
				AND CA.OwnerID = @OwnerID
				AND CA.AuditAction = 'I'
				AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 110) BETWEEN @FromDate AND @ToDate
			GROUP BY DATEDIFF(DAY,@FromDate, CA.LastUpdatedOn)
			
			/* Default Previous 30 Days */
			SELECT	@PreviousValue = COUNT(CA.ContactID)
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			INNER JOIN dbo.Contacts(NOLOCK) c on ca.AccountID = c.AccountID AND c.ContactID = ca.ContactID
			LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
			LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID AND DV.AccountID = C.AccountID
			WHERE	CA.AccountID = @AccountID
					AND C.AccountID = @AccountID
					AND CA.IsDeleted = 0 --AND CA.ContactType = 1 
					AND CA.OwnerID = @OwnerID 
					AND CA.AuditAction = 'I'
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 110) BETWEEN @PrevFromDate AND @PrevToDate

		END

		SELECT	DR.DateNo, ISNULL(AC.Present, 0) Present 
		FROM @DateRecords DR
			LEFT JOIN @AreaChart AC ON DR.DateNo = AC.DateNo
		ORDER BY DR.DateNo

		RETURN @PreviousValue
	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF


END

/*
	  EXEC [dbo].[GET_Account_NewLeads_AreaChart]
		 @AccountID			=  45,
		 @FromDate          = '2014-12-18 00:00:00.000',       
		 @ToDate            = '2015-01-17 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100,
		 @PreviousValue		= 0

*/





