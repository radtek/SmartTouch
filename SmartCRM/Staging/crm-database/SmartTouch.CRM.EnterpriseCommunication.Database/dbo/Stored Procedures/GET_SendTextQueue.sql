

CREATE PROCEDURE [dbo].[GET_SendTextQueue]
	(
		@TextQueue AS dbo.TextQueue readonly,
		@NoOfRecords	smallint
	)
AS
BEGIN
	
	SET NOCOUNT ON 

	BEGIN TRY
		
		DELETE FROM STQ
		FROM dbo.SendTextQueue STQ (NOLOCK)
		INNER JOIN @TextQueue TQ ON TQ.RequestGuid = STQ.RequestGuid
			
		SELECT TOP (@NoOfRecords) TRD.TextResponseID, TRD.[From], TRD.[To], TRD.SenderID, TRD.[Message],
			TRD.[Status], '' ServiceResponse, STQ.RequestGuid, STQ.TokenGuid, STQ.ScheduledTime
			FROM dbo.SendTextQueue STQ 
				INNER JOIN dbo.TextResponse TR 
				ON STQ.RequestGuid = TR.RequestGuid
				INNER JOIN dbo.TextResponseDetails TRD 
				ON TR.TextResponseID = TRD. TextResponseID
		WHERE (STQ.ScheduledTime IS NULL OR STQ.ScheduledTime <= GETUTCDATE()) AND TRD.Status = 4

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
