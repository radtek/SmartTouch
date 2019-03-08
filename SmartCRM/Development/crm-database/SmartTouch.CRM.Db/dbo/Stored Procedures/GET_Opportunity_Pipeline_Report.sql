
CREATE PROCEDURE [dbo].[GET_Opportunity_Pipeline_Report]
	 @AccountID INT,
	 @FromDate DATETIME,
	 @ToDate DATETIME,
	 @AccountExecutives VARCHAR(MAX),
	 @OpportunityStages VARCHAR(MAX),
	 @OwnerID INT= 0
AS
BEGIN
	BEGIN TRY 
	
	DECLARE @SQL VARCHAR(MAX)
	DECLARE @Stages VARCHAR(MAX)
	DECLARE @StagesTotal VARCHAR(MAX)
	
	SELECT @Stages = COALESCE(@Stages+',','') + '['+DropDownValue+']'  FROM DropDownValues (NOLOCK) DDV
	INNER JOIN OpportunityStageGroups  OSG (NOLOCK) ON OSG.DropdownValueID = DDV.DropdownValueID
	WHERE DDV.AccountID = @AccountID AND DropdownID = 6 AND ISActive = 1 AND DDV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@OpportunityStages,','))
	ORDER BY OSG.OpportunityGroupID ASC, DDV.SortID ASC

	SELECT @StagesTotal = COALESCE(@StagesTotal+'+','') + '['+DropDownValue+']' FROM DropDownValues (NOLOCK) DDV
	INNER JOIN OpportunityStageGroups  OSG (NOLOCK) ON OSG.DropdownValueID = DDV.DropdownValueID
	WHERE DDV.AccountID = @AccountID AND DropdownID = 6 AND ISActive = 1 AND DDV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@OpportunityStages,','))
	ORDER BY OSG.OpportunityGroupID ASC, DDV.SortID ASC


	IF @OwnerID = 0
	BEGIN

		-- Pass IsAdmin = 0 always.
		
		--EXEC [dbo].[GET_Account_Opportunity_Pipeline_FunnelChart] @accountId, @FromDate, @ToDate, 0, @AccountExecutives

		SELECT DDV.DropdownValue,U.FirstName +' ' + U.LastName as AccountExecutive, OCM.StageID , OCM.Owner--, SUM(O.Potential) as Potential--,
		INTO #temp_OpportunityReport
		FROM DropdownValues (NOLOCK) DDV
		INNER JOIN Opportunities O (NOLOCK) ON O.AccountID = DDV.AccountID
		INNER JOIN OpportunityContactMap OCM (NOLOCK)  ON OCM.StageID = DDV.DropdownValueID AND O.OpportunityID = OCM.OpportunityID
		INNER JOIN Users (NOLOCK) U ON OCM.[Owner] = U.UserID 
		WHERE DDV.AccountID = @AccountID 
				AND DDV.DropdownID = 6 
				AND DDV.IsActive = 1
				AND U.UserID IN (SELECT DataValue FROM dbo.Split(@AccountExecutives,','))
				AND CONVERT(VARCHAR(10), OCM.ExpectedToClose, 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120)
				AND DDV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@OpportunityStages,','))
				AND OCM.IsDeleted = 0
		
		SELECT @SQL = 
		'SELECT ID, Name ,' + @StagesTotal + ' AS Total ,  '+@Stages+' FROM
		(
			SELECT U.UserID as ID, U.FirstName+ '' '' +  U.LastName as Name, t.DropDownValue, t.StageID
			FROM #temp_OpportunityReport t
			RIGHT JOIN Users (NOLOCK) U ON U.UserID = t.Owner 
			WHERE  U.UserID IN ('+@AccountExecutives+')
		) src '+
		'PIVOT
		(
			COUNT(StageID)
			FOR DropdownValue IN ('+@Stages+ ')
		) piv'

		PRINT @SQL
		EXECUTE (@SQL)
	END
	ELSE
		BEGIN
			SELECT DISTINCT OCM.ContactID as ContactID, 0 AS ContactEmailID
			FROM DropdownValues (NOLOCK) DDV
			INNER JOIN Opportunities O (NOLOCK) ON O.AccountID = DDV.AccountID
			INNER JOIN OpportunityContactMap OCM (NOLOCK)  ON OCM.StageID = DDV.DropdownValueID AND O.OpportunityID = OCM.OpportunityID
			INNER JOIN Users (NOLOCK) U ON OCM.[Owner] = U.UserID
			WHERE DDV.AccountID = @AccountID AND DDV.DropdownID = 6 AND DDV.IsActive = 1
			AND CONVERT(VARCHAR(10), OCM.ExpectedToClose, 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120)
			AND OCM.IsDeleted = 0
			AND U.UserID = @OwnerID AND DDV.DropdownValueID IN (SELECT DataValue FROM dbo.Split(@OpportunityStages,','))
		END
	
END TRY
	BEGIN CATCH
		SELECT 'SEL-002' ResultCode 

		INSERT INTO dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

	END CATCH
	SET NOCOUNT OFF

END
/*
declare @fromdate datetime
declare @todate datetime
set @fromdate = '2/2/2017 6:24:33 PM'
set @todate = '3/4/2017 6:24:33 PM'
	exec [dbo].[GET_Opportunity_Pipeline_Report] 4218,@fromdate, @todate, '6889,6868,6876,6877,6878,6880,6882,6883,6885,6886,6887,6888','4669,4670,4671,4681,4682,5126'
*/

