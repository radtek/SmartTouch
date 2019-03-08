CREATE     PROCEDURE [dbo].[Deleting_WTrackMessages_sp]
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
				DELETE	WWTA
				FROM	Workflow.TrackActions AS WWTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionID
							FROM Workflow.TrackActions AS WWTA  WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackMessages]  AS WWTM ON WWTA.TrackMessageID = WWTM.TrackMessageID  
							WHERE	WWTM.AccountID = @accountid 
						) tmp on tmp.TrackActionID = WWTA.TrackActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted as WorkflowTrackActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackActions]_[Workflow].[TrackMessages]'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WWTA
				FROM	Workflow.TrackMessageLogs AS WWTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackMessageLogID
							FROM Workflow.TrackMessageLogs AS WWTA  WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackMessages]  AS WWTM ON WWTA.TrackMessageID = WWTM.TrackMessageID  
							WHERE	WWTM.AccountID = @accountid 
						) tmp on tmp. TrackMessageLogID= WWTA.TrackMessageLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted AS  WorkflowTrackMessageLogs
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackMessageLogs]_[Workflow].[TrackMessages]'


				BEGIN
				DELETE	WT
				FROM	[Workflow].[TrackMessages]  AS  WT  WITH (NOLOCK)
				WHERE	WT.AccountID = @Accountid
	          SELECT @@ROWCOUNT WorkflowTrackMessages
			END
			PRINT ' records deleted from  WorkflowTrackMessages'
			   
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
	EXEC [dbo].[Deleting_WTrackMessages_sp]
		@AccountID	= 19

*/



