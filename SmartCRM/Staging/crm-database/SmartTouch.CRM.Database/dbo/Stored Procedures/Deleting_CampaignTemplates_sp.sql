CREATE     PROCEDURE [dbo].[Deleting_CampaignTemplates_sp]
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
				DELETE	C
				FROM	dbo.Campaigns AS C  INNER JOIN(
							SELECT TOP (@RecordPerBatch)cte.CampaignTemplateID
							FROM dbo.CampaignTemplates AS Cte (NOLOCK)
							  INNER JOIN dbo.Campaigns AS c ON C.CampaignTemplateID=cte.CampaignTemplateID
							WHERE	c.AccountID = @accountid 
						) tmp on tmp.CampaignTemplateID = C.CampaignTemplateID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignTemplates_CampaignsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignTemplates'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.CampaignTemplates AS CT  INNER JOIN(
							SELECT TOP (@RecordPerBatch)IM.ImageID
							FROM dbo.Images AS IM (NOLOCK)
							  INNER JOIN dbo.CampaignTemplates AS cT ON CT.[ThumbnailImage]=IM.ImageID
							WHERE	IM.AccountID = @accountid 
						) tmp on tmp.ImageID = CT.[ThumbnailImage]

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @TotalRecordsDeleted CampaignTemplates_Images
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImageS'  


				BEGIN
				DELETE	CT
				FROM	[dbo].[CampaignTemplates]  AS  CT  (NOLOCK)
				WHERE	CT.AccountID = @Accountid
	          SELECT @@ROWCOUNT CampaignTemplates
			END
			PRINT ' records deleted from  CampaignTemplates'

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
	EXEC [dbo].[Deleting_CampaignTemplates_sp]
		@AccountID	= 19

*/



