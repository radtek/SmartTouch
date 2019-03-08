

/*
		Purpose		: Delete Ignored TrackMessages
		Input		: @NoOfRecords
		Output		: Return Codes
		Created By	: Narendra M
		Created On	: Dec 02, 2015
		Modified On	: 
*/
CREATE PROCEDURE [dbo].[DELETE_Ignored_TrackMessages]
	(
		@NoOfRecords	int = 4000
	)
AS
BEGIN	
	SET NOCOUNT ON
	BEGIN TRY			
			
			DECLARE @MinTrackMessageID	int = 0, 
					@MaxTrackMessageID	int = 5000				

			/*Get Min TrackMessageID */
			SELECT	@MinTrackMessageID = MIN(TrackMessageID)
							FROM (	SELECT TOP (@NoOfRecords) TrackMessageID FROM Workflow.TrackMessages WITH (NOLOCK)
										WHERE MessageProcessStatusID = 702 AND TrackMessageID > @MinTrackMessageID ORDER BY TrackMessageID 
								 ) Temp1

			/*Get Max TrackMessageID */
			SELECT	@MaxTrackMessageID = MAX(TrackMessageID)
							FROM (	SELECT TOP (@NoOfRecords) TrackMessageID FROM Workflow.TrackMessages WITH (NOLOCK)
										WHERE MessageProcessStatusID = 702 AND TrackMessageID > @MinTrackMessageID ORDER BY TrackMessageID 
								 ) Temp1

			WHILE (ISNULL(@MinTrackMessageID, 0) > 0)
				BEGIN		
					
					/* Move Ignored TrackMessages to archive database */
					INSERT INTO SmartCRMArchive.dbo.TrackMessages (TrackMessageID, MessageID, LeadScoreConditionType, EntityID, LinkedEntityID, ContactID, UserID, AccountID, ConditionValue, CreatedOn, MessageProcessStatusID)
					SELECT TrackMessageID, MessageID, LeadScoreConditionType, EntityID, LinkedEntityID, ContactID, UserID, AccountID, ConditionValue, CreatedOn, MessageProcessStatusID
						FROM Workflow.TrackMessages WITH (NOLOCK)
						WHERE MessageProcessStatusID = 702 
							AND TrackMessageID >= @MinTrackMessageID AND TrackMessageID <= @MaxTrackMessageID
					
					/* DELETE Ignored TrackMessages	*/
						DELETE FROM Workflow.TrackMessages
						WHERE MessageProcessStatusID = 702 
							AND TrackMessageID >= @MinTrackMessageID AND TrackMessageID <= @MaxTrackMessageID

					SET @MinTrackMessageID	= @MaxTrackMessageID
		
					/*Get Min TrackMessageID */
					SELECT	@MinTrackMessageID = MIN(TrackMessageID)
									FROM (	SELECT TOP (@NoOfRecords) TrackMessageID FROM Workflow.TrackMessages WITH (NOLOCK)
												WHERE MessageProcessStatusID = 702 AND TrackMessageID > @MinTrackMessageID ORDER BY TrackMessageID 
										 ) Temp1
		
					/*Get Max TrackMessageID */
					SELECT	@MaxTrackMessageID = MAX(TrackMessageID)
									FROM (	SELECT TOP (@NoOfRecords) TrackMessageID FROM Workflow.TrackMessages WITH (NOLOCK)
												WHERE MessageProcessStatusID = 702 AND TrackMessageID > @MinTrackMessageID ORDER BY TrackMessageID 
										 ) Temp1
				END
	END TRY
	BEGIN CATCH
	
		INSERT INTO CRMLogs.dbo.CRMDBLogs (UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate, Scripts)
		VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), 
			GETUTCDATE(), CONVERT(VARCHAR(MAX), @MinTrackMessageID) +' || '+ CONVERT(VARCHAR(MAX), @MaxTrackMessageID))

	END CATCH
	SET NOCOUNT OFF
END

/*
	EXEC dbo.DELETE_Ignored_TrackMessages
		@NoOfRecords		= 4000

*/



