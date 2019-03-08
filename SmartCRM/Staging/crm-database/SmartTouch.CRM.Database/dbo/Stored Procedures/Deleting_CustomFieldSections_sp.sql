CREATE     PROCEDURE [dbo].[Deleting_CustomFieldSections_sp]
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
				DELETE	F
				FROM	dbo.Fields AS F  INNER JOIN(
							SELECT TOP (@RecordPerBatch) FieldID
							FROM dbo.Fields AS FS WITH (NOLOCK)
							  INNER JOIN [dbo].CustomFieldSections AS CFS  ON FS.CustomFieldSectionID =CFS.CustomFieldSectionID
							WHERE FS.AccountID = @AccountID
						) tmp on tmp.FieldID = F.FieldID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted Fields
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Fields'
			
				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CFS
				FROM	dbo.CustomFieldSections AS CFS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CFSS.CustomFieldSectionID
							FROM dbo.CustomFieldSections AS CFSS (NOLOCK)
							  INNER JOIN [dbo].CustomFieldTabs AS CFT ON CFSS.TabID= CFT.CustomFieldTabID
							WHERE	CFT.AccountID = @accountid 
						) tmp on tmp.CustomFieldSectionID = CFS.CustomFieldSectionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CustomFieldSectionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CustomFieldSections'
			   
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
	EXEC [dbo].[Deleting_CustomFieldSections_sp]
		@AccountID	= 94

*/



