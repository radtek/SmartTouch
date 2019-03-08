CREATE    PROCEDURE [dbo].[Deleting_SearchDefinitions_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY
    BEGIN TRANSACTION

DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WT
				FROM	dbo.WorkflowTriggers AS WT INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
							FROM dbo.WorkflowTriggers AS WT (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON WT.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SF
				FROM	dbo.SearchFilters AS SF INNER JOIN(
							SELECT TOP (@RecordPerBatch) SearchFilterID
							FROM dbo.SearchFilters AS SF (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON SF.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.SearchFilterID = SF.SearchFilterID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SearchFiltersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchFilters'


				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SDTM
				FROM	dbo.SearchDefinitionTagMap AS SDTM INNER JOIN(
							SELECT TOP (@RecordPerBatch) SearchDefinitionTagMapID
							FROM dbo.SearchDefinitionTagMap AS SDTM (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON SDTM.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.SearchDefinitionTagMapID = SDTM.SearchDefinitionTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SearchDefinitionTagMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchDefinitionTagMap'


		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SDSM
				FROM	dbo.SearchDefinitionSubscriptionMap AS SDSM INNER JOIN(
							SELECT TOP (@RecordPerBatch) SearchDefinitionSubscriptionMapID
							FROM dbo.SearchDefinitionSubscriptionMap AS SDSM (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON SDSM.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.SearchDefinitionSubscriptionMapID = SDSM.SearchDefinitionSubscriptionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SearchDefinitionSubscriptionMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchDefinitionSubscriptionMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CSDM
				FROM	dbo.CampaignSearchDefinitionMap AS CSDM INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignSearchDefinitionMapID
							FROM dbo.CampaignSearchDefinitionMap AS CSDM (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON CSDM.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.CampaignSearchDefinitionMapID = CSDM.CampaignSearchDefinitionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignSearchDefinitionMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignSearchDefinitionMap'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	BO
				FROM	dbo.BulkOperations AS BO INNER JOIN(
							SELECT TOP (@RecordPerBatch) BulkOperationID
							FROM dbo.BulkOperations AS b (NOLOCK)
							  INNER JOIN dbo.SearchDefinitions AS SD ON b.SearchDefinitionID=SD.SearchDefinitionID 
							WHERE	SD.AccountID = @accountid 
						) tmp on tmp.BulkOperationID = BO.BulkOperationID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT BulkOperationsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from BulkOperations'

			BEGIN
				DELETE	SD
				FROM	dbo.SearchDefinitions  AS SD  (NOLOCK)
				WHERE	SD.AccountID = @Accountid 
	           select @@rowcount SearchDefinitionsCOUNT
			END
			PRINT  ' records deleted from  SearchDefinitions'


--successfull execution query-- 
SELECT 'DEL-001' ResultCode 
SELECT @@ROWCOUNT TotalCount
 Commit TRANSACTION 
	END TRY

BEGIN CATCH
	ROLLBACK TRANSACTION
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_SearchDefinitions_sp]
		@AccountID	= 19

*/

/*
	SELECT COUNT(*) FROM FormTags WITH (NOLOCK) WHERE FormID IN (SELECT FormID FROM Forms WHERE Accountid = 22)
	SELECT COUNT(*) FROM SearchDefinitions WITH (NOLOCK) where Accountid = 22
	*/

