CREATE    PROCEDURE [dbo].[Deleting_Campaigns_sp]
(
@AccountID INT = 0
)
AS 
BEGIN 
 SET NOCOUNT ON
  BEGIN TRY


DECLARE @TotalRecordsDeleted int = 1,
					 @RecordsDeleted int = 1,
					 @RecordPerBatch int = 5000

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR1
				FROM	dbo.CampaignRecipients_0001 AS CR1  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0001 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR1.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0001COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0001'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR2
				FROM	dbo.CampaignRecipients_0002 AS CR2  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0002 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR2.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0002COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0002'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR3
				FROM	dbo.CampaignRecipients_0003 AS CR3  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0003 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR3.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0003COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0003'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR4
				FROM	dbo.CampaignRecipients_0004 AS CR4 INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0004 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR4.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0004COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0004'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR5
				FROM	dbo.CampaignRecipients_0005 AS CR5  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0005 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR5.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0005COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0005'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CR6
				FROM	dbo.CampaignRecipients_0006 AS CR6  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignRecipientID
							FROM dbo.CampaignRecipients_0006 AS CR (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignRecipientID = CR6.CampaignRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignRecipients_0006COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignRecipients_0006'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS1
				FROM	dbo.CampaignStatistics_0001 AS CS1  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0001 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS1.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0001COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0001'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS2
				FROM	dbo.CampaignStatistics_0002 AS CS2  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0002 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS2.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0002COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0002'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS3
				FROM	dbo.CampaignStatistics_0003 AS CS3  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0003 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS3.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0003COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0003'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS4
				FROM	dbo.CampaignStatistics_0004 AS CS4  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0004 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS4.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0004COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0004'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS5
				FROM	dbo.CampaignStatistics_0005 AS CS5  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0005 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS5.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0005COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0005'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS6
				FROM	dbo.CampaignStatistics_0006 AS CS6  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTrackerID
							FROM dbo.CampaignStatistics_0006 AS CS (NOLOCK)
							  INNER JOIN [dbo].vCampaignRecipients AS CR ON CS.CampaignRecipientID = CR.CampaignRecipientID
							  INNER JOIN [dbo].[Campaigns] AS C ON CR.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTrackerID = CS6.CampaignTrackerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignStatistics_0006COUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignStatistics_0006'

	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	RCs
				FROM	dbo.ResentCampaigns AS RCs  INNER JOIN(
							SELECT TOP (@RecordPerBatch) ResentCampaignID
							FROM dbo.ResentCampaigns AS RC (NOLOCK)
							  INNER JOIN  [dbo].[Campaigns] AS C ON RC.CampaignID = C.CampaignID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ResentCampaignID = RCs.ResentCampaignID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ResentCampaignsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ResentCampaigns'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CT
				FROM	dbo.CampaignTagMap AS CT  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignTagMapID
							FROM dbo.CampaignTagMap AS CTM (NOLOCK)
							  INNER JOIN  [dbo].[Campaigns] AS C ON CTM.CampaignID = C.CampaignID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignTagMapID = CT.CampaignTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignTagMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignTagMap'

		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CCM
				FROM	dbo.CampaignContactTagMap AS CCM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignContactTagMapID
							FROM dbo.CampaignContactTagMap AS CCTM (NOLOCK)
							  INNER JOIN dbo.Campaigns AS C ON CCTM.CampaignID=C.CampaignID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignContactTagMapID = CCM.CampaignContactTagMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignContactTagMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignContactTagMap'


		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	U
				FROM	dbo.UserSocialMediaPosts AS U  INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSocialMediaPostID
							FROM dbo.UserSocialMediaPosts AS US (NOLOCK)
							  INNER JOIN dbo.Campaigns AS C ON US.CampaignID=C.CampaignID 
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.UserSocialMediaPostID = U.UserSocialMediaPostID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserSocialMediaPostsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSocialMediaPosts'

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
				SELECT @@ROWCOUNT CampaignLinksCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignLinks'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WCA
				FROM	dbo.WorkflowCampaignActions AS WCA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowCampaignActionID
							FROM dbo.WorkflowCampaignActions AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.WorkflowCampaignActionID = WCA.WorkflowCampaignActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowCampaignActionsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowCampaignActions'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CS
				FROM	dbo.CampaignSearchDefinitionMap AS CS  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignSearchDefinitionMapID
							FROM dbo.CampaignSearchDefinitionMap AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignSearchDefinitionMapID = CS.CampaignSearchDefinitionMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignSearchDefinitionMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignSearchDefinitionMap'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CA
				FROM	dbo.CampaignAnalytics AS CA  INNER JOIN(
							SELECT TOP (@RecordPerBatch) CampaignStatisticsID
							FROM dbo.CampaignAnalytics AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignStatisticsID = CA.CampaignStatisticsID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignAnalyticsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignAnalytics'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CA
				FROM	dbo.CampaignLogDetails AS CA  INNER JOIN(
							SELECT TOP (@RecordPerBatch)CampaignLogDetailsID
							FROM dbo.CampaignLogDetails AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.CampaignLogDetailsID = CA.CampaignLogDetailsID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CampaignLogDetailsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CampaignLogDetails'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CA
				FROM	dbo.MomentaryCampaignRecipients AS CA  INNER JOIN(
							SELECT TOP (@RecordPerBatch)MomentaryRecipientID
							FROM dbo.MomentaryCampaignRecipients AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.MomentaryRecipientID = CA.MomentaryRecipientID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT MomentaryCampaignRecipientsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from MomentaryCampaignRecipients'

			
SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CA
				FROM	dbo.ClassicCampaigns AS CA  INNER JOIN(
							SELECT TOP (@RecordPerBatch)ClassicCampaignID
							FROM dbo.ClassicCampaigns AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.ClassicCampaignID = CA.ClassicCampaignID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ClassicCampaignsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ClassicCampaigns'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	CA
				FROM	[dbo].[WorkflowTriggers] AS CA  INNER JOIN(
							SELECT TOP (@RecordPerBatch)WorkflowTriggerID
							FROM dbo.WorkflowTriggers AS WCA (NOLOCK)
							  INNER JOIN [dbo].[Campaigns] AS C ON WCA.CampaignID = C.CampaignID
							WHERE	C.AccountID = @accountid 
						) tmp on tmp.WorkflowTriggerID = CA.WorkflowTriggerID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTriggersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [WorkflowTriggers]'

			BEGIN
				DELETE	c
				FROM	dbo.Campaigns  AS c  (NOLOCK)
				WHERE	c.AccountID = @Accountid
	          SELECT @@ROWCOUNT Campaigns
			END
			PRINT ' records deleted from  Campaigns' 

			

--successfull execution query-- 
SELECT 'DEL-001' ResultCode 
SELECT @@ROWCOUNT TotalCount

	END TRY

BEGIN CATCH
	
		--Unsuccessful execution query-- 
		SELECT 'DEL-002' ResultCode 
		--Error blocking statement in between catch --
		INSERT INTO SmartCRM_Test.dbo.ImportContactLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())

END CATCH
	SET NOCOUNT OFF
END 


/*
	EXEC [dbo].[Deleting_Campaigns_sp]
		@AccountID	= 94

*/

/*
	SELECT COUNT(*) FROM FormTags WITH (NOLOCK) WHERE FormID IN (SELECT FormID FROM Forms WHERE Accountid = 22)
	SELECT COUNT(*) FROM UserActivityLogs WITH (NOLOCK) where Accountid = 19
	*/

