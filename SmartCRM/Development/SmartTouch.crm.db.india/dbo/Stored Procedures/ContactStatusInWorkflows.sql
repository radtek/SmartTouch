
CREATE PROCEDURE [dbo].[ContactStatusInWorkflows]
	@contactID int,
	@accountId int
AS
BEGIN
	    DECLARE @ContactStatusInWorkflow TABLE ( WorkflowName varchar(75),ContactStatus varchar(50) )
		-- Inprogress workflows
		INSERT INTO @ContactStatusInWorkflow
	    SELECT W.WorkflowName,'Inprogress' AS ContactStatus FROM Workflows(NOLOCK) W
		JOIN ContactWorkflowAudit(NOLOCK) CWA ON W.WorkflowID = CWA.WorkflowID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID
		INNER JOIN WorkflowActionTypes(NOLOCK) WAT ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
		WHERE  WAT.WorkflowActionTypeID != 11 and cwa.ContactID=@contactID And w.accountid=@accountId AND W.IsDeleted=0
		EXCEPT
		SELECT W.WorkflowName,'Inprogress' AS ContactStatus FROM Workflows(NOLOCK) W
		JOIN ContactWorkflowAudit(NOLOCK) CWA ON W.WorkflowID = CWA.WorkflowID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID
		INNER JOIN WorkflowActionTypes(NOLOCK) WAT ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
		WHERE  WAT.WorkflowActionTypeID = 11 and cwa.ContactID=@contactID And w.accountid=@accountId AND W.IsDeleted=0

		-- Completed workflows
		INSERT INTO @ContactStatusInWorkflow
		SELECT W.WorkflowName,'Completed' AS ContactStatus FROM Workflows(NOLOCK) W
		JOIN ContactWorkflowAudit(NOLOCK) CWA ON W.WorkflowID = CWA.WorkflowID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID
		INNER JOIN WorkflowActionTypes(NOLOCK) WAT ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
		WHERE  WAT.WorkflowActionTypeID = 11 and cwa.ContactID=@contactID And w.accountid=@accountId AND W.IsDeleted=0

		SELECT * FROM @ContactStatusInWorkflow
END

/*
	EXEC dbo.ContactStatusInWorkflows 1741720,4218
 */
GO


