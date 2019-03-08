CREATE    PROCEDURE [dbo].[Deleting_Notes_sp]
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
				DELETE	NTM
				FROM	dbo.NoteTagMap  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) NoteTagMapID
							FROM dbo.NoteTagMap  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.NoteTagMapID = NTM.NoteTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT NoteTagMap_NotesCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  NoteTagMap'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	NTM
				FROM	dbo.OpportunityNoteMap  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) OpportunityNoteMapID
							FROM dbo.OpportunityNoteMap  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.OpportunityNoteMapID = NTM.OpportunityNoteMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted NoteTagMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  OpportunityNoteMap'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	NTM
				FROM	dbo.OpportunityNoteMap_Audit  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.OpportunityNoteMap_Audit  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.AuditId = NTM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted OpportunityNoteMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  OpportunityNoteMap_Audit'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	NTM
				FROM	dbo.NoteTagMap_Audit  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.NoteTagMap_Audit  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.AuditId = NTM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted NoteTagMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  NoteTagMap_Audit'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	NTM
				FROM	dbo.ContactNoteMap  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactNoteMapID
							FROM dbo.ContactNoteMap  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.ContactNoteMapID = NTM.ContactNoteMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted ContactNoteMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  ContactNoteMap'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	NTM
				FROM	dbo.ContactNoteMap_Audit  AS NTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.ContactNoteMap_Audit  AS NTM (NOLOCK)
							  INNER JOIN [dbo].[Notes] AS N ON NTM.NoteID=N.NoteID
							WHERE	N.AccountID = @Accountid 
						) tmp on tmp.AuditId = NTM.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted ContactNoteMap_Audit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  ContactNoteMap_Audit'


					 BEGIN
				DELETE	NA1
				FROM	dbo.Notes_Audit  AS NA1  (NOLOCK)
				WHERE	NA1.AccountID = @Accountid  
	          select @@rowcount   Notes_Audit1COUNT
			END
			PRINT ' records deleted from  Notes_Audit1'

	BEGIN
				DELETE	N
				FROM	dbo.Notes  AS N  (NOLOCK)
				WHERE	N.AccountID = @Accountid  
	        select @@rowcount  Notes
			END
			PRINT ' records deleted from  Notes'



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
	EXEC [dbo].[Deleting_Notes_sp]
		@AccountID	= 19

*/



