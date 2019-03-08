
    CREATE PROCEDURE [dbo].[Get_DatabaseLifeCycleReport]
		@AccountId INT,
		@Users VARCHAR(MAX),
		@StartDate DATETIME,
		@EndDate DATETIME,
		@DateRange VARCHAR(1),
		@LifeCycleStages VARCHAR(MAX),
		@IsAdmin BIT
    AS
	BEGIN
		DECLARE @LifeCycleIds TABLE(LCID INT)
		DECLARE @UserIds TABLE(USERID INT)
		DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))
		DECLARE @ContactInfo TABLE (RangeId int, ContactCount int)	
		
		
		INSERT INTO @LifeCycleIds SELECT * FROM dbo.Split_2(@LifeCycleStages,',')
		
		INSERT INTO @UserIds SELECT * FROM dbo.Split_2(@Users,',')

		IF @IsAdmin = 1
			BEGIN
				INSERT INTO @UserIds SELECT NULL
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
		
		INSERT INTO @ContactInfo
		SELECT dbo.getDataRangeContactReport(@DateRange, C.LastUpdatedOn, @StartDate) as RangeId, Count(C.ContactID) as ContactCount
		FROM Contacts(NOLOCK) C
		INNER JOIN @UserIds U ON COALESCE(U.UserID,0) = COALESCE(C.OwnerID,0)
		INNER JOIN @LifeCycleIds LCI ON LCI.LCID = C.LifecycleStage
		WHERE C.IsDeleted = 0 AND C.AccountID = @AccountId AND C.LastUpdatedOn BETWEEN @StartDate AND @EndDate
		GROUP BY dbo.getDataRangeContactReport(@DateRange,C.LastUpdatedOn, @StartDate)

		SELECT DR.RangeId as ID, 0 AS P, ISNULL(d.ContactCount,0) AS C FROM @DataRange DR
		LEFT JOIN @ContactInfo d on ABS(DR.RangeID) = d.RangeId

		-- Top 5 Contact Life Cycles & Grid 
		SELECT LCI.LCID LifecycleStageId,Count(C.ContactID) ContactsCount FROM CONTACTS C (NOLOCK)
		INNER JOIN @UserIds U ON COALESCE(U.UserID,0) = COALESCE(C.OwnerID,0)  AND C.IsDeleted = 0 AND C.AccountID = @AccountId AND C.LastUpdatedOn BETWEEN @StartDate AND @EndDate
		RIGHT JOIN @LifeCycleIds LCI ON LCI.LCID = C.LifecycleStage
		group by LCI.LCID
	END

	/*

	exec [dbo].[Get_DatabaseLifeCycleReport]
    	@AccountId  = 4218,
		@Users = '5493,5497,5505,6518,6535,6574,6604,6608,6799,6800,6802,6803,6809,6822,6834,6842,6849,6868,6869,6876,6877,6878,6880,6882,6883,6885,6886,6887,6889,6890,6891,6899,6907,6908,6909,6910,6911,6912,6913,6914,6915,6916,6923,6927,7941,7942,7943,7949,7979,7980,7984,7985,7986,7987,7995,7996,7997,7998,7999,8014,8234,8285,8286,8287,8288,8289,8299,8316,8317,8320,8975,8976,8978,8980,8981,8982,8983',
		@StartDate = '4/24/2016 5:54:36 PM' ,
		@EndDate  = '5/24/2016 5:54:36 PM' ,
		@DateRange = 'D',
		@LifeCycleStages = '5912,5909,20988,20987,4665,4677,5911,5907,5906,16498,4664,5908,18571,4676,5905,5910,21898,21899,21900,21901,21902,21903,21904,21905'

	 */
	