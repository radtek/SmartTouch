
CREATE PROC [dbo].[GET_Account_NewLeads_AreaChart_V1]
	(
		@AccountID		int,
		@FromDate       datetime,
		@ToDate         datetime,
		@Top5Only       bit,
		@IsAdmin		tinyint,
		@OwnerIDs		VARCHAR(MAX),
		@PreviousValue	int OUTPUT,
		@DateRange      varchar(1),
		@StartDatePrev  date,
		@EndDatePrev    date,
		@Debug bit = 0
	)
AS 
BEGIN
	SET NOCOUNT ON

	DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))
	DECLARE @Info ReportInfo --table(ID int, GridValue varchar(255), ContactID int, DateVal date, DateRange varchar(1), GroupDateRange int)

	DECLARE @ParamList	VARCHAR(4000),
			@ResultID	int

	----SET @ParamList = '@FromDate = ''' + CONVERT(VARCHAR(10), @FromDate, 110) + ''', @ToDate = ''' + CONVERT(VARCHAR(10), @ToDate, 110) + ''', @IsAdmin = ' + CAST(@IsAdmin AS VARCHAR)
	--INSERT INTO StoreProcParamsList_V1(AccountID, UserID, ReportName, ParamList)
	--VALUES(@AccountID, @OwnerIDs, 'GET_Account_NewLeads_AreaChart', @ParamList)

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_NewLeads_AreaChart_V1', @AccountID)

	SET @ResultID = scope_identity()

	BEGIN TRY

		IF ( 1=1)--@IsAdmin = 1
		BEGIN
			INSERT INTO @Info 

			--INSERT INTO @AreaChart (DateNo, Present, DateRange, GroupDateRange)
			SELECT C.OwnerID UserID, DV.DropdownValue,  C.ContactID, CONVERT(varchar(10), CA.LastUpdatedOn, 120) Date,'C',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @FromDate, CA.LastUpdatedOn) + 1 
					WHEN 'M' THEN (DATEDIFF(Month, @FromDate, CA.LastUpdatedOn)) + 1 
					ELSE (DATEPART(wk, CA.LastUpdatedOn) - DATEPART(wk, @FromDate)) + 1 
				END 
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			        INNER JOIN dbo.Contacts(NOLOCK) C ON CA.AccountID = C.AccountID AND CA.ContactID = C.ContactID 
                    LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
					LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID AND DV.AccountID = C.AccountID
			WHERE	(CA.AccountID = @AccountID)
					AND (C.AccountID = @AccountID)
					AND C.IsDeleted = 0
					AND CA.IsDeleted = 0
					AND CA.AuditAction = 'I'
					AND (C.OwnerID IS NULL OR 
						C.OwnerID IN (SELECT DataValue FROM dbo.Split(@OwnerIDs, ',')))
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 110) BETWEEN @FromDate AND @ToDate
		END
		ELSE IF (@IsAdmin = 0)
		BEGIN
		    INSERT INTO @Info
			--INSERT INTO @AreaChart (DateNo, Present, DateRange, GroupDateRange)
			SELECT C.OwnerID UserID, DV.DropdownValue,  C.ContactID, CONVERT(varchar(10), CA.LastUpdatedOn, 120) Date,'C',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @FromDate, CA.LastUpdatedOn) + 1 
					WHEN 'M' THEN (DATEDIFF(Month, @FromDate, CA.LastUpdatedOn)) + 1 
					ELSE (DATEPART(wk, CA.LastUpdatedOn) - DATEPART(wk, @FromDate)) + 1 
				END 
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			        INNER JOIN dbo.Contacts(NOLOCK) C ON CA.AccountID = C.AccountID AND CA.ContactID = C.ContactID
						--AND CA.OwnerID IN (SELECT DataValue FROM dbo.Split(@OwnerIDs, ','))
					LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
					LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID	AND DV.AccountID = C.AccountID
			WHERE	CA.AccountID = @AccountID
					AND C.IsDeleted = 0
					AND CA.IsDeleted = 0
					AND CA.AuditAction = 'I'
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 120) BETWEEN @FromDate AND @ToDate
		END

	--IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')
	--BEGIN
		IF ( 1=1)--@IsAdmin = 1
		BEGIN
		    INSERT INTO @Info
			--INSERT INTO @AreaChart (DateNo, Present, DateRange, GroupDateRange)
			SELECT null , null, C.ContactID, CONVERT(varchar(10), CA.LastUpdatedOn, 120) Date, 'P',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, CA.LastUpdatedOn) + 1 
					WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, CA.LastUpdatedOn)) + 1 
					ELSE (DATEPART(wk, CA.LastUpdatedOn) - DATEPART(wk, @StartDatePrev)) + 1 
				END 
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			        INNER JOIN dbo.Contacts(NOLOCK) C ON CA.AccountID = C.AccountID AND CA.ContactID = C.ContactID
                    LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
					LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID AND DV.AccountID = C.AccountID
			WHERE	(CA.AccountID = @AccountID)
					AND (C.AccountID = @AccountID)
					AND C.IsDeleted = 0
					AND CA.IsDeleted = 0
					AND CA.AuditAction = 'I'
					AND (C.OwnerID IS NULL OR 
						C.OwnerID IN (SELECT DataValue FROM dbo.Split(@OwnerIDs, ',')))
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev
			--GROUP BY CA.LastUpdatedOn 
		END
		ELSE
		BEGIN
		    INSERT INTO @Info
			--INSERT INTO @AreaChart (DateNo, Present, DateRange, GroupDateRange)
			SELECT C.OwnerID UserID, DV.DropdownValue, C.ContactID, CONVERT(varchar(10), CA.LastUpdatedOn, 120) Date,  'P',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, CA.LastUpdatedOn) + 1 
					WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, CA.LastUpdatedOn)) + 1 
					ELSE (DATEPART(wk, CA.LastUpdatedOn) - DATEPART(wk, @StartDatePrev)) + 1 
				END 
			FROM	dbo.Contacts_Audit(NOLOCK) CA 
			        INNER JOIN dbo.Contacts(NOLOCK) C ON CA.AccountID = C.AccountID AND CA.ContactID = C.ContactID
						--AND CA.OwnerID IN (SELECT DataValue FROM dbo.Split(@OwnerIDs, ','))
					LEFT JOIN dbo.ContactLeadSourceMap(NOLOCK) CLS ON CA.ContactID = CLS.ContactID AND CLS.IsPrimaryLeadSource = 1
					LEFT JOIN dbo.DropdownValues(NOLOCK) DV ON DV.DropdownValueID = CLS.LeadSouceID	AND DV.AccountID = C.AccountID
			WHERE	CA.AccountID = @AccountID
					AND C.IsDeleted = 0
					AND CA.IsDeleted = 0
					AND CA.AuditAction = 'I'
					AND CONVERT(VARCHAR(10), CA.LastUpdatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev
			--GROUP BY CA.LastUpdatedOn 
		END
	--END

	IF(@Debug = 1)
	BEGIN
		SELECT	*
		FROM	@Info
		WHERE	DateRange = 'P'
	END

	IF ISNULL(@DateRange, 'D') = 'M'
	BEGIN 
		;WITH DateTable
		AS
		(
			SELECT 1 [Number], @FromDate AS [DATE]
			UNION ALL
			SELECT ([Number] + 1) [Number], DATEADD(Month, 1, [DATE])
			FROM DateTable
			WHERE DATEADD(Month, 1, [DATE]) <= @ToDate
		)
		INSERT INTO @DataRange
		SELECT	[Number], CONVERT(VARCHAR(10), [DATE], 120) Label
		FROM	[DateTable]
		OPTION (maxrecursion 0)
	END
	ELSE IF ISNULL(@DateRange, 'D') = 'D'
	BEGIN 
		;WITH DateTable
		AS
		(
			SELECT 1 [Number], @FromDate AS [DATE]
			UNION ALL
			SELECT ([Number] + 1) [Number], DATEADD(day, 1, [DATE])
			FROM DateTable
			WHERE DATEADD(day, 1, [DATE]) <= @ToDate
		)
		INSERT INTO @DataRange
		SELECT	[Number], CONVERT(VARCHAR(10), [DATE], 120) Label
		FROM	[DateTable]
		OPTION (maxrecursion 0)
	END
	ELSE IF ISNULL(@DateRange, 'D') = 'W'
	BEGIN 
		;WITH DateTable
		AS
		(
			SELECT 1 AS [Number], DATEPART(wk, @FromDate) [WeekNum]
			UNION ALL
			SELECT ([Number] + 1) [Number], ([WeekNum] + 1) [WeekNum]
			FROM DateTable
			WHERE ([Number] + 1) <= (DATEPART(wk, @ToDate)-DATEPART(wk, @FromDate))
		)
		INSERT INTO @DataRange
		SELECT	[Number], 'WEEK-' + CAST([WeekNum] AS VARCHAR) Label
		FROM	[DateTable]
		OPTION (maxrecursion 0)
	END

	SELECT	RangeID ID, ISNULL([P], 0) P ,ISNULL([C], 0) C 
	FROM	(
				SELECT	RangeID, DateRange, GroupDateRange, count(RangeID) TotalCount
				FROM	@DataRange DR LEFT JOIN @Info AC on DR.RangeID = AC.GroupDateRange
				GROUP BY RangeID, DateRange, GroupDateRange
			) tmp
	PIVOT(
		MAX(TotalCount)
		FOR DateRange IN ([P], [C])
	) P
	ORDER BY RangeID
	
	IF(@Debug = 1)
	BEGIN
		SELECT	ISNULL(GridValue, '') GridValue, DateRange, count(ISNULL(GridValue, '')) TotalCount
		FROM	@Info
		GROUP BY ISNULL(GridValue, ''), DateRange
	END

	IF @Top5Only = 1
	BEGIN
		SELECT	top 5 GridValue, ISNULL([P], 0) P,ISNULL([C], 0) C
		FROM	(
					SELECT	ISNULL(GridValue, '') GridValue, DateRange, count(ISNULL(GridValue, '')) TotalCount
					FROM	@Info
					GROUP BY GridValue, DateRange
				) tmp
		PIVOT(
			MAX(TotalCount)
			FOR DateRange IN ([P], [C])
		) P
		ORDER BY [C] DESC
	END
	ELSE
	BEGIN
		SELECT	GridValue,ISNULL([P], 0) P, ISNULL([C], 0) C
		FROM	(
					SELECT	ISNULL(GridValue, '') GridValue, DateRange, count(ISNULL(GridValue, '')) TotalCount
					FROM	@Info
					GROUP BY GridValue, DateRange
				) tmp
		PIVOT(
			MAX(TotalCount)
			FOR DateRange IN ([P], [C])
		) P
	END

	--RETURN @PreviousValue
		
	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH

	
	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETUTCDATE(),
			TotalTime = (CAST(GETUTCDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID

	SET NOCOUNT OFF


END



/*

 EXEC [dbo].[GET_Account_NewLeads_AreaChart_V1]
	   @AccountID		= 45,
	   @FromDate        = '2014-10-20 00:00:00.000',  
	   @ToDate          = '2015-01-19 00:00:00.000',
	   @IsAdmin		    = 1,
	   @Top5Only        = 1,
	   @OwnerIDs	    = '',
	   @PreviousValue	= 0,
	   @DateRange       = 'D',
	   @StartDatePrev   = '2014-10-20 00:00:00.000',
	   @EndDatePrev     = '2015-01-20 00:00:00.000'

*/



GO


