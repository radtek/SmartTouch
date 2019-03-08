
CREATE proc [Workflow].[updateTrackActionLogs]
@trackActionItems as Workflow.TrackActionType readonly
,@trackActionLogItems as Workflow.TrackActionLogType readonly
as
begin
	--INSERT into TEMPTrackActionType
	--SELECT * FROM @trackActionItems

	--INSERT into TEMPActionLogItems
	--SELECT * FROM @trackActionLogItems
	BEGIN TRY		
		Update TA
		set TA.ExecutedOn = TAI.ExecutedOn, TA.ActionProcessStatusID = TAI.ActionProcessStatusID
		from Workflow.TrackActions TA
		JOIN @trackActionItems TAI ON TA.TrackActionId = TAI.TrackActionID
	END TRY
	BEGIN CATCH		
		INSERT INTO CRMLogs.dbo.CRMDBLogs(UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())
	END CATCH

	BEGIN TRY
		INSERT INTO Workflow.TrackActionLogs(TrackActionId, ErrorMessage)
		SELECT TrackActionId, ErrorMessage FROM @trackActionLogItems
	END TRY
	BEGIN CATCH
		INSERT INTO CRMLogs.dbo.CRMDBLogs(UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())
	END CATCH
end