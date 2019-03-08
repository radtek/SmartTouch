





CREATE PROCEDURE [dbo].[GET_SendTextQueue]
	(
		@TextQueue AS dbo.TextQueue readonly,
		@NoOfRecords	smallint
	)
AS
BEGIN
	
	SET NOCOUNT ON 

	BEGIN TRY
		
		SET ROWCOUNT @NoOfRecords
		
		DELETE FROM dbo.SendTextQueue
			WHERE RequestGuid IN (SELECT RequestGuid FROM @TextQueue)
			
		SELECT TRD.TextResponseID, TRD.[From], TRD.[To], TRD.SenderID, TRD.[Message],
			TRD.[Status], TRD.ServiceResponse, STQ.RequestGuid, STQ.TokenGuid, STQ.ScheduledTime
			FROM dbo.SendTextQueue STQ 
				INNER JOIN dbo.TextResponse TR 
				ON STQ.RequestGuid = TR.RequestGuid
				INNER JOIN dbo.TextResponseDetails TRD 
				ON TR.TextResponseID = TRD. TextResponseID
		WHERE STQ.ScheduledTime IS NULL OR STQ.ScheduledTime <= GETUTCDATE()

	END TRY
	BEGIN CATCH

		SELECT ErrorNumber = ERROR_NUMBER(), ErrorSeverity = ERROR_SEVERITY(), ErrorState = ERROR_STATE(),
			ErrorProcedure = ERROR_PROCEDURE(), ErrorLine = ERROR_LINE(), ErrorMessage = ERROR_MESSAGE()

	END CATCH
	
	SET NOCOUNT OFF
END

/*
	EXEC	[dbo].[GET_SendTextQueue]
		@TextQueue AS dbo.TextQueue readonly,
		@NoOfRecords	= 100 

*/









