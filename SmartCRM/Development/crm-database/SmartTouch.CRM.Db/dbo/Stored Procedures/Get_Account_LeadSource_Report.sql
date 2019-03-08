
CREATE PROCEDURE [dbo].[Get_Account_LeadSource_Report] 
	@AccountID INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@DateRange VARCHAR(1),
	@GroupType INT,
	@Entities VARCHAR(MAX)
AS
BEGIN

	DECLARE @Info dbo.ReportInfo
	DECLARE @ContactInfo TABLE (RangeId int, ContactCount int)
	DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))
	declare @EntitysTable table
	(
		ID INT
	)
	INSERT INTO @EntitysTable
	SELECT DataValue FROM dbo.Split_2(@Entities,',')

	DECLARE @ContactCommunityMap TABLE (ContactCommunityMapID INT IDENTITY(1,1), ContactID INT, CommunityID INT, RowNumber INT)

	INSERT INTO @ContactCommunityMap 
	SELECT ContactID, CommunityID, RowNumber
	FROM (SELECT CCM.ContactCommunityMapID, CCM.ContactID, CCM.CommunityID, ROW_NUMBER() OVER(PARTITION BY CCM.ContactID ORDER BY CCM.CreatedON ASC) RowNumber FROM Contacts C (NOLOCK)
	INNER JOIN ContactCommunityMap (NOLOCK) CCM ON CCM.ContactID = C.ContactID
	WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND CCM.IsDeleted = 0) x
	UNION
	SELECT tm.ContactID, t.CommunityID,1 FROM Tours t (NOLOCK)
	INNER JOIN ContactTourMap TM (NOLOCK) ON TM.TourID = t.TourID
	WHERE t.AccountID = @AccountID

	-- Top Contacts
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

	DECLARE @startWeek tinyint = DATEPART(wk,DATEADD(yy, DATEDIFF(yy,0,getdate()) + 1, -1)) -  DATEPART(wk, @StartDate)
	SET @DateRange = isnull(@DateRange, 'D')
	
	IF @GroupType = 1
	BEGIN
		INSERT INTO @ContactInfo
		SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) RangeId, count(1) as ContactCount
		FROM Contacts (NOLOCK) C 
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @EntitysTable split ON split.id = C.OwnerID
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
		AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	ELSE IF (@GroupType = 2)
	BEGIN
		INSERT INTO @ContactInfo
		SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) RangeId, count(1) as ContactCount
		FROM Contacts (NOLOCK) C 
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @EntitysTable split ON split.id = C.LifecycleStage
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
		AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	ELSE 
	BEGIN
		INSERT INTO @ContactInfo
		SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) RangeId, count(1) as ContactCount
		FROM Contacts (NOLOCK) C 
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
		INNER JOIN @EntitysTable split ON split.id = CCM.CommunityID
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
		AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	
	SELECT DR.RangeId as Id, 0 AS P, ISNULL(d.ContactCount,0) AS C FROM @DataRange DR
	LEFT JOIN @ContactInfo d on ABS(DR.RangeID) = d.RangeId
	
	-- Top 5 Lead Sources

	IF @GroupType = 1
	BEGIN
		SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(1) as C
		FROM ContactLeadSourceMap CLM (NOLOCK) 
		INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID
		INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID
		INNER JOIN @EntitysTable split ON split.id = C.OwnerID--( (@GroupType =1 AND split.id = C.OwnerID ) OR (@GroupType = 2 AND split.id = C.LifecycleStage )) 
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
				AND CLM.IsPrimaryLeadSource = 1
				AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY DDV.DropdownValue
		ORDER BY COUNT(1) DESC
	END
	ELSE IF (@GroupType = 2)
	BEGIN
		SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(1) as C
		FROM ContactLeadSourceMap CLM (NOLOCK) 
		INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID
		INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID
		INNER JOIN @EntitysTable split ON split.id = C.LifecycleStage --( (@GroupType =1 AND split.id = C.OwnerID ) OR (@GroupType = 2 AND split.id = C.LifecycleStage )) 
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
				AND CLM.IsPrimaryLeadSource = 1
				AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY DDV.DropdownValue
		ORDER BY COUNT(1) DESC
	END
	ELSE 
	BEGIN
		SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(1) as C
		FROM ContactLeadSourceMap CLM (NOLOCK) 
		INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID
		INNER JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID
		INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
		INNER JOIN @EntitysTable split ON split.id = CCM.CommunityID --( (@GroupType =1 AND split.id = C.OwnerID ) OR (@GroupType = 2 AND split.id = C.LifecycleStage )) 
		WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 
				AND CLM.IsPrimaryLeadSource = 1
				AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		GROUP BY DDV.DropdownValue
		ORDER BY COUNT(1) DESC
	END

	-- Grid
	DECLARE @EntitieValues VARCHAR (MAX)
	IF @GroupType = 1
	BEGIN
		SELECT @EntitieValues = COALESCE(@EntitieValues+',','') + '['+ CONVERT(varchar(10), ET.ID) +']' FROM Users(NOLOCK) u
		INNER JOIN @EntitysTable ET ON ET.ID = U.UserID
		WHERE u.IsDeleted = 0 AND (u.AccountID = @AccountID or u.AccountID = 1)

		INSERT INTO @Info (ID,GridValue,ContactID)
		SELECT ddv.DropdownValueID, CONVERT(varchar(10), ET.ID) ,count(clm.ContactID) FROM Contacts c (nolock)
		INNER JOIN ContactLeadSourceMap CLM (nolock) on clm.ContactID = c.ContactID and c.IsDeleted = 0 and c.AccountID = @AccountID and CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		AND clm.IsPrimaryLeadSource = 1 
		INNER JOIN @EntitysTable ET ON ET.ID = c.OwnerID
		RIGHT JOIN DropdownValues ddv(nolock) on ddv.DropdownValueID = clm.LeadSouceID WHERE ddv.IsDeleted = 0 and ddv.AccountID = @AccountID and ddv.DropdownID = 5 and ddv.IsActive = 1
		GROUP BY ddv.DropdownValueID , CONVERT(varchar(10), ET.ID)
	END
	ELSE IF (@GroupType = 2)
	BEGIN
		SELECT @EntitieValues =  COALESCE(@EntitieValues+',','') + '['+ CONVERT(varchar(10), D.DropdownValueID) +']' FROM DropdownValues(NOLOCK) D
		INNER JOIN @EntitysTable ET ON ET.ID = D.DropdownValueID
		WHERE D.AccountID = @AccountID AND D.IsDeleted = 0 AND D.IsActive = 1 AND D.DropdownID = 3

		INSERT INTO @Info (ID,GridValue,ContactID)
		SELECT  ddv.DropdownValueID, CONVERT(varchar(10), ET.id), COUNT(CLM.ContactID) 
		FROM Contacts (NOLOCK) C
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0  AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
		AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @EntitysTable ET ON ET.ID = c.LifecycleStage
		RIGHT JOIN DropdownValues ddv(nolock) on ddv.DropdownValueID = clm.LeadSouceID WHERE ddv.IsDeleted = 0 and ddv.AccountID = @AccountID and ddv.DropdownID = 5 and ddv.IsActive = 1
		GROUP BY ddv.DropdownValueID, ET.id
	END
	ELSE
	BEGIN
		SELECT @EntitieValues =  COALESCE(@EntitieValues+',','') + '['+ CONVERT(varchar(10), D.DropdownValueID) +']' FROM DropdownValues(NOLOCK) D
		INNER JOIN @EntitysTable ET ON ET.ID = D.DropdownValueID
		WHERE D.AccountID = @AccountID AND D.IsDeleted = 0 AND D.IsActive = 1 AND D.DropdownID = 7

		INSERT INTO @Info (ID,GridValue,ContactID)
		SELECT  ddv.DropdownValueID, CONVERT(varchar(10), ET.id), COUNT(CLM.ContactID) 
		FROM Contacts (NOLOCK) C
		INNER JOIN ContactLeadSourceMap CLM (NOLOCK) ON CLM.ContactID = C.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0  AND CLM.LastUpdatedDate 
		BETWEEN @StartDate AND @EndDate AND CLM.IsPrimaryLeadSource = 1
		INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = C.ContactID
		INNER JOIN @EntitysTable ET ON ET.ID = CCM.CommunityID
		RIGHT JOIN DropdownValues ddv(nolock) on ddv.DropdownValueID = clm.LeadSouceID WHERE ddv.IsDeleted = 0 and ddv.AccountID = @AccountID and ddv.DropdownID = 5 and ddv.IsActive = 1
		GROUP BY ddv.DropdownValueID, ET.id
	END
	
	 DECLARE @SQL NVARCHAR(MAX)
	 SELECT @SQL = N' 
	 SELECT Id, ' +  @EntitieValues  + ' FROM 
	 ( SELECT Id, GridValue,ContactID FROM @Info ) AS Info
	 PIVOT
	 (
		MAX(ContactID) FOR GridValue IN (' +@EntitieValues + ')
	 ) AS PivotTable'

	 PRINT @SQL

	 EXEC sp_executesql @SQL, N'@Info dbo.ReportInfo READONLY',  @Info
END
GO


