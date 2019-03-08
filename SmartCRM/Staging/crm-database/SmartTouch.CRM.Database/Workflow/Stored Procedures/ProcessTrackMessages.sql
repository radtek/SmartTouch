CREATE  PROC [Workflow].[ProcessTrackMessages]
@reminder TINYINT
as
begin
	--local variables
	DECLARE @trackMessageID BIGINT
	DECLARE @messageID	UniqueIdentifier
	DECLARE @leadScoreConditionType	tinyint
	DECLARE @entityID	int
	DECLARE @linkedEntityID	int
	DECLARE @contactID	int
	DECLARE @userID	int
	DECLARE @accountID	int
	DECLARE @conditionValue	NVARCHAR(MAX)
	DECLARE @isSubActionTrigger bit
	DECLARE @counter int = 0
	DECLARE @Crid int = 0
	

	declare @workflowTriggers table
	(
		WorkflowTriggerID INT,
		Entityid INT,
		LeadScoreConditionType INT, 
		WorkflowID INT,
		WorkflowStatus INT,
		IsStartTrigger BIT,
		IsSubActionTrigger BIT
	);

	declare @workflowProcessStatus table
	(
		WorkflowID INT,
		CreatedOn DATETIME,
		IsWorkflowAllowedMoreThanOnce BIT,
		AllowedParallelWorkflows BIT,
		IsProcessed INT
	);

	--Track Message Status (701 - ReadyToProcess 702 - Ignored 703 - Processed 704 - Error)
	--Worfklow Status (401-Active, 402-Draft, 403-Paused, 404-InActive)
	--Worfklow Action Status (801 - ReadyToProcess 802 - Executed 803 - Termintaed 804 - Paused 805 - SubWorkflow)

	--get first TrackMessageId	
	SELECT TOP 1 @trackMessageID = TrackMessageID, @messageID = MessageID, @leadScoreConditionType = LeadScoreConditionType 
		,@entityID = EntityID, @linkedEntityID = LinkedEntityID, @contactID = ContactID, @accountID = AccountID, @conditionValue = ConditionValue
		from Workflow.TrackMessages(NOLOCK) where MessageProcessStatusID = 701 and (TrackMessageID % 3) = @reminder AND LeadScoreConditionType !=14
		ORDER BY TrackMessageID	
		--check if current message is sub action trigger
		

BEGIN TRY 
WHILE( @trackMessageID is not null AND @counter <1000)
BEGIN
	delete from @workflowProcessStatus;
	delete from @workflowTriggers;
	 if object_id('tempdb..#TerminateTrackMessagesWorkflow')is not null
	 Begin
	 drop table #TerminateTrackMessagesWorkflow
	 end
	
	SET @isSubActionTrigger =0
	IF @leadScoreConditionType = 2 AND @linkedEntityID > 0
		BEGIN
		    SET @Crid = @conditionValue
			SET @isSubActionTrigger = 1
		END

	--Get workflows Triggers
	;WITH WActiveTriggers 
	AS(
		SELECT WAT.WorkflowTriggerID, 
		COALESCE(Campaignid, Formid, Lifecycledropdownvalueid, TagID, Searchdefinitionid, OpportunitystageID, Leadadapterid, LeadScore, Duration, ActionType,TourType) as Entityid,
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
			--WHEN TriggerTypeID = 11 THEN 5
			--WHEN Len(WebPage) > 0 OR (IsAnyWebPage = 1)  then 26  --ContactVisitsWebPage
			WHEN TriggerTypeID = 11 AND Duration > 0 THEN 26
			WHEN TriggerTypeID = 11 AND Duration = 0 THEN 5
			WHEN TriggerTypeID = 12 THEN 29
			WHEN TriggerTypeID = 13 THEN 32
		ELSE NULL
		END AS LeadScoreConditionType
		, WAT.WorkflowID
		, W.Status as WorkflowStatus
		, WAT.IsStartTrigger, WAT.WebPage,
		WAT.Duration, WAT.DurationOperator, WAT.IsAnyWebPage
		FROM WorkflowTriggers WAT (NOLOCK)
		JOIN Workflows W (NOLOCK) ON WAT.WorkflowID = W.WorkflowId AND W.IsDeleted = 0
		where W.AccountId = @accountID 
	), ActiveTriggers as
	(
		select *, 0 IsSubActionTrigger from WActiveTriggers where EntityId = @entityID and LeadScoreConditionType = @leadScoreConditionType
		UNION
		SELECT *, 0 IsSubActionTrigger FROM WActiveTriggers WHERE   EntityID <= @entityID AND LeadScoreConditionType =  @leadScoreConditionType AND @leadScoreConditionType = 24
		UNION
		SELECT WT.WorkflowTriggerID,WT.CampaignID,2 LeadScoreConditionType,W.WorkflowID
		, W.Status as WorkflowStatus
		, WT.IsStartTrigger, '', 0,'',0 , 1 IsSubActionTrigger FROM WorkflowCampaignActionLinks(NOLOCK) WACL
		INNER JOIN WorkflowCampaignActions(NOLOCK) WAC ON WAC.WorkflowCampaignActionID = WACL.ParentWorkflowActionID
		INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = WAC.WorkflowActionID
		INNER JOIN Workflows(NOLOCK) W ON W.WorkflowID = WA.WorkflowID
		INNER JOIN WorkflowTriggers(NOLOCK) WT ON WT.WorkflowID = w.WorkflowID
		WHERE WACL.LinkID = @linkedEntityID AND WAC.CampaignID=@entityID
		UNION
		SELECT *,0 IsSubActionTrigger FROM WActiveTriggers WHERE LeadScoreConditionType =  @leadScoreConditionType AND @leadScoreConditionType = 26
		 AND ((IsAnyWebPage = 0 AND (
			(DurationOperator =1 AND @linkedEntityID < Duration AND WebPage = @conditionValue ) OR
			(DurationOperator =2 AND @linkedEntityID = Duration AND WebPage = @conditionValue ) OR
			(DurationOperator =3 AND @linkedEntityID > Duration AND WebPage = @conditionValue )
			)) OR (IsAnyWebPage = 1 AND (
			(DurationOperator =1 AND @linkedEntityID < Duration) OR
			(DurationOperator =2 AND @linkedEntityID = Duration) OR
			(DurationOperator =3 AND @linkedEntityID > Duration)
			)))
			UNION
		SELECT *, 0 IsSubActionTrigger FROM WActiveTriggers WHERE @leadScoreConditionType = 25 and WorkflowID = @entityID
	)
	INSERT INTO @workflowTriggers (WorkflowTriggerID, Entityid, LeadScoreConditionType, WorkflowID, WorkflowStatus, IsStartTrigger, IsSubActionTrigger)
	SELECT WorkflowTriggerID, Entityid, LeadScoreConditionType, WorkflowID, WorkflowStatus, IsStartTrigger, IsSubActionTrigger FROM ActiveTriggers
	

	---workflow end State Reached
	select WA.WorkflowID,tm.ContactID,tm.MessageID,wa.ActionID into #TerminateTrackMessagesWorkflow FROM Workflow.TrackActions WA
	JOIN Workflow.TrackMessages (NOLOCK) TM ON WA.TrackMessageID = TM.TrackMessageID
	JOIN @workflowTriggers AT ON WA.WorkflowId = AT.WorkflowId and AT.IsStartTrigger = 0 and AT.IsSubActionTrigger = 0
	where TM.ContactID = @contactID 
	and WA.ActionProcessStatusID in( 801, 805) --ReadyToProcess && SubWorkflow
	AND AT.WorkflowStatus = 401

	--Workflow Terminate
	update WA
		SET WA.ActionProcessStatusID = 803 --Termintaed
	FROM Workflow.TrackActions WA
	JOIN Workflow.TrackMessages (NOLOCK) TM ON WA.TrackMessageID = TM.TrackMessageID
	JOIN @workflowTriggers AT ON WA.WorkflowId = AT.WorkflowId and AT.IsStartTrigger = 0 and AT.IsSubActionTrigger = 0
	where TM.ContactID = @contactID 
	and WA.ActionProcessStatusID in( 801, 805) --ReadyToProcess && SubWorkflow
	AND AT.WorkflowStatus = 401 --Active

	--Inserting End State Workflow Actions Into ContactWorkflowAudit 
	insert into ContactWorkflowAudit
	select ewf.ContactID,ewf.WorkflowID,ewf.ActionID,GETUTCDATE(),ewf.MessageID from #TerminateTrackMessagesWorkflow ewf
	join WorkflowActions(nolock) wa on wa.WorkflowID=ewf.WorkflowID
	where wa.WorkflowActionTypeID=11

	-- inserting into Refresh analytics
	insert into RefreshAnalytics
	select ewf.WorkflowID,6,1,GETUTCDATE() from #TerminateTrackMessagesWorkflow ewf
	join WorkflowActions(nolock) wa on wa.WorkflowID=ewf.WorkflowID
	where wa.WorkflowActionTypeID=11


	INSERT INTO Workflow.TerminatedActions (TrackActionID, TerminatedActionMessageID)
	SELECT WA.TrackActionID, @trackMessageID FROM Workflow.TrackActions WA (NOLOCK)
	JOIN Workflow.TrackMessages TM (NOLOCK) ON WA.TrackMessageID = TM.TrackMessageID
	JOIN @workflowTriggers AT ON WA.WorkflowId = AT.WorkflowId and AT.IsStartTrigger = 0
	where TM.ContactID = @contactID 
	and WA.ActionProcessStatusID = 801 --ReadyToProcess
	AND AT.WorkflowStatus = 401 --Active
	
	--Workflow Start	
	INSERT INTO @workflowProcessStatus (WorkflowId, CreatedOn, IsWorkflowAllowedMoreThanOnce, AllowedParallelWorkflows, IsProcessed)
	SELECT W.WorkflowId, ModifiedOn, IsWorkflowAllowedMoreThanOnce, AllowParallelWorkflows, cast(0 as BIT) as IsProcessed from Workflows W (NOLOCK)
	JOIN @workflowTriggers AT ON W.WorkflowId = AT.WorkflowId 
	WHERE W.Status in (401) --Active
	AND AT.IsStartTrigger = 1
	UNION
	SELECT W.WorkflowId, W.CreatedOn, W.IsWorkflowAllowedMoreThanOnce, W.AllowParallelWorkflows, cast(0 as BIT) as IsProcessed from Workflow.TrackActions TA (NOLOCK)
	JOIN Workflows W (NOLOCK) ON TA.WorkflowID = W.WorkflowID
	join WorkflowActions (NOLOCK) WA ON W.WorkflowID = WA.WorkflowID
	JOIN TriggerWorkflowAction (NOLOCK) TWA ON WA.WorkflowActionID = TWA.WorkflowActionID
	where TA.ActionProcessStatusID = 805 --Waiting for SubWorkflow
	AND W.Status in (401) --Active

	DECLARE @currentWorkflowId INT, @isWorkflowAllowedMoreThanOnce BIT, @allowedParallelWorkflows BIT
	SELECT TOP 1  @currentWorkflowId = WorkflowID, @isWorkflowAllowedMoreThanOnce = IsWorkflowAllowedMoreThanOnce
		, @allowedParallelWorkflows = AllowedParallelWorkflows from @workflowProcessStatus where IsProcessed = 0 order by CreatedOn		
	
	WHILE @currentWorkflowId IS NOT NULL
	BEGIN
		DECLARE @isAllowed BIT = 1, @WorklflowTriggerType TINYINT
		SELECT @WorklflowTriggerType = triggertypeid FROM WorkflowTriggers WHERE WorkflowiD = @currentWorkflowId and IsStartTrigger = 1
		--workflows with saved search as trigger should always allow once
		IF @leadScoreConditionType = 14
			BEGIN
				SET @isWorkflowAllowedMoreThanOnce = 0
			END
		IF @isWorkflowAllowedMoreThanOnce = 0 --Check for previous Instances
		BEGIN
			IF (EXISTS ((SELECT 1 FROM ContactWorkflowAudit CWA (NOLOCK) 
							INNER JOIN WorkflowActions WA (NOLOCK) ON WA.WorkflowActionID = CWA.WorkflowActionID AND WA.WorkflowID = CWA.WorkflowID
							WHERE CWA.ContactId = @contactID and CWA.WorkflowId = @currentWorkflowId AND WA.IsSubAction = 0)) OR
			(Exists (SELECT 1 FROM Workflow.TrackActions TA (NOLOCK) JOIN Workflow.TrackMessages TM (NOLOCK) ON TA.TrackMessageID = TM.TrackMessageID
							INNER JOIN WorkflowActions WA (Nolock) ON WA.WorkflowActionID = TA.ActionID
							WHERE ContactId = @contactID and TA.WorkflowId = @currentWorkflowId AND WA.IsSubAction = 0)))
			BEGIN
				SET @isAllowed = 0; --Has entered into workflow previously
			END
		END
		IF (@isAllowed <> 0 AND @allowedParallelWorkflows = 0 AND @isSubActionTrigger = 0) --Check for other running workflows
		BEGIN
			IF (Exists (SELECT 1 FROM Workflow.TrackActions TA (NOLOCK) JOIN Workflow.TrackMessages TM (NOLOCK) ON TA.TrackMessageID = TM.TrackMessageID
			WHERE ContactId = @contactID and WorkflowId = @currentWorkflowId AND TA.ActionProcessStatusID = 801 )) --Pending to execute
			BEGIN
				SET @isAllowed = 0; --Has entered into workflow previously
			END
		END

		IF @isSubActionTrigger = 1 AND @isWorkflowAllowedMoreThanOnce = 0
			BEGIN
				IF (@Crid != 0)
				BEGIN
					DECLARE @CampaignStatus INT, @CampaignId INT

					SELECT @CampaignId = C.CampaignID, @CampaignStatus = CampaignStatusID FROM CampaignRecipients(NOLOCK) CR
					JOIN Campaigns (NOLOCK) C ON C.CampaignID = CR.CampaignID
					 WHERE CampaignRecipientID = @Crid 

					IF (@CampaignStatus = 107) --Active Campaign (Sent through workflow action)
						BEGIN
							IF NOT EXISTS(SELECT 1 FROM WorkflowCampaignActionLinks(NOLOCK) WACL
									INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = WACL.LinkActionID
									INNER JOIN ContactWorkflowAudit (NOLOCK) CWA ON CWA.WorkflowID = WA.WorkflowID AND CWA.WorkflowActionID = WA.WorkflowActionID AND CWA.ContactID = @contactID
									WHERE WACL.LinkID = @linkedEntityID AND WA.WorkflowID = @currentWorkflowId AND WA.IsDeleted = 0)
							   AND EXISTS
									(SELECT 1 FROM WorkflowCampaignActionLinks(NOLOCK) WACL
									INNER JOIN WorkflowCampaignActions (NOLOCK) WCA ON WCA.WorkflowCampaignActionID = WACL.ParentWorkflowActionID
									INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = WCA.WorkflowActionID
									INNER JOIN ContactWorkflowAudit (NOLOCK) CWA ON CWA.WorkflowID = WA.WorkflowID AND CWA.WorkflowActionID = WA.WorkflowActionID AND CWA.ContactID = @contactID
									INNER JOIN CampaignRecipients (NOLOCK) CR ON CR.WorkflowID = CWA.WorkflowID AND CR.ContactID = CWA.ContactID
									WHERE WA.WorkflowID = @currentWorkflowId AND WA.IsDeleted = 0 AND CR.CampaignRecipientID = @Crid)	--To check if the link clicked track message is allowed for this workflow
								SET @isAllowed = 1;
							ELSE
								SET @isAllowed = 0;
						END
					 ELSE         --Normal campaign 
						BEGIN
							IF NOT EXISTS(SELECT 1 FROM WorkflowCampaignActionLinks(NOLOCK) WACL
								INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = WACL.LinkActionID
								INNER JOIN ContactWorkflowAudit (NOLOCK) CWA ON CWA.WorkflowID = WA.WorkflowID AND CWA.WorkflowActionID = WA.WorkflowActionID AND CWA.ContactID = @contactID
								WHERE WACL.LinkID = @linkedEntityID AND WA.WorkflowID = @currentWorkflowId AND WA.IsDeleted = 0)
								SET @isAllowed = 1;
							ELSE
								SET @isAllowed = 0;
						END
				END
				ELSE 
				BEGIN
					IF NOT EXISTS(SELECT 1 FROM WorkflowCampaignActionLinks(NOLOCK) WACL
								INNER JOIN WorkflowActions(NOLOCK) WA ON WA.WorkflowActionID = WACL.LinkActionID
								INNER JOIN ContactWorkflowAudit (NOLOCK) CWA ON CWA.WorkflowID = WA.WorkflowID AND CWA.WorkflowActionID = WA.WorkflowActionID AND CWA.ContactID = @contactID
								WHERE WACL.LinkID = @linkedEntityID AND WA.WorkflowID = @currentWorkflowId AND WA.IsDeleted = 0)
					SET @isAllowed = 1;
					ELSE
					SET @isAllowed = 0;
				
				END
			END
		--Check if workflow is removed in other worklfows
		--DECLARE @removedWorkflows VARCHAR(MAX) 
		--SELECT @removedWorkflows = COALESCE(@removedWorkflows+',','') + CAST(WorkflowID AS VARCHAR(100)) FROM Workflows (NOLOCK) WHERE RemovedWorkflows = CAST(@currentWorkflowId AS VARCHAR(250))

		--IF EXISTS(SELECT 1 FROM ContactWorkflowAudit CWA (NOLOCK) WHERE ContactID = @contactID AND WorkflowId IN (SELECT DataValue FROM dbo.Split(@removedWorkflows,',')))
		--			OR EXISTS(SELECT 1 FROM Workflow.TrackActions TC (NOLOCK) WHERE TrackMessageID = @trackMessageID AND  WorkflowId IN (SELECT DataValue FROM dbo.Split(@removedWorkflows,',')))
		--	BEGIN
		--		SET @isAllowed = 0
		--	END
		--Call Execute Proc
		IF @isAllowed = 1
		BEGIN		
			IF @isSubActionTrigger = 0
			BEGIN
				INSERT INTO workflow.AutomationProcessLog VALUES (@currentWorkflowId, @trackMessageID,'In If ' + IIF(@isSubActionTrigger = 1,'TRUE','FALSE') +' ' + Cast(@leadScoreConditionType as varchar(10)), GETUTCDATE())
				EXECUTE Workflow.triggerWorkflow @currentWorkflowId, @trackMessageID
			END
			ELSE
			BEGIN
				--TODO
				--Handle Sub Workflow Actions
				INSERT INTO workflow.AutomationProcessLog VALUES (@currentWorkflowId, @trackMessageID,'In Else ' + IIF(@isSubActionTrigger = 1,'TRUE','FALSE') +' ' + Cast(@leadScoreConditionType as varchar(10)), GETUTCDATE())
				EXECUTE [Workflow].[triggerSubActions] @currentWorkflowId, @trackMessageID
			END
		END
		declare @isProcessed int
		if @isAllowed = 0
			set @isProcessed = 2
		else
			set @isProcessed = 1
		--Take next item
		UPDATE @workflowProcessStatus SET IsProcessed = @isProcessed WHERE WorkflowID = @currentWorkflowId
		SET @currentWorkflowId = NULL
		SELECT TOP 1 @currentWorkflowId = WorkflowID, @isWorkflowAllowedMoreThanOnce = IsWorkflowAllowedMoreThanOnce
			,@allowedParallelWorkflows = AllowedParallelWorkflows from @workflowProcessStatus where IsProcessed = 0 order by CreatedOn
	END	 
		
	update Workflow.TrackMessages set MessageProcessStatusID = case when (select count(1) from @workflowProcessStatus where IsProcessed = 1) > 0 
		then 703 else 702 end where TrackMessageId = @trackMessageID
	SET @counter = @counter + 1
	SET @trackMessageID = null
	SELECT TOP 1 @trackMessageID = TrackMessageID, @messageID = MessageID, @leadScoreConditionType = LeadScoreConditionType 
	,@entityID = EntityID, @linkedEntityID = LinkedEntityID, @contactID = ContactID, @accountID = AccountID, @conditionValue = ConditionValue
	from Workflow.TrackMessages (NOLOCK) where MessageProcessStatusID = 701  and (TrackMessageID % 3) = @reminder AND LeadScoreConditionType !=14
	ORDER BY TrackMessageID	
	
	PRINT(@trackMessageID)
	PRINT(GETUTCDATE())
	if @trackMessageID is null break;	
END
END TRY
BEGIN CATCH
	declare @errorMessage varchar(500)
	select @errorMessage = cast(ERROR_NUMBER() as varchar(400)) + '::' 
		+ cast(ERROR_SEVERITY() as varchar(400)) + '::' + cast(ERROR_STATE() as varchar(400)) + '::' + ERROR_PROCEDURE() + '::' 
		+ cast(ERROR_LINE() as varchar(400)) + '::' + ERROR_MESSAGE() ;
	update Workflow.TrackMessages set MessageProcessStatusID = 704 where TrackMessageId = @trackMessageID --Failed
	insert into Workflow.TrackMessageLogs (TrackMessageID, ErrorMessage) values(@trackMessageID, @errorMessage);

	INSERT INTO CRMLogs.dbo.CRMDBLogs(UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
	VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())
END CATCH

END

















