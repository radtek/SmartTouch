
CREATE PROCEDURE [dbo].[Process_ActionMailsToContacts]
AS
BEGIN
	
	DECLARE @count INT = 0
	DECLARE @counter INT = 1


	SELECT AMO.*, A.AccountId, A.ActionStartTime, ROW_NUMBER () OVER (ORDER BY A.ActionStartTime ASC) as RowIndex
	INTO #tempActionMails
	FROM ActionsMailOperations (nolock) AMO
	INNER JOIN Actions (NOLOCK) A ON A.ActionId = AMO.ActionId
	WHERE A.ActionStartTime < GETUTCDATE() AND AMO.IsProcessed = 1 -- Ready To Process
	
	
	SELECT @count = COUNT(1) FROM #tempActionMails
	-- Loop through action mail operation details and insert into sentmailqueue and sentmaildetails
	WHILE @count > 0 and @counter != @count
		BEGIN
			DECLARE @actionId INT
			DECLARE @accountId INT
			DECLARE @actionMailOperationId INT
			DECLARE @tokenGuid UNIQUEIDENTIFIER
			DECLARE @from VARCHAR(125)
			DECLARE @mailOperationId INT
			DECLARE @subject NVARCHAR(MAX)
			DECLARE @body NVARCHAR(MAX)
			DECLARE @to TABLE (Id INT IDENTITY(1,1), ContactId INT, Email VARCHAR(250), RequestGuid UNIQUEIDENTIFIER)
			DECLARE @scheduleTime DATETIME

			SELECT	@actionId = tam.ActionID,
					@accountId = tam.AccountId, 
					@actionMailOperationId = tam.ActionsMailOperationID ,
					@mailOperationId = tam.MailBulkOperationID
			FROM #tempActionMails tam WHERE RowIndex = @counter

			SELECT @from = [From],  @subject = [Subject], @body = Body 
			FROM MailBulkOperations (NOLOCK) WHERE MailBulkOperationID = @mailOperationId

			SELECT TOP 1 @tokenGuid = LoginToken FROM ServiceProviders (NOLOCK) 
												 WHERE AccountID = @accountId AND 
													   CommunicationTypeID = 1 AND 
													   IsDefault = 1 AND 
													   EmailType = 2
			INSERT INTO @to
			SELECT CE.ContactID, CE.Email, NEWID() FROM ContactEmails CE (NOLOCK)
			INNER JOIN ContactActionMap CAM (NOLOCK) ON CAM.ContactId = CE.ContactID
			WHERE CAM.ActionID = @actionId AND CE.AccountID = @accountId AND CE.IsDeleted = 0 AND CE.IsPrimary = 1 AND CE.EmailStatus IN (0,50,51,52)

			
			IF EXISTS (SELECT 1 FROM ActionsMailOperations (NOLOCK) WHERE ActionsMailOperationID = @actionMailOperationId AND IsScheduled = 1)
				BEGIN
					SELECT @scheduleTime = ActionStartTime FROM Actions (NOLOCK) WHERE ActionID = @actionId
				END
			ELSE
				BEGIN
					SELECT @scheduleTime = GETUTCDATE()
				END

			--SentMailQueue
			INSERT INTO EnterpriseCommunication.dbo.SentMailQueue
			SELECT @tokenGuid, RequestGuid, @from,1, @scheduleTime, GETUTCDATE(), 4, '', GETUTCDATE(), 0
			FROM @to 

			--SentMailDetails
			INSERT INTO EnterpriseCommunication.dbo.SentMailDetails
			SELECT RequestGuid, '', '', Email,'','',@subject, @body, 1, GETUTCDATE(), null, 1 -- change category id
			FROM @to

						
			UPDATE ActionsMailOperations 
			SET IsProcessed = 3
			WHERE ActionsMailOperationID = @actionMailOperationId

			DELETE FROM @to
			SET @counter = @counter + 1
		END

	
END
/*
exec dbo.Process_ActionMailsToContacts
*/

GO


