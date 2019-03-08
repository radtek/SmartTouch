CREATE PROCEDURE [dbo].[Get_ToursByContacts]
(
	@AccountId INT,
	@FromDate DATETIME,
	@ToDate DATETIME,
	--@OwnerId INT,
	@TourStatus INT,
	@TourType INT,
	@TourCommunity INT,
	@PageSize INT,
	@PageNumber INT
)
AS
BEGIN
	
	IF(@TourStatus = 1)
	  BEGIN
	   SET @TourStatus = 0
	  END
	ELSE IF(@TourStatus = 2)
	 BEGIN
	   SET @TourStatus = 1
	 END
	ELSE
	 BEGIN
	   SET @TourStatus = -1
	 END

	SELECT T.TourId, T.TourDetails, CTM.IsCompleted TourStatus, T.TourDate, T.TourType, T.CommunityID, T.CreatedOn, T.CreatedBy, C.ContactId , C.FirstName, C.LastName, C.LifecycleStage, 
	DS.DropDownValue as OpportunityStage,COUNT(1) OVER() as TotalCount
	INTO #tourContacts
	FROM Tours (NOLOCK) T
	INNER JOIN ContactTourMap (NOLOCK) CTM ON CTM.TourId = T.TourId
	INNER JOIN Contacts C (NOLOCK) ON C.ContactId = CTM.ContactId AND C.AccountId = T.AccountId
	LEFT JOIN OpportunityContactMap (NOLOCK) OCM ON OCM.ContactId = C.COntactId
	LEFT JOIN Opportunities (NOLOCK) O ON O.OpportunityId = OCM.OpportunityId
	LEFT JOIN DropDownValues DS (NOLOCK) ON DS.DropDownValueId = O.StageId
	WHERE C.IsDeleted = 0 AND T.AccounTid = @AccountId AND
		((@TourStatus > -1 AND CTM.IsCompleted = @TourStatus) OR (@TourStatus = -1 AND CTM.IsCompleted IN(0,1))) AND
		((@TourCommunity > 0 AND T.CommunityID = @TourCommunity) OR (@TourCommunity =0 AND T.CommunityID = T.CommunityID)) AND
		((@TourType > 0 AND T.TourType = @TourType) OR (@TourType = 0 AND T.TourType = T.TourType)) AND
		T.TourDate BETWEEN @FromDate AND @ToDate
	ORDER BY T.TourDate DESC
	OFFSET (@PageNumber-1)*@PageSize ROWS
	FETCH NEXT @PageSize ROWS ONLY

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

	SELECT tc.ContactId, tc.FirstName +' '+ tc.LastName AS ContactName, cp.Email as PRimaryEmail, cpn.PhoneNumber as PrimaryPhoneNumber, 
	tc.TourId, DT.DropdownValue TourType,tc.TourDetails, tc.TourDate, tc.TourStatus,  DC.DropdownValue Commnity,  
	DL.DropdownValue LifeCycleStage, tc.OpportunityStage, a.assignedTo, tc.CreatedOn, U.FirstName +' '+ U.LastName AS CreatedBy,TotalCount
	FROM #tourContacts tc
	INNER JOIN DropDownValues DC (NOLOCK) ON DC.DropDownValueId = tc.CommunityId
	INNER JOIN DropDownValues DT (NOLOCK) ON DT.DropDownValueId = tc.TourType
	INNER JOIN DropDownValues DL (NOLOCK) ON DL.DropDownValueId = tc.LifecycleStage
	INNER JOIN #assignedTo a ON a.tourId = tc.tourId
	INNER JOIN Users u (NOLOCK) ON tc.CreatedBy = U.UserId
	LEFT JOIN ContactEmails (NOLOCK) CP ON CP.ContactId = tc.ContactId AND CP.AccountId = @AccountId AND CP.IsDeleted = 0 AND CP.IsPrimary = 1
	LEFT JOIN ContactPhoneNumbers CPN (NOLOCK) ON CPN.ContactId = tc.ContactId AND CPN.IsPrimary = 1 AND CPN.IsDeleted = 0
	
END

/*

DECLARE @AccountId INT =  339,
	@FromDate DATETIME = '1/5/2016 7:51:13 AM',
	@ToDate DATETIME = '1/5/2017 7:51:13 AM',
	@OwnerId INT = 0,
	@TourStatus INT = 0,
	@TourType INT = 0,
	@TourCommunity INT = 12583,
	@PageSize INT = 10,
	@PageNumber INT = 1

	--SELECT DATEADD(yy, -1,GETUTCDATE())
 EXEC dbo.Get_ToursByContacts @AccountId, @FromDate, @ToDate,  @TourStatus,@TourType,@TourCommunity, @PageSize, @PageNumber
	
*/