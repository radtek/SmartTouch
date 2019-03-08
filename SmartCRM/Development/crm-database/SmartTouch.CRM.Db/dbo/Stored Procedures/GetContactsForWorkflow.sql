CREATE PROCEDURE [dbo].[GetContactsForWorkflow]
	@WorkflowId INT,
	@State TINYINT
AS
BEGIN
	DECLARE @WorkflowsTable TABLE ( WorkflowID INT, WorkflowName VARCHAR(100), [Status] INT, AccountID INT, Total INT)
	DECLARE @AccountID INT

	SELECT @AccountID = AccountID FROM Workflows (NOLOCK) WHERE WorkflowID = @WorkflowId
	INSERT INTO @WorkflowsTable 
	SELECT WF.WorkflowID, WF.WorkflowName,WF.Status, WF.AccountID, COUNT(1) OVER()
	FROM dbo.Workflows WF 
	WHERE WF.WorkflowID = @WorkflowID 

    -- Contacts Started
	IF(@State = 1)
		BEGIN
		;WITH 
		FirstAction AS(
			SELECT TOP 1 WA.WorkflowID, WA.WorkflowActionID FROM WorkflowActions WA (NOLOCK)
			WHERE WA.WorkflowID = @WorkflowId AND WA.IsDeleted = 0 AND WA.IsSubAction = 0 AND WA.WorkflowActionTypeID NOT IN (3,11)
			ORDER BY OrderNumber ASC
		),
		ContactsStarted AS(
			SELECT CWA.ContactID
			FROM ContactWorkflowAudit (NOLOCK) CWA
			INNER JOIN FirstAction FA ON FA.WorkflowID = CWA.WorkflowID AND FA.WorkflowActionID = CWA.WorkflowActionID
			)
		SELECT * FROM ContactsStarted
	END
	-- Contacts InProgress
	ELSE IF (@State = 2)
		-- All Contacts
		SELECT CWA.ContactID FROM Workflows(NOLOCK) W
		JOIN ContactWorkflowAudit(NOLOCK) CWA ON W.WorkflowID = CWA.WorkflowID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID
		INNER JOIN WorkflowActionTypes(NOLOCK) WAT ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
		WHERE W.WorkflowID = @WorkflowId AND WAT.WorkflowActionTypeID != 11
		EXCEPT
		-- Finished Contacts
		SELECT CWA.ContactID FROM Workflows(NOLOCK) W
		JOIN ContactWorkflowAudit(NOLOCK) CWA ON W.WorkflowID = CWA.WorkflowID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = CWA.WorkflowActionID
		INNER JOIN WorkflowActionTypes(NOLOCK) WAT ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
		WHERE W.WorkflowID = @WorkflowId AND WAT.WorkflowActionTypeID = 11 --AND WAT.WorkflowActionTypeID IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12)
	-- Contacts Finished
	ELSE IF (@State = 3)
		BEGIN
			;WITH LastAction AS(
			SELECT W.WorkflowID, WA.WorkflowActionID FROM @WorkflowsTable W
			INNER JOIN WorkflowActions WA (NOLOCK) ON W.WorkflowID = WA.WorkflowID	
			INNER JOIN WorkflowActionTypes WAT (NOLOCK) ON WAT.WorkflowActionTypeID = WA.WorkflowActionTypeID
			WHERE WAT.WorkflowActionTypeID = 11 AND W.WorkflowID = @WorkflowID
		),
		ContactsCompleted AS (
			SELECT CWA.ContactID FROM ContactWorkflowAudit (NOLOCK) CWA
			INNER JOIN @WorkflowsTable W ON W.WorkflowID = CWA.WorkflowID
			INNER JOIN LastAction L ON L.WorkflowID = CWA.WorkflowID AND L.WorkflowActionID = CWA.WorkflowActionID
		)
		SELECT * FROM ContactsCompleted
		END
    -- Contacts OptedOut
	ELSE IF (@State = 4)
	BEGIN
		;WITH ContactsOptedOut AS (
			SELECT CR.ContactID FROM dbo.CampaignRecipients (nolock) CR 
					INNER JOIN @WorkflowsTable WF ON WF.WorkflowID = CR.WorkflowID
			WHERE (CR.DeliveryStatus = 113 or CR.HasUnsubscribed = 1) AND CR.AccountID = @AccountID)--  AND C.IsDeleted = 0
		SELECT * FROM ContactsOptedOut
	END
END