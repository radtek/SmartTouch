CREATE     PROCEDURE [dbo].[Deleting_Roles_sp]
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
				DELETE	RM
				FROM	dbo.RoleModuleMap  AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) RoleModuleMapID
							FROM dbo.RoleModuleMap  AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[Roles] AS R ON RM.RoleID=R.RoleID
							WHERE	R.AccountID = @Accountid 
						) tmp on tmp.RoleModuleMapID = RM.RoleModuleMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted RoleModuleMap_RolesCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from  RoleModuleMap'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	dbo. Users AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserID
							FROM dbo. Users AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[Roles] AS R ON RM.RoleID=R.RoleID
							WHERE	R.AccountID = @Accountid 
						) tmp on tmp.UserID = RM.UserID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted Users
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Users ' 


			
SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RM
				FROM	dbo. BulkOperations AS RM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) BulkOperationID
							FROM dbo. BulkOperations AS RM WITH (NOLOCK)
							  INNER JOIN [dbo].[Roles] AS R ON RM.RoleID=R.RoleID
							WHERE	R.AccountID = @Accountid 
						) tmp on tmp.BulkOperationID = RM.BulkOperationID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted BulkOperations
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from BulkOperations ' 

			BEGIN
				DELETE	R
				FROM	dbo.Roles  AS R WITH (NOLOCK)
				WHERE	R.AccountID = @Accountid 
	          select @@rowcount  Roles
			END
			PRINT ' records deleted from  Roles' 

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
	EXEC [dbo].[Deleting_Roles_sp]
		@AccountID	= 19

*/



