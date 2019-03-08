CREATE PROCEDURE [dbo].[Deleting_Community_sp]
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
				DELETE	CCM
				FROM	dbo.ContactCommunityMap  AS CCM INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactCommunityMapID
							FROM dbo.ContactCommunityMap AS CCM (NOLOCK)
							  INNER JOIN dbo.Communities AS C ON CCM.CommunityID=C.CommunityID
							WHERE	C.AccountID = @AccountID 
						) tmp on tmp.ContactCommunityMapID = CCM.ContactCommunityMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactCommunityMap_Communitiescount
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactCommunityMap_Communities '

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	T
				FROM	dbo.Tours AS T INNER JOIN(
							SELECT TOP (@RecordPerBatch) TourID
							FROM dbo.Tours AS T (NOLOCK)
							  INNER JOIN dbo.Communities AS C ON T.CommunityID=C.CommunityID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.TourID = T.TourID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Tours_CommunitiesCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours_Communities'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	TA
				FROM	dbo.Tours_Audit AS TA INNER JOIN(
							SELECT TOP (@RecordPerBatch) AuditId
							FROM dbo.Tours_Audit AS T (NOLOCK)
							  INNER JOIN dbo.Communities AS C ON T.CommunityID=C.CommunityID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.AuditId = TA.AuditId

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Tours_Audit_CommunitiesCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours_Audit_Communities'



	BEGIN
				DELETE	C1
				FROM	dbo.Communities  AS C1  (NOLOCK)
				WHERE	C1.AccountID = @Accountid 
	        SELECT @@ROWCOUNT  Communities1
			END
			PRINT  ' records deleted from  Communities1'

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
	EXEC [dbo].[Deleting_Community_sp]
		@AccountID	= 94

*/

