
CREATE proc [Workflow].[triggerWorkflow]
@workflowId INT
,@trackMessageID INT
AS
BEGIN
	declare @workflowAcionProcessStatus table
	(
		SequenceNumber INT
		,WorkflowID	Int
		,ActionID	Int
		,OrderNumber SMALLINT
		,CreatedOn	DateTime
		,ScheduledOn	DateTime		
		,WorkflowActionTriggerTypeID SMALLINT
		,IsProcessed BIT		
	);

	DECLARE @EndStateSequence INT
	SELECT @EndStateSequence = COUNT(1) FROM WorkflowActions WHERE WorkflowId = @workflowId and IsDeleted = 0 

	--Get all actions(except link actions)
	INSERT INTO @workflowAcionProcessStatus	
	SELECT ROW_NUMBER() OVER (order by OrderNumber) SequenceNumber, WorkflowId, WorkflowActionID, OrderNumber, GETUTCDATE(), GETUTCDATE()
		, WorkflowActionTypeID, cast(0 as BIT) as IsProcessed FROM WorkflowActions (NOLOCK)
		WHERE WorkflowId = @workflowId and IsDeleted = 0  AND IsSubAction = 0 

	--End Workflow
	INSERT INTO @workflowAcionProcessStatus	
	SELECT (@EndStateSequence + 1) SequenceNumber, WorkflowId, WorkflowActionID, OrderNumber, GETUTCDATE(), GETUTCDATE()
		, WorkflowActionTypeID, cast(0 as BIT) as IsProcessed FROM WorkflowActions (NOLOCK)
		WHERE WorkflowId = @workflowId and WorkflowActionTypeID = 11 

	--Workflow Action Type Id = 3 (Timer)
	--Process message
	DECLARE @rowNumber INT
	SELECT TOP 1  @rowNumber = SequenceNumber from @workflowAcionProcessStatus where IsProcessed = 0 order by SequenceNumber, CreatedOn
	--get Previous Date
	declare @previouDate datetime = GETUTCDATE();
	WHILE @rowNumber IS NOT NULL
	BEGIN
		PRINT ('[triggerWorkflow]')
		select @previouDate = ScheduledOn from @workflowAcionProcessStatus where  SequenceNumber = @rowNumber-1
		IF EXISTS(SELECT 1 from @workflowAcionProcessStatus where IsProcessed = 0 AND SequenceNumber = @rowNumber AND WorkflowActionTriggerTypeID = 3) --Is Timer
			BEGIN		
				UPDATE A
					SET ScheduledOn = Workflow.getNextExecutionTime(WA.TimerType
					, ISNULL(WA.DelayPeriod,0)
					, ISNULL(WA.DelayUnit,0)
					, ISNULL(WA.RunOn,0)
					, ISNULL(WA.RunAt, GetUtcDate())
					, ISNULL(WA.RunType,0)
					, ISNULL(WA.RunOnDate, GetUtcDate())
					, ISNULL(WA.StartDate, GetUtcDate())
					, ISNULL(WA.EndDate, GetUtcDate())
					, WA.RunOnDay
					, WA.DaysOfWeek
					, @previouDate)
				FROM @workflowAcionProcessStatus A
				JOIN WorkflowTimerActions WA ON A.ActionID = WA.WorkflowActionID
				WHERE IsProcessed = 0 AND SequenceNumber = @rowNumber;
			END
		ELSE
			BEGIN
				UPDATE A
				SET ScheduledOn = @previouDate
				FROM @workflowAcionProcessStatus A
				WHERE IsProcessed = 0 AND SequenceNumber = @rowNumber;
							
			END

		--Take next child item
		UPDATE @workflowAcionProcessStatus SET IsProcessed = 1 WHERE SequenceNumber = @rowNumber
		SET @rowNumber = NULL
		SELECT TOP 1  @rowNumber = SequenceNumber from @workflowAcionProcessStatus where IsProcessed = 0 order by SequenceNumber, CreatedOn
	END

	insert into Workflow.TrackActions(TrackMessageID, WorkflowID, ActionID, ScheduledOn, ExecutedOn, CreatedOn, ActionProcessStatusID, WorkflowActionTypeID)
	select @trackMessageID, WorkflowID, ActionID, ScheduledOn, NULL, CreatedOn, 801,WorkflowActionTriggerTypeID from @workflowAcionProcessStatus 
	--where WorkflowActionTriggerTypeID NOT IN (3) --SubAction & Timer Message (801 - Ready To Process)
END

GO


