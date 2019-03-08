
CREATE PROCEDURE [dbo].[ProcessLeadScoreMessages]
AS
BEGIN
	DECLARE @S_ConditionValue VARCHAR(MAX)
	DECLARE @S_EntityId INT = 0
	DECLARE @ConditionValue VARCHAR(MAX)
	DECLARE @EntityId INT = 0
	DECLARE @LeadScoreMessageID UNIQUEIDENTIFIER
	DECLARE @UserID INT
	DECLARE @LeadScoreConditionType INT
	DECLARE @ContactID INT
	DECLARE @AccountID INT
	DECLARE @LinkedEntityID INT
	DECLARE @CreatedOn DATETIME
	DECLARE @ProcessedOn DATETIME
	DECLARE @Remarks VARCHAR(MAX)
	DECLARE @LeadScoreProcessStatusID INT
	DECLARE @LeadScoreRules TABLE (RuleID INT, Score INT, IsAudited BIT)
	DECLARE @RuleCount INT
	DECLARE @LoopCounter INT = 0
	-- Get messages
	SELECT TOP 1 @LeadScoreMessageID = LeadScoreMessageID,
	@UserID = UserID, @LeadScoreConditionType = LeadScoreConditionType, @ContactID = ContactID, @AccountID = AccountID, @LinkedEntityID = LinkedEntityID , @EntityId = EntityID, @ConditionValue = ConditionValue
	FROM LeadScoreMessages (NOLOCK) WHERE LeadScoreProcessStatusID = 701 ORDER BY CreatedOn ASC
	
	
	WHILE @LeadScoreMessageID IS NOT NULL AND @LoopCounter < 1000
	BEGIN
		
		-- set conditionavlue 
		IF @LeadScoreConditionType = 2 -- Contact clicks link
			BEGIN
				SET @S_EntityId = @LinkedEntityID
				SET @S_ConditionValue = @EntityId
			END
		ELSE IF @LeadScoreConditionType = 23 -- An email sent to contact
			BEGIN
				SET @S_ConditionValue = ''
				SET @S_EntityId = @EntityId
			END
		ELSE IF @LeadScoreConditionType = 3 OR @LeadScoreConditionType= 1 -- submits form or opens email
			BEGIN
				SET @S_ConditionValue = @EntityId
				SET @S_EntityId = @EntityId
			END
		-- contact action tag added/not tag added/lead source added/tour type/note category type added
		ELSE IF @LeadScoreConditionType = 6 OR @LeadScoreConditionType =7 OR @LeadScoreConditionType = 8 OR @LeadScoreConditionType = 9  OR @LeadScoreConditionType = 27
			BEGIN
				SET @S_ConditionValue = @LinkedEntityID
				SET @S_EntityId = @EntityId
			END
		ELSE IF @LeadScoreConditionType = 4 OR @LeadScoreConditionType = 5 OR @LeadScoreConditionType = 26 -- contact visits a website/web page/page duration
			BEGIN
				SET @S_ConditionValue = @ConditionValue
				SET @S_EntityId = @EntityId
			END
		ELSE -- default
			BEGIN
				SET @S_EntityId = @LinkedEntityID
				SET @S_ConditionValue = @EntityId
			END
		-- get leadscore rules
		IF @LeadScoreConditionType = 2 -- link clicked
			BEGIN
				INSERT INTO @LeadScoreRules
				SELECT LeadScoreRuleID, Score, 0  
				FROM LeadScoreRules (NOLOCK) LSR 
				WHERE LSR.AccountID = @AccountID  AND 
				ConditionID = @LeadScoreConditionType AND 
				COALESCE(LSR.ConditionValue,@S_ConditionValue) = @S_ConditionValue AND 
				LSR.IsActive = 1 AND
				(LSR.SelectedCampaignLinks IN (@S_EntityId) OR LEN(COALESCE(LSR.SelectedCampaignLinks,'')) = 0)
			END
		ELSE IF @LeadScoreConditionType = 26 -- page visited duration
			BEGIN
				INSERT INTO @LeadScoreRules
				SELECT LeadScoreRuleID, Score, 0 
				FROM LeadScoreRules (NOLOCK) LSR 
				WHERE LSR.AccountID = @AccountID  AND 
				ConditionID = @LeadScoreConditionType AND 
				(LSR.ConditionValue = @S_ConditionValue OR LSR.ConditionValue ='*') AND 
				LSR.IsActive = 1 
			END
		ELSE -- for all
			BEGIN
				INSERT INTO @LeadScoreRules
				SELECT LeadScoreRuleID, Score, 0 
				FROM LeadScoreRules (NOLOCK) LSR 
				WHERE LSR.AccountID = @AccountID  AND 
				ConditionID = @LeadScoreConditionType AND 
				COALESCE(LSR.ConditionValue,@S_ConditionValue) = @S_ConditionValue AND 
				LSR.IsActive = 1
			END
		-- check leadscore audit
		;WITH LeadScoreRulesCTE 
		AS
		(
			SELECT * FROM @LeadScoreRules WHERE RuleID NOT IN (SELECT LeadScoreRuleID FROM LeadScores (NOLOCK) WHERE ContactID = @ContactID AND EntityID = @S_EntityId)
		)
		
		-- insert lead score
		INSERT INTO LeadScores
		SELECT @ContactID, RuleId, Score,GETUTCDATE(),@S_EntityId FROM LeadScoreRulesCTE
		
		SET @RuleCount = @@ROWCOUNT
		IF @RuleCount > 0
			BEGIN
				-- update contact leadscore
				DECLARE @ContactScore INT
				SELECT @ContactScore = SUM(Score) FROM LeadScores (NOLOCK) WHERE ContactID = @ContactID

				UPDATE Contacts
				SET LeadScore = @ContactScore
				WHERE ContactID = @ContactID
				-- update message
				UPDATE LeadScoreMessages
				SET Remarks = 'Lead score applied',
				ProcessedOn = GETUTCDATE(),
				LeadScoreProcessStatusID = 703
				WHERE LeadScoreMessageID = @LeadScoreMessageID
				-- insert into trackmessage
				INSERT INTO Workflow.TrackMessages (MessageID, LeadScoreConditionType, EntityID, ContactID,AccountID)
				VALUES (NEWID(),24, @ContactScore, @ContactID, @AccountID)

				-- insert into indexdata
				INSERT INTO IndexData
				SELECT NEWID(), @ContactID, 1, GETUTCDATE(), NULL, 1
			END
		ELSE
			BEGIN
				-- update message
				UPDATE LeadScoreMessages
				SET Remarks = 'No rule applied',
				ProcessedOn = GETUTCDATE(),
				LeadScoreProcessStatusID = 702
				WHERE LeadScoreMessageID = @LeadScoreMessageID
			END

		-- revisit loop
		SET @LeadScoreMessageID = NULL

		SELECT TOP 1 @LeadScoreMessageID = LeadScoreMessageID,
		@UserID = UserID, @LeadScoreConditionType = LeadScoreConditionType, @ContactID = ContactID, @AccountID = AccountID, @LinkedEntityID = LinkedEntityID , @EntityId = EntityID, @ConditionValue = ConditionValue
		FROM LeadScoreMessages (NOLOCK) WHERE LeadScoreProcessStatusID = 701 ORDER BY CreatedOn ASC
		DELETE @LeadScoreRules

		SET @LoopCounter = @LoopCounter + 1
	END
END

--EXEC dbo.ProcessLeadScoreMessages

GO


