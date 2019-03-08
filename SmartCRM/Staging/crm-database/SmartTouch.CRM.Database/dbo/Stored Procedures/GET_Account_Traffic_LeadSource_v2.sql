

CREATE PROCEDURE [dbo].[GET_Account_Traffic_LeadSource_v2](
	@AccountID int,
	@Type tinyint,
	@Top5Only bit,
	@DateRange varchar(1),
	@StartDate date ,
	@EndDate date ,
	@SelectedList nvarchar(MAX),
	@TrafficSource nvarchar(4000),
	@StartDatePrev date = '1900-01-01',
	@EndDatePrev date = '1900-01-01',
	@Debug bit = 0
)
AS
BEGIN
	DECLARE @Info ReportInfo --table(ID int, GridValue varchar(255), ContactID int, DateVal date, DateRange varchar(1), GroupDateRange int)
	DECLARE @GroupList ReportGroupList --TABLE (ID int, Name varchar(255))
	DECLARE @OptionList TABLE (ID int, Name varchar(255))
	DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))

	DECLARE @ResultID int

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_Traffic_LeadSource_v2', @AccountID)

	SET @ResultID = scope_identity()

	IF(@Type = 0)
	BEGIN
		INSERT INTO @GroupList
		SELECT	*
		FROM	GetInfo(@AccountID, 'user', @SelectedList)
	END
	ELSE
	BEGIN
		INSERT INTO @GroupList
		SELECT	*
		FROM	GetInfo(@AccountID, 'community', @SelectedList)
	END

	IF isnull(@Type, 0) = 0
	BEGIN
		INSERT INTO	@Info
		SELECT	u.userID ID, DV2.DropdownValue GridValue, c.ContactID, CONVERT(varchar(10), t.TourDate, 120) TourDate, 'c',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @StartDate, t.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)
					WHEN 'M' THEN (DATEDIFF(Month, @StartDate, t.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)
					ELSE (DATEPART(wk, t.TourDate) - DATEPART(wk, @StartDate)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)
				END 
		FROM	dbo.DropdownValues(NOLOCK) dv 
					INNER JOIN dbo.Tours t (NOLOCK) ON t.CommunityID = dv.DropdownValueID
					INNER JOIN dbo.ContactTourMap ctm (NOLOCK) ON ctm.TourID = t.TourID
					INNER JOIN dbo.UserTourMap UTM  (NOLOCK) ON UTM.TourID = t.TourID
				--	INNER JOIN dbo.Users u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
				--	INNER JOIN dbo.Users u on u.UserID = t.CreatedBy and u.Accountid=dv.AccountID 
				    INNER JOIN dbo.Users u  (NOLOCK) on (u.accountid = dv.accountid or u.AccountID = 1)  and u.UserID = UTM.UserID and u.Status=1
					INNER JOIN dbo.Contacts c (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID 
					INNER JOIN dbo.DropdownValues DV2  (NOLOCK) ON (t.AccountID = dv2.AccountID)
					--INNER JOIN dbo.DropdownValues DV2 ON dv2.AccountID=t.AccountID
					INNER JOIN dbo.ContactLeadSourceMap csm  (NOLOCK) on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID And csm.IsPrimaryLeadSource=1
				  --INNER JOIN @GroupList gl on gl.ID = u.UserID
			WHERE	dv.accountid =  @AccountID
					AND dv.DropdownID = 7
					AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0 and c.IncludeInReports=1
					AND u.UserID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
					AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficSource, ','))
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120) 
	END
	ELSE
	BEGIN
		INSERT INTO	@Info
		SELECT	CAST(dv.DropdownValueID AS int) ID, DV2.DropdownValue GridValue, c.ContactID, CONVERT(varchar(10), t.TourDate, 120) TourDate, 'c',
				CASE ISNULL(@DateRange, 'D')
					WHEN 'D' THEN  DATEDIFF(dd, @StartDate, t.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)
					WHEN 'M' THEN (DATEDIFF(Month, @StartDate, t.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)
					ELSE (DATEPART(wk, t.TourDate) - DATEPART(wk, @StartDate)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)
				END
		FROM	DropdownValues dv  (NOLOCK)  INNER JOIN Tours t ON t.CommunityID = dv.DropdownValueID
				INNER JOIN ContactTourMap ctm (NOLOCK)  ON ctm.TourID = t.TourID
				INNER JOIN dbo.UserTourMap UTM  (NOLOCK) ON UTM.TourID = t.TourID
				INNER JOIN USERS u on u.accountid IN (dv.accountid, 1) and u.UserID = UTM.UserID
				--INNER JOIN USERS u on u.accountid =dv.AccountID 
				--INNER JOIN dbo.Users u  (NOLOCK) on (u.accountid = dv.accountid or u.AccountID = 1)  and u.UserID = t.CreatedBy and u.Status=1
				INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID 
				INNER JOIN DBO.DropdownValues dv2  (NOLOCK) ON (t.AccountID = dv2.AccountID)
			--INNER JOIN DBO.DropdownValues dv2 ON dv2.AccountID=dv.AccountID
				INNER JOIN ContactLeadSourceMap(NOLOCK) csm on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID And csm.IsPrimaryLeadSource=1
				INNER JOIN @GroupList gl on gl.ID = dv.DropdownValueID
		WHERE	dv.accountid = @AccountID
				AND dv.DropdownID = 7
				AND dv2.DropdownID = 5
				AND dv.IsActive = 1
				AND c.IsDeleted = 0 and c.IncludeInReports=1
				AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficSource, ','))
				AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120) 
				AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120) 
	END

	IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')
	BEGIN
		IF isnull(@Type, 0) = 0
		BEGIN
			INSERT INTO	@Info
			SELECT	u.userID ID, DV2.DropdownValue GridValue, c.ContactID, CONVERT(varchar(10), t.TourDate, 120) TourDate, 'p',
					CASE ISNULL(@DateRange, 'D')
						WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, t.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)
						WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, t.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)
						ELSE (DATEPART(wk, t.TourDate) - DATEPART(wk, @StartDatePrev)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)
					END 
			FROM	DropdownValues dv  (NOLOCK) INNER JOIN Tours t  (NOLOCK) ON t.CommunityID = dv.DropdownValueID
					INNER JOIN ContactTourMap ctm  (NOLOCK) ON ctm.TourID = t.TourID
					INNER JOIN dbo.UserTourMap UTM  (NOLOCK) ON UTM.TourID = t.TourID
				--	INNER JOIN users u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
				-- INNER JOIN users u on u.accountid =dv.AccountID and u.UserID = t.CreatedBy
				    INNER JOIN dbo.Users u  (NOLOCK) on (u.accountid = dv.accountid or u.AccountID = 1)  and u.UserID = UTM.UserID and u.Status=1
					INNER JOIN contacts c (NOLOCK)  on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					INNER JOIN DBO.DropdownValues DV2  (NOLOCK) ON (t.AccountID = DV2.AccountID)
					--INNER JOIN DBO.DropdownValues DV2 ON dv2.AccountID=dv.AccountID
					INNER JOIN ContactLeadSourceMap csm  (NOLOCK) on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID And csm.IsPrimaryLeadSource=1
					--AND IsPrimaryLeadSource = 0
					
					--INNER JOIN @GroupList gl on gl.ID = u.UserID
			WHERE	dv.accountid =  @AccountID
					AND dv.DropdownID = 7
					AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0 and c.IncludeInReports=1
					AND u.UserID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
					AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficSource, ','))
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDatePrev, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDatePrev, 120) 
					--AND CONVERT(VARCHAR(10), t.CreatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev
		END
		ELSE
		BEGIN
			INSERT INTO	@Info
			SELECT	CAST(dv.DropdownValueID AS int) ID, DV2.DropdownValue GridValue, c.ContactID, CONVERT(varchar(10), t.TourDate, 120) TourDate, 'p',
					CASE ISNULL(@DateRange, 'D')
						WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, t.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)
						WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, t.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)
						ELSE (DATEPART(wk, t.TourDate) - DATEPART(wk, @StartDatePrev)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)
					END
			FROM	DropdownValues dv  (NOLOCK) INNER JOIN Tours t (NOLOCK)  ON t.CommunityID = dv.DropdownValueID
					INNER JOIN ContactTourMap ctm  (NOLOCK) ON ctm.TourID = t.TourID
					INNER JOIN dbo.UserTourMap UTM  (NOLOCK) ON UTM.TourID = t.TourID
					INNER JOIN users u on u.accountid =dv.AccountID and u.UserID = UTM.UserID
					--INNER JOIN dbo.Users u on (u.accountid = dv.accountid or u.AccountID = 1)  and u.UserID = t.CreatedBy and u.Status=1
					INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					INNER JOIN DBO.DropdownValues DV2 (NOLOCK)  ON (t.AccountID = DV2.AccountID)
					--INNER JOIN DBO.DropdownValues DV2 ON dv2.AccountID=dv.AccountID
					INNER JOIN ContactLeadSourceMap csm  (NOLOCK) on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID AND IsPrimaryLeadSource = 1
					INNER JOIN @GroupList gl on gl.ID = dv.DropdownValueID
			WHERE	dv.accountid = @AccountID
					AND dv.DropdownID = 7
					AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0 and c.IncludeInReports=1
					AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficSource, ','))
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDatePrev, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDatePrev, 120)
					--AND CONVERT(VARCHAR(10), t.CreatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev
		END
	END

	IF ISNULL(@DateRange, 'D') = 'M'
	BEGIN 
		;WITH DateTable
		AS
		(
			SELECT 1 [Number], @StartDate AS [DATE]
			UNION ALL
			SELECT ([Number] + 1) [Number], DATEADD(Month, 1, [DATE])
			FROM DateTable
			WHERE DATEADD(Month, 1, [DATE]) <= @EndDate
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
			SELECT 1 [Number], @StartDate AS [DATE]
			UNION ALL
			SELECT ([Number] + 1) [Number], DATEADD(day, 1, [DATE])
			FROM DateTable
			WHERE DATEADD(day, 1, [DATE]) <= @EndDate
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
			SELECT 1 AS [Number], DATEPART(WEEK, @StartDate) [WeekNum]
			UNION ALL
			SELECT ([Number] + 1) [Number], ([WeekNum] + 1) [WeekNum]
			FROM DateTable
			WHERE ([Number] + 1) <= DATEDIFF(WEEK, @StartDate, @EndDate) 
		)
		INSERT INTO @DataRange
		SELECT	[Number], 'WEEK-' + CAST((CASE [WeekNum]%52 WHEN 0 THEN 52 ELSE [WeekNum]%52 END) AS VARCHAR) Label
		FROM	[DateTable]
		OPTION (maxrecursion 0)
	END
	
	SELECT	RangeID ID, ISNULL([P], 0) [P], ISNULL([C], 0) [C]
	FROM	(
				SELECT	RangeID, DateRange, GroupDateRange, count(RangeID) TotalCount
				FROM	@DataRange dr left join @info i on dr.RangeID = i.GroupDateRange
				GROUP BY RangeID, DateRange, GroupDateRange
			) tmp
	PIVOT(
		MAX(TotalCount)
		FOR DateRange IN ([P], [C])
	) P
	ORDER BY RangeID

	IF @Top5Only = 1
	BEGIN
		SELECT	top 5 GridValue, ISNULL([P], 0) [P], ISNULL([C], 0) [C]
		FROM	(
					SELECT	GridValue, DateRange, count(GridValue) TotalCount
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
		SELECT	GridValue, ISNULL([P], 0) [P], ISNULL([C], 0) [C]
		FROM	(
					SELECT	GridValue, DateRange, count(GridValue) TotalCount
					FROM	@Info
					GROUP BY GridValue, DateRange
				) tmp
		PIVOT(
			MAX(TotalCount)
			FOR DateRange IN ([P], [C])
		) P
	END

	DECLARE @LeadSourceList NVARCHAR(MAX),
			@LeadSourceListC NVARCHAR(MAX),
			@LeadSourceListP NVARCHAR(MAX),
			@LeadSourceListNull NVARCHAR(MAX),
			@ColumnList NVARCHAR(MAX)

	IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')
	BEGIN
		SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + Name + ' ' + DataValue + ']',
				@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + Name + ' ' + DataValue + '], 0) [' + Name + ' ' + DataValue + ']'
		FROM	dbo.GetInfo(@AccountID, 'LeadSource', @TrafficSource) gi inner join dbo.Split('(c),(p)', ',') i ON 1=1

		SELECT	@LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + ' (c)], 0) ',
				@LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + ' (p)], 0) '
		FROM	dbo.GetInfo(@AccountID, 'LeadSource', @TrafficSource)
	END
	ELSE
	BEGIN
		SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + Name + ']',
				@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + Name + '], 0) [' + Name + ']'
		FROM	dbo.GetInfo(@AccountID, 'LeadSource', @TrafficSource)
		SELECT	@LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + '], 0) ',
				@LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + '], 0) '
		FROM	dbo.GetInfo(@AccountID, 'LeadSource', @TrafficSource)
	END

	PRINT @LeadSourceList
	PRINT @LeadSourceListNull
	PRINT @LeadSourceListC
	PRINT @LeadSourceListP

	DECLARE @SQL NVARCHAR(MAX)
	
	IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')
	BEGIN
		SET @SQL = N'
				SELECT	gl.id, gl.Name,
						ISNULL([Total (c)], 0) [Total (c)],
						ISNULL([Total (p)], 0) [Total (p)],
						' + @LeadSourceListNull + '
				FROM	@GroupList gl left join (
							SELECT	id,
									' + @LeadSourceList + ',
									' + @LeadSourceListC + ' [Total (c)],
									' + @LeadSourceListP + ' [Total (p)]
							FROM	(
										SELECT	id, GridValue + '' ('' + DateRange + '')'' ColumnName, count(*) Total
										FROM	@Info
										GROUP BY id, GridValue + '' ('' + DateRange + '')''
									) tmp
							pivot(
								MAX(Total)
								FOR ColumnName IN (' + @LeadSourceList + ')
							)p
					) tmp on gl.ID = tmp.id
					ORDER BY [Total (c)], [Total (p)] DESC
			'
	END
	ELSE
	BEGIN
		SET @SQL = N'SELECT	gl.id, gl.Name,
						ISNULL([Total], 0) [Total],
						' + @LeadSourceListNull + '
				FROM	@GroupList gl left join (
							SELECT	id, ' + @LeadSourceList + ',
									' + @LeadSourceListC + ' [Total]
							FROM	(
										SELECT	id, GridValue ColumnName, count(*) Total
										FROM	@Info
										GROUP BY id, GridValue
									) tmp
							pivot(
								MAX(Total)
								FOR ColumnName IN (' + @LeadSourceList + ')
							)p
					) tmp on gl.ID = tmp.id
					ORDER BY [Total] DESC
			'
	END

	PRINT @SQL

	EXEC sp_executesql @SQL, N'@GroupList ReportGroupList READONLY, @Info ReportInfo READONLY', @GroupList, @Info

	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID

END