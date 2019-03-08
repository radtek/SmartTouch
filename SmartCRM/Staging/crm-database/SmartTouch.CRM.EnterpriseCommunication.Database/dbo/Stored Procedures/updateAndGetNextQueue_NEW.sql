

CREATE PROCEDURE [dbo].[updateAndGetNextQueue_NEW]
	(
		@MailQueue AS dbo.MailQueue readonly,
		@noOfRecords smallint,
		@instance int
	)
AS
BEGIN
	DECLARE @categories TABLE (Id INT)
	/*
	INSERT INTO dbo.SentMailQueue (TokenGuid, RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, StatusID, ServiceResponse)
	SELECT TokenGuid, RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, StatusID, ServiceResponse
		FROM @MailQueue 
	*/
	IF @instance = 1
		BEGIN
			INSERT INTO @categories
			SELECT 1 UNION SELECT 2 UNION SELECT 3
		END
	ELSE IF @instance = 2
		BEGIN
			INSERT INTO @categories
			SELECT 5
		END
	ELSE IF @instance = 3
		BEGIN
			INSERT INTO @categories
			SELECT 4 UNION  SELECT  6 UNION SELECT 7 UNION SELECT 8 UNION SELECT 9 UNION SELECT 10 UNION SELECT 11 UNION SELECT 12 UNION SELECT 13
			UNION SELECT 14 UNION SELECT 15 UNION SELECT 16 UNION SELECT 17 UNION SELECT 18 UNION SELECT 0
		END
	ELSE IF @instance = 4
		BEGIN
			INSERT INTO @categories
			SELECT 19
		END
		
	SELECT *  FROM dbo.SentMailQueue
		WHERE RequestGuid IN (SELECT RequestGuid FROM @MailQueue)
	
	SET ROWCOUNT @noOfRecords

	SELECT TOP (@noOfRecords) TokenGuid, SMQ.RequestGuid, [From], PriorityID, ScheduledTime, QueueTime, DisplayName,
		ReplyTo, [To], CC, BCC, [Subject], Body, IsBodyHtml, StatusID, ServiceResponse, AE.Email ServiceProviderEmail, AC.DomainURL AccountDomain, smd.AttachmentGUID, SMD.CategoryID
		,SMD.SentMailDetailID
		FROM dbo.SentMailQueue SMQ 
		INNER JOIN dbo.SentMailDetails SMD ON SMQ.RequestGuid = SMD.RequestGuid 
		LEFT JOIN  SmartCRM.dbo.ServiceProviders S on S.LoginToken = SMQ.TokenGuid
		INNER JOIN SmartCRM.dbo.Accounts AC ON AC.AccountID = S.AccountID
        LEFT JOIN SmartCRM.dbo.AccountEmails AE ON S.ServiceProviderID = AE.ServiceProviderID  
		WHERE COALESCE(SMQ.GetProcessedByClassic,0) = 0 AND (SMQ.ScheduledTime IS NULL OR SMQ.ScheduledTime < GETUTCDATE())
		AND SMD.CategoryID IN (SELECT Id FROM @categories)
		ORDER BY QueueTime

END

/*
declare @MailQueue dbo.MailQueue
	EXEC [dbo].[updateAndGetNextQueue] @MailQueue, 100, 4
		
*/



