

create PROCEDURE [dbo].[updateAndGetNextQueue]
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
		ReplyTo, [To], CC, BCC, [Subject], Body, IsBodyHtml, StatusID, ServiceResponse, AE.Email ServiceProviderEmail, AC.DomainURL AccountDomain, smd.AttachmentGUID
		FROM dbo.SentMailQueue SMQ 
		INNER JOIN dbo.SentMailDetails SMD ON SMQ.RequestGuid = SMD.RequestGuid 
		LEFT JOIN  SmartCRM.dbo.ServiceProviders S on S.LoginToken = SMQ.TokenGuid
		INNER JOIN SmartCRM.dbo.Accounts AC ON AC.AccountID = S.AccountID
        LEFT JOIN SmartCRM.dbo.AccountEmails AE ON S.ServiceProviderID = AE.ServiceProviderID  
		WHERE COALESCE(SMQ.GetProcessedByClassic,0) = 0 AND (SMQ.ScheduledTime IS NULL OR SMQ.ScheduledTime < GETUTCDATE())
		ORDER BY QueueTime

END

/*
	EXEC [dbo].[updateAndGetNextQueue]
		@MailQueue		= dbo.MailQueue readonly,
		@noOfRecords	= 100

*/






