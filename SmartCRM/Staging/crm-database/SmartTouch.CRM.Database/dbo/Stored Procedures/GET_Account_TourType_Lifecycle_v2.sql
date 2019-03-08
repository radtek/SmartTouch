  
CREATE PROCEDURE [dbo].[GET_Account_TourType_Lifecycle_v2](  
 @AccountID int,  
 @Type tinyint,  
 @Top5Only bit,  
 @DateRange varchar(1),  
 @StartDate date,  
 @EndDate date,  
 @SelectedList nvarchar(MAX),  
 @TrafficLifeCycle nvarchar(4000),  
 @TourType nvarchar(4000),  
 @StartDatePrev date = '1900-01-01',  
 @EndDatePrev date = '1900-01-01',  
 @Debug bit = 0  
)  
AS  
BEGIN  
 DECLARE @Info ReportInfo_1 --table(ID int, GridValue varchar(255), ContactID int, DateVal date, DateRange varchar(1), GroupDateRange int)  
 DECLARE @GroupList ReportGroupList --TABLE (ID int, Name varchar(255))  
 DECLARE @OptionList TABLE (ID int, Name varchar(255))  
 DECLARE @DataRange TABLE (RangeID int, RangeValue VARCHAR(10))  
  
 DECLARE @ResultID int  
  
 INSERT INTO StoreProcExecutionResults(ProcName, AccountID)  
 VALUES('GET_Account_TourType_Lifecycle_v2', @AccountID)  
  
 SET @ResultID = scope_identity()  
  
 IF(@Type = 0)  
 BEGIN  
  INSERT INTO @GroupList  
  SELECT *  
  FROM GetInfo(@AccountID, 'user', @SelectedList)  
 END  
 ELSE  
 BEGIN  
  INSERT INTO @GroupList  
  SELECT *  
  FROM GetInfo(@AccountID, 'community', @SelectedList)  
 END  
  
 IF isnull(@Type, 0) = 0  
 BEGIN  
  INSERT INTO @Info  
  SELECT U.USERID ID, dv.DropdownValue GridValue,tt.DropdownValue GridVaue_1 ,c.ContactID, CONVERT(varchar(10), T.TourDate, 120) TourDate, 'c',  
    CASE ISNULL(@DateRange, 'D')  
     WHEN 'D' THEN  DATEDIFF(dd, @StartDate, T.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)  
     WHEN 'M' THEN (DATEDIFF(Month, @StartDate, T.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)  
     ELSE (DATEPART(wk, T.TourDate) - DATEPART(wk, @StartDate)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)  
    END   
     FROM DBO.TOURS T  (NOLOCK)   
          INNER JOIN contacts c  (NOLOCK) on T.accountid = c.accountid --and c.contactID = T.ContactID          
    INNER JOIN ContactTourMap ctm  (NOLOCK) on ctm.tourid = t.tourid and c.contactid = ctm.contactid  
    INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TOurID  
    INNER JOIN users u (NOLOCK)  on u.accountid IN (T.accountid, 1) and u.userid = UTM.UserID  
    INNER JOIN dropdownvalues dv  (NOLOCK) on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3  
    --INNER JOIN dropdownvalues co on co.accountid = c.accountid and co.dropdownid = 7  
      
    INNER JOIN dropdownvalues tt  (NOLOCK) on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8  
      
    INNER JOIN @GroupList gl on gl.ID = u.UserID  
    INNER JOIN (  
     SELECT DataValue FROM dbo.Split(@TourType, ',')  
    ) tt1 on tt1.DataValue = tt.dropdownvalueid  
  WHERE T.AccountID = @AccountID  
    AND dv.IsActive = 1  
    AND c.IsDeleted = 0  
    --and u.userid in (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))  
    --AND tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))  
    --AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @StartDate AND @EndDate  
    AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120)   
     AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120)   
 END  
 ELSE  
 BEGIN  
  INSERT INTO @Info  
  SELECT CAST(co.dropdownvalueid AS integer) ID, dv.DropdownValue Name,tt.DropdownValue as GridVaue_1 , c.ContactID, CONVERT(varchar(10), T.TourDate, 120) TourDate, 'c',  
    CASE ISNULL(@DateRange, 'D')  
     WHEN 'D' THEN  DATEDIFF(dd, @StartDate, T.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)  
     WHEN 'M' THEN (DATEDIFF(Month, @StartDate, T.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)  
     ELSE (DATEPART(wk, T.TourDate) - DATEPART(wk, @StartDate)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)  
    END   
  FROM DBO.TOURS T (NOLOCK)   
          INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TOurID  
          INNER JOIN contacts c  (NOLOCK) on T.accountid = c.accountid --and c.contactID = T.ContactID          
    INNER JOIN users u  (NOLOCK) on u.accountid IN (T.accountid, 1) and u.userid = UTM.userid  
    INNER JOIN dropdownvalues dv  (NOLOCK) on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3  
    INNER JOIN dropdownvalues co  (NOLOCK) on co.accountid = c.accountid and T.CommunityID = co.DropdownValueID and co.dropdownid = 7  
    INNER JOIN dropdownvalues tt  (NOLOCK) on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8  
    INNER JOIN ContactTourMap ctm  (NOLOCK) on ctm.tourid = t.tourid and c.contactid = ctm.contactid  
    INNER JOIN @GroupList gl on gl.ID = co.dropdownvalueid  
   INNER JOIN (  
    SELECT DataValue FROM dbo.Split(@TourType, ',')  
    ) tt1 on tt1.DataValue = tt.dropdownvalueid  
  WHERE T.AccountID = @AccountID  
    and dv.IsActive = 1  
    AND c.IsDeleted = 0  
    --and co.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))  
    --and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))  
   -- AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @StartDate AND @EndDate  
   AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120)   
     AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120)   
 END  
  
 IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')  
 BEGIN  
  IF isnull(@Type, 0) = 0  
  BEGIN  
   INSERT INTO @Info  
   SELECT U.USERID ID, dv.DropdownValue GridValue,tt.DropdownValue GridVaue_1, c.ContactID, CONVERT(varchar(10), T.TourDate, 120) TourDate, 'p',  
     CASE ISNULL(@DateRange, 'D')  
      WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, T.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)  
      WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, T.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)  
      ELSE (DATEPART(wk, T.TourDate) - DATEPART(wk, @StartDatePrev)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)  
     END   
   FROM DBO.TOURS T (NOLOCK)   
              INNER JOIN contacts c (NOLOCK)  on T.accountid = c.accountid --and c.contactID = T.ContactID   
     INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TOurID         
        INNER JOIN users u  (NOLOCK) on u.accountid IN (T.accountid, 1) and u.userid = UTM.UserID  
     INNER JOIN dropdownvalues dv (NOLOCK)  on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3  
     --INNER JOIN dropdownvalues co on co.accountid = c.accountid and co.dropdownid = 7       
     INNER JOIN dropdownvalues tt  (NOLOCK) on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8  
     INNER JOIN ContactTourMap ctm  (NOLOCK) on ctm.tourid = t.tourid and c.contactid = ctm.contactid  
  
    INNER JOIN @GroupList gl on gl.ID = u.UserID  
     INNER JOIN (  
      SELECT DataValue FROM dbo.Split(@TourType, ',')  
     ) tt1 on tt1.DataValue = tt.dropdownvalueid  
   WHERE T.AccountID = @AccountID  
     AND dv.IsActive = 1  
     AND c.IsDeleted = 0  
     --and u.userid in (SELECT DataValue FROM dbo.Split(@SelectedIDs, ','))  
     --AND tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))  
    -- AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev  
    AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120)   
     AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120)   
  END  
  ELSE  
  BEGIN  
   INSERT INTO @Info  
   SELECT CAST(co.dropdownvalueid AS integer) ID, dv.DropdownValue Name,tt.DropdownValue GridVaue_1, c.ContactID, CONVERT(varchar(10), T.TourDate, 120) TourDate, 'p',  
     CASE ISNULL(@DateRange, 'D')  
      WHEN 'D' THEN  DATEDIFF(dd, @StartDatePrev, T.TourDate) + 1 --CONVERT(VARCHAR(10), t.TourDate, 120)  
      WHEN 'M' THEN (DATEDIFF(Month, @StartDatePrev, T.TourDate)) + 1 --CONVERT(VARCHAR(10), DATEADD(Month, 1, t.TourDate), 120)  
      ELSE (DATEPART(wk, T.TourDate) - DATEPART(wk, @StartDatePrev)) + 1 --'WEEK-' + CAST(DATEPART(wk, t.TourDate) AS VARCHAR)  
     END   
   FROM DBO.TOURS T (NOLOCK)  
           INNER JOIN UserTourMap UTM (NOLOCK) ON UTM.TourID = T.TOurID   
              INNER JOIN contacts c  (NOLOCK) on T.accountid = c.accountid --and c.contactID = T.ContactID          
        INNER JOIN users u (NOLOCK)  on u.accountid IN (T.accountid, 1) and u.userid = UTM.userid  
     INNER JOIN dropdownvalues dv  (NOLOCK) on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3  
     INNER JOIN dropdownvalues co  (NOLOCK) on co.accountid = c.accountid and co.dropdownid = 7  
     --INNER JOIN tours t on t.communityid = co.dropdownvalueid  
     INNER JOIN dropdownvalues tt  (NOLOCK) on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8  
     INNER JOIN ContactTourMap ctm  (NOLOCK) on ctm.tourid = t.tourid and c.contactid = ctm.contactid  
     INNER JOIN @GroupList gl on gl.ID = co.dropdownvalueid  
     INNER JOIN (  
      SELECT DataValue FROM dbo.Split(@TourType, ',')  
     ) tt1 on tt1.DataValue = tt.dropdownvalueid  
   WHERE T.AccountID = @AccountID  
     and dv.IsActive = 1  
     AND c.IsDeleted = 0  
     --and co.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))  
     --and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))  
    -- AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @StartDatePrev AND @EndDatePrev  
    AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @StartDate, 120)   
     AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @EndDate, 120)   
  END  
 END  
 IF(@Debug=1)  
 BEGIN  
  SELECT *  
  FROM @Info  
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
  SELECT [Number], CONVERT(VARCHAR(10), [DATE], 120) Label  
  FROM [DateTable]  
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
  SELECT [Number], CONVERT(VARCHAR(10), [DATE], 120) Label  
  FROM [DateTable]  
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
  SELECT [Number], 'WEEK-' + CAST([WeekNum] AS VARCHAR) Label  
  FROM [DateTable]  
  OPTION (maxrecursion 0)  
 END  
  
 SELECT RangeID ID, ISNULL([P], 0) [P], ISNULL([C], 0) [C]  
 FROM (  
    SELECT RangeID, DateRange, GroupDateRange, count(RangeID) TotalCount  
    FROM @DataRange dr left join @info i on dr.RangeID = i.GroupDateRange  
    GROUP BY RangeID, DateRange, GroupDateRange  
   ) tmp  
 PIVOT(  
  MAX(TotalCount)  
  FOR DateRange IN ([P], [C])  
 ) P  
 ORDER BY RangeID  
  
 IF @Top5Only = 1  
 BEGIN  
  SELECT top 5 GridValue, ISNULL([P], 0) [P], ISNULL([C], 0) [C]  
  FROM (  
     SELECT GridValue, DateRange, count(GridValue) TotalCount  
     FROM @Info  
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
  SELECT GridValue, ISNULL([P], 0) [P], ISNULL([C], 0) [C]  
  FROM (  
     SELECT GridValue, DateRange, count(GridValue) TotalCount  
     FROM @Info  
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
   @TourTypeList NVARCHAR(MAX),  
   @TourTypeListC NVARCHAR(MAX),  
   @TourTypeListP NVARCHAR(MAX),  
   @TourTypeListNull NVARCHAR(MAX),  
   @ColumnList NVARCHAR(MAX)  
  
 IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')  
 BEGIN  
  SELECT @LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + Name + ' ' + DataValue + ']',  
    @LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + Name + ' ' + DataValue + '], 0) [' + Name + ' ' + DataValue + ']'  
  FROM dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle) gi inner join dbo.Split('(c),(p)', ',') i ON 1=1  
  
  SELECT @LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + ' (c)], 0) ',  
    @LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + ' (p)], 0) '  
  FROM dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)  
  
  SELECT @TourTypeList = COALESCE(@TourTypeList + ', ', '') + '[' + Name + ' ' + DataValue + ']',  
    @TourTypeListNull = COALESCE(@TourTypeListNull + ', ', '') + 'isnull([' + Name + ' ' + DataValue + '], 0) [' + Name + ' ' + DataValue + ']'  
  FROM dbo.GetInfo(@AccountID, 'TourType', @TourType) gi inner join dbo.Split('(c),(p)', ',') i ON 1=1  
  
  SELECT @TourTypeListC = COALESCE(@TourTypeListC + ' + ', '') + 'isnull([' + Name + ' (c)], 0) ',  
    @TourTypeListP = COALESCE(@TourTypeListP + ' + ', '') + 'isnull([' + Name + ' (p)], 0) '  
  FROM dbo.GetInfo(@AccountID, 'TourType', @TourType)  
  
 END  
 ELSE  
 BEGIN  
  SELECT @LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + Name + ']',  
    @LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + Name + '], 0) [' + Name + ']'  
  FROM dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)  
  
  SELECT @LeadSourceListC = COALESCE(@LeadSourceListC + ' + ', '') + 'isnull([' + Name + '], 0) ',  
    @LeadSourceListP = COALESCE(@LeadSourceListP + ' + ', '') + 'isnull([' + Name + '], 0) '  
  FROM dbo.GetInfo(@AccountID, 'LifeCycle', @TrafficLifeCycle)  
  
     SELECT @TourTypeList = COALESCE(@TourTypeList + ', ', '') + '[' + Name + ']',  
    @TourTypeListNull = COALESCE(@TourTypeListNull + ', ', '') + 'isnull([' + Name + '], 0) [' + Name + ']'  
  FROM dbo.GetInfo(@AccountID, 'TourType', @TourType)  
  
  SELECT @TourTypeListC = COALESCE(@TourTypeListC + ' + ', '') + 'isnull([' + Name + '], 0) ',  
    @TourTypeListP = COALESCE(@TourTypeListP + ' + ', '') + 'isnull([' + Name + '], 0) '  
  FROM dbo.GetInfo(@AccountID, 'TourType', @TourType)  
  
    
 END  
  
 PRINT @LeadSourceList  
 PRINT @LeadSourceListNull  
 PRINT @LeadSourceListC  
 PRINT @LeadSourceListP  
  
 PRINT @TourTypeList  
 PRINT @TourTypeListNull  
 PRINT @TourTypeListC  
 PRINT @TourTypeListP  
  
 DECLARE @SQL NVARCHAR(MAX)  
 DECLARE @SQL_1 NVARCHAR(MAX)  
   
 IF(ISNULL(@StartDatePrev, '1900-01-01') > '1900-01-01')  
 BEGIN  
  
  SET @SQL = N'  
    SELECT gl.ID, gl.Name,  
      ' + @LeadSourceListNull + ',ISNULL([Total (c)], 0) [Total (c)],  
      ISNULL([Total (p)], 0) [Total (p)]  
    FROM @GroupList gl left join (  
       SELECT id,  
         ' + @LeadSourceList + ',  
         ' + @LeadSourceListC + ' [Total (c)],  
         ' + @LeadSourceListP + ' [Total (p)]  
       FROM (  
          SELECT id, GridValue + '' ('' + DateRange + '')'' ColumnName, count(*) Total  
          FROM @Info  
          GROUP BY id, GridValue + '' ('' + DateRange + '')''  
         ) tmp  
       pivot(  
        MAX(Total)  
        FOR ColumnName IN (' + @LeadSourceList + ')  
       )p  
     ) tmp on gl.ID = tmp.id  
   '  
  
   SET @SQL_1 = N'  
    SELECT gl.ID, gl.Name,  
      ' + @TourTypeListNull + ',ISNULL([Total (c)], 0) [Total (c)],  
      ISNULL([Total (p)], 0) [Total (p)]  
    FROM @GroupList gl left join (  
       SELECT id,  
         ' + @TourTypeList + ',  
         ' + @TourTypeListC + ' [Total (c)],  
         ' + @TourTypeListP + ' [Total (p)]  
       FROM (  
          SELECT id, GridVaue_1 + '' ('' + DateRange + '')'' ColumnName, count(*) Total  
          FROM @Info  
          GROUP BY id, GridVaue_1 + '' ('' + DateRange + '')''  
         ) tmp  
       pivot(  
        MAX(Total)  
        FOR ColumnName IN (' + @TourTypeList + ')  
       )p  
     ) tmp on gl.ID = tmp.id  
   '  
 END  
 ELSE  
 BEGIN  
  
  SET @SQL = N'  
    SELECT gl.ID, gl.Name,  
      ' + @LeadSourceListNull + ',ISNULL([Total], 0) [Total]  
    FROM @GroupList gl left join (  
       SELECT id, ' + @LeadSourceList + ',  
         ' + @LeadSourceListC + ' [Total]  
       FROM (  
          SELECT id, GridValue ColumnName, count(*) Total  
          FROM @Info  
          GROUP BY id, GridValue  
         ) tmp  
       pivot(  
        MAX(Total)  
        FOR ColumnName IN (' + @LeadSourceList + ')  
       )p  
     ) tmp on gl.ID = tmp.id  
   '  
  
   SET @SQL_1 = N'  
    SELECT gl.ID, gl.Name,  
      ' + @TourTypeListNull + ',ISNULL([Total], 0) [Total]  
    FROM @GroupList gl left join (  
       SELECT id, ' + @TourTypeList + ',  
         ' + @TourTypeListC + ' [Total]  
       FROM (  
          SELECT id, GridVaue_1 ColumnName, count(*) Total  
          FROM @Info  
          GROUP BY id, GridVaue_1  
         ) tmp  
       pivot(  
        MAX(Total)  
        FOR ColumnName IN (' + @TourTypeList + ')  
       )p  
     ) tmp on gl.ID = tmp.id  
   '  
  
   print 'In If'  
 END  
  
 PRINT @SQL  
 PRINT @SQL_1  
  
 EXEC sp_executesql @SQL, N'@GroupList ReportGroupList READONLY, @Info ReportInfo_1 READONLY', @GroupList, @Info  
 EXEC sp_executesql @SQL_1, N'@GroupList ReportGroupList READONLY, @Info ReportInfo_1 READONLY', @GroupList, @Info  
  
  
   
END  
  
  
  
/*  
select * from dropdowns(nolock)   
  
select * from users(nolock) where accountid=339 and isdeleted=0 and status=1  
  
select * from dropdownvalues(nolock) where dropdownid=3 and accountid=339 and isdeleted=0  
  
select * from dropdownvalues(nolock) where dropdownid=8 and accountid=339 and isdeleted=0  
  
EXEC [dbo].[GET_Account_TourType_Lifecycle_v2]  
  @AccountID=339,  
  @Type=1,  
  @Top5Only=1,  
  @DateRange='D',  
  @StartDate='2017-01-01',  
  @EndDate='2017-09-22',  
  @SelectedList='1452,1455,1464,1471,1472,1473,1583,1768,1771,1774,1776,1782,1783,1784,1797,1825,1865,1883,1896,1904,1905,1906,1908,1916,1917,1918,1931',  
  @TrafficLifeCycle='12569,12570,12571,12572,12573,12644,12654,12655,13516,13570,13718,13719,13720,13721,13722,13723,14014',  
  @TourType='12589,12590,12591,12662,12663,12856,13569,14010',  
  @StartDatePrev = '1900-01-01',  
  @EndDatePrev = '1900-01-01',  
  @Debug= 0  
  
*/  
  