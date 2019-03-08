
CREATE PROCEDURE [dbo].[Calc_WorkflowAnalytics]
(
	@WorkflowID INT
)
AS
BEGIN
	
	DECLARE @WorkflowsTable TABLE ( WorkflowID INT, WorkflowName VARCHAR(100), [Status] INT, AccountID INT, Total INT)

	INSERT INTO @WorkflowsTable 
	SELECT WF.WorkflowID, WF.WorkflowName,WF.Status, WF.AccountID, COUNT(1) OVER()
	FROM dbo.Workflows WF 
	WHERE WF.WorkflowID = @WorkflowID   
	
	DELETE FROM dbo.WorkflowAnalytics WHERE WorkflowID = @WorkflowID
	
	;WITH 
		FirstAction AS(
			SELECT TOP 1 WA.WorkflowID, WA.WorkflowActionID FROM WorkflowActions WA (NOLOCK)
			WHERE WA.WorkflowID = @WorkflowID AND WA.IsDeleted = 0 AND WA.IsSubAction = 0 AND WA.WorkflowActionTypeID NOT IN (3,11)
			ORDER BY OrderNumber ASC
		),
		ContactsStarted AS(
			SELECT CWA.WorkflowID, 
			COUNT(DISTINCT CWA.ContactID) 'Started' 
			FROM ContactWorkflowAudit (NOLOCK) CWA
			INNER JOIN FirstAction FA ON FA.WorkflowID = CWA.WorkflowID AND FA.WorkflowActionID = CWA.WorkflowActionID
			GROUP BY CWA.WorkflowID), 
		LastAction AS(
			SELECT W.WorkflowID, WA.WorkflowActionID FROM @WorkflowsTable W
			INNER JOIN WorkflowActions WA (NOLOCK) ON W.WorkflowID = WA.WorkflowID	
			INNER JOIN WorkflowActionTypes WAT (NOLOCK) ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
			WHERE WAT.WorkflowActionTypeID = 11 AND W.WorkflowID = @WorkflowID
		),
		ContactsCompleted AS (
			SELECT CWA.WorkflowID, COUNT(DISTINCT CWA.ContactID) 'Completed' FROM ContactWorkflowAudit (NOLOCK) CWA
			INNER JOIN @WorkflowsTable W ON W.WorkflowID = CWA.WorkflowID
			INNER JOIN LastAction L ON L.WorkflowID = CWA.WorkflowID AND L.WorkflowActionID = CWA.WorkflowActionID
			GROUP BY CWA.WorkflowID
		),
	    ContactsOptedOut AS (
			SELECT CR.WorkflowID, COUNT(DISTINCT CR.ContactID) AS Unsubscribed FROM dbo.CampaignRecipients (nolock) CR 
					INNER JOIN @WorkflowsTable WF ON WF.WorkflowID = CR.WorkflowID AND CR.AccountID = WF.AccountID
			WHERE (CR.DeliveryStatus = 113 or CR.HasUnsubscribed = 1)--  AND C.IsDeleted = 0
			GROUP BY CR.WorkflowID
		)
		

		INSERT INTO dbo.WorkflowAnalytics --(WorkflowID,[Started],[InProgress],[Completed],OptedOut)
		SELECT W.WorkflowID,
		 CS.[Started], 
		CASE WHEN COALESCE(CS.[Started],0) > COALESCE(CC.Completed,0) THEN (COALESCE(CS.[Started],0) - COALESCE(CC.Completed,0)) ELSE 0 END AS 'InProgress' ,
		CASE WHEN COALESCE(CS.[Started],0) > COALESCE(CC.Completed,0) THEN COALESCE(CC.Completed,0) ELSE COALESCE(CS.[Started],0) END AS 'Completed',
		CO.Unsubscribed,GETUTCDATE()
		FROM @WorkflowsTable W
		LEFT JOIN ContactsStarted CS ON W.WorkflowID = CS.WorkflowID
		LEFT JOIN ContactsCompleted CC ON CS.WorkflowID = CC.WorkflowID
		LEFT JOIN ContactsOptedOut CO ON CO.WorkflowID = CS.WorkflowID
		
END

GO


