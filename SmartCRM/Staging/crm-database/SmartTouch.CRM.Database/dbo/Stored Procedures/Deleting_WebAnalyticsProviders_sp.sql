CREATE     PROCEDURE [dbo].[Deleting_WebAnalyticsProviders_sp]
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
				DELETE	WDSEA
				FROM	dbo.WebVisitDailySummaryEmailAudit AS WDSEA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WebVisitDailySummaryEmailAuditID
							FROM dbo.WebVisitDailySummaryEmailAudit AS WDSEA (NOLOCK)
							  INNER JOIN dbo.WebAnalyticsProviders AS WAP ON WDSEA.WebAnalyticsProviderID=WAP.WebAnalyticsProviderID  
							WHERE	WAP.AccountID = @accountid 
						) tmp on tmp.WebVisitDailySummaryEmailAuditID = WDSEA.WebVisitDailySummaryEmailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WebVisitDailySummaryEmailAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitDailySummaryEmailAudit'

			
				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WUNM
				FROM	dbo.WebVisitUserNotificationMap AS WUNM INNER JOIN(
							SELECT TOP (@RecordPerBatch) WebVisitUserNotificationMapID
							FROM dbo.WebVisitUserNotificationMap AS WUNM WITH (NOLOCK)
							  INNER JOIN dbo.WebAnalyticsProviders AS WAP ON WUNM.WebAnalyticsProviderID=WAP.WebAnalyticsProviderID  
							WHERE	WAP.AccountID = @accountid 
						) tmp on tmp.WebVisitUserNotificationMapID = WUNM.WebVisitUserNotificationMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted AS WebVisitUserNotificationMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitUserNotificationMap'

				
				DELETE	WAP1
				FROM	dbo.WebAnalyticsProviders  AS WAP1 WITH (NOLOCK) 
				WHERE	WAP1.AccountID = @Accountid 
	          select @@rowcount WebAnalyticsProvidersCOUNT
			PRINT ' records deleted from  WebAnalyticsProviders1'


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
	EXEC [dbo].[Deleting_WebAnalyticsProviders_sp]
		@AccountID	= 19

*/



