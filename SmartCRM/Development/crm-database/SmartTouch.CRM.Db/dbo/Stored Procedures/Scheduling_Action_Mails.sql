CREATE PROCEDURE [dbo].[Scheduling_Action_Mails]
	@ActionIds VARCHAR(MAX),
	@MarkAsComplete BIT,
	@GroupId UNIQUEIDENTIFIER
	
AS
BEGIN
		
		IF(@MarkAsComplete = 1)
		BEGIN
			INSERT INTO ActionsMailOperations(ActionID,IsScheduled,IsProcessed,MailBulkOperationID,GroupID)
			SELECT A.ActionID,0,1,
			CASE WHEN  A.MailBulkId IS NULL THEN 0
					ELSE A.MailBulkId END AS MailBulkOperationID,@GroupId  FROM Actions(NOLOCK) A WHERE A.ActionID IN(SELECT DATAVALUE FROM dbo.Split(@ActionIds,','))
		END
		ELSE
		BEGIN
			INSERT INTO ActionsMailOperations(ActionID,IsScheduled,IsProcessed,MailBulkOperationID,GroupID)
			SELECT A.ActionID,1,1,
			CASE WHEN  A.MailBulkId IS NULL THEN 0
					ELSE A.MailBulkId END AS MailBulkOperationID,@GroupId  FROM Actions(NOLOCK) A WHERE A.ActionID IN(SELECT DATAVALUE FROM dbo.Split(@ActionIds,','))
		END
		
END