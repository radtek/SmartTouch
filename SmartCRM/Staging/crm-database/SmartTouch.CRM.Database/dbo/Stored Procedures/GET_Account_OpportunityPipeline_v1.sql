
CREATE PROCEDURE [dbo].[GET_Account_OpportunityPipeline_v1](
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

	DECLARE @LeadSourceList			NVARCHAR(MAX),
			@LeadSourceList2		NVARCHAR(MAX),
			@LeadSourceListNull		NVARCHAR(MAX),
			@ResultID				int,
			@SQL					NVARCHAR(MAX)

	INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID)
	VALUES('GET_Account_OpportunityPipeline_v1', @AccountID)

	SET @ResultID = scope_identity()

	SELECT	@LeadSourceList = COALESCE(@LeadSourceList + ', ', '') + '[' + DropdownValue + ']',
			@LeadSourceListNull = COALESCE(@LeadSourceListNull + ', ', '') + 'isnull([' + DropdownValue + '], 0) [' + DropdownValue + ']',
			@LeadSourceList2 = COALESCE(@LeadSourceList2 + ' + ', '') + 'isnull([' + DropdownValue + '], 0) '
	FROM	(
				SELECT	DISTINCT dv.DropdownValue,dv.SortID,os.OpportunityGroupID
				FROM	dbo.DropdownValues(NOLOCK) dv
				        inner join dbo.OpportunityStageGroups(NOLOCK) os on os.DropdownValueID = dv.DropdownValueID
                        
				WHERE	dv.AccountID = @AccountID AND dv.IsActive = 1 and dv.DropdownID = 6 
						AND DV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@TrafficLifeCycle, ','))
						--ORDER BY os.OpportunityGroupID asc, dv.SortID asc
			) tmp ORDER BY OpportunityGroupID asc, SortID asc

	PRINT @LeadSourceList
	PRINT @LeadSourceList2
	PRINT @LeadSourceListNull

	IF(@ContactListID > 0)
	BEGIN
		IF (@Type = 0)
		BEGIN
			SET @SQL = '
				SELECT	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, count(*) Total  
				FROM	dbo.Opportunities(NOLOCK) o 
				        inner join opportunityContactMap(NOLOCK) ocm on o.OpportunityID = ocm.OpportunityID
				        inner join dbo.contacts(NOLOCK) c on o.accountid = c.accountid and c.contactID = ocm.ContactID
						inner join dbo.users(NOLOCK) u on u.accountid IN (o.accountid, 1) and u.userid = o.owner
						inner join dbo.dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and o.StageID = dv.dropdownvalueid and dv.DropdownID = 6 AND DV.IsActive = 1
						INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID
						LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND CP.IsPrimary = 1
						LEFT JOIN dbo.ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
				where	o.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
						AND c.IsDeleted = 0
						and dv.IsActive = 1
						AND DV.DropdownValueID IN (SELECT DataValue FROM dbo.Split('''+@TrafficLifeCycle+''', '',''))
						and u.userid = ' + CAST(@ContactListID AS VARCHAR) + '
						AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
				GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, OSG.OpportunityGroupID, DV.SortID
				ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC --, c.firstname, c.lastname, ce.email,
			'
		END
		ELSE
		BEGIN
			SET @SQL = '
				select	c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, count(*) Total  
				from	dbo.Opportunities(NOLOCK) o 
				        inner join opportunityContactMap(NOLOCK) ocm on o.OpportunityID = ocm.OpportunityID
				        inner join dbo.contacts(NOLOCK) c on o.accountid = c.accountid and c.contactID = ocm.ContactID
						inner join dbo.users(NOLOCK) u on u.accountid IN (o.accountid, 1) and u.userid = o.owner
						inner join dbo.dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and o.StageID = dv.dropdownvalueid and dv.DropdownID = 6 AND DV.IsActive = 1
						INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID 
						inner join dbo.dropdownvalues(NOLOCK) co on co.accountid = c.accountid and co.dropdownid = 7
						inner join dbo.tours(NOLOCK) t on t.communityid = co.dropdownvalueid
						inner join dbo.ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
						LEFT JOIN dbo.ContactPhoneNumbers(NOLOCK) cp on cp.AccountID = c.AccountID AND cp.contactid = c.contactid AND IsPrimary = 1
						LEFT JOIN dbo.ContactEmails(NOLOCK) ce on ce.AccountID = c.AccountID AND ce.contactid = c.contactid AND ce.IsPrimary = 1
				where	o.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
						AND c.IsDeleted = 0
						and dv.IsActive = 1
						AND DV.DropdownValueID IN (SELECT DataValue FROM dbo.Split('''+@TrafficLifeCycle+''', '',''))
						and u.userid = ' + CAST(@ContactListID AS VARCHAR) + '
						AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
				GROUP BY c.contactID, c.firstname, c.lastname, ce.email, cp.phonenumber, OSG.OpportunityGroupID, DV.SortID 
				ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC  --, c.firstname, c.lastname, ce.email, 
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
										select	o.Owner OwnerID, dv.DropdownValue LifeCycle,
												count(*) TotalContacts
										from	dbo.Opportunities(NOLOCK) o 
										        inner join opportunityContactMap(NOLOCK) ocm on o.OpportunityID = ocm.OpportunityID
												inner join dbo.contacts(NOLOCK) c on o.accountid = c.accountid and c.contactID = ocm.ContactID
												inner join dbo.users(NOLOCK) u on  u.accountid IN (o.accountid, 1) and u.userid = o.owner
												inner join dbo.dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and o.StageID = dv.dropdownvalueid and dv.DropdownID = 6
												INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID AND DV.IsActive = 1
										where	o.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
												AND c.IsDeleted = 0
												and dv.IsActive = 1
												and u.userid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
												AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
										group by o.owner, dv.DropdownValue, OSG.OpportunityGroupID, DV.SortID
										--ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC

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
				select	CAST(dv.dropdownvalueID as integer) ID, dv.dropdownvalue Name, isnull(Total, 0) Total,
						' + @LeadSourceListNull + '
				from	dropdownvalues dv left join (
							select	CommunityId,
									(' + @LeadSourceList2 + ') Total,
									' + @LeadSourceList + '
							from	(
										select	co.dropdownvalueid CommunityId, dv.DropdownValue LifeCycle,
												count(*) TotalContacts
										from	dbo.Opportunities(NOLOCK) o 
										        inner join opportunityContactMap(NOLOCK) ocm on o.OpportunityID = ocm.OpportunityID
												inner join dbo.contacts(NOLOCK) c on o.accountid = c.accountid and c.contactID = ocm.ContactID
												inner join dbo.users(NOLOCK) u on u.accountid IN (o.accountid, 1) and u.userid = o.owner
												inner join dbo.dropdownvalues(NOLOCK) dv on dv.accountid = c.accountid and o.StageID = dv.dropdownvalueid and dv.DropdownID = 6
												INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID AND DV.IsActive = 1
												inner join dbo.dropdownvalues(NOLOCK) co on co.accountid = c.accountid and co.dropdownid = 7
												inner join dbo.tours(NOLOCK) t on t.communityid = co.dropdownvalueid
												inner join dbo.ContactTourMap(NOLOCK) ctm on ctm.tourid = t.tourid and c.contactid = ctm.contactid
										where	o.AccountID = ' + CAST(@AccountID AS VARCHAR) + '
												AND c.IsDeleted = 0
												and dv.IsActive = 1
												and co.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
												AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN ''' + CONVERT(varchar(10), @StartDate, 120) + ''' AND ''' + CONVERT(varchar(10), @EndDate, 120) + '''
										group by co.dropdownvalueid, dv.DropdownValue, OSG.OpportunityGroupID, DV.SortID	
										--ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC									
									) tmp
							pivot(
								MAX(TotalContacts)
								FOR LifeCycle IN (' + @LeadSourceList + ')
							)p
						) tmp on dv.dropdownvalueid = tmp.CommunityId
					where	dv.accountid = ' + CAST(@AccountID AS VARCHAR) + '
							and dv.dropdownvalueid in (SELECT DataValue FROM dbo.Split(''' + @SelectedIDs + ''', '',''))
					order by Total desc, ID  
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

exec GET_Account_OpportunityPipeline_v1
		@AccountID = 4218,
		@Type = 0,
		@StartDate = '2014-07-01',
		@EndDate = '2015-02-10',
		@SelectedIDs = '6868,6876,6877,6878,6880,6882,6883,6885,6886,6887,6888',
		@TrafficLifeCycle = '4669,4670,4671,4681,4682,5126'

exec GET_Account_OpportunityPipeline_v1
		@AccountID = 4218,
		@Type = 1,
		@StartDate = '2014-07-01',
		@EndDate = '2015-02-10',
		@SelectedIDs = '6868,6876,6877,6878,6880,6882,6883,6885,6886,6887,6888',
		@TrafficLifeCycle = '4669,4670,4671,4681,4682,5126'

		exec GET_Account_OpportunityPipeline_v1
		@AccountID = 4218,
		@Type = 0,
		@StartDate = '2014-07-01',
		@EndDate = '2015-02-10',
		@SelectedIDs = '6868,6876,6877,6878,6880,6882,6883,6885,6886,6887,6888',
		@TrafficLifeCycle = '4669,4670,4671,4681,4682,5126',
		@ContactListID	=4671

*/