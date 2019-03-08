
CREATE procedure [dbo].[GET_Account_TourType_Lifecycle_v1](
	@AccountID			int,
	@Type				tinyint,
	@StartDate          datetime,
	@EndDate            datetime,
	@SelectedIDs		VARCHAR(MAX),
	@TourTypes			VARCHAR(MAX),
	@TrafficLifeCycle	VARCHAR(MAX),
	@ContactListID		int = 0
)
AS
BEGIN

	DECLARE @LeadSourceList NVARCHAR(MAX),
			@LeadSourceList2 NVARCHAR(MAX),
			@LeadSourceListNull NVARCHAR(MAX)

	DECLARE @ResultID int

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_TourType_Lifecycle_v1', @AccountID)

	SET @ResultID = scope_identity()

	SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + DropdownValue + ']',
			@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + DropdownValue + '], 0) [' + DropdownValue + ']',
			@LeadSourceList2 = COALESCE(@LeadSourceList2 + ' + ', '') + 'isnull([' + DropdownValue + '], 0) '
	FROM	(
				SELECT	distinct DropdownValue
				FROM	dbo.DropdownValues(NOLOCK)
				WHERE	DropdownID = 3
						AND accountid = @AccountID
						AND DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
			) tmp

	PRINT @LeadSourceList
	PRINT @LeadSourceList2
	PRINT @LeadSourceListNull

	DECLARE @SQL nvarchar(MAX)

	IF(@ContactListID > 0)
	BEGIN
		IF (@Type = 0)
		BEGIN
			SET @SQL = '
				 select	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, 0 AS ContactEmailID, count(*) Total
				 from DBO.TOURS(NOLOCK) T
		                    INNER JOIN contacts(NOLOCK) c on T.accountid = c.accountid --and c.contactID = T.ContactID		  				
				            INNER JOIN users(NOLOCK) u on u.accountid IN (T.accountid, 1) and u.userid = T.CreatedBy
				            INNER JOIN dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3							
							--INNER JOIN dropdownvalues co on co.accountid = c.accountid and co.dropdownid = 7
							INNER JOIN dropdownvalues(NOLOCK) tt on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8
							INNER JOIN ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
							LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
							LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
					where	T.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
							and dv.IsActive = 1
							AND c.IsDeleted = 0
							and u.userid = ' + CAST(@ContactListID AS VARCHAR) + '
							and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))
							and dv.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
							AND CONVERT(VARCHAR(10), T.CreatedOn, 101) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 101) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
					GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber
					ORDER BY c.firstname, c.lastname, ce.email
			'
		END
		ELSE
		BEGIN
			SET @SQL = '
				 select	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, 0 AS ContactEmailID, count(*) Total
				 from DBO.TOURS(NOLOCK) T
		                    INNER JOIN contacts(NOLOCK) c on T.accountid = c.accountid --and c.contactID = T.ContactID		  				
				            INNER JOIN users(NOLOCK) u on u.accountid IN (T.accountid, 1) and u.userid = T.CreatedBy
				            INNER JOIN dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3							
							INNER JOIN dropdownvalues(NOLOCK) co on co.accountid = c.accountid and co.dropdownid = 7
							INNER JOIN dropdownvalues(NOLOCK) tt on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8
							INNER JOIN ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
							LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
							LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
					where	T.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
							and dv.IsActive = 1
							AND c.IsDeleted = 0
						--	and u.userid = ' + CAST(@ContactListID AS VARCHAR) + '
							and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))
							and dv.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
							AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
					GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber
					ORDER BY c.firstname, c.lastname, ce.email
				'
		END
	END
	ELSE
	BEGIN
		IF (@Type = 0)
		BEGIN
			SET @SQL = '
				select	u.userid ID, isnull(u.FirstName, '''') + '' '' + isnull(LastName, '''') Name, isnull(Total, 0) Total,
						' + @LeadSourceListNull + '
				from	users u left join (
							select	OwnerID,
									(' + @LeadSourceList2 + ') Total,
									' + @LeadSourceList + '
							from	(
										select	U.USERID OwnerID, dv.DropdownValue LifeCycle,
												count(*) TotalContacts
										from	DBO.TOURS(NOLOCK) T
												inner join contacts(NOLOCK) c on T.accountid = c.accountid --and c.contactID = ocm.ContactID
												inner join users(NOLOCK) u on u.accountid IN (T.accountid, 1) and u.userid = T.CreatedBy
												inner join dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3
												--inner join dropdownvalues co on co.accountid = c.accountid and co.dropdownid = 7
												inner join dropdownvalues(NOLOCK) tt on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8
												inner join ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
										where	T.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
												and dv.IsActive = 1
												AND c.IsDeleted = 0
												and u.userid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
												and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))
												AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
										group by u.UserID, dv.DropdownValue
									) tmp
							pivot(
								MAX(TotalContacts)
								FOR LifeCycle IN (' + @LeadSourceList + ')
							)p
						) tmp on u.userid = tmp.ownerid
				where	u.userid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
				order by Total desc, Name
			'
		END
		ELSE
		BEGIN
			SET @SQL = '
				select	u.userid ID, isnull(u.FirstName, '''') + '' '' + isnull(LastName, '''') Name, isnull(Total, 0) Total,
						' + @LeadSourceListNull + '
				from	users u left join (
							select	OwnerID,
									(' + @LeadSourceList2 + ') Total,
									' + @LeadSourceList + '
							from	(
										select	U.USERID OwnerID, dv.DropdownValue LifeCycle,
												count(*) TotalContacts
										from	DBO.TOURS(NOLOCK) T
												inner join contacts(NOLOCK) c on T.accountid = c.accountid --and c.contactID = ocm.ContactID
												inner join users(NOLOCK) u on u.accountid IN (T.accountid, 1) and u.userid = T.CreatedBy
												inner join dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and c.LifeCycleStage = dv.dropdownvalueid and dv.DropdownID = 3
												inner join dropdownvalues(NOLOCK) co on co.accountid = c.accountid and co.dropdownid = 7
												inner join dropdownvalues(NOLOCK) tt on tt.accountid = c.accountid and t.tourtype = tt.dropdownvalueid and tt.DropdownID = 8
												inner join ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
										where	T.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
												and dv.IsActive = 1
												AND c.IsDeleted = 0
											-- and u.userid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
												and tt.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @TourTypes + ''', '',''))
												AND CONVERT(VARCHAR(10), T.CreatedOn, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
										group by u.UserID, dv.DropdownValue
									) tmp
							pivot(
								MAX(TotalContacts)
								FOR LifeCycle IN (' + @LeadSourceList + ')
							)p
						) tmp on u.userid = tmp.ownerid
				where	u.userid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
				order by Total desc, Name
				'
		END
	END

	PRINT @SQL

	exec sp_executesql @SQL

	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID

END

/*

exec [GET_Account_TourType_Lifecycle_v1]
		@AccountID = 1118,
		@Type = 0,
		@StartDate = '2014-02-11',
		@EndDate = '2014-02-16',
		@SelectedIDs = '2363',
		@TourTypes = '1406',
		@TrafficLifeCycle = '3999'

exec [GET_Account_TourType_Lifecycle_v1]
		@AccountID = 1118,
		@Type = 1,
		@StartDate = '2014-02-11',
		@EndDate = '2015-02-16',
		@SelectedIDs = '2363',
		@TourTypes = '1406',
		@TrafficLifeCycle = '3999'

*/








GO


