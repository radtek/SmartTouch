
CREATE    PROCEDURE [dbo].[Deleting_Users_sp]
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
				DELETE	D
				FROM	[dbo].[CRMOutlookSync] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) OutlookSyncID
							FROM dbo.CRMOutlookSync AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.LastSyncedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.OutlookSyncID = D.OutlookSyncID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT CRMOutlookSync
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from CRMOutlookSync'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	UPA
				FROM	dbo.UserProfileAudit AS UPA INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserProfileAuditID
							FROM dbo.UserProfileAudit AS UPA (NOLOCK)
							  INNER JOIN dbo.Users AS U ON UPA.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserProfileAuditID = UPA.UserProfileAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserProfileAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserProfileAudit'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[LeadScoreRules] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadScoreRuleID
							FROM dbo.LeadScoreRules AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.ModifiedBy=U.UserID 
							WHERE	u.AccountID = @accountid 
						) tmp on tmp.LeadScoreRuleID = D.LeadScoreRuleID

			SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadScoreRules
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScoreRules'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[LeadScoreRules] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadScoreRuleID
							FROM dbo.LeadScoreRules AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	S.AccountID = @accountid 
						) tmp on tmp.LeadScoreRuleID = D.LeadScoreRuleID

			SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadScoreRules
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadScoreRules1'


					
	

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	dbo.WorkflowEmailNotificationAction AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkFlowEmailNotificationActionID
							FROM dbo.WorkflowEmailNotificationAction AS S (NOLOCK)
							  INNER JOIN [dbo].AccountEmails AS U ON S.FromEmailID=U.EmailID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.WorkFlowEmailNotificationActionID = D.WorkFlowEmailNotificationActionID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowEmailNotificationAction_AccountEmails
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowEmailNotificationAction'


	SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	DUSM
				FROM	dbo.DashboardUserSettingsMap AS DUSM INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSettingsMapID
							FROM dbo.DashboardUserSettingsMap AS DUSM (NOLOCK)
							  INNER JOIN dbo.Users AS U ON DUSM.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserSettingsMapID = DUSM.UserSettingsMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT DashboardUserSettingsMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DashboardUserSettingsMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	LA
				FROM	dbo.LoginAudit AS LA INNER JOIN(
							SELECT TOP (@RecordPerBatch) LoginAuditID
							FROM dbo.LoginAudit AS LA (NOLOCK)
							  INNER JOIN dbo.Users AS U ON LA.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.LoginAuditID = LA.LoginAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LoginAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LoginAudit'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	N
				FROM	dbo.Notifications AS N INNER JOIN(
							SELECT TOP (@RecordPerBatch) NotificationID
							FROM dbo.Notifications AS N (NOLOCK)
							  INNER JOIN dbo.Users AS U ON N.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.NotificationID = N.NotificationID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT NotificationsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Notifications'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	UAM
				FROM	dbo.UserAddressMap AS UAM INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserAddressMapID
							FROM dbo.UserAddressMap AS UAM (NOLOCK)
							  INNER JOIN dbo.Users AS U ON UAM.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserAddressMapID = UAM.UserAddressMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserAddressMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserAddressMap'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	US
				FROM	dbo.UserSettings AS US INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSettingID
							FROM dbo.UserSettings AS US (NOLOCK)
							  INNER JOIN dbo.Users AS U ON US.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserSettingID = US.UserSettingID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserSettingsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSettings'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	USMP
				FROM	dbo.UserSocialMediaPosts AS USMP INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSocialMediaPostID
							FROM dbo.UserSocialMediaPosts AS USMP (NOLOCK)
							  INNER JOIN dbo.Users AS U ON USMP.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserSocialMediaPostID = USMP.UserSocialMediaPostID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserSocialMediaPostsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSocialMediaPosts'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WDSEA
				FROM	dbo.WebVisitDailySummaryEmailAudit AS WDSEA INNER JOIN(
							SELECT TOP (@RecordPerBatch) WebVisitDailySummaryEmailAuditID
							FROM dbo.WebVisitDailySummaryEmailAudit AS WDSEA (NOLOCK)
							  INNER JOIN dbo.Users AS U ON WDSEA.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
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
							FROM dbo.WebVisitUserNotificationMap AS WUNM (NOLOCK)
							  INNER JOIN dbo.Users AS U ON WUNM.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.WebVisitUserNotificationMapID = WUNM.WebVisitUserNotificationMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WebVisitUserNotificationMapCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WebVisitUserNotificationMap'



			--SET @RecordsDeleted = 1
			--WHILE( @RecordsDeleted > 0 )
			--BEGIN
			--	DELETE	WUAA
			--	FROM	dbo.WorkFlowUserAssignmentAction AS WUAA INNER JOIN(
			--				SELECT TOP (@RecordPerBatch) WorkFlowUserAssignmentActionID
			--				FROM dbo.WorkFlowUserAssignmentAction AS WUAA (NOLOCK)
			--				  INNER JOIN dbo.Users AS U ON WUAA.UserID=U.UserID 
			--				WHERE	U.AccountID = @accountid 
			--			) tmp on tmp.WorkFlowUserAssignmentActionID = WUAA.WorkFlowUserAssignmentActionID

			--	SET @RecordsDeleted = @@rowcount
			--	SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
			--	SELECT @@ROWCOUNT WorkFlowUserAssignmentActionCOUNT
			--END
			--PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkFlowUserAssignmentAction'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	WTM
				FROM	[Workflow].[TrackMessages]  AS WTM  INNER JOIN(
							SELECT TOP (@RecordPerBatch) [TrackMessageID]
							FROM [Workflow].[TrackMessages]  AS WTM (NOLOCK)
							  INNER JOIN dbo.Users AS u ON WTM.UserID=U.UserID
							WHERE	U.AccountID = @Accountid 
						) tmp on tmp.TrackMessageID = WTM.TrackMessageID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT WorkflowTrackMessages_UsersCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from [Workflow].[TrackMessages] '

				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	AE
				FROM	dbo.AccountEmails AS AE  INNER JOIN(
							SELECT TOP (@RecordPerBatch) EmailID
							FROM dbo.AccountEmails AS AE (NOLOCK)
							  INNER JOIN dbo.Users AS U ON AE.UserID=U.UserID 
							WHERE	U.AccountID = @Accountid 
						) tmp on tmp.EmailID = AE.EmailID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT AccountEmailsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from AccountEmails'


				SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	UAL
				FROM	dbo.UserActivityLogs AS UAL  INNER JOIN(
							SELECT TOP (@RecordPerBatch)UserActivityLogID
							FROM dbo.UserActivityLogs AS UA (NOLOCK)
							  INNER JOIN dbo.Users AS U ON UA.UserID=U.UserID 
							WHERE	UA.AccountID = @Accountid 
						) tmp on tmp.UserActivityLogID = UAL.UserActivityLogID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)

				SELECT @@ROWCOUNT UserActivityLogsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserActivityLogs'


		SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	SP
				FROM	dbo.StoreProcParamsList AS SP  INNER JOIN(
							SELECT TOP (@RecordPerBatch)ListID
							FROM dbo.StoreProcParamsList AS SPP (NOLOCK)
							  INNER JOIN dbo.Users AS U ON SPP.UserID=U.UserID 
							WHERE	U.AccountID = @Accountid 
						) tmp on tmp.ListID = SP.ListID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT StoreProcParamsListCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from StoreProcParamsList'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	US
				FROM	dbo.UserSettings AS US INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSettingID
							FROM dbo.UserSettings AS US (NOLOCK)
							  INNER JOIN dbo.Users AS U ON US.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserSettingID = US.UserSettingID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserSettingsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSettings'

			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	E
				FROM	dbo.Emails  AS E  INNER JOIN(
							SELECT TOP (@RecordPerBatch) EmailID
							FROM dbo.Emails  AS AE (NOLOCK)
							  INNER JOIN dbo.Users AS U ON AE.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.EmailID = E.EmailID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Emails_AccountsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Emails'

			--SET @RecordsDeleted = 1
			--WHILE( @RecordsDeleted > 0 )
			--BEGIN
			--	DELETE	W
			--	FROM	dbo.WorkflowNotifyUserAction  AS W  INNER JOIN(
			--				SELECT TOP (@RecordPerBatch) WorkFlowNotifyUserActionID
			--				FROM dbo.WorkflowNotifyUserAction  AS WAE (NOLOCK)
			--				  INNER JOIN dbo.Users AS U ON WAE.UserID=U.UserID 
			--				WHERE	U.AccountID = @accountid 
			--			) tmp on tmp.WorkFlowNotifyUserActionID = W.WorkFlowNotifyUserActionID

			--	SET @RecordsDeleted = @@rowcount
			--	SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
			--	SELECT @@ROWCOUNT WorkflowNotifyUserAction_AccountsCOUNT
			--END
			--PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from WorkflowNotifyUserAction'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	USMP
				FROM	dbo.UserSocialMediaPosts AS USMP INNER JOIN(
							SELECT TOP (@RecordPerBatch) UserSocialMediaPostID
							FROM dbo.UserSocialMediaPosts AS USMP (NOLOCK)
							  INNER JOIN dbo.Users AS U ON USMP.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.UserSocialMediaPostID = USMP.UserSocialMediaPostID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT UserSocialMediaPostsCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from UserSocialMediaPosts'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	dbo.DailySummaryEmailAudit AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch)DailySummaryEmailAuditID
							FROM dbo.DailySummaryEmailAudit AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.DailySummaryEmailAuditID = D.DailySummaryEmailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT DailySummaryEmailAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from DailySummaryEmailAudit'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	dbo.ReceivedMailAudit AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) ReceivedMailAuditID
							FROM dbo.ReceivedMailAudit AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.UserID=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.ReceivedMailAuditID = D.ReceivedMailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ReceivedMailAuditCOUNT
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ReceivedMailAudit'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Workflows] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowID
							FROM dbo.Workflows AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.WorkflowID = D.WorkflowID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Workflows
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflows'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Workflows] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) WorkflowID
							FROM dbo.Workflows AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.ModifiedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.WorkflowID = D.WorkflowID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Workflows
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Workflows1'

SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Notes] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) NoteID
							FROM dbo.Notes AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.NoteID = D.NoteID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Notes
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Notes'


SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[ClientRefreshTokens] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) ID
							FROM dbo.ClientRefreshTokens AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.LastUpdatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.ID = D.ID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ClientRefreshTokens
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ClientRefreshTokens'



SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[ImportDataSettings] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) ImportDataSettingID
							FROM dbo.ImportDataSettings AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.ProcessBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.ImportDataSettingID = D.ImportDataSettingID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ImportDataSettings
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ImportDataSettings'


			SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[LeadAdapterAndAccountMap] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterAndAccountMapID
							FROM dbo.LeadAdapterAndAccountMap AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.LeadAdapterAndAccountMapID = D.LeadAdapterAndAccountMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadAdapterAndAccountMap
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterAndAccountMap'

						SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[LeadAdapterAndAccountMap] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) LeadAdapterAndAccountMapID
							FROM dbo.LeadAdapterAndAccountMap AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.ModifiedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.LeadAdapterAndAccountMapID = D.LeadAdapterAndAccountMapID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT LeadAdapterAndAccountMap1
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from LeadAdapterAndAccountMap1'

								SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Tours] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) TourID
							FROM dbo.Tours AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.TourID = D.TourID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Tours
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours'

											SET @RecordsDeleted = 1
			WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Tours] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) TourID
							FROM dbo.Tours AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.CreatedBy=U.UserID 
							WHERE	U.AccountID = @accountid 
						) tmp on tmp.TourID = D.TourID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Tours
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Tours'
	
	SET @RecordsDeleted = 1
	WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[Contacts] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactID
							FROM dbo.Contacts AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.OwnerID=U.UserID 
							WHERE	s.AccountID = @accountid 
						) tmp on tmp.ContactID = D.ContactID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT Contacts
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from Contacts'

				SET @RecordsDeleted = 1
	WHILE( @RecordsDeleted > 0 )
			BEGIN
				DELETE	D
				FROM	[dbo].[ContactEmailAudit] AS D INNER JOIN(
							SELECT TOP (@RecordPerBatch) ContactEmailAuditID
							FROM dbo.ContactEmailAudit AS S (NOLOCK)
							  INNER JOIN dbo.Users AS U ON S.SentBy=U.UserID 
							WHERE	u.AccountID = @accountid 
						) tmp on tmp.ContactEmailAuditID = D.ContactEmailAuditID

				SET @RecordsDeleted = @@rowcount
				SET @TotalRecordsDeleted = @TotalRecordsDeleted + ISNULL(@RecordsDeleted, 0)
				SELECT @@ROWCOUNT ContactEmailAudit
			END
			PRINT CAST(@TotalRecordsDeleted AS VARCHAR) + ' records deleted from ContactEmailAudit'


			BEGIN
				DELETE	U
				FROM	dbo.Users  AS U  (NOLOCK)
				WHERE	U.AccountID = @Accountid 
	         select @@rowcount UsersCOUNT
			END
			PRINT  ' records deleted from  Users'	
			




			
			

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
	EXEC [dbo].[Deleting_Users_sp]
		@AccountID	= 2

*/

/*
	SELECT COUNT(*) FROM FormTags WITH (NOLOCK) WHERE FormID IN (SELECT FormID FROM Forms WHERE Accountid = 22)
	SELECT COUNT(*) FROM UserActivityLogs WITH (NOLOCK) where Accountid = 19
	*/


