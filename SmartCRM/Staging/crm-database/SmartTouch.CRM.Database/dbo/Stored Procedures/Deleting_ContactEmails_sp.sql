CREATE    PROCEDURE [dbo].[Deleting_ContactEmails_sp]
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

SELECT COUNT(*)count FROM ContactEmailAudit  WITH (NOLOCK) WHERE  ContactEmailID IN (SELECT ContactEmailID FROM ContactEmails WHERE Accountid = @Accountid )
SELECT COUNT(*)count FROM ContactEmails   WITH (NOLOCK) WHERE Accountid = @Accountid
SELECT COUNT(*)count FROM ContactEmails_Audit  WITH (NOLOCK) WHERE Accountid = @Accountid

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CEA
				FROM	dbo.ContactEmailAudit AS CEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactEmailAuditID
							FROM dbo.ContactEmailAudit AS CEA (NOLOCK)
							  INNER JOIN dbo.ContactEmails AS CE ON CEA.ContactEmailID=CE.ContactEmailID 
							WHERE	CE.AccountID = @accountid 
						) tmp on tmp.ContactEmailAuditID = CEA.ContactEmailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactEmailAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmailAudit'

	BEGIN
				DELETE	CE1
				FROM	dbo.ContactEmails  AS CE1  (NOLOCK)
				WHERE	CE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT ContactEmailsCOUNT
			END
			PRINT  ' records deleted from  ContactEmails'


	BEGIN
				DELETE	CE1
				FROM	dbo.ContactEmails_Audit  AS CE1  (NOLOCK)
				WHERE	CE1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT ContactEmails_AuditCOUNT
			END
			PRINT  ' records deleted from  ContactEmails_Audit'


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
	EXEC [dbo].[Deleting_ContactEmails_sp]
		@AccountID	= 19

*/



