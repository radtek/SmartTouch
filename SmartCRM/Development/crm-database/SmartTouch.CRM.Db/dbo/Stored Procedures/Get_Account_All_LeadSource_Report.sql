
CREATE PROCEDURE [dbo].[Get_Account_All_LeadSource_Report]  
	@AccountID INT,
	@StartDate DATETIME,
	@EndDate DATETIME,
	@DateRange VARCHAR(1),
	@FilterType INT,
	@Entities VARCHAR(MAX)
AS
BEGIN
	DECLARE @ContactInfo TABLE (RangeId int, ContactCount int)	
	DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))
	DECLARE @EntitysTable TABLE
	(
		ID INT
	)
	INSERT INTO @EntitysTable
	SELECT DataValue FROM dbo.Split_2(@Entities,',')
	DECLARE @LeadSourceIDs TABLE
	(
		ID INT, Value VARCHAR(100)
	)
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

	SET @DateRange = isnull(@DateRange, 'D')
	IF (@FilterType = 1)	-- LeadSources
	BEGIN
			INSERT INTO @ContactInfo
			SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) as RangeId, count(1) as ContactCount
			FROM Contacts(NOLOCK) C
			INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.ContactID = C.ContactID
			INNER JOIN @EntitysTable ET ON ET.ID = CLM.LeadSouceID
			WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	ELSE IF (@FilterType = 2)	--LifeCycle
	BEGIN
			INSERT INTO @ContactInfo
			SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) as RangeId, count(1) as ContactCount
			FROM Contacts(NOLOCK) C
			INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.ContactID = C.ContactID
			INNER JOIN @EntitysTable ET ON ET.ID = C.LifecycleStage
			WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	ELSE IF (@FilterType = 3)	--Communities
	BEGIN
			INSERT INTO @ContactInfo
			SELECT dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate) as RangeId, count(1) as ContactCount
			FROM Contacts(NOLOCK) C
			INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.ContactID = C.ContactID
			INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = CLM.ContactID
			INNER JOIN @EntitysTable ET ON ET.ID = CCM.CommunityID
			WHERE C.AccountID = @AccountID AND C.IsDeleted = 0 AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			GROUP BY dbo.getDataRangeContactReport(@DateRange, CLM.LastUpdatedDate, @StartDate)
	END
	
	SELECT DR.RangeId as ID, 0 AS P, ISNULL(d.ContactCount,0) AS C FROM @DataRange DR
	LEFT JOIN @ContactInfo d on ABS(DR.RangeID) = d.RangeId

	-- TOP 5 LeadSources

	IF (@FilterType = 1)
	BEGIN
			SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(1) as C
			FROM ContactLeadSourceMap CLM (NOLOCK) 
			LEFT JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0 
			LEFT JOIN DropDownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID
			INNER JOIN @EntitysTable ET ON ET.ID = CLM.LeadSouceID
			AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			--WHERE   
			GROUP BY DDV.DropdownValue, DDV.SortID
			ORDER BY COUNT(1) DESC, DDV.SortID ASC
	END
	ELSE IF (@FilterType = 2)
	BEGIN
		    SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(CLM.ContactID) as C 
			FROM ContactLeadSourceMap CLM (NOLOCK)
			LEFT JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
			LEFT JOIN DropdownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID AND DDV.AccountID = @AccountID AND DDV.IsDeleted = 0
			INNER JOIN @EntitysTable ET ON ET.ID = C.LifecycleStage
			AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			GROUP BY DDV.DropdownValue, DDV.SortID
			ORDER BY COUNT(1) DESC, DDV.SortID ASC
	END
	ELSE IF (@FilterType = 3)
	BEGIN
			SELECT TOP 5 DDV.DropdownValue as GridValue,  0 as P, COUNT(CLM.ContactID) as C 
			FROM ContactLeadSourceMap CLM (NOLOCK)
			INNER JOIN Contacts C (NOLOCK) ON C.ContactID = CLM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
			INNER JOIN DropdownValues DDV (NOLOCK) ON DDV.DropdownValueID = CLM.LeadSouceID
			INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = CLM.ContactID
			INNER JOIN @EntitysTable ET ON ET.ID = CCM.CommunityID
			AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
			GROUP BY DDV.DropdownValue, DDV.SortID
			ORDER BY COUNT(1) DESC, DDV.SortID ASC
	END

	-- Grid
	IF (@FilterType = 1)	-- LeadSources
	BEGIN
			;WITH TotalLeadSources
			AS
			(
				SELECT ET.ID AS LeadSourceID, dv.dropdownvalue, COUNT(CLM.LeadSouceID) AS TotalLSCount
				FROM @EntitysTable ET 
				LEFT JOIN DropdownValues(NOLOCK) dv ON dv.DropdownValueID = ET.ID AND dv.IsDeleted = 0
				LEFT JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = ET.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				LEFT JOIN Contacts(NOLOCK) C on C.ContactID = CLM.ContactID AND dv.AccountID = C.AccountID AND C.IsDeleted = 0 AND C.AccountID = @AccountID 
				GROUP BY ET.ID, dv.dropdownvalue
			)
			, PrimaryLeadSources
			AS
			( 
				SELECT ET.ID AS LeadSourceID, dv.dropdownvalue, COUNT(CLM.LeadSouceID) AS PrimaryLSCount FROM @EntitysTable ET 
				LEFT JOIN DropdownValues(NOLOCK) dv ON dv.DropdownValueID = ET.ID AND dv.IsDeleted = 0 AND dv.AccountID = @accountID
				LEFT JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = ET.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				LEFT JOIN Contacts(NOLOCK) C on C.ContactID = clm.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
				WHERE CLM.IsPrimaryLeadSource = 1
				GROUP BY ET.ID, dv.dropdownvalue
			)

			SELECT ISNULL(TotalLS.LeadSourceID, PrimaryLS.LeadSourceID) AS LeadSourceID, 
				   ISNULL(TotalLS.dropdownvalue, PrimaryLS.dropdownvalue) AS Value,
				   ISNULL(PrimaryLS.PrimaryLSCount, 0) AS PrimaryLSCount,
				   CAST(ROUND(ISNULL((PrimaryLS.PrimaryLSCount * 100.0 /NULLIF(PT, 0)), 0), 2) AS float) AS PrimaryLSPercent, 
				   ISNULL(TotalLS.TotalLSCount, 0) AS SecondaryLSCount, 
				   CAST(ROUND(ISNULL((TotalLS.TotalLSCount * 100.0 /NULLIF(ST, 0)), 0), 2) AS float) AS SecondaryLSPercent
			FROM 
			(SELECT SUM(TotalLeadSources.TotalLSCount) AS [ST] FROM TotalLeadSources) AS t, 
			(SELECT SUM(PrimaryLeadSources.PrimaryLSCount) AS [PT] FROM PrimaryLeadSources) AS t1,
			TotalLeadSources TotalLS
			LEFT JOIN PrimaryLeadSources PrimaryLS ON TotalLS.LeadSourceID = PrimaryLS.LeadSourceID
	END
	ELSE IF (@FilterType = 2)	--Life Cycle Stages
	BEGIN
		INSERT INTO @LeadSourceIDs
		SELECT DropdownValueID, DropdownValue FROM DropdownValues WHERE AccountID = @AccountID AND IsDeleted = 0 AND DropdownID = 5 AND IsActive = 1
		;WITH TotalLeadSources
			AS
			(
				SELECT Dv.DropdownValueID AS LeadSourceID, Dv.DropdownValue, COUNT(CLM.ContactID) AS TotalLSCount FROM @LeadSourceIDs L 
				INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = L.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				INNER JOIN DropdownValues(NOLOCK) Dv ON Dv.DropdownValueID = L.ID AND dv.IsDeleted = 0 AND  dv.AccountID = @accountID 
				INNER JOIN Contacts(NOLOCK) C ON C.ContactID = CLM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
				INNER JOIN @EntitysTable ET ON ET.ID = C.LifecycleStage
				GROUP BY Dv.DropdownValueID, Dv.DropdownValue
			)
			, PrimaryLeadSources
			AS
			( 
				SELECT Dv.DropdownValueID AS LeadSourceID, Dv.DropdownValue, COUNT(CLM.ContactID) AS PrimaryLSCount FROM @LeadSourceIDs L 
				INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = L.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				INNER JOIN DropdownValues(NOLOCK) Dv ON Dv.DropdownValueID = L.ID AND dv.IsDeleted = 0 AND  dv.AccountID = @accountID 
				INNER JOIN Contacts(NOLOCK) C ON C.ContactID = CLM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
				INNER JOIN @EntitysTable ET ON ET.ID = C.LifecycleStage
				WHERE CLM.IsPrimaryLeadSource = 1
				GROUP BY Dv.DropdownValueID, Dv.DropdownValue
			)

			SELECT LS.ID AS LeadSourceID,
                   LS.Value AS Value,
				   ISNULL(PrimaryLS.PrimaryLSCount, 0) AS PrimaryLSCount,
				   CAST(ROUND(ISNULL((PrimaryLS.PrimaryLSCount * 100.0 /NULLIF(PT, 0)), 0), 2) AS float) AS PrimaryLSPercent, 
				   ISNULL(TotalLS.TotalLSCount, 0) AS SecondaryLSCount, 
				   CAST(ROUND(ISNULL((TotalLS.TotalLSCount * 100.0 /NULLIF(ST, 0)), 0), 2) AS float) AS SecondaryLSPercent
			FROM 
			(SELECT SUM(TotalLeadSources.TotalLSCount) AS [ST] FROM TotalLeadSources) AS t, 
			(SELECT SUM(PrimaryLeadSources.PrimaryLSCount) AS [PT] FROM PrimaryLeadSources) AS t1,
			@LeadSourceIDs LS
            LEFT JOIN TotalLeadSources TotalLS ON LS.ID = TotalLS.LeadSourceID
			LEFT JOIN PrimaryLeadSources PrimaryLS ON LS.ID = PrimaryLS.LeadSourceID
	END
	ELSE
	BEGIN
		INSERT INTO @LeadSourceIDs
		SELECT DropdownValueID, DropdownValue FROM DropdownValues WHERE AccountID = @AccountID AND IsDeleted = 0 AND DropdownID = 5 AND IsActive = 1
		;WITH TotalLeadSources	-- Communities
			AS
			(
				SELECT Dv.DropdownValueID AS LeadSourceID, dv.dropdownvalue,  COUNT(CLM.ContactID) AS TotalLSCount FROM @LeadSourceIDs L 
				INNEr JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = L.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				INNEr JOIN DropdownValues(NOLOCK) Dv ON Dv.DropdownValueID= L.ID AND dv.AccountID = @accountID AND dv.IsDeleted = 0
				INNEr JOIN @ContactCommunityMap CCM ON CCM.ContactID = CLM.ContactID
				INNEr JOIN @EntitysTable ET ON Et.ID = CCM.CommunityID
				INNEr JOIN Contacts(NOLOCK) C ON C.ContactID = CCM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
				GROUP BY Dv.DropdownValueID, dv.dropdownvalue
			)
			, PrimaryLeadSources
			AS
			( 
				SELECT Dv.DropdownValueID AS LeadSourceID, dv.dropdownvalue,  COUNT(CLM.ContactID) AS PrimaryLSCount FROM @LeadSourceIDs L 
				INNER JOIN ContactLeadSourceMap(NOLOCK) CLM ON CLM.LeadSouceID = L.ID AND CLM.LastUpdatedDate BETWEEN @StartDate AND @EndDate
				INNER JOIN DropdownValues(NOLOCK) Dv ON Dv.DropdownValueID= L.ID AND dv.AccountID = @accountID AND dv.IsDeleted = 0
				INNER JOIN @ContactCommunityMap CCM ON CCM.ContactID = CLM.ContactID
				INNER JOIN @EntitysTable ET ON Et.ID = CCM.CommunityID
				INNER JOIN Contacts(NOLOCK) C ON C.ContactID = CCM.ContactID AND C.AccountID = @AccountID AND C.IsDeleted = 0
				WHERE CLM.IsPrimaryLeadSource = 1
				GROUP BY Dv.DropdownValueID, dv.dropdownvalue
			)

			SELECT LS.ID AS LeadSourceID,
                   LS.Value AS Value,
			       ISNULL(PrimaryLS.PrimaryLSCount, 0) AS PrimaryLSCount,
			       CAST(ROUND(ISNULL((PrimaryLS.PrimaryLSCount * 100.0 /NULLIF(PT, 0)), 0), 2) AS float) AS PrimaryLSPercent, 
			       ISNULL(TotalLS.TotalLSCount, 0) AS SecondaryLSCount, 
			       CAST(ROUND(ISNULL((TotalLS.TotalLSCount * 100.0 /NULLIF(ST, 0)), 0), 2) AS float) AS SecondaryLSPercent
			FROM 
			(SELECT SUM(TotalLeadSources.TotalLSCount) AS [ST] FROM TotalLeadSources) AS t, 
			(SELECT SUM(PrimaryLeadSources.PrimaryLSCount) AS [PT] FROM PrimaryLeadSources) AS t1,
			@LeadSourceIDs LS
            LEFT JOIN TotalLeadSources TotalLS ON LS.ID = TotalLS.LeadSourceID
			LEFT JOIN PrimaryLeadSources PrimaryLS ON LS.ID = PrimaryLS.LeadSourceID
	END
END


GO


