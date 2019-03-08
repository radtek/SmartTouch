CREATE    PROCEDURE [dbo].[Deleting_LeadAdapterJobLogs_sp]
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
				FROM	dbo.LeadAdapterJobLogDetails AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterJobLogDetailID
							FROM dbo.LeadAdapterJobLogDetails AS CEA (NOLOCK)
							  INNER JOIN dbo.LeadAdapterJobLogs AS CE ON CEA.LeadAdapterJobLogID=CE.LeadAdapterJobLogID 
							  inner join LeadAdapterAndAccountMap l on l.LeadAdapterAndAccountMapID = ce.LeadAdapterAndAccountMapID
							WHERE	l.AccountID = @accountid 
						) tmp on tmp.LeadAdapterJobLogDetailID = CEA.LeadAdapterJobLogDetailID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadAdapterJobLogDetails
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterJobLogDetails'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ImportTagMap AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportTagMapID
							FROM dbo.ImportTagMap AS CEA (NOLOCK)
							  INNER JOIN dbo.LeadAdapterJobLogs AS CE ON CEA.LeadAdapterJobLogID=CE.LeadAdapterJobLogID 
							  inner join LeadAdapterAndAccountMap l on l.LeadAdapterAndAccountMapID = ce.LeadAdapterAndAccountMapID
							WHERE	l.AccountID = @accountid 
						) tmp on tmp.ImportTagMapID = CEA.ImportTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ImportTagMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportTagMap'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	[dbo].[ImportDataSettings] AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportDataSettingID
							FROM dbo.ImportDataSettings AS CEA (NOLOCK)
							  INNER JOIN dbo.LeadAdapterJobLogs AS CE ON CEA.LeadAdaperJobID=CE.LeadAdapterJobLogID 
							  inner join LeadAdapterAndAccountMap l on l.LeadAdapterAndAccountMapID = ce.LeadAdapterAndAccountMapID
							WHERE	l.AccountID = @accountid 
						) tmp on tmp.ImportDataSettingID = CEA.ImportDataSettingID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ImportDataSettingS
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportDataSetting'

	BEGIN
				DELETE	CE1
				FROM	dbo.LeadAdapterJobLogs  AS CE1  (NOLOCK)
			inner join LeadAdapterAndAccountMap l on l.LeadAdapterAndAccountMapID = CE1.LeadAdapterAndAccountMapID
							WHERE	l.AccountID = @accountid 
			 
	        SELECT @@ROWCOUNT LeadAdapterJobLogs
			END
			PRINT  ' records deleted from  LeadAdapterJobLogs'





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
	EXEC [dbo].[Deleting_LeadAdapterJobLogs_sp]
		@AccountID	= 19

*/



