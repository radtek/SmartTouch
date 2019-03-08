CREATE   PROCEDURE [dbo].[Deleting_ContactPhoneNumbers_sp]
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
				DELETE	CTMA
				FROM	dbo.ContactTextMessageAudit AS CTMA INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactTextMessageAuditID
							FROM dbo.ContactTextMessageAudit AS CTM (NOLOCK)
							  INNER JOIN dbo.ContactPhoneNumbers AS C ON CTM.ContactPhoneNumberID=C.ContactPhoneNumberID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ContactTextMessageAuditID = CTMA.ContactTextMessageAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactTextMessageAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactTextMessageAudit'

BEGIN
		DELETE	CPN
				FROM	dbo.ContactPhoneNumbers  AS CPN  (NOLOCK)
				WHERE	CPN.AccountID = @Accountid 
	          SELECT @@ROWCOUNT ContactPhoneNumbers1COUNT
			END
			PRINT ' records deleted from  ContactPhoneNumbers'

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
	EXEC [dbo].[Deleting_ContactPhoneNumbers_sp]
		@AccountID	= 19

*/

/*
	SELECT COUNT(*) FROM ContactTextMessageAudit WITH (NOLOCK) WHERE ContactPhoneNumberID IN (SELECT ContactPhoneNumberID FROM ContactPhoneNumbers WHERE Accountid = 19)
	SELECT COUNT(*) FROM ContactPhoneNumbers WITH (NOLOCK) where Accountid = 22
*/

