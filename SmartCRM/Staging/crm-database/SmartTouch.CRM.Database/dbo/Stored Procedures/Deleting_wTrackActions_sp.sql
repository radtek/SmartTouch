CREATE     PROCEDURE [dbo].[Deleting_wTrackActions_sp]
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
				DELETE	RMs
				FROM	[dbo].[RoundRobinContactAssignment] AS RMs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) RoundRobinContactAssignmentID
							FROM  RoundRobinContactAssignment  AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[WorkFlowUserAssignmentAction] AS R ON RM.WorkFlowUserAssignmentActionID=R.WorkFlowUserAssignmentActionID
							  INNER JOIN [dbo].[WorkflowActions] AS T ON T.WorkflowActionID=R.WorkflowActionID
							  INNER JOIN  [dbo].[Workflows] w on w.[WorkflowID] = T.WorkflowID
							WHERE	w.AccountID = @Accountid 
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
							WHERE	w.AccountID = @Accountid 
						) tmp on tmp.WorkFlowUserAssignmentActionID = RMs.WorkFlowUserAssignmentActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted WorkFlowUserAssignmentAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  WorkFlowUserAssignmentAction'



	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	[Workflow].[TrackActionLogs]  AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionLogID
							FROM  [Workflow].[TrackActionLogs]  AS RM WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackActions] AS R ON RM.TrackActionID=R.TrackActionID
							  INNER JOIN Actions  w on w.[ActionID] = r.ActionID
							WHERE	w.AccountID = @Accountid 
						) tmp on tmp.TrackActionLogID = RM.TrackActionLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted TrackActionLogs
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  TrackActionLogs'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	 [Workflow].[TerminatedActions] AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TerminatedActionID
							FROM  [Workflow].[TerminatedActions]  AS RM WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackActions] AS R ON RM.TrackActionID=R.TrackActionID
							   INNER JOIN Actions  w on w.[ActionID] = r.ActionID
							WHERE	w.AccountID = @Accountid 
						) tmp on tmp.TerminatedActionID = RM.TerminatedActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted TerminatedActions
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  TerminatedActions'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	[Workflow].[TrackActionLogs]  AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) [TrackActionLogID]
							FROM  [Workflow].[TrackActionLogs]  AS RM WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackActions] AS R ON RM.TrackActionID=R.TrackActionID
							   INNER JOIN Actions  w on w.[ActionID] = r.ActionID
							WHERE	w.AccountID = @Accountid 
						) tmp on tmp.TrackActionLogID = RM.TrackActionLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted TerminatedActions
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  TerminatedActions'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	 [Workflow].[TrackActionLogs] AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionLogID
							FROM  [Workflow].[TrackActionLogs]  AS RM WITH (NOLOCK)
							  INNER JOIN [Workflow].[TrackActions] AS R ON RM.TrackActionID=R.TrackActionID
							  INNER JOIN [Workflow].[TrackMessages] T ON T.TrackMessageID = R.TrackMessageID
							WHERE	T.AccountID = @Accountid 
						) tmp on tmp.TrackActionLogID = RM.TrackActionLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted TerminatedActions
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  TerminatedActions_TrackMessages_TrackActions'

		BEGIN
				DELETE	Uw
				FROM	[Workflow].[TrackActions] as uw 
				where ActionID in (select ActionID from Actions WHERE  AccountID = @Accountid)
				select @@rowcount UsersCOUNT
			END
			PRINT  ' records deleted from  Users'
			
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
	EXEC [dbo].[Deleting_wTrackActions_sp]
		@AccountID	= 94

*/



