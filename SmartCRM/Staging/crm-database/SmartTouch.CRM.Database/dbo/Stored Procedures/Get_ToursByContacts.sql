
CREATE PROCEDURE [dbo].[Get_ToursByContacts]
(
	@AccountId INT,
	@FromDate DATETIME,
	@ToDate DATETIME,
	--@OwnerId INT,
	@TourStatus VARCHAR(6),
	@TourType VARCHAR(1000),
	@TourCommunity VARCHAR(1000),
	@PageSize INT,
	@PageNumber INT,
	@SortColumn Nvarchar(20),
	@SortDirection varchar(4)
)
AS
BEGIN
	
	DECLARE @TourStatusTable TABLE
	(
		StatusID BIT
	)
	DECLARE @TourTypesTable TABLE
	(
		TypeId INT
	)
	DECLARE @TourCommunityTable TABLE
	(
		CommunityId INT
	)
	INSERT INTO @TourStatusTable
	SELECT DataValue FROM dbo.Split_2(@TourStatus,',')
	INSERT INTO @TourTypesTable
	SELECT DataValue FROM dbo.Split_2(@TourType,',')
	INSERT INTO @TourCommunityTable
	SELECT DataValue FROM dbo.Split_2(@TourCommunity,',')

	SELECT T.TourId, T.TourDetails, CTM.IsCompleted TourStatus, T.TourDate, T.TourType, T.CommunityID, T.CreatedOn, T.CreatedBy, C.ContactId , C.FirstName, C.LastName, C.Company, C.LifecycleStage, 
	C.ContactType,COUNT(1) OVER() as TotalCount
	INTO #tourContacts
	FROM Tours (NOLOCK) T
	INNER JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourId = T.TourId
	INNER JOIN Contacts C (NOLOCK) ON C.ContactId = CTM.ContactId AND C.AccountId = T.AccountId
	INNER JOIN @TourStatusTable TS ON TS.StatusID = CTM.IsCompleted
	INNER JOIN @TourTypesTable TT ON TT.TypeId = T.TourType
	INNER JOIN @TourCommunityTable TCM ON TCM.CommunityId = T.CommunityID 
	WHERE C.IsDeleted = 0 AND T.AccounTid = @AccountId AND
		T.TourDate BETWEEN @FromDate AND @ToDate
	ORDER BY T.TourDate DESC
	


	declare @count int 
    declare @counter int = 1
	declare @tours table (tourid int)
	
	insert into @tours
	select distinct tourid from #tourContacts                    

	select @count = count(1) from @tours
	create table #assignedTo (tourId int, assignedTo varchar(max))
	while @counter<= @count
		begin
			declare @tourId int
			select top(@counter) @tourId = tourId from @tours
			order by tourId desc

			declare @assignedTo varchar(max)
						
			select @assignedTo = coalesce(@assignedTo+',','') + u.FirstName + ' ' + u.LastName
			from UserTourMap (nolock) uam
			inner join Users u (nolock) on u.userid = uam.userid
			where uam.tourId = @tourId

			insert into #assignedTo
			select @tourId, @assignedTo

			set @counter = @counter + 1
			set @assignedTo = null
		end


if object_id('tempdb..#K')is not null 
begin
Drop table #K
end


	SELECT tc.ContactId, 
	       CASE WHEN(tc.ContactType = 1) THEN tc.FirstName +' '+ tc.LastName ELSE tc.Company END AS ContactName,   
		   cp.Email as PrimaryEmail, 
		   dbo.GetPhoneFormart(CPN.PhoneNumber,CPN.PhoneType,CPN.CountryCode,CPN.Extension) as PrimaryPhoneNumber, 
	       tc.TourId, 
		   DT.DropdownValue TourType,
		   tc.TourDetails, 
		   tc.TourDate, 
		   tc.TourStatus,  
		   DC.DropdownValue Commnity,  
	       DL.DropdownValue LifeCycleStage, 
		   a.assignedTo, 
		   tc.CreatedOn, 
		   U.FirstName +' '+ U.LastName AS CreatedBy,
		   tc.ContactType,
		   TotalCount
    into #K
	FROM #tourContacts tc
	INNER JOIN DropDownValues DC (NOLOCK) ON DC.DropDownValueId = tc.CommunityId
	INNER JOIN DropDownValues DT (NOLOCK) ON DT.DropDownValueId = tc.TourType
	INNER JOIN #assignedTo a ON a.tourId = tc.tourId
	INNER JOIN Users u (NOLOCK) ON tc.CreatedBy = U.UserId
	LEFT JOIN DropDownValues DL (NOLOCK) ON DL.DropDownValueId = tc.LifecycleStage
	LEFT JOIN ContactEmails (NOLOCK) CP ON CP.ContactId = tc.ContactId AND CP.AccountId = @AccountId AND CP.IsDeleted = 0 AND CP.IsPrimary = 1
	LEFT JOIN ContactPhoneNumbers CPN (NOLOCK) ON CPN.ContactId = tc.ContactId AND CPN.IsPrimary = 1 AND CPN.IsDeleted = 0
	--ORDER BY tc.TourDate DESC
	--OFFSET (@PageNumber-1)*@PageSize ROWS
	--FETCH NEXT @PageSize ROWS ONLY
	
declare @SQL varchar(200),@PageNumber1 varchar(10)=Cast(@PageNumber as varchar(10)),
@PageSize1 varchar(10)=Cast(@PageSize as varchar(10))

Set @SQL=' SELECT * FROM #K'+' '+'
 ORDER BY  '+ @SortColumn+' '+@SortDirection +'  '+'
 OFFSET ('+@PageNumber1+'-1)*'+@PageSize1+' ROWS
 FETCH NEXT '+@PageSize1+' ROWS ONLY '

 Print @SQL
 EXEC (@SQL)


END

/*

DECLARE @AccountId INT =  4218,
	@FromDate DATETIME = '1/5/2016 7:51:13 AM',
	@ToDate DATETIME = '1/5/2017 7:51:13 AM',
	@OwnerId INT = 0,
	@TourStatus INT = 0,
	@TourType INT = 0,
	@TourCommunity INT = 0,
	@PageSize INT = 10,
	@PageNumber INT = 1,
	@SortColumn varchar(20)='ContactName',
	@SortDirection varchar(5)='asc'


	--SELECT DATEADD(yy, -1,GETUTCDATE())
 EXEC dbo.Get_ToursByContacts @AccountId, @FromDate, @ToDate,  @TourStatus,@TourType,@TourCommunity, @PageSize, @PageNumber,@SortColumn,@SortDirection 

	
*/