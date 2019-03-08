CREATE     PROCEDURE [dbo].[Deleting_RoundRobinContactAssignment_sp]
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
				delete 	RMs
				FROM	[dbo].[RoundRobinContactAssignment] AS RMs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) RoundRobinContactAssignmentID
							FROM  RoundRobinContactAssignment  AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[WorkFlowUserAssignmentAction] AS R ON RM.WorkFlowUserAssignmentActionID=R.WorkFlowUserAssignmentActionID
							  INNER JOIN [dbo].[WorkflowActions] AS T ON T.WorkflowActionID=R.WorkflowActionID
							  INNER JOIN  [dbo].[Workflows] w on w.[WorkflowID] = T.WorkflowID
							WHERE	w.AccountID = @AccountID
						) tmp on tmp.RoundRobinContactAssignmentID = RMs.RoundRobinContactAssignmentID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted RoundRobinContactAssignment
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  RoundRobinContactAssignment'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RMs
				FROM	[dbo].[WorkFlowUserAssignmentAction]  AS RMs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowUserAssignmentActionID
							FROM  WorkFlowUserAssignmentAction  AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS R ON RM.WorkflowActionID=R.WorkflowActionID
							  INNER JOIN  [dbo].[Workflows] w on w.[WorkflowID] = r.WorkflowID
							WHERE	w.AccountID =@AccountID
						) tmp on tmp.WorkFlowUserAssignmentActionID = RMs.WorkFlowUserAssignmentActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted WorkFlowUserAssignmentAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  WorkFlowUserAssignmentAction'
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
	EXEC [dbo].[Deleting_Deleting_RoundRobinContactAssignment_sp_sp]
		@AccountID	= 117

*/