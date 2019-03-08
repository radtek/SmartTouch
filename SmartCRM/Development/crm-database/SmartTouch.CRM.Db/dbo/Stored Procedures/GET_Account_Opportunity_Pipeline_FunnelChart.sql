
CREATE PROC [dbo].[GET_Account_Opportunity_Pipeline_FunnelChart]
(
	@AccountID		int,
		@FromDate       datetime,
		@ToDate         datetime,
		@IsAdmin		tinyint ,
		@OwnerID		VARCHAR(MAX)
)
AS 
BEGIN
	SET NOCOUNT ON
	BEGIN TRY 

	DECLARE @SelectedAETable TABLE (UserId INT)

	IF (@IsAdmin = 1)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,DV.DropdownValueID,  COUNT(OCM.ContactID) TotalCount ,SUM(ISNULL(OCM.Potential,0)) as Potential
				FROM dbo.DropdownValues(NOLOCK) DV
					INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID
					LEFT JOIN dbo.Opportunities (NOLOCK) OC ON OC.AccountID = DV.AccountID
					LEFT JOIN dbo.OpportunityContactMap (NOLOCK) OCM ON OCM.OpportunityID = OC.OpportunityID AND DV.DropdownValueID = OCM.StageID
					AND CONVERT(VARCHAR(10), OCM.ExpectedToClose, 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120) 
					AND OCM.IsDeleted = 0
					LEFT JOIN dbo.Contacts C (NOLOCK) ON C.ContactID = OCM.ContactID AND C.AccountID = @AccountID
			WHERE DV.AccountID = @AccountID AND DV.DropdownID = 6 AND DV.IsActive = 1 
			GROUP BY DV.DropdownValue,   OSG.OpportunityGroupID, DV.SortID,DV.DropdownValueID
			ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,DV.DropdownValueID,  COUNT(OCM.ContactID) TotalCount ,SUM(ISNULL(OCM.Potential,0)) as Potential
				FROM dbo.DropdownValues(NOLOCK) DV
					INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID
					LEFT JOIN dbo.Opportunities (NOLOCK) OC ON OC.AccountID = DV.AccountID
					LEFT JOIN dbo.OpportunityContactMap (NOLOCK) OCM ON OCM.OpportunityID = OC.OpportunityID AND DV.DropdownValueID = OCM.StageID 
					AND OCM.Owner IN (SELECT DATAVALUE FROM dbo.Split(@OwnerID,',')) AND OCM.IsDeleted = 0
					AND CONVERT(VARCHAR(10), OCM.ExpectedToClose, 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120)
					LEFT JOIN dbo.Contacts C (NOLOCK) ON C.ContactID = OCM.ContactID AND C.AccountID = @AccountID
			WHERE DV.AccountID = @AccountID AND DV.DropdownID = 6 AND DV.IsActive = 1 
			GROUP BY DV.DropdownValue,   OSG.OpportunityGroupID, DV.SortID,DV.DropdownValueID
			ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC
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
	  EXEC [dbo].[GET_Account_Opportunity_Pipeline_FunnelChart]
		 @AccountID			= 4218,
		 @FromDate          = '2017-01-01 00:00:00.000',       
		 @ToDate            = '2017-02-01 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100

*/