CREATE     PROCEDURE [dbo].[Deleting_LeadAdapterAndAccountMap_sp]
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
				DELETE	LATM
				FROM	dbo.LeadAdapterTagMap AS LATM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterTagMapID
							FROM dbo.LeadAdapterTagMap AS LATM WITH (NOLOCK)
							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] AS LAAM ON LATM.LeadAdapterID = LAAM.LeadAdapterAndAccountMapID
							WHERE	LAAM.AccountID = @Accountid 
						) tmp on tmp.LeadAdapterTagMapID = LATM.LeadAdapterTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted LeadAdapterTagMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterTagMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LAJL
				FROM	dbo.LeadAdapterJobLogs AS LAJL  INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterJobLogID
							FROM dbo.LeadAdapterJobLogs AS LAJL WITH (NOLOCK)
							  INNER JOIN dbo.LeadAdapterAndAccountMap AS LAAM ON LAJL.LeadAdapterAndAccountMapID=LAAM.LeadAdapterAndAccountMapID
							WHERE	LAAM.AccountID = @accountid 
						) tmp on tmp.LeadAdapterJobLogID = LAJL.LeadAdapterJobLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted  LeadAdapterJobLogsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterJobLogs'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LATM
				FROM	dbo.WorkflowTriggers AS LATM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
							FROM dbo.WorkflowTriggers AS LATM WITH (NOLOCK)
							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] AS LAAM ON LATM.LeadAdapterID = LAAM.LeadAdapterAndAccountMapID
							WHERE	LAAM.AccountID = @Accountid 
						) tmp on tmp.WorkflowTriggerID = LATM.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted WorkflowTriggers
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LATM
				FROM	dbo.ContactLeadAdapterMap AS LATM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactLeadAdapterMapID
							FROM dbo.ContactLeadAdapterMap AS LATM WITH(NOLOCK)
							  INNER JOIN [dbo].[LeadAdapterAndAccountMap] AS LAAM ON LATM.LeadAdapterID = LAAM.LeadAdapterAndAccountMapID
							WHERE	LAAM.AccountID = @Accountid 
						) tmp on tmp.ContactLeadAdapterMapID = LATM.ContactLeadAdapterMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted ContactLeadAdapterMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactLeadAdapterMap'
			  

			  		BEGIN
				DELETE	LAAAM
				FROM	dbo.LeadAdapterAndAccountMap  AS LAAAM  WITH (NOLOCK)
				WHERE	LAAAM.AccountID = @Accountid 
	          SELECT @@ROWCOUNT LeadAdapterAndAccountMap1COUNT

			END
			PRINT  ' records deleted from  LeadAdapterAndAccountMap'


SELECT @@ROWCOUNT TotalCount
--successfull execution query-- 
SELECT 'DEL-001' ResultCode 

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
	EXEC [dbo].[Deleting_LeadAdapterAndAccountMap_sp]
		@AccountID	= 94

*/



