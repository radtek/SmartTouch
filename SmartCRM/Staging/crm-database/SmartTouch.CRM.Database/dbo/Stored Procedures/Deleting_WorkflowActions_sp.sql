CREATE    PROCEDURE [dbo].[Deleting_WorkflowActions_sp]
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
				DELETE	WTA
				FROM	[dbo].[ContactWorkflowAudit] AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactWorkflowAuditID
							FROM dbo.ContactWorkflowAudit AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WTA.WorkflowActionID = WAS.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.ContactWorkflowAuditID = WTA.ContactWorkflowAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactWorkflowAudit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTA
				FROM	[dbo].[WorkflowCampaignActions] AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowCampaignActionID
							FROM dbo.WorkflowCampaignActions AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WTA.WorkflowActionID = WAS.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowCampaignActionID = WTA.WorkflowCampaignActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowCampaignActions
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActions'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTA
				FROM	[dbo].WorkFlowTextNotificationAction AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowTextNotificationActionID
							FROM dbo.WorkFlowTextNotificationAction AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WTA.WorkflowActionID = WAS.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkFlowTextNotificationActionID = WTA.WorkFlowTextNotificationActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkFlowTextNotificationAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowTextNotificationAction'




			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTA
				FROM	dbo.WorkflowTimerActions AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTimerActionID
							FROM dbo.WorkflowTimerActions AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WTA.WorkflowActionID = WAS.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowTimerActionID = WTA.WorkflowTimerActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTimerActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTimerActions'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WFSA
				FROM	dbo.WorkFlowLeadScoreAction AS WFSA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowLeadScoreActionID
							FROM dbo.WorkFlowLeadScoreAction AS WFSA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WAS ON WFSA.WorkflowActionID = WAS.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WAS.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkFlowLeadScoreActionID = WFSA.WorkFlowLeadScoreActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkFlowLeadScoreActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowLeadScoreAction'

					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTA
				FROM	dbo.WorkflowTagAction AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTagActionID
							FROM dbo.WorkflowTagAction AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WTA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowTagActionID = WTA.WorkflowTagActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTagActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTagAction'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WLCA
				FROM	dbo.WorkFlowLifeCycleAction AS WLCA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowLifeCycleActionID
							FROM dbo.WorkFlowLifeCycleAction AS WLCA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WLCA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkFlowLifeCycleActionID = WLCA.WorkFlowLifeCycleActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkFlowLifeCycleActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowLifeCycleAction'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WNUA
				FROM	dbo.WorkflowNotifyUserAction AS WNUA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowNotifyUserActionID
							FROM dbo.WorkflowNotifyUserAction AS WNUA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WNUA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowNotifyUserActionID = WNUA.WorkflowNotifyUserActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowNotifyUserActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowNotifyUserAction'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WENA
				FROM	dbo.WorkflowEmailNotificationAction AS WENA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowEmailNotificationActionID
							FROM dbo.WorkflowEmailNotificationAction AS WENA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WENA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowEmailNotificationActionID = WENA.WorkflowEmailNotificationActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowEmailNotificationActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowEmailNotificationAction'

	
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WENA
				FROM	[dbo].[WorkFlowUserAssignmentAction] AS WENA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowUserAssignmentActionID
							FROM dbo.WorkFlowUserAssignmentAction AS WENA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WENA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkFlowUserAssignmentActionID = WENA.WorkFlowUserAssignmentActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkFlowUserAssignmentAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowUserAssignmentAction'
	
	
	
	
						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	TWA
				FROM	dbo.TriggerWorkflowAction AS TWA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TriggerWorkflowActionID
							FROM dbo.TriggerWorkflowAction AS TWA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON TWA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.TriggerWorkflowActionID = TWA.TriggerWorkflowActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT TriggerWorkflowActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from TriggerWorkflowAction'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WCFA
				FROM	dbo.WorkflowContactFieldAction AS WCFA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowContactFieldActionID
							FROM dbo.WorkflowContactFieldAction AS WCFA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WA ON WCFA.WorkflowActionID = WA.WorkflowActionID
							  INNER JOIN  [dbo].Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowContactFieldActionID = WCFA.WorkflowContactFieldActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowContactFieldActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowContactFieldAction'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WA
				FROM	dbo.WorkflowActions AS WA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowActionID
							FROM dbo.WorkflowActions AS WA (NOLOCK)
							  INNER JOIN dbo.Workflows AS W ON WA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowActionID = WA.WorkflowActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowActions'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTA
				FROM	dbo.WorkflowTagAction AS WTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTagActionID
							FROM dbo.WorkflowTagAction AS WTA (NOLOCK)
							  INNER JOIN [dbo].[WorkflowActions] AS WfA ON WTA.WorkflowActionID=WfA.WorkflowActionID 
							INNER JOIN dbo.Workflows AS W ON WfA.WorkflowID = W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowTagActionID = WTA.WorkflowTagActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTagActionCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTagAction'

	


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
	EXEC [dbo].[Deleting_WorkflowActions_sp]
		@AccountID	= 19

*/



