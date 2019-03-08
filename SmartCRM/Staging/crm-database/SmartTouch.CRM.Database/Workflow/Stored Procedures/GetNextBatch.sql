
CREATE PROC [Workflow].[GetNextBatch]
AS
BEGIN

 --   DECLARE @ResultID INT
	--INSERT INTO dbo.StoreProcExecutionResults(ProcName, AccountID, ParamList)
	--VALUES('GetNextBatch', 0,CONVERT(VARCHAR(23),GETDATE(),121) )
	--SET @ResultID = scope_identity()

	DECLARE @processTime DATETIME;
	SELECT @processTime = GETUTCDATE();

	--Main Table
	SELECT TOP 200 * INTO #TempTrackActions FROM Workflow.TrackActions
	WHERE ActionProcessStatusID = 801 AND ScheduledOn <= @processTime --801 - ReadyToProcess
	ORDER BY ScheduledOn, CreatedOn
	--SELECT TOP 200 * INTO #TempTrackActions FROM Workflow.TrackActions
	--WHERE  ScheduledOn = '2018-03-12 10:04:01.287' --801 - ReadyToProcess
	--ORDER BY ScheduledOn, CreatedOn
		
	--Workflow Paused
	UPDATE TA
	SET TA.ActionProcessStatusID = 804 --Terminated
	FROM #TempTrackActions TA
	JOIN Workflows W ON TA.WorkflowID = W.WorkflowID AND W.Status = 403 --Workflow Paused

	--Workflow Deavtivated On
	UPDATE TA
	SET TA.ActionProcessStatusID = 804 --Terminated
	FROM #TempTrackActions TA
	JOIN Workflows W ON TA.WorkflowID = W.WorkflowID AND W.DeactivatedOn <= @processTime

	--Return Main Set
	SELECT TrackActions.* FROM #TempTrackActions as TrackActions
	
	--Messages
	SELECT TM.* from Workflow.TrackMessages TM (NOLOCK)
	JOIN #TempTrackActions TA ON TA.TrackMessageID = TM.TrackMessageID	
	
	--Workflows
	SELECT W.*
	FROM #TempTrackActions TA
	JOIN Workflows W ON TA.WorkflowID = W.WorkflowID

	--WorkflowCampaignActions
	select A.*, WA.WorkflowActionTypeID from WorkflowCampaignActions A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--WorkflowNotifyUserAction
	select A.*, WA.WorkflowActionTypeID from WorkflowNotifyUserAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId
	
	--Add a Tag
	select A.*, WA.WorkflowActionTypeID from WorkflowTagAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Remove a Tag
	select A.*, WA.WorkflowActionTypeID from WorkflowTagAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Adjust Lead Score
	select A.*, WA.WorkflowActionTypeID from WorkFlowLeadScoreAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Change Life Cycle
	select A.*, WA.WorkflowActionTypeID from WorkFlowLifeCycleAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Update a Field
	select A.*, WA.WorkflowActionTypeID from WorkflowContactFieldAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Assign to a User
	select A.*, WA.WorkflowActionTypeID from WorkFlowUserAssignmentAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Trigger Workflow
	select A.*, WA.WorkflowActionTypeID from TriggerWorkflowAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId

	--Send email
	select A.*, WA.WorkflowActionTypeID from WorkflowEmailNotificationAction A
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join #TempTrackActions TA ON A.WorkflowActionID = TA.ActionId
END



