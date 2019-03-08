
CREATE PROC [dbo].[GET_Account_Opportunity_Pipeline_FunnelChart]
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

	IF (@IsAdmin = 1)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,DV.DropdownValueID, COUNT(OC.ContactID) TotalCount 
				FROM dbo.DropdownValues(NOLOCK) DV
					INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID
					LEFT JOIN dbo.Opportunities(NOLOCK) O ON O.StageID = DV.DropdownValueID AND O.AccountID = @AccountID 
						AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120)
						LEFT JOIN (select OCM.ContactID,OCM.OpportunityID from dbo.OpportunityContactMap(NOLOCK)  OCM
                    INNER JOIN dbo.Contacts(NOLOCK) C ON OCM.ContactID = C.ContactID AND C.IsDeleted = 0) OC ON O.OpportunityID = OC.OpportunityID
			WHERE (O.IsDeleted = 0 OR O.IsDeleted IS NULL) AND DV.AccountID = @AccountID AND DV.DropdownID = 6 AND DV.IsActive = 1		
			GROUP BY DV.DropdownValue, O.StageID, DV.DropdownValue, OSG.OpportunityGroupID, DV.SortID,DV.DropdownValueID
			ORDER BY OSG.OpportunityGroupID ASC, DV.SortID ASC
		END
	ELSE IF (@IsAdmin = 0)
		BEGIN
			/* Default 30 Days */
			SELECT DV.DropdownValue,DV.DropdownValueID, COUNT(OC.ContactID) TotalCount 
				FROM dbo.DropdownValues(NOLOCK) DV 
					INNER JOIN dbo.OpportunityStageGroups(NOLOCK) OSG ON DV.DropdownValueID = OSG.DropdownValueID AND DV.AccountID = OSG.AccountID
					LEFT JOIN dbo.Opportunities(NOLOCK) O ON O.StageID = DV.DropdownValueID AND O.AccountID = @AccountID AND O.[Owner] = @OwnerID
						AND CONVERT(VARCHAR(10), ISNULL(O.LastModifiedOn, O.CreatedOn), 120) BETWEEN CONVERT(VARCHAR(10), @FromDate, 120) AND  CONVERT(VARCHAR(10), @ToDate, 120)
				LEFT JOIN (select OCM.ContactID,OCM.OpportunityID from dbo.OpportunityContactMap(NOLOCK)  OCM
                    INNER JOIN dbo.Contacts(NOLOCK) C ON OCM.ContactID = C.ContactID AND C.IsDeleted = 0) OC ON O.OpportunityID = OC.OpportunityID
			WHERE (O.IsDeleted = 0 OR O.IsDeleted IS NULL) AND DV.AccountID = @AccountID AND DV.DropdownID = 6 AND DV.IsActive = 1			
			GROUP BY DV.DropdownValue, O.StageID, DV.DropdownValue, OSG.OpportunityGroupID, DV.SortID,DV.DropdownValueID
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
		 @AccountID			= 45,
		 @FromDate          = '2014-12-18 00:00:00.000',       
		 @ToDate            = '2015-1-20 00:00:00.000',
		 @IsAdmin			= 1,
		 @OwnerID			= 100

*/


GO


