CREATE    PROCEDURE [dbo].[Deleting_Forms_sp]
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
							  INNER JOIN dbo.Forms AS F ON WT.FormID=F.FormID   
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = WT.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowTriggers'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WT
				FROM	[dbo].[SubmittedFormData] AS WT INNER JOIN(
							SELECT TOP (@RecordPerBatch) SubmittedFormDataID
							FROM dbo.SubmittedFormData AS WT (NOLOCK)
							  INNER JOIN dbo.Forms AS F ON WT.FormID=F.FormID   
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.SubmittedFormDataID = WT.SubmittedFormDataID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT SubmittedFormData
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SubmittedFormData'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	FF
				FROM	dbo.FormFields AS FF INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormFieldID
							FROM dbo.FormFields AS FF (NOLOCK)
							  INNER JOIN dbo.Forms AS F ON FF.FormID=F.FormID   
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.FormFieldID = FF.FormFieldID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT FormFields_FormsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormFields_Forms'

			
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	FT
				FROM	dbo.FormTags AS FT INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormTagID
							FROM dbo.FormTags AS FT (NOLOCK)
							  INNER JOIN dbo.Forms AS F ON FT.FormID=F.FormID   
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.FormTagID = FT.FormTagID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT FormTagsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormTags'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	FS
				FROM	dbo.FormSubmissions AS FS INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormSubmissionID
							FROM dbo.FormSubmissions AS FS (NOLOCK)
							  INNER JOIN dbo.Forms AS F ON FS.FormID=F.FormID   
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.FormSubmissionID = FS.FormSubmissionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT FormSubmissionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormSubmissions'

			 BEGIN
				DELETE	F
				FROM	dbo.Forms  AS F  (NOLOCK)
				WHERE	F.AccountID = @Accountid 
	         SELECT @@ROWCOUNT   Forms
			END
			PRINT ' records deleted from  Forms'
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
	EXEC [dbo].[Deleting_Forms_sp]
		@AccountID	= 94

*/



