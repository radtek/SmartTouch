CREATE   PROCEDURE [dbo].[Deleting_CampaignLinkS_sp]
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
				DELETE	CLs
				FROM	dbo.WorkflowCampaignActionLinks AS CLs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowCampaignLinkID
							FROM dbo.WorkflowCampaignActionLinks AS WCL (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CS ON  CS.CampaignLinkID = WCL.LinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.WorkflowCampaignLinkID = CLS.WorkflowCampaignLinkID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted WorkflowCampaignActionLinks
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActionLinks'



	  	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS1
				FROM	dbo.CampaignStatistics_0001 AS CS1  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0001 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS1.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0001
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0001'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS2
				FROM	dbo.CampaignStatistics_0002 AS CS2  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0002 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS2.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0002
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0002'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS3
				FROM	dbo.CampaignStatistics_0003 AS CS3  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0003 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS3.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0003
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0003'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS4
				FROM	dbo.CampaignStatistics_0004 AS CS4  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0004 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS4.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0004
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0004'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS5
				FROM	dbo.CampaignStatistics_0005 AS CS5  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0005 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS5.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0005
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0005'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS6
				FROM	dbo.CampaignStatistics_0006 AS CS6  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0006 AS CS WITH (NOLOCK)
							  INNER JOIN  [dbo].CampaignLinkS AS CL ON CS.CampaignID = CL.CampaignID AND CS.CampaignLinkID = CL.CampaignLinkID
							  INNER JOIN CampaignS C ON CS.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS6.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignStatistics_0006
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0006'


					SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CLs
				FROM	dbo.CampaignLinks AS CLs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignLinkID
							FROM dbo.CampaignLinks AS CL (NOLOCK)
							  INNER JOIN  [dbo].[Campaigns] AS C ON CL.CampaignID = C.CampaignID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignLinkID = CLs.CampaignLinkID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignLinks
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignLinks'


		

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
	EXEC [dbo].[Deleting_CampaignLinkS_sp]
		@AccountID	= 94

*/



