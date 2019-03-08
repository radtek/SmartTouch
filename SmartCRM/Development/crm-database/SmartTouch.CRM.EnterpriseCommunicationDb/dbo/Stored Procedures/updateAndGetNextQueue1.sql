
create PROCEDURE [dbo].[updateAndGetNextQueue1]
	(
		@MailQueue AS dbo.MailQueue readonly,
		@noOfRecords smallint
	)
AS
BEGIN
	/*
	INSERT INTO dbo.SentMailQueue (TokenGuid, RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, StatusID, ServiceResponse)
	SELECT TokenGuid, RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, StatusID, ServiceResponse
		FROM @MailQueue 
	*/
	DELETE FROM dbo.SentMailQueue
		WHERE RequestGuid IN (SELECT RequestGuid FROM @MailQueue)
	
	SET ROWCOUNT @noOfRecords

       SELECT TOP (@noOfRecords) TokenGuid, SMQ.RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, DisplayName,
		ReplyTo, [To], CC, BCC, [Subject], Body, IsBodyHtml, StatusID, ServiceResponse,a.Email ServiceProviderEmail
		FROM dbo.SentMailQueue SMQ 
		INNER JOIN dbo.SentMailDetails SMD ON SMQ.RequestGuid = SMD.RequestGuid 
		LEFT JOIN  dbo.ServiceProviders  s on s.LoginToken = smq.TokenGuid
        LEFT JOIN dbo.Accountemails a on s.ServiceProviderID = a.ServiceProviderID 
		
		
		WHERE SMQ.ScheduledTime IS NULL OR SMQ.ScheduledTime < GETUTCDATE()
		ORDER BY QueueTime

END

/*
	EXEC [dbo].[updateAndGetNextQueue]
		@MailQueue		= dbo.MailQueue readonly,
		@noOfRecords	= 100

*/






