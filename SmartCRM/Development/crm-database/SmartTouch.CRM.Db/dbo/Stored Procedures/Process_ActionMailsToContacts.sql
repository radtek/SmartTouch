CREATE PROCEDURE [dbo].[Process_ActionMailsToContacts]
AS
BEGIN
	
	;WITH tempBulkOperationsCTE
	AS
	(
		SELECT DISTINCT AMO.MailBulkOperationID, A.ActionId, A.AccountId,AMO.GroupID
		FROM ActionsMailOperations (nolock) AMO
		INNER JOIN Actions (NOLOCK) A ON A.ActionId = AMO.ActionId
		WHERE CAST( DATEADD(dd, 0, DATEDIFF(dd, 0, ActionDate)) + ' ' + DATEADD(Day, -DATEDIFF(Day, 0, ActionStartTime), ActionStartTime)  AS DATETIME) < GETUTCDATE() 
		AND AMO.IsScheduled = 1 AND AMO.IsProcessed = 1 -- Ready To Process
		UNION
		SELECT DISTINCT AMO.MailBulkOperationID, A.ActionId, A.AccountId,AMO.GroupID
		FROM ActionsMailOperations (nolock) AMO
		INNER JOIN Actions (NOLOCK) A ON A.ActionId = AMO.ActionId
		AND AMO.IsScheduled = 0 AND AMO.IsProcessed = 1 AND AMO.MailBulkOperationID > 0-- Ready To Process
	)

	SELECT * , ROW_NUMBER () OVER (ORDER BY cte.MailBulkOperationID ASC) as RowIndex	
	INTO #tempBulkOperations
	FROM tempBulkOperationsCTE cte

	DECLARE @count INT = 0
	DECLARE @counter INT = 0

	SELECT @count = COUNT(1) FROM #tempBulkOperations
	
	WHILE @counter < @count
		BEGIN
			BEGIN TRY

				DECLARE @actionMails TABLE (Id INT IDENTITY(1,1), ActionId INT,IsScheduled BIT,GroupId UNIQUEIDENTIFIER)
				DECLARE @bulkMailOperationId INT
				DECLARE @accountId INT
				DECLARE @tokenGuid UNIQUEIDENTIFIER
				DECLARE @from VARCHAR(125)
				DECLARE @mailOperationId INT
				DECLARE @subject NVARCHAR(MAX)
				DECLARE @body NVARCHAR(MAX)
				DECLARE @to TABLE (Id INT IDENTITY(1,1), ContactId INT,ContactEmailID INT, Email VARCHAR(250), RequestGuid UNIQUEIDENTIFIER)
				DECLARE @scheduleTime DATETIME
				DECLARE @userId INT
				DECLARE @actionId INT

				SELECT @bulkMailOperationId = MailBulkOperationID, @accountId = AccountID, @actionId = ActionId FROM #tempBulkOperations WHERE RowIndex = (@counter + 1)

				INSERT INTO @actionMails
				SELECT ActionID,0,GroupID FROM #tempBulkOperations (NOLOCK) WHERE ActionID = @actionId

				SELECT @from = [From],  @subject = [Subject], @body = Body 
				FROM MailBulkOperations (NOLOCK) WHERE MailBulkOperationID = @bulkMailOperationId

				SELECT TOP 1 @userId = UserID FROM Users(NOLOCK) WHERE PrimaryEmail=@from AND AccountID IN(1,@accountId) AND IsDeleted=0

				SELECT TOP 1 @tokenGuid = LoginToken 
						FROM ServiceProviders (NOLOCK) 
						WHERE AccountID = @accountId AND 
							CommunicationTypeID = 1 AND 
							IsDefault = 1 AND 
							EmailType = 2

				;WITH UniqueTo
				AS
				(
					SELECT DISTINCT CE.ContactID,CE.ContactEmailID, CE.Email FROM ContactEmails CE (NOLOCK)
					INNER JOIN ContactActionMap CAM (NOLOCK) ON CAM.ContactId = CE.ContactID
					INNER JOIN @actionMails am ON am.ActionId = CAM.ActionID AND 
						ISNULL(AM.GroupId,ISNULL(CAM.GroupID,cast(cast(0 as binary) as uniqueidentifier)) ) = ISNULL(CAM.GroupID,cast(cast(0 as binary) as uniqueidentifier)) 
					WHERE CE.AccountID = @accountId AND CE.IsDeleted = 0 AND CE.IsPrimary = 1 AND CE.EmailStatus IN (0,50,51,52)
				)

				INSERT INTO @to
				SELECT *, NEWID() FROM UniqueTo
			
				SELECT @scheduleTime = GETUTCDATE()
--SentMailQueue
				INSERT INTO EnterpriseCommunication.dbo.SentMailQueue
				SELECT @tokenGuid, RequestGuid, @from,1, @scheduleTime, GETUTCDATE(), 4, '', GETUTCDATE(), 0
				FROM @to 

				--SentMailDetails
				INSERT INTO EnterpriseCommunication.dbo.SentMailDetails
				SELECT RequestGuid, '', '', Email,'','',@subject, @body, 1, GETUTCDATE(), null, 19 ,@accountId 
				FROM @to

				--Updating Lasttouched details
				UPDATE  C
		 			SET  C.LastContactedThrough = 25,
						C.LastContacted = GETUTCDATE() FROM dbo.Contacts C
						JOIN @to t on t.ContactId = C.ContactID
					
				--ContactEmailAudit
				INSERT INTO dbo.ContactEmailAudit
				SELECT ContactEmailID,@userId,GETUTCDATE(),1,RequestGuid FROM @to 

				--IndexData
				INSERT INTO dbo.IndexData
				SELECT NEWID(), ContactId,1,GETUTCDATE(),NULL,1,1 FROM @to 
			
				--UPDATING ACTIONSMAILOPERATIONS		
				UPDATE ActionsMailOperations 
				SET IsProcessed = 3
				WHERE MailBulkOperationID = @bulkMailOperationId

				-- Updating Action Status to Completed to Contact Actions
				--UPDATE CAM
				--SET CAM.IsCompleted=1,CAM.LastUpdatedOn=GETUTCDATE()
				--FROM ContactActionMap CAM 
				--JOIN @actionMails AM ON AM.ActionId = CAM.ActionID AND AM.IsScheduled = 1 AND CAM.IsCompleted = 0

				-- Updating Action Status to Completed to Opportunity Actions
				--UPDATE OAM
				--SET OAM.IsCompleted = 1
				--FROM OpportunityActionMap OAM
				--JOIN @actionMails AM ON AM.ActionId = OAM.ActionID AND AM.IsScheduled = 1  AND OAM.IsCompleted = 0

				DELETE FROM @to
				DELETE FROM @actionMails
				DELETE FROM #tempBulkOperations WHERE ActionID = @actionId
				SET @counter = @counter + 1

		END TRY
		BEGIN CATCH
			--UPDATING ACTIONSMAILOPERATIONS to failed		
				UPDATE ActionsMailOperations 
				SET IsProcessed = 2
				WHERE MailBulkOperationID = @bulkMailOperationId
        END CATCH;
	END
END
/*
exec dbo.Process_ActionMailsToContacts
*/
