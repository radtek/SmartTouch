
CREATE PROC [dbo].[GET_Account_Traffic_LeadSource_AreaChart]
	(
		@AccountID		int,
		@FromDate       datetime,
		@ToDate         datetime,
		@IsAdmin		tinyint,
		@OwnerID		int
	)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY	
	
	DECLARE @AreaChart		TABLE (DateNo tinyint, Present int, Previous int)
	DECLARE @DateRecords	TABLE (DateNo tinyint)
	DECLARE @PrevFromDate	datetime = DATEADD(day, -30, @FromDate),
			@PrevToDate		datetime = DATEADD(day, -30, @ToDate)
	
		;WITH DateTable
		AS
		(
			SELECT @FromDate AS [DATE]
			UNION ALL
			SELECT DATEADD(day, 1, [DATE])
			FROM DateTable
			WHERE DATEADD(day, 1, [DATE]) <=  @ToDate
		)

		INSERT INTO @DateRecords (DateNo)
		SELECT DISTINCT DATEPART(day, [DATE])
		FROM	[DateTable]
		OPTION (maxrecursion 0)

	IF (@IsAdmin = 1)
		BEGIN
			/* Default 30 Days */
			INSERT INTO @AreaChart (DateNo, Present, Previous)
			SELECT	DATEPART(DAY, T.TourDate) DateNo, COUNT(T.TourID) TourCounts, 0
			FROM	dbo.DropdownValues(NOLOCK) dv 
					INNER JOIN dbo.Tours (NOLOCK) t ON t.CommunityID = dv.DropdownValueID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) ctm ON ctm.TourID = t.TourID
					INNER JOIN dbo.Users  (NOLOCK) u on (u.accountid = dv.accountid or u.AccountID = 1)  and u.UserID = t.CreatedBy and u.Status=1
					INNER JOIN dbo.Contacts  (NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID 
					--INNER JOIN dbo.DropdownValues DV2  (NOLOCK) ON (t.AccountID = DV2.AccountID)
					--INNER JOIN dbo.ContactLeadSourceMap  (NOLOCK) csm on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID and csm.IsPrimaryLeadSource=1
			WHERE	dv.accountid =  @AccountID
					AND dv.DropdownID = 7
					--AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @FromDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @ToDate, 120) 
					--AND CONVERT(VARCHAR(10), T.CreatedOn, 110) BETWEEN @FromDate AND @ToDate
			--GROUP BY DATEDIFF(DAY,@FromDate, T.CreatedOn)
			     -- AND  T.CreatedOn >= @FromDate AND T.CreatedOn <=  @ToDate
				GROUP BY DATEPART(DAY, T.TourDate)

			INSERT INTO @AreaChart (DateNo, Present, Previous)
			SELECT	DATEPART(DAY, T.TourDate) DateNo, 0, COUNT(T.TourID) TourCounts
			FROM	dbo.DropdownValues(NOLOCK) dv 
					INNER JOIN dbo.Tours  (NOLOCK) t ON t.CommunityID = dv.DropdownValueID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) ctm ON ctm.TourID = t.TourID
					INNER JOIN dbo.Users  (NOLOCK) u on (u.accountid =dv.accountid or u.AccountID = 1) and u.UserID = t.CreatedBy and u.Status=1
					INNER JOIN dbo.Contacts  (NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID 
					--INNER JOIN dbo.DropdownValues  (NOLOCK) DV2 ON (t.AccountID = DV2.AccountID)
					--INNER JOIN dbo.ContactLeadSourceMap  (NOLOCK) csm on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID and csm.IsPrimaryLeadSource=1
			WHERE	dv.accountid =  @AccountID
					AND dv.DropdownID = 7
					--AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @PrevFromDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @PrevToDate, 120) 
					  -- AND  T.CreatedOn >= @PrevFromDate AND T.CreatedOn <=  @PrevToDate
					--AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @PrevFromDate AND @PrevToDate
			--GROUP BY DATEDIFF(DAY,@FromDate, T.CreatedOn)
				GROUP BY DATEPART(DAY, T.TourDate)
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			INSERT INTO @AreaChart (DateNo, Present, Previous)
			SELECT	DATEPART(DAY, T.TourDate) DateNo, COUNT(T.TourID) TourCounts, 0
			FROM	dbo.DropdownValues(NOLOCK) dv
					INNER JOIN dbo.Tours  (NOLOCK) t ON t.CommunityID = dv.DropdownValueID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) ctm ON ctm.TourID = t.TourID
				--	INNER JOIN dbo.Users u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy and u.Status=1
				INNER JOIN dbo.Users  (NOLOCK) u on (u.accountid = dv.accountid or u.AccountID = 1) and u.UserID = t.CreatedBy and u.Status=1
					INNER JOIN dbo.Contacts  (NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					--INNER JOIN dbo.DropdownValues  (NOLOCK) DV2 ON (t.AccountID = DV2.AccountID)
					--INNER JOIN dbo.ContactLeadSourceMap  (NOLOCK) csm on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID and csm.IsPrimaryLeadSource=1
			WHERE	dv.accountid =  @AccountID
					AND T.CreatedBy = @OwnerID
					AND dv.DropdownID = 7
					--AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @FromDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @ToDate, 120) 
					--   AND  T.CreatedOn >= @FromDate AND T.CreatedOn <=  @ToDate
					--AND CONVERT(VARCHAR(10), T.CreatedOn, 110) BETWEEN @FromDate AND @ToDate
			--GROUP BY DATEDIFF(DAY,@FromDate, T.CreatedOn)
				GROUP BY DATEPART(DAY, T.TourDate)

			INSERT INTO @AreaChart (DateNo, Present, Previous)
			SELECT	DATEPART(DAY, T.TourDate) DateNo, 0, COUNT(T.TourID) TourCounts
			FROM	dbo.DropdownValues(NOLOCK) dv 
					INNER JOIN dbo.Tours  (NOLOCK) t ON t.CommunityID = dv.DropdownValueID
					INNER JOIN dbo.ContactTourMap  (NOLOCK) ctm ON ctm.TourID = t.TourID
					INNER JOIN dbo.Users  (NOLOCK) u on (u.accountid =dv.accountid or u.AccountID = 1) and u.UserID = t.CreatedBy and u.Status=1
					INNER JOIN dbo.Contacts  (NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
					--INNER JOIN dbo.DropdownValues  (NOLOCK) DV2 ON (t.AccountID = DV2.AccountID)
					--INNER JOIN dbo.ContactLeadSourceMap  (NOLOCK) csm on csm.ContactID = c.ContactID and csm.LeadSouceID = dv2.DropdownValueID and csm.IsPrimaryLeadSource=1
			WHERE	dv.accountid =  @AccountID
					AND T.CreatedBy = @OwnerID
					AND dv.DropdownID = 7
					--AND dv2.DropdownID = 5
					AND dv.IsActive = 1
					AND c.IsDeleted = 0
					AND CONVERT(VARCHAR(10), T.TourDate, 120) >= CONVERT(VARCHAR(10), @PrevFromDate, 120) 
					AND CONVERT(VARCHAR(10), T.TourDate, 120) <= CONVERT(VARCHAR(10), @PrevToDate, 120) 
					--  AND  T.CreatedOn >= @PrevFromDate AND T.CreatedOn <=  @PrevToDate
					--AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN @PrevFromDate AND @PrevToDate
				--GROUP BY DATEDIFF(DAY,@FromDate, T.CreatedOn)
				GROUP BY DATEPART(DAY, T.TourDate)
		END

		SELECT	DR.DateNo, ISNULL(AC.Present, 0) Present, ISNULL(AC.Previous, 0) Previous
			FROM @DateRecords DR
				LEFT JOIN @AreaChart AC ON DR.DateNo = AC.DateNo
			ORDER BY DR.DateNo

	END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF


END

/*
	  
	  EXEC [dbo].[GET_Account_Traffic_LeadSource_AreaChart]
		 @AccountID			= 4218,
		 @FromDate          = '8/29/2016 6:30:00 PM',       
		 @ToDate            = '9/29/2016 6:29:00 PM',
		 @IsAdmin			= 1,
		 @OwnerID			= 6889

*/
GO


