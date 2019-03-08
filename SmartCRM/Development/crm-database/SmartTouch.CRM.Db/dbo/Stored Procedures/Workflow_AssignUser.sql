

CREATE PROCEDURE [dbo].[Workflow_AssignUser]
	@userAssignmentActionID INT,
	@ScheduledID TINYINT,
	@ContactID INT,
	@workflowID INT
AS
BEGIN

IF(NOT EXISTS(SELECT * FROM WorkflowUserAssignmentAudit (NOLOCK) WUA JOIN RoundRobinContactAssignment(NOLOCK) RR ON RR.WorkFlowUserAssignmentActionID = WUA.WorkflowUserAssignmentActionID
 WHERE WUA.WorkflowUserAssignmentActionID = @userAssignmentActionID AND ContactID = @ContactID AND RR.IsRoundRobinAssignment = 1))
BEGIN
	DECLARE @datePart TINYINT,
			@isRoundRobinAssignment BIT,
			@userIds VARCHAR(100),
			@usersCount INT = 0,
			@userIndex INT = 0,
			@userID INT,
			@contactsCount INT = 0
	DECLARE @EntitiesTable TABLE
	(
		EntitiesTableID INT IDENTITY(1,1),
		ID INT
	)
	BEGIN
		SELECT @datePart = (DATEPART(dw,GETDATE()) - 1)
		IF (@datePart = 0)
		   SELECT @datePart = 7
		IF (@ScheduledID = 2)
			IF (@datePart > 5)
				SELECT @datePart = 9
			ELSE
				SELECT @datePart = 8
		ELSE IF (@ScheduledID = 1)
			SELECT @datePart = 10			 
		SELECT @isRoundRobinAssignment = IsRoundRobinAssignment, @userIds = UserID FROM RoundRobinContactAssignment (NOLOCK)
		WHERE WorkFlowUserAssignmentActionID = @userAssignmentActionID AND [DayOfWeek] = @datePart

		INSERT INTO @EntitiesTable
		SELECT DataValue FROM dbo.Split_2(@userIds,',') SP
		INNER JOIN Users (NOLOCK) U ON U.UserID = DataValue
		WHERE U.IsDeleted = 0 AND U.Status = 1

		IF (@isRoundRobinAssignment = 1)
		BEGIN
			SELECT @usersCount = COUNT(*) FROM @EntitiesTable
			SELECT @contactsCount = COUNT(ContactID) FROM dbo.WorkflowUserAssignmentAudit(NOLOCK) WHERE WorkflowUserAssignmentActionID = @userAssignmentActionID
			       AND [DayOfWeek] = @datePart
			SELECT @userIndex = @contactsCount % @usersCount
			SELECT @userID = ID FROM @EntitiesTable ORDER BY EntitiesTableID ASC OFFSET @userIndex ROWS FETCH NEXT 1 ROW ONLY;
		END
		ELSE
		BEGIN
			SELECT TOP 1 @userID = ID FROM @EntitiesTable
		END

		IF (@userID != 0 AND @userID != '' AND @userID IS NOT NULL)
		BEGIN
			UPDATE CONTACTS
			SET OwnerID = @userID WHERE CONTACTID = @contactID

			INSERT INTO dbo.WorkflowUserAssignmentAudit(ContactID, UserID, WorkflowUserAssignmentActionID, [DayOfWeek])
			SELECT @ContactID, @userID, @userAssignmentActionID, @datePart
	
			INSERT INTO Notifications(Details, EntityID, [Subject], NotificationTime, [Status], UserID, ModuleID)
			SELECT 'A contact has been assigned to you.', @contactID, 'A contact has been assigned to you.', GETUTCDATE(), 1, @userID, 3

			INSERT INTO IndexData(ReferenceID, EntityID, IndexType, [Status], CreatedOn)
			SELECT NEWID(), @contactID, 1, 1, GETUTCDATE() 
		END
	END
	END
END