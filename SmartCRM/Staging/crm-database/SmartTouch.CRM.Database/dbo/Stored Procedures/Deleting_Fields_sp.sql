CREATE     PROCEDURE [dbo].[Deleting_Fields_sp]
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

SELECT COUNT(*)count FROM WorkflowContactFieldAction   WITH (NOLOCK) WHERE  FieldID IN (SELECT FieldID FROM  Fields WHERE Accountid = @Accountid )

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE 	ASF
				FROM	[dbo].[CustomFieldValueOptions] AS ASF INNER JOIN(
							SELECT TOP (@RecordPerBatch)CustomFieldValueOptionID
							FROM dbo.CustomFieldValueOptions AS SF WITH (NOLOCK)
							  INNER JOIN dbo.Fields AS F ON SF.CustomFieldID=F.FieldID    
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.CustomFieldValueOptionID = ASF.CustomFieldValueOptionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS CustomFieldValueOptions
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CustomFieldValueOptions'
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WCFA
				FROM	dbo.WorkflowContactFieldAction AS WCFA INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowContactFieldActionID
							FROM dbo.WorkflowContactFieldAction AS WCFA WITH (NOLOCK)
							  INNER JOIN dbo.Fields AS F ON WCFA.FieldID=F.FieldID  
							WHERE	F.AccountID = @accountid
						) tmp on tmp.WorkflowContactFieldActionID = WCFA.WorkflowContactFieldActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS WorkflowContactFieldAction
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowContactFieldAction'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	FF
				FROM	dbo.FormFields AS FF  INNER JOIN(
							SELECT TOP (@RecordPerBatch) FormFieldID
							FROM dbo.FormFields AS FF WITH (NOLOCK)
							  INNER JOIN [dbo].[Fields] AS F ON FF.[FieldID]= F.[FieldID] 
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.FormFieldID = FF.FormFieldID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS FormFields_Fields
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from FormFields_Fields'

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SF
				FROM	dbo.SearchFilters AS SF INNER JOIN(
							SELECT TOP (@RecordPerBatch) SearchFilterID
							FROM dbo.SearchFilters AS SF WITH (NOLOCK)
							  INNER JOIN dbo.Fields AS F ON SF.FieldID=F.FieldID    
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.SearchFilterID = SF.SearchFilterID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS SearchFilters
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from SearchFilters'



				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ASF
				FROM	dbo.AVColumnPreferences AS ASF INNER JOIN(
							SELECT TOP (@RecordPerBatch) AVColumnPreferenceID
							FROM dbo.AVColumnPreferences AS SF WITH (NOLOCK)
							  INNER JOIN dbo.Fields AS F ON SF.FieldID=F.FieldID    
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.AVColumnPreferenceID = ASF.AVColumnPreferenceID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS AVColumnPreferences
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AVColumnPreferences'
	
			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	ASF
				FROM	dbo.ImportCustomData AS ASF INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportCustomDataID
							FROM dbo.ImportCustomData AS SF WITH (NOLOCK)
							  INNER JOIN dbo.Fields AS F ON SF.FieldID=F.FieldID    
							WHERE	F.AccountID = @accountid 
						) tmp on tmp.ImportCustomDataID = ASF.ImportCustomDataID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT  @TotalRecordsDeleted AS ImportCustomData
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportCustomData'


	

			BEGIN
				DELETE	F
				FROM	dbo.Fields  AS F WITH (NOLOCK)
				WHERE	F.AccountID = @Accountid 
	           SELECT @@ROWCOUNT   Fields
			END
			PRINT  ' records deleted from  Fields'

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
	EXEC [dbo].[Deleting_Fields_sp]
		@AccountID	= 94

*/



