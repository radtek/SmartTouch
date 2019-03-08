CREATE     PROCEDURE [dbo].[Deleting_ServiceProviders_sp]
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
				DELETE	CR
				FROM	dbo.CampaignRecipients_0001 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
							 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0001_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001_ServiceProviders'




SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR
				FROM	dbo.CampaignRecipients_0002 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
						 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0002_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002_ServiceProviders'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR
				FROM	dbo.CampaignRecipients_0003 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
					 INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0003_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003_ServiceProviders'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR
				FROM	dbo.CampaignRecipients_0004 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
                           INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0004_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004_ServiceProviders'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR
				FROM	dbo.CampaignRecipients_0005 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
	                        INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0005_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005_ServiceProviders'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR
				FROM	dbo.CampaignRecipients_0006 AS CR  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
				                  INNER JOIN dbo.ServiceProviders AS SP ON CR.ServiceProviderID=SP.ServiceProviderID 
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0006_ServiceProvidersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006_ServiceProviders'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	C
				FROM	dbo.Campaigns AS C  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignID
							FROM dbo.Campaigns AS C (NOLOCK)
							  INNER JOIN dbo.ServiceProviders AS SP ON C.ServiceProviderID=SP.ServiceProviderID
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.CampaignID = C.CampaignID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Campaigns'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	AE
				FROM	dbo.AccountEmails AS AE  INNER JOIN(
							SELECT TOP (@RecordPerBatch) EmailID
							FROM dbo.AccountEmails AS A (NOLOCK)
							  INNER JOIN dbo.ServiceProviders AS SP ON A.ServiceProviderID=SP.ServiceProviderID
							WHERE	SP.AccountID = @accountid 
						) tmp on tmp.EmailID = AE.EmailID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountEmails'

			BEGIN
				DELETE	SP
				FROM	dbo.ServiceProviders  AS SP  (NOLOCK)
				WHERE	SP.AccountID = @Accountid 
	          select @@rowcount  ServiceProvidersCOUNT
			END
			PRINT  ' records deleted from  ServiceProviders'

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
	EXEC [dbo].[Deleting_ServiceProviders_sp]
		@AccountID	= 94

*/



