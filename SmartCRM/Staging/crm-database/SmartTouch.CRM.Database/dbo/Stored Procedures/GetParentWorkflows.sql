CREATE PROCEDURE [dbo].[GetParentWorkflows]
	@WorkflowId int
AS
BEGIN
	DECLARE @ParentWorkflow TABLE (WorkflowID INT, ParentWorkflowID INT,WorkflowName varchar(75),ModifiedOn datetime,Status varchar(50))
	;WITH Workflows_CTE AS 
	(
		SELECT WF.WorkflowID, WF.ParentWorkflowID,WF.WorkflowName,WF.ModifiedOn,SS.Name AS Status FROM Workflows WF (NOLOCK) 
		INNER JOIN  Statuses (NOLOCK) SS ON WF.Status = SS.StatusID
		WHERE WorkflowID = @WorkflowId
		UNION ALL
		SELECT W.WorkflowID, W.ParentWorkflowID,W.WorkflowName,W.ModifiedOn,S.Name AS Status FROM Workflows_CTE CTE
		INNER JOIN Workflows (NOLOCK) W ON W.WorkflowID = CTE.ParentWorkflowID
		INNER JOIN Statuses (NOLOCK) S ON W.Status = S.StatusID
	)
	
	INSERT INTO @ParentWorkflow
	SELECT * FROM Workflows_CTE

	SELECT * FROM @ParentWorkflow
END

/*
	EXEC [dbo].[GetParentWorkflows] 1448
*/