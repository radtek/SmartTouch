CREATE PROC [Workflow].[ProcessTrackMessages_NotUsing]
as
begin
		--local variables
	DECLARE @trackMessageID BIGINT
	--DECLARE @messageID	UniqueIdentifier
	--DECLARE @leadScoreConditionType	tinyint
	--DECLARE @entityID	int
	--DECLARE @linkedEntityID	int
	DECLARE @contactID	int
	DECLARE @userID	int
	DECLARE @accountID	int
	DECLARE @conditionValue	nvarchar(4000)

	declare @workflowTriggers table
	(
		WorkflowTriggerID INT,
		Entityid INT,
		LeadScoreConditionType INT, 
		WorkflowID INT,
		WorkflowStatus INT,
		IsStartTrigger BIT
	);

	declare @initialData table
	(
		TrackMessageID BIGINT PRIMARY KEY,
		MessageID UniqueIdentifier,
		LeadScoreConditionType TINYINT,
		EntityID int,
		LinkedEntityID INT,
		ContactID INT,
		AccountID INT,
		ConditionValue NVARCHAR(4000),
		IsProcessed BIT default(0),
		MessageProcessStatusID SmallINT default(701),
		CreatedOn DATETIME
	);

	declare @workflowProcessStatus table
	(
		WorkflowID INT,
		CreatedOn DATETIME,
		IsWorkflowAllowedMoreThanOnce BIT,
		AllowedParallelWorkflows BIT,
		IsProcessed BIT
	);

	--Track Message Status (701 - ReadyToProcess 702 - Ignored 703 - Processed 704 - Error)
	--Worfklow Status (401-Active, 402-Draft, 403-Paused, 404-InActive)
	--Worfklow Action Status (801 - ReadyToProcess 802 - Executed 803 - Termintaed 804 - Paused 805 - SubWorkflow)

	--get first Account ID	
	SELECT TOP 1 @accountID = AccountID
		from Workflow.TrackMessages where MessageProcessStatusID = 701 ORDER BY TrackMessageID
	
	print (@accountID)

	INSERT INTO @initialData
	SELECT TOP 2000 TrackMessageID,  MessageID, LeadScoreConditionType 
		, EntityID, LinkedEntityID, ContactID, AccountID, ConditionValue, CAST(0 AS BIT) AS IsProcessed, MessageProcessStatusID, CreatedOn
		from Workflow.TrackMessages where MessageProcessStatusID = 701 and AccountID = @accountID ORDER BY TrackMessageID	

	while EXISTS (SELECT 1 FROM @initialData)
	BEGIN
		DELETE FROM @workflowTriggers;	

		;WITH ActiveTriggers 
		AS(
			SELECT WAT.WorkflowTriggerID, 
			COALESCE(Campaignid, Formid, Lifecycledropdownvalueid, TagID, Searchdefinitionid, OpportunitystageID, Leadadapterid, LeadScore, Duration) as Entityid,
			CASE 
			WHEN CampaignId > 0 and LEN(SelectedLinks) > 0 then 2 --Campaign Clicked
			WHEN CampaignId > 0 and SelectedLinks is null then 18 --Campaign Sent
			WHEN FormId > 0 then 3  --From Submitted
			WHEN LifecycleDropdownValueID > 0 then 12  --Lifecycle Changed
			WHEN tagid > 0 then 10  --Tag Added
			WHEN SearchDefinitionID > 0 then 14  --Saved Search Percolation
			WHEN opportunitystageid > 0 then 16  --Opportunity State Changed
			WHEN leadadapterid > 0 then 22  --Lead Adapter Submitted		
			WHEN LeadScore > 0 then 24  --LeadscoreReached
			WHEN Len(WebPage) > 0 and (Duration > 0)  then 5  --ContactVisitsWebPage		

			ELSE NULL
			END AS LeadScoreConditionType
			, WAT.WorkflowID
			, W.Status as WorkflowStatus
			, WAT.IsStartTrigger
			FROM WorkflowTriggers WAT 
			JOIN Workflows W ON WAT.WorkflowID = W.WorkflowId 
			where W.AccountId = @accountID 
		)
		INSERT INTO @workflowTriggers (WorkflowTriggerID, Entityid, LeadScoreConditionType, WorkflowID, WorkflowStatus, IsStartTrigger)
		SELECT * FROM ActiveTriggers

		--Workflow Terminate
		update WA
			SET WA.ActionProcessStatusID = 803 --Termintaed
		FROM Workflow.TrackActions WA (NOLOCK)
		JOIN Workflow.TrackMessages TM  (NOLOCK) ON WA.TrackMessageID = TM.TrackMessageID
		JOIN @initialData TM1 ON TM1.ContactID = TM.ContactID
		JOIN @workflowTriggers AT ON WA.WorkflowId = AT.WorkflowId and AT.IsStartTrigger = 0	
		WHERE AT.EntityId = TM1.EntityID and AT.LeadScoreConditionType = TM1.leadScoreConditionType	
		--where TM.ContactID = @contactID 
		and WA.ActionProcessStatusID in( 801, 805) --ReadyToProcess && SubWorkflow
		AND AT.WorkflowStatus = 401 --Active

		INSERT INTO Workflow.TerminatedActions (TrackActionID, TerminatedActionMessageID)
		select WA.TrackActionID, TM.TrackMessageID FROM Workflow.TrackActions WA (NOLOCK)
		JOIN Workflow.TrackMessages TM (NOLOCK) ON WA.TrackMessageID = TM.TrackMessageID
		JOIN @initialData TM1 ON TM1.ContactID = TM.ContactID
		JOIN @workflowTriggers AT ON WA.WorkflowId = AT.WorkflowId and AT.IsStartTrigger = 0	
		WHERE AT.EntityId = TM1.EntityID and AT.LeadScoreConditionType = TM1.leadScoreConditionType		
		--where TM.ContactID = @contactID 
		and WA.ActionProcessStatusID in( 803) --Terminated
		AND AT.WorkflowStatus = 401 --Active
		and WA.TrackActionID not in (select TrackActionID from Workflow.TerminatedActions);

		--Process one by one
				--get first TrackMessageId	
		SELECT TOP 1 @trackMessageID = TrackMessageID, @contactID = ContactID, @accountID = AccountID, @conditionValue = ConditionValue
			from @initialData where MessageProcessStatusID = 701 ORDER BY TrackMessageID	
		
		BEGIN TRY 
		WHILE( @trackMessageID is not null )
		BEGIN
			delete from @workflowProcessStatus;	
			--Workflow Start	
			INSERT INTO @workflowProcessStatus (WorkflowId, CreatedOn, IsWorkflowAllowedMoreThanOnce, AllowedParallelWorkflows, IsProcessed)
			SELECT W.WorkflowId, ModifiedOn, IsWorkflowAllowedMoreThanOnce, AllowParallelWorkflows, cast(0 as BIT) as IsProcessed from Workflows W (NOLOCK)
			JOIN @workflowTriggers AT ON W.WorkflowId = AT.WorkflowId 
			WHERE W.Status in (401) --Active
			AND AT.IsStartTrigger = 1
			UNION
			SELECT W.WorkflowId, W.CreatedOn, W.IsWorkflowAllowedMoreThanOnce, W.AllowParallelWorkflows, cast(0 as BIT) as IsProcessed from Workflow.TrackActions TA (NOLOCK)
			join Workflow.TrackMessages tm (NOLOCK) on TM.TrackMessageID = TA.TrackMessageID	
			JOIN Workflows W (NOLOCK) ON TA.WorkflowID = W.WorkflowID
			join WorkflowActions WA (NOLOCK) ON W.WorkflowID = WA.WorkflowID
			JOIN TriggerWorkflowAction (NOLOCK) TWA ON WA.WorkflowActionID = TWA.WorkflowActionID
			where TA.ActionProcessStatusID = 805 --Waiting for SubWorkflow	
			AND W.Status in (401) --Active
			AND TM.ContactID = @contactID

			UPDATE @workflowProcessStatus SET IsProcessed = 0;

			DECLARE @currentWorkflowId INT, @isWorkflowAllowedMoreThanOnce BIT, @allowedParallelWorkflows BIT
			SELECT TOP 1  @currentWorkflowId = WorkflowID, @isWorkflowAllowedMoreThanOnce = IsWorkflowAllowedMoreThanOnce
				, @allowedParallelWorkflows = AllowedParallelWorkflows from @workflowProcessStatus where IsProcessed = 0 order by CreatedOn		
	
			WHILE @currentWorkflowId IS NOT NULL
			BEGIN
				DECLARE @isAllowed BIT = 1;

				IF @isWorkflowAllowedMoreThanOnce = 0 --Check for previous Instances
				BEGIN
					IF (EXISTS ((SELECT 1 FROM ContactWorkflowAudit (NOLOCK) WHERE ContactId = @contactID and WorkflowId = @currentWorkflowId)) OR
					(Exists (SELECT 1 FROM Workflow.TrackActions TA (NOLOCK) JOIN Workflow.TrackMessages TM (NOLOCK) ON TA.TrackMessageID = TM.TrackMessageID
					WHERE ContactId = @contactID and WorkflowId = @currentWorkflowId )))
					BEGIN
						SET @isAllowed = 0; --Has entered into workflow previously
					END
				END
				IF @isAllowed <> 0 AND @allowedParallelWorkflows = 0 --Check for other running workflows
				BEGIN
					IF (Exists (SELECT 1 FROM Workflow.TrackActions TA (NOLOCK) JOIN Workflow.TrackMessages TM (NOLOCK) ON TA.TrackMessageID = TM.TrackMessageID
					WHERE ContactId = @contactID and WorkflowId = @currentWorkflowId AND TA.ActionProcessStatusID = 801 )) --Pending to execute
					BEGIN
						SET @isAllowed = 0; --Has entered into workflow previously
					END
				END

				--Call Execute Proc
				IF @isAllowed = 1
				BEGIN		
					EXECUTE Workflow.triggerWorkflow @currentWorkflowId, @trackMessageID
				END
		
				--Take next item
				UPDATE @workflowProcessStatus SET IsProcessed = 1 WHERE WorkflowID = @currentWorkflowId
				SET @currentWorkflowId = NULL
				SELECT TOP 1 @currentWorkflowId = WorkflowID, @isWorkflowAllowedMoreThanOnce = IsWorkflowAllowedMoreThanOnce
					,@allowedParallelWorkflows = AllowedParallelWorkflows from @workflowProcessStatus where IsProcessed = 0 order by CreatedOn
			END	 
		
			update @initialData set MessageProcessStatusID = case when (select count(1) from @workflowProcessStatus) > 0 
				then 703 else 702 end where TrackMessageId = @trackMessageID

			SET @trackMessageID = null
			SELECT TOP 1 @trackMessageID = TrackMessageID, @contactID = ContactID, @accountID = AccountID, @conditionValue = ConditionValue
				from @initialData where MessageProcessStatusID = 701 ORDER BY TrackMessageID	

			PRINT(@trackMessageID)
			PRINT(GETUTCDATE())
			if @trackMessageID is null break;				
		END
		END TRY
		BEGIN CATCH
			declare @errorMessage varchar(500);
			select @errorMessage = cast(ERROR_NUMBER() as varchar(400)) + '::' 
				+ cast(ERROR_SEVERITY() as varchar(400)) + '::' + cast(ERROR_STATE() as varchar(400)) + '::' + ERROR_PROCEDURE() + '::' 
				+ cast(ERROR_LINE() as varchar(400)) + '::' + ERROR_MESSAGE() ;
			update Workflow.TrackMessages set MessageProcessStatusID = 704 where TrackMessageId = @trackMessageID --Failed
			insert into Workflow.TrackMessageLogs (TrackMessageID, ErrorMessage) values(@trackMessageID, @errorMessage);

			INSERT INTO CRMLogs.dbo.CRMDBLogs(UserName, ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine, ErrorMessage, LogDate)
			VALUES (CONVERT(sysname, CURRENT_USER), ERROR_NUMBER(), ERROR_SEVERITY(), ERROR_STATE(), ERROR_PROCEDURE(), ERROR_LINE(), ERROR_MESSAGE(), GETUTCDATE())
		END CATCH

		Update A
					set A.MessageProcessStatusID = B.MessageProcessStatusID
				from Workflow.TrackMessages A
				JOIN @initialData B ON A.TrackMessageID = B.TrackMessageID;

		delete from @initialData;

		INSERT INTO @initialData
		SELECT TOP 2000 TrackMessageID,  MessageID, LeadScoreConditionType 
			, EntityID, LinkedEntityID, ContactID, AccountID, ConditionValue, CAST(0 AS BIT) AS IsProcessed, MessageProcessStatusID, CreatedOn
			from Workflow.TrackMessages where MessageProcessStatusID = 701 and AccountID = @accountID ORDER BY TrackMessageID	

		IF NOT EXISTS (SELECT 1 FROM @initialData) break;

	END



end