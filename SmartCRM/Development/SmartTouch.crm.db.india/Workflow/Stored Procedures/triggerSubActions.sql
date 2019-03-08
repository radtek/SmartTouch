CREATE PROCEDURE [Workflow].[triggerSubActions]
@workflowId INT
,@trackMessageID INT
AS
BEGIN
INSERT INTO workflow.AutomationProcessLog VALUES (@workflowId, @trackMessageID,'In [triggerSubActions] Start', GETUTCDATE())
	DECLARE @linkID INT
	DECLARE @entityID INT
	DECLARE @entityActionID INT
	DECLARE @leadScoreConditionType INT 
	SELECT @linkID = LinkedEntityID, @entityID = EntityID, @leadScoreConditionType = LeadScoreConditionType FROM Workflow.Trackmessages (NOLOCK) WHERE TrackMessageID = @trackMessageID

	IF NOT (@leadScoreConditionType = 2 AND @linkID > 0)
		BEGIN
			EXEC workflow.triggerWorkflow @workflowId, @trackmessageID
			RETURN
		END


	SELECT TOP 1 @entityActionID = WorkflowCampaignActionID FROM WorkflowCampaignActions (NOLOCK) WCA
	INNER JOIN WorkflowActions WA (NOLOCK) ON WA.WorkflowActionID = WCA.WorkflowActionID
	WHERE WA.WorkflowID = @workflowId AND WA.IsDeleted = 0 AND WCA.CampaignID = @entityID 
	
	INSERT INTO Workflow.TrackActions(TrackMessageID, WorkflowID, ActionID, ScheduledOn, ExecutedOn, CreatedOn, ActionProcessStatusID, WorkflowActionTypeID)
	SELECT @trackMessageID, @workflowId, WCAL.LinkActionID, GETUTCDATE(),GETUTCDATE(),GETUTCDATE(),801,WA.WorkflowActionTypeID F
		FROM WorkflowActions(NOLOCK) WA 
		INNER JOIN WorkflowCampaignActionLinks(NOLOCK) WCAL ON WCAL.LinkActionID = WA.WorkflowActionID
		INNER JOIN Workflows (NOLOCK) W ON W.WorkflowID = WA.WorkflowID
		WHERE WCAL.LinkID = @linkID AND WA.WorkflowID = @workflowId AND WA.IsDeleted = 0 AND WCAL.ParentWorkflowActionID = @entityActionID AND W.[Status] = 401
		Order by WA.WorkflowActionID Asc,WA.OrderNumber Asc
		INSERT INTO workflow.AutomationProcessLog VALUES (@workflowId, @trackMessageID,'In [triggerSubActions] End', GETUTCDATE())
END