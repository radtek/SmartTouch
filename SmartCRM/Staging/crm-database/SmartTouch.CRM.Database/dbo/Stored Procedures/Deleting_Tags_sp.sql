CREATE    PROCEDURE [dbo].[Deleting_Tags_sp]
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
				DELETE	CEA
				FROM	dbo.WorkflowTriggers AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTriggerID
							FROM dbo.WorkflowTriggers AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = CEA.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTriggers
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.FormTags AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormTagID
							FROM dbo.FormTags AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.FormTagID = CEA.FormTagID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT FormTags
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormTags'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.WorkflowTagAction AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowTagActionID
							FROM dbo.WorkflowTagAction AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.WorkflowTagActionID = CEA.WorkflowTagActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTagAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTagAction'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ActionTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ActionTagMapID
							FROM dbo.ActionTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.ActionTagMapID = CEA.ActionTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ActionTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ActionTagMap'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.CampaignContactTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignContactTagMapID
							FROM dbo.CampaignContactTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.CampaignContactTagMapID = CEA.CampaignContactTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignContactTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignContactTagMap'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.CampaignTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTagMapID
							FROM dbo.CampaignTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.CampaignTagMapID = CEA.CampaignTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignTagMap'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.NoteTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) NoteTagMapID
							FROM dbo.NoteTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.NoteTagMapID = CEA.NoteTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT NoteTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from NoteTagMap'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.NoteTagMap_Audit AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.NoteTagMap_Audit AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.AuditId = CEA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT NoteTagMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from NoteTagMap_Audit'
SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ImportTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportTagMapId
							FROM dbo.ImportTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.ImportTagMapId = CEA.ImportTagMapId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ImportTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportTagMap'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.LeadAdapterTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterTagMapId
							FROM dbo.LeadAdapterTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.LeadAdapterTagMapId = CEA.LeadAdapterTagMapId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadAdapterTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterTagMap'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.OpportunityTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityTagMapId
							FROM dbo.OpportunityTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.OpportunityTagMapId = CEA.OpportunityTagMapId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityTagMap'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.OpportunityTagMap_Audit AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityTagMap_Audit AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.AuditId = CEA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT OpportunityTagMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from OpportunityTagMap_Audit'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ContactTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactTagMapId
							FROM dbo.ContactTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.Tags AS CE ON CEA.TagID=CE.TagID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.ContactTagMapId = CEA.ContactTagMapId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTagMap'
	BEGIN
				DELETE	CE1
				FROM	dbo.Tags  AS CE1  (NOLOCK)
				WHERE	CE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT Tags
			END
			PRINT  ' records deleted from  Tags'


	BEGIN
				DELETE	CE1
				FROM	dbo.Tags_Audit  AS CE1  (NOLOCK)
				WHERE	CE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT Tags_Audit
			END
			PRINT  ' records deleted from  Tags_Audit'


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
	EXEC [dbo].[Deleting_Tags_sp]
		@AccountID	= 2

*/



