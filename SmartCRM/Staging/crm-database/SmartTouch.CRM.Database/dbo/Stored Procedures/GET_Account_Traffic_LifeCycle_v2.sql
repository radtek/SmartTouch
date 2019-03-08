

CREATE PROCEDURE [dbo].[GET_Account_Traffic_LifeCycle_v2](
	@AccountID int,
	@Type tinyint,
	@Top5Only bit,
	@DateRange varchar(1),
	@StartDate date,
	@EndDate date,
	@SelectedList nvarchar(MAX),
	@TrafficLifeCycle nvarchar(4000),
	@StartDatePrev date = '1900-01-01',
	@EndDatePrev date = '1900-01-01'
)
AS
BEGIN
	DECLARE @Info ReportInfo --table(ID int, GridValue varchar(255), ContactID int, DateVal date, DateRange varchar(1), GroupDateRange int)
	DECLARE @GroupList ReportGroupList --TABLE (ID int, Name varchar(255))
	DECLARE @OptionList TABLE (ID int, Name varchar(255))
	DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))

	DECLARE @ResultID int

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_Traffic_LifeCycle_v2', @AccountID)

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
		FROM	DropdownValues dv  (NOLOCK) 
		        INNER JOIN Tours t (NOLOCK) ON t.CommunityID = dv.DropdownValueID
				INNER JOIN ContactTourMap ctm (NOLOCK) ON ctm.TourID = t.TourID
				INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TourID
				INNER JOIN users u  (NOLOCK) on u.accountid IN (dv.accountid, 1) and u.UserID = UTM.UserID
				INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
				INNER JOIN DBO.DropdownValues DV2  (NOLOCK) ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
			--	INNER JOIN @GroupList gl on gl.ID = u.UserID
		WHERE	dv.accountid = @AccountID
				AND dv.DropdownID = 7
				AND dv2.DropdownID = 3
				AND dv.IsActive = 1
				AND c.IsDeleted = 0
				AND u.UserID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
				AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
			--	AND CONVERT(VARCHAR(10), t.CreatedOn, 120) BETWEEN @StartDate AND @EndDate
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
		FROM	DropdownValues dv  (NOLOCK) 
				INNER JOIN Tours t  (NOLOCK) ON t.CommunityID = dv.DropdownValueID
				INNER JOIN ContactTourMap ctm  (NOLOCK) ON ctm.TourID = t.TourID
				INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TourID
				INNER JOIN users u (NOLOCK) ON u.accountid =dv.AccountID and u.UserID = UTM.UserID
				INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
				INNER JOIN DBO.DropdownValues DV2  (NOLOCK) ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
				INNER JOIN @GroupList gl on gl.ID = dv.DropdownValueID
		WHERE	dv.accountid = @AccountID
				AND dv.DropdownID = 7
				AND dv2.DropdownID = 3
				AND dv.IsActive = 1
				AND c.IsDeleted = 0
				--AND dv.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
				AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
				--AND CONVERT(VARCHAR(10), t.CreatedOn, 120) BETWEEN @StartDate AND @EndDate
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
			FROM	DropdownValues dv  (NOLOCK) 
					INNER JOIN Tours t  (NOLOCK) ON t.CommunityID = dv.DropdownValueID
					INNER JOIN ContactTourMap ctm  (NOLOCK) ON ctm.TourID = t.TourID
					INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TourID
					INNER JOIN users u  (NOLOCK) on u.accountid =dv.AccountID and u.UserID = UTM.UserID
					INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					INNER JOIN DBO.DropdownValues DV2  (NOLOCK) ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
					--INNER JOIN @GroupList gl on gl.ID = u.UserID
			WHERE	dv.accountid = @AccountID
					AND dv.DropdownID = 7
					AND dv2.DropdownID = 3
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND u.UserID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
					AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
				--	AND CONVERT(VARCHAR(10), t.CreatedOn, 120) BETWEEN @StartDatePrev AND @StartDatePrev
				AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120) 
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
			FROM	DropdownValues dv  (NOLOCK) 
					INNER JOIN Tours t  (NOLOCK) ON t.CommunityID = dv.DropdownValueID
					INNER JOIN ContactTourMap ctm  (NOLOCK) ON ctm.TourID = t.TourID
					INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TourID
					INNER JOIN users u  (NOLOCK) on u.accountid =dv.AccountID and u.UserID = UTM.UserID
					INNER JOIN contacts c  (NOLOCK) on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					INNER JOIN DBO.DropdownValues DV2  (NOLOCK) ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
					--INNER JOIN @GroupList gl on gl.ID = dv.DropdownValueID
			WHERE	dv.accountid = @AccountID
					AND dv.DropdownID = 7
					AND dv2.DropdownID = 3
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND dv.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@SelectedList, ','))
					AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
					--AND CONVERT(VARCHAR(10), t.TourDate, 120) BETWEEN @StartDatePrev AND @StartDatePrev
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120) 
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
			SELECT 1 AS [Number], DATEPART(wk, @StartDate) [WeekNum]
			UNION ALL
			SELECT ([Number] + 1) [Number], ([WeekNum] + 1) [WeekNum]
			FROM DateTable
			WHERE ([Number] + 1) <= (DATEPART(wk, @EndDate)-DATEPART(wk, @StartDate))
		)
		INSERT INTO @DataRange
		SELECT	[Number], 'WEEK-' + CAST([WeekNum] AS VARCHAR) Label
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
		FROM	dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle) gi inner join dbo.Split('(c),(p)', ',') i ON 1=1

		SELECT	@LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + ' (c)], 0) ',
				@LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + ' (p)], 0) '
		FROM	dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)
	END
	ELSE
	BEGIN
		SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + Name + ']',
				@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + Name + '], 0) [' + Name + ']'
		FROM	dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)
		SELECT	@LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + '], 0) ',
				@LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + '], 0) '
		FROM	dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)
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
			'
	END
	ELSE
	BEGIN
		SET @SQL = N'
				SELECT	gl.id, gl.Name,
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

/*
	select * from dropdowns(nolock)
	select userid from users(nolock) where accountid in (1,4218) and status=1
	select DropdownValueID from dropdownvalues(nolock) where accountid=4218 and isdeleted=0 and dropdownid=3
	exec [dbo].[GET_Account_Traffic_LifeCycle_v2] 4218,0,1,'D','10/1/2017 1:08:30 PM','10/31/2017 1:08:30 PM','5493,5497,5505,6518,6535,6574,6604,6608,6799,6800,6802,6803,6809,6822,6834,6842,6849,6889,6910,7979,8234,8285,8286,8287,8288,8289,8299,8316,8317,8320,8996,6868,6869,6877,6878,6880,6882,6883,6885,6886,6887,6890,6891,6895,6899,6907,6911,6913,6914,6915,6923,6927,7941,7942,7943,7949,7980,7982,7984,7985,7986,7987,7995,7996,7997,7998,7999,8014,8975,8980,8989,8990,9000,9005,9007,9009,9010,9011,9012','4663,4664,4665,4676,4677,5905,5906,5907,5908,5909,5910,5911,5912,5913,6169,16498,18571,20986,20987,20988,21898,21899,21900,21901,21902,21903,21904,21905,22988,22989,22990,22991','1/1/1900 12:00:00 AM','1/1/1900 12:00:00 AM'
*/