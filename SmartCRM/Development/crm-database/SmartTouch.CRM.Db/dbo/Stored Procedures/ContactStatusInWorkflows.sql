
CREATE PROCEDURE [dbo].[ContactStatusInWorkflows]
	@contactID int,
	@accountId int
AS
BEGIN
	    DECLARE @Allworkflows TABLE (Id INT IDENTITY(1,1), WorkflowID INT, WorkflowLastAction INT, WorkflowStatus INT)

		;WITH AllWorkflowsCTE as (
		SELECT  DISTINCT  W.WorkflowID, SUM(CWA.ContactWorkflowAuditID)  ORDR FROM ContactWorkflowAudit (NOLOCK) CWA
		INNER JOIN Workflows (NOLOCK) W ON CWA.WorkflowID = W.WorkflowID
		WHERE CWA.ContactID = @contactId
		GROUP BY W.WorkflowID)

		INSERT INTO @Allworkflows(WorkflowID, WorkflowLastAction)
		SELECT AC.WorkflowID, WA.WorkflowActionID FROM AllWorkflowsCTE AC
		INNER JOIN WorkflowActions (NOLOCK) WA ON WA.WOrkflowID = AC.WorkflowID
		WHERE WA.WorkflowActionTypeID = 11
		ORDER BY ORDR ASC

		;WITH FinalCTE AS (
		SELECT AW.WorkflowID, (SELECT COUNT(1) FROM ContactWorkflowAudit (NOLOCK) WHERE ContactID = @contactId and WorkflowActionID = AW.WorkflowLastAction) AS S FROM @Allworkflows AW)

		SELECT W.WorkflowName, CASE WHEN S = 0 THEN 'In Progress' ELSE 'Completed' END As ContactStatus  FROM FinalCTE FC
		INNER JOIN Workflows (NOLOCK) W ON W.WorkflowID = FC.WorkflowID
END

/*
	EXEC dbo.ContactStatusInWorkflows 1741720,4218
 */