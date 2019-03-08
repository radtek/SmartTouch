-- =============================================
-- Author:		Avinash Reddy M
-- Create date: Feb 6th 2017
-- Description:	SP to find out if a contact matches a end trigger of a workflow
-- =============================================
CREATE PROCEDURE [dbo].[CheckIfMatchedEndTrigger] 
	@ContactID INT,
	@WorkflowID INT,
	@trackMessageID BIGINT
AS
BEGIN
	
	DECLARE @HasMatched BIT = 0
    DECLARE @LCT TINYINT
	DECLARE @AccountID INT
	DECLARE @SearchDefinitionIDs VARCHAR(256) = ''
	DECLARE @Message VARCHAR(50)=''
    DECLARE @workflowTriggers table
	(
		WorkflowTriggerID INT,
		AccountID INT,
		Entityid INT,
		LeadScoreConditionType INT, 
		WorkflowID INT,
		SelectedLinks VARCHAR(256),
		WebPage VARCHAR(600),
		Duration SMALLINT,
		DurationOperator TINYINT,
		IsAnyWebPage BIT
	);

	   SELECT @Message= MessageID FROM Workflow.TrackMessages(NOLOCK) WHERE TrackMessageID=@trackMessageID

		INSERT INTO @workflowTriggers
		SELECT WAT.WorkflowTriggerID, W.AccountID,
		COALESCE(Campaignid, Formid, Lifecycledropdownvalueid, TagID, Searchdefinitionid, OpportunitystageID, Leadadapterid, LeadScore, Duration) as Entityid,
		CASE
			WHEN TriggerTypeID = 1 THEN 14
			WHEN TriggerTypeID = 2 THEN 3
			WHEN TriggerTypeID = 3 THEN 12
			WHEN TriggerTypeID = 4 THEN 10
			WHEN TriggerTypeID = 5 THEN 18
			WHEN TriggerTypeID = 6 THEN 16
			WHEN TriggerTypeID = 7 THEN 2
			WHEN TriggerTypeID = 9 THEN 22
			WHEN TriggerTypeID = 10 THEN 24
			WHEN TriggerTypeID = 11 AND Duration > 0 THEN 26
			WHEN TriggerTypeID = 11 AND Duration = 0 THEN 5
		ELSE NULL
		END AS LeadScoreConditionType
		, SelectedLinks
		, WAT.WorkflowID
		, WAT.WebPage,
		WAT.Duration, WAT.DurationOperator, WAT.IsAnyWebPage
		FROM WorkflowTriggers WAT (NOLOCK)
		JOIN Workflows W (NOLOCK) ON WAT.WorkflowID = W.WorkflowId AND W.IsDeleted = 0
		where w.WorkflowID = @WorkflowID AND WAT.IsStartTrigger = 0 AND W.Status IN (401, 405)
	
	DECLARE @removedWorkflows VARCHAR(MAX) 
	DECLARE @noOtherWorkflows VARCHAR(MAX)
	DECLARE @LCT_Copy INT
	DECLARE @ContactValue INT = 0
	DECLARE @WorkflowValue INT = 0

	SELECT @LCT = LeadScoreConditionType, @AccountID = AccountID FROM @workflowTriggers
	
	SET @LCT_Copy = @LCT

	SELECT @removedWorkflows = COALESCE(@removedWorkflows+',','') + CAST(WorkflowID AS VARCHAR(100)) FROM Workflows (NOLOCK) WHERE RemovedWorkflows = CAST(@WorkflowID AS VARCHAR(250))
	SELECT @noOtherWorkflows = COALESCE(@noOtherWorkflows+',','') + CAST(WorkflowID AS VARCHAR(10)) FROM Workflows (NOLOCK) WHERE AllowParallelWorkflows = 2 AND WorkflowID NOT IN (@WorkflowID) AND Status = 401

	IF EXISTS(SELECT 1 FROM ContactWorkflowAudit CWA (NOLOCK) WHERE ContactID = @contactID AND WorkflowId IN (SELECT DataValue FROM dbo.Split(@removedWorkflows,',')))
				OR EXISTS(SELECT 1 FROM Workflow.TrackActions TC (NOLOCK) WHERE TrackMessageID = @trackMessageID AND  WorkflowId IN (SELECT DataValue FROM dbo.Split(@removedWorkflows,',')))
		BEGIN
			SET @LCT = 0
		END
		
	--IF EXISTS(SELECT 1 FROM ContactWorkflowAudit CWA (NOLOCK) WHERE ContactID = @contactID AND WorkflowId IN (SELECT DataValue FROM dbo.Split(@noOtherWorkflows,',')))
	--			OR EXISTS(SELECT 1 FROM Workflow.TrackActions TC (NOLOCK) WHERE TrackMessageID = @trackMessageID AND  WorkflowId IN (SELECT DataValue FROM dbo.Split(@noOtherWorkflows,',')))
	--	BEGIN
	--		SET @LCT = 0
	--	END
	IF NOT EXISTS (SELECT * FROM @workflowTriggers)
	BEGIN
		SET @HasMatched = 1
		SET @LCT = 0
		SET @AccountID = 0
	END
		
	--Contact Submits Form
	ELSE IF (@LCT = 3)
	BEGIN

		SELECT @ContactValue = FS.FormID, @WorkflowValue = WT.Entityid FROM FormSubmissions(NOLOCK) FS
			      JOIN @workflowTriggers WT ON WT.Entityid = FS.FormID WHERE ContactID = @ContactID
		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0
	END

	--Contact Lifecycle Change
	ELSE IF (@LCT = 12)
	BEGIN
		SELECT @ContactValue = C.LifecycleStage, @WorkflowValue = WT.Entityid FROM Contacts(NOLOCK) C
				  JOIN @workflowTriggers WT ON WT.Entityid = C.LifecycleStage WHERE ContactID = @ContactID AND C.AccountID = @AccountID
		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0		
	END

	--Contact Tag Added
	ELSE IF (@LCT = 10)
	BEGIN
		SELECT @ContactValue = CTM.TagID, @WorkflowValue = WT.Entityid FROM ContactTagMap(NOLOCK) CTM
				  JOIN @workflowTriggers WT ON WT.Entityid = CTM.TagID WHERE ContactID = @ContactID AND CTM.AccountID = @AccountID
		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0	
	END

	--Campaign Sent
	ELSE IF (@LCT = 18)
	BEGIN
		SELECT @ContactValue = CampaignID, @WorkflowValue = WT.Entityid FROM CampaignRecipients(NOLOCK) CR
				   JOIN @workflowTriggers WT ON WT.Entityid = CampaignID AND ContactID = @ContactID AND CR.AccountId = @AccountID AND DeliveryStatus IN (111, 114, 115)
		IF (@ContactValue != 0) --Delivered, Sent, Abuse
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0	
	END

	--Opportunity Status Changed
	ELSE IF(@LCT = 16)
	BEGIN
		SELECT @ContactValue = OCM.StageID, @WorkflowValue = WT.Entityid FROM Opportunities (NOLOCK) O 
				  JOIN OpportunityContactMap (NOLOCK) OCM ON OCM.OpportunityID = O.OpportunityID
				  JOIN @workflowTriggers WT ON WT.Entityid = OCM.StageID
				  WHERE OCM.ContactID = @ContactID AND O.IsDeleted = 0
		IF(@ContactValue != 0)
		    SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0	
	END

	--LeadAdapter Submitted
	ELSE IF(@LCT = 22)
	BEGIN
		DECLARE @RefId UNIQUEIDENTIFIER 
		SELECT @RefId = ReferenceID FROM Contacts (NOLOCK) WHERE ContactID = @ContactID AND AccountID = @AccountID

		SELECT @ContactValue = LAM.LeadAdapterAndAccountMapID, @WorkflowValue = WT.Entityid FROM @workflowTriggers WT 
				  JOIN LeadAdapterAndAccountMap (NOLOCK) LAM ON LAM.LeadAdapterAndAccountMapID = WT.Entityid
				  JOIN LeadAdapterJobLogs (NOLOCK) LJL ON LJL.LeadAdapterAndAccountMapID = LAM.LeadAdapterAndAccountMapID
				  JOIN LeadAdapterJobLogDetails (NOLOCK) LJLD ON LJLD.LeadAdapterJobLogID = LJL.LeadAdapterJobLogID
				  WHERE LJLD.ReferenceID = @RefId AND LAM.AccountID = @AccountID
		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0
	END

	--Contact Clicks Link
	ELSE IF(@LCT = 2)
	BEGIN
		DECLARE @LinkIDs VARCHAR(256)
		SELECT @LinkIDs = SelectedLinks FROM @workflowTriggers
		DECLARE @EntitysTable TABLE
		(
			ID INT
		)
		INSERT INTO @EntitysTable
		SELECT DataValue FROM dbo.Split_2(@LinkIDs,',')

		SELECT @ContactValue = CS.CampaignID, @WorkflowValue = WT.Entityid FROM @workflowTriggers WT
				  JOIN CampaignStatistics (NOLOCK) CS ON CS.CampaignID = WT.Entityid AND CS.AccountId = @AccountID
				  JOIN @EntitysTable ET ON ET.ID = CS.CampaignLinkID
 				  JOIN CampaignRecipients (NOLOCK) CR ON CR.CampaignRecipientID = CS.CampaignRecipientID AND CR.AccountId = @AccountID
				  WHERE CS.ContactID = @ContactID AND CS.ActivityType = 2

		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0
	END

	--Leadscore Reached
	ELSE IF (@LCT = 24)
	BEGIN
		SELECT @ContactValue = C.LeadScore, @WorkflowValue = WT.Entityid FROM Contacts (NOLOCK) C
				  JOIN @workflowTriggers WT ON WT.Entityid <= C.LeadScore
				  WHERE ContactID = @ContactID AND C.AccountID = @AccountID

		IF(@ContactValue != 0)
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0
	END
	
	--WebPage Duration
	ELSE IF (@LCT = 26)
	BEGIN
		DECLARE @WebPage VARCHAR(600)
		DECLARE @Duration SMALLINT
		DECLARE @DurationOperator TINYINT
		DECLARE @IsAnyWebPage BIT
		SELECT @WebPage = WebPage, @Duration = Duration, @DurationOperator = DurationOperator, @IsAnyWebPage = IsAnyWebPage FROM @workflowTriggers

		IF(EXISTS(SELECT * FROM contactwebvisits(NOLOCK) CW WHERE 
				  ((@IsAnyWebPage = 0 AND (
				  (@DurationOperator = 1 AND @Duration < Duration AND PageVisited = @WebPage ) OR
				  (@DurationOperator = 2 AND @Duration = Duration AND PageVisited = @WebPage ) OR
				  (@DurationOperator = 3 AND @Duration > Duration AND PageVisited = @WebPage )
				  )) OR (@IsAnyWebPage = 1 AND (
				  (@DurationOperator = 1 AND @Duration < Duration) OR
				  (@DurationOperator = 2 AND @Duration = Duration) OR
				  (@DurationOperator = 3 AND @Duration > Duration)
				  ))) AND ContactID = @ContactID))
			SELECT @HasMatched = 1
		ELSE
			SELECT @HasMatched = 0
	END

	--Contact matches a saved search
	ELSE IF (@LCT = 14)
	BEGIN
		SELECT @SearchDefinitionIDs = @SearchDefinitionIDs + CONVERT(varchar, Entityid) + ','
		FROM @workflowTriggers
		SELECT @SearchDefinitionIDs = LEFT(@SearchDefinitionIDs, LEN(@SearchDefinitionIDs) - 1)
	END
	ELSE IF (@LCT = 0)
	BEGIN
		SET @HasMatched = 1
		SET @LCT = @LCT_Copy
	END

	INSERT INTO EndTriggerMatchAudit (ContactID, WorkflowID, TrackMessageID, LeadScoreConditionType, ContactValue, WorkflowValue, HasMatched)
	VALUES (@ContactID, @WorkflowID, @trackMessageID, @LCT, @ContactValue, @WorkflowValue, @HasMatched)

	if(@HasMatched =1)
	begin
		INSERT INTO ContactWorkflowAudit
		select @ContactID,WA.WorkflowID,WA.WorkflowActionID,GETUTCDATE(),@Message from WorkflowActions(nolock) wa
		join @workflowTriggers wt on wt.WorkflowID=wa.WorkflowID
		where wa.WorkflowActionTypeID=11

	end

	SELECT @HasMatched AS HasMatched, @LCT AS LeadScoreConditionType, @SearchDefinitionIDs AS SearchDefinitionIds, @AccountID AS AccountId

END


