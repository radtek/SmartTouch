
CREATE procedure [dbo].[GET_Account_Traffic_LifeCycle_v1](
	@AccountID			int,
	@Type				tinyint,
	@StartDate          datetime,
	@EndDate            datetime,
	@SelectedIDs		VARCHAR(MAX),
	@TrafficLifeCycle	VARCHAR(MAX),
	@ContactListID		int = 0
)
AS
BEGIN

	DECLARE @LeadSourceList NVARCHAR(MAX),
			@LeadSourceList2 NVARCHAR(MAX),
			@LeadSourceListNull NVARCHAR(MAX),
			@ColumnList NVARCHAR(MAX)

	DECLARE @ResultID int

	INSERT INTO StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_Traffic_LifeCycle_v1', @AccountID)

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

	PRINT '@LeadSourceList: ' + isnull(@LeadSourceList, '')
	PRINT '@LeadSourceList2: ' + isnull(@LeadSourceList2, '')
	PRINT '@LeadSourceListNull: ' + isnull(@LeadSourceListNull, '')

	DECLARE @SQL nvarchar(MAX)
	IF(@ContactListID > 0)
	BEGIN
			IF(@Type = 1)
		BEGIN
			SET @SQL = '
					SELECT	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, 0 AS ContactEmailID, count(*) Total
					FROM	DropdownValues(NOLOCK) dv INNER JOIN Tours t ON t.CommunityID = dv.DropdownValueID
							INNER JOIN ContactTourMap(NOLOCK) ctm ON ctm.TourID = t.TourID
							INNER JOIN users(NOLOCK) u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
							INNER JOIN contacts(NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
							INNER JOIN DBO.DropdownValues(NOLOCK) DV2 ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
							LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
							LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
					WHERE	dv.accountid = ' + CAST(@AccountID AS VARCHAR) + '
							AND c.IsDeleted = 0
							AND dv.DropdownID = 7
							AND dv2.DropdownID = 3
							AND dv.IsActive = 1
							AND dv.DropdownValueID = ' + CAST(@ContactListID AS VARCHAR) + '
							AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
							AND CONVERT(VARCHAR(10), t.TourDate, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
					GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber
					ORDER BY c.firstname, c.lastname, ce.email
				'
		END
		ELSE
		BEGIN
			SET @SQL = '
					SELECT	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, 0 AS ContactEmailID, count(*) Total
					FROM	DropdownValues(NOLOCK) dv INNER JOIN Tours t ON t.CommunityID = dv.DropdownValueID
							INNER JOIN ContactTourMap(NOLOCK) ctm ON ctm.TourID = t.TourID
							INNER JOIN users(NOLOCK) u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
							INNER JOIN contacts(NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
							INNER JOIN DBO.DropdownValues(NOLOCK) DV2 ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
							LEFT JOIN ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
							LEFT JOIN ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
					WHERE	dv.accountid = ' + CAST(@AccountID AS VARCHAR) + '
							AND c.IsDeleted = 0
							AND dv.DropdownID = 7
							AND dv2.DropdownID = 3
							AND dv.IsActive = 1
							AND u.UserID = ' + CAST(@ContactListID AS VARCHAR) + '
							AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
							AND CONVERT(VARCHAR(10), t.TourDate, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
					GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber
					ORDER BY c.firstname, c.lastname, ce.email
				'
		END
	END
	ELSE
	BEGIN
		IF(@Type = 1)
		BEGIN
			SET @SQL = '
						SELECT	CAST(dv.DropdownValueID as integer) ID, dv.DropdownValue Name, isnull(Total, 0) Total,
								' + @LeadSourceListNull + '
						FROM	DBO.DropdownValues(NOLOCK) dv LEFT JOIN (
									SELECT	CommunityID,
											(' + @LeadSourceList2 + ') Total,
											' + @LeadSourceList + '
									FROM	(
												SELECT	CommunityID, DropdownValue LeadSourceName, count(*) TotalContacts
												FROM	(
															SELECT	dv.DropdownValueID CommunityID, DV2.DropdownValue, c.ContactID
															FROM	DropdownValues(NOLOCK) dv INNER JOIN Tours t ON t.CommunityID = dv.DropdownValueID
																	INNER JOIN ContactTourMap(NOLOCK) ctm ON ctm.TourID = t.TourID
																	INNER JOIN users(NOLOCK) u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
																	INNER JOIN contacts(NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
																	INNER JOIN DBO.DropdownValues(NOLOCK) DV2 ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
															WHERE	dv.accountid = ' + CAST(@AccountID AS VARCHAR) + '
																	AND c.IsDeleted = 0
																	AND dv.DropdownID = 7
																	AND dv2.DropdownID = 3
																	AND dv.IsActive = 1
																	AND dv.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
																	AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
																	AND CONVERT(VARCHAR(10), t.TourDate, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
														) tmp
												GROUP BY CommunityID, DropdownValue			
											) tmp
									PIVOT(
										MAX(TotalContacts)
										FOR LeadSourceName IN (' + @LeadSourceList + ')
									)p
							) tmp ON tmp.CommunityID = dv.DropdownValueID and dv.DropdownID = 7	
						WHERE	dv.accountID = ' + CAST(@AccountID AS VARCHAR) + '
								AND dv.DropdownID = 7
								AND isnull(IsActive, 1) = 1
								AND dv.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
						ORDER BY Total desc, dv.DropdownValue
				'
		END
		ELSE
		BEGIN
			SET @SQL = '
						SELECT	u.UserID ID, ISNULL(FirstName, '''') + '' '' + ISNULL(LastName, '''') Name, isnull(Total, 0) Total,
								' + @LeadSourceListNull + '
						FROM	DBO.users(NOLOCK) u LEFT JOIN (
									SELECT	UserID,
											(' + @LeadSourceList2 + ')  Total,
											' + @LeadSourceList + '
									FROM	(
												SELECT	UserID, DropdownValue LeadSourceName, count(*) TotalContacts
												FROM	(
															SELECT	u.userID, DV2.DropdownValue, c.ContactID
															FROM	DropdownValues(NOLOCK) dv INNER JOIN Tours t ON t.CommunityID = dv.DropdownValueID
																	INNER JOIN ContactTourMap(NOLOCK) ctm ON ctm.TourID = t.TourID
																	INNER JOIN users(NOLOCK) u on u.accountid IN (dv.accountid, 1) and u.UserID = t.CreatedBy
																	INNER JOIN contacts(NOLOCK) c on c.AccountID = dv.AccountID and c.contactid = ctm.ContactID
																	INNER JOIN DBO.DropdownValues(NOLOCK) DV2 ON U.AccountID = DV2.AccountID AND c.LifeCycleStage = dv2.DropdownValueID
															WHERE	dv.accountid = ' + CAST(@AccountID AS VARCHAR) + '
																	AND c.IsDeleted = 0
																	AND dv.DropdownID = 7
																	AND dv2.DropdownID = 3
																	AND dv.IsActive = 1
																	AND u.UserID IN (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
																	AND dv2.DropdownValueID IN (SELECT DataValue FROM dbo.Split(''' + @TrafficLifeCycle + ''', '',''))
																	AND CONVERT(VARCHAR(10), t.TourDate, 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
														) tmp
												GROUP BY UserID, DropdownValue			
											) tmp
									PIVOT(
										MAX(TotalContacts)
										FOR LeadSourceName IN (' + @LeadSourceList + ')
									)p
							) tmp ON tmp.UserID = u.userId
						WHERE	u.accountID = ' + CAST(@AccountID AS VARCHAR) + '
								AND u.UserID IN (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
						ORDER BY Total desc, Name
				'
		END
	END
	PRINT @SQL
	EXEC sp_executesql @SQL

	UPDATE	StoreProcExecutionResults
	SET		EndTime = GETDATE(),
			TotalTime = (CAST(GETDATE()-StartTime AS float) * 60 * 60 * 24),
			Status = 'C'
	WHERE	ResultID = @ResultID

END

/*

exec GET_Account_Traffic_LifeCycle_v1
		@AccountID = 45,
		@Type = 0,
		@StartDate = '2014-07-01',
		@EndDate = '2014-11-30',
		@SelectedIDs = '7,9,11,38,52,68,73,74,75,76,80,1094,1145,1151,1152,1157,1159,1167,1168,1186,1190,1207,5290,5301,5316,5319,5320,5327',
		@TrafficLifeCycle = '49,50,51,64,65,2420'


exec GET_Account_Traffic_LifeCycle_v1
		@AccountID = 45,
		@Type = 1,
		@StartDate = '2014-07-01',
		@EndDate = '2014-11-30',
		@SelectedIDs = '5,6,7,406,412',
		@TrafficLifeCycle = '49,50,51,64,65,2420'


*/








GO


