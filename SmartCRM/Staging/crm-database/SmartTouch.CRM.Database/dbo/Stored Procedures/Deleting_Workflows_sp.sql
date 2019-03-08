CREATE    PROCEDURE [dbo].[Deleting_Workflows_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY


DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0001 AS CRS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
					  INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0001_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001_Workflows'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0002 AS CRS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
			  INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0002_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002_Workflows'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0003 AS CRS INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0003_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003_Workflows'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0004 AS CRS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
                 				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0004_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004_Workflows'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0005 AS CRS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
                 				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0005_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005_Workflows'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CRS
				FROM	dbo.CampaignRecipients_0006 AS CRS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
            				 INNER JOIN  dbo.Workflows AS W ON CR.WorkflowID=W.WorkflowID 
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CRS.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0006_WorkflowsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006_Workflows'

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
				DELETE	CWA
				FROM	dbo.ContactWorkflowAudit AS CWA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactWorkflowAuditID
							FROM dbo.ContactWorkflowAudit AS CWA (NOLOCK)
							  INNER JOIN dbo.Workflows AS W ON CWA.WorkflowID=W.WorkflowID   
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.ContactWorkflowAuditID = CWA.ContactWorkflowAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactWorkflowAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactWorkflowAudit'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WT
				FROM	dbo.WorkflowTriggers AS WT  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
							FROM dbo.WorkflowTriggers AS WT (NOLOCK)
							  INNER JOIN dbo.Workflows AS W ON WT.WorkflowID=W.WorkflowID   
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WWTA
				FROM	Workflow.TrackActions AS WWTA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) TrackActionID
							FROM Workflow.TrackActions AS WWTA  (NOLOCK)
							  INNER JOIN [Workflow].[TrackMessages]  AS WWTM ON WWTA.TrackMessageID = WWTM.TrackMessageID  
							WHERE	WWTM.AccountID = @accountid 
						) tmp on tmp.TrackActionID = WWTA.TrackActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTrackActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackActions]_[Workflow].[TrackMessages]'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WT
				FROM	[dbo].[Workflows]AS WT  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowID
							FROM dbo.Workflows AS WT (NOLOCK)
							  INNER JOIN dbo.Users AS W ON WT.CreatedBy=W.UserID   
							WHERE	W.AccountID = @accountid 
						) tmp on tmp.WorkflowID = WT.WorkflowID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Workflows_Users
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflows_Users'

	BEGIN
				DELETE	W
				FROM	dbo.Workflows  AS W  (NOLOCK)
				WHERE	W.AccountID = @Accountid  
	         select @@rowcount Workflows1COUNT
			END
		PRINT  ' records deleted from  Workflows'




--successfull execution query-- 
SELECT 'DEL-001' ResultCode 
SELECT @@ROWCOUNT TotalCount

	END TRY

BEGIN CATCH
	
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_Workflows_sp]
		@AccountID	= 94

*/

/*
	SELECT COUNT(*) FROM FormTags WITH (NOLOCK) WHERE FormID IN (SELECT FormID FROM Forms WHERE Accountid = 22)
	SELECT COUNT(*) FROM UserActivityLogs WITH (NOLOCK) where Accountid = 19
	*/

