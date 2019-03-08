

CREATE  PROCEDURE [dbo].[GetWorkflow]
(
	@WorkflowID INT,
	@IsReport BIT = 1
)
AS
BEGIN

	DECLARE @Workflows TABLE (WorkflowID INT, ParentWorkflwoID INT)
	DECLARE @WorkflowActions TABLE (WorkflowID INT, WorkflowActionID INT)
	;WITH Workflows_CTE AS 
	(
		SELECT WorkflowID, ParentWorkflowID FROM Workflows (NOLOCK) WHERE WorkflowID = @WorkflowID
		UNION ALL
		SELECT W.WorkflowID, W.ParentWorkflowID FROM Workflows_CTE CTE
		INNER JOIN Workflows (NOLOCK) W ON W.WorkflowID = CTE.ParentWorkflowID
		WHERE @IsReport = 1
	)
	
	INSERT INTO @Workflows
	SELECT * FROM Workflows_CTE

	SELECT W.WorkflowID,W.ParentWorkflowID, W.WorkflowName,W.AccountID,W.Status, W.DeactivatedOn,W.IsWorkflowAllowedMoreThanOnce,W.AllowParallelWorkflows,W.RemovedWorkflows,W.CreatedBy,W.CreatedOn,W.ModifiedBy,W.ModifiedOn,W.IsDeleted, WA.Started ContactsStarted, WA.InProgress ContactsInProgress,WA.Completed ContactsFinished, WA.OptedOut ContactsUnsubscribed FROM Workflows (NOLOCK) W
	LEFT JOIN WorkflowAnalytics WA (NOLOCK) ON w.WorkflowID = WA.WorkflowID
	INNER JOIN @Workflows TW ON TW.WorkflowID = W.WorkflowID

	SELECT WT.* FROM WorkflowTriggers WT (NOLOCK)
	INNER JOIN @Workflows TW ON TW.WorkflowID = WT.WorkflowID

	INSERT INTO @WorkflowActions
	SELECT WA.WorkflowID, WA.WorkflowActionID FROM WorkflowActions (NOLOCK) WA
	INNER JOIN @Workflows TW ON TW.WorkflowID = WA.WorkflowID
	WHERE WA.IsDeleted = 0
	ORDER BY WA.WorkflowID DESC, WA.OrderNumber ASC


	SELECT WA.* FROM WorkflowActions (NOLOCK) WA
	INNER JOIN @Workflows TW ON TW.WorkflowID = WA.WorkflowID
	WHERE WA.IsDeleted = 0
	ORDER BY WA.WorkflowID DESC, WA.OrderNumber ASC

	--WorkflowCampaignActions
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkflowCampaignActions A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--WorkflowNotifyUserAction
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkflowNotifyUserAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID
	
	--Add a Tag
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkflowTagAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Adjust Lead Score
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkFlowLeadScoreAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Change Life Cycle
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkFlowLifeCycleAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Update a Field
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkflowContactFieldAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Assign to a User
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkFlowUserAssignmentAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Trigger Workflow
	SELECT A.*, WA.WorkflowActionTypeID FROM TriggerWorkflowAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Send email
	SELECT A.*, WA.WorkflowActionTypeID FROM WorkflowEmailNotificationAction A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID	
	join @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID

	--Set timer
	SELECT A.*, WA.WorkflowActionTypeID  FROM WorkflowTimerActions A (NOLOCK)
	JOIN WorkflowActions WA ON A.WorkflowActionID = WA.WorkflowActionID
	JOIN @WorkflowActions TA ON A.WorkflowActionID = TA.WorkflowActionID
	
END
/*
	EXEC dbo.GetWorkflow 1422,1
*/