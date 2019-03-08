

CREATE PROCEDURE [dbo].[GET_WORKFLOW_SUMMARY]
	(
		@NAME	VARCHAR(MAX),
		@ID  INT
	)
AS
BEGIN	
		SELECT WF.WorkflowID, WF.AcCOUNTID,WF.CReatedBY,WF.CReatedON,WF.Status,
               WF.WorkflowName,WF.DeactivatedON,WF.IsWorkfloWAllowedMoreThanONce,
               WF.RemovedWorkflows,WF.AllowParallelWorkflows,WF.ModifiedON
		FROM dbo.Workflows(NOLOCK) WF 
		WHERE WF.AcCOUNTID = @ID AND WF.WorkflowName LIKE @NAME AND WF.IsDeleted != 1
		ORDER BY WF.ModifiedON DESC

   --QUERY TO FETCH CONTACTS STARTED

		SELECT WF.WorkflowID, COUNT(DISTINCT CWA.ContactID) AS ContactsCount FROM dbo.ContactWorkflowAudit(NOLOCK) CWA 
			   INNER JOIN dbo.WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID 
			   INNER JOIN dbo.Workflows(NOLOCK) WF ON CWA.WorkflowID = WF.WorkflowID
		WHERE WA.WorkflowActionTypeID != 11 AND WF.AccountID = @ID AND WF.WorkflowName LIKE @NAME AND WF.IsDeleted != 1
		GROUP BY WF.workflowid
		ORDER BY WF.workflowid

	--QUERY TO FETCH CONTACTS FINISHED
	
		SELECT ENDSTATE.WorkflowID, COUNT(DISTINCT ENDSTATE.ContactID) AS ContactsCount FROM(
		      (SELECT DISTINCT WF.workflowid, CWA.cONtactid FROM dbo.CONtactWorkfloWAudit(NOLOCK) CWA 
			    INNER JOIN dbo.workfloWActiONs(NOLOCK) WA ON CWA.workfloWActiONid = WA.workfloWActiONid 
				INNER JOIN dbo.workflows(NOLOCK) WF ON CWA.workflowid = WF.workflowid
				WHERE WA.workfloWActiONtypeid = 11 AND WF.acCOUNTid = @ID AND WF.workflowname LIKE @NAME AND WF.isdeleted != 1) ENDSTATE
											
				INNER JOIN (SELECT CWA.workflowid, CWA.cONtactid FROM dbo.CONtactWorkfloWAudit(NOLOCK) CWA 									
							INNER JOIN dbo.workfloWActiONs(NOLOCK) WA ON CWA.workfloWActiONid = WA.workfloWActiONid 
							WHERE WA.workfloWActiONtypeid != 11) STARTSTATE ON STARTSTATE.CONtactID = ENDSTATE.CONtactID AND STARTSTATE.WorkflowID = ENDSTATE.WorkflowID)
							GROUP BY ENDSTATE.workflowid
							ORDER BY ENDSTATE.WorkflowID

	--QUERY TO FETCH CONTACTS WHO ARE UNSUBSCRIBED

		SELECT CR.WorkflowID, COUNT(DISTINCT CR.ContactID) AS ContactsCount FROM dbo.CampaignRecipients (nolock) CR 
				INNER JOIN dbo.Workflows(NOLOCK) WF ON WF.WorkflowID = CR.WorkflowID
                INNER JOIN dbo.Contacts(NOLOCK) C ON CR.ContactID = C.ContactID AND CR.AccountID = C.AccountID
		WHERE WF.AccountID = @ID AND WF.WorkflowName LIKE @NAME AND WF.IsDeleted != 1 AND (CR.DeliveryStatus = 113 or CR.HasUnsubscribed = 1) AND CR.AccountID = @ID  AND C.IsDeleted = 0
	    GROUP BY CR.WorkflowID

END







GO


